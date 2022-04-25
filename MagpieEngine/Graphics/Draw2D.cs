using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public static class Draw2D {

        public class GradientMapGenerator1D {
            public float min, max, value;
            public Color start_color;

            public Texture2D debug_band;

            public void build_debug_band_texture(int width = 256) {
                debug_band = new Texture2D(EngineState.graphics_device, width, 1);
                var data = new Color[width];
                
                for (var i = 0; i < data.Length; i++) {
                    float a = i; float b = data.Length;
                    float ab = a / b;

                    data[i] = get_color_at(ab);
                }

                debug_band.SetData(data);
            }

            public struct CLerpPck {
                public Color color; public float position;
                public CLerpPck(Color color, float position) {
                    this.color = color;
                    this.position = position;
                }
            }

            List<CLerpPck> Lerps = new List<CLerpPck>();

            public Color get_color_at(float position) {
                float position_within_lerp = 0.0f;
                float position_end = 1f;
                float lerp_length = 1f;
                float norm_pos_in_lerp = 0.0f;

                var first_lerp = Lerps[0];
                if (position <= first_lerp.position) {
                    lerp_length = first_lerp.position;

                    norm_pos_in_lerp = position / lerp_length;
                    return ColorInterpolate(start_color, first_lerp.color, norm_pos_in_lerp);
                }
                

                for (int i = 0; i < Lerps.Count-1; i++) {
                    CLerpPck lerp = Lerps[i];
                    CLerpPck next_lerp = Lerps[i+1];

                    if (position >= lerp.position && position < next_lerp.position) {
                        position_within_lerp = (position - lerp.position);
                        
                        position_end = next_lerp.position;
                        lerp_length = next_lerp.position - lerp.position;

                        norm_pos_in_lerp = position_within_lerp / lerp_length;

                        return ColorInterpolate(lerp.color, next_lerp.color, norm_pos_in_lerp);
                    }
                }

                return start_color;
            }

            public Color current_color {
                get {
                    foreach (CLerpPck lerp in Lerps) {

                    }

                    return Color.White;
                }
            }
            
            public GradientMapGenerator1D(Color start_color) {
                min = 0.0f;
                max = 0.0f;
                value = 0.0f;

                this.start_color = start_color;
            }

            public void add_lerp(Color color, float position) {
                if (position > max) max = position;
                
                var tmp = new CLerpPck(color, position);

                Lerps.Add(tmp);
            }
        }

        /// <summary>
        /// Interpolates between two colors
        /// </summary>
        /// <param name="colorA">The first color</param>
        /// <param name="colorB">The second color</param>
        /// <param name="bAmount">The amount to interpolate; 0.0 for 100% color A, 1.0 for color B</param>
        /// <returns>The resulting Color</returns>
        public static Color ColorInterpolate(Color colorA, Color colorB, float bAmount) {
            var aAmount = 1.0f - bAmount;
            var r = (int)(colorA.R * aAmount + colorB.R * bAmount);
            var g = (int)(colorA.G * aAmount + colorB.G * bAmount);
            var b = (int)(colorA.B * aAmount + colorB.B * bAmount);

            return Color.FromNonPremultiplied(r, g, b, 255);
        }

        public static T Limit<T>(T input, T min, T max) {
            if (Comparer<T>.Default.Compare(input, max) > 0) return max;
            return Comparer<T>.Default.Compare(min, input) > 0 ? min : input;
        }
        /// <summary>
        /// Generates a randomized color which is similar to the input color
        /// </summary>
        /// <param name="inputColor">the input color</param>
        /// <param name="maxDifference">a float from 0.0-1.0 determining the maximum difference</param>
        /// <returns></returns>
        public static Color SimilarColor(Color inputColor, float maxDifference) {
            var diff = (int)(maxDifference * 255);

            var r = Limit((int)(inputColor.R + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);
            var g = Limit((int)(inputColor.G + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);
            var b = Limit((int)(inputColor.B + diff * ((RNG.rng_float * 2f) - 1.0f)), 0, 255);

            return Color.FromNonPremultiplied(r, g, b, 255);
        }

        /// <summary>
        /// Generates a muted version of the input color
        /// </summary>
        /// <param name="input">the color to mute</param>
        /// <param name="amount">the amount to mute by</param>
        /// <returns></returns>
        public static Color MuteColor(Color input, float amount) {
            return Color.FromNonPremultiplied(
                Limit((int)(input.R * (1.0f - amount)), 0, 255),
                Limit((int)(input.G * (1.0f - amount)), 0, 255),
                Limit((int)(input.B * (1.0f - amount)), 0, 255),
                input.A);
        }

        /// <summary>
        /// Generates a monochrome color between white and black
        /// </summary>
        /// <param name="fromWhite">The maximum amount of difference from 255/255/255, flat white</param>
        /// <returns></returns>
        public static Color RandomShadeOfGrey(float fromWhite) {
            var val = 255 - ((int)(RNG.rng_float * (255 * fromWhite)));
            return Color.FromNonPremultiplied(val, val, val, 255);
        }

        #region Drawing functions

        public static XYPair get_txt_size_pf(string txt) => Math2D.measure_string("pf", txt);

        public static void text(string font, string s, Vector2 position, Color color) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color);
        }

        public enum text_rotation {
            NONE = 0,
            CW_90 = 90,
            CCW_90 = 270,
            UPSIDE_DOWN = 180
        }
        public static void text(string font, string s, Vector2 position, Color color, text_rotation rotation) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color, MathHelper.ToRadians((int)rotation), Vector2.Zero, 1f, SpriteEffects.None, 1f);
        }

        public static void text(string font, string s, Vector2 position, Color color, text_rotation rotation, Vector2 origin, float scale = 1f, SpriteEffects effect = SpriteEffects.None, float z = 1f) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color, MathHelper.ToRadians((int)rotation), origin, scale, effect, z);
        }

        public static void text_shadow(string font, string s, XYPair position, Color color) {
            //Console.WriteLine(s);

            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.One + position, Color.Black);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.Zero + position, color);
        }
        public static void text_shadow(string font, string s, Vector2 position, Color color) {
            //Console.WriteLine(s);

            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position + Vector2.One, Color.Black);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color);
        }
        public static void text_shadow(string font, string s, XYPair position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.One + position, Color.Black, rotation, origin, scale, effects, depth - 1);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.Zero + position, color, rotation, origin, scale, effects, depth);
        }
        public static void text_shadow(string font, string s, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float depth) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position + Vector2.One, Color.Black, rotation, origin, scale, effects, depth - 1);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color, rotation, origin, scale, effects, depth);
        }
        public static void text_shadow(string font, string s, XYPair position, Vector2 norm, Color color, Color shadow_color) {
            //Console.WriteLine(s);

            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, norm + position, shadow_color);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.Zero + position, color);
        }
        public static void text_shadow(string font, string s, Vector2 position, Vector2 norm, Color color, Color shadow_color) {
            //Console.WriteLine(s);

            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position + norm, shadow_color);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color);
        }
        public static void text_shadow(string font, string s, XYPair position, Vector2 norm, Color color, Color shadow_color, float rotation, Vector2 origin, float scale, SpriteEffects effects) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, norm + position, shadow_color, rotation, origin, scale, effects, 0);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, Vector2.Zero + position, color, rotation, origin, scale, effects, 0);
        }
        public static void text_shadow(string font, string s, Vector2 position, Vector2 norm, Color color, Color shadow_color, float rotation, Vector2 origin, float scale, SpriteEffects effects) {
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position + norm, shadow_color, rotation, origin, scale, effects, 0);
            EngineState.spritebatch.DrawString(ContentHandler.resources[font].value_ft, s, position, color, rotation, origin, scale, effects, 0);
        }


        /// <summary>
        /// Draws a line between two points
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw with</param>
        /// <param name="a">the line's start position</param>
        /// <param name="b">the line's end position</param>
        /// <param name="line_width">the line thickness</param>
        /// <param name="color">the line color</param>
        public static void line(Vector2 a, Vector2 b, float line_width, Color color) {
            var tan = b - a;
            var rotation = (float)Math.Atan2(tan.Y, tan.X);

            var middlePoint = new Vector2(0, 0.5f);
            var scale = new Vector2(tan.Length(), line_width);

            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
        }

        public static void line(XYPair a, XYPair b, float line_width, Color color) {
            var tan = b - a;
            var rotation = (float)Math.Atan2(tan.Y, tan.X);

            var middlePoint = new Vector2(0, 0.5f);
            var scale = new Vector2(tan.Length(), line_width);

            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, a.ToVector2(), null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
        }

        public static void line(LineSegment segment, float line_width) {
            line(segment.a, segment.b, line_width, segment.color);
        }

        /// <summary>
        /// Draws a line between two points
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw with</param>
        /// <param name="a">the line's start position</param>
        /// <param name="b">the line's end position</param>
        /// <param name="thickness">the line thickness</param>
        /// <param name="color">the line color</param>
        public static void glow_line(Vector2 a, Vector2 b, float thickness, Color color) {
            var tan = b - a;
            var rotation = (float)Math.Atan2(tan.Y, tan.X);

            var middlePoint = new Vector2(0, ContentHandler.resources["center_glow"].value_tx.Height / 2f);
            var scale = new Vector2(tan.Length(), thickness);

            EngineState.spritebatch.Draw(ContentHandler.resources["center_glow"].value_tx, a, null, color, rotation, middlePoint, scale, SpriteEffects.None, 0f);
        }
        /// <summary>
        /// Draws a horizontally skewed rectangle
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw to</param>
        /// <param name="rect">the base rectangle; at 0.0 skewRight this function will just draw this rectangle</param>
        /// <param name="skewRightByPix">the amount to skew the top by</param>
        /// <param name="col">the color to draw the rectangle as</param>
        public static void skewed_rect_horizontal(Rectangle rect, float skewRightByPix, Color col) {
            for (var i = 0; i < rect.Height; i++) {
                var skewAmount = Math.Round((Convert.ToDouble(i) / Convert.ToDouble(rect.Height)) * skewRightByPix, MidpointRounding.AwayFromZero);

                EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx,
                    new Rectangle(rect.X + (int)(skewRightByPix - skewAmount),
                        rect.Y + i, rect.Width, 1), col);
            }
        }

        /// <summary>
        /// Draws a vertically skewed rectangle
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw to</param>
        /// <param name="rect">the base rectangle; at 0.0 skewUp this function will just draw this rectangle</param>
        /// <param name="skewUpByPix">the amount to skew the right side up by</param>
        /// <param name="col">the color to draw the rectangle as</param>
        public static void skewed_rect_vertical(Rectangle rect, float skewUpByPix, Color col) {
            for (var i = 0; i < rect.Width; i++) {
                var skewAmount = (Convert.ToDouble(i) / Convert.ToDouble(rect.Width)) * skewUpByPix;

                EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, new Rectangle(rect.X + i, rect.Y - (int)skewAmount, 1, rect.Height), col);
            }
        }

        /// <summary>
        /// Draws a very basic fake 3D "cube"
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw to</param>
        /// <param name="frontRect">a rectangle representing the face facing the screen</param>
        /// <param name="depth">the "depth" of the cube</param>
        /// <param name="horiSkew">the amount to skew horizontally</param>
        /// <param name="vertSkew">the amount to skew vertically</param>
        /// <param name="front">the front face color</param>
        /// <param name="top">the top and bottom face colors</param>
        /// <param name="side">the side colors</param>
        public static void skew_cube(Rectangle frontRect, int depth, float horiSkew, float vertSkew, Color front, Color top, Color side) {
            var depthVertSkew = (int)Math.Abs(depth * vertSkew);
            var depthHoriSkew = (int)Math.Abs(depth * horiSkew);

            //Draw top
            if (vertSkew < 0f) {
                skewed_rect_horizontal(new Rectangle(frontRect.X, (frontRect.Y - depthVertSkew), frontRect.Width, depthVertSkew),
                                        depth * horiSkew, top);
                if (horiSkew > 0f)
                    EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, new Rectangle(frontRect.Right, frontRect.Top - depthVertSkew, depthHoriSkew, depthVertSkew), top);
            } else { //Draw bottom
                skewed_rect_horizontal(new Rectangle((int)(frontRect.X + depth * horiSkew), frontRect.Bottom, frontRect.Width, depthVertSkew),
                                        -(depth * horiSkew), top);

                if (horiSkew > 0f)
                    EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, new Rectangle(frontRect.Right, frontRect.Bottom, depthHoriSkew, depthVertSkew), top);
            }

            if (horiSkew > 0f) { //Right
                skewed_rect_vertical(
                                    new Rectangle(frontRect.Right, frontRect.Y, depthHoriSkew, frontRect.Height),
                                    (vertSkew > 0f ? -depthVertSkew : depthVertSkew), side);
            } else { //Left
                skewed_rect_vertical(
                                    new Rectangle((frontRect.X - depthHoriSkew), (frontRect.Y - (vertSkew > 0f ? -depthVertSkew : depthVertSkew)), depthHoriSkew, frontRect.Height),
                                    (vertSkew < 0f ? -depthVertSkew : depthVertSkew), side);
            }

            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, frontRect, front);
        }
        static Rectangle sq_f;
        public static void fill_square(int tlx, int tly, int sizex, int sizey, Color color) {
            sq_f = new Rectangle(tlx, tly, sizex, sizey);
            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, sq_f, color);
        }
        public static void fill_square(XYPair top_left, XYPair size, Color color) {
            sq_f = new Rectangle(top_left.X, top_left.Y, size.X, size.Y);
            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, sq_f, color);
        }

        //ContentHandlerSingleFN

        public static void image(ContentHandlerSingleFN content_handler, XYPair position, XYPair size, Color tint) {
            image(content_handler, position.X, position.Y, size.X, size.Y, tint);
        }
        public static void image(ContentHandlerSingleFN content_handler, int X, int Y, int W, int H, Color tint) {
            EngineState.spritebatch.Draw(content_handler.resource.value_tx, new Rectangle(X, Y, W, H), tint);
        }
        public static void image(RenderTarget2D rt, XYPair position, XYPair size, Color tint) {
            image(rt, position.X, position.Y, size.X, size.Y, tint);
        }
        public static void image(Texture2D texture, XYPair position, XYPair size, Color tint) {
            image(texture, position.X, position.Y, size.X, size.Y, tint, Vector2.Zero);
        }
        public static void image(Texture2D texture, XYPair position, XYPair size, Color tint, SamplerState sampler_state) {
            //Renderer.gd.BlendState = blend_mode;
            //Renderer.gd.RasterizerState = raster_mode;
            

            image(texture, position.X, position.Y, size.X, size.Y, tint, Vector2.Zero);
        }
        public static void image(Texture2D texture, XYPair position, XYPair size, Color tint, Vector2 origin) {
            image(texture, position.X, position.Y, size.X, size.Y, tint, origin);
        }
        public static void image(Texture2D texture, XYPair position, XYPair size, Color tint, Vector2 origin, SamplerState sampler_state) {
            //Renderer.gd.BlendState = blend_mode;
            //Renderer.gd.RasterizerState = raster_mode;


            image(texture, position.X, position.Y, size.X, size.Y, tint, origin);
        }
        public static void image(RenderTarget2D rt, int X, int Y, int W, int H, Color tint) {
            EngineState.spritebatch.Draw(rt, new Rectangle(X, Y, W, H), tint);
        }
        public static void image(Texture2D texture, int X, int Y, int W, int H, Color tint, Vector2 origin) {
            EngineState.spritebatch.Draw(texture, new Rectangle(X, Y, W, H), null, tint, 0f, origin, SpriteEffects.None, 0f);
        }
        public static void image(string name, XYPair position, XYPair size, Color tint, Vector2 origin) {
            image(name, position.X, position.Y, size.X, size.Y, tint);
        }
        public static void image(string name, XYPair offset_index, XYPair offset_cell_size, XYPair position, XYPair size, Color tint) {
            
            EngineState.spritebatch.Draw(ContentHandler.resources[name].value_tx,
                new Rectangle(position.X, position.Y, size.X, size.Y),
                new Rectangle(offset_cell_size.X * offset_index.X, offset_cell_size.Y * offset_index.Y, offset_cell_size.X, offset_cell_size.Y),
                tint);
        }
        public static void image(string name, XYPair offset_index, XYPair offset_cell_size, XYPair position, XYPair size, Color tint, Vector2 origin) {
            EngineState.spritebatch.Draw(ContentHandler.resources[name].value_tx,
                new Rectangle(position.X, position.Y, size.X, size.Y),
                new Rectangle(offset_cell_size.X * offset_index.X, offset_cell_size.Y * offset_index.Y, offset_cell_size.X, offset_cell_size.Y),
                tint, 0f, origin, SpriteEffects.None, 10f);
        }
        public static void image(Texture2D texture, XYPair offset_index, XYPair offset_cell_size, XYPair position, XYPair size, Color tint) {
            EngineState.spritebatch.Draw(texture,
                new Rectangle(position.X, position.Y, size.X, size.Y),
                new Rectangle(offset_cell_size.X * offset_index.X, offset_cell_size.Y * offset_index.Y, offset_cell_size.X, offset_cell_size.Y),
                tint);
        }
        public static void image(Texture2D texture, XYPair offset_index, XYPair offset_cell_size, XYPair position, XYPair size, Color tint, Vector2 origin) {
            EngineState.spritebatch.Draw(texture,
                new Rectangle(position.X, position.Y, size.X, size.Y),
                new Rectangle(offset_cell_size.X * offset_index.X, offset_cell_size.Y * offset_index.Y, offset_cell_size.X, offset_cell_size.Y),
                tint, 0f, origin, SpriteEffects.None, 10f);
        }
        
        public static void image(string name, int X, int Y, int W, int H, Color tint) {
            EngineState.spritebatch.Draw(ContentHandler.resources[name].value_tx, new Rectangle(X, Y, W, H), tint);
        }

        public static void image(string name, XYPair position, XYPair size, Color tint, SpriteEffects flipmode) {
            EngineState.spritebatch.Draw(ContentHandler.resources[name].value_tx, new Rectangle(position.X, position.Y, size.X, size.Y), null, tint, 0f, Vector2.Zero, flipmode, 1f);
        }

        public static void cross(XYPair position, int sizeX, int sizeY, Color color) {
            line(position, position - (Vector2.UnitX * sizeX / 2f), 1f, color);
            line(position, position - (Vector2.UnitY * sizeY / 2f), 1f, color);
            line(position, position + (Vector2.UnitX * sizeX / 2f), 1f, color);
            line(position, position + (Vector2.UnitY * sizeY / 2f), 1f, color);
        }
        public static void cross(Vector2 position, int sizeX, int sizeY, Color color) {
            line(position, position - (Vector2.UnitX * sizeX / 2f), 1f, color);
            line(position, position - (Vector2.UnitY * sizeY / 2f), 1f, color);
            line(position, position + (Vector2.UnitX * sizeX / 2f), 1f, color);
            line(position, position + (Vector2.UnitY * sizeY / 2f), 1f, color);
        }

        /*
        public static bool image_from_file(string filename, int X, int Y, int W, int H, Color tint) {
            if (File.Exists(filename)) {

                }


                return true;
            }*/

        public static void graph(int X, int Y, int width, int height, float[] values) {

        }

        public static void square(Vector2 top_left, Vector2 bottom_right, float line_width, Color col) {
            var w = Vector2.UnitX * (bottom_right.X - top_left.X);
            var h = Vector2.UnitY * (bottom_right.Y - top_left.Y);
            line(top_left - (Vector2.UnitX * (line_width / 2)), (top_left + w) + (Vector2.UnitX * (line_width / 2)), line_width, col); //Top
            line((top_left + h) - (Vector2.UnitX * (line_width / 2)), bottom_right + (Vector2.UnitX * (line_width / 2)), line_width, col); //Bottom
            line(top_left, top_left + h, line_width, col); //Left
            line((top_left + w), bottom_right, line_width, col); //Right
        }

        public static void square(XYPair top_left, XYPair bottom_right, float line_width, Color col) {
            var w = Vector2.UnitX * (bottom_right.X - top_left.X);
            var h = Vector2.UnitY * (bottom_right.Y - top_left.Y);
            line(top_left - (XYPair.UnitX), top_left + w, line_width, col); //Top
            line(top_left + h, bottom_right, line_width, col); //Bottom
            line(top_left, top_left + h, line_width, col); //Left
            line(top_left + w, bottom_right, line_width, col); //Right
        }

        public static void square(Square square, float line_width) {
            Draw2D.square(square.top_left, square.bottom_right, line_width, square.color);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw to</param>
        /// <param name="rect">the rectangle to draw</param>
        /// <param name="lineWidth">the width of the outline</param>
        /// <param name="col">the color to draw the line with</param>
        public static void glow_box(Rectangle rect, float lineWidth, Color col) {
            glow_line(new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), lineWidth, col); //Top
            glow_line(new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Bottom

            glow_line(new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), lineWidth, col); //Left
            glow_line(new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), lineWidth, col); //Right
        }

        /// <summary>
        /// Draws an outline of a polygon
        /// </summary>
        /// <param name="sb">the SpriteBatch to draw to</param>
        /// <param name="lineWidth">the width of the line</param>
        /// <param name="col">the color to draw the line as</param>
        /// <param name="complete">if it's set to true, the line will automatically complete between the last and first point</param>
        /// <param name="points">each of the points to draw lines between</param>
        public static void poly(float lineWidth, Color col, bool complete, params Vector2[] points) {
            //If there's less than 2 points then just return
            if (points.Length <= 1) return;

            //else continue and draw lines between each of the points
            for (var i = 0; i < points.Length - 1; i++) {
                var p = points[i];
                var pP = points[i + 1];

                line(p, pP, lineWidth, col);
            }

            //And then finish the line if complete is true
            if (complete)
                line(points[points.Length - 1], points[0], lineWidth, col);
        }

        public static void poly(Poly2D polygon, float line_width, Color color, bool complete) {
            poly(line_width, color, complete, polygon.points);
        }

        public static void poly(LineSegment[] segments, float line_width, bool complete) {
            if (segments.Length < 1) return;

            for (int i = 0; i < segments.Length; i++)
                line(segments[i], line_width);

            if (complete)
                line(segments[segments.Length - 1].a, segments[0].b, line_width, segments[0].color);
        }


        private static void put_pixel(Vector2 pos, Color col) {
            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, pos, col);
        }

        public static void circle(Vector2 pos, int r, Color color) {
            int x = -1;
            int y = r;
            int f = 1 - r;
            int ddF_x = 0;
            int ddF_y = -2 * r;

            put_pixel(new Vector2(pos.X, pos.Y + r), color);
            put_pixel(new Vector2(pos.X, pos.Y - r), color);
            put_pixel(new Vector2(pos.X + r, pos.Y), color);
            put_pixel(new Vector2(pos.X - r, pos.Y), color);

            while (x < y+1) {
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }

                x++;

                ddF_x += 2;
                f += ddF_x + 1;
                               
                put_pixel(new Vector2(pos.X + x, pos.Y + y), color);
                put_pixel(new Vector2(pos.X - x, pos.Y + y), color);

                put_pixel(new Vector2(pos.X + x, pos.Y - y), color);
                put_pixel(new Vector2(pos.X - x, pos.Y - y), color);

                put_pixel(new Vector2(pos.X + y, pos.Y + x), color);
                put_pixel(new Vector2(pos.X - y, pos.Y + x), color);

                put_pixel(new Vector2(pos.X + y, pos.Y - x), color);
                put_pixel(new Vector2(pos.X - y, pos.Y - x), color);

            }
        }

        public static void fill_circle(Vector2 pos, int r, Color color) {
            int x = -1;
            int y = r;
            int f = 1 - r;
            int ddF_x = 0;
            int ddF_y = -2 * r;

            put_pixel(new Vector2(pos.X, pos.Y + r), color);
            put_pixel(new Vector2(pos.X, pos.Y - r), color);
            put_pixel(new Vector2(pos.X + r, pos.Y), color);
            put_pixel(new Vector2(pos.X - r, pos.Y), color);

            while (x < y+1) {
                if (f >= 0) {
                    y--;
                    ddF_y += 2;
                    f += ddF_y;
                }

                x++;
                ddF_x += 2;
                f += ddF_x + 1;

                line(new Vector2(pos.X - x, pos.Y + y), new Vector2(pos.X + x, pos.Y + y), 1, color);
                line(new Vector2(pos.X - x, pos.Y - y), new Vector2(pos.X + x, pos.Y - y), 1, color);
                line(new Vector2(pos.X - y, pos.Y + x), new Vector2(pos.X + y, pos.Y + x), 1, color);
                line(new Vector2(pos.X - y, pos.Y - x), new Vector2(pos.X + y, pos.Y - x), 1, color);
            }
        }

        public static void poly_circle(Vector2 position,
            float radius, int points, float line_width, Color color) {
            if (points < 6) points = 6;

            Vector2[] poly = new Vector2[points];

            float current_angle = 0;

            for (int i = 0; i < points; i++) {
                current_angle = (float)((i / (float)points) * 2) - 1f;
                poly[i] = position + ((
                    (Vector2.UnitX * (float)Math.Cos(current_angle * MathHelper.Pi))
                    +
                    (Vector2.UnitY * (float)Math.Sin((current_angle) * MathHelper.Pi))
                    ) * radius);
            }

            Draw2D.poly(line_width, color, true, poly);

        }

        public static void DrawPoint(Vector2 pos, Color col, float size = 1f) {
            EngineState.spritebatch.Draw(ContentHandler.resources["OnePXWhite"].value_tx, pos, null, col, 0f, Vector2.One * 0.5f, size, SpriteEffects.None, 0f);
        }

        #endregion
    }

    public struct LineSegment {
        public Vector2 a;
        public Vector2 b;

        public Color color;

        public Vector2 direction => Vector2.Normalize(b - a);
        public float length => Vector2.Distance(a, b);

        public LineSegment(Vector2 A, Vector2 B, Color color) {
            a = A;
            b = B;
            this.color = color;
        }
    }

    public struct Square {
        public Vector2 top_left;
        public Vector2 bottom_right;

        float width => bottom_right.X - top_left.X;
        float height => bottom_right.Y - top_left.Y;

        float left => top_left.X;
        float right => bottom_right.X;

        float top => top_left.Y;
        float bottom => bottom_right.Y;

        Vector2 _top_right;
        Vector2 _bottom_left;

        public Vector2 top_right { get { update_bounds(); return _top_right; } }
        public Vector2 bottom_left { get { update_bounds(); return _bottom_left; } }

        private void update_bounds() {
            _top_right = new Vector2(right, top);
            _bottom_left = new Vector2(left, bottom);
        }

        public Vector2 origin => top_left + ((bottom_right - top_left) / 2);

        public float FindRadius() { return Vector2.Distance(origin, bottom_right); }

        public Color color;

        public Square(Vector2 top_left, Vector2 bottom_right, Color col) {
            this.top_left = top_left;
            this.bottom_right = bottom_right;
            _top_right = new Vector2(bottom_right.X, top_left.Y);
            _bottom_left = new Vector2(top_left.X, bottom_right.Y);
            color = col;
        }
    }

    public struct Poly2D {
        public Vector2[] points;
        public Poly2D(params Vector2[] points) {
            this.points = points;
        }
    }
}