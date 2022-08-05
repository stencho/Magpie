using Magpie.Engine;
using Magpie.Engine.Brushes;
using Magpie.Engine.Stages;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics.UI;
using Magpie.Graphics.Lights;
using Magpie.Engine.Physics;
using System.Linq;
using Magpie;
using MagpieHitBuilder.Actors;
using MagpieHitBuilder.TestObjects;
using MagpieHitBuilder.ObjectViewer;

namespace MagpieHitBuilder
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class HitBuilder : Game {
        World world;

        SDFSprite2D crosshair_sdf;

        GraphicsDeviceManager graphics;

        ObjectViewerObject object_viewer;

        public HitBuilder() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            this.IsMouseVisible = false;
            this.graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = true;
            //this.graphics.
            //this.ResetElapsedTime this will be handy

            this.TargetElapsedTime = new TimeSpan(0, 0, 0, 0, 1000 / 33);

            this.MaxElapsedTime = new TimeSpan(0, 0, 0, 0, 500);
            this.InactiveSleepTime = new TimeSpan(0, 0, 0, 0, 16);
        }

        protected override void Initialize()
        {
            base.Initialize();

            EngineState.initialize(new XYPair(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), Window, GraphicsDevice, graphics, this);

            world = new World();

            add_bind(new KeyBind(Keys.W, "forward"));
            add_bind(new KeyBind(Keys.A, "left"));
            add_bind(new KeyBind(Keys.D, "right"));
            add_bind(new KeyBind(Keys.S, "backward"));

            add_bind(new KeyBind(Keys.Space, "up"));
            add_bind(new KeyBind(Keys.C, "down"));

            add_bind(new KeyBind(Keys.F2, "switch_buffer"));
            add_bind(new KeyBind(Keys.F5, "screenshot"));

            add_bind(new KeyBind(Keys.LeftShift, "shift"));
            add_bind(new KeyBind(Keys.LeftControl, "ctrl"));

            add_bind(new MouseButtonBind(MouseButtons.Left, "ui_select"));
            add_bind(new MouseButtonBind(MouseButtons.Right, "click_right"));
            add_bind(new MouseButtonBind(MouseButtons.Middle, "click_middle"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollUp, "scroll_up"));
            add_bind(new MouseButtonBind(MouseButtons.ScrollDown, "scroll_down"));

            /*
            for (int i = 0; i < 150; i++) {
                world.current_map.add_object("test_sphere" + i, new TestSphere());
                world.current_map.objects["test_sphere" + i].position = (Vector3.Forward * (RNG.rng_float * 30)) + (Vector3.Right * (RNG.rng_float_neg_one_to_one * 10)) + (Vector3.Up * (RNG.rng_float * 20));
            }*/
            
            //world.current_map.add_brush("test_floor", new FloorPlane());

            world.current_map.player_actor = new FreeCamActor();
            EngineState.camera = ((FreeCamActor)world.current_map.player_actor).cam;

            EngineState.ui.add_form("top_panel", new UIPanel(XYPair.One * -3, new XYPair(EngineState.resolution.X + 5, 16)));
            EngineState.ui.add_form("test_form", new UIButton(new XYPair(EngineState.resolution.X - 17, 0), new XYPair(17, 18), "close_button", "X", false));

            crosshair_sdf = new SDFSprite2D(
                (EngineState.resolution.ToVector2() * 0.5f),
                new Vector2(16, 16), 0.75f);

            object_viewer = new ObjectViewerObject();
        }

        protected override void LoadContent() {
            ContentHandler.LoadContent(Content, GraphicsDevice);
            ContentHandler.LoadAllResources();
        }

        protected override void UnloadContent() {
            ContentHandler.UnloadAll();
        }

        protected override void Update(GameTime gameTime) {
            EngineState.Update(gameTime, this);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            world.Update();

            if (bind_pressed("click_right"))
                crosshair_sdf.position = mouse_position.ToVector2();
            else
                crosshair_sdf.position = mouse_position.ToVector2();

            if (bind_just_pressed("screenshot")) Scene.screenshot_at_end_of_frame();

            Scene.sun_moon.set_time_of_day(0.5f);
            base.Update(gameTime);
            
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.SetRenderTargets(EngineState.buffer.buffer_targets);

            world.Draw(GraphicsDevice, EngineState.camera);
            Draw3D.xyz_cross(Vector3.Zero, 1f, Color.Red);

            object_viewer.debug_draw();
            object_viewer.draw();

            GraphicsDevice.SetRenderTarget(EngineState.buffer.rt_2D);
            GraphicsDevice.Clear(Color.Transparent);

            /*
            foreach (Brush brush in world.current_map.brushes.Values) {
                brush.debug_draw();
            }
            foreach (GameObject go in world.current_map.objects.Values) {
                //go.debug_draw();
            }
            foreach (Actor actor in world.current_map.actors.Values) {
                //actor.debug_draw();
            }
           */

            EngineState.spritebatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap);

            Draw2D.text_shadow("pf",
                Clock.frame_rate.ToString() + " FPS [" + Clock.frame_rate_immediate + " average/" + Clock.FPS_buffer_length + " frames] " + Clock.frame_time_delta_ms + "ms\n" +
                "Position " + world.player_actor.position.simple_vector3_string_brackets() + "\n" + (((int)Scene.buffer == -1) ? "combined" : ((Scene.buffers)Scene.buffer).ToString())       
                , Vector2.One * 2 + (Vector2.UnitY * 20), Color.White);

            Draw2D.text_shadow("pf", list_active_binds_w_status(), (Vector2.One * 2) + (Vector2.UnitY * 150));

            EngineState.ui.draw();

            EngineState.spritebatch.End();

            crosshair_sdf.draw();

            GraphicsDevice.SetRenderTarget(null);

            Scene.compose();


            //base.Draw(gameTime);
        }
    }
}
