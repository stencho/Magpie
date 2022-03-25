using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Graphics.Lights {
    public class Spotlight : DynamicLight {
        public int depth_map_resolution => 1024;

        RenderTarget2D _depth;
        public RenderTarget2D depth_map => _depth;

        public string shader => "spotlight";

        public float far_clip { get; set; } = 100f;
        public BoundingFrustum frustum { get; set; }

        public Vector3 position { get; set; } = (Vector3.Up * 0.5f);
        public Matrix orientation { get; set; } 

        public Matrix view { get; set; }
        public Matrix projection { get; set; }
               
        public Spotlight() {
            _depth = new RenderTarget2D(EngineState.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Vector4, DepthFormat.Depth24);

            //orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f));
            //orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(280));

            orientation = Matrix.Identity;

            view = orientation;
            projection = Matrix.CreatePerspectiveFieldOfView(1f, 1f, 0.1f, far_clip);

            frustum = new BoundingFrustum(view * projection);
        }

        public void build_depth_map() {
            
        }
    }
}
