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
using Magpie.Engine.WorldElements.Brushes;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace Magpie.Graphics {
    public static class Renderer {
        static Effect e_gbuffer = ContentHandler.resources["fill_gbuffer"].value_fx;
        static Effect e_light_depth = ContentHandler.resources["light_depth"].value_fx;
        static Effect e_exp_light_depth = ContentHandler.resources["exp_light_depth"].value_fx;
        static Effect e_skybox = ContentHandler.resources["skybox"].value_fx;
        static Effect e_compositor = ContentHandler.resources["compositor"].value_fx;
        static Effect e_clear = ContentHandler.resources["clear"].value_fx;

        static Effect e_directionallight = ContentHandler.resources["directionallight"].value_fx;
        static Effect e_spotlight = ContentHandler.resources["spotlight"].value_fx;
        static Effect e_pointlight = ContentHandler.resources["pointlight"].value_fx;

        public static VerticalQuad quad;

        public class render_obj {
            public VertexBuffer vertex_buffer;
            public IndexBuffer index_buffer;
            
            public Matrix world;

            public Texture2D texture;
        }

        static volatile int visible_count = 0;
        static volatile int light_count = 0;

        static volatile render_obj[] visible = new render_obj[Map.max_actors + Map.max_objects];
        static volatile light[] visible_lights = new light[light.max_visible_lights];
        public static void init() {
            clear_visible();
        }

        static void clear_visible() {

            var term = 0;
            for (int i = 0; i < visible.Length; i++) {
                if (term == visible_count) break;
                if (visible[i] == null) continue;
                visible[i] = null;
                term++;
            }

            term = 0;
            for (int i = 0; i < light.max_visible_lights; i++) {
                if (term == light_count) break;
                if (visible_lights[i] == null) continue;
                visible_lights[i] = null;
                term++;
            }

            visible_count = 0;
            light_count = 0;

        }

        static void add_visible(int index, VertexBuffer vb, IndexBuffer ib, Matrix world, Texture2D tex) {
            visible_count++;
            for (int i = 0; i < visible.Length; i++) {
                if (visible[i] == null) {
                    visible[i] = new render_obj {
                        vertex_buffer = vb,
                        index_buffer = ib,
                        world = world,
                        texture = tex
                    };
                    break;
                }
            }
        }

        static void add_visible_to_spot_light(light l, int index, VertexBuffer vb, IndexBuffer ib, Matrix world, Texture2D tex) {
            l.spot_info.visible_count++;
            for (int i = 0; i < l.spot_info.visible.Length; i++) {
                if (l.spot_info.visible[i] == null) {
                    l.spot_info.visible[i] = new render_obj {
                        vertex_buffer = vb,
                        index_buffer = ib,
                        world = world,
                        texture = tex
                    };
                    break;
                }
            }
        }


        public static void create_spot_light_visibility_list(Map map, light l) {
            for (int i = 0; i < l.spot_info.visible.Length; i++) {  l.spot_info.visible[i] = null; }
            l.spot_info.visible_count = 0;

            int u = 0;
            
            for (int i = 0; i < Map.max_objects; i++) {
                if (u >= map.object_count) break;
                if (map.objects[i] == null) continue;

                //if (map.objects[i].bounds.Intersects(l.spot_info.bounds)) {

                    foreach (ModelMesh mm in ContentHandler.resources[map.objects[i].model].value_gfx.Meshes) {
                        foreach (ModelMeshPart mmp in mm.MeshParts) {
                            add_visible_to_spot_light(l, i,
                            mmp.VertexBuffer,
                            mmp.IndexBuffer,
                            map.objects[i].world,
                            ContentHandler.resources[map.objects[i].textures[0]].value_tx
                            );
                        }
                    }
                //}

                u++;
            }

            u = 0;
            for (int i = 0; i < Map.max_actors; i++) {
                if (u >= map.actor_count) break;
                if (map.actors[i] == null) continue;

                u++;
            }
        }

        public static void create_visibility_lists(Map map, Camera camera) {
            //OBJECTS
            int u = 0;
            for (int i = 0; i < Map.max_objects; i++) {
                if (u >= map.object_count) break;
                if (map.objects[i] == null) continue;
                
                var go = map.objects[i];
                if (go.lights != null) {
                    for (int il = 0; il < go.lights.Length; il++) {
                        if (go.lights[il] == null) continue;

                        if (go.lights[il].type == LightType.SPOT) {

                            update_spot_light(ref go.lights[il], camera);

                            if (go.lights[il].spot_info.bounds.Intersects(camera.frustum)) {
                                for (int v = 0; v < light.max_visible_lights; v++) {
                                    if (visible_lights[v] != null) continue;
                                    visible_lights[v] = go.lights[il];
                                    light_count += 1;
                                    break;
                                }
                            }
                        } else {

                            update_point_light(ref go.lights[il], camera);

                            for (int v = 0; v < light.max_visible_lights; v++) {
                                if (visible_lights[v] != null) continue;
                                visible_lights[v] = go.lights[il];
                                light_count += 1;
                                break;
                            }
                        }
                    }
                }

                if (!map.objects[i].bounds.Intersects(camera.frustum)) continue;

                foreach (ModelMesh mm in ContentHandler.resources[go.model].value_gfx.Meshes) {
                    foreach (ModelMeshPart mmp in mm.MeshParts) {
                        add_visible(i,
                            mmp.VertexBuffer,
                            mmp.IndexBuffer,
                            go.world,
                            ContentHandler.resources[go.textures[0]].value_tx
                            );
                    }
                }

                u++;
            }

            //ACTORS
            u = 0;
            for (int i = 0; i < Map.max_actors; i++) {
                if (u >= map.actor_count) continue;
                if (map.actors[i] == null) continue;

                var ac = map.actors[i];
                if (ac.lights != null) {
                    for (int il = 0; il < ac.lights.Length; il++) {
                        if (ac.lights[il] == null) continue;

                        if (ac.lights[il].type == LightType.SPOT) {
                            light l = ac.lights[il];

                            update_spot_light(ref ac.lights[il], camera);
                            
                            if (l.spot_info.bounds.Intersects(camera.frustum)) {
                                for (int v = 0; v < light.max_visible_lights; v++) {
                                    if (visible_lights[v] != null) continue;
                                    visible_lights[v] = ac.lights[il];
                                    light_count += 1;
                                    break;
                                }
                            }
                        } else {
                            update_point_light(ref ac.lights[il], camera);

                            for (int v = 0; v < light.max_visible_lights; v++) {
                                if (visible_lights[v] != null) continue;
                                visible_lights[v] = ac.lights[il];
                                light_count += 1;
                                break;
                            }
                        }
                    }
                }

                u++;
            }

            //PLAYER
            var pa = map.player_actor;

            //pa.lights[0].position = pa.position + (EngineState.camera.orientation.Right * 0.5f) + (EngineState.camera.orientation.Down * 0.4f) + (Vector3.Forward * 0.5f);
            //pa.lights[1].position = pa.position + (EngineState.camera.orientation.Right * 0.5f) + (EngineState.camera.orientation.Down * 0.4f) + (Vector3.Forward * 0.5f);

            //pa.lights[1].spot_info.orientation = EngineState.camera.orientation * Matrix.CreateFromAxisAngle(EngineState.camera.orientation.Up, MathHelper.ToRadians(5f));

            if (pa.lights != null) {
                for (int il = 0; il < pa.lights.Length; il++) {
                    if (pa.lights[il] == null) continue;

                    if (pa.lights[il].type == LightType.SPOT) {
                        light l = pa.lights[il];

                        update_spot_light(ref pa.lights[il], camera);

                        if (l.spot_info.bounds.Intersects(camera.frustum)) {
                            for (int v = 0; v < light.max_visible_lights; v++) {
                                if (visible_lights[v] != null) continue;
                                visible_lights[v] = pa.lights[il];
                                light_count += 1;
                                break;
                            }
                        }
                    } else {
                        update_point_light(ref pa.lights[il], camera);
                        for (int v = 0; v < light.max_visible_lights; v++) {
                            if (visible_lights[v] != null) continue;
                            visible_lights[v] = pa.lights[il];
                            light_count += 1;
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

        public static void draw_scene(Camera camera, GBuffer buffer) {
            EngineState.graphics_device.SetRenderTargets(buffer.buffer_targets);

            e_gbuffer.Parameters["atmosphere_color"].SetValue(Scene.sun_moon.atmosphere_color.ToVector3());
            e_gbuffer.Parameters["sky_color"].SetValue(Scene.sun_moon.sky_color.ToVector3());

            e_gbuffer.Parameters["FarClip"].SetValue(camera.far_clip);
            e_gbuffer.Parameters["camera_pos"].SetValue(camera.position);

            e_gbuffer.Parameters["View"].SetValue(camera.view);
            e_gbuffer.Parameters["Projection"].SetValue(camera.projection);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.NonPremultiplied;
            EngineState.graphics_device.BlendFactor = Color.Transparent;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            e_gbuffer.Parameters["fog"].SetValue(true);

            var term = 0;
            foreach(var v in visible) {
                if (term >= visible_count) break;
                if (v == null) continue;

                e_gbuffer.Parameters["World"].SetValue(v.world);
                e_gbuffer.Parameters["WVIT"].SetValue(Matrix.Transpose(Matrix.Invert(v.world * camera.view)));

                e_gbuffer.Parameters["DiffuseMap"].SetValue(v.texture);
                e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());

                EngineState.graphics_device.SetVertexBuffer(v.vertex_buffer);
                EngineState.graphics_device.Indices = v.index_buffer;

                e_gbuffer.CurrentTechnique.Passes[0].Apply();
                EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, v.vertex_buffer.VertexCount);

                term++;
            }

            Draw3D.sprite_line(Vector3.Up * 20, Vector3.Up * 50 + (Vector3.Right * 50), 0.1f, Color.Red);

            Draw3D.sprite_line(Vector3.Up * 50, Vector3.Up * 50 + (Vector3.Right * 50), 0.1f, Color.Red);

            Draw3D.sprite_line(Vector3.Up * 20, Vector3.Up * 50, 0.1f, Color.Red);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
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
            var term = 0;

            foreach (light light in visible_lights) {
                if (term >= light_count) break;
                if (light == null) continue;

                if (light.type == LightType.SPOT) {

                    create_spot_light_visibility_list(map, light);

                    EngineState.graphics_device.SetRenderTarget(light.spot_info.depth_map);
                    EngineState.graphics_device.BlendState = BlendState.Opaque;
                    EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                    EngineState.graphics_device.Clear(Color.Transparent);

                    e_exp_light_depth.Parameters["View"].SetValue(light.spot_info.view);
                    e_exp_light_depth.Parameters["Projection"].SetValue(light.spot_info.projection);

                    var iterm = 0;
                    foreach (var so in light.spot_info.visible) {
                        if (iterm >= light.spot_info.visible_count) break;
                        if (so == null) continue;
                        e_exp_light_depth.Parameters["World"].SetValue(so.world);
                        e_exp_light_depth.Parameters["LightPosition"].SetValue(light.position);
                        e_exp_light_depth.Parameters["LightDirection"].SetValue(light.spot_info.orientation.Forward);
                        e_exp_light_depth.Parameters["LightClip"].SetValue(light.spot_info.far_clip);
                        e_exp_light_depth.Parameters["C"].SetValue(light.spot_info.C);

                        e_exp_light_depth.Parameters["DiffuseMap"].SetValue(so.texture);

                        EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                        EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                        EngineState.graphics_device.Indices = so.index_buffer;

                        foreach (EffectTechnique tech in e_exp_light_depth.Techniques) {
                            foreach (EffectPass pass in tech.Passes) {
                                pass.Apply();
                                EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                            }
                        }

                        iterm++;

                    }


                } else if (light.type == LightType.POINT) {
                
                }

                term++;
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
            int term = 0;
            foreach(light light in visible_lights) {
                if (term >= light_count) break;
                if (light == null) continue;

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

                term++; 
            }

        }

        public static void compose() {

        }

        

        public static void render(Map map, Camera camera, GBuffer buffer) {
            Clock.frame_probe.set("RENDER");

            Clock.frame_probe.set("create_vis");
            clear_visible();            
            create_visibility_lists(map, camera);


            Clock.frame_probe.set("build_lighting");
            // update_lighting();
            build_lighting(map, camera, buffer);


            Clock.frame_probe.set("draw_scene");
            clear_to_skybox(camera, buffer); 
            draw_scene(camera, buffer);


            Clock.frame_probe.set("draw_lighting");
            draw_lighting(camera, buffer);

            Clock.frame_probe.set("END_RENDER");
            //compose(); //???????
        }


    }
}
