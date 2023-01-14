using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Collision;
using Magpie.Engine.Stages;
using Magpie.Engine.WorldElements.Brushes;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Magpie.Graphics.Draw2D;

namespace Magpie.Graphics {
    public enum ObjectType {
        BRUSH,
        OBJECT,
        ACTOR
    }

    public struct SceneObject {
        public IndexBuffer index_buffer { get; set; }
        public VertexBuffer vertex_buffer { get; set; }

        public Matrix parent_world { get; set; }
        public Matrix world { get; set; }

        public BoundingBox mesh_bounds { get; set; }

        public string texture { get; set; }        
        public Color tint { get; set; }

        public Matrix light_wvp { get; set; }
        public Vector3 light_pos { get; set; }
        public float light_clip { get; set; }

        public bool in_light { get; set; }

        public bool wireframe { get; set; }

        public ObjectType object_type { get; set; }

        public float camera_distance { get; set; }
    }

    public class SceneRenderInfo {
        public IndexBuffer[] index_buffers { get; set; }
        public VertexBuffer[] vertex_buffers { get; set; }

        public bool[] draw_buffers { get; set; }

        public bool render { get; set; }
        public bool wireframe { get; set; }

        public string[] textures { get; set; }
        public string model { get; set; }

        public float camera_distance { get; set; }

        public Color tint { get; set; }

    }


    public class SunMoonSystem {
        public Color night_ambient = Color.FromNonPremultiplied(3,1,3, 255);

        public Color atmosphere_color = Color.FromNonPremultiplied(4, 4, 9, 255);

        public Color sky_color = Color.Lerp(Color.Purple, Color.LightSkyBlue, 0.2f);

        public Vector3 sun_direction => Vector3.Normalize((Vector3.Down * 5) + (Vector3.Down * 3) + Vector3.Forward);

        private Matrix sun_orientation = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(-75f)) * Matrix.CreateRotationZ(MathHelper.ToRadians(-15f));
        public Color current_color = Color.White;

        public GradientLineGenerator lerps;

        public double time_multiplier = 12f;

        //Directional light and distance fog info
        public float sun_max_brightness = 0.75f;
        public float sun_brightness_percent = 1.0f;

        public float moon_max_brightness = 0.2f;
        public float moon_brightness_percent = 0f;
        
        public double entire_day_cycle_length_ms = 60 * 10 * 1000;

        public double day_length_ratio = 0.5;
        public double night_length_ratio = 0.5;

        public double day_length => day_length_ratio * entire_day_cycle_length_ms;
        public double night_length => 1 - night_length_ratio * entire_day_cycle_length_ms;

        public double current_time_ms = 0;
        public double current_time_entire_day_percent => current_time_ms / entire_day_cycle_length_ms;

        public double current_day_value => current_time_ms / entire_day_cycle_length_ms;

        public bool time_stopped = true;

        public TimeSpan cycle_ts => new TimeSpan(0, 0, 0, 0, (int)entire_day_cycle_length_ms);
        public TimeSpan cycle_ts_scaled => new TimeSpan(0, 0, 0, 0, (int)(entire_day_cycle_length_ms / time_multiplier));

        public SunMoonSystem() {
            lerps = new GradientLineGenerator(night_ambient);

            lerps.add_lerp(night_ambient, .10f);

            //back down to orange just before dawn
            lerps.add_lerp(Color.FromNonPremultiplied(180, 130, 194, 255), .25f);

            //midday sky
            lerps.add_lerp(Color.FromNonPremultiplied(180, 175, 245, 255), .35f);

            lerps.add_lerp(Color.FromNonPremultiplied(200, 200, 255, 255), .55f);

            //back down to orange just before dusk
            lerps.add_lerp(Color.FromNonPremultiplied(220, 150, 165, 255), .75f);

            lerps.add_lerp(night_ambient, .9f);
            lerps.add_lerp(night_ambient, 1f);

            lerps.build_debug_band_texture();

            current_time_ms = entire_day_cycle_length_ms / 2f;
        }

        public void update() {
            //haven't maxed out the day yet
            if (current_time_ms <= entire_day_cycle_length_ms)
                current_time_ms += (!time_stopped ? Clock.frame_time_delta_ms : 0) * time_multiplier;
            //have maxed out day, subtract a day
            if (current_time_ms > entire_day_cycle_length_ms)
                current_time_ms -= entire_day_cycle_length_ms;

            if (current_time_ms < 0)
                current_time_ms = entire_day_cycle_length_ms - Math.Abs(current_time_ms);

            current_color = lerps.get_color_at((float)Scene.sun_moon.current_day_value);

            sky_color = Color.Lerp(Color.MidnightBlue, current_color, 0.9f) * 0.3f;
            atmosphere_color = Color.Lerp(Color.LightSkyBlue, current_color, .5f) * 0.75f;
        }

        public void set_time_of_day(double normalized_time) {
            current_time_ms = normalized_time * entire_day_cycle_length_ms;
        }


        public void configure_dlight_shader(Effect e_directionallight) {

            //e_directionallight.Parameters["fog"].SetValue(true);
            //e_directionallight.Parameters["fog_start"].SetValue(0.5f);

            //e_directionallight.Parameters["camera_pos"].SetValue(EngineState.camera.position);
            //e_directionallight.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);

            e_directionallight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
            //e_directionallight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);

            e_directionallight.Parameters["InverseView"].SetValue(Matrix.Invert(EngineState.camera.view));

            e_directionallight.Parameters["AtmosphereColor"].SetValue(lerps.get_color_at((float)Scene.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["AtmosphereIntensity"].SetValue(0.8f);

            e_directionallight.Parameters["LightColor"].SetValue(lerps.get_color_at((float)Scene.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["LightIntensity"].SetValue(1f);

            e_directionallight.Parameters["LightDirection"].SetValue(sun_direction);
            //e_directionallight.Parameters["camera_pos"].SetValue(EngineState.camera.position);

            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);           

        }

    }

    public class Scene {
        static Effect e_gbuffer = ContentHandler.resources["fill_gbuffer"].value_fx;
        static Effect e_light_depth = ContentHandler.resources["light_depth"].value_fx;
        static Effect e_exp_light_depth = ContentHandler.resources["exp_light_depth"].value_fx;
        static Effect e_skybox = ContentHandler.resources["skybox"].value_fx;
        static Effect e_compositor = ContentHandler.resources["compositor"].value_fx;
        static Effect e_clear = ContentHandler.resources["clear"].value_fx;

        static Effect e_directionallight = ContentHandler.resources["directionallight"].value_fx;
        static Effect e_spotlight = ContentHandler.resources["spotlight"].value_fx;
        static Effect e_pointlight = ContentHandler.resources["pointlight"].value_fx;

        public const int max_lights_per_object = 10;

        public static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
        public static VertexPositionNormalColorUv[] skybox_data;
        public static int[] skybox_indices;
        public static int skybox_face_res = 1024;
        public static RenderTarget2D skybox_cm;
        public static RenderTarget2D skybox_cm_e;


        static DateTime last_frame_draw_time = DateTime.Now;
        static TimeSpan time_since_last_draw = TimeSpan.Zero;
        static double frame_limit_goal_ms => 1000 / Clock.frame_limit;

        //skybox color scheme

        public static SunMoonSystem sun_moon = new SunMoonSystem();

        public static VerticalQuad quad;

        public static bool quantized_shading = false;

        public enum buffers {
            diffuse,
            normal,
            depth,
            lighting
        }

        public static byte buffer_count = 3;
        public static int buffer = -1;

        static bool _screenshot = false;
        public static void screenshot() { _screenshot = true; }
        public static bool taking_screenshot => _screenshot;
        public static int terrain_segments_rendered = 0;
        public static int vertex_buffer_draws = 0;

        static RasterizerState rs_wireframe = new RasterizerState() {
            CullMode = CullMode.None,
            FillMode = FillMode.WireFrame
        };


        public static void configure_renderer() {
            skybox_t.PrivateCreateSkyboxFromCrossImage(out skybox_data, out skybox_indices, 1, 0, 1, 2, 3, 5, 4);
            skybox_t.Subdivide(skybox_data, skybox_indices, out skybox_data, out skybox_indices, 16, MathHelper.Pi);
            skybox_cm = new RenderTarget2D(EngineState.graphics_device, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
            skybox_cm_e = new RenderTarget2D(EngineState.graphics_device, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

            EngineState.graphics_device.SetRenderTarget(skybox_cm);
            EngineState.graphics_device.Clear(sun_moon.atmosphere_color);

            EngineState.graphics_device.SetRenderTarget(null);
                                   
            quad = new VerticalQuad(EngineState.graphics_device);
        }
        

        public static void clear_buffer(Color color) {
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            e_clear.Parameters["color"].SetValue(color.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public static void draw_texture_to_screen(Texture2D tex) {
            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            e_gbuffer.Parameters["DiffuseMap"].SetValue(tex);
            e_gbuffer.Parameters["World"].SetValue(Matrix.Identity);
            e_gbuffer.Parameters["View"].SetValue(Matrix.Identity);
            e_gbuffer.Parameters["Projection"].SetValue(Matrix.Identity);
            e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_gbuffer.Parameters["clip_trans"].SetValue(false);
            e_gbuffer.Techniques["BasicColorDrawing"].Passes[0].Apply();
            

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            e_gbuffer.Parameters["clip_trans"].SetValue(true);
        }
        public static void draw_texture_to_screen(Texture2D tex, Vector2 pos, Vector2 size) {
            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            e_gbuffer.Parameters["DiffuseMap"].SetValue(tex);
            e_gbuffer.Parameters["World"].SetValue(Matrix.CreateScale(new Vector3(size / EngineState.resolution.ToVector2(), 1)) *
                Matrix.CreateTranslation(new Vector3(((pos - (EngineState.resolution.ToVector2() - (size ))) / EngineState.resolution.ToVector2()) * (Vector2.UnitX + (-Vector2.UnitY)), 0)));

            e_gbuffer.Parameters["clip_trans"].SetValue(false);

            e_gbuffer.Parameters["View"].SetValue(Matrix.Identity);
            e_gbuffer.Parameters["Projection"].SetValue(Matrix.Identity);
            e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_gbuffer.Techniques["BasicColorDrawing"].Passes[0].Apply();

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            e_gbuffer.Parameters["clip_trans"].SetValue(true);
        }

        public static void spritebatch_draw_to_screen(Vector2 offset, Texture2D tex) {
            EngineState.graphics_device.SetRenderTarget(null);
            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            EngineState.spritebatch.Draw(tex, offset, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            EngineState.spritebatch.End();
        }
        public static void spritebatch_draw_to_screen(Vector2 offset, Vector2 scale, Texture2D tex) {
            EngineState.graphics_device.SetRenderTarget(null);
            if (scale.X >= 1f)
                EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            else
                EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, null, null, null);

            EngineState.spritebatch.Draw(tex, offset, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            EngineState.spritebatch.End();
        }


        public static void compose() {
            Clock.frame_probe.set("compose");

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.BlendState = BlendState.Additive;

            /*
            if (FXAA) {
                gd.SetRenderTarget(graphics_buffer.rt_fxaa);
            } else {
                gd.SetRenderTarget(graphics_buffer.rt_final);
            }
            */

            EngineState.graphics_device.SetRenderTarget(EngineState.buffer.rt_final);

            clear_buffer(sun_moon.atmosphere_color);

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            //e_compositor.Parameters["fog"].SetValue(true);

            e_compositor.Parameters["DiffuseLayer"].SetValue(EngineState.buffer.rt_diffuse);
            e_compositor.Parameters["DepthLayer"].SetValue(EngineState.buffer.rt_depth);
            e_compositor.Parameters["LightLayer"].SetValue(EngineState.buffer.rt_lighting);
            e_compositor.Parameters["NormalLayer"].SetValue(EngineState.buffer.rt_normal);
            //e_compositor.Parameters["sky_brightness"].SetValue(sky_brightness);
            // e_compositor.Parameters["atmosphere_color"].SetValue(atmosphere_color.ToVector3());
            e_compositor.Parameters["buffer"].SetValue(buffer);

            e_compositor.Techniques["draw"].Passes[0].Apply();

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            /*
            if (FXAA) {
                draw_FXAA_to_final_buffer();

            }
            */


            //e_compositor.Parameters["fog"].SetValue(false);

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            spritebatch_draw_to_screen(Vector2.Zero, Vector2.One / gvars.get_float("super_resolution_scale"),  EngineState.buffer.rt_final);


            EngineState.graphics_device.SetRenderTarget(null);
            draw_texture_to_screen(EngineState.buffer.rt_2D);


            if (_screenshot) {
                if (!Directory.Exists("scr")) Directory.CreateDirectory("scr");

                using (FileStream fs = new FileStream("scr/scr" + DateTime.Now.ToFileTime() + ".png", FileMode.Create)) {
                    EngineState.buffer.rt_final.SaveAsPng(fs, EngineState.buffer.rt_final.Width, EngineState.buffer.rt_final.Height);

                }

                _screenshot = false;
                EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            }

            if (Clock.frame_limit > 0) {
                /*
                while (true) {
                    time_since_last_draw = DateTime.Now - last_frame_draw_time;
                    if (time_since_last_draw.TotalMilliseconds >= frame_limit_goal_ms) {
                        time_since_last_draw = TimeSpan.Zero;
                        break;
                    }
                }*/
            }
            last_frame_draw_time = DateTime.Now;

        }

    }
}
