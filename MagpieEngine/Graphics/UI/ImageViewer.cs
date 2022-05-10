using Magpie.Engine;
using Magpie.Engine.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace Magpie.Graphics.UI { 
    public class ImageViewer {
        public Vector2 viewport_position;
        public Vector2 viewport_size;

        Vector2 viewport_half_size => viewport_size / 2;
        Vector2 viewport_zoom_ratio => (viewport_size / viewport_size);
        Vector2 viewport_zoom_offset = Vector2.Zero;

        public Vector2 viewport_offset = Vector2.Zero;

        Vector2 viewport_offset_lerp = Vector2.Zero;

        float viewport_aspect_ratio => (viewport_size.X / viewport_size.Y);
        float viewport_aspect_ratio_v => (viewport_size.Y / viewport_size.X);

        Vector2 image_size => image == null ? Vector2.Zero : new Vector2(image.Bounds.Width, image.Bounds.Height);
        Vector2 image_half_size => image_size / 2;
        Vector2 image_center => image_size / 2f;
        Vector2 image_top_left => image_center - image_half_size;

        float image_aspect_ratio => (image.Width / image.Height);
        float image_aspect_ratio_v => (image.Height / image.Width);

        Viewport actual_viewport;

        public Rectangle viewport_rect => new Rectangle((int)viewport_offset.X, (int)viewport_offset.Y, (int)(viewport_size.X), (int)(viewport_size.Y));

        Vector2 camera_distance_from_center => viewport_offset - viewport_half_size;
        Matrix view;
        Matrix projection;

        public Texture2D image;

        public string shader = "";

        Effect e_texture_effects = ContentHandler.resources["texture_effects"].value_fx;

        BasicEffect effect;
        VerticalQuad quad;

        public RenderTarget2D image_viewed;

        float cursor_scroll_speed => 7f ;

        float zoom = 1f;

        float zoom_lerp_to = 1f;
        float zoom_lerp_speed => (0.5f + ((zoom_lerp_to / max_zoom) * 0.5f)) * 9f;
               
        float max_zoom = 2.5f;
        float min_zoom = 0.1f;

        XYPair mouse_pos => (mouse_position - viewport_position);
        Vector2 normal_mouse_pos => (mouse_position-viewport_position).ToVector2() / viewport_size;

        public bool active { get; set; } = true;
        public bool mouse_over => _mouse_over;
        bool _mouse_over = false;

        public void zoom_to_fit() {
            zoom_lerp_to = (viewport_aspect_ratio_v);
        }        

        public ImageViewer(float x, float y, float w, float h) {
            quad = new VerticalQuad(EngineState.graphics_device, Vector3.Zero);

            viewport_position = new Vector2(x,y);
            viewport_size = new Vector2(w,h);

            image = ContentHandler.resources["zerocool_sharper"].value_tx;
            //image = ContentHandler.resources["circles"].value_tx;
            //image = ContentHandler.resources["sdf_test"].value_tx;

            effect = new BasicEffect(EngineState.graphics_device);
            effect.Texture = image;
            effect.DiffuseColor = Color.White.ToVector3();
            effect.World = Matrix.Identity;

            effect.TextureEnabled = true;
            effect.LightingEnabled = false;

            image_viewed = new RenderTarget2D(EngineState.graphics_device, (int)w, (int)h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            reset_view_zoom_to_fit_image();
        }

        public void change_size(float w, float h) {
            viewport_size = new Vector2(w, h);

            image_viewed = new RenderTarget2D(EngineState.graphics_device, (int)w, (int)h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
        }

        public void change_position(float x, float y) {
            viewport_position = new Vector2(x,y);            
        }

        void center_view_offset_on_image() {
            viewport_offset = -(viewport_half_size);
        }

        Vector2 image_zoom_offset = Vector2.Zero;
        Vector2 tmp_ratio;

        void reset_view_zoom_to_fit_image() {
            
            Vector2 image_tmp_size = image_size;
            
            tmp_ratio = viewport_size / image_size;

            if (tmp_ratio.Y < tmp_ratio.X) {
                zoom = tmp_ratio.Y;
            }
            if (tmp_ratio.Y > tmp_ratio.X) {
                zoom = tmp_ratio.X;
            }

            zoom_lerp_to = zoom;
            center_view_offset_on_image();



            Rectangle r = new Rectangle((int)viewport_offset.X, (int)viewport_offset.Y, (int)(viewport_size.X), (int)(viewport_size.Y));

            actual_viewport = new Viewport(r);
        }

        void reset_view() {
            zoom = 1f;
            zoom_lerp_to = 1;


            //viewport_size = viewport_real_size;

            center_view_offset_on_image();

            Rectangle r = new Rectangle((int)viewport_offset.X, (int)viewport_offset.Y, (int)(viewport_size.X), (int)(viewport_size.Y));

            actual_viewport = new Viewport(r);
        }


        public void update() {
            var mv = Vector2.Zero;

            if (active) {
                if (bind_pressed("wasd_left")) {
                    mv -= Vector2.UnitX;
                }
                if (bind_pressed("wasd_right")) {
                    mv += Vector2.UnitX;
                }
                if (bind_pressed("wasd_forward") && bind_released("shift")) {
                    mv -= Vector2.UnitY;
                }
                if (bind_pressed("wasd_backward") && bind_released("shift")) {
                    mv += Vector2.UnitY;
                }
                
                if (bind_just_pressed("zero")) {
                    reset_view_zoom_to_fit_image();
                }
                if (bind_just_pressed("one")) {
                    reset_view();
                }

                if (zoom < 8f) {
                    if (bind_pressed("scroll_up")) {
                        int x = (Controls.wheel_delta / 120) * 5;
                        float zoomwas = zoom;
                        zoom_lerp_to += (0.05f * x);
                        if (zoom_lerp_to > max_zoom) { zoom_lerp_to = max_zoom; }
                    }
                } else if (zoom > max_zoom) { zoom = max_zoom; }


                if (zoom > min_zoom) {
                    if (bind_pressed("scroll_down")) {
                        int x = (Controls.wheel_delta / -120) * 5;
                        zoom_lerp_to -= (0.05f * x);
                        if (zoom_lerp_to < min_zoom) { zoom_lerp_to = min_zoom; }
                    }

                } else if (zoom < min_zoom) { zoom = min_zoom; }



            }




            zoom = MathHelper.LerpPrecise(zoom, zoom_lerp_to, zoom_lerp_speed * Clock.frame_time_delta);


            viewport_offset += mv * ((cursor_scroll_speed) * (50 + (30 * (zoom - 1f)))) * Clock.frame_time_delta;

            Rectangle r = new Rectangle((int)viewport_offset.X, (int)viewport_offset.Y, (int)(viewport_size.X), (int)(viewport_size.Y));

            actual_viewport = new Viewport(r);

            projection = (Matrix.CreateOrthographicOffCenter(r.Left, r.Right, r.Bottom, r.Top, 0f, 10f));

            effect.World = Matrix.CreateTranslation(-1f, 1f, 0);
            effect.World *= Matrix.CreateScale(1, -1, 1);
            effect.World *= Matrix.CreateScale(image.Width / 2, image.Height / 2, 1);
            effect.World *= Matrix.CreateTranslation(image_half_size.X, image_half_size.Y, 0);

            effect.World *= Matrix.CreateScale(zoom, zoom, 1);

            view = Matrix.CreateLookAt(Vector3.Zero, Vector3.Forward, Vector3.Up);

            effect.Projection = projection;
            effect.View = view;

            if (mouse_pos.X > 0 && mouse_pos.X < viewport_size.X && mouse_pos.Y > 0 && mouse_pos.Y < viewport_size.Y)
                _mouse_over = true;
            else
                _mouse_over = false;

        }

        public void draw() {
            //EngineState.graphics_device.SetRenderTarget(image_viewed);
            //EngineState.graphics_device.Clear(Color.White);

            /*
            EngineState.spritebatch.Begin(SpriteSortMode.Immediate);

            EngineState.spritebatch.Draw(ContentHandler.resources["cash_santa"].value_tx, new Rectangle((int)viewport_position.X,(int)viewport_position.Y,(int)viewport_size.X, (int)viewport_size.Y), Color.White);

            EngineState.spritebatch.End();
            */

            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.RasterizerState = RasterizerState.CullNone;


            EngineState.graphics_device.SetRenderTarget(image_viewed);
            EngineState.graphics_device.Clear(Color.White);


            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, null, null, e_texture_effects);

            e_texture_effects.Parameters["UV_Scale"].SetValue(new Vector2(viewport_size.X / 24, viewport_size.Y / 24));
            e_texture_effects.Parameters["tint"].SetValue(Color.White.ToVector3());
            e_texture_effects.Parameters["alpha_scissor"].SetValue(0f);
            e_texture_effects.Parameters["TEXTURE"].SetValue(ContentHandler.resources["checker"].value_tx);

            Draw2D.image("checker", Vector2.Zero, viewport_size, Color.White);

            EngineState.spritebatch.End();

            //Draw2D.image(ContentHandler.resources["OnePXWhite"].value_tx, Vector2.Zero, viewport_real_size, Color.White);

            //draw quad with image being viewed as its texture
            EngineState.graphics_device.SetVertexBuffer(quad.vertex_buffer);
            EngineState.graphics_device.Indices = quad.index_buffer;

            effect.CurrentTechnique.Passes[0].Apply();

            if (shader != "")
                ContentHandler.resources[shader].value_fx.CurrentTechnique.Passes[0].Apply();

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, quad.vertex_buffer.VertexCount);

            EngineState.spritebatch.Begin(SpriteSortMode.Immediate);


            //debug
            Draw2D.circle(viewport_half_size, 3, Color.LightGreen);
            /*
            Draw2D.circle(-viewport_offset, 10, Color.Red);

            Draw2D.circle(-viewport_offset - (image_half_size * zoom), 10, Color.HotPink);                                                
            Draw2D.circle(-viewport_offset + (Vector2.UnitX * image_half_size.X * zoom) - (Vector2.UnitY * image_half_size.Y * zoom), 10, Color.HotPink);
            Draw2D.circle(-viewport_offset + (Vector2.UnitY * image_half_size.Y * zoom) - (Vector2.UnitX * image_half_size.X * zoom),  10, Color.HotPink);
            Draw2D.circle(-viewport_offset + (image_half_size * zoom), 10, Color.HotPink);


            Draw2D.line(-viewport_offset, viewport_half_size, 2f, Color.HotPink);
            */

            /*
            Draw2D.text_shadow("pf",
                String.Format("view_pos\n{0}", (viewport_offset + viewport_half_size).simple_vector2_x_string_no_dec()), 
                viewport_half_size + (Vector2.One * 3), Color.White);

            Draw2D.text_shadow("pf", "image_center", -viewport_offset + (Vector2.One * 5f), Color.White);

            Draw2D.text_shadow("pf",
                String.Format("image_top_left\n{0}", (-image_half_size).simple_vector2_x_string_no_dec()),
                -viewport_offset - (image_half_size * zoom) - (Vector2.One * 11) - (Vector2.UnitX * 85), Color.White);

            Draw2D.text_shadow("pf",
                String.Format("image_bottom_right\n{0}", image_half_size.simple_vector2_x_string_no_dec()),                
                -viewport_offset + (image_half_size * zoom) + (Vector2.One * 11), Color.White);
            */

            Draw2D.text_shadow("pf", (Vector2.UnitX * 8) + (Vector2.UnitY * (viewport_size.Y - 70)), Color.White,
@"
viewport size {0}
image resolution {1}
zoom level {2},
tmp {3}
",
                viewport_size.simple_vector2_x_string_no_dec(),
                image_size.simple_vector2_x_string_no_dec(),
                string.Format("{0:F2}", zoom),
                tmp_ratio.simple_vector2_x_string()
                );

            //if (active)
            //Draw2D.square(2, 2, viewport_size.X-2, viewport_size.Y-2, 4f, Color.White);

            if (active) {
                Draw2D.square(Vector2.One, viewport_size - Vector2.One, 6f, Color.HotPink);
                Draw2D.square(Vector2.One, viewport_size, 3f, Color.DarkGray);
            } else {
                Draw2D.square(Vector2.One, viewport_size - Vector2.One, 6f, Color.DarkGray);
            }


            EngineState.spritebatch.End();            

        }


        public void draw_output_to_screen() {
            Draw2D.image(image_viewed, viewport_position, viewport_size, Color.White);
        }
    }
}
