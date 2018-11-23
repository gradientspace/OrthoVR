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

public class OrthoVRSceneConfig : BaseSceneConfig
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
        SceneGraphConfig.DefaultSceneCurveVisualDegrees = 0.5f;
        SceneGraphConfig.DefaultPivotVisualDegrees = 2.3f;
        SceneGraphConfig.DefaultAxisGizmoVisualDegrees = 25.0f;
        PolyCurveSO.DefaultHitWidthMultiplier = 2.5f;

        SceneOptions options = new SceneOptions();
        options.UseSystemMouseCursor = false;
        options.Use2DCockpit = false;
        options.EnableTransforms = true;
        options.EnableCockpit = true;
        options.EnableDefaultLighting = false;
        options.CockpitInitializer = new SetupOrthoVRCockpit();

        options.MouseCameraControls = new MayaCameraHotkeys() { MousePanSpeed = 5.0f, MouseZoomSpeed = 5.0f };
        options.SpatialCameraRig = VRCameraRig;

        // very verbose
        options.LogLevel = 2;

        // hacks for stuff
#if F3_ENABLE_TEXT_MESH_PRO
        SceneGraphConfig.TextLabelZOffset = -0.01f;
#else
        SceneGraphConfig.TextLabelZOffset = -0.3f;
#endif


        context = new FContext();
        OG.Context = context;
        OrthogenUI.ActiveContext = context;
        context.Start(options);


        // Set up standard scene lighting if enabled
        if (options.EnableDefaultLighting) {
            GameObject lighting = GameObject.Find("SceneLighting");
            if (lighting == null)
                lighting = new GameObject("SceneLighting");
            SceneLightingSetup setup = lighting.AddComponent<SceneLightingSetup>();
            setup.Context = context;
            setup.ShadowLightCount = 0;
            setup.AdjustShadowDistance = false;
            setup.LightDistance = 1000.0f; // related to total scene scale...
        }

        // override sun so that it doesn't stick to one of the scene lights
        RenderSettings.sun = GameObject.Find("SunLight").GetComponent<Light>();

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
        OGActions.InitializeVRUsageContext();
        OrthogenMaterials.InitializeMaterials();
        OrthogenMaterials.ScanMaterial = new UnitySOMaterial(MaterialUtil.SafeLoadMaterial("scan_material"));
        //OrthogenMaterials.RectifiedLegMaterial = OrthogenMaterials.ScanMaterial;
        OGActions.InitializeF3Scene(context);
        OGActions.InitializeF3Tools(context);
        OGActions.InitializeF3VRTools(context);
        OGActions.PostConfigureTools_Demo();
        OGActions.ConfigurePlatformInput_VR();


        /*
         * optional things specific to demo app
         */

        // ground plane stays below socket as it is updated
        DemoActions.AddRepositionGroundPlaneOnSocketEdit();


        /*
         * import sample mesh
         */
        bool do_scan_demo = true;

        // load sample mesh
        string assetPath = Application.dataPath;
        string samplesPath = Path.Combine(assetPath, "..", "sample_files");
        //string sampleFile = Path.Combine(samplesPath, "sample_socket_off.obj");
        string sampleFile = Path.Combine(samplesPath, "sample_socket_1.obj");
        if (do_scan_demo)
            sampleFile = Path.Combine(samplesPath, "scan_1_remesh.obj");
        if (File.Exists(sampleFile) == false)
            sampleFile = Path.Combine(samplesPath, "sample_socket_1.obj");
        DMesh3 mesh = StandardMeshReader.ReadMesh(sampleFile);
        // read sample file from Resources instead
        //MemoryStream sampleFileStream = FResources.LoadBinary("sample_socket_1");
        //DMesh3 mesh = StandardMeshReader.ReadMesh(sampleFileStream, "obj");
        if (mesh.HasVertexColors == false)
            mesh.EnableVertexColors(Colorf.Silver);

        // transform to our coordinate system
        double scale = Units.MetersTo(Units.Linear.Millimeters);   // this mesh is in meters, so scale to mm
        MeshTransforms.FlipLeftRightCoordSystems(mesh);   // convert to unity coordinate system
        MeshTransforms.Scale(mesh, scale);

        if (do_scan_demo)
            OGActions.SetSizeMode(OGActions.SizeModes.RealSize);
        else
            OGActions.SetSizeMode(OGActions.SizeModes.DemoSize);

        // initialize the datamodel
        OGActions.BeginSocketDesignFromScan(Context, mesh);

        // set up my UI tests/etc
        configure_unity_ui();

        // [RMS] do this next frame because SteamVR needs a chance to set up and position the cockpit
        OGActions.RecenterVRView(true);

        add_vr_head(context);

        // dgraph tests
        //DGTest.test(Debug.Log);
    }




    static void add_vr_head(FContext context)
    {
        // [TODO] need to do this at cockpit level!!
        GameObject head = GameObject.Find("VRHead");

        if (head != null && head.IsVisible()) {
            head.transform.position = Vector3f.Zero;
            head.transform.rotation = Quaternionf.Identity;
            context.ActiveCamera.AddChild(head, false);

            GameObject mesh = head.FindChildByName("head_mesh", false);
            Colorf c = mesh.GetColor();
            SmoothCockpitTracker tracker = context.ActiveCockpit.CustomTracker as SmoothCockpitTracker;
            tracker.OnTrackingStateChange += (eState) => {
                if (eState == SmoothCockpitTracker.TrackingState.NotTracking)
                    mesh.SetColor(c);
                else if (eState == SmoothCockpitTracker.TrackingState.Tracking)
                    mesh.SetColor(Colorf.VideoRed);
                else if (eState == SmoothCockpitTracker.TrackingState.TrackingWarmup || eState == SmoothCockpitTracker.TrackingState.TrackingCooldown)
                    mesh.SetColor(Colorf.Orange);
            };
        }

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
