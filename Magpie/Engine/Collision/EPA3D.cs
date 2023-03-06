using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision {


    public class polytope {       

        public struct index_tri {
            public int A;
            public int B; 
            public int C;
            
            public index_tri(int a, int b, int c, bool remove) {
                A = a;
                B = b;
                C = c;
            }
        }
        public Vector3 support_point = Vector3.Zero;
        public Vector3 closest_facet = Vector3.Zero;
        public List<Vector3> points = new List<Vector3>();
        public List<index_tri> triangle_indices = new List<index_tri>();
        public List<(int A, int B)> edge_indices = new List<(int A, int B)>();

        Vector3 tri_normal(index_tri tri) {
            return Vector3.Cross(points[tri.B] - points[tri.A], points[tri.C] - points[tri.A]);
        }
        Vector3 tri_center(index_tri tri) {
            return (points[tri.A] + points[tri.B] + points[tri.C] ) / 3;
        }

        public polytope copy() {
            var p = new polytope();
            p.points = points;
            p.triangle_indices = triangle_indices;
            p.edge_indices = edge_indices;
            p.support_point = support_point;
            return p;
        }

        public polytope() { }
        public polytope(gjk_simplex simplex) {
            if (simplex.stage != simplex_stage.tetrahedron)
                throw new Exception("Simplex was too small to apply EPA");

            triangle_indices = new List<index_tri>();
            edge_indices = new List<(int A, int B)>();
            points = new List<Vector3>();

            add_triangle(simplex.A, simplex.B, simplex.C);
            add_triangle(simplex.A, simplex.C, simplex.D);
            add_triangle(simplex.A, simplex.D, simplex.B);
            add_triangle(simplex.C, simplex.B, simplex.D);
        }

        int try_add(Vector3 P) {
            for (int i = 0; i < points.Count; i++) {
                if (P == points[i]) return i;
            }

            points.Add(P);
            return points.Count - 1;
        }

        void add_triangle(Vector3 A, Vector3 B, Vector3 C) {
            var pa = try_add(A);
            var pb = try_add(B);
            var pc = try_add(C);

            triangle_indices.Add(new index_tri(pa, pb, pc, false));
        }

        void add_edge(int A, int B) {
            var c = edge_indices.Count;
            for (int i = 0; i < c; i++) {
                if (edge_indices[i].A == B && edge_indices[i].B == A) {
                    edge_indices.RemoveAt(i);
                    return;
                }
            }

            edge_indices.Add((A, B));
        }

        public void expand(Vector3 P, ref gjk_simplex simplex, ref collision_result result) {
            support_point = P;


            for (int i = 0; i < triangle_indices.Count; i++) {
                var index = i;

                if (
                    Math3D.same_dir(
                        tri_normal(triangle_indices[index]),
                        P - tri_center(triangle_indices[index]) )) {
                    add_edge(triangle_indices[index].A, triangle_indices[index].C);
                    add_edge(triangle_indices[index].C, triangle_indices[index].B);
                    add_edge(triangle_indices[index].B, triangle_indices[index].A);

                    triangle_indices.RemoveAt(index);
                    i--;
                }

            }

            points.Add(P);
            var pi = points.Count-1;

            for (int i = 0; i < edge_indices.Count; i++) {
                triangle_indices.Add(new index_tri(pi,edge_indices[i].B,edge_indices[i].A,false));

            }

            edge_indices.Clear();
        }

        public void draw() {
            if (triangle_indices == null) return;
            foreach (var tri in triangle_indices) {
                //Draw3D.fill_tri(Matrix.Identity, points[tri.A], points[tri.B], points[tri.C], Color.Red);
            }
            foreach (var tri in triangle_indices) {                
                Draw3D.sprite_line(points[tri.A], points[tri.B], 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.A], points[tri.C], 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.C], points[tri.B], 0.02f, Color.Orange);
                Draw3D.sprite_line(tri_center(tri), tri_center(tri) + tri_normal(tri), 0.02f, 
                    
                    Math3D.same_dir(tri_normal(tri), support_point - tri_center(tri)) ? Color.Orange : Color.Green);
            }
            foreach (var edge in edge_indices) {
                Draw3D.sprite_line(points[edge.A], points[edge.B], 0.02f, Color.Red);
            }

            foreach (var tri in triangle_indices) {
                Draw3D.sprite_line(points[tri.A], points[tri.B], 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.A], points[tri.C], 0.02f, Color.Orange);
                Draw3D.sprite_line(points[tri.C], points[tri.B], 0.02f, Color.Orange);
            }
            Draw3D.xyz_cross(closest_facet, 2f, Color.Red);
            Draw3D.xyz_cross(support_point, 2f, Color.MonoGameOrange);

        }
    }
    public static class EPA3D {

        //find closest facet /
        //find new support in direction /
        //tag faces that have the same normal dir as -P for removal /
        //add their edges to the edge list, CCW /
        //if an opposite edge already exists, remove it and don't add the new edge /
        //remove tris /
        //add new triangles using the support point + the edges in the edge list /
        //mind the wind /
        //clear the edge list /
        //repeat until closest facet is already in list /

        public static polytope expand_polytope(Shape3D shape_A, Shape3D shape_B, ref gjk_simplex simplex, ref collision_result result) {
            polytope poly = new polytope(simplex);

            Vector3 last_closest = Vector3.Zero;

            float closest = float.MaxValue;
            Vector3 closest_facet_point = Vector3.Zero;
            int closest_facet_index = -1;

            var iterations = 0;
            while (iterations < 10) {
                closest = float.MaxValue;
                closest_facet_point = Vector3.Zero;
                closest_facet_index = -1;

                for (int i = 0; i < poly.triangle_indices.Count; i++) {
                    var v = CollisionHelper.triangle_closest_point_alternative(
                        poly.points[poly.triangle_indices[i].A],
                        poly.points[poly.triangle_indices[i].B],
                        poly.points[poly.triangle_indices[i].C],
                        Vector3.Zero
                        );
                    var d = v.Length();
                    if (d < closest) {
                        closest = d;
                        closest_facet_point = v;
                        closest_facet_index = i;
                    }
                }

                poly.closest_facet = closest_facet_point;

                if (iterations > 0) {
                    if (Vector3.Distance(last_closest, closest_facet_point) < Math3D.big_epsilon) {

                        result.penetration = Vector3.Distance(closest_facet_point, Vector3.Zero);
                        result.penetration_normal = Vector3.Normalize(closest_facet_point);

                        break;
                    }
                }

                last_closest = closest_facet_point;
                var A = Vector3.Transform(
                    shape_A.support(
                        Vector3.Transform(
                            closest_facet_point,
                            Matrix.Invert(simplex.A_transform_direction)),
                        Vector3.Zero),
                    simplex.A_transform);

                var B = Vector3.Transform(
                    shape_B.support(
                        Vector3.Transform(
                            -closest_facet_point,
                            Matrix.Invert(simplex.B_transform_direction)),
                        Vector3.Zero),
                    simplex.B_transform);

                var support = A - B;

                poly.expand(support, ref simplex, ref result);

                
                iterations++;
            }

            return poly;
        }
    }
}
