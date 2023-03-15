using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision.Support2D {
    public class BoundingBox2D : Shape2D {

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
                var size = bottom_right - top_left;
                top_left = value;
                bottom_right = top_left + size;
            }
        }

        public Vector2 origin => top_left + ((bottom_right - top_left) / 2);

        public Color debug_color { get; set; } = Color.Turquoise;

        public BoundingBox2D(int X, int Y, int width, int height) {
            this.top_left = new Vector2(X, Y);
            this.bottom_right = new Vector2(X + width, Y + height);
            update_mg_bb();
        }
        public BoundingBox2D(float X, float Y, float width, float height) {
            this.top_left = new Vector2(X, Y);
            this.bottom_right = new Vector2(X + width, Y + height);
            update_mg_bb();
        }
        public BoundingBox2D(Vector2 top_left, Vector2 bottom_right) {
            this.top_left = top_left;
            this.bottom_right = bottom_right;
            update_mg_bb();
        }
        public BoundingBox2D(XYPair top_left, XYPair bottom_right) {
            this.top_left = top_left.ToVector2();
            this.bottom_right = bottom_right.ToVector2();
            update_mg_bb();
        }

        public Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true) {
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

            Draw2D.square(top_left, bottom_right, 1f, color);
        }

        public float FindRadius() {
            return Vector2.Distance(origin, bottom_right);
        }

        public void set(Vector2 top_left, Vector2 bottom_right) {
            this.top_left = top_left;
            this.bottom_right = bottom_right;
        }

        public void SetPosition(Vector2 position) {
            var size = bottom_right - top_left;
            top_left = position;
            bottom_right = top_left + size;
        }

        public void SetSize(Vector2 size) {
            var offset = position - origin;
            bottom_right = top_left + size;
        }

        public void TranslatePosition(Vector2 distance) {
            top_left += distance;
            bottom_right += distance;
        }

        public void set(XYPair top_left, XYPair bottom_right) {
            this.top_left = top_left.ToVector2();
            this.bottom_right = bottom_right.ToVector2();
        } 

        public void SetPosition(XYPair position) {
            var size = bottom_right - top_left;
            top_left = position.ToVector2();
            bottom_right = top_left + size;
        }

        public void SetSize(XYPair size) {
            var offset = position - origin;
            bottom_right = top_left + size;
        }

        public void TranslatePosition(XYPair distance) {
            top_left += distance;
            bottom_right += distance;
        }
    }
}
