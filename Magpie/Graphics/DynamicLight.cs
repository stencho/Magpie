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
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };
    }
    


    public struct light {
        LightType type;
        Vector3 position;
        Matrix world;
        Color color;
        float far_clip;
        float near_clip;
    }


    public interface DynamicLight {
        Vector3 position { get; set; }

        Matrix world { get; set; }

        Color light_color { get; set; }

        LightType type { get; }
        
        float far_clip { get;  }
        float near_clip { get; }
        

        void update();
    }
}
