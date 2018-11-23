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

    class SetupOrthoVRCockpit : ICockpitInitializer
    {
        public void Initialize(Cockpit cockpit)
        {
            cockpit.Name = "modelCockpit";



            // Configure how the cockpit moves

            //cockpit.PositionMode = Cockpit.MovementMode.TrackPosition;
            // [RMS] use orientation mode to make cockpit follow view orientation.
            //  (however default widgets below are off-screen!)
            //cockpit.PositionMode = Cockpit.MovementMode.TrackOrientation;
            var tracker = SmoothCockpitTracker.Enable(cockpit);
            //cockpit.TiltAngle = 10.0f;
            cockpit.TiltAngle = 0.0f;
            tracker.ShowIndicator = false;
            



            FScene Scene = cockpit.Scene;
            //ISurfaceBoxRegion region = new CylinderBoxRegion() { Radius = 1.0f, MinHeight = -1.0f, MaxHeight = 0.3f, HorzDegreeLeft = 50, HorzDegreeRight = 50 };
            ISurfaceBoxRegion region = new SphereBoxRegion() { Radius = 1.0f, VertDegreeBottom = 30, VertDegreeTop = 10, HorzDegreeLeft = 55, HorzDegreeRight = 55 };
            //Frame3f f = new Frame3f(new Vector3f(0, 0, 1), Vector3f.AxisZ);
            //f.RotateAround(Vector3f.Zero, Quaternionf.AxisAngleD(Vector3f.AxisX, 10));
            //f.RotateAround(Vector3f.Zero, Quaternionf.AxisAngleD(Vector3f.AxisY, -50));
            //ISurfaceBoxRegion region = new PlaneBoxRegion() {
            //    Frame = f, Dimensions = new AxisAlignedBox2f(-0.15f, -0.5f, 0.5f, 0.2f)
            //};
            BoxContainer leftPanelContainer = new BoxContainer(new BoxRegionContainerProvider(cockpit, region));
            PinnedBoxes3DLayoutSolver leftPanelLayout = new PinnedBoxes3DLayoutSolver(leftPanelContainer, region);
            PinnedBoxesLayout layout = new PinnedBoxesLayout(cockpit, leftPanelLayout) {
                StandardDepth = 0.0f
            };
            cockpit.AddLayout(layout, "3D", true);


            ISurfaceBoxRegion cylregion = new CylinderBoxRegion() { Radius = 1.0f, MinHeight = -1.0f, MaxHeight = 0.2f, HorzDegreeLeft = 55, HorzDegreeRight = 50 };
            BoxContainer cylPanelContainer = new BoxContainer(new BoxRegionContainerProvider(cockpit, cylregion));
            PinnedBoxes3DLayoutSolver cylPanelLayout = new PinnedBoxes3DLayoutSolver(cylPanelContainer, cylregion);
            PinnedBoxesLayout cyl_layout = new PinnedBoxesLayout(cockpit, cylPanelLayout) {
                StandardDepth = 0.0f
            };
            cockpit.AddLayout(cyl_layout, "cylinder", true);


            float button_width = 0.32f;
            float button_height = 0.075f;
            float button_spacing = 0.015f;
            float text_height = button_height * 0.6f;
            float row_y_shift = button_height + button_spacing;



            Func<string, float, HUDLabel> MakeButtonF = (label, buttonW) => {
                HUDLabel button = new HUDLabel() {
                    Shape = OrthogenUI.MakeMenuButtonRect(buttonW, button_height),
                    TextHeight = text_height,
                    AlignmentHorz = HorizontalAlignment.Center,
                    BackgroundColor = OrthogenUI.ButtonBGColor, 
                    TextColor = OrthogenUI.ButtonTextColor,
                    DisabledTextColor = OrthogenUI.DisabledButtonTextColor,
                    Text = label,
                    EnableBorder = false, BorderWidth = OrthogenUI.StandardButtonBorderWidth, BorderColor = OrthogenUI.ButtonTextColor
                };
                button.Create();
                button.Name = label;
                button.Enabled = true;
                return button;
            };


            /*
             * Scan UI
             */

            HUDElementList scan_buttons_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
				Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Vertical
            };


            HUDLabel trim_scan_button = MakeButtonF("Trim Scan", button_width);
            trim_scan_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.TrimScanStartT);
            };
            scan_buttons_list.AddListItem(trim_scan_button);

            HUDLabel align_scan_button = MakeButtonF("Align Scan", button_width);
            align_scan_button.OnClicked += (sender, e) => {
                OG.Transition(OGVRWorkflow.VRAlignScanStartT);
            };
            scan_buttons_list.AddListItem(align_scan_button);

            HUDLabel accept_scan_button = MakeButtonF("Done Scan", button_width);
            accept_scan_button.OnClicked += (sender, e) => {
                OG.TransitionToState(RectifyState.Identifier);
            };
            scan_buttons_list.AddListItem(accept_scan_button);

            scan_buttons_list.Create();
            scan_buttons_list.Name = "scan_buttons_list";
            cyl_layout.Add(scan_buttons_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(scan_buttons_list, BoxPosition.CenterTop),
                PinTargetPoint2D = LayoutUtil.BoxPointF(cylPanelContainer, BoxPosition.TopLeft)
            });


            /*
             * Model UI UI
             */


            HUDElementList model_buttons_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Vertical
            };


            HUDLabel draw_offset_area_button = MakeButtonF("Offset Area", button_width);
            draw_offset_area_button.OnClicked += (sender, e) => {
                OGActions.CurrentLegDeformType = LegModel.LegDeformationTypes.Offset;
                OG.Transition(OGWorkflow.DrawAreaStartT);
            };
            model_buttons_list.AddListItem(draw_offset_area_button);

            HUDLabel draw_smooth_area_button = MakeButtonF("Smooth Area", button_width);
            draw_smooth_area_button.OnClicked += (sender, e) => {
                OGActions.CurrentLegDeformType = LegModel.LegDeformationTypes.Smooth;
                OG.Transition(OGWorkflow.DrawAreaStartT);
            };
            model_buttons_list.AddListItem(draw_smooth_area_button);


            HUDLabel add_plane_button = MakeButtonF("Add Plane", button_width);
            add_plane_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.AddDeformRingStartT);
            };
            model_buttons_list.AddListItem(add_plane_button);

            HUDLabel add_lengthen_button = MakeButtonF("Add Lengthen", button_width);
            add_lengthen_button.OnClicked += (sender, e) => {
                if (OGActions.CanAddLengthenOp())
                    OGActions.AddLengthenOp();
            };
            model_buttons_list.AddListItem(add_lengthen_button);

            HUDLabel sculpt_curve_model_button = MakeButtonF("Sculpt Curve", button_width);
            WorkflowRouter sculpt_router = WorkflowRouter.Build(new[] {
                OGWorkflow.RectifyState, OGWorkflow.SculptAreaStartT,
                OGWorkflow.SocketState, OGWorkflow.SculptTrimlineStartT });
            sculpt_curve_model_button.OnClicked += (sender, e) => {
                sculpt_router.Apply(OG.Model.Workflow);
			};
            model_buttons_list.AddListItem(sculpt_curve_model_button);


            HUDLabel accept_rectify_button = MakeButtonF("Begin Socket", button_width);
            accept_rectify_button.OnClicked += (sender, e) => {
                OG.TransitionToState(SocketDesignState.Identifier);
                OG.Leg.SetOpWidgetVisibility(false);
            };
            model_buttons_list.AddListItem(accept_rectify_button);


            model_buttons_list.Create();
            model_buttons_list.Name = "model_buttons_list";
            cyl_layout.Add(model_buttons_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(model_buttons_list, BoxPosition.CenterTop),
                PinTargetPoint2D = LayoutUtil.BoxPointF(cylPanelContainer, BoxPosition.TopLeft)
            });




            /*
             * Model UI UI
             */


            HUDElementList socket_buttons_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Vertical
            };



            HUDLabel draw_trim_line_button = MakeButtonF("Draw Trimline", button_width);
            draw_trim_line_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.DrawTrimlineStartT);
            };
            socket_buttons_list.AddListItem(draw_trim_line_button);

            HUDLabel plane_trim_line_button = MakeButtonF("Plane Trimline", button_width);
            plane_trim_line_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.PlaneTrimlineStartT);
            };
            socket_buttons_list.AddListItem(plane_trim_line_button);

            HUDLabel sculpt_trimline_button = MakeButtonF("Sculpt Trimline", button_width);
            sculpt_trimline_button.OnClicked += (sender, e) => {
                OG.Transition(OGWorkflow.SculptTrimlineStartT);
            };
            socket_buttons_list.AddListItem(sculpt_trimline_button);

            HUDLabel add_socket_button = MakeButtonF("Add Socket", button_width);
            add_socket_button.OnClicked += (sender, e) => {
                if ( OGActions.CanAddSocket() )
                    OGActions.AddSocket();
            };
            socket_buttons_list.AddListItem(add_socket_button);

            HUDLabel export_socket_button = MakeButtonF("Export", button_width);
            export_socket_button.OnClicked += (sender, e) => {
                if (OGActions.CanExportSocket())
                    OGActions.ExportSocket();
            };
            socket_buttons_list.AddListItem(export_socket_button);

            // align button list top top-left of ui
            socket_buttons_list.Create();
            socket_buttons_list.Name = "socket_buttons";
            cyl_layout.Add(socket_buttons_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(socket_buttons_list, BoxPosition.CenterTop),
                PinTargetPoint2D = LayoutUtil.BoxPointF(cylPanelContainer, BoxPosition.TopLeft)
            });





            HUDElementList ok_cancel_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Horizontal
            };

            HUDLabel accept_button = MakeButtonF("Accept", button_width*0.75f);
            accept_button.OnClicked += (sender, e) => {
                OGActions.AcceptCurrentTool();
            };

            HUDLabel cancel_button = MakeButtonF("Cancel", button_width * 0.75f);
            cancel_button.OnClicked += (sender, e) => {
                OGActions.CancelCurrentTool();
            };

            ok_cancel_list.AddListItem(accept_button);
            ok_cancel_list.AddListItem(cancel_button);


            // align button list top top-left of ui
            ok_cancel_list.Create();
            ok_cancel_list.Name = "ok_cancel_list";
            layout.Add(ok_cancel_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(ok_cancel_list, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(leftPanelContainer, BoxPosition.BottomLeft)
            });



            HUDElementList size_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Horizontal
            };

            HUDLabel size_1to1 = MakeButtonF("Real Size", button_width * 0.75f);
            size_1to1.OnClicked += (sender, e) => {
                OGActions.SetSizeMode(OGActions.SizeModes.RealSize);
                OGActions.RecenterVRView(false);
            };

            HUDLabel size_medium = MakeButtonF("Zoom Size", button_width * 0.75f);
            size_medium.OnClicked += (sender, e) => {
                OGActions.SetSizeMode(OGActions.SizeModes.DemoSize);
                OGActions.RecenterVRView(false);
            };

            size_list.AddListItem(size_1to1);
            size_list.AddListItem(size_medium);

            size_list.Create();
            size_list.Name = "size_list";
            layout.Add(size_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(size_list, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(leftPanelContainer, BoxPosition.BottomLeft),
                FrameAxesShift = new Vector3f(0, -row_y_shift, 0)
            });






            HUDElementList view_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Horizontal
            };


            HUDLabel recenter_button = MakeButtonF("Recenter", 2 * button_width * 0.75f);
            recenter_button.OnClicked += (sender, e) => {
                OGActions.RecenterVRView(true);
            };

            view_list.AddListItem(recenter_button);

            view_list.Create();
            view_list.Name = "view_list";
            layout.Add(view_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(view_list, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(leftPanelContainer, BoxPosition.BottomLeft),
                FrameAxesShift = new Vector3f(0, -2*row_y_shift, 0)
            });





            HUDElementList capture_list = new HUDElementList() {
                Width = button_width,
                Height = button_height,
                Spacing = button_spacing,
                SizeMode = HUDElementList.SizeModes.AutoSizeToFit,
                Direction = HUDElementList.ListDirection.Horizontal
            };

            HUDLabel capture_button = MakeButtonF("Capture", button_width * 0.75f);
            capture_button.OnClicked += (sender, e) => {
                if (FBCapture.CaptureOption.Active != null) {
                    if (capture_button.Text == "Capture") {
                        DebugUtil.Log("Starting 2D Capture...");
                        FBCapture.CaptureOption.Active.doSurroundCaptureOption = false;
                        cockpit.Context.RegisterNextFrameAction(() => {
                            //FBCapture.CaptureOption.Active.videoWidth = 4096;
                            //FBCapture.CaptureOption.Active.videoHeight = 2048;
                            FBCapture.CaptureOption.Active.StartCaptureVideo();
                            capture_button.Text = "Stop";
                        });
                    } else {
                        FBCapture.CaptureOption.Active.StopCaptureVideo();
                        capture_button.Text = "Capture";
                    }
                }
            };


            HUDLabel vrcapture_button = MakeButtonF("VRCapture", button_width * 0.75f);
            vrcapture_button.OnClicked += (sender, e) => {
                if (FBCapture.CaptureOption.Active != null) {
                    if (vrcapture_button.Text == "VRCapture") {
                        
                        // [RMS] when we set this flag, we need to give CaptureOption.Update() a chance to see 
                        //  it, which means we need to wait up to 2 frames
                        FBCapture.CaptureOption.Active.doSurroundCaptureOption = true;
                        cockpit.Context.RegisterNextFrameAction(() => {
                            cockpit.Context.RegisterNextFrameAction(() => {
                                GameObject encoderObj = GameObject.Find("EncoderObject");
                                encoderObj.transform.position = Camera.main.transform.position;
                                encoderObj.transform.rotation = Quaternionf.Identity;
                                GameObject head = UnityUtil.FindGameObjectByName("VRHead");
                                head.SetVisible(false);
                                FBCapture.CaptureOption.Active.StartCaptureVideo();
                                vrcapture_button.Text = "Stop";
                            });
                        });
                    } else {
                        FBCapture.CaptureOption.Active.StopCaptureVideo();
                        vrcapture_button.Text = "VRCapture";
                    }
                }
            };

            capture_list.AddListItem(capture_button);
            capture_list.AddListItem(vrcapture_button);


            // align button list top top-left of ui
            capture_list.Create();
            capture_list.Name = "capture_list";
            layout.Add(capture_list, new LayoutOptions() {
                Flags = LayoutFlags.None,
                PinSourcePoint2D = LayoutUtil.LocalBoxPointF(capture_list, BoxPosition.CenterBottom),
                PinTargetPoint2D = LayoutUtil.BoxPointF(leftPanelContainer, BoxPosition.BottomLeft),
                FrameAxesShift = new Vector3f(0, -3*row_y_shift, 0)
            });






            leftPanelLayout.RecomputeLayout();
            leftPanelLayout.RecomputeLayout();
            leftPanelLayout.RecomputeLayout();
            leftPanelLayout.RecomputeLayout();
            leftPanelLayout.RecomputeLayout();

            cylPanelLayout.RecomputeLayout();


            // Configure interaction behaviors
            //   - below we add behaviors for mouse, gamepad, and spatial devices (oculus touch, etc)
            //   - keep in mind that Tool objects will register their own behaviors when active

            // setup key handlers (need to move to behavior...)
            cockpit.AddKeyHandler(new OrthoVRKeyHandler(cockpit.Context));

            // these behaviors let us interact with UIElements (ie left-click/trigger, or either triggers for Touch)
            if ( cockpit.Context.Use2DCockpit )
                cockpit.InputBehaviors.Add(new Mouse2DCockpitUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRSpatialDeviceUIBehavior(cockpit.Context) { Priority = 0 });
            cockpit.InputBehaviors.Add(new VRMouseUIBehavior(cockpit.Context) { Priority = 1 });

            cockpit.InputBehaviors.Add(new SpatialDeviceGrabViewBehavior(cockpit) { Priority = 2 });

            //cockpit.InputBehaviors.Add(new TwoHandViewManipBehavior(cockpit) { Priority = 1 });
            //cockpit.InputBehaviors.Add(new SpatialDeviceViewManipBehavior(cockpit) { Priority = 2 });


            // selection / multi-selection behaviors
            cockpit.InputBehaviors.Add(new MouseMultiSelectBehavior(cockpit.Context) { Priority = 10 });
            cockpit.InputBehaviors.Add(new SpatialDeviceMultiSelectBehavior(cockpit.Context) { Priority = 10 });


            cockpit.InputBehaviors.Add(new MouseDeselectBehavior(cockpit.Context) { Priority = 999 });
            cockpit.InputBehaviors.Add(new SpatialDeviceDeselectBehavior(cockpit.Context) { Priority = 999 });





            // update buttons enable/disable on state transitions, selection changes
            string main_state = "";
            Action updateStateChangeButtons = () => {

                if (OG.IsInState(OGWorkflow.ScanState))
                    main_state = OGWorkflow.ScanState;
                else if (OG.IsInState(OGWorkflow.RectifyState))
                    main_state = OGWorkflow.RectifyState;
                else if (OG.IsInState(OGWorkflow.SocketState))
                    main_state = OGWorkflow.SocketState;

                scan_buttons_list.IsVisible = (main_state == OGWorkflow.ScanState);
                model_buttons_list.IsVisible = (main_state == OGWorkflow.RectifyState);
                socket_buttons_list.IsVisible = (main_state == OGWorkflow.SocketState);

                trim_scan_button.Enabled = OG.CanTransition(OGWorkflow.TrimScanStartT);
                align_scan_button.Enabled = OG.CanTransition(OGVRWorkflow.VRAlignScanStartT);
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

                sculpt_curve_model_button.Enabled = sculpt_router.CanApply(OG.Model.Workflow);
                sculpt_trimline_button.Enabled = OG.CanTransition(OGWorkflow.SculptTrimlineStartT);
            };
            OG.OnWorfklowInitialized += (o,e) => { updateStateChangeButtons(); };
            OG.OnStateTransition += (from, to) => { updateStateChangeButtons(); };
            OG.OnDataModelModified += (from, to) => { updateStateChangeButtons(); };
            cockpit.Scene.SelectionChangedEvent += (o,e) => { if (OG.WorkflowInitialized) updateStateChangeButtons(); };
            cockpit.Scene.ChangedEvent += (scene,so,type) => { if (OG.WorkflowInitialized) updateStateChangeButtons(); };

            // accept/cancel buttons need to be checked every frame because the CanApply state
            // could change at any time, and there is no event about it
            cockpit.Context.RegisterEveryFrameAction(() => {
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












    public class OrthoVRKeyHandler : IShortcutKeyHandler
    {
        FContext context;
        public OrthoVRKeyHandler(FContext c)
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
                }
                return true;



            } else if (Input.GetKeyUp(KeyCode.LeftBracket) || Input.GetKeyUp(KeyCode.RightBracket)) {
                SculptCurveTool tool = OG.ActiveToolAs<SculptCurveTool>();
                if (tool != null) {
                    float fSign = Input.GetKeyUp(KeyCode.LeftBracket) ? -1 : 1;
                    double fRadiusS = tool.Radius.SceneValue;
                    fRadiusS = MathUtil.Clamp(fRadiusS + 2.5*fSign, 5.0, 100.0);
                    tool.Radius = fDimension.Scene(fRadiusS);
                }
                return true;



            } else if (bCtrlDown && Input.GetKeyUp(KeyCode.Q)) {
                FPlatform.QuitApplication();
                return true;


            } else
                return false;
        }
    }

}
