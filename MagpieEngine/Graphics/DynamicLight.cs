using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public interface DynamicLight {
        Vector3 position { get; set; }
        Matrix orientation { get; set; }
        Matrix view { get; set; }
        Matrix projection { get; set; }

        int depth_map_resolution { get; }
        RenderTarget2D depth_map { get; }
        string shader { get; }
        float far_clip { get; set; }

        BoundingFrustum frustum { get; set; }

        void build_depth_map();
    }
}
