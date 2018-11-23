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
        // [RMS] this is a thing I use for the VR version, will always be 1 on desktop/tablet
        public static float scene_to_world_scale = 1.0f;
        public static float SceneToWorldScale {
            get { return scene_to_world_scale; }
            set { scene_to_world_scale = value; }
        }


        // gizmo names (should go here?)
        public const string LenthenMovePivotGizmoType = "move_lengthen_pivot";


        /// <summary>
        /// Initialize datamodel / workflow / etc for Socket design, based on this scan mesh
        /// </summary>
        public static void BeginSocketDesignFromScan(FContext context, DMesh3 scanMesh)
        {
            context.NewScene(false, false);
            OG.Reinitialize(context, DesignTypes.StandardLegSocket);
            OGActions.InitializeScan(scanMesh);
        }


        /// <summary>
        /// Initialize datamodel / workflow / etc for Socket design, based on restored scan SO
        /// </summary>
        public static void RestoreSocketDesignFromScan(FContext context, ScanSO scanSO)
        {
            OG.Reinitialize(context, DesignTypes.StandardLegSocket);
            OGActions.RestoreScan(scanSO);
        }


        
        /// <summary>
        /// Initialize datamodel / workflow / etc for AFO design, based on this scan mesh
        /// </summary>
        public static void BeginAFODesignFromScan(FContext context, DMesh3 scanMesh)
        {
            context.NewScene(false);
            OG.Reinitialize(context, DesignTypes.StandardAFO);
            throw new NotImplementedException("Have not implemented AFO yet");
            //OGActions.InitializeScan(mesh);
        }



        public static void BeginRestoreExistingScene()
        {
            OG.Context.NewScene(false, false);
        }


        // [RMS] to support customizing socket design graph
        public static Action<SocketDesignGraph> ExtendDesignGraphF = null;

        public static void InitializeSocketDataModel()
        {
            SocketDesignGraph graph = SocketDesignGraph.Build();
            OG.Model.InitializeWorkflow(graph);
            if (ExtendDesignGraphF != null)
                ExtendDesignGraphF(graph);
        }





        /// <summary>
        /// Used for setup functions that are called from initial SceneConfig. 
        /// This is used to tailor some settings that are messy to expose otherwise.
        /// </summary>
        public enum UsageContext
        {
            SocketGenDemo = 0,
            NiaOrthogenApp = 1,
            OrthoVRApp = 2
        }
        public static UsageContext CurrentUsageContext = UsageContext.SocketGenDemo;


        public static void InitializeUsageContext(UsageContext context)
        {
            CurrentUsageContext = context;
        }



        /// <summary>
        /// Set up materials and gizmos
        /// </summary>
        public static void InitializeF3Scene(FContext context)
        {
            // initialize materials and selection-materials
            context.Scene.DisableSelectionMaterial = false;

            context.Scene.SelectedMaterial = MaterialUtil.CreateStandardVertexColorMaterialF(Colorf.VideoYellow);

            context.Scene.PerTypeSelectionMaterial.Add(
                BodyModelSOTypes.Scan, MaterialUtil.ToUnityMaterial(OrthogenMaterials.ScanMaterial));
            context.Scene.PerTypeSelectionMaterial.Add(
                BodyModelSOTypes.Leg, MaterialUtil.ToUnityMaterial(OrthogenMaterials.LegMaterial));
            fMaterial selectedCurveMat = MaterialUtil.CreateFlatMaterialF(Colorf.Gold, 1);
            context.Scene.PerTypeSelectionMaterial.Add(BodyModelSOTypes.PlaneIntersectionCurve, selectedCurveMat);
            context.Scene.PerTypeSelectionMaterial.Add(BodyModelSOTypes.TrimLoop, selectedCurveMat);
            context.Scene.PerTypeSelectionMaterial.Add(BodyModelSOTypes.EnclosedPatch, selectedCurveMat);


            // if you had other gizmos, you would register them here
            //context.TransformManager.RegisterGizmoType("snap_drag", new SnapDragGizmoBuilder());
            //controller.TransformManager.SetActiveGizmoType("snap_drag");
            context.TransformManager.RegisterGizmoType(AxisTransformGizmo.DefaultName, new AxisTransformGizmoBuilder());
            context.TransformManager.RegisterGizmoType(LenthenMovePivotGizmoType, new MoveLengthenPivotGizmoBuilder() {
                WidgetScale = 0.5f * Vector3f.One
            });

            context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);


            BodyModelSOTypes.RegisterSocketGenTypes(context.Scene.TypeRegistry);
        }


        /// <summary>
        /// set up tools
        /// </summary>
        public static void InitializeF3Tools(FContext context)
        {
            // Register tools

            // Scan tools

            // tool for selecting faces subset
            context.ToolManager.RegisterToolType(BodyModelTools.CreateSelectScanSubsetTool, new TwoPointFaceSelectionToolBuilder() {
                SphereIndicatorSizeScene = 5.0f,
                OnApplyF = OGActions.CropScanFromSelection
            });

            // tool for aligning scan
            context.ToolManager.RegisterToolType(BodyModelTools.AlignScanTool, new SocketAlignmentToolBuilder() {
                DefaultIndicatorSizeScene = 10.0f
            });



            // Tool for drawing surface regions
            context.ToolManager.RegisterToolType(BodyModelTools.DrawSurfaceCurveRegionIdentifier, new DrawSurfaceCurveToolBuilder() {
                Closed = (CurrentUsageContext == UsageContext.NiaOrthogenApp) ? false : true,
                IsOverlayCurve = true,
                DefaultSamplingRateS = 2.5f,
                DefaultSurfaceOffsetS = 0.5f,
                DefaultCloseThresholdS = 5.0f,
                EmitNewCurveF = OGActions.EmitRegionDeformationFromCurvePreview
            });

            // tool for creating expand/contract band from plane
            context.ToolManager.RegisterToolType(BodyModelTools.CreateInOutPlaneTool, new CreatePlaneFromSurfacePointToolBuilder() {
                SphereIndicatorSizeScene = 5.0f,
                PlaneIndicatorWidthScene = 150.0f,
                OnApplyF = OGActions.EmitPlaneBandExpansionFromTool
            });


            // tool for creating expand/contract band from plane
            context.ToolManager.RegisterToolType(BodyModelTools.CreateOffsetBandTool, new TwoPointBandToolBuilder() {
                DefaultIndicatorSizeScene = 5.0f,
                PlaneIndicatorWidthScene = 150.0f,
                OnApplyF = OGActions.EmitOffsetBandFromTool
            });


            // Tool for drawing trimline
            context.ToolManager.RegisterToolType(BodyModelTools.DrawTrimLineIdentifier, new DrawMultiClickLoopToolBuilder() {
                CurveMaterialF = () => { return OrthogenMaterials.TrimLoopMaterial; },
                EmitNewCurveF = OGActions.EmitTrimCurveFromPreview,
                DefaultCloseThresholdS = 5.0f,
                DefaultSamplingRateS = 1.0f,
                IndicatorSize = fDimension.Scene(2.5)
            });

            // tool for creating trim curve from plane
            context.ToolManager.RegisterToolType(BodyModelTools.CreateTrimlineFromPlaneIdentifier, new CreatePlaneFromSurfacePointToolBuilder() {
                SphereIndicatorSizeScene = 2.5f,
                PlaneIndicatorWidthScene = 150.0f,
                OnApplyF = OGActions.EmitTrimCurveFromPlane
            });



            // tool for sculpting curves
            context.ToolManager.RegisterToolType(BodyModelTools.SculptRegionCurveIdentifer, new SculptCurveToolBuilder() {
                InitialRadius = fDimension.Scene(30.0f),
                SmoothAlpha = 0.3f
            });


            // measure tools
            context.ToolManager.RegisterToolType(TwoPointMeasureTool.Identifier, new TwoPointMeasureToolBuilder() {
                AllowSelectionChanges = false,
                ShowTextLabel = false, TextHeightDimensionType = DimensionType.WorldUnits, TextHeightDimension = 10.0f
            });


            context.ToolManager.SetActiveToolType(BodyModelTools.DrawSurfaceCurveRegionIdentifier, ToolSide.Right);
        }




        /// <summary>
        /// Sample codes for tweaking tool settings
        /// </summary>
        public static void PostConfigureTools_Demo()
        {
            TwoPointBandToolBuilder planeBuilder = 
                (TwoPointBandToolBuilder)OG.Context.ToolManager.FindToolTypeBuilder(BodyModelTools.CreateOffsetBandTool);
            if ( planeBuilder != null ) {
                // set a custom indicator factory that overrides some default properties
                planeBuilder.IndicatorBuilder = new TwoPointBandTool_IndicatorFactory();

                // set a custom hit-test that uses double radius
                planeBuilder.CustomHitTestF = (rayS, centerS, radiusS, id) => {
                    return IntersectionUtil.RaySphere(ref rayS.Origin, ref rayS.Direction, ref centerS, 2 * radiusS);
                };
            }
        }

        class TwoPointBandTool_IndicatorFactory : StandardIndicatorFactory
        {
            public override SectionPlaneIndicator MakeSectionPlaneIndicator(
                int id, string name,
                fDimension Width,
                Func<Frame3f> SceneFrameF,
                Func<Colorf> ColorF,
                Func<bool> VisibleF)
            {
                if (name == "endPlane")     // this name comes from TwoPointBandTool.Setup
                    ColorF = () => { return new Colorf(Colorf.Orange, 0.5f); };
                return base.MakeSectionPlaneIndicator(id, name, Width, SceneFrameF, ColorF, VisibleF);
            }
        }






        /// <summary>
        /// register various input behaviours specific to mouse input
        /// </summary>
        public static void ConfigurePlatformInput_Mouse()
        {
            // add mouse right-ctrl-drag behaviors for various deformations
            OG.OnModelingOpAdded += (so, op) => {
                if (so is BaseSO && op is IVectorDisplacementSourceOp)
                    add_input_behaviors_for_mouse(so as BaseSO, op);
            };

        }




        static void add_input_behaviors_for_mouse(BaseSO so, IVectorDisplacementSourceOp op)
        {
            if (op is EnclosedRegionOffsetOp) {
                EnclosedRegionOffsetOp deformOp = op as EnclosedRegionOffsetOp;
                so.InputBehaviors.Add(
                    new RightMouseClickDragBehavior() {
                        WantsCaptureF = (input) => { return input.bCtrlKeyDown; },
                        UpdateCaptureF = (input, lastInput) => {
                            deformOp.PushPullDistance += input.vMouseDelta2D.x * 0.1;
                        }
                    });

            } else if (op is EnclosedRegionSmoothOp) {
                EnclosedRegionSmoothOp deformOp = op as EnclosedRegionSmoothOp;
                so.InputBehaviors.Add(
                    new RightMouseClickDragBehavior() {
                        WantsCaptureF = (input) => { return input.bCtrlKeyDown; },
                        UpdateCaptureF = (input, lastInput) => {
                            deformOp.OffsetDistance += input.vMouseDelta2D.x * 0.1;
                            deformOp.SmoothAlpha += input.vMouseDelta2D.y * 0.1;
                        }
                    });

            } else if (op is PlaneBandExpansionOp) {
                PlaneBandExpansionOp deformOp = op as PlaneBandExpansionOp;
                so.InputBehaviors.Add(
                    new RightMouseClickDragBehavior() {
                        WantsCaptureF = (input) => { return input.bCtrlKeyDown; },
                        UpdateCaptureF = (input, lastInput) => {
                            deformOp.PushPullDistance += input.vMouseDelta2D.x * 0.1;
                            deformOp.BandDistance += input.vMouseDelta2D.y * 0.1;
                        }
                    });
            }
        }




        
    }
}
