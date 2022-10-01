using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public class VerticalQuad {
        public VertexPositionNormalTexture[] quad => _quad;

        public static VertexPositionNormalTexture[] _quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0), Vector3.UnitZ,  new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 0), Vector3.UnitZ,   new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0), Vector3.UnitZ,  new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), Vector3.UnitZ, new Vector2(0, 1))
            };

        static ushort[] indices = { 0, 1, 2, 2, 3, 0 };

        static VertexBuffer vertex_buffer_s;
        public VertexBuffer vertex_buffer => vertex_buffer_s;
        static IndexBuffer index_buffer_s;
        public IndexBuffer index_buffer => index_buffer_s;

        private static bool loaded = false;

        public Matrix world { get; set; } = Matrix.Identity;
        Matrix ori = Matrix.Identity;

        public string[] textures { get; set; } = new string[] { "OnePXWhite" };
        public Color tint { get; set; } = Color.White;

        Vector3 pos;
        Vector3 s;

        public void update() { }

        public Matrix orientation {
            get { return ori; }
            set {
                ori = value;
                world = Matrix.CreateScale(s) * ori * Matrix.CreateTranslation(pos);
            }
        }

        public Vector3 position {
            get { return pos; }
            set {
                pos = value;
                world = Matrix.CreateScale(s) * ori * Matrix.CreateTranslation(pos);
            }
        }

        public Vector3 scale {
            get => s;
            set {
                s = value;
                world = Matrix.CreateScale(s) * ori * Matrix.CreateTranslation(pos);
            }

        }


        public bool transparent { get; set; } = false;
        public float camera_distance { get; set; } = 0f;

        //public Camera current_camera { get => ; set => ; }

        public VerticalQuad(GraphicsDevice gd, string texture_map) {
            textures[0] = texture_map;
            init_world(Vector3.Zero, Vector3.One); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, string texture_map, Vector3 position) {
            textures[0] = texture_map;
            init_world(position, Vector3.One); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, string texture_map, Vector3 position, float scale) {
            textures[0] = texture_map;
            init_world(position, Vector3.One * scale); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, string texture_map, Vector3 position, Vector3 scale) {
            textures[0] = texture_map;
            init_world(position, scale); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, string texture_map, Vector3 position, Vector2 scale) {
            textures[0] = texture_map;
            init_world(position, scale); create_buffers(gd);
        }

        public VerticalQuad(GraphicsDevice gd) {
            init_world(Vector3.Zero, Vector3.One); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, Vector3 position) {
            init_world(position, Vector3.One); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, Vector3 position, float scale) {
            init_world(position, Vector3.One * scale); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, Vector3 position, Vector3 scale) {
            init_world(position, scale); create_buffers(gd);
        }
        public VerticalQuad(GraphicsDevice gd, Vector3 position, Vector2 scale) {
            init_world(position, scale); create_buffers(gd);
        }

        private void init_world(Vector3 position, Vector2 scale) {
            init_world(position, new Vector3(scale.X, scale.Y, 1));
        }


        private void init_world(Vector3 position, Vector3 scale) {
            pos = position;
            s = scale;
            world = Matrix.CreateScale(s) * ori * Matrix.CreateTranslation(pos);
        }

        private void create_buffers(GraphicsDevice gd) {
            if (loaded) return;

            vertex_buffer_s = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration, _quad.Length, BufferUsage.None);
            vertex_buffer.SetData<VertexPositionNormalTexture>(_quad);
            index_buffer_s = new IndexBuffer(gd, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            index_buffer.SetData<ushort>(indices);

            loaded = true;
        }

        public void add_instance() {
        }
        public void add_instance(Vector3 position, Vector3 scale) {

        }
    }

}
