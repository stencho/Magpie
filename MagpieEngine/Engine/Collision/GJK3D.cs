using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision {
    public class GJK3D {

        public static Vector3 highest_dot(Vector3 direction, out int index, params Vector3[] verts) {
            float dot = float.MinValue; index = 0;
            Vector3 v = Vector3.Zero;

            for (int i = 0; i < verts.Length; i++) {
                float d = Vector3.Dot(verts[i], direction);

                if (d > dot) {
                    index = i;
                    dot = d;
                    v = verts[i];
                }
            }

            return v;
        }

        private const float epsilon = 1e-6f;
        private static bool compare(float x, float y) {
            return Math.Abs(x - y) <= epsilon * Math.Max(1.0f, Math.Max(Math.Abs(x), Math.Abs(y)));
        }
        public static Vector3 barycentric(Vector3 p, Vector3 A, Vector3 B, Vector3 C) {
            Vector3 v0 = B - A;
            Vector3 v1 = C - A;
            Vector3 v2 = p - A;

            float f0 = Vector3.Dot(v0, v0);
            float f1 = Vector3.Dot(v0, v1);
            float f2 = Vector3.Dot(v1, v1);
            float f3 = Vector3.Dot(v2, v0);
            float f4 = Vector3.Dot(v2, v1);

            float denom = f0 * f2 - f1 * f1;
            if (compare(denom, 0.0f)) {
                return Vector3.Zero;
            }

            Vector3 res;
            res.Y = (f2 * f3 - f1 * f4) / denom;
            res.Z = (f0 * f4 - f1 * f3) / denom;
            res.X = 1.0f - res.Y - res.Z;

            return res;
        }


        public static Vector3 point_of_minimum_norm(Vector3 a, Vector3 b, Vector3 p) {
            var ab = b - a;
            var t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        public interface shape3D {
            Vector3 find_point_in_direction(Vector3 direction);
            void draw();
        }


        public struct gjk_result {
            public bool hit;
            public int iterations;

            public Vector3 direction;

            public int num_points;
            public Vector3[] simplex;

            public float distance;
            public float penetration;

            public shape3D shape_A, shape_B;

            public Vector3 closest_point_A;
            public Vector3 closest_point_B;
            public Vector3 closest_simplex_point_to_origin;

            public Vector3 A => simplex[0];
            public Vector3 B => simplex[1];
            public Vector3 C => simplex[2];
            public Vector3 D => simplex[3];
        }


        public static bool vector3_contains_nan(Vector3 a) { return (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z)); }
        public static bool same_direction(Vector3 direction, Vector3 origin_dir) {
            if (vector3_contains_nan(direction) || vector3_contains_nan(origin_dir)) return false;
            var vd = Vector3.Dot(direction, origin_dir);            
            return (vd > 0f);
        }

        public static string exit_path = "";
        public static int max_iterations = 50;

        public static gjk_result intersects(shape3D shape_A, shape3D shape_B) {
            gjk_result result = new gjk_result();


            result.shape_A = shape_A;
            result.shape_B = shape_B;


            result.simplex = new Vector3[4];
            result.num_points = 2;

            result.direction = Vector3.Forward;

            result.simplex[1] = shape_A.find_point_in_direction(result.direction) - shape_B.find_point_in_direction(-result.direction);
            result.direction = -result.simplex[1];

            result.simplex[0] = shape_A.find_point_in_direction(result.direction) - shape_B.find_point_in_direction(-result.direction);


            while (result.iterations < max_iterations && result.hit == false) {
                do_simplex(ref result);
                result.iterations++;
            }

            //find closest points on solved simplex
            switch (result.num_points) {
                case 1:
                    result.closest_simplex_point_to_origin = result.A;
                    break;
                case 2:
                    result.closest_simplex_point_to_origin = point_of_minimum_norm(result.A, result.B, Vector3.Zero);
                    break;
                case 3:
                    result.closest_simplex_point_to_origin = CollisionHelper.closest_point_on_triangle(result.A, result.B, result.C, Vector3.Zero);
                    break;
                case 4:
                    Vector3 f = CollisionHelper.closest_point_on_triangle(result.A, result.B, result.C, Vector3.Zero);

                    result.closest_simplex_point_to_origin = f;

                    f = CollisionHelper.closest_point_on_triangle(result.A, result.C, result.D, Vector3.Zero);
                    if (Vector3.Distance(f, Vector3.Zero) < Vector3.Distance(result.closest_simplex_point_to_origin, Vector3.Zero))
                        result.closest_simplex_point_to_origin = f;

                    f = CollisionHelper.closest_point_on_triangle(result.A, result.B, result.D, Vector3.Zero);
                    if (Vector3.Distance(f, Vector3.Zero) < Vector3.Distance(result.closest_simplex_point_to_origin, Vector3.Zero))
                        result.closest_simplex_point_to_origin = f;
                    break;

            }

            result.closest_point_A = shape_A.find_point_in_direction(-result.closest_simplex_point_to_origin);
            result.closest_point_B = result.closest_point_A - result.closest_simplex_point_to_origin;


            return result;
        }

        public static Vector3 AO = Vector3.Zero;
        public static Vector3 AB = Vector3.Zero;
        public static Vector3 AC = Vector3.Zero;
        public static Vector3 AD = Vector3.Zero;

        public static Vector3 ABC = Vector3.Zero;
        public static Vector3 ADB = Vector3.Zero;
        public static Vector3 ACD = Vector3.Zero;
        public static Vector3 BDC = Vector3.Zero;

        static void do_simplex(ref gjk_result result) {
            exit_path = "";
            switch (result.num_points) {
                //single point simplex
                case 1:
                    //move A to B and get a new A, then move onto line simplex
                    result.simplex[1] = result.simplex[0];
                    result.simplex[0] = result.shape_A.find_point_in_direction(result.direction) - result.shape_B.find_point_in_direction(-result.direction);
                    
                    result.num_points = 2;
                    break;


                //line simplex
                case 2:
                    AO = -result.A;
                    AB = result.B - result.A;


                    //we have two points, so we use the point of minimum norm from the line that forms to get current distance from origin
                    result.distance = Vector3.Distance(point_of_minimum_norm(result.A, result.B, Vector3.Zero), Vector3.Zero);
                    
                    //exit early if origin is directly on the line simplex
                    if (result.distance < epsilon) {
                        result.hit = true;
                        result.iterations = max_iterations;
                        result.distance = 0f;
                        break;
                    }
                    
                    //if B was on the other side of A from O, AB and O will be in the same direction
                    if (same_direction(AB, AO)) {
                        result.direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);

                        result.simplex[2] = result.simplex[1];
                        result.simplex[1] = result.simplex[0];

                        result.simplex[0] = result.shape_A.find_point_in_direction(result.direction) - result.shape_B.find_point_in_direction(-result.direction);

                        result.num_points = 3;

                        //otherwise, start at square one- set direction to face origin and reset
                    } else {
                        result.direction = AO;
                        result.num_points = 1;
                        result.simplex[1] = Vector3.Zero;
                    }

                    break;

                //triangle simplex
                case 3:
                    AO = -result.A;
                    AB = result.B - result.A;
                    AC = result.C - result.A;
                    ABC = Vector3.Cross(AB, AC);

                    //result.closest_simplex_point_to_origin = CollisionHelper.closest_point_on_triangle(result.A, result.B, result.C, Vector3.Zero);

                    //the majority of the below is testing each of the sides of the tri to see if origin is the same direction, 
                    //but the most important ones are near the bottom
                    if (same_direction(Vector3.Cross(ABC, AC), AO)) {
                        if (same_direction(AC, AO)) {
                            result.simplex[1] = result.simplex[2];
                            result.simplex[2] = Vector3.Zero;

                            result.direction = Vector3.Cross(Vector3.Cross(AC, AO), AC);
                            result.num_points = 2;

                        } else {
                            if (same_direction(AB, AO)) {
                                result.direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);

                                result.simplex[2] = Vector3.Zero;
                                result.num_points = 2;
                            } else {
                                result.direction = AO;

                                result.simplex[1] = Vector3.Zero;
                                result.simplex[2] = Vector3.Zero;

                                result.num_points = 1;
                            }
                        }
                    } else {
                        if (same_direction(Vector3.Cross(AB, ABC), AO)) {
                            if (same_direction(AB, AO)) {
                                result.direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);

                                result.simplex[2] = Vector3.Zero;
                                result.num_points = 2;
                            } else {
                                result.direction = AO;

                                result.simplex[1] = Vector3.Zero;
                                result.simplex[2] = Vector3.Zero;

                                result.num_points = 1;
                            }

                        //here is the good stuff, we've set up a triangle where ABC or -ABC faces origin, so we can move to trying to form a polyhedron around it
                        } else {
                            //ABC faces origin
                            if (same_direction(ABC, AO)) {

                                //something has gone wrong, exit early
                                if (ABC == Vector3.Zero) {
                                    result.iterations = max_iterations;
                                    //result.num_points =1;
                                    break;
                                }

                                result.direction = ABC;

                                result.distance = Vector3.Distance(CollisionHelper.closest_point_on_triangle(result.A, result.B, result.C, Vector3.Zero), Vector3.Zero);


                                result.simplex[3] = result.simplex[2];
                                result.simplex[2] = result.simplex[1];
                                result.simplex[1] = result.simplex[0];
                                                               
                                result.simplex[0] = result.shape_A.find_point_in_direction(result.direction) - result.shape_B.find_point_in_direction(-result.direction);
                                
                                result.num_points = 4;
                            //-ABC faces origin
                            } else {
                                //something has gone wrong, exit early
                                if (ABC == Vector3.Zero) {
                                    result.iterations = max_iterations;
                                    //result.num_points = 2;
                                    break;
                                }

                                result.direction = -ABC;

                                result.distance = Vector3.Distance(CollisionHelper.closest_point_on_triangle(result.A, result.B, result.C, Vector3.Zero), Vector3.Zero);


                                var t = result.B;

                                result.simplex[1] = result.simplex[2];
                                result.simplex[2] = t;

                                result.simplex[3] = result.simplex[2];
                                result.simplex[2] = result.simplex[1];
                                result.simplex[1] = result.simplex[0];

                                result.simplex[0] = result.shape_A.find_point_in_direction(result.direction) - result.shape_B.find_point_in_direction(-result.direction);
                                
                                result.num_points = 4;
                            }

                        }
                    }

                    break;


                case 4:
                    AO = -result.A;
                    AB = result.B - result.A;
                    AC = result.C - result.A;
                    AD = result.D - result.A;

                    Vector3 BC = result.C - result.B;
                    Vector3 BD = result.D - result.B;

                    ABC = Vector3.Cross(AB, AC);
                    ADB = Vector3.Cross(AD, AB);
                    ACD = Vector3.Cross(AC, AD);
                    BDC = Vector3.Cross(BD, BC);


                    //the three sides below can face origin, if they do, change up the simplex and attempt to find a new 4th point to face origin
                    if (same_direction(ACD, AO)) {
                        result.direction = ACD;
                        
                        result.simplex[1] = result.simplex[2];
                        result.simplex[2] = result.simplex[3];

                        result.num_points = 3;
                        exit_path = "ACD";

                    } else if (same_direction(ABC, AO)) {
                        result.direction = ABC;

                        result.simplex[3] = Vector3.Zero;
                        result.num_points = 3;
                        exit_path = "ABC";
                    } else if (same_direction(ADB, AO)) {
                        result.direction = ADB;


                        result.simplex[2] = result.simplex[1];
                        result.simplex[1] = result.simplex[3];

                        result.num_points = 3;
                        exit_path = "ADB";
                    }

                    //if all of the sides above face away from origin, that's a hit

                    if (same_direction(-ABC, AO) &&
                        same_direction(-ADB, AO) &&
                        same_direction(-ACD, AO)) {

                        result.distance = 0f;
                            
                        result.hit = true;
                        exit_path = "HIT BABY";
                    }
                    
        
                    break;
            }
        }
    }
}
