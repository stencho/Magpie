using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Sphere : Shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => P;

        public shape_type shape { get; } = shape_type.sphere;

        public Vector3 P;

        public float radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            return new BoundingBox(Vector3.Transform(position, orientation) - (Vector3.One * radius), Vector3.Transform(position, orientation) + (Vector3.One * radius));
        }

        public Sphere(float radius) {
            P = Vector3.Zero;

            this.radius = radius;
        }

        public void draw(Vector3 offset) {
            Draw3D.sphere(Vector3.Transform(Vector3.Zero, orientation * Matrix.CreateTranslation(offset + position)), radius, Color.MonoGameOrange);
            Draw3D.cube(find_bounding_box(), Color.MonoGameOrange);
        }
    }

    public class PointSphere : Shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => P;

        public shape_type shape { get; } = shape_type.point_sphere;

        public Vector3 P;

        public float radius { 
            get { return 0f; }
            set { fake_radius = value; }
        }
        public float fake_radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            return new BoundingBox(Vector3.Transform(position, orientation) - (Vector3.One * radius), Vector3.Transform(position, orientation) + (Vector3.One * radius));
        }

        public PointSphere(float radius) {
            P = Vector3.Zero;
            this.fake_radius = radius;
        }

        public void draw(Vector3 offset) {
            Draw3D.sphere(Vector3.Transform(Vector3.Zero, orientation * Matrix.CreateTranslation(offset + position)), fake_radius, Color.MonoGameOrange);
            Draw3D.cube(find_bounding_box(), Color.MonoGameOrange);
        }
    }
}
