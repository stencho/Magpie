using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Triangle : shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A;

        public shape_type shape { get; } = shape_type.tri;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;

        public AABB find_bounding_box() {
            return new AABB();
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

        public void draw() {
            Matrix w = orientation * Matrix.CreateTranslation(position);

            Draw3D.fill_tri(EngineState.graphics_device, w, A, B, C, Color.White * 0.9f, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.lines(EngineState.graphics_device, Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection,
                Vector3.Transform(A, w),
                Vector3.Transform(B, w),
                Vector3.Transform(C, w),
                Vector3.Transform(A, w));
        }

    }

}
