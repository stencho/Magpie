using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static MG2DSDFTool.Controls;
using static MG2DSDFTool.DigitalControlBindings;

namespace MG2DSDFTool {

    public class SDFLayerList {
        public interface Layer {
            bool visible { get; set; }
            string name { get; set; }

            void draw();
            void draw_layer_view();
        }

        public class RasterImageLayer : Layer {
            RenderTarget2D rt;

            XYPair resolution;

            XYPair position;
            XYPair size;

            public bool visible { get; set; }
            public string name { get; set; }

            public void draw_layer_view() {

            }

            public void draw() {

            }
        }

        public class RasterDrawLayer {

        }

        public class SDFLayer {
            public sdf_lib.sdf sdf;

            public void draw_layer_view() {

            }

            public void draw() {

            }
        }

        List<Layer> layers = new List<Layer>();

        public SDFLayerList() {
            layers.Add(new RasterImageLayer());
        }

        public void draw() {

        }
    }

    public class Engine {
        public static XYPair resolution;

        public static bool is_active;

        public static float delta_seconds_f => (float)delta_seconds;
        public static float delta_milliseconds_f => (float)delta_milliseconds;

        public static double delta_seconds;
        public static double delta_milliseconds;

        public static Texture2D onePXWhite;
        public static Texture2D checkerboard_tx;

        public static SpriteFont debug_font;
        
        public GraphicsDevice graphics_device;
        public GraphicsDeviceManager graphics;
        public GameWindow window;

        public SpriteBatch spritebatch;

        public GameTime game_time;
        ContentManager content;

        public RenderTarget2D rt_background;
        public RenderTarget2D rt_display;
        public RenderTarget2D rt_gui;

        Rectangle screen_rect;

        public Camera camera;

        BasicEffect basic_effect;

        Effect compose_effect;
        Effect background_effect;

        sdf_lib.sdf_circle circ;

        struct background_settings {
            XYPair repeat_count;
        }

        background_settings background_settings_checkerboard;

        public void change_resolution(int x, int y) {
            resolution = new XYPair(x, y);
            apply_resolution_change();
        }

        public void apply_resolution_change() {
            rt_background = new RenderTarget2D(graphics_device, resolution.X, resolution.Y);
            rt_display = new RenderTarget2D(graphics_device, resolution.X, resolution.Y);
            rt_gui = new RenderTarget2D(graphics_device, resolution.X, resolution.Y);
            screen_rect = new Rectangle(0, 0, resolution.X, resolution.Y);
        }


        public Engine(XYPair resolution, ContentManager cm, GraphicsDevice gd, GraphicsDeviceManager g, SpriteBatch sb, GameWindow window) {
            debug_font = cm.Load<SpriteFont>("font/pf");

            onePXWhite = new Texture2D(gd, 1, 1);
            onePXWhite.SetData<Color>(new Color[1] { Color.White });

            checkerboard_tx = new Texture2D(gd, 2, 2);
            checkerboard_tx.SetData<Color>(new Color[4] {
                    Color.White, Color.FromNonPremultiplied(127,127,127,255),
                    Color.FromNonPremultiplied(127,127,127,255), Color.White });

            Engine.resolution = resolution;

            this.graphics_device = gd;
            this.graphics = g;
            this.spritebatch = sb;
            this.content = cm;
            this.window = window;

            camera = new Camera(Vector3.Zero);

            basic_effect = new BasicEffect(graphics_device);
            basic_effect.World = Matrix.Identity;

            compose_effect = content.Load<Effect>("shaders/compose");
            background_effect = content.Load<Effect>("shaders/background");
                       

            circ = new sdf_lib.sdf_circle(cm, Vector2.Zero, Vector2.One * 300);
            circ.inner_texture = Texture2D.FromFile(gd, "J:\\nrol_39.png");
            circ.border_texture = Texture2D.FromFile(gd, "J:\\r0341187.jpg");
            circ.outer_texture = Texture2D.FromFile(gd, "J:\\download.jpg");

            apply_resolution_change();
        }

        public void init(ContentManager cm, GraphicsDevice gd, GraphicsDeviceManager g) {
        }


        public void update(GameTime gt, bool is_active) {
            game_time = gt;

            delta_seconds = gt.ElapsedGameTime.TotalSeconds;
            delta_milliseconds = gt.ElapsedGameTime.TotalMilliseconds;

            Engine.is_active = is_active;

            Controls.update(window, is_active, resolution);

            
        }

        public void draw() {
            //drawing background first
            graphics_device.SetRenderTarget(rt_background);
            graphics_device.Clear(Color.White);

            background_effect.Parameters["resolution"].SetValue(resolution.ToVector2());

            //draw checkerboard background
            spritebatch.Begin(SpriteSortMode.Immediate, 
                BlendState.AlphaBlend, SamplerState.PointClamp, 
                DepthStencilState.None, RasterizerState.CullNone,
                background_effect);
            spritebatch.Draw(onePXWhite, screen_rect, Color.Red);
            spritebatch.End();


            
            graphics_device.SetRenderTarget(rt_display);
            graphics_device.Clear(Color.Transparent);
            
            //spritebatch.Begin();
            circ.draw(spritebatch);
            //spritebatch.End();


            //graphics_device.SetRenderTarget(rt_gui);
            

            graphics_device.SetRenderTarget(null);

            spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp);
                
            /*
            
            SpriteSortMode.Immediate,
                BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone,
                compose_effect);

            compose_effect.Parameters["background_texture"].SetValue(rt_background);
            spritebatch.Draw(rt_background, screen_rect, Color.White);
            //compose_effect.Parameters["display_texture"].SetValue(rt_display);
            //compose_effect.Parameters["gui_texture"].SetValue(rt_gui);

            */
                        
            spritebatch.Draw(rt_background, Vector2.Zero, Color.White);
            spritebatch.Draw(rt_display, Vector2.Zero, Color.White);
            spritebatch.Draw(rt_gui, Vector2.Zero, Color.White);

            spritebatch.End();
        }
    }
}
