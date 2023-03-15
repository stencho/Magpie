using Magpie.Engine.Collision;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    class Cylinder : Shape3D {
        //static vert buffer goes here and gets built in ContentManager on startup then placed here

        public Vector3 start_point => A; 
        public Vector3 center => (A + B) / 2f;

        public Vector3 A { get; set; } = Vector3.Zero;
        public Vector3 B { get; set; } = Vector3.One;

        public shape_type shape { get; } = shape_type.cylinder;

        public float radius { get; set; } = 1f;


        public BoundingBox sweep_bounding_box(Matrix world, Vector3 sweep) {
            throw new NotImplementedException();
        }

        public BoundingBox find_bounding_box(Matrix world) {
            throw new NotImplementedException();
        }

        public void draw(Matrix world) {
            Draw3D.cylinder(Vector3.Transform(A, world), Vector3.Transform(B, world), radius, Color.MonoGameOrange);
            //Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);

            Draw3D.cube(find_bounding_box(world), Color.Red);

            Draw3D.xyz_cross(Vector3.Transform(A, world), 1f, Color.LightPink);
            Draw3D.xyz_cross(Vector3.Transform(B, world), 1f, Color.HotPink);
        }
        public Vector3 support(Vector3 direction, Vector3 sweep) {
            throw new NotImplementedException();
        }
    }
}
