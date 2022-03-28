using Magpie.Engine;
using Magpie.Engine.Floors;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
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
    }

    public class Scene {
        static Effect e_diffuse = ContentHandler.resources["diffuse"].value_fx;
        static Effect e_lit_diffuse = ContentHandler.resources["lit_diffuse"].value_fx;
        static Effect e_light_depth = ContentHandler.resources["light_depth"].value_fx;

        public const int max_lights_per_object = 10;
        public static float LIGHT_BIAS = 0.00001f;
        public static SceneObject[] create_scene_from_lists(Dictionary<string,Floor> floors, Dictionary<string, GameObject> objects, Dictionary<string, Actor> actors, IEnumerable<DynamicLight> lights, BoundingFrustum view_frustum) {
            List<SceneObject> scene = new List<SceneObject>();
            bool any_visible_light_frustum = false;

            foreach (Floor floor in floors.Values) {
                foreach (DynamicLight l in lights) {
                    if (l.frustum.Intersects(floor.bounds) && l.frustum.Intersects(EngineState.camera.frustum)) {
                        any_visible_light_frustum = true;
                        break;
                    }
                }

                if (floor.bounds.Intersects(view_frustum) || any_visible_light_frustum) {
                    scene.Add(new SceneObject {
                        vertex_buffer = floor.vertex_buffer,
                        index_buffer = floor.index_buffer,
                        mesh_bounds = floor.bounds,
                        world = floor.world,
                        texture = floor.texture,

                        shadow_maps = new List<Texture2D>()
                   });

                }
            }
            foreach (GameObject go in objects.Values) {

                foreach (DynamicLight l in lights) {
                    if (l.frustum.Intersects(go.bounds) && l.frustum.Intersects(EngineState.camera.frustum)) {
                        any_visible_light_frustum = true;
                        break;
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
                e_lit_diffuse.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[so.texture].value_tx);

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
    }
}
