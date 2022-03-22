using Magpie.Engine;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagpieTestbed.TestObjects {
    [Serializable]
    class TestSphere : GameObject {
        public Vector3 position { get; set; } = Vector3.Zero;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public float radius = 1f;


        public void Draw(GraphicsDevice gd, Camera camera) {
            Draw3D.sphere(gd, position, radius, Color.LightGreen, camera.view, camera.projection);
        }

        public void Update() {

        }
    }
}
