using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;
using gs;

namespace orthogen
{
    public class SpatialEditDeformOpSOBehavior : SpatialClickDragBehaviour
    {
        BaseSO sourceSO;
        DMeshSO legSO;
        IVectorDisplacementSourceOp op;

        public SpatialEditDeformOpSOBehavior(FContext context, BaseSO sourceSO, DMeshSO legSO, IVectorDisplacementSourceOp op) : base(context)
        {
            this.sourceSO = sourceSO;
            this.legSO = legSO;
            this.op = op;

            this.WantsCaptureF = wants_capture;
            base.BeginCaptureF = begin_capture;
            base.UpdateCaptureF = update_capture;
        }

        Frame3f lastPosS;

        Frame3f startSocketFrameS;
        Line3f startAxisS;
        Frame3f startPosS;

        double startParam0, startParam1;

        bool wants_capture(InputState input, CaptureSide eSide) {
            Ray3d ray = (eSide == CaptureSide.Left) ? input.vLeftSpatialWorldRay : input.vRightSpatialWorldRay;

            SORayHit hit;
            if (context.Scene.FindSORayIntersection((Ray3f)ray, out hit)) {
                if (hit.hitSO == legSO || hit.hitSO == sourceSO)
                    return true;
            }
            return false;
        }

        void begin_capture(InputState input, CaptureSide eSide)
        {
            Frame3f handFrameW = (eSide == CaptureSide.Left) ? input.LeftHandFrame : input.RightHandFrame;
            lastPosS = SceneTransforms.WorldToScene(context.Scene, handFrameW);
            startPosS = lastPosS;
            startSocketFrameS = legSO.GetLocalFrame(CoordSpace.SceneCoords);
            startAxisS = new Line3f(startSocketFrameS.Origin, startSocketFrameS.Y);


            if (op is EnclosedRegionOffsetOp) {
                EnclosedRegionOffsetOp deformOp = op as EnclosedRegionOffsetOp;
                startParam0 = deformOp.PushPullDistance;

            } else if (op is EnclosedRegionSmoothOp) {
                EnclosedRegionSmoothOp deformOp = op as EnclosedRegionSmoothOp;
                startParam0 = deformOp.OffsetDistance;
                startParam1 = deformOp.SmoothAlpha;

            } else if (op is PlaneBandExpansionOp) {
                PlaneBandExpansionOp deformOp = op as PlaneBandExpansionOp;
                startParam0 = deformOp.PushPullDistance;
                startParam1 = deformOp.BandDistance;
            }

        }

        void update_capture(InputState input, InputState lastInput, CaptureSide eSide)
        {
            Frame3f handFrameW = (eSide == CaptureSide.Left) ? input.LeftHandFrame : input.RightHandFrame;
            Frame3f newPosS = SceneTransforms.WorldToScene(context.Scene, handFrameW);

            Vector3f start = startSocketFrameS.ToFrameP(startPosS.Origin);
            Vector3f cur = startSocketFrameS.ToFrameP(newPosS.Origin);

            float dy = cur.y - start.y;
            start.y = cur.y = 0;
            float dx = cur.Length - start.Length;

            if (op is EnclosedRegionOffsetOp) {
                EnclosedRegionOffsetOp deformOp = op as EnclosedRegionOffsetOp;
                deformOp.PushPullDistance = startParam0 + 0.1*dx;

            } else if (op is EnclosedRegionSmoothOp) {
                EnclosedRegionSmoothOp deformOp = op as EnclosedRegionSmoothOp;
                deformOp.OffsetDistance = startParam0 + 0.1*dx;
                deformOp.SmoothAlpha = startParam1 + 0.1*dy;

            } else if (op is PlaneBandExpansionOp) {
                PlaneBandExpansionOp deformOp = op as PlaneBandExpansionOp;
                deformOp.PushPullDistance = startParam0 + 0.1*dx;
                deformOp.BandDistance = startParam1 + 0.25*dy;
            }



            lastPosS = newPosS;

        }



    }
}
