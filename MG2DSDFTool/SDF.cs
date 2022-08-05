/*
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
        public Vector2 position;
        public Vector2 scale;

        private Vector2 _tex_size;

        public SDFDrawAnchor anchor = SDFDrawAnchor.Center;

        public float alpha_scissor = 1f;

        public string resource_name = "sdf_circle";

        public string overlay_inside_texture_name = "";
        public string overlay_outside_texture_name = "";
        public string overlay_outline_texture_name = "";

        public Color inside_color = Color.White;
        public Color outside_color = Color.Transparent;
        public Color outline_color = Color.Transparent;

        public bool invert_map = false;
        public float opacity = 1f;

        public bool enable_outline = false;
        public float outline_width = 0f;

        public Vector2 inside_tile_count =  Vector2.One;
        public Vector2 outside_tile_count = Vector2.One;
        public Vector2 outline_tile_count = Vector2.One;
                                            
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

        public SDFSprite2D(Vector2 position, Vector2 scale, float alpha_scissor, string sdf_texture) {
            this.position = position;
            this.scale = scale;
            this.alpha_scissor = alpha_scissor;
            this.resource_name = sdf_texture;
        }
        public SDFSprite2D(Vector2 position, Vector2 scale, float alpha_scissor, string sdf_texture, SDFDrawAnchor anchor) {
            this.position = position;
            this.scale = scale;
            this.alpha_scissor = alpha_scissor;
            this.resource_name = sdf_texture;
            this.anchor = anchor;
        }
        
        public void draw() {
            _tex_size = new Vector2(ContentHandler.resources[resource_name].value_tx.Bounds.Size.X, ContentHandler.resources[resource_name].value_tx.Bounds.Size.Y);

            Effect e = ContentHandler.resources["sdf_pixel"].value_fx;

            e.Parameters["alpha_scissor"].SetValue(alpha_scissor);
            
            e.Parameters["invert_map"].SetValue(invert_map);
            e.Parameters["opacity"].SetValue(opacity);
            
            e.Parameters["enable_outline"].SetValue(enable_outline);
            e.Parameters["outline_width"].SetValue(outline_width);
            
            e.Parameters["inside_color"].SetValue(inside_color.ToVector4());
            e.Parameters["outside_color"].SetValue(outside_color.ToVector4());
            e.Parameters["outline_color"].SetValue(outline_color.ToVector4());

            e.Parameters["inside_tile_count"].SetValue(inside_tile_count);
            e.Parameters["outside_tile_count"].SetValue(outside_tile_count);
            e.Parameters["outline_tile_count"].SetValue(outline_tile_count);

            if (!string.IsNullOrEmpty(overlay_inside_texture_name)) {
                e.Parameters["enable_inside_overlay"].SetValue(true);
                e.Parameters["OVERLAY_INSIDE"].SetValue(ContentHandler.resources[overlay_inside_texture_name].value_tx);
            } else {
                e.Parameters["enable_inside_overlay"].SetValue(false);
                e.Parameters["OVERLAY_INSIDE"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);
            }
            
            if (!string.IsNullOrEmpty(overlay_outside_texture_name)) {
                e.Parameters["enable_outside_overlay"].SetValue(true);
                e.Parameters["OVERLAY_OUTSIDE"].SetValue(ContentHandler.resources[overlay_outside_texture_name].value_tx);
            } else {
                e.Parameters["enable_outside_overlay"].SetValue(false);
                e.Parameters["OVERLAY_OUTSIDE"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);
            }

            if (!string.IsNullOrEmpty(overlay_outline_texture_name)) {
                e.Parameters["enable_outline_overlay"].SetValue(true);
                e.Parameters["OVERLAY_OUTLINE"].SetValue(ContentHandler.resources[overlay_outline_texture_name].value_tx);
            } else {
                e.Parameters["enable_outline_overlay"].SetValue(false);
                e.Parameters["OVERLAY_OUTLINE"].SetValue(ContentHandler.resources["OnePXWhite"].value_tx);
            }


            EngineState.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, e, null);

            EngineState.spritebatch.Draw(ContentHandler.resources[resource_name].value_tx, new Rectangle((int)position.X, (int)position.Y, (int)scale.X, (int)scale.Y), null, Color.White, 0f, 
                SDFDrawAnchorExtensions.sdf_anchor_offset(Vector2.One, anchor) * _tex_size, SpriteEffects.None, 0f);

            EngineState.spritebatch.End();

        }
    }

    /*
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
}*/
