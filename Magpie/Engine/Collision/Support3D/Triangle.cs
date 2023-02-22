using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Triangle : Shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A;

        public shape_type shape { get; } = shape_type.tri;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public Vector3 normal => CollisionHelper.triangle_normal(A, B, C);

        public float radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            return new BoundingBox();
        }

        public Triangle() {
            create(1, 1);
        }
        public Triangle(float scale) {
            create(scale, scale);
        }
        public Triangle(float scale_x, float scale_y) {
            create(scale_x, scale_y);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Up * 0.5f * scale_y);
            B = (Vector3.Left * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
        }

        public Triangle(Vector3 A, Vector3 B, Vector3 C) {
            this.A = A;
            this.B = B;
            this.C = C;
        }

        public void draw(Vector3 offset) {
            Matrix w = orientation * Matrix.CreateTranslation(offset + position);

            Draw3D.fill_tri(w, A, B, C, Color.White * 0.9f);

            Draw3D.lines(Color.MonoGameOrange,
                Vector3.Transform(A, w),
                Vector3.Transform(B, w),
                Vector3.Transform(C, w),
                Vector3.Transform(A, w));
        }

    }

}
