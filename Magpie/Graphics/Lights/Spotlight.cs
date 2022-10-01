using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Graphics.Lights {
    public class SpotLight : DynamicLight {
        public LightType type => LightType.SPOT;
        public int depth_map_resolution => 2048;

        RenderTarget2D _depth;
        public RenderTarget2D depth_map => _depth;

        public string shader => "spotlight";

        public float far_clip { get; set; } = 50;
        public float near_clip { get; set; } = 0.1f;

        public float fov { get; set; } = MathHelper.Pi / 4;

        public BoundingFrustum frustum { get; set; }
        public Vector3 position { get; set; } = (Vector3.Up * 15.91909f) + (Vector3.Forward *3.921314f);
        public Matrix orientation { get; set; } 

        public Matrix view { get; set; }
        public Matrix projection { get; set; }
        public Matrix world { get; set; }

        public Color light_color { get; set; } = Color.Wheat;

        public float radial_scale;
        public Matrix actual_scale;

        public float angle_cos => (float)Math.Cos(fov);

        public SpotLight() {
            _depth = new RenderTarget2D(EngineState.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);

            orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(90f));

            view = Matrix.CreateLookAt(position, position + orientation.Forward, orientation.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov) * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = actual_scale * orientation * Matrix.CreateTranslation(position);
            
            frustum = new BoundingFrustum(view * projection);
        }

        public void update() {

            view = Matrix.CreateLookAt(position, position + orientation.Forward, orientation.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov) * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = actual_scale * orientation * Matrix.CreateTranslation(position);

            frustum = new BoundingFrustum(view * projection);
        }
        
    }
}
