using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using f3;
using g3;
using gs;
using gsbody;

namespace orthogen
{

    class SetupOrthoGenCockpit : ICockpitInitializer
    {
        public void Initialize(Cockpit cockpit)
        {
            cockpit.Name = "modelCockpit";
            

            // Configure how the cockpit moves

            cockpit.PositionMode = Cockpit.MovementMode.TrackPosition;
            // [RMS] use orientation mode to make cockpit follow view orientation.
            //  (however default widgets below are off-screen!)
            //cockpit.PositionMode = Cockpit.MovementMode.TrackOrientation;



            FScene Scene = cockpit.Scene;
            BoxContainer screenContainer = new BoxContainer(new Cockpit2DContainerProvider(cockpit));
            PinnedBoxes2DLayoutSolver screenLayout = new PinnedBoxes2DLayoutSolver(screenContainer);
            PinnedBoxesLayout layout = new PinnedBoxesLayout(cockpit, screenLayout) {
                StandardDepth = 1.5f
            };
            cockpit.AddLayout(layout, "2D", true);


            Func<string, float, HUDLabel> MakeButtonF = (label, buttonW) => {
                HUDLabel button = new HUDLabel() {
                    Shape = OrthogenUI.MakeMenuButtonRect(buttonW, OrthogenUI.MenuButtonHeight),
                    TextHeight = OrthogenUI.MenuButtonTextHeight,
                    AlignmentHorz = HorizontalAlignment.Center,
                    BackgroundColor = OrthogenUI.ButtonBGColor, 
                    TextColor = OrthogenUI.ButtonTextColor,
                    DisabledTextColor = OrthogenUI.DisabledButtonTextColor,
                    Text = label,
                    EnableBorder = true, BorderWidth = OrthogenUI.StandardButtonBorderWidth, BorderColor = OrthogenUI.ButtonTextColor
                };
                button.Create();
                button.Name = label;
                button.Enabled = true;
                return button;
            };
            Func<string, float, float, HUDSpacer> MakeSpacerF = (label, spacerw, spacerh) => {
                HUDSpacer spacer = new HUDSpacer() {
                    Shape = new HUDShape(HUDShapeType.Rectangle, spacerw, spacerh)
                };
                spacer.Create();
                spacer.Name = label;
                return spacer;
            };


            HUDElementList button_list = new HUDElementList() {
                Width = OrthogenUI.MenuButtonWidth,
                Height = 5*OrthogenUI.MenuButtonHeight,
				Spacing = 10*OrthogenUI.PixelScale,
				Direction = HUDElementList.ListDirection.Vertical
            };



            HUDLabel trim_scan_button = MakeButtonF("Trim Scan", OrthogenUI.MenuButtonWidth);
            trim_scan_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.TrimScanStartT);
            };
            button_list.AddListItem(trim_scan_button);

            HUDLabel align_scan_button = MakeButtonF("Align Scan", OrthogenUI.MenuButtonWidth);
            align_scan_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.AlignScanStartT);
            };
            button_list.AddListItem(align_scan_button);


            HUDLabel accept_scan_button = MakeButtonF("Done Scan", OrthogenUI.MenuButtonWidth);
            accept_scan_button.OnClicked += (sender, e) => {
                OG.TransitionToState(RectifyState.Identifier);
            };
            button_list.AddListItem(accept_scan_button);



            button_list.AddListItem(MakeSpacerF("space", OrthogenUI.MenuButtonWidth, 0.5f * OrthogenUI.MenuButtonHeight));


            HUDLabel draw_offset_area_button = MakeButtonF("Offset Area", OrthogenUI.MenuButtonWidth);
			draw_offset_area_button.OnClicked += (sender, e) => {
                OGActions.CurrentLegDeformType = LegModel.LegDeformationTypes.Offset;
                OG.Transition(OGWorkflow.DrawAreaStartT);
            };
			button_list.AddListItem(draw_offset_area_button);

            HUDLabel draw_smooth_area_button = MakeButtonF("Smooth Area", OrthogenUI.MenuButtonWidth);
            draw_smooth_area_button.OnClicked += (sender, e) => {
                OGActions.CurrentLegDeformType = LegModel.LegDeformationTypes.Smooth;
                OG.Transition(OGWorkflow.DrawAreaStartT);
            };
            button_list.AddListItem(draw_smooth_area_button);


            HUDLabel add_plane_button = MakeButtonF("Add Plane", OrthogenUI.MenuButtonWidth);
            add_plane_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.AddDeformRingStartT);
            };
            button_list.AddListItem(add_plane_button);

            HUDLabel add_lengthen_button = MakeButtonF("Add Lengthen", OrthogenUI.MenuButtonWidth);
            add_lengthen_button.OnClicked += (sender, e) => {
                if (OGActions.CanAddLengthenOp())
                    OGActions.AddLengthenOp();
            };
            button_list.AddListItem(add_lengthen_button);


            HUDLabel sculpt_curve_button = MakeButtonF("Sculpt Curve", OrthogenUI.MenuButtonWidth);
            WorkflowRouter sculpt_router = WorkflowRouter.Build(new[] {
                OGWorkflow.RectifyState, OGWorkflow.SculptAreaStartT,
                OGWorkflow.SocketState, OGWorkflow.SculptTrimlineStartT });
            sculpt_curve_button.OnClicked += (sender, e) => {
                sculpt_router.Apply(OG.Model.Workflow);
			};
			button_list.AddListItem(sculpt_curve_button);


            HUDLabel accept_rectify_button = MakeButtonF("Begin Socket", OrthogenUI.MenuButtonWidth);
            accept_rectify_button.OnClicked += (sender, e) => {
                OG.Leg.SetOpWidgetVisibility(false);
                OG.TransitionToState(SocketDesignState.Identifier);
            };
            button_list.AddListItem(accept_rectify_button);



            button_list.AddListItem(MakeSpacerF("space", OrthogenUI.MenuButtonWidth, 0.5f * OrthogenUI.MenuButtonHeight));



            HUDLabel draw_trim_line_button = MakeButtonF("Draw Trimline", OrthogenUI.MenuButtonWidth);
            draw_trim_line_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.DrawTrimlineStartT);
            };
            button_list.AddListItem(draw_trim_line_button);

            HUDLabel plane_trim_line_button = MakeButtonF("Plane Trimline", OrthogenUI.MenuButtonWidth);
            plane_trim_line_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.PlaneTrimlineStartT);
            };
            button_list.AddListItem(plane_trim_line_button);

            HUDLabel add_socket_button = MakeButtonF("Add Socket", OrthogenUI.MenuButtonWidth);
            add_socket_button.OnClicked += (sender, e) => {
                if ( OGActions.CanAddSocket() )
                    OGActions.AddSocket();
            };
            button_list.AddListItem(add_socket_button);

            HUDLabel export_socket_button = MakeButtonF("Export", OrthogenUI.MenuButtonWidth);
            export_socket_button.OnClicked += (sender, e) => {
                if (OGActions.CanExportSocket())
                    OGActions.ExportSocket();
            };
            button_list.AddListItem(export_socket_button);



            button_list.AddListItem(MakeSpacerF("space", OrthogenUI.MenuButtonWidth, 1.0f * OrthogenUI.MenuButtonHeight));


            HUDLabel accept_button = MakeButtonF("Accept", OrthogenUI.MenuButtonWidth);
            accept_button.OnClicked += (sender, e) => {
                OGActions.AcceptCurrentTool();
            };


            HUDLabel cancel_button = MakeButtonF("Cancel", OrthogenUI.MenuButtonWidth);
            cancel_button.OnClicked += (sender, e) => {
                OGActions.CancelCurrentTool();
            };
            button_list.AddListItem(accept_button);
            button_list.AddListItem(cancel_button);



            button_list.Create();
            button_list.Name = "button_bar";

            // align button list to center of timeline
            layout.Add(button_list, new LayoutOptions() { Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.BoxPointF(button_list, BoxPosition.TopLeft),
                PinTargetPoint2D = LayoutUtil.BoxPointF(screenContainer, BoxPosition.TopLeft, 10*OrthogenUI.PixelScale*(Vector2f.AxisX-Vector2f.AxisY) )
            });

            screenLayout.RecomputeLayout();



            // Configure interaction behaviors
            //   - below we add behaviors for mouse, gamepad, and spatial devices (oculus touch, etc)
            //   - keep in mind that Tool objects will register their own behaviors when active

            // setup key handlers (need to move to behavior...)
            cockpit.AddKeyHandler(new OrthoGenKeyHandler(cockpit.Context));

            // these behaviors let us interact with UIElements (ie left-click/trigger, or either triggers for Touch)
            cockpit.InputBehaviors.Add(new Mouse2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 1 });

            // selection / multi-selection behaviors
            // Note: this custom behavior implements some selection redirects that we use in various parts of Archform
            cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });

            // left click-drag to tumble, and left click-release to de-select
            cockpit.InputBehaviors.Add(new MouseClickDragSuperBehavior() {
                Priority = 100,
                DragBehavior = new MouseViewRotateBehavior(cockpit.Context) { Priority = 100, RotateSpeed = 3.0f },
                ClickBehavior = new MouseDeselectBehavior(cockpit.Context) { Priority = 999 }
            });

            // also right-click-drag to tumble
            cockpit.InputBehaviors.Add(new MouseViewRotateBehavior(cockpit.Context) {
                Priority = 100, RotateSpeed = 3.0f,
                ActivateF = MouseBehaviors.RightButtonPressedF, ContinueF = MouseBehaviors.RightButtonDownF
            });

            // middle-click-drag to pan
            cockpit.InputBehaviors.Add(new MouseViewPanBehavior(cockpit.Context) {
                Priority = 100, PanSpeed = 10.0f,
                ActivateF = MouseBehaviors.MiddleButtonPressedF, ContinueF = MouseBehaviors.MiddleButtonDownF
            });


            cockpit.OverrideBehaviors.Add(new MouseWheelZoomBehavior(cockpit) { Priority = 100, ZoomScale = 100.0f });

            // touch input
            cockpit.InputBehaviors.Add(new TouchUIBehavior(cockpit.Context) { Priority = 1 });
            cockpit.InputBehaviors.Add(new Touch2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new TouchViewManipBehavior(cockpit.Context) {
                Priority = 999, TouchZoomSpeed = 1.0f, TouchPanSpeed = 0.3f
            });


            // update buttons enable/disable on state transitions, selection changes
            Action updateStateChangeButtons = () => {
                trim_scan_button.Enabled = OG.CanTransition(OGWorkflow.TrimScanStartT);
                align_scan_button.Enabled = OG.CanTransition(OGWorkflow.AlignScanStartT);
                accept_scan_button.Enabled = 
                    OG.IsInState(ScanState.Identifier) && OG.CanTransitionToState(RectifyState.Identifier);

                draw_offset_area_button.Enabled = OG.CanTransition(OGWorkflow.DrawAreaStartT);
                draw_smooth_area_button.Enabled = OG.CanTransition(OGWorkflow.DrawAreaStartT);
                add_plane_button.Enabled = OG.CanTransition(OGWorkflow.AddDeformRingStartT);
                add_lengthen_button.Enabled = OGActions.CanAddLengthenOp();
                accept_rectify_button.Enabled = OG.IsInState(RectifyState.Identifier) &&
                        OG.CanTransitionToState(SocketDesignState.Identifier);

                draw_trim_line_button.Enabled = OG.CanTransition(OGWorkflow.DrawTrimlineStartT);
                plane_trim_line_button.Enabled = OG.CanTransition(OGWorkflow.PlaneTrimlineStartT);
                add_socket_button.Enabled = OGActions.CanAddSocket();
                export_socket_button.Enabled = OGActions.CanExportSocket();

                sculpt_curve_button.Enabled = sculpt_router.CanApply(OG.Model.Workflow);
            };
            OG.OnWorfklowInitialized += (o,e) => { updateStateChangeButtons(); };
            OG.OnStateTransition += (from, to) => { updateStateChangeButtons(); };
            OG.OnDataModelModified += (from, to) => { updateStateChangeButtons(); };
            cockpit.Scene.SelectionChangedEvent += (o,e) => { if (OG.WorkflowInitialized) updateStateChangeButtons(); };
            cockpit.Scene.ChangedEvent += (scene,so,type) => { if (OG.WorkflowInitialized) updateStateChangeButtons(); };

            // accept/cancel buttons need to be checked every frame because the CanApply state
            // could change at any time, and there is no event about it
            cockpit.Context.RegisterEveryFrameAction("update_buttons", () => {
                if (cockpit.Context.ToolManager.ActiveRightTool != null) {
                    cancel_button.Enabled = true;
                    accept_button.Enabled = cockpit.Context.ToolManager.ActiveRightTool.CanApply;
                } else {
                    cancel_button.Enabled = accept_button.Enabled = false;
                }

                // [RMS] currently this state changes outside workflow state changes...
                add_socket_button.Enabled = OGActions.CanAddSocket();

            });


        }
    }












    public class OrthoGenKeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public OrthoGenKeyHandler(FContext c)
        {
            context = c;
        }
        public bool HandleShortcuts()
        {
            bool bShiftDown = Input.GetKey(KeyCode.LeftShift);
            bool bCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // ESCAPE CLEARS ACTIVE TOOL OR SELECTION
            if (Input.GetKeyUp(KeyCode.Escape)) {
                if (context.ToolManager.HasActiveTool(0) || context.ToolManager.HasActiveTool(1)) {
                    OGActions.CancelCurrentTool();
                } else if (context.Scene.Selected.Count > 0) {
                    context.Scene.ClearSelection();
                }
                return true;


            // ENTER AND LETTER A APPLY CURRENT TOOL IF POSSIBLE
            } else if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.A)) {
                if (OGActions.CanAcceptCurrentTool())
                    OGActions.AcceptCurrentTool();
                return true;

            } else if (Input.GetKeyUp(KeyCode.T)) {
                //RMSTests.TestIsoCurve();
                return true;

            } else if (Input.GetKeyUp(KeyCode.Delete)) {
                if (context.Scene.Selected.Count == 1) {
                    DeleteSOChange change = new DeleteSOChange() { scene = context.Scene, so = context.Scene.Selected[0] };
                    context.Scene.History.PushChange(change, false);
                }
                return true;


                // CENTER TARGET (??)
            } else if (Input.GetKeyUp(KeyCode.C)) {
                Ray3f cursorRay = context.MouseController.CurrentCursorWorldRay();
                AnyRayHit hit = null;
                if (context.Scene.FindSceneRayIntersection(cursorRay, out hit)) {
                    context.ActiveCamera.Manipulator().ScenePanFocus(context.Scene, context.ActiveCamera, hit.hitPos, true);
                }
                return true;

                // TOGGLE FRAME TYPE
            } else if (Input.GetKeyUp(KeyCode.F)) {
                FrameType eCur = context.TransformManager.ActiveFrameType;
                context.TransformManager.ActiveFrameType = (eCur == FrameType.WorldFrame)
                    ? FrameType.LocalFrame : FrameType.WorldFrame;
                return true;

            } else if (Input.GetKeyUp(KeyCode.D)) {
                return true;

                // VISIBILITY  (V HIDES, SHIFT+V SHOWS)
            } else if (Input.GetKeyUp(KeyCode.V)) {
                // show/hide (should be abstracted somehow?? instead of directly accessing GOs?)
                if (bShiftDown) {
                    foreach (SceneObject so in context.Scene.SceneObjects)
                        so.RootGameObject.Show();
                } else {
                    foreach (SceneObject so in context.Scene.Selected)
                        so.RootGameObject.Hide();
                    context.Scene.ClearSelection();
                }
                return true;

                // UNDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Z)) {
                context.Scene.History.InteractiveStepBack();
                return true;

                // REDO
            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Y)) {
                context.Scene.History.InteractiveStepForward();
                return true;


            } else if (Input.GetKeyUp(KeyCode.Backspace)) {
                if (OG.IsInState(OGWorkflow.SocketState) && OG.CanTransitionToState(OGWorkflow.RectifyState)) {
                    OG.TransitionToState(OGWorkflow.RectifyState);
                    OG.Leg.SetOpWidgetVisibility(true);
                }
                return true;

            } else if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)) {
                float sign = Input.GetKeyUp(KeyCode.UpArrow) ? 1 : -1;
                if (OG.LastActiveModelingOp != null) {
                    if (OG.LastActiveModelingOp is PlaneBandExpansionOp) {
                        PlaneBandExpansionOp deform = OG.LastActiveModelingOp as PlaneBandExpansionOp;
                        deform.BandDistance = MathUtil.Clamp(deform.BandDistance + sign * 2.0f, 10.0f, 1000.0f);
                    }
                    if (OG.LastActiveModelingOp is EnclosedRegionSmoothOp) {
                        EnclosedRegionSmoothOp deform = OG.LastActiveModelingOp as EnclosedRegionSmoothOp;
                        deform.OffsetDistance += sign * 0.1f;
                    }
                }
                return true;

            } else if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)) {
                float sign = Input.GetKeyUp(KeyCode.RightArrow) ? 1 : -1;
                if (OG.LastActiveModelingOp != null) {
                    if (OG.LastActiveModelingOp is PlaneBandExpansionOp) {
                        PlaneBandExpansionOp deform = OG.LastActiveModelingOp as PlaneBandExpansionOp;
                        deform.PushPullDistance += sign * 0.25f;
                    }
                    if (OG.LastActiveModelingOp is EnclosedRegionOffsetOp) {
                        EnclosedRegionOffsetOp deform = OG.LastActiveModelingOp as EnclosedRegionOffsetOp;
                        deform.PushPullDistance += sign * 0.25f;
                    }
                    if (OG.LastActiveModelingOp is EnclosedRegionSmoothOp) {
                        EnclosedRegionSmoothOp deform = OG.LastActiveModelingOp as EnclosedRegionSmoothOp;
                        deform.SmoothAlpha += sign * 0.1f;
                    }

                } else if ( bCtrlDown && bShiftDown && OG.Model.HasSocket() ) {
                    int debug = OG.Socket.DeviceGenerator.DebugStep;
                    if (debug == int.MaxValue) debug = 0;
                    else debug = MathUtil.Clamp(debug + (int)sign, 0, 10);
                    OG.Socket.DeviceGenerator.DebugStep = debug;
                }

                return true;
            } else if (Input.GetKeyUp(KeyCode.LeftBracket) || Input.GetKeyUp(KeyCode.RightBracket)) {
                SculptCurveTool tool = OG.ActiveToolAs<SculptCurveTool>();
                if (tool != null) {
                    float fSign = Input.GetKeyUp(KeyCode.LeftBracket) ? -1 : 1;
                    double fRadiusS = tool.Radius.SceneValue;
                    fRadiusS = MathUtil.Clamp(fRadiusS + 2.5 * fSign, 5.0, 100.0);
                    tool.Radius = fDimension.Scene(fRadiusS);
                }
                return true;


                // REDO
            } else if (Input.GetKeyUp(KeyCode.M)) {
                if (OG.IsInState(OGWorkflow.ScanState))
                    context.Scene.Select(OG.Scan.SO, true);
                else if (OG.IsInState(OGWorkflow.RectifyState) )
                    context.Scene.Select(OG.Leg.SO, true);
                if (context.Scene.Selected.Count > 0) {
                    context.ToolManager.SetActiveToolType(TwoPointMeasureTool.Identifier, 0);
                    context.ToolManager.ActivateTool(0);
                }
                return true;


            } else if (Input.GetKeyUp(KeyCode.L)) {
                OGActions.AddLengthenOp();
                return true;

            } else if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.R)) {
                OGSerializer serializer = new OGSerializer();
                if (Input.GetKeyUp(KeyCode.R)) {
                    serializer.RestoreToCurrent("c:\\scratch\\OGSCENE.txt");
                } else {
                    serializer.StoreCurrent("c:\\scratch\\OGSCENE.txt");
                }
                return true;


            } else
                return false;
        }
    }

}