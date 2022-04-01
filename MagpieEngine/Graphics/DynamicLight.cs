using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public enum LightType {
        SPOT,
        POINT
    }

    public class DynamicLightRequirements {
        public static BlendState blend_state = new BlendState {
            AlphaSourceBlend = Blend.One,
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One
        };
    }
    

    public interface DynamicLight {
        Vector3 position { get; set; }
        Matrix orientation { get; set; }
        Matrix view { get; set; }
        Matrix projection { get; set; }
        Matrix world { get; set; }

        Color light_color { get; set; }

        LightType type { get; }

        int depth_map_resolution { get; }
        RenderTarget2D depth_map { get; }
        string shader { get; }
        float far_clip { get; set; }
        float near_clip { get; set; }

        BoundingFrustum frustum { get; set; }
        
        void update();
    }
}
