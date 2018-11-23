using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using f3;
using gs;
using gsbody;

namespace orthogen
{


    /*
     * Custom states for our workflow graphs
     */ 

    public class ScanState : WorkflowState
    {
        public static string Identifier = OGWorkflow.ScanState;
    }

    public class RectifyState : WorkflowState
    {
        public static string Identifier = OGWorkflow.RectifyState;
    }

    public class SocketDesignState : WorkflowState
    {
        public static string Identifier = OGWorkflow.SocketState;
    }





    /// <summary>
    /// Customized WorkflowGraph for Socket Design Workflow.
    /// use static Build() function to set up the workflow graph.
    /// </summary>
    public class SocketDesignGraph : WorkflowGraph
    {



        public static SocketDesignGraph Build()
        {
            ScanState scanState = new ScanState() { Name = ScanState.Identifier };
            RectifyState rectifyState = new RectifyState() { Name = RectifyState.Identifier };
            SocketDesignState socketState = new SocketDesignState() { Name = SocketDesignState.Identifier };

            SocketDesignGraph graph = new SocketDesignGraph();
            WorkflowBuilder builder = new WorkflowBuilder(graph);

            graph.AddState(scanState);
            graph.AddState(rectifyState);
            graph.AddState(socketState);


            WorkflowTransition toRectify = graph.AddTransition(scanState, rectifyState);
            toRectify.BeforeTransition.Add(() => {
                OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
                OGActions.InitializeLegFromScan();
            });


            WorkflowTransition toSocket = graph.AddTransition(rectifyState, socketState);
            toSocket.BeforeTransition.Add(() => {
                OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
            });

            WorkflowTransition backToRectify = graph.AddTransition(socketState, rectifyState);
            backToRectify.BeforeTransition.Add(() => {
                OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
            });



            // scan tool states

            var trimScanTool = builder.AddState(OGWorkflow.TrimScanState, OGActions.BeginTrimScanTool);
            builder.AddTransition(scanState, trimScanTool, OGWorkflow.TrimScanStartT, OGActions.CanTrimScan, null, null);
            builder.AddTransition(trimScanTool, scanState, OGWorkflow.TrimScanAcceptT, OGActions.CanAcceptTrimScanTool, OGActions.AcceptTrimScanTool, null);
            builder.AddTransition(trimScanTool, scanState, OGWorkflow.TrimScanCancelT, null, OGActions.CancelTrimScanTool, null);

            var alignScanTool = builder.AddState(OGWorkflow.AlignScanState, OGActions.BeginAlignScanTool);
            builder.AddTransition(scanState, alignScanTool, OGWorkflow.AlignScanStartT, OGActions.CanAlignScan, null, null);
            builder.AddTransition(alignScanTool, scanState, OGWorkflow.AlignScanAcceptT, OGActions.CanAcceptAlignScanTool, OGActions.AcceptAlignScanTool, null);
            builder.AddTransition(alignScanTool, scanState, OGWorkflow.AlignScanCancelT, null, OGActions.CancelAlignScanTool, null);


            // rectify tool states

            var drawAreaTool = builder.AddState(OGWorkflow.DrawAreaState, OGActions.BeginDrawAreaTool);
            builder.AddTransition(rectifyState, drawAreaTool, OGWorkflow.DrawAreaStartT, OGActions.CanDrawArea, null, null);
            builder.AddTransition(drawAreaTool, rectifyState, OGWorkflow.DrawAreaExitT, null, OGActions.EndDrawAreaTool, null);

            var deformRingTool = builder.AddState(OGWorkflow.AddDeformRingState, OGActions.BeginDeformRingTool);
            builder.AddTransition(rectifyState, deformRingTool, OGWorkflow.AddDeformRingStartT, OGActions.CanAddDeformRing, null, null);
            builder.AddTransition(deformRingTool, rectifyState, OGWorkflow.AddDeformRingAcceptT, OGActions.CanAcceptDeformRingTool, OGActions.AcceptDeformRingTool, null);
            builder.AddTransition(deformRingTool, rectifyState, OGWorkflow.AddDeformRingCancelT, null, OGActions.CancelDeformRingTool, null);

            var sculptAreaTool = builder.AddState(OGWorkflow.SculptAreaState, OGActions.BeginSculptCurveTool );
            builder.AddTransition(rectifyState, sculptAreaTool, OGWorkflow.SculptAreaStartT, OGActions.CanSculptCurve, null, null);
            builder.AddTransition(sculptAreaTool, rectifyState, OGWorkflow.SculptAreaExitT, null, OGActions.EndSculptCurveTool, null);



            // socket tool states
            var drawTrimlineTool = builder.AddState(OGWorkflow.DrawTrimlineState, OGActions.BeginDrawTrimLineTool);
            builder.AddTransition(socketState, drawTrimlineTool, OGWorkflow.DrawTrimlineStartT, OGActions.CanDrawTrimLine, null, null);
            builder.AddTransition(drawTrimlineTool, socketState, OGWorkflow.DrawTrimlineExitT, null, OGActions.EndDrawTrimLineTool, null);

            var planeTrimlineTool = builder.AddState(OGWorkflow.PlaneTrimlineState, OGActions.BeginPlaneTrimLineTool);
            builder.AddTransition(socketState, planeTrimlineTool, OGWorkflow.PlaneTrimlineStartT, OGActions.CanAddPlaneTrimLine, null, null);
            builder.AddTransition(planeTrimlineTool, socketState, OGWorkflow.PlaneTrimlineAcceptT, OGActions.CanAcceptPlaneTrimLineTool, OGActions.AcceptPlaneTrimLineTool, null);
            builder.AddTransition(planeTrimlineTool, socketState, OGWorkflow.PlaneTrimlineCancelT, null, OGActions.CancelPlaneTrimLineTool, null);

            var sculptTrimlineTool = builder.AddState(OGWorkflow.SculptTrimlineState, OGActions.BeginSculptCurveTool);
            builder.AddTransition(socketState, sculptTrimlineTool, OGWorkflow.SculptTrimlineStartT, OGActions.CanSculptCurve, null, null);
            builder.AddTransition(sculptTrimlineTool, socketState, OGWorkflow.SculptTrimlineExitT, null, OGActions.EndSculptCurveTool, null);


            graph.LogF = DebugUtil.Log;   // print debug output


            return graph;
        }

    }






}
