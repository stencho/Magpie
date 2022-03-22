using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;

namespace Magpie.Engine.Floors {
    public class FloorPlane : Floor {
        public Vector3 position { get; set; } = (Vector3.Backward * 20) + (Vector3.Up * 2f);
        public Vector2 size { get; set; } = Vector2.One * 50f;
        public Matrix orientation { get; set; } = Matrix.Identity;

        public void Draw() {
            Draw3D.fill_quad(EngineState.graphics_device, Matrix.CreateTranslation(position) * orientation,
                (Vector3.Forward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f),
                (Vector3.Forward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f),
                (Vector3.Backward * size.Y * 0.5f) + (Vector3.Right * size.X * 0.5f),
                (Vector3.Backward * size.Y * 0.5f) + (Vector3.Left * size.X * 0.5f),
                Color.White, EngineState.camera.view, EngineState.camera.projection);

        }

        public void Update() {

        }

        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }

        public float get_footing_height(float X, float Z) {
            return position.Y;
        }

        public bool within_vertical_bounds(Vector2 XZ) {
            if (XZ.X > position.X - (size.X * 0.5f) && XZ.X < position.X + (size.X * 0.5f) &&
                XZ.Y > position.Z - (size.Y * 0.5f) && XZ.Y < position.Z + (size.Y * 0.5f)) { 
                return true;
            } else { 
                return false;
            }
            
        }
    }
}
