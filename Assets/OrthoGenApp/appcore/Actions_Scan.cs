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
    /// Actions for Scan Cleanup set of Orthogen workflow
    /// </summary>
    public static partial class OGActions
    {
        /*
         *  CONFIGURATION PARAMETERS
         *  These control behaviors of the actions below
         */ 

        /// <summary>
        /// We auto-shift position the base point of scan this high above ground plane
        /// </summary>
        public static float BaseHeightAboveGroundPlaneMM = 10.0f;


        /// <summary>
        /// If true, when we crop a scan we will try to fill in the hole we create
        /// </summary>
        public static bool FillHoleInScan = true;




        /// <summary>
        /// Initialize the data model to the new-imported-scan state, with the given mesh (presumably loaded from file?)
        /// </summary>
        public static void InitializeScan(DMesh3 mesh)
        {
            AxisAlignedBox3d bounds = mesh.CachedBounds;
            Vector3d translate = -bounds.Center;
            double dy = 0.5 * bounds.Height;
            MeshTransforms.Translate(mesh, translate);

            ScanSO scanSO = new ScanSO();
            scanSO.Create(mesh, OrthogenMaterials.ScanMaterial);
            OG.Scene.AddSceneObject(scanSO);

            Frame3f f = scanSO.GetLocalFrame(CoordSpace.SceneCoords);
            f.Translate((float)dy * Vector3f.AxisY);
            scanSO.SetLocalFrame(f, CoordSpace.SceneCoords);

            OG.Model.InitializeScan(scanSO);
            OG.Model.Workflow.SetInitialState(ScanState.Identifier);

            // reposition camera
            Vector3f c = scanSO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
            //OG.Context.ActiveCamera.Animator().PanFocus(c);
            OG.Context.ActiveCamera.Manipulator().ScenePanFocus(
                OG.Context.Scene, OG.Context.ActiveCamera, c, false);

            // [TODO] this should happen via a transition, I think...

            // set up xforms/etc
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }




        /// <summary>
        /// assuming we restored a ScanSO from save file, restore Scan parts of datamodel
        /// </summary>
        public static void RestoreScan(ScanSO scanSO)
        {
            OG.Model.InitializeScan(scanSO);
            OG.Model.Workflow.SetInitialState(ScanState.Identifier);

            // reposition camera
            Vector3f c = scanSO.GetLocalFrame(CoordSpace.WorldCoords).Origin;
            OG.Context.ActiveCamera.Manipulator().ScenePanFocus(
                OG.Context.Scene, OG.Context.ActiveCamera, c, false);

            // [TODO] this should happen via a transition, I think...

            // set up xforms/etc
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }




        /// <summary>
        /// This is the action we give to the trim-scan tool, to run on accept
        /// </summary>
        public static void CropScanFromSelection(DMeshSO so, MeshFaceSelection selection, object tool)
        {
            DMesh3 beforeMesh = new DMesh3(so.Mesh);
            DMesh3 mesh = so.Mesh;

            // [RMS] if we are using the two-point tool, then we can use the user input points to
            // try to figure out an up axis, by assuming the first point is on the base of the scan. Steps are:
            //   1) guess a midpoint. Currently centroid of upper-half of geodesic selection.
            //   2) construct up axis as (midpoint-basepoint). this axis to Y-up.
            Vector3f upAxisS = Vector3f.AxisY;
            TwoPointFaceSelectionTool ptool = tool as TwoPointFaceSelectionTool;
            if (ptool != null) {
                var cache = ptool.SelectionCache;
                Interval1d range = new Interval1d(cache.CurrentScalarThreshold / 2, cache.CurrentScalarThreshold);
                List<int> triangles = new List<int>(selection.Count);
                cache.FindTrianglesInScalarInterval(range, triangles);
                Vector3d c = MeshMeasurements.Centroid(triangles, mesh.GetTriCentroid);
                Vector3d cS = SceneTransforms.ObjectToSceneP(so, c);
                Vector3d basePosS = ptool.SourcePositionS.Origin;
                upAxisS = (Vector3f)(cS - basePosS).Normalized;
            }

            // crop scan and fill top hole
            List<int> borderTris = selection.FindBorderTris();
            MeshEditor editor = new MeshEditor(mesh);
            editor.RemoveTriangles((tid) => { return selection.IsSelected(tid) == false; }, true);
            if ( OGActions.FillHoleInScan ) {
                SmoothedHoleFill fill = new SmoothedHoleFill(mesh) {
                    TargetEdgeLengthMM = 2.5f,
                    SmoothAlpha = 0.5f,
                    BorderHintTris = borderTris,
                    OffsetDirection = SceneTransforms.SceneToObjectN(so, upAxisS),
                    OffsetDistance = (ptool != null) ? 25.0 : 0.0
                };
                fill.Apply();
            }

            so.NotifyMeshEdited();
            DMesh3 afterMesh = new DMesh3(so.Mesh);
            so.GetScene().History.PushChange(new ReplaceEntireMeshChange(so, beforeMesh, afterMesh), true);
            mesh = so.Mesh;

            // Now we auto-align the scan so it points upwards, and then
            // recenter pivot and shift to above ground plane
            if ( ptool != null ) {
                Vector3d basePosS = ptool.SourcePositionS.Origin;
                Quaternionf alignUp = Quaternionf.FromTo(upAxisS, Vector3f.AxisY);

                // rotate part so that axis points up
                Frame3f curF = so.GetLocalFrame(CoordSpace.SceneCoords);
                Frame3f newF = curF.Rotated(alignUp);
                TransformSOChange alignUpChange = new TransformSOChange(so, curF, newF, CoordSpace.SceneCoords);
                basePosS = newF.FromFrameP(curF.ToFrameP(basePosS));   // map to new frame
                so.GetScene().History.PushChange(alignUpChange, false);

                // recenter pivot at bbox center
                // [RMS] previously was using vertex centroid, but this is then affected by mesh density
                //   (maybe tri centroid? but bbox makes more sense...and below we assume box center)
                Vector3d centerL = mesh.CachedBounds.Center;
                Vector3d centerO = newF.FromFrameP(centerL);
                Frame3f newPivotO = new Frame3f(centerO);
                so.GetScene().History.PushChange(new RepositionPivotChangeOp(newPivotO,so), false);

                // position above ground plane
                AxisAlignedBox3d bounds = so.Mesh.CachedBounds;
                float h = (float)bounds.Height;
                Vector3f o = newPivotO.Origin;
                Vector3f translateO = new Vector3f(-o.x, h * 0.5f - o.y + BaseHeightAboveGroundPlaneMM, -o.z);
                //Vector3f translateO = new Vector3f(0, h * 0.5f - o.y + BaseHeightAboveGroundPlaneMM, 0);
                newPivotO.Translate(translateO);
                so.GetScene().History.PushChange(new TransformSOChange(so, newPivotO, CoordSpace.ObjectCoords), false);

                // save base point in frame of scan
                basePosS += translateO;
                Vector3d basePosL = SceneTransforms.SceneToObjectP(so, basePosS);
                OG.Scan.UserBasePoint = basePosL;
            }

            so.GetScene().History.PushInteractionCheckpoint();
        }




        /*
         * These are the actions we use for the trim tool workflow
         */
        public static bool CanTrimScan()
        {
            var M = OG.Model;
            return M.Context.ToolManager.ActiveRightTool == null;
        }
        public static void BeginTrimScanTool()
        {
            var M = OG.Model;

            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
            M.Context.ToolManager.SetActiveToolType(BodyModelTools.CreateSelectScanSubsetTool, ToolSide.Right);
            M.Scene.Select(OG.Scan.SO, true);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);
        }
        public static bool CanAcceptTrimScanTool()
        {
            var M = OG.Model;
            return (M.Context.ToolManager.ActiveRightTool != null) && M.Context.ToolManager.ActiveRightTool.CanApply;
        }
        public static void AcceptTrimScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.ActiveRightTool.Apply();
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }
        public static void CancelTrimScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }






        /*
         * These are the actions we use for the Align tool workflow
         */
        public static bool CanAlignScan()
        {
            var M = OG.Model;
            return M.Context.ToolManager.ActiveRightTool == null;
        }
        public static void BeginAlignScanTool()
        {
            var M = OG.Model;

            M.Context.ToolManager.DeactivateTool(ToolSide.Right);
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(TransformManager.NoGizmoType);
            M.Context.ToolManager.SetActiveToolType(BodyModelTools.AlignScanTool, ToolSide.Right);
            M.Scene.Select(OG.Scan.SO, true);
            M.Context.ToolManager.ActivateTool(ToolSide.Right);

            SocketAlignmentTool tool = M.Context.ToolManager.ActiveRightTool as SocketAlignmentTool;
            if ( OG.Scan.HasUserBasePoint ) {
                tool.Initialize_KnownBasePoint(OG.Scan.UserBasePoint, Vector3f.AxisY);
            } else {
                tool.Initialize_AutoFitBox();
            }

        }
        public static bool CanAcceptAlignScanTool()
        {
            var M = OG.Model;
            return (M.Context.ToolManager.ActiveRightTool != null) && M.Context.ToolManager.ActiveRightTool.CanApply;
        }
        public static void AcceptAlignScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.ActiveRightTool.Apply();
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }
        public static void CancelAlignScanTool()
        {
            var M = OG.Model;
            M.Context.ToolManager.DeactivateTools();
            M.Scene.ClearSelection();
            OG.Context.TransformManager.SetActiveGizmoType(AxisTransformGizmo.DefaultName);
        }











    }
}
