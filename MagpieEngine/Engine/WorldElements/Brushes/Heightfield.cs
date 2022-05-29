using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Engine.Brushes {
    [Serializable]
    class Heightfield : Brush {
        public BrushType type => BrushType.HEIGHTFIELD;

        public Vector3 position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Matrix orientation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Matrix world { get; }

        public shape3D collision { get; set; }

        public IndexBuffer index_buffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public VertexBuffer vertex_buffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string texture { get; set; } = "zerocool_sharper";

        public Vector3 movement_vector { get; set; } = Vector3.Zero;
        public Vector3 final_position { get; set; }

        public void debug_draw() {
            throw new NotImplementedException();
        }

        public void Draw() {
            throw new NotImplementedException();
        }

        public void draw_depth(DynamicLight light) {
            throw new NotImplementedException();
        }

        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }

        public float get_footing_height(Vector3 pos) {
            throw new NotImplementedException();
        }

        public void Update() {
            throw new NotImplementedException();
        }

        public bool within_vertical_bounds(Vector3 pos) {
            throw new NotImplementedException();
        }
    }
}
