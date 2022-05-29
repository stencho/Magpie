using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Graphics.Lights;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Graphics.Draw2D;

namespace Magpie.Graphics {
    public struct SceneObject {
        public IndexBuffer index_buffer { get; set; }
        public VertexBuffer vertex_buffer { get; set; }

        public Matrix parent_world { get; set; }
        public Matrix world { get; set; }

        public BoundingBox mesh_bounds { get; set; }

        public string texture { get; set; }        

        public List<Texture2D> shadow_maps { get; set; }
        public Matrix light_wvp { get; set; }
        public Vector3 light_pos { get; set; }
        public float light_clip { get; set; }

        public bool in_light { get; set; }
        public List<LightInfo> lights { get; set; }
    }
    public struct LightInfo {
        public Texture2D shadow_map { get; set; }
        public Matrix LightWVP { get; set; }
        public Vector3 LightPosition { get; set; }
        public Vector3 LightDirection { get; set; }
    }

    public class SunMoonSystem {
        public Color night_ambient = Color.FromNonPremultiplied(3,1,3, 255);

        public Color atmosphere_color = Color.FromNonPremultiplied(4, 4, 9, 255);

        public Color sky_color = Color.Lerp(Color.Purple, Color.LightSkyBlue, 0.2f);

        public Vector3 sun_direction => sun_orientation.Forward;
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

            sky_color = Color.Lerp(Color.MidnightBlue, current_color, 0.7f) * 0.3f;
            atmosphere_color = Color.Lerp(Color.LightSkyBlue, current_color, .5f) * 0.75f;
        }

        public void set_time_of_day(double normalized_time) {
            current_time_ms = normalized_time * entire_day_cycle_length_ms;
        }


        public void configure_dlight_shader(Effect e_directionallight) {

            e_directionallight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
            e_directionallight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);

            e_directionallight.Parameters["InverseView"].SetValue(Matrix.Invert(EngineState.camera.view));

            e_directionallight.Parameters["LightColor"].SetValue(lerps.get_color_at((float)Scene.sun_moon.current_day_value).ToVector3());
            e_directionallight.Parameters["LightIntensity"].SetValue(1f);

            e_directionallight.Parameters["LightDirection"].SetValue(sun_direction);
            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);

            /*
            e_directionallight.Parameters["LightDirection"].SetValue((sun_orientation * Matrix.CreateRotationY(MathHelper.ToRadians(90))).Forward);
            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);

            e_directionallight.Parameters["LightDirection"].SetValue((sun_orientation * Matrix.CreateRotationY(MathHelper.ToRadians(-90))).Forward);
            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);

            e_directionallight.Parameters["LightDirection"].SetValue((sun_orientation * Matrix.CreateRotationY(MathHelper.ToRadians(180))).Forward);
            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);
            */

            /*
            //sun
            e_directionallight.Parameters["LightColor"].SetValue(Color.LightGoldenrodYellow.ToVector3());
            e_directionallight.Parameters["LightDirection"].SetValue(Vector3.Down);
            e_directionallight.Parameters["LightIntensity"].SetValue(0.3f);

            e_directionallight.CurrentTechnique.Passes[0].Apply();
            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Scene.quad.vertex_buffer.VertexCount);
            */

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

        static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
        static VertexPositionNormalColorUv[] skybox_data;
        static int[] skybox_indices;
        static int skybox_face_res = 1024;
        static RenderTarget2D skybox_cm;
        static RenderTarget2D skybox_cm_e;

        //skybox color scheme

        public static SunMoonSystem sun_moon = new SunMoonSystem();

        public static VerticalQuad quad;

        public static bool quantized = false;

        public enum buffers {
            diffuse,
            normal,
            depth,
            lighting
        }
        static byte buffer_count = 3;
        public static int buffer = -1;

        static bool screenshot = false;
        public static void screenshot_at_end_of_frame() { screenshot = true; }
        public static bool taking_screenshot => screenshot;

        public static SceneObject[] create_scene_from_lists(Dictionary<string,Brush> floors, Dictionary<string, GameObject> objects, Dictionary<string, Actor> actors, IEnumerable<DynamicLight> lights, BoundingFrustum view_frustum) {
            List<SceneObject> scene = new List<SceneObject>(floors.Count + objects.Count + actors.Count);
            bool any_visible_light_frustum = false;

            foreach (Brush floor in floors.Values) {
                //RE-ADD LIGHT/FRUSTUM CHECKS

                //if (floor.bounds.Intersects(view_frustum) || any_visible_light_frustum) {
                if (floor.type == BrushType.PLANE) {
                    scene.Add(new SceneObject {
                        vertex_buffer = ((FloorPlane)floor).vertex_buffer,
                        index_buffer = ((FloorPlane)floor).index_buffer,
                        mesh_bounds = floor.collision.find_bounding_box(),
                        world = floor.world,
                        texture = floor.texture,
                        in_light = false,
                        shadow_maps = new List<Texture2D>()
                    });
                } else if (floor.type == BrushType.SEGMENTED_HEIGHTFIELD) {
                    //do frustum test here to see which segments are in view and get the correct LOD buffer
                    //then add a new scene object for each of the segments added, easy

                    //for (int y = 0; y < segment count Y; y++) {
                    //  for (int x = 0; x < segment count X; x++) 
                    //      scene.Add(new SceneObject {

                    //      });
                    //  }
                    //}

                    //may be more efficient to use brain and or maths to only do the frustum check with segments that
                    //are both in front of the camera (easy) and in a cone within the front of the camera (less easy)
                }

                //}
            }
            foreach (GameObject go in objects.Values) {
                //RE-ADD LIGHT/FRUSTUM CHECKS

                //if (go.bounds.Intersects(view_frustum) || any_visible_light_frustum) {

                    int texture_index = 0;
                    foreach (ModelMesh mm in ContentHandler.resources[go.model].value_gfx.Meshes) {
                        foreach (ModelMeshPart mmp in mm.MeshParts) {
                            scene.Add(new SceneObject {
                                vertex_buffer = mmp.VertexBuffer,
                                index_buffer = mmp.IndexBuffer,
                                world = go.world,
                                mesh_bounds = go.bounds,
                                texture = go.textures[texture_index],
                                in_light = false,
                                shadow_maps = new List<Texture2D>()                               
                                
                            });
                        }
                        texture_index++;
                    }
                //}
            }

            foreach (Actor actor in actors.Values) {

            }

            //scene.OrderBy((a) => Vector3.Distance(view_frustum.Matrix.Translation, a.world.Translation));

            return scene.ToArray();
        }

        public static void build_lighting(IEnumerable<DynamicLight> lights, SceneObject[] scene) {

            EngineState.graphics_device.SetRenderTarget(EngineState.buffer.rt_lighting);
            EngineState.graphics_device.Clear(sun_moon.atmosphere_color);

            foreach (DynamicLight light in lights) {
                if (light.type == LightType.SPOT) {
                    EngineState.graphics_device.SetRenderTarget(((SpotLight)light).depth_map);
                    EngineState.graphics_device.BlendState = BlendState.Opaque;
                    EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                    EngineState.graphics_device.Clear(Color.Transparent);

                    e_exp_light_depth.Parameters["View"].SetValue(((SpotLight)light).view);
                    e_exp_light_depth.Parameters["Projection"].SetValue(((SpotLight)light).projection);

                    //e_exp_light_depth.Parameters["LVP"].SetValue(((SpotLight)light).view * ((SpotLight)light).projection);

                    for (int i = 0; i < scene.Length; i++) {
                        SceneObject so = scene[i];

                        if (((SpotLight)light).frustum.Intersects(so.mesh_bounds) && ((SpotLight)light).frustum.Intersects(EngineState.camera.frustum)) {
                            scene[i].in_light = true;

                            e_exp_light_depth.Parameters["World"].SetValue(so.world);
                            e_exp_light_depth.Parameters["LightPosition"].SetValue(light.position);
                            e_exp_light_depth.Parameters["DepthPrecision"].SetValue(((SpotLight)light).far_clip);

                            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                            EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                            EngineState.graphics_device.Indices = so.index_buffer;

                            foreach (EffectTechnique tech in e_exp_light_depth.Techniques) {
                                foreach (EffectPass pass in tech.Passes) {
                                    pass.Apply();
                                    EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                                }
                            }

                            scene[i].light_wvp = so.world * ((SpotLight)light).view * ((SpotLight)light).projection;
                            scene[i].light_clip = light.far_clip;
                            scene[i].light_pos = light.position;
                            so.shadow_maps.Add(((SpotLight)light).depth_map);
                        }
                    }
                } else if (light.type == LightType.POINT) {
                    //build lighting cubemap
                    //oh boy might just never do this
                }
            }
        }

        public static void draw(IEnumerable<SceneObject> scene) {
            EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);
            
            //e_lit_diffuse.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);
            e_gbuffer.Parameters["View"].SetValue(EngineState.camera.view);
            e_gbuffer.Parameters["Projection"].SetValue(EngineState.camera.projection);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            foreach (SceneObject so in scene) {
                e_gbuffer.Parameters["World"].SetValue(so.world);
                e_gbuffer.Parameters["WVIT"].SetValue(Matrix.Transpose(Matrix.Invert(so.world * EngineState.camera.view)));

                e_gbuffer.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[so.texture].value_tx);
                //e_gbuffer.Parameters["ambient_light"].SetValue(Color.White.ToVector3());
                
                EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                EngineState.graphics_device.Indices = so.index_buffer;

                e_gbuffer.CurrentTechnique.Passes[0].Apply();
                EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                    
            }
        }

        public static void draw_lighting(IEnumerable<DynamicLight> lights) {

            EngineState.graphics_device.SetRenderTarget(EngineState.buffer.rt_lighting);
            
            e_pointlight.Parameters["View"].SetValue(EngineState.camera.view);
            e_pointlight.Parameters["Projection"].SetValue(EngineState.camera.projection);
            e_pointlight.Parameters["InverseView"].SetValue(Matrix.Invert(EngineState.camera.view));
            e_pointlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(EngineState.camera.view * EngineState.camera.projection));

            e_spotlight.Parameters["View"].SetValue(EngineState.camera.view);
            e_spotlight.Parameters["Projection"].SetValue(EngineState.camera.projection);
            e_spotlight.Parameters["InverseView"].SetValue(Matrix.Invert(EngineState.camera.view));
            e_spotlight.Parameters["InverseViewProjection"].SetValue(Matrix.Invert(EngineState.camera.view * EngineState.camera.projection));

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.None;

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            sun_moon.configure_dlight_shader(e_directionallight);



            EngineState.graphics_device.BlendState = DynamicLightRequirements.blend_state;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;



            foreach (DynamicLight light in lights) {
                switch (light.type) {
                    case LightType.SPOT:
                        
                        e_spotlight.Parameters["World"].SetValue(light.world);

                        e_spotlight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
                        e_spotlight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);
                        e_spotlight.Parameters["COOKIE"].SetValue(ContentHandler.resources["radial_glow"].value_tx);
                        e_spotlight.Parameters["SHADOW"].SetValue(((SpotLight)light).depth_map);

                        e_spotlight.Parameters["LightViewProjection"].SetValue(((SpotLight)light).view * ((SpotLight)light).projection);
                        e_spotlight.Parameters["LightColor"].SetValue(light.light_color.ToVector4());
                        e_spotlight.Parameters["LightPosition"].SetValue(light.position);
                        e_spotlight.Parameters["LightDirection"].SetValue(((SpotLight)light).orientation.Forward);
                        e_spotlight.Parameters["LightAngleCos"].SetValue(((SpotLight)light).angle_cos);
                        e_spotlight.Parameters["LightClip"].SetValue(((SpotLight)light).far_clip);
                        e_spotlight.Parameters["DepthBias"].SetValue(0.0002f);
                        e_spotlight.Parameters["shadowMapSize"].SetValue((float)((SpotLight)light).depth_map_resolution);

                        e_spotlight.Parameters["Shadows"].SetValue(true);

                        e_spotlight.Parameters["GBufferTextureSize"].SetValue(EngineState.resolution.ToVector2());

                        EngineState.graphics_device.SetVertexBuffer(ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer);
                        EngineState.graphics_device.Indices = ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].IndexBuffer;

                        float SL = Math.Abs(Vector3.Dot(Vector3.Normalize(light.position - EngineState.camera.position), ((SpotLight)light).orientation.Forward));

                        if (SL < ((SpotLight)light).angle_cos) {
                            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                        } else {
                            EngineState.graphics_device.RasterizerState = RasterizerState.CullClockwise;
                        }

                        e_spotlight.CurrentTechnique.Passes[0].Apply();
                        EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ContentHandler.resources["cone"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer.VertexCount);
                                                
                        break;

                    case LightType.POINT:
                        e_pointlight.Parameters["World"].SetValue(light.world);

                        e_pointlight.Parameters["NORMAL"].SetValue(EngineState.buffer.rt_normal);
                        EngineState.graphics_device.SamplerStates[0] = SamplerState.LinearClamp;

                        e_pointlight.Parameters["DEPTH"].SetValue(EngineState.buffer.rt_depth);
                        EngineState.graphics_device.SamplerStates[1] = SamplerState.LinearClamp;

                        //e_pointlight.Parameters["CameraPosition"].SetValue(EngineState.camera.position);
                        e_pointlight.Parameters["LightColor"].SetValue(light.light_color.ToVector4());
                        e_pointlight.Parameters["LightPosition"].SetValue(light.position);
                        e_pointlight.Parameters["LightIntensity"].SetValue(1f);
                        e_pointlight.Parameters["LightRadius"].SetValue(((PointLight)light).radius);

                        e_pointlight.Parameters["Shadows"].SetValue(false);
                        e_pointlight.Parameters["quantized"].SetValue(quantized);

                        e_pointlight.Parameters["GBufferTextureSize"].SetValue(EngineState.resolution.ToVector2());


                        EngineState.graphics_device.SetVertexBuffer(ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer);
                        EngineState.graphics_device.Indices = ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].IndexBuffer;

                        Vector3 sdiff = (EngineState.camera.position) - light.position;
                        float skyCameraToLight = (float)Math.Sqrt((float)Vector3.Dot(sdiff, sdiff)) / 100.0f;

                        if (skyCameraToLight <= ((PointLight)light).radius) {
                            EngineState.graphics_device.RasterizerState = RasterizerState.CullClockwise;
                        } else {
                            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
                        }

                        e_pointlight.CurrentTechnique.Passes[0].Apply();
                        EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, ContentHandler.resources["sphere"].value_gfx.Meshes[0].MeshParts[0].VertexBuffer.VertexCount);
                            
                        
                        break;
                }
            }
        }


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
        
        public static void clear_buffer() {
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            e_clear.Parameters["color"].SetValue(Color.Transparent.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
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
            e_gbuffer.Techniques["BasicColorDrawing"].Passes[0].Apply();

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public static void spritebatch_draw_to_screen(Vector2 offset, Texture2D tex) {
            EngineState.graphics_device.SetRenderTarget(null);
            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            EngineState.spritebatch.Draw(tex, offset, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            EngineState.spritebatch.End();
        }

        public static void clear_all_and_draw_skybox(Camera camera, GBuffer graphics_buffer) {
            EngineState.graphics_device.DepthStencilState = DepthStencilState.DepthRead;

            EngineState.graphics_device.SetRenderTargets(graphics_buffer.buffer_targets);

            e_clear.Parameters["color"].SetValue(sun_moon.atmosphere_color.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullNone;
            EngineState.graphics_device.BlendState = BlendState.Opaque;


            e_skybox.Parameters["atmosphere_color"].SetValue(sun_moon.atmosphere_color.ToVector4());
            e_skybox.Parameters["sky_color"].SetValue(sun_moon.sky_color.ToVector4());

            e_skybox.Parameters["World"].SetValue(Matrix.CreateScale(1f) * Matrix.CreateTranslation(camera.position));
            e_skybox.Parameters["View"].SetValue(camera.view);
            e_skybox.Parameters["Projection"].SetValue(camera.projection);

            e_skybox.Techniques["draw"].Passes[0].Apply();

            EngineState.graphics_device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, skybox_data, 0, 2, skybox_indices, 0, skybox_indices.Length / 3, VertexPositionNormalColorUv.VertexDeclaration);
        }

        public static void compose() {
            if (DigitalControlBindings.get_bind_result("switch_buffer") == DigitalControlBindings.digital_bind_result.just_pressed) {
                if (buffer < buffer_count) buffer++; else buffer = -1;
            }


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

            draw_texture_to_screen(EngineState.buffer.rt_2D);

            //e_compositor.Parameters["fog"].SetValue(false);

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;

            spritebatch_draw_to_screen(Vector2.Zero, EngineState.buffer.rt_final);

            if (screenshot) {
                if (!Directory.Exists("scr")) Directory.CreateDirectory("scr");

                using (FileStream fs = new FileStream("scr/scr" + DateTime.Now.ToFileTime() + ".jpg", FileMode.Create)) {
                    EngineState.buffer.rt_final.SaveAsJpeg(fs, EngineState.buffer.rt_final.Width, EngineState.buffer.rt_final.Height);

                }

                screenshot = false;
                EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            }

        }

    }
}
