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

        public void draw() {
            //Draw3D.capsule
        }
    }
}
