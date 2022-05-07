using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Quad : shape3D {
        public Matrix orientation {
            get { return Matrix.Identity; }
            set {
                _orientation = value;
            }
        }

        private Matrix _orientation = Matrix.Identity;
        public Vector3 position { get; set; }
        public Vector3 start_point => A;


        public shape_type shape { get; } = shape_type.quad;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;

        public Vector3 wA => Vector3.Transform(A, Matrix.CreateTranslation(position));
        public Vector3 wB => Vector3.Transform(B, Matrix.CreateTranslation(position));
        public Vector3 wC => Vector3.Transform(C, Matrix.CreateTranslation(position));
        public Vector3 wD => Vector3.Transform(D, Matrix.CreateTranslation(position));

        public AABB find_bounding_box() {
            return new AABB();
        }

        public Quad() {
            create(1, 1);
        }
        public Quad(float scale) {
            create(scale, scale);
        }
        public Quad(float scale_x, float scale_y) {
            create(scale_x, scale_y);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f * scale_x) + (Vector3.Up * 0.5f * scale_y);
            B = (Vector3.Right * 0.5f * scale_x) + (Vector3.Up * 0.5f * scale_y);
            D = (Vector3.Left * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Down * 0.5f * scale_y);
        }


        public void draw() {
            Matrix w = Matrix.CreateTranslation(position);

            Draw3D.fill_quad(w, A, B, C, D, Color.White * 0.9f, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.xyz_cross(Vector3.Transform(A, w), 5f, Color.Green);
            Draw3D.xyz_cross(Vector3.Transform(B, w), 5f, Color.Blue);
            Draw3D.xyz_cross(Vector3.Transform(C, w), 5f, Color.Yellow);
            Draw3D.xyz_cross(Vector3.Transform(D, w), 5f, Color.Red);

            Draw3D.line(Vector3.Transform(A, w), Vector3.Transform(C, w), Color.Red);
            Draw3D.lines(Color.MonoGameOrange,
                Vector3.Transform(A, w),
                Vector3.Transform(B, w),
                Vector3.Transform(C, w),
                Vector3.Transform(D, w),
                Vector3.Transform(A, w));
        }
    }
}
