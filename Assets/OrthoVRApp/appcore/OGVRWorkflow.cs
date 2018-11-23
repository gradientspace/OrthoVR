using System;
using gs;
using gsbody;

namespace orthogen
{
    public static class OGVRWorkflow
    {
        public static string VRAlignScanState = "VRAlignScan";
        public static string VRAlignScanStartT = "VRAlign_Start";
        public static string VRAlignScanAcceptT = "VRAlign_Accept";
        public static string VRAlignScanCancelT = "VRAlign_Cancel";




        public static void AddVRWorkflowGraphTransitions(SocketDesignGraph graph)
        {
            WorkflowBuilder builder = new WorkflowBuilder(graph);

            WorkflowState scanState = graph.FindStateByName(ScanState.Identifier);

            var alignScanTool = builder.AddState(OGVRWorkflow.VRAlignScanState, OGActions.BeginVRAlignScanTool);
            builder.AddTransition(scanState, alignScanTool, OGVRWorkflow.VRAlignScanStartT, OGActions.CanVRAlignScan, null, null);
            builder.AddTransition(alignScanTool, scanState, OGVRWorkflow.VRAlignScanAcceptT, OGActions.CanAcceptVRAlignScanTool, OGActions.AcceptVRAlignScanTool, null);
            builder.AddTransition(alignScanTool, scanState, OGVRWorkflow.VRAlignScanCancelT, null, OGActions.CancelVRAlignScanTool, null);
        }

    }


}
