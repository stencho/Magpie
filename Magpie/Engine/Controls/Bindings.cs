﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Controls;
using Magpie.Graphics;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;
using System.Security.Cryptography.Pkcs;

namespace Magpie.Engine {
    public enum controller_type {
        keyboard,
        mouse,
        xinput
    }

    public enum bind_type { digital, analog };

    public enum analog_bind_type {
        //mouse_delta_axis,
        //mouse_wheel_axis,?????????? is this even worth doing, it seems kinda pointless
        //steam_input_axis,??? steamworks is a very good idea
        xinput_axis
    }

    [Flags]
    public enum digital_bind_state {
        NONE = 0,

        pressed = 1 << 0,
        released = 1 << 1,

        just_pressed = 1 << 2,
        just_released = 1 << 3,

        held = 1 << 4,
        just_held = 1 << 5,

        tapped = 1 << 6,
        released_hold = 1 << 7
    };
    public static class StaticControlBinds {
        public static AnalogBinds analog_binds = new AnalogBinds();
        public static DigitalBinds digital_binds = new DigitalBinds();

        public static bool global_enable = true;
        public static bool global_enable_prev = true;
        static bool just_enabled => global_enable && !global_enable_prev;
        static bool just_disabled => !global_enable && global_enable_prev;

        public static bool bind_exists(string bind) {
            return digital_binds.bind_exists(bind);
        }

        public static string list_binds() {
            string s = "";

            for (int i = 0; i < digital_binds.binds.Count; i++) {
                IDigitalBind b = digital_binds.binds[i];
                if (i < digital_binds.bind_count() - 1)
                    s += $"{b.button_string} :: {b.is_pressed} { b.force_enable }\n";
                else
                    s += $"{b.button_string} :: {b.is_pressed} {b.force_enable}";
            }

            return s;
        }

        public static bool force_enabled(string bind) {
            foreach (IDigitalBind b in digital_binds.binds) {
                if (b.binds.Contains(bind)) {
                    if (b.force_enable) {
                        return true;
                    }
                }
                
            }
            return false;
        }
        public static void force_enable(string bind) {
            foreach (IDigitalBind b in digital_binds.binds) {
                if (b.bind_type == controller_type.keyboard) {
                    if (b.binds.Contains(bind)) {
                        b.force_enable = true;
                    }
                }
            }
        }
        public static void force_disable(string bind) {
            foreach (IDigitalBind b in digital_binds.binds) {
                if (b.bind_type == controller_type.keyboard) {
                    if (b.binds.Contains(bind)) {
                        b.force_enable = false;
                    }
                }
            }
        }

        public static PlayerIndex player_index => _player_index;
        static PlayerIndex _player_index;

        static bool bind_enabled(string bind) {
            if (!global_enable) return force_enabled(bind);
            else return true;
        }

        #region ANALOG
        public static float get_axis(string bind) => analog_binds.get_axis(bind);
        public static void add_bind_analog(XInputAxis axis, string bind) => analog_binds.add_bind(axis, bind);
        #endregion

        #region DIGITAL
        //state
        public static bool pressed(string bind) => bind_enabled(bind) ? digital_binds.bind_pressed(bind) : false;
        public static bool released(string bind) => bind_enabled(bind) ? digital_binds.bind_released(bind) : true;

        public static bool just_pressed(string bind) => bind_enabled(bind) ? 
            digital_binds.bind_just_pressed(bind) || (just_enabled && digital_binds.bind_pressed(bind)) : false;
        public static bool just_released(string bind) => bind_enabled(bind) ? 
            digital_binds.bind_just_released(bind) || (just_disabled && digital_binds.bind_pressed(bind)) : false;

        public static bool held(string bind) => bind_enabled(bind) ? digital_binds.bind_held(bind) : false;
        public static bool just_held(string bind) => bind_enabled(bind) ? digital_binds.bind_just_held(bind) : false;
        public static double held_time(string bind) => bind_enabled(bind) ? digital_binds.bind_held_time(bind) : 0.0;
        public static double pressed_time(string bind) => bind_enabled(bind) ? digital_binds.bind_pressed_time(bind) : 0.0;

        public static bool tapped(string bind) => bind_enabled(bind) ? digital_binds.bind_tapped(bind) : false;

        public static int times_bind_pressed(string bind) => bind_enabled(bind) ? digital_binds.bind_buttons_pressed(bind) : 0;


        //add binds
        public static void add_bind_digital(Keys button, params string[] binds) => digital_binds.add_bind(button, binds);
        public static void add_bind_digital(MouseButtons button, params string[] binds) => digital_binds.add_bind(button, binds);
        public static void add_bind_digital(XInputButtons button, params string[] binds) => digital_binds.add_bind(button, binds);
        #endregion

        public static void change_player_index(PlayerIndex index) { _player_index = index; }

        public static void add_bindings(params (bind_type type, object bind_type_type, object bind_data, string bind)[] data) {
            
            foreach ((bind_type type, object bind_type_type, object bind_data, string bind) param in data) {
                switch (param.type) {

                    case bind_type.digital:

                        switch ((controller_type)param.bind_type_type) {
                            case controller_type.keyboard:
                                add_bind_digital((Keys)param.bind_data, param.bind);
                                break;
                            case controller_type.xinput:
                                add_bind_digital((XInputButtons)param.bind_data, param.bind);
                                break;
                            case controller_type.mouse:
                                add_bind_digital((MouseButtons)param.bind_data, param.bind);
                                break;
                        }
                        break;

                    case bind_type.analog:

                        switch ((analog_bind_type)param.bind_type_type) {
                            case analog_bind_type.xinput_axis:
                                add_bind_analog((XInputAxis)param.bind_data, param.bind);
                                break;
                        }
                        break;
                }
            }
        }

        public static void add_bindings(params (bind_type type, object bind_type_type, object bind_data, string[] binds)[] data) {
            
            foreach ((bind_type type, object bind_type_type, object bind_data, string[] binds) param in data) {
                switch (param.type) {

                    case bind_type.digital:

                        switch ((controller_type)param.bind_type_type) {
                            case controller_type.keyboard:
                                add_bind_digital((Keys)param.bind_data, param.binds);
                                break;
                            case controller_type.xinput:
                                add_bind_digital((XInputButtons)param.bind_data, param.binds);
                                break;
                            case controller_type.mouse:
                                add_bind_digital((MouseButtons)param.bind_data, param.binds);
                                break;
                        }
                        break;

                    case bind_type.analog:

                        switch ((analog_bind_type)param.bind_type_type) {
                            case analog_bind_type.xinput_axis:
                                add_bind_analog((XInputAxis)param.bind_data, param.binds[0]);
                                break;
                        }
                        break;
                }
            }
        }

        public static bool bind_enabled(IDigitalBind bind) {
            return digital_binds.bind_enabled(bind);
        }


        public static void update() {
            global_enable_prev = global_enable;

            analog_binds.update(player_index);
            digital_binds.update(player_index);
        }
        
        public static void draw_state(int X, int Y, int max_Y, int graph_width, int graph_min_height) {
            int current_X = 0;
            int current_Y = 0;

            int highest_X = 0;
            int highest_Y = 0;

            int max_column_width = 0;

            bool height_doink = false;

            Draw2D.text_shadow("pf", "static", new XYPair(X, Y - 20f), Color.White);

            foreach (IDigitalBind bind in digital_binds.binds) {
                var s = bind.binds.Aggregate((a, b) => a + "\n" + b) + " ";

                var ms = Math2D.measure_string("pf", s);

                var graph_height = graph_min_height;
                if (ms.Y > graph_height)
                    graph_height = ms.Y + 8;

                if (ms.X > max_column_width) {
                    max_column_width = ms.X;
                }

                //Draw2D.graph_pwm(X + current_X + 1, Y + current_Y + 1, graph_width, graph_height, "", Color.Black, Color.Black, bind.recent_state);
                //Draw2D.graph_pwm(X + current_X, Y + current_Y, graph_width, graph_height, "", bind.is_pressed && bind_enabled(bind) ? Color.LimeGreen : Color.Red, Color.Pink, bind.recent_state);
                
                Draw2D.fill_square(X + current_X - 2, Y + current_Y - 2, graph_width, graph_height,
                    bind.is_pressed && bind_enabled(bind) ? 
                        (digital_binds.bind_held(bind) ? Color.Green : Color.LimeGreen)
                    : Color.IndianRed);

                Draw2D.text_shadow("pf", s, new XYPair(X + current_X + graph_width, Y + current_Y));

                current_Y += graph_height + 4;
                
                height_doink = false;
                if (current_Y > max_Y) {
                    current_X += graph_width + max_column_width + 8;

                    if (current_Y > highest_Y)
                        highest_Y = current_Y;

                    current_Y = 0;
                    max_column_width = 0;
                    height_doink = true;
                }                
            }

            if (height_doink)
                highest_X = current_X;
            else
                highest_X = current_X + graph_width + max_column_width + 8;


           // Draw2D.square(-5 + X + 1, -5 + Y + 1, highest_X + 5, highest_Y + 5, 1f, Color.Black);
           // Draw2D.square(-5 + X, -5 + Y, highest_X + 5, highest_Y + 5, 1f, Color.White);
        }
    }


    public class ControlBinds {
        AnalogBinds analog_binds;
        DigitalBinds digital_binds;

        public PlayerIndex player_index => _player_index;
        PlayerIndex _player_index;
        
        #region ANALOG
        public float get_axis(string bind) => analog_binds.get_axis(bind);
        public void add_bind_analog(XInputAxis axis, string bind) => analog_binds.add_bind(axis, bind);
        #endregion

        #region DIGITAL
        public bool pressed(string bind) => digital_binds.bind_pressed(bind); 
        public bool released(string bind) => digital_binds.bind_released(bind);

        public bool just_pressed(string bind) => digital_binds.bind_just_pressed(bind);

        public bool just_released(string bind) =>
            (global_enable
                ? digital_binds.bind_just_released(bind)
                : ((global_enable != global_enable_prev && digital_binds.bind_pressed_ignore_enable(bind)) ? true : false));

        public double pressed_time(string bind) => digital_binds.bind_pressed_time(bind);

        public bool held(string bind) => digital_binds.bind_held(bind);
        public bool just_held(string bind) => digital_binds.bind_just_held(bind);

        public double held_time(string bind) => digital_binds.bind_held_time(bind);

        public bool tapped(string bind) => digital_binds.bind_tapped(bind);

        public int times_bind_pressed(string bind) => digital_binds.bind_buttons_pressed(bind);

        public void add_bind_digital(Keys button, params string[] binds) => digital_binds.add_bind(button, binds);
        public void add_bind_digital(MouseButtons button, params string[] binds) => digital_binds.add_bind(button, binds);
        public void add_bind_digital(XInputButtons button, params string[] binds) => digital_binds.add_bind(button, binds);
        #endregion

        public bool bind_exists(string bind) => digital_binds.bind_exists(bind);

        public void change_player_index(PlayerIndex index) { _player_index = index; }

        public bool global_enable = true;
        public bool global_enable_prev = true;

        public void force_enable(string bind) {
            foreach (IDigitalBind b in digital_binds.binds) {
                if (b.bind_type == controller_type.keyboard) {
                    if (b.binds.Contains(bind)) {
                        b.force_enable = true;
                    }
                }
            }
        }
        public void force_disable(string bind) {
            foreach (IDigitalBind b in digital_binds.binds) {
                if (b.bind_type == controller_type.keyboard) {
                    if (b.binds.Contains(bind)) {
                        b.force_enable = false;
                    }
                }                
            }
        }

        public bool bind_enabled(IDigitalBind bind) {
            return digital_binds.bind_enabled(bind);
        }

        public ControlBinds(params (bind_type type, object bind_type_type, object bind_data, string[] binds)[] data) {            
            digital_binds = new DigitalBinds();
            analog_binds = new AnalogBinds();

            foreach ((bind_type type, object bind_type_type, object bind_data, string[] binds) param in data) {
                switch (param.type) {

                    case bind_type.digital:

                        switch ((controller_type)param.bind_type_type) {
                            case controller_type.keyboard:
                                add_bind_digital((Keys)param.bind_data, param.binds);
                                break;
                            case controller_type.xinput:
                                add_bind_digital((XInputButtons)param.bind_data, param.binds);
                                break;
                            case controller_type.mouse:
                                add_bind_digital((MouseButtons)param.bind_data, param.binds);
                                break;
                        }
                        break;

                    case bind_type.analog:

                        switch ((analog_bind_type)param.bind_type_type) {
                            case analog_bind_type.xinput_axis:
                                add_bind_analog((XInputAxis)param.bind_data, param.binds[0]);
                                break;
                        }
                        break;
                }
            }
        }

        public void update() {


            analog_binds.update(player_index);
            digital_binds.update(player_index);

            global_enable_prev = global_enable;
        }

        public void draw_state(int X, int Y, int max_Y, int graph_width, int graph_min_height, string title) {
            int current_X = 0;
            int current_Y = 0;

            int highest_X = 0;
            int highest_Y = 0;

            int max_column_width = 0;

            bool height_doink = false;

            Draw2D.text_shadow("pf", title, new XYPair(X, Y - 20f), Color.White);

            foreach (IDigitalBind bind in digital_binds.binds) {
                var s = bind.binds.Aggregate((a, b) => a + "\n" + b) + " ";

                var ms = Math2D.measure_string("pf", s);

                var graph_height = graph_min_height;
                if (ms.Y > graph_height)
                    graph_height = ms.Y + 8;

                if (ms.X > max_column_width) {
                    max_column_width = ms.X;
                }



                //Draw2D.graph_pwm(X+current_X + 1, Y+current_Y + 1, graph_width, graph_height, "", Color.Black, Color.Black, bind.recent_state);
                //Draw2D.graph_pwm(X + current_X, Y + current_Y, graph_width, graph_height, "", bind.is_pressed && bind_enabled(bind) ? Color.LimeGreen : Color.Red, Color.Pink, bind.recent_state);

                Draw2D.fill_square(X + current_X - 2, Y + current_Y - 2, graph_width, graph_height,
                    bind.is_pressed && bind_enabled(bind) ?
                        (digital_binds.bind_held(bind) ? Color.Green : Color.LimeGreen)
                    : Color.IndianRed);
                Draw2D.text_shadow("pf", s, new XYPair(X+current_X + graph_width, Y+current_Y));

                current_Y += graph_height + 4;

                height_doink = false;
                if (current_Y > max_Y) {
                    current_X += graph_width + max_column_width + 8;

                    if (current_Y > highest_Y)
                        highest_Y = current_Y;

                    current_Y = 0;
                    max_column_width = 0;
                    height_doink = true;
                }                
            }

            if (height_doink)
                highest_X = current_X;
            else
                highest_X = current_X + graph_width + max_column_width + 8;


           // Draw2D.square(-5 + X + 1, -5 + Y + 1, highest_X + 5, highest_Y + 5, 1f, Color.Black);
           // Draw2D.square(-5 + X, -5 + Y, highest_X + 5, highest_Y + 5, 1f, Color.White);
        }
    }


    public interface IAnalogBind {
        analog_bind_type bind_type { get; }
        string bind { get; }
        float state { get; }
        float deadzone { get; }

        object axis_info { get; }

        void update_state(PlayerIndex player_index);
    }

    /*
    public class MouseDeltaAxisBind : IAnalogBind {
        public analog_bind_type bind_type => analog_bind_type.mouse_delta_axis;

        public string bind => _bind;
        string _bind = "";

        public float state => _state;
        float _state = 0;

        public float deadzone => _deadzone;
        float _deadzone = 0;

        public object axis_info => _axis_info;
        public MouseAxis axis_info_actual => (MouseAxis)_axis_info;
        object _axis_info;

        public MouseDeltaAxisBind(MouseAxis axis, string bind) {
            _axis_info = axis;
            _bind = bind;
        }

        public void update_state() {
            if (!mouse_lock) { _state = 0; return; }
            else {
                if (axis_info_actual == MouseAxis.X) {
                    _state = mouse_delta.X;
                } else {
                    _state = mouse_delta.Y;
                }

            }
        }
    }
    */
    public class XinputAxisBind : IAnalogBind {
        public analog_bind_type bind_type => analog_bind_type.xinput_axis;

        public string bind => _bind;
        string _bind = "";

        public float state => _state;
        float _state = 0;

        public float deadzone => _deadzone;
        float _deadzone = 0;

        public object axis_info => _axis_info;
        public XInputAxis axis_info_actual => (XInputAxis)_axis_info;
        object _axis_info;

        public PlayerIndex player_index => _player_index;
        PlayerIndex _player_index;

        public XinputAxisBind(XInputAxis axis, PlayerIndex player_index, string bind) {
            _axis_info = axis;
            _bind = bind;
            this._player_index = player_index;
        }

        public void update_state(PlayerIndex player_index) {

        }
    }

    public class AnalogBinds {
        public List<IAnalogBind> binds = new List<IAnalogBind>();
        List<IAnalogBind> _binds = new List<IAnalogBind>();

        

        public AnalogBinds() { }
        public AnalogBinds(params (analog_bind_type type, object axis, string bind)[] binds) {
            foreach((analog_bind_type type, object axis, string bind) param in binds) {
                switch (param.type) {
                    //case analog_bind_type.mouse_delta_axis:
                    //    add_bind((MouseAxis)param.axis, param.bind);
                    //    break;
                    case analog_bind_type.xinput_axis:
                        add_bind((XInputAxis)param.axis, param.bind);
                        break;
                }
            }
        }

        public void add_bind(XInputAxis axis, string bind) {
            //this is just to make this so there can only be one bind per axis and one axis per bind
            foreach (IAnalogBind b in binds) {
                if (b.bind == bind) {
                    return;
                }

                switch (b.bind_type) {
                    case analog_bind_type.xinput_axis:
                        if (((XinputAxisBind)b).axis_info_actual == axis) { return; }
                        break;
                    default: break;
                }
            }

            binds.Add(new XinputAxisBind(axis, PlayerIndex.One, bind));
        }
        /*
        public void add_bind(MouseAxis axis, string bind) {
            //this is just to make this so there can only be one bind per axis and one axis per bind
            foreach(IAnalogBind b in binds) {
                if (b.bind == bind) {
                    return;
                }

                switch (b.bind_type) {
                    case analog_bind_type.mouse_delta_axis:
                        if (((MouseDeltaAxisBind)b).axis_info_actual == axis) { return; } break;
                    default: break;
                }
            }

            binds.Add(new MouseDeltaAxisBind(axis, bind));
        }
        */


        public float get_axis(string bind) {
            foreach (IAnalogBind b in binds) {
                if (b.bind == bind) {                   
                    return b.state;
                }
            } return 0;
        }

        /*
        public byte get_axis_byte(string bind) {
            float s = (get_axis(bind) + 1) / 2f;
            return (byte)(255*s);
        }

        public int get_axis_int(string bind) {
            float s = get_axis(bind);
            return (int)(int.MaxValue * s);
        }
        */

        public float get_deadzone(string bind) {
            foreach (IAnalogBind b in binds) {
                if (b.bind == bind) {
                    return b.deadzone;
                }
            } return 0;
        }

        public void update(PlayerIndex player_index) {
            for (int i = 0; i < binds.Count; i++) {
                binds[i].update_state(player_index);
            }
        }
    }

    public interface IDigitalBind {
        controller_type bind_type { get; }
        digital_bind_state bind_state { get; }
        
        bool force_enable { get; set; }

        bool is_pressed { get; }
        bool is_released { get; }

        bool just_pressed { get; }
        bool just_released { get; }
                
        bool held { get; }
        bool just_held { get; }

        bool tapped { get; }
        bool released_hold { get; }        

        List<string> binds { get; }

        string button_string { get; }

        double hold_time { get; }

        DateTime pressed_at { get; }

        TimeSpan time_pressed { get; }

        void update_state(PlayerIndex player_index);
                bool[] recent_state { get; }
    }

    public class KeyboardBind : IDigitalBind {
        public controller_type bind_type => controller_type.keyboard;
        
        digital_bind_state _bind_state;
        digital_bind_state _bind_state_prev;
        public digital_bind_state bind_state => _bind_state;

        public bool is_pressed => Controls.is_pressed(button);// bind_state.HasFlag(digital_bind_state.pressed);
        public bool is_released => !is_pressed;

        public bool held => bind_state.HasFlag(digital_bind_state.held);
        public bool just_held => bind_state.HasFlag(digital_bind_state.just_held);

        public bool tapped => bind_state.HasFlag(digital_bind_state.tapped);
        public bool released_hold => bind_state.HasFlag(digital_bind_state.released_hold);

        public bool just_pressed => bind_state.HasFlag(digital_bind_state.just_pressed);
        public bool just_released => bind_state.HasFlag(digital_bind_state.just_released);

        DateTime _pressed_at = DateTime.MinValue; public DateTime pressed_at => _pressed_at;
        TimeSpan _time_pressed = TimeSpan.Zero; public TimeSpan time_pressed => _time_pressed;
        
        List<string> _binds = new List<string>();
        public List<string> binds => _binds;

        public Keys button => _button;
        Keys _button = Keys.A;

        public bool[] recent_state => _recent_state;
        bool[] _recent_state;

        public string button_string => button.ToString();

        public double hold_time => _hold_time;
        public bool force_enable { get; set; } = false;

        double _hold_time = 300;

        bool _was_pressed = false;

        public KeyboardBind(Keys button, string[] binds) {
            this.binds.AddRange(binds);
            _button = button;
        }

        public void update_state(PlayerIndex player_index) {
            if (Controls.is_pressed(button)) {
                _bind_state = digital_bind_state.pressed;

                if (!_was_pressed) {
                    _bind_state |= digital_bind_state.just_pressed;

                    _pressed_at = DateTime.Now;
                }
                _time_pressed = DateTime.Now - _pressed_at;


                if (_time_pressed.TotalMilliseconds >= _hold_time) {
                    _bind_state |= digital_bind_state.held;

                    if (!_bind_state_prev.HasFlag(digital_bind_state.held))
                        _bind_state |= digital_bind_state.just_held;
                }

            } else {
                _bind_state = digital_bind_state.released;

                if (_was_pressed) {
                    _bind_state |= digital_bind_state.just_released;
                }

                _time_pressed = DateTime.Now - _pressed_at;
                bool h = _time_pressed.TotalMilliseconds >= _hold_time;

                _time_pressed = TimeSpan.Zero;

                if (h && just_released)
                    _bind_state |= digital_bind_state.released_hold;

                if (!h && just_released)
                    _bind_state |= digital_bind_state.tapped;
            }

            _bind_state_prev = _bind_state;
            _was_pressed = Controls.is_pressed(button);

        }

    }

    public class XInputBind : IDigitalBind {
        public controller_type bind_type => controller_type.xinput;

        digital_bind_state _bind_state;
        digital_bind_state _bind_state_prev;
        public digital_bind_state bind_state => _bind_state;

        public bool is_pressed => bind_state.HasFlag(digital_bind_state.pressed);
        public bool is_released => bind_state.HasFlag(digital_bind_state.released);

        public bool held => bind_state.HasFlag(digital_bind_state.held);
        public bool just_held => bind_state.HasFlag(digital_bind_state.just_held);

        public bool tapped => bind_state.HasFlag(digital_bind_state.tapped);
        public bool released_hold => bind_state.HasFlag(digital_bind_state.released_hold);

        public bool just_pressed => bind_state.HasFlag(digital_bind_state.just_pressed);
        public bool just_released => bind_state.HasFlag(digital_bind_state.just_released);

        DateTime _pressed_at; public DateTime pressed_at => _pressed_at;
        TimeSpan _time_pressed = TimeSpan.Zero; public TimeSpan time_pressed => _time_pressed;
        
        public List<string> binds => _binds;
        List<string> _binds = new List<string>();

        public XInputButtons button => _button;
        XInputButtons _button = XInputButtons.A;

        public bool[] recent_state => _recent_state;
        bool[] _recent_state;

        public string button_string => button.ToString();
        
        public double hold_time => _hold_time;
        double _hold_time = 400;
        
        public bool force_enable { get; set; } = false;

        public XInputBind(XInputButtons button, string[] binds) {
            this.binds.AddRange(binds);
            _button = button;
            this._recent_state = new bool[60];
        }

        public void update_state(PlayerIndex player_index) {
            if (Controls.is_pressed(button, player_index)) {
                _bind_state = digital_bind_state.pressed;

                if (!Controls.was_pressed(button, player_index)) {
                    _bind_state |= digital_bind_state.just_pressed;

                    _pressed_at = DateTime.Now;
                }
                _time_pressed = DateTime.Now - _pressed_at;


                if (_time_pressed.TotalMilliseconds >= _hold_time) {
                    _bind_state |= digital_bind_state.held;

                    if (!_bind_state_prev.HasFlag(digital_bind_state.held))
                        _bind_state |= digital_bind_state.just_held;
                }

            } else {
                _bind_state = digital_bind_state.released;

                if (Controls.was_pressed(button, player_index)) {
                    _bind_state |= digital_bind_state.just_released;
                }

                _time_pressed = DateTime.Now - _pressed_at;
                bool h = _time_pressed.TotalMilliseconds >= _hold_time;

                _time_pressed = TimeSpan.Zero;

                if (h && just_released)
                    _bind_state |= digital_bind_state.released_hold;

                if (!h && just_released)
                    _bind_state |= digital_bind_state.tapped;
            }

            _bind_state_prev = _bind_state;

            for (int i = 0; i < _recent_state.Length - 1; i++) {
                _recent_state[i] = _recent_state[i + 1];
            }
            _recent_state[_recent_state.Length - 1] = is_pressed;
        }

    }

    public class MouseButtonBind : IDigitalBind {
        public controller_type bind_type => controller_type.mouse;

        digital_bind_state _bind_state;
        digital_bind_state _bind_state_prev;
        public digital_bind_state bind_state => _bind_state;

        public bool is_pressed => Controls.is_pressed(button);
        public bool is_released => !is_pressed;

        public bool held => bind_state.HasFlag(digital_bind_state.held);
        public bool just_held => bind_state.HasFlag(digital_bind_state.just_held);

        public bool tapped => bind_state.HasFlag(digital_bind_state.tapped);
        public bool released_hold => bind_state.HasFlag(digital_bind_state.released_hold);

        public bool just_pressed => bind_state.HasFlag(digital_bind_state.just_pressed);
        public bool just_released => bind_state.HasFlag(digital_bind_state.just_released);

        DateTime _pressed_at; public DateTime pressed_at => _pressed_at;
        TimeSpan _time_pressed = TimeSpan.Zero; public TimeSpan time_pressed => _time_pressed;
        
        List<string> _binds = new List<string>();
        public List<string> binds => _binds;

        public string button_string => button.ToString();

        public MouseButtons button => _button;
        MouseButtons _button = MouseButtons.Left;

        public bool[] recent_state => _recent_state;
        bool[] _recent_state;

        public double hold_time => _hold_time;
        double _hold_time = 75;

        public bool force_enable { get; set; } = false;

        public MouseButtonBind(MouseButtons button, string[] binds) {
            this.binds.AddRange(binds);
            _button = button;
            this._recent_state = new bool[60];
        }

        public void force_state(digital_bind_state state) {
            _bind_state |= state;
        }
        bool _was_pressed = false;
        public void update_state(PlayerIndex player_index) {
            if (Controls.is_pressed(button)) {
                _bind_state = digital_bind_state.pressed;

                if (!_was_pressed) {
                    _bind_state |= digital_bind_state.just_pressed;

                    _pressed_at = DateTime.Now;
                }
                _time_pressed = DateTime.Now - _pressed_at;


                if (_time_pressed.TotalMilliseconds >= _hold_time) {
                    _bind_state |= digital_bind_state.held;

                    if (!_bind_state_prev.HasFlag(digital_bind_state.held))
                        _bind_state |= digital_bind_state.just_held;
                }

            } else {
                _bind_state = digital_bind_state.released;

                if (_was_pressed) {
                    _bind_state |= digital_bind_state.just_released;
                }

                _time_pressed = DateTime.Now - _pressed_at;
                bool h = _time_pressed.TotalMilliseconds >= _hold_time;

                _time_pressed = TimeSpan.Zero;

                if (h && just_released)
                    _bind_state |= digital_bind_state.released_hold;
                
                if (!h && just_released)
                    _bind_state |= digital_bind_state.tapped;
            }

            _bind_state_prev = _bind_state;

            for (int i = 0; i < _recent_state.Length - 1; i++) {
                _recent_state[i] = _recent_state[i + 1];
            }
            _recent_state[_recent_state.Length - 1] = is_pressed;
            _was_pressed = Controls.is_pressed(button);
        }

    }

    public class DigitalBinds {
        public List<IDigitalBind> binds => _binds;
        volatile List<IDigitalBind> _binds = new List<IDigitalBind>();

        bool _enabled = true;

        public void enable() { _enabled = true; }
        public void disable() { _enabled = false; }

        public DigitalBinds() { }

        public bool bind_exists(string bind) {            
            foreach (var s in _binds) {
                if (s.binds.Contains(bind)) return true;
            }
            return false;
        }

        public DigitalBinds(params (controller_type input_type, object button, string[] binds)[] bind_data) {
            foreach ((controller_type input_type, object button, string[] binds) bd in bind_data) {
                switch (bd.input_type) {
                    case controller_type.keyboard:
                        add_bind((Keys)bd.button, bd.binds);
                        break;
                    case controller_type.xinput:
                        add_bind((XInputButtons)bd.button, bd.binds);
                        break;
                    case controller_type.mouse:
                        add_bind((MouseButtons)bd.button, bd.binds);
                        break;
                }
            }
        }

        public void add_bind(Keys button, params string[] binds) {
            foreach (IDigitalBind b in _binds) {
                if (b.bind_type != controller_type.keyboard) continue;
                var cb = ((KeyboardBind)b);
                if (b.bind_type == controller_type.keyboard) {
                    if (cb.button == button) {
                        b.binds.AddRange(binds);
                        return;
                    }
                }
            }

            _binds.Add(new KeyboardBind(button, binds));
        }


        public void add_bind(XInputButtons button, params string[] binds) {
            foreach (IDigitalBind b in _binds) {
                if (b.bind_type != controller_type.xinput) continue;
                var cb = ((XInputBind)b);
                if (b.bind_type == controller_type.xinput) {
                    if (cb.button == button) {
                        b.binds.AddRange(binds);
                        return;
                    }
                }
            }

            _binds.Add(new XInputBind(button, binds));

        }

        public void add_bind(MouseButtons button, params string[] binds) {
            foreach (IDigitalBind b in _binds) {
                if (b.bind_type != controller_type.mouse) continue;
                var cb = ((MouseButtonBind)b);
                if (b.bind_type == controller_type.mouse) {
                    if (cb.button == button) {
                        b.binds.AddRange(binds);
                        return;
                    }
                }
            }

            _binds.Add(new MouseButtonBind(button, binds));

        }

        #region bind status functions
        public bool bind_enabled(IDigitalBind bind) {
            return bind.force_enable || _enabled;
        }

        public bool bind_pressed(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) && 
                    b.is_pressed && b.binds.Contains(bind))
                    return true;
            }

            return false;
        }
        public bool bind_pressed_ignore_enable(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (b.is_pressed && b.binds.Contains(bind))
                    return true;
            }

            return false;
        }
        public bool bind_released(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.is_pressed && b.binds.Contains(bind))
                    return false;
            }

            return true;
        }
                public double bind_pressed_time(string bind) {
            double hi = 0f;
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.held && b.binds.Contains(bind))
                    if (b.time_pressed.TotalMilliseconds > hi)
                        hi = b.time_pressed.TotalMilliseconds;
            }

            return hi;
        }

        public bool bind_just_pressed(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.just_pressed && b.binds.Contains(bind))
                    return true;
            }
            return false;
        }
        public bool bind_just_released(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.just_released && b.binds.Contains(bind))
                    return true;
            }

            return false;
        }

        public bool bind_held(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.held && b.binds.Contains(bind))
                    return true;
            }

            return false;
        }
        public bool bind_held(IDigitalBind b) {
            if (b.held)
                return true;            

            return false;
        }
        public bool bind_just_held(string bind) {

            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.just_held && b.binds.Contains(bind))
                    return true;
            }
            return false;
        }

        public double bind_held_time(string bind) {
            double hi = 0f;
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.held && b.binds.Contains(bind))
                    if (b.time_pressed.TotalMilliseconds - b.hold_time > hi)
                        hi = b.time_pressed.TotalMilliseconds - b.hold_time;
            }

            return hi;
        }

        public bool bind_just_released_hold(string bind) { 

            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.released_hold && b.binds.Contains(bind))
                    return true;
            }

            return false;
        }

        public bool bind_tapped(string bind) {
            foreach (IDigitalBind b in _binds) {
                if (//bind_enabled(bind) &&
                    b.tapped && b.binds.Contains(bind))
                    return true;
            }
            return false;
        }


        public int bind_count() {
            var c = 0;
            List<string> bi = new List<string>();
            foreach (IDigitalBind b in _binds) {
                foreach (string s in b.binds) {
                    if (!bi.Contains(s)) {
                        bi.Add(s);
                        c++;
                    }
                }
            }
            return c;
        }

        public int bind_buttons_pressed(string bind) {
            var c = 0;
            foreach (IDigitalBind b in _binds) {
                if (b.is_pressed && b.binds.Contains(bind)) c++;
            }
            return c;
        }
        #endregion

        public string list() {
            var s = "# binds #";
            foreach (IDigitalBind bind in _binds) {
                s += string.Format(
                        "\n - {0} :: {1} \n | pressed: {2} | pressed_time: {3} status: {4}",
                        bind.button_string,
                        bind.binds.Aggregate((a,b) =>  a + ", " + b),
                        bind.is_pressed,
                        bind.time_pressed.TotalMilliseconds.ToString(), 
                        (bind.held ? "held " : "") + (bind.tapped ? "tapped " : "") + (bind.released_hold ? "released_hold " : "")
                        );
            }

            s += "\n\n";
            var lb = list_binds();
            s += "# " + lb.Item2.ToString() + " binds total # \n" + lb.Item1.Aggregate((a,b) => a+", "+b);

            return s;
        }

        public string binds_pressed() {
            var s = "";

            foreach (string bind in list_binds().Item1) {
                s += bind + " : " + bind_pressed(bind).ToString().ToLower() + (bind_pressed(bind) ? ", " + bind_buttons_pressed(bind) + "\n" : "\n");
            }           

            return s;
        }

        (string[], int) list_binds() {
            var c = 0;
            List<string> bi = new List<string>();
            foreach (IDigitalBind b in _binds) {
                foreach(string s in b.binds) {
                    if (!bi.Contains(s)) {
                        bi.Add(s);
                        c++;
                    }
                }
            }
            return (bi.ToArray(), c);
        }


        public void update(PlayerIndex player_index) {
            //lock (_binds) { 
                foreach (IDigitalBind b in _binds) {
                    b.update_state(player_index);
                }
            //}
        }

    }
}