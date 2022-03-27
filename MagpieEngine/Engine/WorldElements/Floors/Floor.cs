using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Floors {
    public interface Floor {
        Vector3 position { get; set; }
        Matrix orientation { get; set; }
        Matrix world { get; }
        string texture { get; set; }

        float get_footing_height(Vector3 pos);
        Vector3 get_footing(float X, float Z);
        bool within_vertical_bounds(Vector3 pos);

        BoundingBox bounds { get; set; }

        IndexBuffer index_buffer { get; set; }
        VertexBuffer vertex_buffer { get; set; }

        void Update();
    }
}
