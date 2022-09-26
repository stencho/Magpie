using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Line3D : Shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A;

        public shape_type shape { get; } = shape_type.line;

        public Vector3 A;
        public Vector3 B;

        public float radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            return new BoundingBox();
        }

        public Line3D() {
            A = Vector3.Zero;
            B = Vector3.Up * 1.8f;
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f);
            B = (Vector3.Right * 0.5f);
        }

        public void draw() {
            Matrix w = orientation * Matrix.CreateTranslation(position);

            Draw3D.line(
                Vector3.Transform(A, w),
                Vector3.Transform(B, w),
                Color.MonoGameOrange);
        }

    }

}
