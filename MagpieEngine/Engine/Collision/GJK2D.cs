using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Math2D;

namespace Magpie.Engine.Collision {
    public class Collision2D {
        public delegate void ui_delegate();
        public delegate void ui_delegate_option(object data);

        public enum CollisionLevel {
            NONE = 0,
            NEAR = 1,
            COLLIDING = 2
        }
        public enum DebugDrawLevel {
            NONE = 0,
            SIMPLE = 1,
            MAX = 2,
            COMMENTARY = 3
        }

        public interface ISupport2D {
            Vector2 origin { get; }
            Color debug_color { get; set; }

            Vector2 FarthestPoint(Vector2 direction_n, bool normalize = true, bool transform = true);

            float FindRadius();
            void Draw(Color color);

            void SetPosition(Vector2 position);
            void TranslatePosition(Vector2 distance);
        }

        public class Point2D : ISupport2D {
            public Vector2 position;

            public Vector2 origin => position;

            public Color debug_color { get; set; } = Color.MediumPurple;

            public Point2D(Vector2 pos) {
                position = pos;
            }

            public void Draw(Color color) {
                Draw2D.DrawPoint(position, color, 1);
            }

            public Vector2 FarthestPoint(Vector2 direction_n, bool normalize = true, bool transform = true) {
                return position;
            }

            public float FindRadius() {
                return 0;
            }

            public void SetPosition(Vector2 position) {
                this.position = position;
            }

            public void TranslatePosition(Vector2 distance) {
                this.position += position;
            }
        }

        public class Line2D : ISupport2D {
            public Vector2 actual_pos = Vector2.Zero;
            public Vector2 actual_A;
            public Vector2 actual_B;
            public Vector2 A => actual_pos + actual_A;
            public Vector2 B => actual_pos + actual_B;
            public Vector2 origin => (A+B)/2;
            float r;

            public Color debug_color { get; set; } = Color.Red;

            public Line2D(Vector2 A, Vector2 B) {
                this.actual_A = A; this.actual_B = B;               
            }

            public void Draw(Color color) {
                Draw2D.line(A, B, 2, color);
            }

            public Vector2 FarthestPoint(Vector2 direction_n, bool normalize = true, bool transform = true) {
                if (Vector2.Dot(direction_n, A) < Vector2.Dot(direction_n, B))
                    return B;                
                else
                    return A;
            }

            public float FindRadius() {
                return r;
            }

            public void SetPosition(Vector2 position) {
                actual_pos = position;
            }

            public void TranslatePosition(Vector2 distance) {
                actual_pos += distance;
            }
        }

        public class Circle2D : ISupport2D {
            Vector2 pos;
            public Vector2 origin => pos;
            float r;
            public float radius => r;

            public Color debug_color { get; set; } = Color.Red;

            public Circle2D(Vector2 pos, float r) {
                this.pos = pos;
                this.r = r;
            }

            public void Draw(Color color) {
                Draw2D.poly_circle(pos, r, 16, 1f, color);
            }

            public Vector2 FarthestPoint(Vector2 direction_n, bool normalize = true, bool transform = true) {
                return origin + (Vector2.Normalize(direction_n) * radius);
            }

            public float FindRadius() {
                return r;
            }

            public void SetPosition(Vector2 position) {
                pos = position;
            }

            public void TranslatePosition(Vector2 distance) {
                pos += distance;
            }
        }

        public class AABB2D : ISupport2D {

            public Vector2 top_left;
            public Vector2 bottom_right;

            Vector2 _top_right;
            Vector2 _bottom_left;

            public Vector2 top_right { get { update_bounds(); return _top_right; } }
            public Vector2 bottom_left { get { update_bounds(); return _bottom_left; } }

            public ui_delegate click;
            public ui_delegate_option click_option;
            public ui_delegate right_click;
            public ui_delegate_option right_click_option;

            BoundingBox monogame_bb;
            
            public BoundingBox mg_bounding_box { get { update_mg_bb(); return monogame_bb; } }

            void update_mg_bb() {
                monogame_bb = new BoundingBox(new Vector3(top_left.X, top_left.Y, 0), new Vector3(bottom_right.X, bottom_right.Y, 1));
            }

            private void update_bounds() {
                _top_right = new Vector2(right, top);
                _bottom_left = new Vector2(left, bottom);
            }

            public float width => bottom_right.X - top_left.X;
            public float height => bottom_right.Y - top_left.Y;

            public float left => top_left.X;
            public float right => bottom_right.X;

            public float top => top_left.Y;
            public float bottom => bottom_right.Y;

            public Vector2 position {
                get => origin; set {
                    var offset = value - origin;
                    top_left = top_left + offset;
                    bottom_right = bottom_right + offset;
                }
            }

            public Vector2 origin => top_left + ((bottom_right - top_left) / 2);

            public Color debug_color { get; set; } = Color.Turquoise;

            public AABB2D(int X, int Y, int width, int height) {
                this.top_left = new Vector2(X, Y);
                this.bottom_right = new Vector2(X+width, Y+height);
                update_mg_bb();
            }
            public AABB2D(Vector2 top_left, Vector2 bottom_right) {
                this.top_left = top_left;
                this.bottom_right = bottom_right;
                update_mg_bb();
            }
            public AABB2D(XYPair top_left, XYPair bottom_right) {
                this.top_left = top_left.ToVector2();
                this.bottom_right = bottom_right.ToVector2();
                update_mg_bb();
            }

            public Vector2 FarthestPoint(Vector2 direction_n, bool normalize = true, bool transform = true) {
                Vector2 test_point = origin + direction_n;
                Vector2 result = Vector2.Zero;

                if (test_point.X > right) {
                    result.X = right;
                } else if (test_point.X < left) {
                    result.X = left;
                } else result.X = test_point.X;

                if (test_point.Y < top) {
                    result.Y = top;
                } else if (test_point.Y > bottom) {
                    result.Y = bottom;
                } else result.Y = test_point.Y;
                

                return result;
            }

            public void Draw(Color color) {
                //Helper2D.DrawPoint(sb, origin, color);
                Draw2D.square(top_left, bottom_right, 1f, Color.White);
            }

            public float FindRadius() {
                return Vector2.Distance(origin, bottom_right);
            }

            public void SetPosition(Vector2 position) {
                var offset = position - origin;
                top_left = top_left + offset;
                bottom_right = bottom_right + offset;
            }

            public void TranslatePosition(Vector2 distance) {
                top_left += distance;
                bottom_right += distance;
            }
        }

        public static class GJK2D {

            public static Vector2 AB(ISupport2D A, ISupport2D B, Vector2 dir) {
                return (A.FarthestPoint(dir)) - (B.FarthestPoint(-dir));
            }

            public struct GJKResult {
                public CollisionLevel collision_level;

                public Vector2 A;
                public Vector2 B;
                public Vector2 C;

                public Vector2 dbg_offset;

                public Color color;

                public void draw() {
                    color = Color.White;

                    if (collision_level == CollisionLevel.NONE)
                        color = Color.Red;
                    else if (collision_level == CollisionLevel.NEAR)
                        color = Color.Orange;
                    else if (collision_level == CollisionLevel.COLLIDING)
                        color = Color.LightGreen;
                    else
                        color = Color.DarkGray;

                    Draw2D.line(A, B, 1f, color);

                    Draw2D.cross(new XYPair(dbg_offset), 15, 15, Color.Red);
                }
            }
            
            public static GJKResult TestShapes(ISupport2D A, ISupport2D B, SpriteBatch sb = null) {
                GJKResult result = new GJKResult();
                if (A == null || B == null) {
                    result.collision_level = CollisionLevel.NONE;
                    return result;
                }
                //setup initial direction, points, etc
                Vector2 direction = Vector2.Normalize(A.origin - B.origin);
                Vector2 first_point = AB(A, B, direction);

                direction = -(first_point);

                Vector2 second_point = AB(A, B, direction);
                Vector2 third_point = second_point;
                
                Vector2 ab = first_point - second_point;
                Vector2 bc = Vector2.Zero;
                Vector2 ca = Vector2.Zero;
                Vector2 ao = -second_point;

                Vector2 p = point_of_minimum_norm(first_point, second_point, Vector2.Zero);
                Vector2 p2 = Vector2.Zero;
                Vector2 p3 = Vector2.Zero;

                Vector2 dbg_offset = (Vector2.UnitX * 1100) + (Vector2.UnitY * 500);

                result.A = (A.FarthestPoint(direction));
                result.B = (B.FarthestPoint(-direction));

                if (Vector2.Dot(second_point, direction) < 0) {
                    result.collision_level = CollisionLevel.NONE;

                } else {
                    if (same_direction_as_origin(ab, ao)) {
                        if (same_direction_as_origin(perpendicular(-ab), ao)) {
                            direction -= perpendicular(ab);
                        } else {
                            direction -= perpendicular_inverse(ab);
                        }

                        result.collision_level = CollisionLevel.NEAR;

                        third_point = AB(A, B, direction);

                        p2 = point_of_minimum_norm(second_point, third_point, Vector2.Zero);
                        p3 = point_of_minimum_norm(third_point, first_point, Vector2.Zero);

                        bc = second_point - third_point;
                        ca = third_point - first_point;
                        bool bct, cat;

                        if (same_direction_as_origin(perpendicular(-ab), ao)) {
                            bct = same_direction_as_origin(perpendicular_inverse(bc), ao);
                            cat = same_direction_as_origin(perpendicular(ca), ao);

                        } else {
                            bct = same_direction_as_origin(perpendicular(bc), ao);
                            cat = same_direction_as_origin(perpendicular_inverse(ca), ao);
                        }

                        if (bct && cat) {
                            result.collision_level = CollisionLevel.COLLIDING;
                        }

                    } else {
                        direction -= second_point;
                    }
                }
                
                result.dbg_offset = dbg_offset;

                return result;
            }
        }
    }
}
