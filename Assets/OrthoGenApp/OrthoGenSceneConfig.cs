using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using g3;
using f3;
using gs;
using gsbody;
using orthogen;

public class OrthoGenSceneConfig : BaseSceneConfig
{
    public GameObject VRCameraRig;

    FContext context;
    public override FContext Context { get { return context; } }

    // Use this for initialization
    public override void Awake()
    {
        // if we need to auto-configure Rift vs Vive vs (?) VR, we need
        // to do this before any other F3 setup, because MainCamera will change
        // and we are caching that in a lot of places...
        if (AutoConfigVR) {
            VRCameraRig = gs.VRPlatform.AutoConfigureVR();
        }

        // restore any settings
        SceneGraphConfig.RestorePreferences();

        // set up some defaults
        // this will move the ground plane down, but the bunnies will be floating...
        //SceneGraphConfig.InitialSceneTranslate = -4.0f * Vector3f.AxisY;
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.35f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 2.3f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 25.0f;

        // make curves easier to click
        PolyCurveSO.DefaultHitWidthMultiplier = 2.0f;

        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = true;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.CockpitInitializer = new SetupOrthoGenCockpit();

        options.MouseCameraControls = new MayaCameraHotkeys() { MousePanSpeed = 5.0f, MouseZoomSpeed = 5.0f };
        options.SpatialCameraRig = VRCameraRig;

        options.Use2DCockpit = true;
        options.ConstantSize2DCockpit = true;
        FPlatform.EditorPixelScaleFactor = 1.0f;

        // very verbose
        options.LogLevel = 2;

        context = new FContext();
        OG.Context = context;
        OrthogenUI.ActiveContext = context;
        context.Start(options);

        DebugUtil.Log("started context");

        // Set up standard scene lighting if enabled
        if (options.EnableDefaultLighting) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.LightDistance = 30.0f; // related to total scene scale...
        }


        //GameObjectFactory.CurveRendererSource = new VectrosityCurveRendererFactory();

        // set up ground plane geometry (optional)
        GameObject boundsObject = GameObject.Find("Bounds");
        if (boundsObject != null) {
            context.Scene.AddWorldBoundsObject(boundsObject);
        }


        /*
         * ORTHOGEN-SPECIFIC SETUP STARTS HERE
         */

        // set up scene and tools like Orthogen wants them
        OGActions.InitializeUsageContext(OGActions.UsageContext.OrthoVRApp);
        //OGActions.InitializeUsageContext(OGActions.UsageContext.NiaOrthogenApp);
        OrthogenMaterials.InitializeMaterials();
        OGActions.InitializeF3Scene(context);
        OGActions.InitializeF3Tools(context);
        OGActions.PostConfigureTools_Demo();
        OGActions.ConfigurePlatformInput_Mouse();

        /*
         * optional things specific to demo app
         */

        // ground plane stays below socket as it is updated
        DemoActions.AddRepositionGroundPlaneOnSocketEdit();

        
        /*
         * import sample mesh
         */ 

        // load sample mesh
        string assetPath = Application.dataPath;
        string samplesPath = Path.Combine(assetPath, "..", "sample_files");
        //string sampleFile = Path.Combine(samplesPath, "sample_socket_off.obj");
        //string sampleFile = Path.Combine(samplesPath, "sample_socket_1.obj");
        //string sampleFile = Path.Combine(samplesPath, "scan_1_raw.obj");
        string sampleFile = Path.Combine(samplesPath, "scan_1_remesh.obj");
        DMesh3 mesh = StandardMeshReader.ReadMesh(sampleFile);
        if (mesh.HasVertexColors == false)
            mesh.EnableVertexColors(Colorf.Silver);
        // read sample file from Resources instead
        //MemoryStream sampleFileStream = FResources.LoadBinary("sample_socket_1");
        //DMesh3 mesh = StandardMeshReader.ReadMesh(sampleFileStream, "obj");
        double scale = Units.MetersTo(Units.Linear.Millimeters);   // this mesh is in meters, so scale to mm
        MeshTransforms.FlipLeftRightCoordSystems(mesh);   // convert to unity coordinate system
        MeshTransforms.Scale(mesh, scale);

        // initialize the datamodel
        OGActions.BeginSocketDesignFromScan(Context, mesh);

        // set up my UI tests/etc
        configure_unity_ui();

        // dgraph tests
        //DGTest.test(Debug.Log);
    }







    static void configure_unity_ui()
    {
        Button button = UnityUtil.FindGameObjectByName("CancelToolButton").GetComponent<Button>();
        button.onClick.AddListener(() => {
            OrthogenUI.ActiveContext.RegisterNextFrameAction(() => {
                OrthogenUI.ActiveContext.ToolManager.DeactivateTools();
            });
        });
    }




}