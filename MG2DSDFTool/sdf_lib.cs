using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MG2DSDFTool {
    public static class sdf_lib {
        public enum sdf_shape {
            circle,
            rectangle,
            tri,
            quad,
            polygon,
            map
        }

        public class sdf_batch {
            Dictionary<string, sdf> sdf_list;

            public sdf_batch() {
                sdf_list = new Dictionary<string, sdf>();
            }
            
            public void draw() {

            }
        }

        public interface sdf {
            
            Effect sdf_effect { get; set; }

            bool inner_draw { get; set; }
            bool border_draw { get; set; }

            Vector2 position { get; set; }

            Vector2 size { get; set; }

            Vector2 top_left { get; }
            Vector2 top_right { get; }

            Vector2 bottom_left { get; }
            Vector2 bottom_right { get; }
            
            Texture2D inner_texture { get; set; }
            Texture2D border_texture { get; set; }

            Color inner_color { get; set; }
            Color border_color { get; set; }

            void draw(SpriteBatch sb);
        }

        public class sdf_circle : sdf {
            public sdf_shape shape => sdf_shape.circle;
            public Effect sdf_effect { get; set; }

            public bool inner_draw { get; set; } = true;
            public bool border_draw { get; set; } = true;
            public bool outer_draw { get; set; } = false;
            public bool distance_field_draw { get; set; } = false;

            public Vector2 position { get; set; }
            public Vector2 size { get; set; }

            public Vector2 top_left => position - (size / 2f);
            public Vector2 bottom_right => position + (size / 2f);
            public Vector2 top_right => top_left + (Vector2.UnitX * size.X);
            public Vector2 bottom_left => bottom_right - (Vector2.UnitX * size.X);
            
            public float border_size { get; set; } = 0.3f;

            public float scale { get; set; } = 0.7f;

            public Texture2D inner_texture { get; set; }
            public Texture2D border_texture { get; set; }
            public Texture2D outer_texture { get; set; }

            public Color inner_color { get; set; }
            public Color border_color { get; set; }
            public Color outer_color { get; set; }

            public sdf_circle(ContentManager cm, Vector2 top_left, Vector2 size) {
                sdf_effect = cm.Load<Effect>("shape_shaders/circle");
                position = top_left + (size/2);
                this.size = size;
            }

            public void draw(SpriteBatch sb) {
                sdf_effect.Parameters["draw_inner"].SetValue(inner_draw);
                sdf_effect.Parameters["draw_outer"].SetValue(outer_draw);
                sdf_effect.Parameters["draw_border"].SetValue(border_draw);

                sdf_effect.Parameters["total_scale"].SetValue(scale);
                sdf_effect.Parameters["draw_distance_field"].SetValue(distance_field_draw);
                
                //sdf_effect.Parameters["total_size"].SetValue(size);

                if (border_draw)
                    sdf_effect.Parameters["border_size"].SetValue(border_size);
                else
                    sdf_effect.Parameters["border_size"].SetValue(0f);

                if (inner_texture != null)
                    sdf_effect.Parameters["texture_inside"].SetValue(inner_texture);

                if (border_texture!= null)
                    sdf_effect.Parameters["texture_border"].SetValue(border_texture);

                if (outer_texture != null)
                    sdf_effect.Parameters["texture_outer"].SetValue(outer_texture);


                sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, sdf_effect);
                sb.Draw(Engine.onePXWhite, new Rectangle((int)top_left.X, (int)top_left.Y, (int)size.X, (int)size.Y), Color.White);
                sb.End();
            }
        }

        public class sdf_tri {

        }

        public class sdf_rect : sdf {

            public Effect sdf_effect { get; set; }

            public bool inner_draw { get; set; } = true;
            public bool border_draw { get; set; } = true;
            public bool distance_field_draw { get; set; } = false;

            public Vector2 position { get; set; }
            public Vector2 size { get; set; }
            
            public Vector2 top_left => position - (size / 2f);
            public Vector2 bottom_right => position + (size / 2f);
            public Vector2 top_right => top_left + (Vector2.UnitX * size.X);
            public Vector2 bottom_left => bottom_right - (Vector2.UnitX * size.X);

            public float border_size { get; set; } = 0.02f;

            public Texture2D inner_texture { get; set; }
            public Texture2D border_texture { get; set; }

            public Color inner_color { get; set; } = Color.White; 
            public Color border_color { get; set; } = Color.White;

            public int inner_repeats { get; set; } = 5; 
            public int border_repeats { get; set; } = 1; 

            public Vector2 inner_scroll { get; set; } = Vector2.Zero; 
            public Vector2 border_scroll { get; set; } = Vector2.UnitX * 0.2f; 

            public Vector2 inner_offset { get; set; } = Vector2.Zero;
            public Vector2 border_offset { get; set; } = Vector2.Zero;

            public sdf_rect(ContentManager cm, Vector2 top_left, Vector2 size) {
                sdf_effect = cm.Load<Effect>("shape_shaders/rect");
                position = top_left + (size / 2);
                this.size = size;
            }

            public void update() {
                inner_offset += inner_scroll * Engine.delta_seconds_f;
                border_offset += border_scroll * Engine.delta_seconds_f;
            }

            public void draw(SpriteBatch sb) {

                sdf_effect.Parameters["inner_color"].SetValue(inner_color.ToVector4());
                sdf_effect.Parameters["border_color"].SetValue(border_color.ToVector4());

                sdf_effect.Parameters["inner_repeats"].SetValue(inner_repeats);
                sdf_effect.Parameters["border_repeats"].SetValue(border_repeats);

                while (inner_offset.X > 1) { inner_offset -= Vector2.UnitX; }
                while (inner_offset.X < -1) { inner_offset += Vector2.UnitX; }

                while (inner_offset.Y > 1) { inner_offset -= Vector2.UnitY; }
                while (inner_offset.Y < -1) { inner_offset += Vector2.UnitY; }

                sdf_effect.Parameters["inner_offset"].SetValue(-inner_offset);
                sdf_effect.Parameters["border_offset"].SetValue(-border_offset);

                if (distance_field_draw) {
                    sdf_effect.Parameters["inner_draw"].SetValue(false);
                    sdf_effect.Parameters["border_draw"].SetValue(false);
                    sdf_effect.Parameters["distance_field_draw"].SetValue(true);
                } else {
                    sdf_effect.Parameters["inner_draw"].SetValue(inner_draw);
                    sdf_effect.Parameters["border_draw"].SetValue(border_draw);
                    sdf_effect.Parameters["distance_field_draw"].SetValue(false);
                }


                sdf_effect.Parameters["total_size"].SetValue(size);

                if (border_draw)
                    sdf_effect.Parameters["border_size"].SetValue(border_size);
                else
                    sdf_effect.Parameters["border_size"].SetValue(0f);

                if (inner_texture != null) {
                    sdf_effect.Parameters["inner_texture"].SetValue(inner_texture);
                    sdf_effect.Parameters["inner_texture_resolution"].SetValue(new Vector2(inner_texture.Width, inner_texture.Height));

                    //if (inner_texture.Width >= inner_texture.Height)
                        //sdf_effect.Parameters["inner_texture_aspect_ratio"].SetValue(((float)inner_texture.Height / (float)inner_texture.Width));
                    //else
                        //sdf_effect.Parameters["inner_texture_aspect_ratio"].SetValue(((float)inner_texture.Width / (float)inner_texture.Height));
                }

                if (border_texture != null) {
                    sdf_effect.Parameters["border_texture"].SetValue(border_texture);
                    sdf_effect.Parameters["border_texture_aspect_ratio"].SetValue(((float)border_texture.Width / (float)border_texture.Height));
                }

                sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, sdf_effect);
                sb.Draw(Engine.onePXWhite, new Rectangle((int)top_left.X, (int)top_left.Y, (int)size.X, (int)size.Y), Color.White);
                sb.End();
            }
        }

        public class sdf_map {

        }
        public class sdf_quad {

        }
        public class sdf_polygon {

        }

        public static void load_content(ContentManager cm) {
        }

    }
}
