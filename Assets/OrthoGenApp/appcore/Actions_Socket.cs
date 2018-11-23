using System;
using System.Collections.Generic;
using System.IO;
using g3;
using f3;
using gs;
using gsbody;

namespace orthogen
{
    /// <summary>
    /// Actions for Socket step of Orthogen workflow
    /// </summary>
    public static partial class OGActions
    {
        // File dialog will open here
        public static string ExportSocketPath = "c:\\scratch\\";

        // if false, then socket just gets written out automatically (Ryan likes this)
        public static bool ShowExportDialogInEditor = false;


        /// <summary>
        /// this is set as result action in draw-surface-loop tool, converts curve preview into a TrimLoopSO
        /// </summary>
        public static void EmitTrimCurveFromPreview(CurvePreview preview)
        {
            TrimLoopSO curveSO = TrimLoopSO.CreateFromPreview(preview, OrthogenMaterials.TrimLoopMaterial, OG.Scene);
            AddNewTrimCurve(curveSO);

            // next frame, transition, and select the curve
            OG.Context.RegisterNextFrameAction(() => {
                OG.Transition(OGWorkflow.DrawTrimlineExitT);
                OG.Scene.Select(curveSO, true);
            });
        }


        /// <summary>
        /// add new trimloop to datamodel
        /// </summary>
        public static void AddNewTrimCurve(TrimLoopSO curveSO)
        {
            curveSO.AssignSOMaterial(OrthogenMaterials.TrimLoopMaterial);
            curveSO.ConnectToTarget(OG.Leg, OG.Leg.SO, true);
            curveSO.Name = "TrimLine";
            curveSO.RootGameObject.SetLayer(OrthogenMaterials.CurvesLayer);
            OG.Model.InitializeTrimline(curveSO);

            // if we delete this SO, we need to update some things
            OG.Model.RegisterDeleteSOAction(curveSO, OnTrimLineDeleted);

        }



        /*
         * These are the actions we use for the Trim-Line tool (ie draw closed loop to select region)
         */
        public static bool CanDrawTrimLine()
        {
            var M = OG.Model;
            return M.Workflow.IsInState(SocketDesignState.Identifier) && M.trimline == null && M.Context.ToolManager.ActiveRightTool == null;
        }
        public static void BeginDrawTrimLineTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Scene.ClearSelection();
            M.Context.ToolManager.SetActiveToolType(BodyModelTools.DrawTrimLineIdentifier, ToolSide.Right);
            M.Scene.Select(OG.Leg.SO, true);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);

            DrawSurfaceCurveTool tool = M.Context.ToolManager.ActiveRightTool as DrawSurfaceCurveTool;
            var target = new SOProjectionTarget(OG.Leg.SO) { Offset = M.Scene.ToWorldDimension(0.5f) };
            tool.CurveProcessorF = gs.CurveDrawingUtil.MakeLoopOnSurfaceProcessorF(M.Scene, target,
                () => { return tool.SamplingRateScene; }, 
                () => { return tool.Closed; },
                0.5f, 25);
        }
        public static void EndDrawTrimLineTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
        }








        /// <summary>
        /// initial trimline from plane-cut
        /// </summary>
        public static void EmitTrimCurveFromPlane(SceneObject targetLegSO, Frame3f planeFrameS)
        {
            TrimLoopSO curveSO = TrimLoopSO.CreateFromPlane(
                targetLegSO as DMeshSO, planeFrameS, OrthogenMaterials.PlaneCurveMaterial,
                targetLegSO.GetScene(), OrthogenUI.CurveOnSurfaceOffsetTol);
            curveSO.ConnectToTarget(OG.Leg, OG.Leg.SO, true);
            curveSO.Name = "TrimLine";
            curveSO.RootGameObject.SetLayer(OrthogenMaterials.CurvesLayer);
            OG.Model.InitializeTrimline(curveSO);

            // if we delete this SO, we need to update some things
            OG.Model.RegisterDeleteSOAction(curveSO, OnTrimLineDeleted);

            // next frame, transition, and select the curve
            OG.Context.RegisterNextFrameAction(() => {
                OG.Scene.Select(curveSO, true);
            });
        }



        /*
         * These are the actions we use for the Trim-Line tool (ie draw closed loop to select region)
         */
        public static bool CanAddPlaneTrimLine()
        {
            var M = OG.Model;
            return M.Workflow.IsInState(SocketDesignState.Identifier) && M.trimline == null && M.Context.ToolManager.ActiveRightTool == null;
        }
        public static void BeginPlaneTrimLineTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Scene.ClearSelection();
            M.Context.ToolManager.SetActiveToolType(BodyModelTools.CreateTrimlineFromPlaneIdentifier, ToolSide.Right);
            M.Scene.Select(OG.Leg.SO, true);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);
        }
        public static bool CanAcceptPlaneTrimLineTool()
        {
            var M = OG.Model;
            return (M.Context.ToolManager.ActiveRightTool != null) && M.Context.ToolManager.ActiveRightTool.CanApply;
        }
        public static void AcceptPlaneTrimLineTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.ActiveRightTool.Apply();
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
        }
        public static void CancelPlaneTrimLineTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
        }





        public static void OnTrimLineDeleted()
        {
            OG.Model.RemoveTrimLine();
        }






        public static bool CanAddSocket()
        {
            return OG.IsInState(OGWorkflow.SocketState) 
                && (OG.TrimLine != null) 
                && OG.Model.HasSocket() == false;
        }

        /// <summary>
        /// initialize SocketModel
        /// </summary>
        public static void AddSocket()
        {
            var M = OG.Model;
            SocketSO so = new SocketSO();
            so.Create(new DMesh3(), OrthogenMaterials.SocketMaterial);
            so.Name = "Socket";
            M.Scene.AddSceneObject(so, false);
            OG.Model.InitializeSocket(so);

            // move trimline to goemetry layer so that it is clipped by socket (?)
            OG.TrimLine.SetLayer(FPlatform.GeometryLayer);

            //so.AssignSOMaterial(OrthogenMaterials.LegMaterial);
            //so.SetLayer(FPlatform.WidgetOverlayLayer);
        }




        public static bool CanExportSocket()
        {
            return OG.IsInState(OGWorkflow.SocketState)
                && OG.Model.HasSocket();
        }

        public static void ExportSocket()
        {
            if (OG.Model.HasSocket() == false)
                return;

            string filename = null;
            if (ShowExportDialogInEditor || FPlatform.InUnityEditor() == false) {
                filename = FPlatform.GetSaveFileName("Export Socket",
                    Path.Combine(ExportSocketPath, "socket.obj"), new string[] { "*.obj" }, "Mesh Files (*.OBJ)");
            } else {
                filename = Path.Combine(ExportSocketPath, "socket.obj");
            }
            if (filename == null)
                return;

            DMesh3 SocketMesh = new DMesh3(OG.Socket.Socket.Mesh);
            AxisAlignedBox3d bounds = SocketMesh.CachedBounds;
            MeshTransforms.Translate(SocketMesh, -bounds.Min.y * Vector3d.AxisZ);
            MeshTransforms.FlipLeftRightCoordSystems(SocketMesh);   // convert from unity coordinate system

            WriteOptions opt = WriteOptions.Defaults;
            opt.bWriteGroups = true;
            StandardMeshWriter.WriteMesh(filename, SocketMesh, opt);
        }




    }
}
