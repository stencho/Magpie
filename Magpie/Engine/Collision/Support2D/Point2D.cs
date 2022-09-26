using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision.Support2D {
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
}
