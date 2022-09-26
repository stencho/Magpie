using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Brushes {
    public enum BrushType {
        PLANE,
        TERRAIN,
        SEGMENTED_TERRAIN,
        BOX,
        DUMMY
    }

    public interface Brush {
        BrushType type { get; }

        Vector3 position { get; set; }
        Matrix orientation { get; set; }
        Matrix world { get; }
        string texture { get; set; }

        float get_footing_height(Vector3 pos);
        Vector3 get_footing(float X, float Z);
        bool within_vertical_bounds(Vector3 pos);

        BoundingBox bounds { get; }
        Shape3D collision { get; set; }

        float distance_to_camera { get; }

        Vector3 movement_vector { get; set; }
        Vector3 final_position { get; set; }

        SceneRenderInfo render_info { get; set; }

        void Update();
        void debug_draw();
    }
}
