using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision.Support2D {
    public class Circle2D : Shape2D {
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
            Draw2D.circle(pos, (int)r, color);
        }

        public Vector2 support(Vector2 direction_n, bool normalize = true, bool transform = true) {
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
}
