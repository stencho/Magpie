using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support2D;
using Magpie.Engine.Collision.Support3D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;
using static Magpie.Engine.Collision.Raycasting;

namespace Magpie.Engine {
    public static class Controls {
        static KeyboardState ks;
        public static KeyboardState keyboard_state => ks;
        static KeyboardState ksp;
        public static KeyboardState keyboard_state_prev => ksp;

        static MouseState ms;
        public static MouseState mouse_state => ms;
        static MouseState msp;
        public static MouseState mouse_state_prev => msp;

        public static XYPair mouse_position => new XYPair(mouse_state.Position.X, mouse_state.Position.Y);
        public static Vector2 mouse_position_float => new Vector2(mouse_state.Position.X, mouse_state.Position.Y);

        public static ISupport2D mouse_collision_object => _mouse_coll_obj;
        static ISupport2D _mouse_coll_obj;

        public static bool mouse_in_bounds => is_mouse_in_bounds();

        static bool is_mouse_in_bounds() {
            return (mouse_position.X > 0 
                 && mouse_position.Y > 0
                 && mouse_position.X < EngineState.resolution.X 
                 && mouse_position.Y < EngineState.resolution.Y);
        }

        static GamePadState[] xs = new GamePadState[4];
        public static GamePadState[] xinput_state => xs;
        static GamePadState[] xsp = new GamePadState[4];
        public static GamePadState[] xinput_state_prev => xsp;

        public static class picker_raycasts {
            public static raycast crosshair_ray;
            public static raycast mouse_pick_ray;

            public static Line3D gjk_crosshair_ray = new Line3D();
            public static Line3D gjk_mouse_pick_ray = new Line3D();

            public static void update() {
                crosshair_ray = new raycast(EngineState.camera.position, EngineState.camera.direction);

                gjk_crosshair_ray.A = EngineState.camera.position;
                gjk_crosshair_ray.B = EngineState.camera.direction;

                //mouse picker stuff
                Vector3 n = new Vector3(mouse_position.X, mouse_position.Y, 0);
                Vector3 f = new Vector3(mouse_position.X, mouse_position.Y, 1);

                Vector3 near = EngineState.viewport.Unproject(n, EngineState.camera.projection, EngineState.camera.view, Matrix.Identity);
                Vector3 far = EngineState.viewport.Unproject(f, EngineState.camera.projection, EngineState.camera.view, Matrix.Identity);

                Vector3 d = far - near;
                d.Normalize();

                mouse_pick_ray = new raycast(near, d);
                
                gjk_mouse_pick_ray.A = near;
                gjk_mouse_pick_ray.B = d;
            }
        }

        //public static float xinput_deadzone_ls = 0f;
        //public static float xinput_deadzone_rs = 0f;

        #region enums
        public enum MouseButtons {
            Left,
            Right,
            Middle,
            Mouse4,
            Mouse5,
            ScrollUp,
            ScrollDown
        }

        public enum MouseAxis {
            X, Y
        }

        public enum XInputAxis {
            LSX, LSY, RSX, RSY,
            TriggerL, TriggerR
        }

        public enum XInputStick {
            Left,
            Right            
        }

        public enum XInputButtons {
            A, B, X, Y,
            LB, RB,
            DPadUp, DPadDown, DPadLeft, DPadRight,
            Start, Back,
            LStick, RStick
        }

        #endregion

        #region analog controls
        public static float get_axis(MouseAxis axis) { return 0f; }
        public static float get_axis(XInputAxis axis) { return 0f; }
        #endregion

        #region is/was pressed
        public static bool was_pressed(Keys k) { return ksp.IsKeyDown(k); }
        public static bool is_pressed(Keys k) { return ks.IsKeyDown(k); }
        public static bool just_pressed(Keys k) { return is_pressed(k) && !was_pressed(k); }
        public static bool just_released(Keys k) { return !is_pressed(k) && was_pressed(k); }

        public static int old_wheel_value = 0;
        public static int wheel_delta = 0;
        static int old_delta = 0;

        static int scroll_wheel_changed() {
            old_delta = wheel_delta;
            wheel_delta = mouse_state.ScrollWheelValue - old_wheel_value;                        
            old_wheel_value = mouse_state.ScrollWheelValue;
            return wheel_delta;
        }

        public static bool is_pressed(MouseButtons mb) {
            switch (mb) {
                case MouseButtons.Left:
                    return mouse_state.LeftButton == ButtonState.Pressed;

                case MouseButtons.Right:
                    return mouse_state.RightButton == ButtonState.Pressed;

                case MouseButtons.Middle:
                    return mouse_state.MiddleButton == ButtonState.Pressed;

                case MouseButtons.Mouse4:
                    return mouse_state.XButton1 == ButtonState.Pressed;

                case MouseButtons.Mouse5:
                    return mouse_state.XButton2 == ButtonState.Pressed;

                case MouseButtons.ScrollUp:
                    return wheel_delta > 0;
                    
                case MouseButtons.ScrollDown:
                    return wheel_delta < 0;

                default: return false;
            }
        }
        public static bool was_pressed(MouseButtons mb) {
            switch (mb) {
                case MouseButtons.Left:
                    return mouse_state_prev.LeftButton == ButtonState.Pressed;

                case MouseButtons.Right:
                    return mouse_state_prev.RightButton == ButtonState.Pressed;

                case MouseButtons.Middle:
                    return mouse_state_prev.MiddleButton == ButtonState.Pressed;

                case MouseButtons.Mouse4:
                    return mouse_state_prev.XButton1 == ButtonState.Pressed;

                case MouseButtons.Mouse5:
                    return mouse_state_prev.XButton2 == ButtonState.Pressed;

                case MouseButtons.ScrollUp:
                    return old_delta > 0;

                case MouseButtons.ScrollDown:
                    return old_delta < 0;

                default: return false;
            }
        }
        public static bool just_pressed(MouseButtons mb) {
            return is_pressed(mb) && !was_pressed(mb);
        }
        public static bool just_released(MouseButtons mb) {
            return !is_pressed(mb) && was_pressed(mb);
        }

        public static bool is_pressed(XInputButtons test_button, PlayerIndex player) {
            switch (test_button) {
                case XInputButtons.A:
                    return xs[(int)player].Buttons.A == ButtonState.Pressed;
                case XInputButtons.B:
                    return xs[(int)player].Buttons.B == ButtonState.Pressed;
                case XInputButtons.X:
                    return xs[(int)player].Buttons.X == ButtonState.Pressed;
                case XInputButtons.Y:
                    return xs[(int)player].Buttons.Y == ButtonState.Pressed;

                case XInputButtons.LB:
                    return xs[(int)player].Buttons.LeftShoulder == ButtonState.Pressed;
                case XInputButtons.RB:
                    return xs[(int)player].Buttons.RightShoulder == ButtonState.Pressed;

                case XInputButtons.Start:
                    return xs[(int)player].Buttons.Start == ButtonState.Pressed;
                case XInputButtons.Back:
                    return xs[(int)player].Buttons.Back == ButtonState.Pressed;

                case XInputButtons.LStick:
                    return xs[(int)player].Buttons.LeftStick == ButtonState.Pressed;
                case XInputButtons.RStick:
                    return xs[(int)player].Buttons.RightStick == ButtonState.Pressed;

                case XInputButtons.DPadUp:
                    return xs[(int)player].DPad.Up == ButtonState.Pressed;
                case XInputButtons.DPadDown:
                    return xs[(int)player].DPad.Down == ButtonState.Pressed;
                case XInputButtons.DPadLeft:
                    return xs[(int)player].DPad.Left == ButtonState.Pressed;
                case XInputButtons.DPadRight:
                    return xs[(int)player].DPad.Right == ButtonState.Pressed;
            }
            return false;
        }

        public static bool was_pressed(XInputButtons test_button, PlayerIndex player) {
            switch (test_button) {
                case XInputButtons.A:
                    return xsp[(int)player].Buttons.A == ButtonState.Pressed;
                case XInputButtons.B:
                    return xsp[(int)player].Buttons.B == ButtonState.Pressed;
                case XInputButtons.X:
                    return xsp[(int)player].Buttons.X == ButtonState.Pressed;
                case XInputButtons.Y:
                    return xsp[(int)player].Buttons.Y == ButtonState.Pressed;

                case XInputButtons.LB:
                    return xsp[(int)player].Buttons.LeftShoulder == ButtonState.Pressed;
                case XInputButtons.RB:
                    return xsp[(int)player].Buttons.RightShoulder == ButtonState.Pressed;

                case XInputButtons.Start:
                    return xsp[(int)player].Buttons.Start == ButtonState.Pressed;
                case XInputButtons.Back:
                    return xsp[(int)player].Buttons.Back == ButtonState.Pressed;

                case XInputButtons.LStick:
                    return xsp[(int)player].Buttons.LeftStick == ButtonState.Pressed;
                case XInputButtons.RStick:
                    return xsp[(int)player].Buttons.RightStick == ButtonState.Pressed;

                case XInputButtons.DPadUp:
                    return xsp[(int)player].DPad.Up == ButtonState.Pressed;
                case XInputButtons.DPadDown:
                    return xsp[(int)player].DPad.Down == ButtonState.Pressed;
                case XInputButtons.DPadLeft:
                    return xsp[(int)player].DPad.Left == ButtonState.Pressed;
                case XInputButtons.DPadRight:
                    return xsp[(int)player].DPad.Right == ButtonState.Pressed;
            }
            return false;
        }

        public static bool just_pressed(XInputButtons test_button, PlayerIndex player) {
            return is_pressed(test_button, player) && !was_pressed(test_button, player);
        }
        public static bool just_released(XInputButtons test_button, PlayerIndex player) {
            return is_pressed(test_button, player) && !was_pressed(test_button, player);
        }

#endregion

        private static XYPair current_res;
        public static bool window_active = true;
        private static bool window_was_active;
        private static XYPair pre_pos = XYPair.Zero;
        private static XYPair window_center = XYPair.Zero;
        public static bool mouse_lock = false;
        static bool mouse_lock_p = false;
        public static Vector2 mouse_delta;
        public static Vector2 mouse_delta_actual;
        static XYPair mdel;

        static Keys[] pressed_keys;
        static Keys[] pressed_keys_previous;
        
        public static void update(GameWindow window, bool is_active, XYPair res) {
            window_center.X = (window.ClientBounds.Width / 2);
            window_center.Y = (window.ClientBounds.Height / 2) ;
            window_active = is_active;

            ksp = ks;
            msp = ms;
            xsp = xs; 

            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            pressed_keys_previous = pressed_keys;
            pressed_keys = ks.GetPressedKeys();

            //mouse_lock = true;
            
            if (mouse_lock && window_active)
                Mouse.SetPosition(window_center.X, window_center.Y);

            if (mouse_lock && mouse_lock_p && window_active && window_was_active) {
                mdel = (window_center)
                                - (XYPair.UnitX * ms.X)
                                - (XYPair.UnitY * ms.Y);

                mouse_delta = new Vector2(mdel.X, mdel.Y);

            } else if (!mouse_lock && !mouse_lock_p) {
                mdel = ((XYPair.UnitX * -(msp.X - ms.X)) + (XYPair.UnitY * -(msp.Y - ms.Y)));

                mouse_delta = new Vector2(mdel.X, mdel.Y);

                
            } else {
                mouse_delta = Vector2.Zero;
            }

            mouse_delta_actual = mouse_delta + Vector2.Zero;

            mouse_delta *= 1 / Clock.frame_probe.sinceF("mouse_delta");
            mouse_delta *= 16f;
            Clock.frame_probe.set("mouse_delta");

            //mouse_delta *= 1/(float)tp.since;
            //mouse_delta *= 4f;
            //tp.setup();

            scroll_wheel_changed();

            window_was_active = window_active;
            mouse_lock_p = mouse_lock;

            _mouse_coll_obj = new Circle2D(Controls.mouse_position_float, 1f);

            picker_raycasts.update();

            xsp[0] = xs[0];
            xsp[1] = xs[1];
            xsp[2] = xs[2];
            xsp[3] = xs[3];

            xs[0] = GamePad.GetState(PlayerIndex.One);
            xs[1] = GamePad.GetState(PlayerIndex.Two);
            xs[2] = GamePad.GetState(PlayerIndex.Three);
            xs[3] = GamePad.GetState(PlayerIndex.Four);

        }

    }
}
