using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.GJK;

namespace Magpie.Engine.Collision {
    public  class ModelCollision {
        Triangle[] tris;
        Vector3[] points;

        GJK3DParallel gjkp = new GJK3DParallel();

        BoundingBox bounds;

        public BoundingBox get_bounds (Matrix world) {
            return CollisionHelper.BoundingBox_around_transformed_points(world, points);
        }

        public void draw(Matrix matrix) {
            foreach(Triangle t in tris) {
                //Draw3D.xyz_cross(Vector3.Transform(t.A, matrix), 1f, Color.Red);
                //Draw3D.xyz_cross(Vector3.Transform(t.B, matrix), 1f, Color.Red);
                //Draw3D.xyz_cross(Vector3.Transform(t.C, matrix), 1f, Color.Red);

                Draw3D.lines(Color.MonoGameOrange,
                    Vector3.Transform(t.A,matrix),
                    Vector3.Transform(t.B,matrix),
                    Vector3.Transform(t.C,matrix),
                    Vector3.Transform(t.A,matrix));
                //bounds = get_bounds(matrix);
                Draw3D.cube(bounds, Color.Purple);
            }
        }

        public gjk_result gjk(Shape3D shape, Matrix world, Matrix collision_world) {
            gjk_result? result = new gjk_result?();

            foreach(Triangle tri in tris) {
                gjk_result result_tmp;

                result_tmp = gjkp.gjk_intersects(shape, tri, world, collision_world);
                if (result_tmp.hit) { return result_tmp; }
                if (!result.HasValue || result_tmp.distance < result.Value.distance || result_tmp.hit) {
                    result = result_tmp;
                }
            }

            return result.Value;
        }

        public ModelCollision(VertexBuffer vb, IndexBuffer ib) {
            tris = new Triangle[ib.IndexCount/3];

            lock (vb) {
                points = new Vector3[vb.VertexCount];
                
                
                vb.GetData<Vector3>(0, points, 0, vb.VertexCount, vb.VertexDeclaration.VertexStride);

                ushort[] idata = new ushort[ib.IndexCount];
                ib.GetData(idata);

                int v = 0;

                for (int i = 0; i < ib.IndexCount; i+=3) {
                    tris[v] = new Triangle(
                        points[idata[i]], 
                        points[idata[i + 1]],
                        points[idata[i + 2]]);
                
                    v++;
                }

                bounds = get_bounds(Matrix.Identity);
            }


        }
    }
}
