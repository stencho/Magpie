using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;

namespace Magpie.Engine.Collision.Support3D {
    public class Capsule : Shape3D {
        public Vector3 AB_normal => Vector3.Up;
        public float AB_length => Vector3.Distance(A, B);
        public float AB_full_length => Vector3.Distance(A - (AB_normal * radius), B + (AB_normal * radius));

        public Vector3 origin => (A + B) / 2f;

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A;

        public shape_type shape { get; } = shape_type.capsule;

        public Vector3 A;
        public Vector3 B;

        public float radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            Matrix w = orientation * Matrix.CreateTranslation(position);
            return CollisionHelper.BoundingBox_around_capsule(Vector3.Transform(A, w), Vector3.Transform(B, w), radius);
        }

        public Capsule() {
            A = Vector3.Zero;
            B = Vector3.Up * 1.8f;
            radius = 1f;
        }

        public Capsule(float height) {
            A = Vector3.Zero;
            B = Vector3.Up * height;
            radius = 0.4f;
        }

        public Capsule(float height, float radius) {
            A = Vector3.Zero;
            B = Vector3.Up * height;
            this.radius = radius;
        }
        
        public Capsule(Vector3 A, Vector3 B, float radius) {
            this.A = A;
            this.B = B;
            this.radius = radius;

            this.position = Vector3.Zero;
        }

        public void draw(Vector3 offset) {
            Matrix w = orientation * Matrix.CreateTranslation(offset + position);
            var aw = Vector3.Transform(A, w);
            var bw = Vector3.Transform(B, w);
            Draw3D.cylinder(aw, bw, radius, Color.MonoGameOrange);

            Draw3D.sphere(aw, radius, Color.MonoGameOrange);
            Draw3D.sphere(bw, radius, Color.MonoGameOrange);

            //Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);
            BoundingBox bb = find_bounding_box();

            Draw3D.cube(origin + offset + position, (bb.Max - bb.Min)/2, Color.MonoGameOrange, Matrix.Identity);

        }
    }


    public class PointCapsule : Shape3D {
        public Vector3 AB_normal => Vector3.Up;
        public float AB_length => Vector3.Distance(A, B);
        public float AB_full_length => Vector3.Distance(A - (AB_normal * radius), B + (AB_normal * radius));

        public Vector3 origin => (A + B) / 2f;

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A + ((B - A) / 2f);

        public shape_type shape { get; } = shape_type.point_capsule;

        public Vector3 A;
        public Vector3 B;

        public float radius { get { return 0f; } set { fake_radius = value; } }
        public float fake_radius = 0f;

        public BoundingBox find_bounding_box() {
            Matrix w = orientation * Matrix.CreateTranslation(position);
            return CollisionHelper.BoundingBox_around_capsule(Vector3.Transform(A, w), Vector3.Transform(B, w), radius);
        }

        public PointCapsule() {
            A = Vector3.Zero;
            B = Vector3.Up * 1.8f;
            radius = 1f;
        }

        public PointCapsule(float height) {
            A = Vector3.Zero;
            B = Vector3.Up * height;
            radius = 0.4f;
        }

        public PointCapsule(float height, float radius) {
            A = Vector3.Up * height; 
            B = Vector3.Zero;
            this.radius = radius;
        }

        public PointCapsule(Vector3 A, Vector3 B, float radius) {
            this.A = A;
            this.B = B;
            this.radius = radius;

            this.position = Vector3.Zero;
        }

        public void draw(Vector3 offset) {
            Matrix w = orientation * Matrix.CreateTranslation(offset + position);
            var aw = Vector3.Transform(A, w);
            var bw = Vector3.Transform(B, w);
            Draw3D.cylinder(aw, bw, fake_radius, Color.MonoGameOrange);

            Draw3D.sphere(aw, fake_radius, Color.MonoGameOrange);
            Draw3D.sphere(bw, fake_radius, Color.MonoGameOrange);

            //Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);
            BoundingBox bb = find_bounding_box();

            Draw3D.cube(origin + offset + position, (bb.Max - bb.Min) / 2, Color.MonoGameOrange, Matrix.Identity);

        }
    }
}
