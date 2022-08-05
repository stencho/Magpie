using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public static class Extensions {
        public static Vector2 XZ(this Vector3 pos) {
            return new Vector2(pos.X, pos.Z);
        }

        public static string simple_vector3_string(this Vector3 input) {
            return string.Format("{0:F2}, {1:F2}, {2:F2}", input.X, input.Y, input.Z);
        }
        public static string simple_vector2_string(this Vector2 input) {
            return string.Format("{0:F2}, {1:F2}", input.X, input.Y);
        }
        public static string simple_vector2_x_string(this Vector2 input) {
            return string.Format("{0:F2}x{1:F2}", input.X, input.Y);
        }

        public static string simple_vector2_x_string_no_dec(this Vector2 input) {
            return string.Format("{0:F0}x{1:F0}", input.X, input.Y);
        }

        public static string simple_vector3_string_brackets(this Vector3 input) {
            return string.Format("[{0:F2}, {1:F2}, {2:F2}]", input.X, input.Y, input.Z);
        }
        public static string simple_vector2_string_brackets(this Vector2 input) {
            return string.Format("[{0:F2}, {1:F2}]", input.X, input.Y);
        }

        public static Vector3 ToVector3XZ(this Vector2 v) {
            return new Vector3(v.X, 0, v.Y);
        }

        public static bool contains_nan(this Vector3 a) { return (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z)); }

        public static Vector3 dimensions (this BoundingBox bb) {
            return bb.Max - bb.Min;
        }
        public static Vector3 half_size(this BoundingBox bb) {
            return bb.Max - ((bb.Min + bb.Max) / 2f);
        }

        public static Vector3 center(this BoundingBox bb) {
            return (bb.Min + bb.Max) / 2f;
        }

        public static void draw_debug(this BoundingBox bb, Color color) {
            Draw3D.cube(A(bb), B(bb), C(bb), D(bb), E(bb), F(bb), G(bb), H(bb), color);
        }

        public static Vector3 A (this BoundingBox bb) { return bb.center() + bb.half_size(); }
        public static Vector3 B (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2)); }

        public static Vector3 C (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitY * 2)); }
        public static Vector3 D (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2)); }

        public static Vector3 E (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2)); }
        public static Vector3 F (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitZ * 2)); }
                                
        public static Vector3 G (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2) - (Vector3.UnitY * 2)); }
        public static Vector3 H (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2) - (Vector3.UnitZ * 2)); }
    }
}
