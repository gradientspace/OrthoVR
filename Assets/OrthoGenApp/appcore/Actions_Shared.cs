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
    /// Actions for several parts of Orthogen workflow that are shared between different stages
    /// </summary>
    public static partial class OGActions
    {


        /*
         * setup/teardown for Sculpt Curve tool, which is applied to area curves, trimline curve, etc
         */
        public static bool CanSculptCurve()
        {
            var M = OG.Model;
            if ( M.Context.ToolManager.ActiveRightTool != null )
                return false;

            // can always sculpt trimline
            if (SceneUtil.IsSelectionMatch(M.Scene, typeof(TrimLoopSO), 1) )
                return true;

            // if polycurve, check that it is attached to the right kind of modeling op
            if (SceneUtil.IsSelectionMatch(M.Scene, typeof(PolyCurveSO), 1) == false)
                return false;
            PolyCurveSO so = M.Scene.Selected[0] as PolyCurveSO;
            ModelingOperator op = M.leg.FindOpForSO(so);
            if (op is EnclosedRegionOffsetOp || op is EnclosedRegionSmoothOp)
                return true;
            return false;
        }
        public static void BeginSculptCurveTool()
        {
            var M = OG.Model;

            if (M.Scene.Selected.Count != 1 || M.Scene.Selected[0] is PolyCurveSO == false) {
                throw new Exception("OGActions.BeginSculptCurveTool: invalid selection to start Sculpt Curve tool!");
            }
            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Context.ToolManager.SetActiveToolType(BodyModelTools.SculptRegionCurveIdentifer, ToolSide.Right);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);

            SculptCurveTool tool = M.Context.ToolManager.ActiveRightTool as SculptCurveTool;
            tool.ProjectionTarget = new SOProjectionTarget(OG.Leg.SO, CoordSpace.WorldCoords) { Offset = M.Scene.ToWorldDimension(0.5f) };
            tool.BrushTarget = new SOWorldIntersectionTarget(OG.Leg.SO);
        }
        public static void EndSculptCurveTool()
        {
            OG.Model.Context.ToolManager.DeactivateTools();
        }







        static WorkflowRouter cancel_router;
        public static void InitializeCancelRouter()
        {
            // [TODO] this might have to adapt to different graphs
            cancel_router = WorkflowRouter.Build(new[] {
                    OGWorkflow.TrimScanState, OGWorkflow.TrimScanCancelT,
                    OGWorkflow.AlignScanState, OGWorkflow.AlignScanCancelT,
                    OGWorkflow.DrawAreaState, OGWorkflow.DrawAreaExitT,
                    OGWorkflow.AddDeformRingState, OGWorkflow.AddDeformRingCancelT,
                    OGWorkflow.DrawTrimlineState, OGWorkflow.DrawTrimlineExitT,
                    OGWorkflow.SculptAreaState, OGWorkflow.SculptAreaExitT,
                    OGWorkflow.PlaneTrimlineState, OGWorkflow.PlaneTrimlineCancelT,
                    OGWorkflow.SculptTrimlineState, OGWorkflow.SculptTrimlineExitT
                });
            cancel_router.UnknownAction = () => { throw new Exception("CANCEL BUTTON: not sure what state to transition to?"); };
        }
        public static void CancelCurrentTool()
        {
            if (cancel_router == null)
                InitializeCancelRouter();
            cancel_router.Apply(OG.Model.Workflow);
        }
        public static void AddToCancelRouter(string state, string transition) {
            cancel_router.AddTransition(state, transition);
        }



        static WorkflowRouter accept_router;
        public static void InitializeAcceptRouter()
        {
            // [TODO] this might have to adapt to different graphs
            accept_router = WorkflowRouter.Build(new[] {
                OGWorkflow.TrimScanState, OGWorkflow.TrimScanAcceptT,
                OGWorkflow.AlignScanState, OGWorkflow.AlignScanAcceptT,
                OGWorkflow.AddDeformRingState, OGWorkflow.AddDeformRingAcceptT,
                OGWorkflow.PlaneTrimlineState, OGWorkflow.PlaneTrimlineAcceptT
            });
            accept_router.UnknownAction = () => { throw new Exception("ACCEPT BUTTON: not sure what state to transition to?"); };
        }
        public static bool CanAcceptCurrentTool()
        {
            if (accept_router == null)
                InitializeAcceptRouter();
            return accept_router.CanApply(OG.Model.Workflow);
        }
        public static void AcceptCurrentTool()
        {
            if (accept_router == null)
                InitializeAcceptRouter();
            accept_router.Apply(OG.Model.Workflow);
        }
        public static void AddToAcceptRouter(string state, string transition) {
            accept_router.AddTransition(state, transition);
        }




    }
}
