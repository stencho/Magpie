using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Graphics.Lights {
    public class Spotlight : DynamicLight {
        public LightType type => LightType.SPOT;
        public int depth_map_resolution => 2048;

        RenderTarget2D _depth;
        public RenderTarget2D depth_map => _depth;

        public string shader => "spotlight";

        public float far_clip { get; set; } = 50f;
        public float near_clip { get; set; } = 0.2f;

        float fov = MathHelper.PiOver2;

        public BoundingFrustum frustum { get; set; }
        public Vector3 position { get; set; } = (Vector3.Up * 15.91909f) + (Vector3.Backward *9.921314f);
        public Matrix orientation { get; set; } 

        public Matrix view { get; set; }
        public Matrix projection { get; set; }
        public Matrix world { get; set; }

        public Color light_color { get; set; } = Color.Purple;

        float radial_scale;
        Matrix actual_scale;


        public Spotlight() {
            _depth = new RenderTarget2D(EngineState.graphics_device, depth_map_resolution, depth_map_resolution, false, SurfaceFormat.Single, DepthFormat.Depth24);

            orientation = Matrix.CreateFromAxisAngle(Vector3.Left, MathHelper.ToRadians(45f));

            view = Matrix.CreateLookAt(position, position + orientation.Forward, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov / 2.0) * 2f * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = Matrix.CreateTranslation(Vector3.Forward) * actual_scale * orientation * Matrix.CreateTranslation(position);

            frustum = new BoundingFrustum(view * projection);
        }

        public void update() {
            view = Matrix.CreateLookAt(position, position + orientation.Forward, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(fov, 1f, near_clip, far_clip);

            radial_scale = (float)Math.Tan((double)fov / 2.0) * 2f * far_clip;
            actual_scale = Matrix.CreateScale(radial_scale, radial_scale, far_clip);

            world = Matrix.CreateTranslation(Vector3.Forward) * actual_scale * orientation * Matrix.CreateTranslation(position);

            frustum = new BoundingFrustum(view * projection);
        }
        
    }
}
