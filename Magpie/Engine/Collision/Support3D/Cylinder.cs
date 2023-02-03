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

        public Matrix orientation { get; set; }
        public Vector3 position { get; set; } = Vector3.Zero;

        public Vector3 start_point => A;

        public Matrix world => orientation * Matrix.CreateTranslation(position);

        public Vector3 A { get; set; } = Vector3.Zero;
        public Vector3 B { get; set; } = Vector3.One;

        public shape_type shape { get; } = shape_type.cylinder;

        public float radius { get; set; } = 1f;
        

        public BoundingBox find_bounding_box() {
            throw new NotImplementedException();
        }

        public void draw(Vector3 offset) {
            Matrix w = orientation * Matrix.CreateTranslation(offset + position);
            Draw3D.cylinder(Vector3.Transform(A, w), Vector3.Transform(B, w), radius, Color.MonoGameOrange);
            //Draw3D.cube(find_bounding_box(), Color.MonoGameOrange, EngineState.camera.view, EngineState.camera.projection);
            BoundingBox bb = find_bounding_box();

            //Draw3D.cube(origin + offset + position, (bb.Max - bb.Min)/2, Color.MonoGameOrange, Matrix.Identity);

            Draw3D.xyz_cross(A + offset + position, 1f, Color.LightPink);
            Draw3D.xyz_cross(B + offset + position, 1f, Color.HotPink);
        }
    }
}
