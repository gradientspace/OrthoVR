using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace orthogen
{
    public static class RMSTests
    {

        public static void TestIsoCurve()
        {
            var meshSO = OG.Scan.SO;
            DMesh3 mesh = new DMesh3(meshSO.Mesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh, true);
            AxisAlignedBox3d bounds = mesh.CachedBounds;

            Frame3f plane = new Frame3f(bounds.Center);

            Func<Vector3d, double> planeSignedDistanceF = (v) => {
                return (v - plane.Origin).Dot(plane.Y);
            };

            Func<Vector3d, double> sphereDistF = (v) => {
                double d = v.Distance(plane.Origin);
                return d - 50.0;
            };

            MeshIsoCurves iso = new MeshIsoCurves(mesh, sphereDistF);
            iso.Compute();

            DGraph3Util.Curves curves = DGraph3Util.ExtractCurves(iso.Graph);

            foreach (DCurve3 c in curves.Loops) {
                List<Vector3d> verts = new List<Vector3d>(c.Vertices);
                for ( int i = 0; i < verts.Count; ++i )
                    verts[i] = verts[i] + 0.5 * mesh.GetTriNormal(spatial.FindNearestTriangle(verts[i]));
                DebugUtil.EmitDebugCurve("curve", verts.ToArray(), true, 1, Colorf.Red, Colorf.Red, meshSO.RootGameObject, false);
            }

            foreach (DCurve3 c in curves.Paths) {
                List<Vector3d> verts = new List<Vector3d>(c.Vertices);
                for (int i = 0; i < verts.Count; ++i)
                    verts[i] = verts[i] + 0.5 * mesh.GetTriNormal(spatial.FindNearestTriangle(verts[i]));
                DebugUtil.EmitDebugCurve("curve", verts.ToArray(), false, 1, Colorf.Blue, Colorf.Blue, meshSO.RootGameObject, false);
            }

            //foreach ( Segment3d seg in iso.Graph.Segments()) {
            //    Vector3d a = seg.P0 + 1.0 * mesh.GetTriNormal(spatial.FindNearestTriangle(seg.P0));
            //    Vector3d b = seg.P1 + 1.0 * mesh.GetTriNormal(spatial.FindNearestTriangle(seg.P1));

            //    DebugUtil.EmitDebugLine("seg", a, b, 1.0f, Colorf.Red, meshSO.RootGameObject, false);
            //}


        }

    }
}
