using Magpie;
using Magpie.Engine;
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
        public Vector3 position { get; set; } = Vector3.Zero;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public float radius = 1f;


        public void Draw() {
            Draw3D.sphere(EngineState.graphics_device, position, radius, Color.LightGreen, EngineState.camera.view, EngineState.camera.projection);
        }

        public void Update() {

        }
    }
}
