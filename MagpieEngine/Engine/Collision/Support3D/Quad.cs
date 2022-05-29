using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;

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

        public float radius { get; set; } = 0f;

        public shape_type shape { get; } = shape_type.quad;

        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 D;

        public Vector3 origin => (A + B + C + D) / 4f;

        public Vector3 wA => Vector3.Transform(A, Matrix.CreateTranslation(position));
        public Vector3 wB => Vector3.Transform(B, Matrix.CreateTranslation(position));
        public Vector3 wC => Vector3.Transform(C, Matrix.CreateTranslation(position));
        public Vector3 wD => Vector3.Transform(D, Matrix.CreateTranslation(position));

        public BoundingBox find_bounding_box() {
            return CollisionHelper.BoundingBox_around_BoundingBoxes(
                CollisionHelper.BoundingBox_around_capsule(D, A, radius),
                CollisionHelper.BoundingBox_around_capsule(C, B, radius)
                );
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
        public Quad(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            create(A, B, C, D);
        }

        public void create(float scale_x, float scale_y) {
            A = (Vector3.Left * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            B = (Vector3.Right * 0.5f * scale_x) + (Vector3.Forward * 0.5f * scale_y);
            D = (Vector3.Left * 0.5f * scale_x) + (Vector3.Backward* 0.5f * scale_y);
            C = (Vector3.Right * 0.5f * scale_x) + (Vector3.Backward* 0.5f * scale_y);
        }

        public void create(Vector3 A, Vector3 B, Vector3 C, Vector3 D) {
            this.A = A;
            this.B = B;
            this.C = C;
            this.D = D;
        }

        public void draw() {
            Matrix w = Matrix.CreateTranslation(position);

            //Draw3D.fill_quad(w, A, B, C, D, Color.White * 0.9f, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.xyz_cross(Vector3.Transform(A, w), 5f, Color.DeepPink);
            Draw3D.xyz_cross(Vector3.Transform(B, w), 5f, Color.LightPink);
            Draw3D.xyz_cross(Vector3.Transform(C, w), 5f, Color.LightPink);
            Draw3D.xyz_cross(Vector3.Transform(D, w), 5f, Color.DeepPink);

            var c = Vector3.Normalize(Vector3.Cross(wA-wB, wA-wC));

            //Draw3D.line(Vector3.Transform(A + c, w), Vector3.Transform(C + c, w), Color.Red);

            Draw3D.lines(Color.LightPink,
                Vector3.Transform(A, w),
                Vector3.Transform(B, w),
                Vector3.Transform(C, w),
                Vector3.Transform(D, w),
                Vector3.Transform(A, w));


            Draw3D.line(
                A + (c * radius),
                B + (c * radius), Color.LightPink);
            Draw3D.line(
                D + -(c * radius),
                C + -(c * radius), Color.LightPink);
                
            Draw3D.line(
                A + -(Vector3.Normalize(wD - wA) * radius),
                B + -(Vector3.Normalize(wC - wB) * radius), Color.LightPink);

            Draw3D.line(
                D + (Vector3.Normalize(wD - wA) * radius),
                C + (Vector3.Normalize(wC - wB) * radius), Color.LightPink);
                
            Draw3D.cube(find_bounding_box(), Color.Magenta, EngineState.camera.view, EngineState.camera.projection);
        }
    }
}
