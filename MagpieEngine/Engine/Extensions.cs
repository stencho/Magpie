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

        public static string simple_vector3_string_brackets(this Vector3 input) {
            return string.Format("[{0:F2}, {1:F2}, {2:F2}]", input.X, input.Y, input.Z);
        }
        public static string simple_vector2_string_brackets(this Vector2 input) {
            return string.Format("[{0:F2}, {1:F2}]", input.X, input.Y);
        }
    }
}
