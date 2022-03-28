using Magpie;
using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace MagpieTestbed.TestObjects {
    [Serializable]
    class TestSphere : GameObject {
        public Vector3 position { get; set; } = Vector3.Up * 3f;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public Vector3 scale { get; set; } = Vector3.One;
        public Matrix world => Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position) ;

        public float radius = 1f;

        public BoundingBox bounds { get; set; }

        public string model { get; set; } = "cube";
        public string[] textures { get; set; } = new string[] { "zerocool_sharper" };

        public shape3D collision { get; set; }

        public TestSphere() {
             collision = new Cube(scale);
        }

        public void Update() {

            bounds = CollisionHelper.BoundingBox_around_OBB((Cube)collision, world);
        }
    }
}
