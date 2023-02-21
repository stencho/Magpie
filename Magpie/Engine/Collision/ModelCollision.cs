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

        GJK3DParallel gjkp = new GJK3DParallel();

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
                
                Vector3[] vdata = new Vector3[vb.VertexCount];
                vb.GetData<Vector3>(0, vdata, 0, vb.VertexCount, vb.VertexDeclaration.VertexStride);

                ushort[] idata = new ushort[ib.IndexCount];
                ib.GetData(idata);

                int v = 0;
                bool ib_working = idata[0] != 65539;

                for (int i = 0; i < ib.IndexCount; i+=3) {
                    //for (int v = 0; v < 3; v++) {
                    tris[v] = new Triangle(
                        vdata[idata[i]], 
                        vdata[idata[i + 1]], 
                        vdata[idata[i + 2]]);
                
                    v++;
                    //}
                }
            }


        }
    }
}
