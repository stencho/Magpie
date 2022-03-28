using Magpie.Engine.Collision.Support3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision {
    public static class ExtensionMethods {
        public static void forEach(this Vector3 vec, Action<float> action) {
            action(vec.X);
            action(vec.Y);
            action(vec.Z);
        }
        public static XYPair ToXYPair(this Vector2 v) { return new XYPair(v); }
    }

    public static class V3 {
        public static Vector2 XY(this Vector3 vec) => new Vector2(vec.X, vec.Y);        
        public static Vector2 XZ(this Vector3 vec) => new Vector2(vec.X, vec.Z);        
        public static Vector2 YZ(this Vector3 vec) => new Vector2(vec.Y, vec.Z); 
        public static Vector2 YX(this Vector3 vec) => new Vector2(vec.Y, vec.X);        
        public static Vector2 ZX(this Vector3 vec) => new Vector2(vec.Z, vec.X);        
        public static Vector2 ZY(this Vector3 vec) => new Vector2(vec.Z, vec.Y);        
    }

    public static class CollisionHelper {
        public const float epsilon = 1e-6f;
        public static BasicEffect e_basic;

        public static bool same_direction(Vector3 direction, Vector3 origin_dir) {
            var vd = Vector3.Dot(direction, origin_dir);
            return (vd >= 0f);
        }
        private static bool close_enough(Vector3 A, Vector3 B) { return Vector3.Distance(A, B) <= epsilon; }

        public static bool point_within_square(Vector2 min, Vector2 max, Vector2 point) {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y) return true;
            return false;
        }
        public static bool perpendicular(Vector3 direction, Vector3 direction2) {
            var vd = Vector3.Dot(direction, direction2);
            return (vd > -.250f && vd < .250f);
        }
        private static float index_to_axis(Vector3 input, int index) {
            if (index == 0) return input.X;
            if (index == 1) return input.Y;
            if (index == 2) return input.Z;
            return 0f;
        }
        private static Vector3 index_to_direction(Matrix input, int index) {
            if (index == 0) return input.Right;
            if (index == 1) return input.Up;
            if (index == 2) return input.Forward;
            return Vector3.Zero;
        }
        
        public static Vector3 closest_point_on_line(Vector3 a, Vector3 b, Vector3 point) {
            var ab = b - a;
            var t = Vector3.Dot(point - a, ab) / Vector3.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        public static Vector2 closest_point_on_line(Vector2 a, Vector2 b, Vector2 point) {
            var ab = b - a;
            var t = Vector2.Dot(point - a, ab) / Vector2.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        public static Vector3 closest_point_on_triangle(Vector3 A, Vector3 B, Vector3 C, Vector3 point) {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 ap = point - A;

            float d1 = Vector3.Dot(ab, ap);
            float d2 = Vector3.Dot(ac, ap);

            if (d1 <= 0f && d2 <= 0f) return A;

            Vector3 bp = point - B;

            float d3 = Vector3.Dot(ab, bp);
            float d4 = Vector3.Dot(ac, bp);

            if (d3 >= 0f && d4 <= 0f) return B;

            float vc = d1 * d4 - d3 * d2;
            float v, w;
            if (vc <= 0f && d1 >= 0f && d3 <= 0f) {
                v = d1 / (d1 - d3);
                return A + v * ab;
            }

            Vector3 cp = point - C;
            float d5 = Vector3.Dot(ab, cp);
            float d6 = Vector3.Dot(ac, cp);

            if (d6 >= 0f && d5 <= d6) return C;

            float vb = d5 * d2 - d1 * d6;
            if (vb <= 0f && d2 >= 0f && d6 <= 0f) {
                w = d2 / (d2 - d6);
                return A + w * ac;
            }

            float va = d3 * d6 - d5 * d4;
            if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) {
                w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
                return B + w * (C - B);
            }

            float denom = 1.0f / (va + vb + vc);
            v = vb * denom;
            w = vc * denom;

            return A + ab * v + ac * w;
        }

        public static Vector3 closest_point_on_triangle_slow(Vector3 A, Vector3 B, Vector3 C, Vector3 point) {
            Vector3 ab = B - A;
            Vector3 ac = C - A;
            Vector3 bc = C - B;

            float unom = Vector3.Dot(point - B, bc);
            float sdnom = Vector3.Dot(point - B, A - B);
            float tdnom = Vector3.Dot(point - C, A - C);
            float udnom = Vector3.Dot(point - C, B - C);

            if (sdnom <= 0f && unom <= 0f) return B;
            if (tdnom <= 0f && udnom <= 0f) return C;

            float snom = Vector3.Dot(point - A, ab);
            float tnom = Vector3.Dot(point - A, ac);

            Vector3 n = Vector3.Cross(ab, ac);
            float vc = Vector3.Dot(n, Vector3.Cross(A - point, B - point));
            if (vc <= 0f && snom >= 0f && sdnom >= 0f)
                return A + snom / (snom + sdnom) * ab;

            float va = Vector3.Dot(n, Vector3.Cross(B - point, C - point));

            if (va <= 0f && unom >= 0f && udnom >= 0f)
                return B + unom / (unom + udnom) * bc;

            float vb = Vector3.Dot(n, Vector3.Cross(C - point, A - point));

            if (vb <= 0f && tnom >= 0f && tdnom >= 0f)
                return A + tnom / (tnom + tdnom) * ac;

            float u = va / (va + vb + vc);
            float v = vb / (va + vb + vc);
            float w = 1.0f - u - v; // = vc / (va + vb + vc)

            return u * A + v * B + w * C;
        }

        public static Vector3 closest_point_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            Vector3 ABD_closest = CollisionHelper.closest_point_on_triangle(A, B, D, point);
            Vector3 BCD_closest = CollisionHelper.closest_point_on_triangle(B, C, D, point);

            if (Vector3.Distance(point, ABD_closest) < Vector3.Distance(point, BCD_closest)) {
                return ABD_closest;
            } else return BCD_closest;
        }


        public static Vector3 closest_corner_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            int o = -1;
            float d = float.MaxValue;
            float cd = Vector3.Distance(A, point);
            if (cd < d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd < d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd < d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd < d) { d = cd; o = 4; }
            cd = Vector3.Distance(A, point);
            if (cd < d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd < d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd < d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd < d) { d = cd; o = 4; }

            switch (o) {
                case 1: return A;
                case 2: return B;
                case 3: return C;
                case 4: return D;
                default: return Vector3.Zero;
            }
        }

        public static Vector3 farthest_corner_on_quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            int o = -1;
            float d = 0f;
            float cd = Vector3.Distance(A, point);
            if (cd > d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd > d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd > d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd > d) { d = cd; o = 4; }
            cd = Vector3.Distance(A, point);
            if (cd > d) { d = cd; o = 1; }
            cd = Vector3.Distance(B, point);
            if (cd > d) { d = cd; o = 2; }
            cd = Vector3.Distance(C, point);
            if (cd > d) { d = cd; o = 3; }
            cd = Vector3.Distance(D, point);
            if (cd > d) { d = cd; o = 4; }

            switch (o) {
                case 1: return A;
                case 2: return B;
                case 3: return C;
                case 4: return D;
                default: return Vector3.Zero;
            }
        }


        public static Vector3 closest_point_on_OBB(Vector3 point, Vector3 obb_origin, Matrix obb_orientation, Vector3 obb_half_scale) {
            Vector3 d = point - obb_origin;
            Vector3 outp = obb_origin;


            float dist = Vector3.Dot(d, obb_orientation.Right);
            if (dist > obb_half_scale.X) dist = obb_half_scale.X;
            if (dist < -obb_half_scale.X) dist = -obb_half_scale.X;

            outp += obb_orientation.Right * dist;

            dist = Vector3.Dot(d, obb_orientation.Up);
            if (dist > obb_half_scale.Y) dist = obb_half_scale.Y;
            if (dist < -obb_half_scale.Y) dist = -obb_half_scale.Y;

            outp += obb_orientation.Up * dist;


            dist = Vector3.Dot(d, obb_orientation.Forward);
            if (dist > obb_half_scale.Z) dist = obb_half_scale.Z;
            if (dist < -obb_half_scale.Z) dist = -obb_half_scale.Z;

            outp += obb_orientation.Forward * dist;

            return outp;
        }

        public static float closest_points_on_lines(Vector3 AA, Vector3 AB, Vector3 BA, Vector3 BB, out float s, out float t, out Vector3 P1, out Vector3 P2) {
            Vector3 d1 = AB - AA;
            Vector3 d2 = BB - BA;
            Vector3 r = AA - BA;

            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);

            if (a <= epsilon && e <= epsilon) {
                s = t = 0.0f;
                P1 = AA;
                P2 = BA;
                return Vector3.Dot(P1, P2);
            }

            if (a <= epsilon) {
                s = 0.0f;
                t = f / e;
                t = MathHelper.Clamp(t, 0.0f, 1.0f);
            } else {
                float c = Vector3.Dot(d1, r);
                if (e <= epsilon) {
                    t = 0.0f;
                    s = MathHelper.Clamp(-c / a, 0f, 1f);
                } else {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;

                    if (denom != 0f) {
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0f, 1f);
                    } else s = 0f;

                    t = (b * s + f) / e;

                    if (t < 0f) {
                        t = 0f;
                        s = MathHelper.Clamp(-c / a, 0f, 1f);
                    } else if (t > 1f) {
                        t = 1f;
                        s = MathHelper.Clamp((b - c) / a, 0f, 1f);
                    }
                }
            }

            P1 = AA + d1 * s;
            P2 = BA + d2 * t;

            return Vector3.Dot(P1 - P2, P1 - P2);
        }

        public static bool point_inside_plane_facing(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 P) { return (Vector3.Dot(P - A, Vector3.Cross(B - A, C - A)) * Vector3.Dot(D - A, Vector3.Cross(B - A, C - A))) > 0f; }

        public static bool point_inside_tetrahedron(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 point) {
            var t = point_inside_plane_facing(A, B, C, D, Vector3.Zero);
            var t2 = point_inside_plane_facing(A, B, D, C, Vector3.Zero);
            var t3 = point_inside_plane_facing(B, D, C, A, Vector3.Zero);
            var t4 = point_inside_plane_facing(A, C, D, B, Vector3.Zero);
            return (t && t2 && t3 && t4);
        }

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
        
        public static Vector3 point_of_minimum_norm(Vector3 a, Vector3 b, Vector3 p) {
            var ab = b - a;
            var t = Vector3.Dot(p - a, ab) / Vector3.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        public static AABB AABB_around_OBB(OBB obb) {
            float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
            float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

            if (obb.A.X > Xmax) Xmax = obb.A.X; if (obb.A.X < Xmin) Xmin = obb.A.X;
            if (obb.A.Y > Ymax) Ymax = obb.A.Y; if (obb.A.Y < Ymin) Ymin = obb.A.Y;
            if (obb.A.Z > Zmax) Zmax = obb.A.Z; if (obb.A.Z < Zmin) Zmin = obb.A.Z;

            if (obb.B.X > Xmax) Xmax = obb.B.X; if (obb.B.X < Xmin) Xmin = obb.B.X;
            if (obb.B.Y > Ymax) Ymax = obb.B.Y; if (obb.B.Y < Ymin) Ymin = obb.B.Y;
            if (obb.B.Z > Zmax) Zmax = obb.B.Z; if (obb.B.Z < Zmin) Zmin = obb.B.Z;

            if (obb.C.X > Xmax) Xmax = obb.C.X; if (obb.C.X < Xmin) Xmin = obb.C.X;
            if (obb.C.Y > Ymax) Ymax = obb.C.Y; if (obb.C.Y < Ymin) Ymin = obb.C.Y;
            if (obb.C.Z > Zmax) Zmax = obb.C.Z; if (obb.C.Z < Zmin) Zmin = obb.C.Z;

            if (obb.D.X > Xmax) Xmax = obb.D.X; if (obb.D.X < Xmin) Xmin = obb.D.X;
            if (obb.D.Y > Ymax) Ymax = obb.D.Y; if (obb.D.Y < Ymin) Ymin = obb.D.Y;
            if (obb.D.Z > Zmax) Zmax = obb.D.Z; if (obb.D.Z < Zmin) Zmin = obb.D.Z;

            if (obb.E.X > Xmax) Xmax = obb.E.X; if (obb.E.X < Xmin) Xmin = obb.E.X;
            if (obb.E.Y > Ymax) Ymax = obb.E.Y; if (obb.E.Y < Ymin) Ymin = obb.E.Y;
            if (obb.E.Z > Zmax) Zmax = obb.E.Z; if (obb.E.Z < Zmin) Zmin = obb.E.Z;

            if (obb.F.X > Xmax) Xmax = obb.F.X; if (obb.F.X < Xmin) Xmin = obb.F.X;
            if (obb.F.Y > Ymax) Ymax = obb.F.Y; if (obb.F.Y < Ymin) Ymin = obb.F.Y;
            if (obb.F.Z > Zmax) Zmax = obb.F.Z; if (obb.F.Z < Zmin) Zmin = obb.F.Z;

            if (obb.G.X > Xmax) Xmax = obb.G.X; if (obb.G.X < Xmin) Xmin = obb.G.X;
            if (obb.G.Y > Ymax) Ymax = obb.G.Y; if (obb.G.Y < Ymin) Ymin = obb.G.Y;
            if (obb.G.Z > Zmax) Zmax = obb.G.Z; if (obb.G.Z < Zmin) Zmin = obb.G.Z;

            if (obb.H.X > Xmax) Xmax = obb.H.X; if (obb.H.X < Xmin) Xmin = obb.H.X;
            if (obb.H.Y > Ymax) Ymax = obb.H.Y; if (obb.H.Y < Ymin) Ymin = obb.H.Y;
            if (obb.H.Z > Zmax) Zmax = obb.H.Z; if (obb.H.Z < Zmin) Zmin = obb.H.Z;

            return new AABB(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
        }

        public static AABB AABB_around_OBB(OBB obb, Matrix world) {
            float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
            float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

            Vector3 tmp_A = Vector3.Transform(obb.A, world);
            Vector3 tmp_B = Vector3.Transform(obb.B, world);
            Vector3 tmp_C = Vector3.Transform(obb.C, world);
            Vector3 tmp_D = Vector3.Transform(obb.D, world);
            Vector3 tmp_E = Vector3.Transform(obb.E, world);
            Vector3 tmp_F = Vector3.Transform(obb.F, world);
            Vector3 tmp_G = Vector3.Transform(obb.G, world);
            Vector3 tmp_H = Vector3.Transform(obb.H, world);

            if (tmp_A.X > Xmax) Xmax = tmp_A.X; if (tmp_A.X < Xmin) Xmin = tmp_A.X;
            if (tmp_A.Y > Ymax) Ymax = tmp_A.Y; if (tmp_A.Y < Ymin) Ymin = tmp_A.Y;
            if (tmp_A.Z > Zmax) Zmax = tmp_A.Z; if (tmp_A.Z < Zmin) Zmin = tmp_A.Z;

            if (tmp_B.X > Xmax) Xmax = tmp_B.X; if (tmp_B.X < Xmin) Xmin = tmp_B.X;
            if (tmp_B.Y > Ymax) Ymax = tmp_B.Y; if (tmp_B.Y < Ymin) Ymin = tmp_B.Y;
            if (tmp_B.Z > Zmax) Zmax = tmp_B.Z; if (tmp_B.Z < Zmin) Zmin = tmp_B.Z;

            if (tmp_C.X > Xmax) Xmax = tmp_C.X; if (tmp_C.X < Xmin) Xmin = tmp_C.X;
            if (tmp_C.Y > Ymax) Ymax = tmp_C.Y; if (tmp_C.Y < Ymin) Ymin = tmp_C.Y;
            if (tmp_C.Z > Zmax) Zmax = tmp_C.Z; if (tmp_C.Z < Zmin) Zmin = tmp_C.Z;

            if (tmp_D.X > Xmax) Xmax = tmp_D.X; if (tmp_D.X < Xmin) Xmin = tmp_D.X;
            if (tmp_D.Y > Ymax) Ymax = tmp_D.Y; if (tmp_D.Y < Ymin) Ymin = tmp_D.Y;
            if (tmp_D.Z > Zmax) Zmax = tmp_D.Z; if (tmp_D.Z < Zmin) Zmin = tmp_D.Z;

            if (tmp_E.X > Xmax) Xmax = tmp_E.X; if (tmp_E.X < Xmin) Xmin = tmp_E.X;
            if (tmp_E.Y > Ymax) Ymax = tmp_E.Y; if (tmp_E.Y < Ymin) Ymin = tmp_E.Y;
            if (tmp_E.Z > Zmax) Zmax = tmp_E.Z; if (tmp_E.Z < Zmin) Zmin = tmp_E.Z;

            if (tmp_F.X > Xmax) Xmax = tmp_F.X; if (tmp_F.X < Xmin) Xmin = tmp_F.X;
            if (tmp_F.Y > Ymax) Ymax = tmp_F.Y; if (tmp_F.Y < Ymin) Ymin = tmp_F.Y;
            if (tmp_F.Z > Zmax) Zmax = tmp_F.Z; if (tmp_F.Z < Zmin) Zmin = tmp_F.Z;

            if (tmp_G.X > Xmax) Xmax = tmp_G.X; if (tmp_G.X < Xmin) Xmin = tmp_G.X;
            if (tmp_G.Y > Ymax) Ymax = tmp_G.Y; if (tmp_G.Y < Ymin) Ymin = tmp_G.Y;
            if (tmp_G.Z > Zmax) Zmax = tmp_G.Z; if (tmp_G.Z < Zmin) Zmin = tmp_G.Z;

            if (tmp_H.X > Xmax) Xmax = tmp_H.X; if (tmp_H.X < Xmin) Xmin = tmp_H.X;
            if (tmp_H.Y > Ymax) Ymax = tmp_H.Y; if (tmp_H.Y < Ymin) Ymin = tmp_H.Y;
            if (tmp_H.Z > Zmax) Zmax = tmp_H.Z; if (tmp_H.Z < Zmin) Zmin = tmp_H.Z;

            return new AABB(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
        }
        public static BoundingBox BoundingBox_around_OBB(OBB obb, Matrix world) {
            float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
            float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

            Vector3 tmp_A = Vector3.Transform(obb.A, world);
            Vector3 tmp_B = Vector3.Transform(obb.B, world);
            Vector3 tmp_C = Vector3.Transform(obb.C, world);
            Vector3 tmp_D = Vector3.Transform(obb.D, world);
            Vector3 tmp_E = Vector3.Transform(obb.E, world);
            Vector3 tmp_F = Vector3.Transform(obb.F, world);
            Vector3 tmp_G = Vector3.Transform(obb.G, world);
            Vector3 tmp_H = Vector3.Transform(obb.H, world);

            if (tmp_A.X > Xmax) Xmax = tmp_A.X; if (tmp_A.X < Xmin) Xmin = tmp_A.X;
            if (tmp_A.Y > Ymax) Ymax = tmp_A.Y; if (tmp_A.Y < Ymin) Ymin = tmp_A.Y;
            if (tmp_A.Z > Zmax) Zmax = tmp_A.Z; if (tmp_A.Z < Zmin) Zmin = tmp_A.Z;

            if (tmp_B.X > Xmax) Xmax = tmp_B.X; if (tmp_B.X < Xmin) Xmin = tmp_B.X;
            if (tmp_B.Y > Ymax) Ymax = tmp_B.Y; if (tmp_B.Y < Ymin) Ymin = tmp_B.Y;
            if (tmp_B.Z > Zmax) Zmax = tmp_B.Z; if (tmp_B.Z < Zmin) Zmin = tmp_B.Z;

            if (tmp_C.X > Xmax) Xmax = tmp_C.X; if (tmp_C.X < Xmin) Xmin = tmp_C.X;
            if (tmp_C.Y > Ymax) Ymax = tmp_C.Y; if (tmp_C.Y < Ymin) Ymin = tmp_C.Y;
            if (tmp_C.Z > Zmax) Zmax = tmp_C.Z; if (tmp_C.Z < Zmin) Zmin = tmp_C.Z;

            if (tmp_D.X > Xmax) Xmax = tmp_D.X; if (tmp_D.X < Xmin) Xmin = tmp_D.X;
            if (tmp_D.Y > Ymax) Ymax = tmp_D.Y; if (tmp_D.Y < Ymin) Ymin = tmp_D.Y;
            if (tmp_D.Z > Zmax) Zmax = tmp_D.Z; if (tmp_D.Z < Zmin) Zmin = tmp_D.Z;

            if (tmp_E.X > Xmax) Xmax = tmp_E.X; if (tmp_E.X < Xmin) Xmin = tmp_E.X;
            if (tmp_E.Y > Ymax) Ymax = tmp_E.Y; if (tmp_E.Y < Ymin) Ymin = tmp_E.Y;
            if (tmp_E.Z > Zmax) Zmax = tmp_E.Z; if (tmp_E.Z < Zmin) Zmin = tmp_E.Z;

            if (tmp_F.X > Xmax) Xmax = tmp_F.X; if (tmp_F.X < Xmin) Xmin = tmp_F.X;
            if (tmp_F.Y > Ymax) Ymax = tmp_F.Y; if (tmp_F.Y < Ymin) Ymin = tmp_F.Y;
            if (tmp_F.Z > Zmax) Zmax = tmp_F.Z; if (tmp_F.Z < Zmin) Zmin = tmp_F.Z;

            if (tmp_G.X > Xmax) Xmax = tmp_G.X; if (tmp_G.X < Xmin) Xmin = tmp_G.X;
            if (tmp_G.Y > Ymax) Ymax = tmp_G.Y; if (tmp_G.Y < Ymin) Ymin = tmp_G.Y;
            if (tmp_G.Z > Zmax) Zmax = tmp_G.Z; if (tmp_G.Z < Zmin) Zmin = tmp_G.Z;

            if (tmp_H.X > Xmax) Xmax = tmp_H.X; if (tmp_H.X < Xmin) Xmin = tmp_H.X;
            if (tmp_H.Y > Ymax) Ymax = tmp_H.Y; if (tmp_H.Y < Ymin) Ymin = tmp_H.Y;
            if (tmp_H.Z > Zmax) Zmax = tmp_H.Z; if (tmp_H.Z < Zmin) Zmin = tmp_H.Z;

            return new BoundingBox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
        }
        public static BoundingBox BoundingBox_around_OBB(Cube obb, Matrix world) {
            float Xmin = float.MaxValue, Ymin = float.MaxValue, Zmin = float.MaxValue;
            float Xmax = float.MinValue, Ymax = float.MinValue, Zmax = float.MinValue;

            Vector3 tmp_A = Vector3.Transform(obb.A, world);
            Vector3 tmp_B = Vector3.Transform(obb.B, world);
            Vector3 tmp_C = Vector3.Transform(obb.C, world);
            Vector3 tmp_D = Vector3.Transform(obb.D, world);
            Vector3 tmp_E = Vector3.Transform(obb.E, world);
            Vector3 tmp_F = Vector3.Transform(obb.F, world);
            Vector3 tmp_G = Vector3.Transform(obb.G, world);
            Vector3 tmp_H = Vector3.Transform(obb.H, world);

            if (tmp_A.X > Xmax) Xmax = tmp_A.X; if (tmp_A.X < Xmin) Xmin = tmp_A.X;
            if (tmp_A.Y > Ymax) Ymax = tmp_A.Y; if (tmp_A.Y < Ymin) Ymin = tmp_A.Y;
            if (tmp_A.Z > Zmax) Zmax = tmp_A.Z; if (tmp_A.Z < Zmin) Zmin = tmp_A.Z;

            if (tmp_B.X > Xmax) Xmax = tmp_B.X; if (tmp_B.X < Xmin) Xmin = tmp_B.X;
            if (tmp_B.Y > Ymax) Ymax = tmp_B.Y; if (tmp_B.Y < Ymin) Ymin = tmp_B.Y;
            if (tmp_B.Z > Zmax) Zmax = tmp_B.Z; if (tmp_B.Z < Zmin) Zmin = tmp_B.Z;

            if (tmp_C.X > Xmax) Xmax = tmp_C.X; if (tmp_C.X < Xmin) Xmin = tmp_C.X;
            if (tmp_C.Y > Ymax) Ymax = tmp_C.Y; if (tmp_C.Y < Ymin) Ymin = tmp_C.Y;
            if (tmp_C.Z > Zmax) Zmax = tmp_C.Z; if (tmp_C.Z < Zmin) Zmin = tmp_C.Z;

            if (tmp_D.X > Xmax) Xmax = tmp_D.X; if (tmp_D.X < Xmin) Xmin = tmp_D.X;
            if (tmp_D.Y > Ymax) Ymax = tmp_D.Y; if (tmp_D.Y < Ymin) Ymin = tmp_D.Y;
            if (tmp_D.Z > Zmax) Zmax = tmp_D.Z; if (tmp_D.Z < Zmin) Zmin = tmp_D.Z;

            if (tmp_E.X > Xmax) Xmax = tmp_E.X; if (tmp_E.X < Xmin) Xmin = tmp_E.X;
            if (tmp_E.Y > Ymax) Ymax = tmp_E.Y; if (tmp_E.Y < Ymin) Ymin = tmp_E.Y;
            if (tmp_E.Z > Zmax) Zmax = tmp_E.Z; if (tmp_E.Z < Zmin) Zmin = tmp_E.Z;

            if (tmp_F.X > Xmax) Xmax = tmp_F.X; if (tmp_F.X < Xmin) Xmin = tmp_F.X;
            if (tmp_F.Y > Ymax) Ymax = tmp_F.Y; if (tmp_F.Y < Ymin) Ymin = tmp_F.Y;
            if (tmp_F.Z > Zmax) Zmax = tmp_F.Z; if (tmp_F.Z < Zmin) Zmin = tmp_F.Z;

            if (tmp_G.X > Xmax) Xmax = tmp_G.X; if (tmp_G.X < Xmin) Xmin = tmp_G.X;
            if (tmp_G.Y > Ymax) Ymax = tmp_G.Y; if (tmp_G.Y < Ymin) Ymin = tmp_G.Y;
            if (tmp_G.Z > Zmax) Zmax = tmp_G.Z; if (tmp_G.Z < Zmin) Zmin = tmp_G.Z;

            if (tmp_H.X > Xmax) Xmax = tmp_H.X; if (tmp_H.X < Xmin) Xmin = tmp_H.X;
            if (tmp_H.Y > Ymax) Ymax = tmp_H.Y; if (tmp_H.Y < Ymin) Ymin = tmp_H.Y;
            if (tmp_H.Z > Zmax) Zmax = tmp_H.Z; if (tmp_H.Z < Zmin) Zmin = tmp_H.Z;

            return new BoundingBox(new Vector3(Xmin, Ymin, Zmin), new Vector3(Xmax, Ymax, Zmax));
        }

        public static BoundingBox find_bounding_box_around_points(params Vector3[] points) {
            Vector3 min = Vector3.One * float.MaxValue, max = Vector3.One * float.MinValue;

            foreach (Vector3 point in points) {
                if (point.X < min.X)
                    min.X = point.X;
                if (point.Y < min.Y)
                    min.Y = point.Y;

                if (point.X > max.X)
                    max.X = point.X;
                if (point.Y > max.Y)
                    max.Y = point.Y;
            }

            return new BoundingBox(min, max);
        }
        ///
        ///  DYNAMIC      :  STAGE
        ///    SWEPT QUAD |  ! PLANE 
        ///    SWEPT TRI  |  ! QUAD 
        ///  ! CAPSULE    |  ! QUADSTRIP
        ///  ! SPHERE     |  ! TRI
        ///    OBB        |    TERRAIN
        ///               |    EACH OTHER 
        ///               |    THEMSELVES 
        ///              
        /// ALL DYNAMIC SHAPES SHOULD BE ABLE TO COLLIDE WITH ALL STAGE SHAPES, EACH OTHER, AND THEMSELVES
        /// THE ! CONNOTES EXCITEMENT AND THAT IT'S DONE

    }
}