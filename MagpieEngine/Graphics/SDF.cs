using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public enum SDFDrawAnchor {
        TopLeft, Top, TopRight,
        Left,   Center,  Right,
        BottomLeft, Bottom, BottomRight
    }

    public static class SDFDrawAnchorExtensions {
        public static Vector2 sdf_anchor_offset(Vector2 offset_size, SDFDrawAnchor anchor) {
            Vector2 p = Vector2.Zero;

            switch (anchor) {
                case SDFDrawAnchor.TopLeft: break;

                case SDFDrawAnchor.Top:
                    p.X = 0.5f * offset_size.X; p.Y = 0;
                    break;

                case SDFDrawAnchor.TopRight:
                    p.X = 1f * offset_size.X; p.Y = 0;                    
                    break;

                case SDFDrawAnchor.Left:
                    p.X = 0f; p.Y = 0.5f * offset_size.Y;
                    break;

                case SDFDrawAnchor.Center:
                    p.X = 0.5f * offset_size.X; p.Y = 0.5f * offset_size.Y;
                    break;

                case SDFDrawAnchor.Right:
                    p.X = 1f * offset_size.X; p.Y = 0.5f * offset_size.Y;
                    break;
                case SDFDrawAnchor.BottomLeft:
                    p.X = 0f; p.Y = 1f * offset_size.Y;
                    break;

                case SDFDrawAnchor.Bottom:
                    p.X = 0.5f * offset_size.X; p.Y = 1f * offset_size.Y;
                    break;

                case SDFDrawAnchor.BottomRight:
                    p.X = 1f * offset_size.X; p.Y = 1f * offset_size.Y;
                    break;
            }

            return p;
        }
    }

    public interface SDF2D {
        Vector2 position { get; set; }
        Vector2 scale { get; set; }

        SDFDrawAnchor anchor{ get; set; }

        float alpha_scissor { get; set; }

        string resource_name { get; set; }

        Color tint { get; set; }
    }

    public class SDFSprite2D {
        public Vector2 position { get; set; }
        public Vector2 scale { get; set; }

        private Vector2 _tex_size;

        public SDFDrawAnchor anchor { get; set; } = SDFDrawAnchor.Center;

        public float alpha_scissor { get; set; } = 1f;

        public string resource_name { get; set; } = "sdf_circle";

        public Color tint { get; set; } = Color.White;

        public SDFSprite2D(Vector2 position, Vector2 scale) {
            this.position = position;
            this.scale = scale;
            alpha_scissor = 0.5f;
        }

        public SDFSprite2D(Vector2 position, Vector2 scale, float alpha_scissor) {
            this.position = position;
            this.scale = scale;
            this.alpha_scissor = alpha_scissor;
        }

        public void update() {

        }

        public void draw() {
            _tex_size = new Vector2(ContentHandler.resources[resource_name].value_tx.Bounds.Size.X, ContentHandler.resources[resource_name].value_tx.Bounds.Size.Y);

            Draw2D.SDFToScreenImmediate(ContentHandler.resources[resource_name].value_tx, position, scale, tint,
                 SDFDrawAnchorExtensions.sdf_anchor_offset(Vector2.One ,anchor) * _tex_size, 
                 alpha_scissor);
        }
    }

    public class SDFFont2D : SDF2D {
        public Vector2 position { get; set; }
        public Vector2 scale { get; set; }

        private Vector2 _tex_size;

        public SDFDrawAnchor anchor { get; set; } = SDFDrawAnchor.Center;

        public float alpha_scissor { get; set; } = 1f;

        public string resource_name { get; set; } = "sdf_circle";

        public Color tint { get; set; } = Color.White;

        public SDFFont2D(Vector2 position, Vector2 scale) {
            this.position = position;
            this.scale = scale;
            alpha_scissor = 0.5f;
        }

        public SDFFont2D(Vector2 position, Vector2 scale, float sdf_thickness) {
            this.position = position;
            this.scale = scale;
            alpha_scissor = sdf_thickness;
        }

        public void update() {

        }

        public void draw() {
            _tex_size = new Vector2(ContentHandler.resources[resource_name].value_tx.Bounds.Size.X, ContentHandler.resources[resource_name].value_tx.Bounds.Size.Y);

            Draw2D.SDFToScreenImmediate(ContentHandler.resources[resource_name].value_tx, position, scale, tint,
                 SDFDrawAnchorExtensions.sdf_anchor_offset(Vector2.One, anchor) * _tex_size,
                 alpha_scissor);
        }
    }

    class SDFSprite {
        Vector3 position;

        Matrix world;

        public string resource_name = "sdf_circle";

        public SDFSprite() {

        }

        public void update() {

        }

        public void draw() {
            world = Matrix.CreateBillboard(position, EngineState.camera.position, EngineState.camera.orientation.Up, EngineState.camera.orientation.Forward);

            EngineState.graphics_device.SetVertexBuffer(Scene.quad.vertex_buffer);
            EngineState.graphics_device.Indices = Scene.quad.index_buffer;

            
        }

    }
}
