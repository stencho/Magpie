using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Graphics.Lights {
    public class Spotlight : DynamicLight {
        public int depth_map_resolution => 2048;

        RenderTarget2D _depth;
        public RenderTarget2D depth_map => _depth;

        public string shader => "spotlight";

        public float far_clip { get; set; } = 60f;
        public BoundingFrustum frustum { get; set; }
        public Vector3 position { get; set; } = (Vector3.Up * 15.91909f) + (Vector3.Backward *9.921314f) + (Vector3.Right * 4f);
        public Matrix orientation { get; set; } 

        public Matrix view { get; set; }
        public Matrix projection { get; set; }
               
        public Spotlight() {
            _depth = new RenderTarget2D(EngineState.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Single, DepthFormat.Depth24);

            orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f));
            orientation *= Matrix.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(280));

            // orientation = Matrix.Identity;

            view = Matrix.CreateLookAt(position, (Vector3.Up * 2f + Vector3.Forward * 8), Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(40),1f, 0.2f, far_clip);
            //projection = Matrix.CreateOrthographic(depth_map_resolution/100, depth_map_resolution/100, 1f, far_clip);
            frustum = new BoundingFrustum(view * projection);
        }

        public void build_depth_map() {
            
        }
    }
}
