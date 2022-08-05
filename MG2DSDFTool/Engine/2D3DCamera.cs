using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2DSDFTool {
    public class Camera {
        public Vector3 position;

        public Matrix view;        
        public Matrix projection;

        Matrix orientation;


        public Camera(Vector3 pos) {
            position = pos;

            view = Matrix.CreateOrthographic(Engine.resolution.X, Engine.resolution.Y, 0.01f, 10);
            orientation = Matrix.CreateLookAt(position, position + orientation.Forward, Vector3.Up);

        }

    }
}
