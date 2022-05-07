using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Capsule : shape3D {
        public Vector3 AB_normal => Vector3.Normalize(B - A);
        public float AB_length => Vector3.Distance(A, B);
        public float AB_full_length => Vector3.Distance(A - (AB_normal * radius), B + (AB_normal * radius));

        public Vector3 origin => (A + B) / 2f;

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => A;//A + ((B-A) / 2f);

        public shape_type shape { get; } = shape_type.capsule;

        public Vector3 A;
        public Vector3 B;
        public float radius;

        public AABB find_bounding_box() {
            Vector3 min, max;

            min = position;
            max = position;

            min += Vector3.Min(A, B);
            max += Vector3.Max(A, B);

            min -= (Vector3.One * radius);
            max += (Vector3.One * radius);

            min -= origin;
            max -= origin;

            return new AABB(min, max);
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


        public void draw() {
            Matrix w = orientation * Matrix.CreateTranslation(position);
            Draw3D.capsule(Vector3.Transform(A, w), Vector3.Transform(B, w), radius, Color.MonoGameOrange);
            find_bounding_box().draw(origin, Color.Red);
        }

    }
}
