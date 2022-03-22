using Magpie;
using Magpie.Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public static class Math2D {
        #region Math/Conversion functions

        public static XYPair measure_string(string font, string str) {
            var v = ContentHandler.resources[font].value_ft.MeasureString(str);
            return new XYPair(v.X, v.Y);
        }

        public static void clamp(Vector2 input, out Vector2 output, float Xmin, float Ymin, float Xmax, float Ymax) {
            output = input;
            if (output.X > Xmax) output.X = Xmax;
            if (output.X < Xmin) output.X = Xmin;

            if (output.Y > Ymax) output.Y = Ymax;
            if (output.Y < Ymin) output.Y = Ymin;
        }


        public static void clamp(XYPair input, out XYPair output, int Xmin, int Ymin, int Xmax, int Ymax) {
            output = input;
            if (output.X > Xmax) output.X = Xmax;
            if (output.X < Xmin) output.X = Xmin;

            if (output.Y > Ymax) output.Y = Ymax;
            if (output.Y < Ymin) output.Y = Ymin;
        }

        public static Vector2 clamp(Vector2 input, float Xmin, float Ymin, float Xmax, float Ymax) {
            Vector2 output = input;
            if (output.X > Xmax) output.X = Xmax;
            if (output.X < Xmin) output.X = Xmin;

            if (output.Y > Ymax) output.Y = Ymax;
            if (output.Y < Ymin) output.Y = Ymin;

            return output;
        }

        public static XYPair clamp(XYPair input, int Xmin, int Ymin, int Xmax, int Ymax) {
            XYPair output = input;

            if (output.X > Xmax) output.X = Xmax;
            if (output.X < Xmin) output.X = Xmin;

            if (output.Y > Ymax) output.Y = Ymax;
            if (output.Y < Ymin) output.Y = Ymin;

            return output;
        }


        public static float ComputeGaussian(float n, float blur_amount) {
            float theta = blur_amount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                            Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        public static bool point_within_square(Vector2 min, Vector2 max, Vector2 point) {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y) return true;
            return false;
        }

        public static bool point_within_square(Vector2 min, Vector2 max, XYPair point) {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y) return true;
            return false;
        }

        public static bool point_within_square(XYPair min, XYPair max, XYPair point) {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y) return true;
            return false;
        }

        public static bool point_within_square(XYPair min, XYPair max, Vector2 point) {
            if (point.X > min.X && point.X < max.X && point.Y > min.Y && point.Y < max.Y) return true;
            return false;
        }

        public static bool point_within_regular_obb(Vector2 A, Vector2 B, Vector2 C, Vector2 D, Vector2 point) {
            Vector2 AB = Vector2.Normalize(B - A);
            Vector2 BC = Vector2.Normalize(C - B);
            Vector2 CD = Vector2.Normalize(D - C);
            Vector2 DA = Vector2.Normalize(A - D);
            
            if (Vector2.Dot(AB, point - B) > 0)
                return false;

            if (Vector2.Dot(BC, point - C) > 0)
                return false;

            if (Vector2.Dot(CD, point - D) > 0)
                return false;

            if (Vector2.Dot(DA, point - A) > 0)
                return false;

            return true;            
        }

        public static bool point_within_polygon(Vector2 point, params Vector2[] poly_points) {
            bool result = true;

            for (int i = 0; i < poly_points.Length; i++) {
                Vector2 v1, v2;
                if (i < poly_points.Length-1) {
                    v1 = (Vector2)poly_points[i];
                    v2 = (Vector2)poly_points[i + 1];
                } else {
                    v1 = (Vector2)poly_points[i];
                    v2 = (Vector2)poly_points[0];
                }
                if (((point.X-v1.X) * (v1.Y-v2.Y)) + ((point.Y-v1.Y)*(v2.X-v1.X)) <= 0) {
                    return false;
                }
            }

            return result;
        }

        public static bool same_direction_as_origin(Vector2 direction, Vector2 origin_dir) {
            return (Vector2.Dot(direction, origin_dir) > 0);
        }

        public static Vector2 perpendicular(Vector2 a) {
            return new Vector2(a.Y, -a.X);
        }

        public static Vector2 perpendicular_inverse(Vector2 a) {
            return new Vector2(-a.Y, a.X);
        }

        public static Vector2 cross2D3D(Vector2 a_up, Vector2 b) {
            Vector3 cross_tmp = Vector3.Cross(new Vector3(a_up, 0), new Vector3(b.X, 0, b.Y));

            return new Vector2(cross_tmp.X, cross_tmp.Y);
        }

        public static Vector2 point_of_minimum_norm(Vector2 a, Vector2 b, Vector2 p) {
            var ab = b - a;
            var t = Vector2.Dot(p - a, ab) / Vector2.Dot(ab, ab);

            if (t < 0) t = 0;
            if (t > 1) t = 1;

            return a + t * ab;
        }

        #endregion
    }
}
