using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.GJK;

namespace Magpie.Engine.Collision {
    public  class ModelCollision {
        public (int,int) mesh_meshpart_id = (-1,-1);
        Triangle[] tris;
        Vector3[] points;

        GJK3DParallel gjkp = new GJK3DParallel();

        BoundingBox bounds;

        public Octree octree;

        public BoundingBox get_bounds (Matrix world) {
            return CollisionHelper.BoundingBox_around_transformed_points(world, points);
        }

        public void draw(Matrix matrix) {
            Draw3D.cube(bounds, matrix, Color.Purple);
            octree.draw(matrix);

            foreach (Node n in octree.nodes) {
                foreach (int t in n.values) {
                    
                    //Draw3D.xyz_cross(Vector3.Transform(t.A, matrix), 1f, Color.Red);
                    //Draw3D.xyz_cross(Vector3.Transform(t.B, matrix), 1f, Color.Red);
                    //Draw3D.xyz_cross(Vector3.Transform(t.C, matrix), 1f, Color.Red);

                    Draw3D.lines(n.color,
                        Vector3.Transform(tris[t].A, matrix),
                        Vector3.Transform(tris[t].B, matrix),
                        Vector3.Transform(tris[t].C, matrix),
                        Vector3.Transform(tris[t].A, matrix));
                    //bounds = get_bounds(matrix);
                }

            }

        }
        public gjk_result[] gjk_multi_sample(Shape3D shape, Matrix world, Matrix collision_world, int samples) { 
            return gjk_multi_sample(shape, world, collision_world, samples, Vector3.Zero);
        }

        public gjk_result[] gjk_multi_sample(Shape3D shape, Matrix world, Matrix collision_world, int samples, Vector3 sweep) {

            BoundingBox bb;
            if (sweep != Vector3.Zero) {
                bb = CollisionHelper.BoundingBox_around_BoundingBoxes(
                    shape.find_bounding_box(world * Matrix.Invert(collision_world)),
                    shape.find_bounding_box(world * Matrix.CreateTranslation(sweep) * Matrix.Invert(collision_world)));
            } else {
                bb = shape.find_bounding_box(world * Matrix.Invert(collision_world));
            }


            var hits = octree.get_all_values(bb);
            bool anything = false;

            gjk_result[] results = new gjk_result[samples];
            for (int i = 0; i < results.Length; i++) {
                results[i].distance = float.MaxValue;
            }


            foreach (int i in hits) {
                Triangle tri = tris[i];
                gjk_result result_tmp = gjkp.gjk_intersects(shape, tri, world, collision_world);

                for (int s = 0; s < samples; s++) {

                    if ((result_tmp.distance < results[s].distance 
                        &&  Vector3.Dot(tri.normal, result_tmp.AB) > 0)                     
                    || results[s].distance == float.MaxValue) {

                        for (int sre = samples-1; sre > s; sre--) {
                            results[sre] = results[sre - 1];
                        }

                        results[s] = result_tmp;
                        anything = true;
                        break;
                    }

                }
            }


            return results;

        }

        public bool gjk(Shape3D shape, Matrix world, Matrix collision_world, out gjk_result result) {
            return gjk(shape, world, collision_world, out result, Vector3.Zero);
        }
        public bool gjk(Shape3D shape, Matrix world, Matrix collision_world, out gjk_result result, Vector3 sweep) {
            BoundingBox bb;
            if (sweep != Vector3.Zero) {
                bb = CollisionHelper.BoundingBox_around_BoundingBoxes(
                    shape.find_bounding_box(world * Matrix.Invert(collision_world)),
                    shape.find_bounding_box(world * Matrix.CreateTranslation(sweep) * Matrix.Invert(collision_world)));
            } else {
                bb = shape.find_bounding_box(world * Matrix.Invert(collision_world));
            }

            var hits = octree.get_all_values(bb);
            bool anything = false;

            gjk_result closest_result = new gjk_result() {
                distance = float.MaxValue
            };

            foreach (int i in hits) {
                var tri = tris[i];

                gjk_result result_tmp = gjkp.gjk_intersects(shape, tri, world, collision_world);

                if (result_tmp.distance < closest_result.distance || result_tmp.hit) {
                    
                      if (Vector3.Dot(tri.normal, result_tmp.AB) > 0) {
                          closest_result = result_tmp;
                      }
                    
                    anything = true;
                }
            }

            result = closest_result;

            return anything;
                
        }

        public ModelCollision(VertexBuffer vb, IndexBuffer ib, int meshid, int meshpartid) {
            mesh_meshpart_id = (meshid, meshpartid);

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
                octree = new Octree(bounds.Min, bounds.Max);
                //octree.subdivide_all(2);
            }


            int ti = 0;
            foreach(Triangle t in tris) {
                octree.add_value(ti, t.find_bounding_box(Matrix.Identity));
                ti++;
            }


        }
    }
}
