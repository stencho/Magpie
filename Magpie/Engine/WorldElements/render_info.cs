using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.Graphics.Scene;

namespace Magpie.Engine.WorldElements {



    public class render_info_model : render_info {
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public Model model => _model; Model _model;
        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;
        public string[] textures { get; set; }

        public BoundingSphere render_bounds { get; set; }

        public render_info_model(string model_name) {
            _model = ContentHandler.resources[model_name].value_gfx;
            textures = new string[1] { "OnePXWhite" };
        }
        public render_info_model(string model_name, string texture_name) {
            _model = ContentHandler.resources[model_name].value_gfx;
            textures = new string[1] { texture_name };
        }

        public void draw() {
            foreach (ModelMesh mm in _model.Meshes) {
                foreach (ModelMeshPart mmp in mm.MeshParts) {
                    Renderer.e_gbuffer.Parameters["World"].SetValue(world);
                    Renderer.e_gbuffer.Parameters["WVIT"].SetValue(Matrix.Transpose(Matrix.Invert(world * EngineState.camera.view)));

                    Renderer.e_gbuffer.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[textures[0]].value_tx);
                    Renderer.e_gbuffer.Parameters["tint"].SetValue(Color.White.ToVector3());

                    EngineState.graphics_device.SetVertexBuffer(mmp.VertexBuffer);
                    EngineState.graphics_device.Indices = mmp.IndexBuffer;

                    Renderer.e_gbuffer.CurrentTechnique.Passes[0].Apply();
                    EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mmp.VertexBuffer.VertexCount);

                }
            }
        }

        public void draw_to_light(light light) {
            Renderer.e_exp_light_depth.Parameters["World"].SetValue(world);

            foreach (ModelMesh mm in _model.Meshes) {
                foreach (ModelMeshPart mmp in mm.MeshParts) {

                    Renderer.e_exp_light_depth.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[textures[0]].value_tx);

                    EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

                    EngineState.graphics_device.SetVertexBuffer(mmp.VertexBuffer);
                    EngineState.graphics_device.Indices = mmp.IndexBuffer;

                    foreach (EffectTechnique tech in Renderer.e_exp_light_depth.Techniques) {
                        foreach (EffectPass pass in tech.Passes) {
                            pass.Apply();
                            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mmp.VertexBuffer.VertexCount);
                        }
                    }

                }
            }

        }

    }

    public class render_info_vertex_buffer : render_info {
        public BoundingSphere render_bounds { get; set; }
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public VertexBuffer vertex_buffer { get; }
        public IndexBuffer index_buffer { get; }


        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;

        public string[] textures { get; set; }

        public render_info_vertex_buffer(VertexBuffer vbuffer, IndexBuffer ibuffer) {
            vertex_buffer = vbuffer;
            index_buffer = ibuffer;
        }

        public void draw() { }
        public void draw_to_light(light light) { }
    }
    public class render_info_vertex_buffers : render_info {
        public BoundingSphere render_bounds { get; set; }
        public Vector3 render_offset { get; set; } = Vector3.Zero;
        public Vector3 scale { get; set; } = Vector3.One;
        public VertexBuffer[] vertex_buffer { get; }
        public IndexBuffer[] index_buffer { get; }
        public string[] textures { get; set; }


        public Matrix world { get; set; } = Matrix.Identity;
        public Matrix orientation { get; set; } = Matrix.Identity;


        public void draw() {}
        public void draw_to_light(light light) { }
    }

    public interface render_info {
        public BoundingSphere render_bounds { get; set; }

        public Vector3 render_offset { get; set; }
        public Vector3 scale { get; set; }

        public Matrix world { get; set; }
        public Matrix orientation { get; set; }

        public string[] textures { get; set; }

        public void draw();
        public void draw_to_light(light light);
    }
}
