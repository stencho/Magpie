using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Stages;
using Magpie.Engine.WorldElements;
using Magpie.Engine.WorldElements.Brushes;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Color = Microsoft.Xna.Framework.Color;

namespace Magpie.Graphics {
    public static class Renderer {
        public static Effect e_gbuffer = ContentHandler.resources["fill_gbuffer"].value_fx;
        public static Effect e_light_depth = ContentHandler.resources["light_depth"].value_fx;
        public static Effect e_exp_light_depth = ContentHandler.resources["exp_light_depth"].value_fx;
        public static Effect e_skybox = ContentHandler.resources["skybox"].value_fx;
        public static Effect e_compositor = ContentHandler.resources["compositor"].value_fx;
        public static Effect e_clear = ContentHandler.resources["clear"].value_fx;

        public static Effect e_directionallight = ContentHandler.resources["directionallight"].value_fx;
        public static Effect e_spotlight = ContentHandler.resources["spotlight"].value_fx;
        public static Effect e_pointlight = ContentHandler.resources["pointlight"].value_fx;

        public static VerticalQuad quad;

        static volatile List<light> visible_lights = new List<light>();
        static volatile List<int> visible = new List<int>();

        public class render_obj {
            public VertexBuffer vertex_buffer;
            public IndexBuffer index_buffer;

            public Matrix world;

            public Texture2D texture;
        }

        public static void init() {
        }

        public static void create_visibility_lists(Map map, Camera camera) {
            visible_lights.Clear();
            visible.Clear();



            foreach (var o in map.game_objects) {
               // if (o.Value.in_frustum(camera.frustum)) {
                    visible.Add(o.Key);
               // }

                if (o.Value.lights != null) {
                    for (int i = 0; i < o.Value.lights.Length; i++) {
                        light l = o.Value.lights[i];
                        switch (l.type) {
                            case LightType.SPOT:
                                update_spot_light(ref o.Value.lights[i], camera);

                                if (l.spot_info.bounds.Intersects(camera.frustum)) {
                                    visible_lights.Add(l);

                                    l.spot_info.visible.Clear();
                                    foreach (int k in map.game_objects.Keys) {
                                        if (l.spot_info.bounds.Intersects(camera.frustum)) {
                                            l.spot_info.visible.Add(k);
                                        }
                                    }
                                }
                                break;
                            case LightType.POINT:
                                update_point_light(ref o.Value.lights[i], camera);

                                if (camera.frustum.Intersects(l.point_info.bounds)) {
                                    visible_lights.Add(l);
                                }
                                break;
                        }
                    }
                }
            }

        }


        public static void clear_to_skybox(Camera camera, GBuffer buffer) {
            if (quad == null) { quad = new VerticalQuad(EngineState.graphics_device); }

            EngineState.graphics_device.DepthStencilState = DepthStencilState.None;

            EngineState.graphics_device.SetRenderTargets(buffer.buffer_targets);
            e_clear.Parameters["color"].SetValue(Scene.sun_moon.atmosphere_color.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();


            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            e_skybox.Parameters["atmosphere_color"].SetValue(Scene.sun_moon.atmosphere_color.ToVector4());
            e_skybox.Parameters["sky_color"].SetValue(Scene.sun_moon.sky_color.ToVector4());

            e_skybox.Parameters["World"].SetValue(Matrix.CreateScale(1f) * Matrix.Identity);
            e_skybox.Parameters["View"].SetValue(Matrix.CreateLookAt(Vector3.Zero, camera.direction, camera.up_direction));
            e_skybox.Parameters["Projection"].SetValue(camera.projection);

            e_skybox.Techniques["draw"].Passes[0].Apply();

            EngineState.graphics_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, Scene.skybox_data, 0, 2, Scene.skybox_indices, 0, Scene.skybox_indices.Length / 3, VertexPositionNormalColorUv.VertexDeclaration);

            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;
        }

        public static void draw_scene(Map map, Camera camera, GBuffer buffer) {

            EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);

            e_gbuffer.Parameters["atmosphere_color"].SetValue(Scene.sun_moon.atmosphere_color.ToVector3());
            e_gbuffer.Parameters["sky_color"].SetValue(Scene.sun_moon.sky_color.ToVector3());

            e_gbuffer.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);
            e_gbuffer.Parameters["camera_pos"].SetValue(EngineState.camera.position);

            e_gbuffer.Parameters["View"].SetValue(EngineState.camera.view);
            e_gbuffer.Parameters["Projection"].SetValue(EngineState.camera.projection);

            foreach (var o in visible) {
                EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                EngineState.graphics_device.BlendState = BlendState.NonPremultiplied;
                EngineState.graphics_device.BlendFactor = Color.Transparent;
                EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                e_gbuffer.Parameters["fog"].SetValue(true);

                map.game_objects[o].draw();

                Renderer.e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());
                Renderer.e_gbuffer.Parameters["fog"].SetValue(false);

            }

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;
            e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_gbuffer.Parameters["fog"].SetValue(false);


        }

        public static void update_point_light(ref light l, Camera camera) {
            l.world = Matrix.CreateScale(l.point_info.radius) * Matrix.CreateTranslation(l.point_info.position);
        }

        public static void update_spot_light(ref light l, Camera camera) {
            spot_info si = l.spot_info;

            si.view
                = Matrix.CreateLookAt(l.position, l.position + si.orientation.Forward, si.orientation.Up);
            si.projection
                = Matrix.CreatePerspectiveFieldOfView(si.fov, 1f, si.near_clip, si.far_clip);

            si.radial_scale = (float)Math.Tan((double)si.fov) * si.far_clip;

            si.actual_scale = Matrix.CreateScale(si.radial_scale, si.radial_scale, si.far_clip);


            si.bounds = new BoundingFrustum(si.view * si.projection);

            l.spot_info = si;
            l.world = si.actual_scale * si.orientation * Matrix.CreateTranslation(si.position);
        }

        public static void build_lighting(Map map, Camera camera, GBuffer buffer) {
            EngineState.graphics_device.SetRenderTarget(buffer.rt_lighting);
            EngineState.graphics_device.Clear(Scene.sun_moon.atmosphere_color);

            foreach (light light in visible_lights) {
                if (light.type == LightType.SPOT) {

                    EngineState.graphics_device.SetRenderTarget(light.spot_info.depth_map);

                    EngineState.graphics_device.BlendState = BlendState.Opaque;
                    EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                    EngineState.graphics_device.Clear(Color.Transparent);

                    e_exp_light_depth.Parameters["View"].SetValue(light.spot_info.view);
                    e_exp_light_depth.Parameters["Projection"].SetValue(light.spot_info.projection);
                    //create_spot_light_visibility_list(map, light);

                    e_exp_light_depth.Parameters["LightPosition"].SetValue(light.position);
                    e_exp_light_depth.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                    e_exp_light_depth.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                    e_exp_light_depth.Parameters["C"].SetValue(light.spot_info.C);

                    foreach (int i in light.spot_info.visible) {
                        map.game_objects[i].draw_to_light(light);
                    }


                } else if (light.type == LightType.POINT) {
                }
            }
        }

        public static void draw_lighting(Camera camera, GBuffer buffer) {
            EngineState.graphics_device.SetRenderTarget(buffer.rt_lighting);

            e_pointlight.Parameters["View"].SetValue(camera.view);
            e_pointlight.Parameters["Projection"].SetValue(camera.projection);
            e_pointlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
            e_pointlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

            e_spotlight.Parameters["View"].SetValue(camera.view);
            e_spotlight.Parameters["Projection"].SetValue(camera.projection);
            e_spotlight.Parameters["InverseView"].SetValue(Matrix.Invert(camera.view));
            e_spotlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(camera.view * camera.projection));

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            Scene.sun_moon.configure_dlight_shader(e_directionallight);

            EngineState.graphics_device.BlendState = DynamicLightRequirements.blend_state;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;
            foreach(light light in visible_lights) {
                if (light.type == LightType.SPOT) {
                    e_spotlight.Parameters["World"].SetValue(light.world);

                    e_spotlight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
                    e_spotlight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);
                    e_spotlight.Parameters["COOKIE"].SetValue(light.spot_info.cookie);
                    e_spotlight.Parameters["SHADOW"].SetValue(light.spot_info.depth_map);

                    e_spotlight.Parameters["LightViewProjection"].SetValue(light.spot_info.view * light.spot_info.projection);
                    e_spotlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                    e_spotlight.Parameters["LightPosition"].SetValue(light.position);
                    e_spotlight.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                    e_spotlight.Parameters["LightAngleCos"].SetValue(light.spot_info.angle_cos);
                    e_spotlight.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                    e_spotlight.Parameters["DepthBias"].SetValue(light.spot_info.bias);
                    e_spotlight.Parameters["C"].SetValue(light.spot_info.C);

                    e_spotlight.Parameters["Shadows"].SetValue(light.spot_info.shadows);

                    EngineState.graphics_device.SetVertexBuffer(ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer);
                    EngineState.graphics_device.Indices = ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].IndexBuffer;

                    float SL = Math.Abs(Vector3.Dot(Vector3.Normalize(light.position - camera.position), light.spot_info.orientation.Forward));

                    if (SL <= (light.spot_info.angle_cos)) {
                        EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                    } else {
                        EngineState.graphics_device.RasterizerState = RasterizerState.CullClockwise;
                    }

                    e_spotlight.CurrentTechnique.Passes[0].Apply();
                    EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer.VertexCount);


                } else if (light.type == LightType.POINT) {
                    e_pointlight.Parameters["World"].SetValue(
                    Matrix.CreateScale(light.point_info.radius) * Matrix.CreateTranslation(light.point_info.position));

                    e_pointlight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
                    e_pointlight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);

                    e_pointlight.Parameters["LightColor"].SetValue(light.color.ToVector4());
                    e_pointlight.Parameters["LightPosition"].SetValue(light.position);
                    e_pointlight.Parameters["LightIntensity"].SetValue(1f);
                    e_pointlight.Parameters["LightRadius"].SetValue(light.point_info.radius);

                    e_pointlight.Parameters["Shadows"].SetValue(false);
                    e_pointlight.Parameters["quantized"].SetValue(light.point_info.quantize);

                    EngineState.graphics_device.SetVertexBuffer(ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer);
                    EngineState.graphics_device.Indices = ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].IndexBuffer;

                    Vector3 sdiff = (camera.position) - light.position;
                    float skyCameraToLight = (float)Math.Sqrt((float)Vector3.Dot(sdiff, sdiff)) / 100.0f;

                    if (skyCameraToLight <= light.point_info.radius) {
                        EngineState.graphics_device.RasterizerState = RasterizerState.CullClockwise;
                    } else {
                        EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                    }

                    e_pointlight.CurrentTechnique.Passes[0].Apply();
                    EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer.VertexCount);
                }
            }

        }

        public static void compose() {

        }

        

        public static void render(Map map, Camera camera, GBuffer buffer) {
            Clock.frame_probe.set("RENDER");

            Clock.frame_probe.set("create_vis");
            //clear_visible();            
            create_visibility_lists(map, camera);


            Clock.frame_probe.set("build_lighting");
            // update_lighting();
            build_lighting(map, camera, buffer);


            Clock.frame_probe.set("draw_scene");
            clear_to_skybox(camera, buffer);


            draw_scene(map, camera, buffer);

            Clock.frame_probe.set("draw_lighting");
            draw_lighting(camera, buffer);

            Clock.frame_probe.set("END_RENDER");
            //compose(); //???????
        }


    }
}
