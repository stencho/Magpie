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
        public Vector3 position { get; set; } = Vector3.Up * 3f;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public float radius = 1f;


        public void Draw() {
            //Draw3D.sphere(EngineState.graphics_device, position, radius, Color.LightGreen, EngineState.camera.view, EngineState.camera.projection);
            Model mesh = ContentHandler.resources["cube"].value_gfx;
            foreach (ModelMesh mm in mesh.Meshes) {
                foreach (ModelMeshPart mp in mm.MeshParts) {
                    Draw3D.draw_buffers_diffuse_texture(EngineState.graphics_device, mp.VertexBuffer, mp.IndexBuffer, Draw3D.tum, Color.White, orientation * Matrix.CreateTranslation(position), EngineState.camera.view, EngineState.camera.projection);
                }
            }

            
        }
        
        public void draw_depth(DynamicLight light) {
            Model mesh = ContentHandler.resources["cube"].value_gfx;

            foreach (ModelMesh mm in mesh.Meshes) {
                foreach (ModelMeshPart mp in mm.MeshParts) {
                    Draw3D.draw_buffers_depth(light, orientation * Matrix.CreateTranslation(position), mp.VertexBuffer, mp.IndexBuffer);
                }
            }
        }

        public void Update() {

        }
    }
}
