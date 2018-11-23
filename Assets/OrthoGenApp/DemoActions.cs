using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;

namespace orthogen
{
    static class DemoActions
    {

        /// <summary>
        /// when the socket is updated, shift the ground plane to be directly below it
        /// </summary>
        public static void AddRepositionGroundPlaneOnSocketEdit()
        {
            OG.OnSocketUpdated += () => {
                // compute scene-space bbox of socket mesh
                Frame3f socketF = OG.Socket.Socket.GetLocalFrame(CoordSpace.ObjectCoords);
                AxisAlignedBox3d boundsS =
                    MeshMeasurements.Bounds(OG.Socket.Socket.Mesh, socketF.FromFrameP);

                // vertically translate bounds objects to be at same y 
                //  (assumes they are xz planes!!)
                Vector3d baseS = boundsS.Center - boundsS.Extents[1] * Vector3d.AxisY;
                Vector3d baseW = OG.Scene.ToWorldP(baseS);
                foreach (var go in OG.Scene.BoundsObjects) {
                    Vector3f pos = go.GetPosition();
                    pos.y = (float)baseW.y;
                    go.SetPosition(pos);
                }
            };
        }

    }
}
