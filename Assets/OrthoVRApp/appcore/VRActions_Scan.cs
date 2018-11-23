using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;
using gs;
using gsbody;

namespace orthogen
{
    public static partial class OGActions
    {


        /*
         * These are the actions we use for the Align tool workflow
         */
        public static bool CanVRAlignScan()
        {
            var M = OG.Model;
            return M.Context.ToolManager.ActiveRightTool == null;
        }
        public static void BeginVRAlignScanTool()
        {
            var M = OG.Model;

            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
            M.Context.ToolManager.SetActiveToolType(SpatialDeviceScanAlignmentTool.Identifier, ToolSide.Right);
            M.Scene.Select(OG.Scan.SO, true);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);
        }
        public static bool CanAcceptVRAlignScanTool()
        {
            var M = OG.Model;
            return (M.Context.ToolManager.ActiveRightTool != null) && M.Context.ToolManager.ActiveRightTool.CanApply;
        }
        public static void AcceptVRAlignScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.ActiveRightTool.Apply();
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }
        public static void CancelVRAlignScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }

    }
}
