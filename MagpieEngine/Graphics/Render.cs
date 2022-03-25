using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public static class Renderer {
        public static Effect e_diffuse;
        public static Effect e_terrain;
        public static Effect e_compositor;
        public static Effect e_clear;
        public static Effect e_skybox;
        private static BlendState light_blendstate;
        public static Effect e_light_depth;
        public static Effect e_light_spot_geom;
        public static Effect e_light_point_geom;

        static SkyBoxTesselator skybox_t = new SkyBoxTesselator();
        static VertexPositionNormalColorUv[] skybox_data;
        static int[] skybox_indices;
        static int skybox_face_res = 1024;

        static Vector3 light_dir = Vector3.Normalize(Vector3.Down + Vector3.Right + (Vector3.Forward * 0.4f));

        public static Color atmosphere_color = Color.MediumPurple;
        public static Color sky_color = Color.Lerp(Color.Purple, Color.LightSkyBlue, 0.2f);
        // public static Color atmosphere_color = Color.Lerp(Color.DeepPink, Color.White, 0.8f);
        // public static Color sky_color = Color.DeepPink;

        public static float sky_brightness = 1f;

        static RenderTarget2D skybox_cm;
        static RenderTarget2D skybox_cm_e;

        public static XYPair current_res;

        public static GraphicsDevice gd;
        public static GraphicsDeviceManager graphics;
        public static GameWindow window;
        public static SpriteBatch sb;

        public static GBuffer graphics_buffer;

        public static VerticalQuad quad;

        static RasterizerState wf_rasterizer_state = new RasterizerState() { FillMode = FillMode.WireFrame, CullMode = CullMode.None };

        public static bool wireframe = false;
        public static bool fullbright = true;

        public static int frame_draw_count = 0;

        public static Camera last_camera { get; internal set; }
        static bool screenshot = false;
        public static bool taking_screenshot => screenshot;

        public static InstancedVertexData[] Instance_buffer_temp { get => instance_buffer_temp; set => instance_buffer_temp = value; }

        public static void screenshot_at_end_of_frame() { screenshot = true; }

        public enum buffers {
            diffuse,
            normal,
            depth,
            lighting,
            final
        }
        static byte buffer_count = 4;

        static VertexBuffer _fsq_vb;
        static IndexBuffer _fsq_ib;
        private static VertexPositionTexture[] _quad = new VertexPositionTexture[4] {
                new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 0)),
                new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, 1)),
                new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(0, 1))
            };
        public static VertexBuffer fsq_vertex_buffer { get => _fsq_vb; set => _fsq_vb = value; }
        public static IndexBuffer fsq_index_buffer { get => _fsq_ib; set => _fsq_ib = value; }
        public static VertexPositionTexture[] fsq_vb_data { get => _quad; set => _quad = value; }

        public static void configure_renderer(XYPair res, GraphicsDevice graphicsDevice, GraphicsDeviceManager graphicsdm, GameWindow Window, SpriteBatch spriteBatch) {
            gd = graphicsDevice;
            graphics = graphicsdm;
            window = Window;
            sb = spriteBatch;
            current_res = res;

            _fsq_vb = new VertexBuffer(gd, VertexPositionTexture.VertexDeclaration,
            fsq_vb_data.Length, BufferUsage.None);
            _fsq_vb.SetData<VertexPositionTexture>(fsq_vb_data);
            ushort[] indices = { 0, 1, 2, 2, 3, 0 };
            _fsq_ib = new IndexBuffer(gd, IndexElementSize.SixteenBits,
            indices.Length, BufferUsage.None);
            _fsq_ib.SetData<ushort>(indices);

            skybox_t.PrivateCreateSkyboxFromCrossImage(out skybox_data, out skybox_indices, 64, 0, 1, 2, 3, 5, 4);
            skybox_t.Subdivide(skybox_data, skybox_indices, out skybox_data, out skybox_indices, 16, MathHelper.Pi);
            skybox_cm = new RenderTarget2D(gd, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);
            skybox_cm_e = new RenderTarget2D(gd, skybox_face_res * 4, skybox_face_res * 3, false, SurfaceFormat.Rgba64, DepthFormat.Depth16);

            gd.SetRenderTarget(skybox_cm);
            gd.Clear(atmosphere_color);

            gd.SetRenderTarget(null);


            e_diffuse = ContentHandler.resources["diffuse"].value_fx;
            e_clear = ContentHandler.resources["clear"].value_fx;
            e_terrain = ContentHandler.resources["terrain"].value_fx;
            e_compositor = ContentHandler.resources["compositor"].value_fx;

            e_light_depth = ContentHandler.resources["light_depth"].value_fx;

            e_skybox = ContentHandler.resources["skybox"].value_fx;

            light_blendstate = new BlendState();
            light_blendstate.ColorSourceBlend = Blend.One;
            light_blendstate.ColorDestinationBlend = Blend.One;
            light_blendstate.ColorBlendFunction = BlendFunction.Add;
            light_blendstate.AlphaSourceBlend = Blend.One;
            light_blendstate.AlphaDestinationBlend = Blend.One;
            light_blendstate.AlphaBlendFunction = BlendFunction.Add;

            quad = new VerticalQuad(gd);
            graphics_buffer = GBuffer.Create(gd, res.X, res.Y, 1, true);
        }

        public static void clear_buffer() {
            gd.Clear(Color.FromNonPremultiplied(0, 0, 0, 0));
            gd.DepthStencilState = DepthStencilState.DepthRead;

            e_clear.Parameters["color"].SetValue(Color.Transparent.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public static void clear_buffer(Camera cam) {
            gd.Clear(Color.FromNonPremultiplied(0, 0, 0, 0));
            gd.DepthStencilState = DepthStencilState.DepthRead;

            last_camera = cam;

            e_clear.Parameters["color"].SetValue(Color.Transparent.ToVector4());
            e_clear.Techniques["Default"].Passes[0].Apply();

            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
        public static void clear_buffer(Color color) {
            //gd.Clear(color);
            gd.DepthStencilState = DepthStencilState.DepthRead;

            e_clear.Parameters["color"].SetValue(color.ToVector4());
            //e_clear.Parameters["color"].SetValue(Vector4.Zero);
            e_clear.Techniques["Default"].Passes[0].Apply();

            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
        public static void clear_buffer(Camera cam, Color color) {
            //gd.Clear(color);
            gd.DepthStencilState = DepthStencilState.DepthRead;

            last_camera = cam;

            e_clear.Parameters["color"].SetValue(color.ToVector4());
            //e_clear.Parameters["color"].SetValue(Vector4.Zero);
            e_clear.Techniques["Default"].Passes[0].Apply();

            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public static void clear_all_and_draw_skybox(Camera camera, GBuffer graphics_buffer) {
            //gd.Clear(color);
            gd.DepthStencilState = DepthStencilState.DepthRead;

            gd.SetRenderTargets(graphics_buffer.buffer_targets);

            e_clear.Parameters["color"].SetValue(atmosphere_color.ToVector4());
            //e_clear.Parameters["color"].SetValue(Vector4.Zero);
            e_clear.Techniques["Default"].Passes[0].Apply();

            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            gd.RasterizerState = RasterizerState.CullNone;
            gd.BlendState = BlendState.AlphaBlend;


            e_skybox.Parameters["atmosphere_color"].SetValue(atmosphere_color.ToVector4());
            e_skybox.Parameters["sky_color"].SetValue(sky_color.ToVector4());
            e_skybox.Parameters["sky_brightness"].SetValue(sky_brightness);

            e_skybox.Parameters["World"].SetValue(Matrix.CreateScale(150f) * Matrix.CreateTranslation(camera.position));
            e_skybox.Parameters["View"].SetValue(camera.view);
            e_skybox.Parameters["Projection"].SetValue(camera.projection);

            //e_skybox.Parameters["cubemap"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);
            //e_skybox.Parameters["cubemap"].SetValue(skybox_cm);
            //e_skybox.Parameters["cubemap_emissive"].SetValue(skybox_cm_e);

            e_skybox.Techniques["draw"].Passes[0].Apply();

            frame_draw_count++;
            gd.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, skybox_data, 0, 2, skybox_indices, 0, skybox_indices.Length / 3, VertexPositionNormalColorUv.VertexDeclaration);
            

        }

        public static void reset_draw_count() {
            frame_draw_count = 0;
        }


        public static void set_shader(string shader) {
            e_diffuse = ContentHandler.resources[shader].value_fx;
        }
        public static void set_texture(string map) {
            e_diffuse.Parameters["DiffuseMap"].SetValue(ContentHandler.resources[map].value_tx);
        }
        public static void set_texture(ContentHandlerSingleFN handler) {
            e_diffuse.Parameters["DiffuseMap"].SetValue(handler.resource.value_tx);
        }
        public static void set_world(Matrix w) {
            e_diffuse.Parameters["World"].SetValue(w);
        }
        public static void set_buffers(VertexBuffer vb, IndexBuffer ib, int offset = 0) {
            gd.SetVertexBuffer(vb, offset);
            gd.Indices = ib;
        }
        static VertexBuffer vertex_buffer_tmp;
        static IndexBuffer index_buffer_tmp;

        public static void set_and_draw_buffer_data(VertexPositionNormalTexture[] vd, ushort[] id) {

            vertex_buffer_tmp = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, vd.Length, BufferUsage.None);
            vertex_buffer_tmp.SetData<VertexPositionNormalTexture>(vd);
            index_buffer_tmp = new IndexBuffer(gd, IndexElementSize.SixteenBits, id.Length, BufferUsage.None);
            index_buffer_tmp.SetData<ushort>(id);


            gd.SetVertexBuffer(vertex_buffer_tmp, 0);
            gd.Indices = index_buffer_tmp;

            frame_draw_count++;

            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vertex_buffer_tmp.VertexCount));
        }
        public static void set_rasterizer_state(RasterizerState mode) {
            gd.RasterizerState = mode;
        }

        private static void draw_buffers_to_screen(VertexBuffer vb) {
            frame_draw_count++;
            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount));
        }
        private static void draw_buffers_to_screen(VertexBuffer vb, int start_index, int prim_count) {
            frame_draw_count++;
            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, start_index, prim_count);
        }


        public static void draw_buffer_data(VertexPositionNormalTexture[] vd, ushort[] id, Matrix world, string texture) {
            if (!String.IsNullOrWhiteSpace(texture))
                set_texture(texture);

            set_world(world);
            set_and_draw_buffer_data(vd, id);
        }
        public static void draw_buffers(VertexBuffer vb, IndexBuffer ib, Matrix world, string texture) {
            if (!String.IsNullOrWhiteSpace(texture))
                set_texture(texture);

            set_world(world);
            set_buffers(vb, ib);
            draw_buffers_to_screen(vb);
        }


        public static void draw_part(ModelMeshPart part, Matrix world, string texture) {
            if (!String.IsNullOrWhiteSpace(texture))
                set_texture(texture);

            set_world(world);
            set_buffers(part.VertexBuffer, part.IndexBuffer, part.VertexOffset);
            draw_buffers_to_screen(part.VertexBuffer);
        }


        public static void draw_model(Model model, Camera camera, Matrix world, ContentHandlerSingleFN[] textures) {
            e_diffuse.Parameters["View"].SetValue(camera.view);
            e_diffuse.Parameters["Projection"].SetValue(camera.projection);

            r_count = 0;
            foreach (ModelMesh mmmmm in model.Meshes) {
                foreach (ModelMeshPart mlurp in mmmmm.MeshParts) {

                    if ((r_count < textures.Length) && textures[r_count] != null)
                        set_texture(textures[r_count]);

                    set_world(world);
                    set_buffers(mlurp.VertexBuffer, mlurp.IndexBuffer, mlurp.VertexOffset);
                    draw_buffers_to_screen(mlurp.VertexBuffer, mlurp.StartIndex, mlurp.PrimitiveCount);
                    r_count++;
                }
            }

            gd.DepthStencilState = DepthStencilState.Default;
        }



        public static void draw_model(Model model, Camera camera, Matrix world, string[] textures) {
            e_diffuse.Parameters["View"].SetValue(camera.view);
            e_diffuse.Parameters["Projection"].SetValue(camera.projection);

            r_count = 0;
            foreach (ModelMesh mmmmm in model.Meshes) {
                foreach (ModelMeshPart mlurp in mmmmm.MeshParts) {

                    if ((r_count < textures.Length) && !String.IsNullOrWhiteSpace(textures[r_count]))
                        set_texture(textures[r_count]);

                    set_world(world);
                    set_buffers(mlurp.VertexBuffer, mlurp.IndexBuffer, mlurp.VertexOffset);
                    draw_buffers_to_screen(mlurp.VertexBuffer, mlurp.StartIndex, mlurp.PrimitiveCount);
                    r_count++;
                }
            }

            /*
            foreach (ModelBone bone in model.Bones) {

                foreach (ModelBone child in bone.Children) {

                    if (child.Children.Count > 0) {
                        gd.DepthStencilState = DepthStencilState.None;
                        //Draw.xyz_cross(gd, (child.Transform * world).Translation, .1f, Color.LightGreen, last_camera.view, last_camera.projection);
                    }
                }
            }
            */
            gd.DepthStencilState = DepthStencilState.Default;
        }




        static int r_count = 0;

        public static void draw_buffers_start_to_finish_instanced(VertexBuffer vb, IndexBuffer ib, DynamicVertexBuffer instance_buffer, Matrix world, string texture, List<instance> instance_data) {
            if (instance_data.Count <= 0)
                return;

            set_texture(texture);
            set_world(Matrix.Identity);

            //tmp_instance_binding[0] = new VertexBufferBinding(_fsq_vb);
            tmp_instance_binding[0] = new VertexBufferBinding(vb);

            tmp_instance_binding[1] = new VertexBufferBinding(instance_buffer, 0, 1);



            gd.SetVertexBuffers(tmp_instance_binding);
            //gd.Indices = _fsq_ib;
            gd.Indices = ib;

            gd.DepthStencilState = DepthStencilState.Default;
            gd.BlendState = BlendState.AlphaBlend;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;

            if (instance_data.Count < 1) return;
            frame_draw_count++;
            e_diffuse.Techniques["instanced"].Passes[0].Apply();
            //e_diffuse.Techniques["instanced"].Passes[0].Apply();
            //gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount) / 2);
            //e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
            // gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount) / 2);
            gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount / 2, instance_data.Count);
        }

        public struct InstancedVertex : IVertexType {
            public VertexDeclaration VertexDeclaration => throw new NotImplementedException();

            public static readonly VertexElement[] VertexElements = {
                //R1 - R4
                new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
                new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
                new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5),
                new VertexElement(sizeof(float) * 20, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            };
            /*
            //position
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 5),
            //velocity
            new VertexElement((sizeof(float) * 4 * 4) + ((sizeof(float) * 3) * 1), VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 6),
            //tint
            new VertexElement((sizeof(float) * 4 * 4) + ((sizeof(float) * 3) * 2), VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 7),

            //tex offset
            new VertexElement((sizeof(float) * 4 * 4) + ((sizeof(float) * 3) * 3), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 8),

            //tex rotation/opacity/flip_h
            new VertexElement((sizeof(float) * 4 * 4) + ((sizeof(float) * 3) * 3) + ((sizeof(float) * 2) * 1), VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 9)
            */

            //boilerplate
            public static VertexDeclaration Declaration {
                get { return new VertexDeclaration(VertexElements); }
            }
            VertexDeclaration IVertexType.VertexDeclaration {
                get { return new VertexDeclaration(VertexElements); }
            }
        }


        //packed world matrix
        [StructLayout(LayoutKind.Explicit)]
        public struct InstancedVertexData {
            [FieldOffset(0)] public Vector4 tex_offset_flip;
            [FieldOffset(sizeof(float) * 4)] public Vector4 r1;
            [FieldOffset(sizeof(float) * 8)] public Vector4 r2;
            [FieldOffset(sizeof(float) * 12)] public Vector4 r3;
            [FieldOffset(sizeof(float) * 16)] public Vector4 r4;
            [FieldOffset(sizeof(float) * 20)] public Color tint;
        }
        //[FieldOffset(sizeof(float) * 4 * 4)] public Vector3 position;
        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 1))]     public Vector3 velocity;
        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 2))]     public Vector3 tint;

        // [FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 3))]     public Vector2 tex_offset;

        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 3) +     ((sizeof(float) * 2) * 1))]     public Vector3 rot_opa_flip;




        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 3) +     ((sizeof(float) * 2) * 1))]     public float tex_rotation;
        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 3) +     ((sizeof(float) * 2) * 1) +     (sizeof(float) * 1))] public float opacity;     
        //[FieldOffset((sizeof(float) * 4 * 4) +     ((sizeof(float) * 3) * 3) +     ((sizeof(float) * 2) * 1) +     (sizeof(float) * 2))] public bool flip_texture_h;
        // OFFSETS          1 MATRIX         |              VECTOR 3S     N  |           VECTOR 2S        N  |             FLOAT    N


        private static InstancedVertexData[] instance_buffer_temp;
        private static DynamicVertexBuffer temp_return_buffer;

        public static DynamicVertexBuffer pack_instance_data(List<instance> instances) {
            instance_buffer_temp = new InstancedVertexData[instances.Count];

            for (int y = 0; y < instance_buffer_temp.Length; y++) {
                instance_buffer_temp[y].tex_offset_flip.X = instances[y].texture_offset.X;
                instance_buffer_temp[y].tex_offset_flip.Y = instances[y].texture_offset.Y;
                instance_buffer_temp[y].tex_offset_flip.Z = (instances[y].flip_h ? 1 : 0);
                instance_buffer_temp[y].tex_offset_flip.W = (instances[y].flip_v ? 1 : 0);

                instance_buffer_temp[y].r1.X = instances[y].world.M11;
                instance_buffer_temp[y].r1.Y = instances[y].world.M21;
                instance_buffer_temp[y].r1.Z = instances[y].world.M31;
                instance_buffer_temp[y].r1.W = instances[y].world.M41;

                instance_buffer_temp[y].r2.X = instances[y].world.M12;
                instance_buffer_temp[y].r2.Y = instances[y].world.M22;
                instance_buffer_temp[y].r2.Z = instances[y].world.M32;
                instance_buffer_temp[y].r2.W = instances[y].world.M42;

                instance_buffer_temp[y].r3.X = instances[y].world.M13;
                instance_buffer_temp[y].r3.Y = instances[y].world.M23;
                instance_buffer_temp[y].r3.Z = instances[y].world.M33;
                instance_buffer_temp[y].r3.W = instances[y].world.M43;

                instance_buffer_temp[y].r4.X = instances[y].world.M14;
                instance_buffer_temp[y].r4.Y = instances[y].world.M24;
                instance_buffer_temp[y].r4.Z = instances[y].world.M34;
                instance_buffer_temp[y].r4.W = instances[y].world.M44;

                instance_buffer_temp[y].tint = instances[y].tint;

                //instance_buffer_temp[y].opacity = instances[y].opacity;
            }

            temp_return_buffer = new DynamicVertexBuffer(gd, InstancedVertex.Declaration, instance_buffer_temp.Length, BufferUsage.WriteOnly);

            temp_return_buffer.SetData(instance_buffer_temp);

            return temp_return_buffer;
            /*
            data.position = info.position;
            data.velocity = info.velocity_normal * info.velocity_delta;
            data.tint = info.tint.ToVector3();

            data.tex_offset = info.texture_offset;

            data.rot_opa_flip.X = info.texture_rotation;
            data.rot_opa_flip.Y = info.opacity;
            data.rot_opa_flip.Z = (info.flip_texture_h ? 1f : 0f) + (info.flip_texture_v ? 0f : 0.5f);// and then in shader if (rot_opa_flip.z > 0 && < 1) || (rot_opa_flip.z > 1), flip vertical, also if (rot_opa_flip >= 1), flip horizontal

            */
            //data.tex_rotation = info.texture_rotation;
            //data.opacity = info.opacity;
            //data.flip_texture_h = info.flip_texture_h;
        }



        private static VertexBufferBinding[] tmp_instance_binding = new VertexBufferBinding[2];
        /* public static void draw_buffers_start_to_finish_instanced(VertexBuffer vb, IndexBuffer ib, Matrix world, string texture, params instance[] instance_data) {
             if (instance_data.Length <= 0)
                 return;

             set_texture(texture);
             set_world(Matrix.Identity);

             //tmp_instance_binding[0] = new VertexBufferBinding(_fsq_vb);
             tmp_instance_binding[0] = new VertexBufferBinding(vb);


             tmp_instance_binding[1] = new VertexBufferBinding(pack_instance_data(instance_data), 0, 1);



             gd.SetVertexBuffers(tmp_instance_binding);
             //gd.Indices = _fsq_ib;
             gd.Indices = ib;

             gd.DepthStencilState = DepthStencilState.Default;
             gd.BlendState = BlendState.AlphaBlend;
             gd.RasterizerState = RasterizerState.CullCounterClockwise;

             if (instance_data.Length < 1) return;
             frame_draw_count++;
             e_diffuse.Techniques["instanced"].Passes[0].Apply();
             //e_diffuse.Techniques["instanced"].Passes[0].Apply();
             //gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount) / 2);
             //e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
             // gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount) / 2);
             gd.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount / 3, instance_data.Length);
         }*/

        public enum transform_type {
            scale_orientation_translation,
            billboard,
            constrained_billboard
        }

        public struct instance {
            public Matrix world;
            public Vector2 texture_offset;
            public bool flip_h;
            public bool flip_v;
            public Color tint;
        }

        public class model_instance_info {
            public instance instance => new instance() { world = world, texture_offset = texture_offset, flip_h = flip_texture_h, flip_v = flip_texture_v, tint = tint };

            public Matrix orientation = Matrix.Identity;

            public transform_type world_type = transform_type.scale_orientation_translation;

            public Matrix world {
                get {
                    switch (world_type) {
                        case transform_type.scale_orientation_translation:
                            return world_normal;
                        case transform_type.billboard:
                            return world_billboard;
                        case transform_type.constrained_billboard:
                            return world_constrained_billboard;

                        default: return world_normal;
                    }
                }
            }

            private Matrix world_normal => Matrix.Identity * Matrix.CreateScale(scale) * orientation * Matrix.CreateTranslation(position);

            private Matrix world_billboard => Matrix.Identity * Matrix.CreateScale(scale) * Matrix.CreateBillboard(position, last_camera.position, last_camera.up_direction, last_camera.direction);

            private Matrix world_constrained_billboard => Matrix.Identity * Matrix.CreateScale(scale) * Matrix.CreateBillboard(position, last_camera.position, Vector3.Up, last_camera.direction);

            public Vector3 position = Vector3.Zero;
            public Vector3 scale = Vector3.One;

            public Vector3 velocity_normal = Vector3.Zero;
            public float velocity_delta = 0;
            public Vector4 velocity_v4() { return new Vector4(velocity_normal, velocity_delta); }

            public Color tint = Color.White;
            //public Vector3 bounds;

            public Vector2 texture_offset = Vector2.Zero;

            public int system_id;

            public float camera_distance;
            public bool camera_frustum_hit = false;
            public void update_camera_distance_and_frustum(Camera cam) {
                camera_distance = Vector3.Distance(cam.position, this.position);

                if (cam.frustum.Contains(position) != ContainmentType.Disjoint) {
                    //update this upon adding scaled bounding boxes to this class
                    //if that ever happens
                    //might also require creation of hitbox -> bone attachment to
                    // create an accurate bounding box
                    //cylinders + OBBs + their bounding boxen should make creating a composite large bounding box ez
                    //not super necessary tho, literally this is instanced and by design shouldn't be heaving around a shitty butt ass load of data w/ every instance

                    camera_frustum_hit = true;


                }
            }

            public void update(Camera cam) {
                update_camera_distance_and_frustum(cam);


            }

            public float opacity;
            public float texture_rotation;

            public bool flip_texture_h;
            public bool flip_texture_v;

            //public AABB bounding_box; 
            //public OBB bounding_box; // ???????
            //maybe, this will be a big perf hit but could be handy for some cute effects, it might also be possible to do some gpu collision in combination w/ the velocity info
            //should be possible to write a shader that just outputs literally just the IDs of the two colliding objects, then on cpu check that list and just reflect their velocity norms or whatever
            //should also be possible to test collisions vs static world geometry here too, or at least simplified versions of it
            //will be far more useful once I've implemented a static world geometry collision editor for stuff like this/projectiles/raycasts

        }

        /*
        public static void render_drawable(IGeometry render_object, Camera camera) {
            if (camera == null) return;
            if (last_camera != null && camera != last_camera) {
            }
            last_camera = camera;

            if (wireframe)
                gd.RasterizerState = wf_rasterizer_state;
            else {
               // if (!render_object.transparent)
                    gd.RasterizerState = RasterizerState.CullCounterClockwise;
               // else
                   // gd.RasterizerState = RasterizerState.CullNone;

            }
            gd.DepthStencilState = DepthStencilState.Default;
            gd.BlendState = BlendState.AlphaBlend;
            //set_shader(render_object.shader);

            e_diffuse.Parameters["WVIT"].SetValue((Matrix.Invert(render_object.orientation * camera.view)));
            e_diffuse.Parameters["View"].SetValue(camera.view);
            e_diffuse.Parameters["Projection"].SetValue(camera.projection);
            //e_diffuse.Parameters["fullbright"].SetValue(fullbright);
            e_diffuse.Parameters["light_dir"].SetValue(light_dir);
            e_diffuse.Parameters["sky_brightness"].SetValue(sky_brightness);
            e_diffuse.Parameters["opacity"].SetValue(1f);
            e_diffuse.Parameters["tint"].SetValue(1f);
            e_diffuse.Parameters["flip_h"].SetValue(false);
            e_diffuse.Parameters["flip_v"].SetValue(false);
            e_diffuse.Parameters["NearClip"].SetValue(camera.near_clip);
            //e_diffuse.Parameters["FarClip"].SetValue(camera.far_clip);
            //e_diffuse.Parameters["transparency"].SetValue(-1f);
            //e_diffuse.Parameters["transparent"].SetValue(-1f);

            render_object.draw();

            set_shader("diffuse");
        }*/

        public static int clip_render_count = 0;

        static DepthStencilState dss = new DepthStencilState { DepthBufferEnable = true, DepthBufferWriteEnable = false, DepthBufferFunction = CompareFunction.GreaterEqual };

        public static void spritebatch_draw_to_screen(Texture2D tex, Vector2 offset, Vector2 scale, bool flip_h = false, bool flip_v = false) {
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            sb.Draw(tex, offset, null, Color.White, 0f, Vector2.Zero, scale, (flip_h ? SpriteEffects.FlipHorizontally : 0) | (flip_v ? SpriteEffects.FlipVertically : 0), 0f);
            sb.End();
        }

        public static void spritebatch_draw_to_screen(Vector2 offset, Texture2D tex) {
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            sb.Draw(tex, offset, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            sb.End();
        }

        public static void spritebatch_draw_to_screen(XYPair offset, Texture2D tex) {
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            sb.Draw(tex, offset.ToVector2(), null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            sb.End();
        }

        public static void spritebatch_draw_to_screen(Vector2 offset, params Texture2D[] tex) {
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            foreach (Texture2D t in tex)
                sb.Draw(t, offset, null, Color.White, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            sb.End();
        }

        public static void spritebatch_draw_to_screen(params Texture2D[] tex) {
            gd.SetRenderTarget(null);
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, null, null, null);
            foreach (Texture2D t in tex)
                sb.Draw(t, Vector2.Zero, Color.White);
            sb.End();
        }

        public static void draw_texture_to_screen(Texture2D tex) {
            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            gd.BlendState = BlendState.AlphaBlend;
            e_diffuse.Parameters["opacity"].SetValue(-1f);
            e_diffuse.Parameters["DiffuseMap"].SetValue(tex);
            e_diffuse.Parameters["World"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["View"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["Projection"].SetValue(Matrix.Identity);
            e_diffuse.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();
            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            //e_compositor.Parameters["offset"].SetValue(Vector2.Zero);

        }
        static bool FXAA = false;
        public static void draw_FXAA_to_final_buffer() {
            gd.SetRenderTarget(graphics_buffer.rt_final);

            gd.DepthStencilState = DepthStencilState.Default;

            ContentHandler.resources["FXAA"].value_fx.Parameters["texScreen"].SetValue(graphics_buffer.rt_fxaa);

            ContentHandler.resources["FXAA"].value_fx.Parameters["invViewportWidth"].SetValue(1f / graphics_buffer.width);
            ContentHandler.resources["FXAA"].value_fx.Parameters["invViewportHeight"].SetValue(1f / graphics_buffer.height);

            ContentHandler.resources["FXAA"].value_fx.Parameters["fxaaQualitySubpix"].SetValue(0.75f);
            ContentHandler.resources["FXAA"].value_fx.Parameters["fxaaQualityEdgeThreshold"].SetValue(0.166f);
            ContentHandler.resources["FXAA"].value_fx.Parameters["fxaaQualityEdgeThresholdMin"].SetValue(0.0833f);

            ContentHandler.resources["FXAA"].value_fx.Techniques[0].Passes[0].Apply();

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }

        public static int buffer = -1;
        internal static RasterizerState wireframe_state = new RasterizerState() { CullMode = CullMode.None, FillMode = FillMode.WireFrame };

        public static void compose(GBuffer graphics_buffer) {
            if (DigitalControlBindings.get_bind_result("switch_buffer") == DigitalControlBindings.digital_bind_result.just_pressed) {
                if (buffer < buffer_count) buffer++; else buffer = -1;
            }


            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;

            gd.BlendState = BlendState.Additive;

            if (FXAA) {
                gd.SetRenderTarget(graphics_buffer.rt_fxaa);
            } else {
                gd.SetRenderTarget(graphics_buffer.rt_final);
            }

            clear_buffer(atmosphere_color);

            gd.BlendState = BlendState.AlphaBlend;

            e_compositor.Parameters["fog"].SetValue(true);

            e_compositor.Parameters["DiffuseLayer"].SetValue(graphics_buffer.rt_diffuse);
            e_compositor.Parameters["DepthLayer"].SetValue(graphics_buffer.rt_depth);
            e_compositor.Parameters["LightLayer"].SetValue(graphics_buffer.rt_lighting);
            e_compositor.Parameters["NormalLayer"].SetValue(graphics_buffer.rt_normal);
            e_compositor.Parameters["sky_brightness"].SetValue(sky_brightness);
            e_compositor.Parameters["atmosphere_color"].SetValue(atmosphere_color.ToVector3());
            //e_compositor.Parameters["eyelid"].SetValue(0f);
            e_compositor.Parameters["buffer"].SetValue(buffer);
            //e_compositor.Parameters["fullbright"].SetValue(fullbright);

            e_compositor.Techniques["draw"].Passes[0].Apply();

            frame_draw_count++;
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            if (FXAA) {
                draw_FXAA_to_final_buffer();

            }

            draw_texture_to_screen(graphics_buffer.rt_2D);

            e_compositor.Parameters["fog"].SetValue(false);
            //e_compositor.Parameters["LightLayer"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);
            //e_compositor.Parameters["DepthLayer"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);


            gd.BlendState = BlendState.AlphaBlend;


            spritebatch_draw_to_screen(graphics_buffer.position, graphics_buffer.rt_final);


            // spritebatch_draw_to_screen(lights[0].depth_buffer, Vector2.Zero, Vector2.One * 0.3f);

            /*
            //e_compositor.Parameters["DiffuseLayer"].SetValue(graphics_buffer.rt_depth);
            //e_compositor.Techniques["draw"].Passes[0].Apply();
            //gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            //e_compositor.Parameters["LightLayer"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);

            


            gd.SetRenderTarget(null);
            gd.SetVertexBuffer(quad.vertex_buffer);
            gd.Indices = quad.index_buffer;
            e_compositor.Parameters["DiffuseLayer"].SetValue(graphics_buffer.rt_final);
            e_compositor.Techniques["draw"].Passes[0].Apply();
            gd.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
            */
            if (screenshot) {
                if (!Directory.Exists("scr")) Directory.CreateDirectory("scr");

                using (FileStream fs = new FileStream("scr/scr" + DateTime.Now.ToFileTime() + ".jpg", FileMode.Create)) {
                    graphics_buffer.rt_final.SaveAsJpeg(fs, graphics_buffer.rt_final.Width, graphics_buffer.rt_final.Height);

                }

                screenshot = false;
                gd.DepthStencilState = DepthStencilState.Default;

            }

            clip_render_count = 0;
        }
    }
}
