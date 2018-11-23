using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;
using gs;
using gsbody;

namespace orthogen
{
    /// <summary>
    /// Actions used for initialization in Orthogen (scene, tools, initialize from scan, save/load, etc)
    /// </summary>
    public static partial class OGActions
    {
        public enum SizeModes
        {
            RealSize = 1,
            DemoSize = 2,
        }

        static SizeModes size_mode = SizeModes.RealSize;
        static float meter_size = 1000;

        public static SizeModes CurrentSizeMode {
            get { return size_mode; }
        }
        public static void SetSizeMode(SizeModes mode)
        {
            if (size_mode != mode) {
                size_mode = mode;
                switch(size_mode) {
                    case SizeModes.RealSize:
                        meter_size = 1000;
                        break;
                    case SizeModes.DemoSize:
                        meter_size = 300;
                        break;
                }
                OGActions.SceneToWorldScale = 1.0f / meter_size;
            }
        }







        public static void InitializeVRUsageContext()
        {
            InitializeUsageContext(UsageContext.OrthoVRApp);

            OGActions.ExtendDesignGraphF = OGVRWorkflow.AddVRWorkflowGraphTransitions;

            OG.OnWorfklowInitialized += updateWorkflowForVR;

        }

        private static void updateWorkflowForVR(object sender, EventArgs e)
        {
            OGActions.AddToAcceptRouter(OGVRWorkflow.VRAlignScanState, OGVRWorkflow.VRAlignScanAcceptT);
            OGActions.AddToCancelRouter(OGVRWorkflow.VRAlignScanState, OGVRWorkflow.VRAlignScanCancelT);
        }



        /// <summary>
        /// set up tools
        /// </summary>
        public static void InitializeF3VRTools(FContext context)
        {
            // tool for aligning scan
            context.ToolManager.RegisterToolType(SpatialDeviceScanAlignmentTool.Identifier, new SpatialDeviceScanAlignmentToolBuilder());
        }










        /// <summary>
        /// register various input behaviours specific to mouse input
        /// </summary>
        public static void ConfigurePlatformInput_VR()
        {
            // add mouse right-ctrl-drag behaviors for various deformations
            OG.OnModelingOpAdded += (so, op) => {
                if (so is BaseSO && op is IVectorDisplacementSourceOp) {
                    var b = new SpatialEditDeformOpSOBehavior(
                        OG.Context, so as BaseSO, OG.Leg.SO, op) { Priority = 5 };
                    (so as BaseSO).InputBehaviors.Add(b);
                }
            };

        }



        public static void RecenterVRView(bool bRecenter, bool bAnimated = false)
        {
            FContext ctx = OG.Context;

            OG.Context.RegisterNextFrameAction(() => {

                ctx.ResetView(false);
                ctx.Scene.SetSceneScale(1.0f / meter_size);

                //OG.Context.ActiveCamera.

                Frame3f cockpitF = ctx.ActiveCockpit.GetLevelViewFrame(CoordSpace.WorldCoords);
                Frame3f forwardF = cockpitF.Translated(1.2f, 2);

                ctx.ActiveCamera.SetTarget(forwardF.Origin);
                ctx.ActiveCamera.Manipulator().SceneZoom(ctx.Scene, ctx.ActiveCamera, -0.7f, false);

                Vector3f legCenterW = OG.Scan.SO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
                if ( size_mode == SizeModes.RealSize )
                    legCenterW += 0.5f * forwardF.Y;
                
                ctx.ActiveCamera.Manipulator().ScenePanFocus(ctx.Scene, ctx.ActiveCamera, legCenterW, bAnimated);


                //OG.Context.ScaleView(OG.Scan.SO.GetLocalFrame(CoordSpace.WorldCoords).Origin, meter_size);

                //OG.Context.ActiveCamera.Manipulator().SceneTranslate(OG.Scene, 100 * Vector3f.AxisZ, false);
                //OG.Context.ActiveCamera.SetTarget(OG.Context.ActiveCamera.GetPosition() + 100 * Vector3f.AxisZ);
            });
        }




        
    }
}
