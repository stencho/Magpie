using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision.Support3D {
    public class Sphere : shape3D {
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 position { get; set; } = Vector3.Zero;
        public Vector3 start_point => P;

        public shape_type shape { get; } = shape_type.sphere;

        public Vector3 P;

        public float radius { get; set; } = 0f;

        public BoundingBox find_bounding_box() {
            return new BoundingBox(position - (Vector3.One * radius), position + (Vector3.One * radius));
        }

        public Sphere() {
            P = Vector3.Zero;

            radius = 1.1f;
        }

        public void draw() {
            Draw3D.sphere(position, radius, Color.MonoGameOrange);
            Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);
        }
    }
}
