using Magpie.Engine;
using Magpie.Engine.Floors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }

    public class Scene {
        static Effect e_diffuse = ContentHandler.resources["diffuse"].value_fx;
        static Effect e_lit_diffuse = ContentHandler.resources["lit_diffuse"].value_fx;
        static Effect e_light_depth = ContentHandler.resources["light_depth"].value_fx;
        static Effect e_skybox = ContentHandler.resources["skybox"].value_fx;
        static Effect e_compositor = ContentHandler.resources["compositor"].value_fx;
        static Effect e_clear = ContentHandler.resources["clear"].value_fx;

        public const int max_lights_per_object = 10;
        public static float LIGHT_BIAS = 0.00001f;

        static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
        static VertexPositionNormalColorUv[] skybox_data;
        static int[] skybox_indices;
        static int skybox_face_res = 1024;
        static RenderTarget2D skybox_cm;
        static RenderTarget2D skybox_cm_e;

        public static Color atmosphere_color = Color.MediumPurple;
        public static Color sky_color = Color.Lerp(Color.Purple, Color.LightSkyBlue, 0.2f);
        public static float sky_brightness = 1f;

        public static VerticalQuad quad;

        public static SceneObject[] create_scene_from_lists(Dictionary<string,Floor> floors, Dictionary<string, GameObject> objects, Dictionary<string, Actor> actors, IEnumerable<DynamicLight> lights, BoundingFrustum view_frustum) {
            List<SceneObject> scene = new List<SceneObject>();
            bool any_visible_light_frustum = false;

            foreach (Floor floor in floors.Values) {
                foreach (DynamicLight l in lights) {
                    if (l.frustum.Intersects(EngineState.camera.frustum)) {
                        if (l.frustum.Intersects(floor.bounds)) {
                            any_visible_light_frustum = true;
                            break;
                        }
                    }
                }

                if (floor.bounds.Intersects(view_frustum) || any_visible_light_frustum) {
                    scene.Add(new SceneObject {
                        vertex_buffer = floor.vertex_buffer,
                        index_buffer = floor.index_buffer,
                        mesh_bounds = floor.bounds,
                        world = floor.world,
                        texture = floor.texture,
                        in_light = false,
                        shadow_maps = new List<Texture2D>()
                   });

                }
            }
            foreach (GameObject go in objects.Values) {

                foreach (DynamicLight l in lights) {
                    if (l.frustum.Intersects(EngineState.camera.frustum)) {
                        if (l.frustum.Intersects(go.bounds)) {
                            any_visible_light_frustum = true;
                            break;
                        }
                    }
                }

                if (go.bounds.Intersects(view_frustum) || any_visible_light_frustum) {

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
                }
            }

            foreach (Actor actor in actors.Values) {

            }

            //scene.OrderBy((a) => Vector3.Distance(view_frustum.Matrix.Translation, a.world.Translation));

            return scene.ToArray();
        }

        public static void build_lighting(IEnumerable<DynamicLight> lights, SceneObject[] scene) {
            foreach (DynamicLight light in lights) {
                EngineState.graphics_device.SetRenderTarget(light.depth_map);

                EngineState.graphics_device.Clear(Color.Transparent);

                e_light_depth.Parameters["LVP"].SetValue(light.view * light.projection);
                
                for (int i = 0; i < scene.Length; i++) {
                    SceneObject so = scene[i];

                    if (light.frustum.Intersects(so.mesh_bounds) && light.frustum.Intersects(EngineState.camera.frustum)) {
                        scene[i].in_light = true;

                        e_light_depth.Parameters["World"].SetValue(so.world);
                        
                        EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
                        EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                        EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                        EngineState.graphics_device.Indices = so.index_buffer;

                        foreach (EffectTechnique tech in e_light_depth.Techniques) {
                            foreach (EffectPass pass in tech.Passes) {
                                pass.Apply();
                                EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                            }
                        }

                        scene[i].light_wvp = so.world * light.view * light.projection;
                        scene[i].light_clip = light.far_clip;
                        scene[i].light_pos = light.position;
                        so.shadow_maps.Add(light.depth_map);
                    }
                }

            }
        }

        public static void draw(IEnumerable<SceneObject> scene, IEnumerable<DynamicLight> lights) {
            EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);
            
            e_lit_diffuse.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);
            e_lit_diffuse.Parameters["View"].SetValue(EngineState.camera.view);
            e_lit_diffuse.Parameters["Projection"].SetValue(EngineState.camera.projection);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            foreach (SceneObject so in scene) {
                e_lit_diffuse.Parameters["World"].SetValue(so.world);
                e_lit_diffuse.Parameters["WVIT"].SetValue(Matrix.Invert(so.world * EngineState.camera.view));

                e_lit_diffuse.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[so.texture].value_tx);
                e_lit_diffuse.Parameters["in_light"].SetValue(so.in_light);
                e_lit_diffuse.Parameters["light_color"].SetValue(Color.Red.ToVector3());
                e_lit_diffuse.Parameters["ambient_light"].SetValue(sky_color.ToVector3() * 0.1f);

                if (so.shadow_maps.Count > 0) {
                    e_lit_diffuse.Parameters["shadow_map"].SetValue(so.shadow_maps[0]);
                    e_lit_diffuse.Parameters["lightWVP"].SetValue(so.light_wvp);
                    e_lit_diffuse.Parameters["LightPosition"].SetValue(so.light_pos);
                    e_lit_diffuse.Parameters["LightClip"].SetValue(so.light_clip);
                }
                EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                EngineState.graphics_device.Indices = so.index_buffer;

                foreach (EffectTechnique tech in e_lit_diffuse.Techniques) {
                    foreach (EffectPass pass in tech.Passes) {
                        pass.Apply();
                        EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                    }
                }
            }
        }

        public static void draw_basic_diffuse(IEnumerable<SceneObject> scene) {
            EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);

            e_diffuse.Parameters["opacity"].SetValue(-1f);
            e_diffuse.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_diffuse.Parameters["FarClip"].SetValue(EngineState.camera.far_clip);

            e_diffuse.Parameters["View"].SetValue(EngineState.camera.view);
            e_diffuse.Parameters["Projection"].SetValue(EngineState.camera.projection);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            foreach (SceneObject so in scene) {
                e_diffuse.Parameters["World"].SetValue(so.world);
                e_diffuse.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[so.texture].value_tx);
                
                EngineState.graphics_device.SetVertexBuffer(so.vertex_buffer);
                EngineState.graphics_device.Indices = so.index_buffer;

                foreach (EffectTechnique tech in e_diffuse.Techniques) {
                    foreach (EffectPass pass in tech.Passes) {
                        pass.Apply();
                        EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, so.vertex_buffer.VertexCount);
                    }
                }
            }
        }



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

        public static void configure_renderer() {
            skybox_t.PrivateCreateSkyboxFromCrossImage(out skybox_data, out skybox_indices, 1, 0, 1, 2, 3, 5, 4);
            skybox_t.Subdivide(skybox_data, skybox_indices, out skybox_data, out skybox_indices, 16, MathHelper.Pi);
            skybox_cm = new RenderTarget2D(EngineState.graphics_device, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
            skybox_cm_e = new RenderTarget2D(EngineState.graphics_device, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

            EngineState.graphics_device.SetRenderTarget(skybox_cm);
            EngineState.graphics_device.Clear(atmosphere_color);

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

            e_diffuse.Parameters["opacity"].SetValue(-1f);
            e_diffuse.Parameters["DiffuseMap"].SetValue(tex);
            e_diffuse.Parameters["World"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["View"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["Projection"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();

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

            e_clear.Parameters["color"].SetValue(atmosphere_color.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullNone;
            EngineState.graphics_device.BlendState = BlendState.Opaque;


            e_skybox.Parameters["atmosphere_color"].SetValue(atmosphere_color.ToVector4());
            e_skybox.Parameters["sky_color"].SetValue(sky_color.ToVector4());
            e_skybox.Parameters["sky_brightness"].SetValue(sky_brightness);

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


            clear_buffer(atmosphere_color);

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
