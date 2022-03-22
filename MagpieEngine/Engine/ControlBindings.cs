using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Controls;

namespace Magpie.Engine {
    public static class AnalogControlBindings {
        // so this is a lot more complicated than the digital status, for a weird reason
        // this is designed as being able to return either one or two values at once,
        // depending on the IAnalogBind and what the status is being returned from
        //
        // some of the IAnalogBinds I've put together below use stuff like the XInputStick
        // enum, or are designed to map two axes at once
        public enum bind_status_type {
            X,
            XY
        }
        public class analog_bind_status {
            float[] d;
            public float[] data => d;
            int data_count = 0;
            public int count => data_count;

            Vector2 v2;
            Vector2 vector2 => v2;

            bind_status_type t = bind_status_type.X;
            bind_status_type type => t;

            public analog_bind_status() {
                this.d = new float[1] { 0f };
                this.data_count = d.Length;

                v2 = Vector2.Zero;

                updatev();
            }

            public analog_bind_status(float[] data) {
                this.d = data;
                this.data_count = data.Length;

                v2 = Vector2.Zero;

                updatev();
            }

            public void update_values(float X) {
                this.d[0] = X;

                v2 = Vector2.Zero;

                updatev();
            }

            public void update_values(float X, float Y) {
                if (type == bind_status_type.X) return;
                this.d[0] = X;
                this.d[1] = Y;

                this.data_count = data.Length;

                v2 = Vector2.Zero;

                updatev();
            }


            public void update_values(float[] data) {
                if (data.Length > d.Length) return;

                this.d = data;
                this.data_count = data.Length;

                v2 = Vector2.Zero;

                updatev();
            }

            void updatev() {
                v2 = Vector2.Zero;

                if (d.Length == 1) {
                    t = bind_status_type.X;
                    v2.X = d[0];
                } else {
                    t = bind_status_type.XY;
                    v2.X = d[0];
                    v2.Y = d[1];
                }
            }
        }

        public static readonly int max_binds_per_axis = 8;

        public interface IAnalogBind {
            float deadzone { get; set; }
            string[] binds { get; }
            controller_type type { get; }
            analog_bind_status status { get; }
            void update();
        }
        
        static Dictionary<string, analog_bind_status> status = new Dictionary<string, analog_bind_status>();
        public static Dictionary<string, analog_bind_status> axis_status => status;

        public static List<IAnalogBind> analog_binds = new List<IAnalogBind>();

        public static float[] get_bind_status(string bind) {
            return new float[1] { 0f };
        }

        public static void update() {
            status.Clear();

            for (int i = 0; i < analog_binds.Count; i++) {
                analog_bind_status f = analog_binds[i].status;
                for (int b = 0; b < analog_binds[i].binds.Length; b++) {
                    if (!String.IsNullOrEmpty(analog_binds[i].binds[b])) {
                        status.Add(analog_binds[i].binds[b], f);
                    }
                }
            }
        }


        public static void add_bind() {

        }

        public static void is_bound() {

        }

        public static void is_bound(string bind) {

        }
        public static void add_bind_to_axis(string bind) {


        }



        public static void add_bind(MouseAxisBind bind) {

        }

        public static void is_bound(MouseAxis axis) {

        }

        public static void is_bound(MouseAxis axis, string bind) {

        }

        public static void add_bind_to_axis(MouseAxis axis, string bind) {

        }
        /*

        public static void add_bind(XInputButtonBind xi_bind) {
            if (!is_bound(xi_bind.button)) {
                digital_binds.Add(xi_bind);
            }
        }

        #region is_bound tests
        //KEYBOARD
        public static bool is_bound(Keys key) {
            for (int i = 0; i < digital_binds.Count; i++) {
                if (digital_binds[i].type == controller_type.keyboard) {
                    if (((KeyBind)digital_binds[i]).key == key)
                        return true;
                }
            }
            return false;
        }
        public static bool is_bound(MouseButtons button, string bind) {
            for (int i = 0; i < digital_binds.Count; i++) {
                if (digital_binds[i].type == controller_type.mouse) {
                    if (((MouseButtonBind)digital_binds[i]).button == button) { 
                        if (digital_binds[i].binds.Contains(bind))
                            return true;
                        else return false;
                    }
                }
            }
            return false;
        }
        public static void add_bind_to_key(Keys key, string bind) {
            for (int i = 0; i < digital_binds.Count; i++) {

                if (digital_binds[i].type == controller_type.keyboard && ((KeyBind)digital_binds[i]).key == key ) {

                    for (int b = 0; b < digital_binds[i].binds.Length; b++) {
                        if (string.IsNullOrEmpty(digital_binds[i].binds[b])) {
                            digital_binds[i].binds[b] = bind;
                        }
                    }
                }
            }
        }
        */







        /*
        public class KeyBind : IDigitalBind {
            public controller_type type => controller_type.keyboard;

            Keys k;
            public Keys key => k;

            public digital_bind_result status => determine_status(is_pressed(k), was_pressed(k));

            public KeyBind(Keys key) {
                this.k = key;
                b = new string[max_binds_per_button];
            }

            public KeyBind(Keys key, params string[] initial_binds) {
                this.k = key;
                b = new string[max_binds_per_button];

                int c = 0;
                if (initial_binds.Length > max_binds_per_button)
                    c = max_binds_per_button;
                else
                    c = initial_binds.Length;

                for (int i = 0; i < c; i++) {
                    b[i] = initial_binds[i];
                }
            }

            string[] b;
            public string[] binds => b;
        }
        */

        public class MouseAxisBind : IAnalogBind {
            public controller_type type => controller_type.mouse;

            MouseAxis ma;
            public MouseAxis axis => ma;

            string[] b = new string[max_binds_per_axis];
            public string[] binds => b;

            analog_bind_status s;
            public analog_bind_status status => s;
            
            public float deadzone { get; set; } = 0f;
            
            public MouseAxisBind(MouseAxis axis, params string[] initial_binds) {
                ma = axis;
                b = new string[max_binds_per_axis];

                int c = 0;
                if (initial_binds.Length > max_binds_per_axis)
                    c = max_binds_per_axis;
                else
                    c = initial_binds.Length;

                for (int i = 0; i < c; i++) {
                    b[i] = initial_binds[i];
                }

                update();
            }

            public void update() {
                s.update_values(Controls.get_axis(ma));
            }
        }

        public class XInputAxisBind {

        }

        public class XInputStickBind {

        }

        public class TwoKeyFakeAxisBind {

        }
        
        
    }

    public static class DigitalControlBindings {
        public enum digital_bind_result {
            released,
            just_released,
            just_pressed,
            pressed
        }

        public enum special_action_status { 
            none,            
            hold_start,
            held,
            tap   
        }

        static int max_binds_per_button = 16;
        static List<IDigitalBind> _digital_binds = new List<IDigitalBind>();

        static List<string> disabled_binds = new List<string>();

        public static bool bind_disabled(string bind) => disabled_binds.Contains(bind);

        public static void enable_bind(string bind) { if (bind_disabled(bind)) disabled_binds.Remove(bind); }
        public static void enable_binds(params string[] binds) { foreach (string b in binds) { if (bind_disabled(b)) disabled_binds.Remove(b); } }
        public static void disable_bind(string bind) { if (!bind_disabled(bind)) disabled_binds.Add(bind); }
        public static void disable_binds(params string[] binds) { foreach (string b in binds) { if (!bind_disabled(b)) disabled_binds.Add(b); } }

        public static void enable_all_binds() { disabled_binds.Clear(); }
        public static void disable_all_binds() { foreach (IDigitalBind b in _digital_binds) disable_binds(b.binds); }

        static Dictionary<string, bind_state> status = new Dictionary<string, bind_state>();
        public static Dictionary<string, bind_state> button_bind_status => status;

        public static List<IDigitalBind> digital_binds { get => _digital_binds; set => _digital_binds = value; }


        public static digital_bind_result get_bind_result(string bind) {
            if (status.ContainsKey(bind)) {
                return button_bind_status[bind].result;

            } //else throw new Exception("invalid bind name: " + bind);
            else return digital_bind_result.released;
        }
        public static special_action_status get_bind_special(string bind) {
            if (status.ContainsKey(bind)) {
                return button_bind_status[bind].special;
            } //else throw new Exception("invalid bind name: " + bind);
            else return special_action_status.none;
        }

        

        public static bool bind_pressed(string bind) =>       (!bind_disabled(bind)) && (get_bind_result(bind) == digital_bind_result.pressed || get_bind_result(bind) == digital_bind_result.just_pressed);
        public static bool bind_just_held(string bind) =>     (!bind_disabled(bind)) && (get_bind_special(bind) == special_action_status.hold_start);
        public static bool bind_held(string bind) =>          (!bind_disabled(bind)) && (get_bind_special(bind) == special_action_status.held);
        public static bool bind_tapped(string bind) =>        (!bind_disabled(bind)) && (get_bind_special(bind) == special_action_status.tap);
        public static bool bind_just_pressed(string bind) =>  (!bind_disabled(bind)) && (get_bind_result(bind) == digital_bind_result.just_pressed);
        public static bool bind_released(string bind) =>      (!bind_disabled(bind)) && (get_bind_result(bind) == digital_bind_result.released || get_bind_result(bind) == digital_bind_result.just_released);
        public static bool bind_just_released(string bind) => (!bind_disabled(bind)) && (get_bind_result(bind) == digital_bind_result.just_released);

        public static void update() {
            status.Clear();
            
            for (int i = 0; i < _digital_binds.Count; i++) {
                _digital_binds[i].update();

                bind_state r = _digital_binds[i].state;

                for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                    if (!String.IsNullOrEmpty(_digital_binds[i].binds[b]))
                        status.Add(_digital_binds[i].binds[b], r);                        
                }

            }
        }

        public static Keys get_bind_key(string bind) {
            foreach(IDigitalBind b in digital_binds) {
                if (b.type == controller_type.keyboard && b.binds.Contains(bind)) {
                    return ((KeyBind)b).key;
                }
            }
            return Keys.None;
        }

        public static string list_binds() {
            string st = "";
            foreach (string s in status.Keys)
                st += s + "\n";
            return st;
        }
        public static string list_disabled_binds() {
            string st = "";
            foreach (string s in disabled_binds)
                st += s + "\n";
            return st;
        }
        public static string list_binds_w_status() {
            string st = "";
            foreach (string s in status.Keys) {
                st += s + " " + status[s].ToString() + "\n";
            }
            return st;
        }
        public static string list_active_binds_w_status() {
            string st = "binds:\n";
           // foreach (IDigitalBind db in digital_binds) {
           foreach (string s in status.Keys) {
                if (status[s].result != digital_bind_result.released) {
                    st += s + " " + status[s].result + " " + status[s].special.ToString() + " " + (disabled_binds.Contains(s) ? "[disabled]" : "") + "\n";
                }
            }
            return st;
        }

        public static int count_active_binds() {
            // foreach (IDigitalBind db in digital_binds) {
            int c = 0;
            foreach (string s in status.Keys) {
                if (status[s].result != digital_bind_result.released) {
                    c++;
                }
            }
            return c;
        }

        //ADD BINDS
        public static void add_bind(KeyBind key_bind) {
            if (!is_bound(key_bind.key)) {
                //key not bound, easy mode, just add the new bind and you're off
                _digital_binds.Add(key_bind);
                
            } else {
                //key is already bound, so we wanna add any new binds to it
                //iterate through the binds on the new keybind
                for (int i = 0; i < key_bind.binds.Length; i++) {
                    //if the new bind isn't found on the current key already
                    if (!is_bound(key_bind.key, key_bind.binds[i])) {
                        //and it's not whitespace, empty or null, then add the bind to the existing key
                        if (!string.IsNullOrWhiteSpace(key_bind.binds[i])) {
                            add_bind_to_key(key_bind.key, key_bind.binds[i]);
                        }
                        //same shit different sounding flush below
                    }
                }                
            } 
        }

        public static void add_bind(MouseButtonBind m_bind) {
            if (!is_bound(m_bind.button)) {
                _digital_binds.Add(m_bind);
            } else {
                for (int i = 0; i < m_bind.binds.Length; i++) {
                    if (!is_bound(m_bind.button, m_bind.binds[i])) {
                        if (!string.IsNullOrWhiteSpace(m_bind.binds[i])) {
                            add_bind_to_mouse_button(m_bind.button, m_bind.binds[i]);
                        }
                    }
                }
            }
        }

        public static void add_bind(XInputButtonBind xi_bind) {
            if (!is_bound(xi_bind.button)) {
                _digital_binds.Add(xi_bind);
            } else {
                for (int i = 0; i < xi_bind.binds.Length; i++) {
                    if (!is_bound(xi_bind.button, xi_bind.binds[i])) {
                        if (!string.IsNullOrWhiteSpace(xi_bind.binds[i])) {
                            add_bind_to_xinput_button(xi_bind.button, xi_bind.binds[i]);
                        }
                    }
                }
            }
        }

        #region is_bound tests
        //KEYBOARD
        public static bool is_bound(string bind_string) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                for (int c = 0; c < _digital_binds[i].binds.Length; c++) {
                    if (bind_string == _digital_binds[i].binds[c]) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool is_bound(Keys key) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.keyboard) {
                    if (((KeyBind)_digital_binds[i]).key == key)
                        return true;
                }
            }
            return false;
        }
        public static bool is_bound(Keys key, out int index) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.keyboard) {
                    if (((KeyBind)_digital_binds[i]).key == key) {
                        index = i;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }
        public static bool is_bound(Keys key, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.keyboard) {
                    if (((KeyBind)_digital_binds[i]).key == key) {

                        if (_digital_binds[i].binds.Contains(bind))
                            return true;

                        else return false;
                    }
                }
            }
            return false;

        }


        //MOUSE
        public static bool is_bound(MouseButtons button) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.mouse) {
                    if (((MouseButtonBind)_digital_binds[i]).button == button)
                        return true;
                }
            }
            return false;
        }

        public static bool is_bound(MouseButtons button, out int index) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.mouse) {
                    if (((MouseButtonBind)_digital_binds[i]).button == button) {
                        index = i;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }


        public static bool is_bound(MouseButtons button, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.mouse) {
                    if (((MouseButtonBind)_digital_binds[i]).button == button) { 
                        if (_digital_binds[i].binds.Contains(bind))
                            return true;
                        else return false;
                    }
                }
            }
            return false;
        }


        //XINPUT
        public static bool is_bound(XInputButtons button) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.xinput) {
                    if (((XInputButtonBind)_digital_binds[i]).button == button)
                        return true;
                }
            }
            return false;
        }
        public static bool is_bound(XInputButtons button, out int index) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.xinput) {
                    if (((XInputButtonBind)_digital_binds[i]).button == button) {
                        index = i;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }
        public static bool is_bound(XInputButtons button, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {
                if (_digital_binds[i].type == controller_type.xinput) {
                    if (((XInputButtonBind)_digital_binds[i]).button == button) { 
                        if (_digital_binds[i].binds.Contains(bind))
                            return true;
                        else return false;
                    }
                }
            }
            return false;
        }
        #endregion

        #region add/remove digital
        public static void add_bind_to_key(Keys key, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.keyboard && ((KeyBind)_digital_binds[i]).key == key ) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (string.IsNullOrWhiteSpace(_digital_binds[i].binds[b])) {
                            _digital_binds[i].binds[b] = bind;
                        }
                    }
                }
            }
        }
        

        public static void add_bind_to_mouse_button(MouseButtons mb, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.mouse && ((MouseButtonBind)_digital_binds[i]).button == mb) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (string.IsNullOrWhiteSpace(_digital_binds[i].binds[b])) {
                            _digital_binds[i].binds[b] = bind;
                        }
                    }
                }
            }
        }

        public static void add_bind_to_xinput_button(XInputButtons button, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.xinput && ((XInputButtonBind)_digital_binds[i]).button == button) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (string.IsNullOrWhiteSpace(_digital_binds[i].binds[b])) {
                            _digital_binds[i].binds[b] = bind;
                        }
                    }
                }
            }
        }

        public static void remove_bind_from_key(Keys key, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.keyboard && ((KeyBind)_digital_binds[i]).key == key) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (_digital_binds[i].binds[b] == bind) {
                            _digital_binds[i].binds[b] = "";
                        }
                    }
                }
            }
        }


        public static void remove_bind_from_mouse_button(MouseButtons mb, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.mouse && ((MouseButtonBind)_digital_binds[i]).button == mb) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (_digital_binds[i].binds[b] == bind) {
                            _digital_binds[i].binds[b] = "";
                        }
                    }
                }
            }
        }

        public static void remove_bind_from_xinput_button(XInputButtons button, string bind) {
            for (int i = 0; i < _digital_binds.Count; i++) {

                if (_digital_binds[i].type == controller_type.xinput && ((XInputButtonBind)_digital_binds[i]).button == button) {

                    for (int b = 0; b < _digital_binds[i].binds.Length; b++) {
                        if (_digital_binds[i].binds[b] == bind) {
                            _digital_binds[i].binds[b] = "";
                        }
                    }
                }
            }
        }
        
        #endregion

        public struct bind_state {
            public digital_bind_result result;
            public special_action_status special;
        }

        public interface IDigitalBind {
            controller_type type { get; }
            string[] binds { get; set; }
            //string requires_other_bind { get; set; }
            bind_state state { get; }
            hold_status hold_state { get; }
            void update();
        }

        public class hold_status {
            special_action_status s_status = special_action_status.none;
            public special_action_status special_status => s_status;

            public double hold_delay { get; set; } = 300;

            double ht = 0; public double hold_time => ht;
            double hs = 0; public double hold_start => hs;
            bool hb = false;

            //this is the heart of the hold/tap special status system            
            public void update_timer(digital_bind_result status) {
                //we default s_status to nothing
                s_status = special_action_status.none;

                //if we've just pressed the button, set hs to the current uptime in ms
                if (status == digital_bind_result.just_pressed) {
                    hs = Clock.total_ms;

                //if we've been holding the button for more than a frame
                } else if (status == digital_bind_result.pressed) {
                    //update ht to be the difference between the start time and the current uptime
                    //this is how long the button has been held
                    ht = Clock.total_ms - hs;

                    //if we're holding and haven't released the button yet
                    if (hb) {
                        s_status = special_action_status.held;
                    }
                    //if we've been holding the button for longer than hold_delay, 
                    //set s_status to hold, set hb to true and reset the timer
                    //hb is here to make sure that the hold signal only triggers once
                    if (ht > hold_delay && !hb) {
                         s_status = special_action_status.hold_start;
                         hb = true;
                         ht = 0; hs = 0;
                    }


                //button was just released, if we haven't been holding it long enough to trigger the above hold state                
                } else if (status == digital_bind_result.just_released) {
                    //then we consider this to be a tap and send a tap status
                    if (ht < hold_delay && s_status != special_action_status.hold_start) {
                        s_status = special_action_status.tap;                        
                    }
                    //otherwise just reset everything
                    ht = 0; hs = 0; hb = false;

                //once the key is fully released, we default the status to none
                } else if (status == digital_bind_result.released) {
                        s_status = special_action_status.none;
                    
                }

            }
        }


        public static digital_bind_result determine_status(bool pressed, bool was_pressed) {
            if (pressed && was_pressed) return digital_bind_result.pressed;
            else if (pressed && !was_pressed) return digital_bind_result.just_pressed;
            else if (!pressed && was_pressed) return digital_bind_result.just_released;
            else if (!pressed && !was_pressed) return digital_bind_result.released;
            else return digital_bind_result.released;
        }


        public class KeyBind : IDigitalBind {
            public controller_type type { get; } = controller_type.keyboard;
            Keys k; public Keys key => k;

            //public string requires_other_bind { get; set; } = "";
            //public bool requires_other_bind_held = false;

            public string[] binds { get; set; }

            bind_state s;
            public bind_state state => s;

            hold_status h = new hold_status();
            public hold_status hold_state => h;

            public void update() {
                //this can be reenabled to allow for binds that only turn on if another bind is held
                //however, it's very ambiguous in practice (which binds have this enabled at any given time????)
                //and also it poses a problem: since binds are implemented as a dictionary
                //by their BUTTON, that means it's either or, you can have either F12 or Ctrl F12, but not both
                //making the end user class test two binds and do exactly what is needed is better

                /*if (!string.IsNullOrWhiteSpace(requires_other_bind) && is_bound(requires_other_bind)) {
                    if (DigitalControlBindings.get_bind_result(requires_other_bind) == digital_bind_result.pressed) {
                        s.result = determine_status(is_pressed(k), was_pressed(k));
                        
                        s.special = h.special_status;
                        h.update_timer(state.result);
                    } else {
                        s.result = digital_bind_result.released;
                        s.special = special_action_status.none;
                        h.update_timer(state.result);
                    }
                } else {*/
                    s.result = determine_status(is_pressed(k), was_pressed(k));
                    s.special = h.special_status;
                    h.update_timer(state.result);
                //}
            }

            public KeyBind(Keys key) {
                this.k = key;
                binds = new string[max_binds_per_button];

                s = new bind_state() { result = determine_status(is_pressed(k), was_pressed(k)), special = h.special_status };
            }

            public KeyBind(Keys key, params string[] initial_binds) {
                this.k = key;
                binds = new string[max_binds_per_button];

                int c = 0;
                if (initial_binds.Length > max_binds_per_button)
                    c = max_binds_per_button;
                else
                    c = initial_binds.Length;

                for (int i = 0; i < c; i++) {
                    binds[i] = initial_binds[i];
                }

                s = new bind_state() { result = determine_status(is_pressed(k), was_pressed(k)), special = h.special_status };
            }


        }

        public class MouseButtonBind : IDigitalBind {
            public controller_type type => controller_type.mouse;

            MouseButtons mb;
            public MouseButtons button => mb;

            //public string requires_other_bind { get; set; } = "";
            //public bool requires_other_bind_held = false;

            public string[] binds { get; set; }

            bind_state s;
            public bind_state state => s;

            hold_status h = new hold_status();
            public hold_status hold_state => h;

            public void update() {
                /*if (!string.IsNullOrWhiteSpace(requires_other_bind) && is_bound(requires_other_bind)) {
                    if (DigitalControlBindings.get_bind_result(requires_other_bind) == digital_bind_result.pressed) {
                        s.result = determine_status(is_pressed(mb), was_pressed(mb));
                        s.special = h.special_status;

                        if (!(mb == MouseButtons.ScrollDown || mb == MouseButtons.ScrollUp))
                            h.update_timer(state.result);
                    } else {
                        s.result = digital_bind_result.released;
                        s.special = special_action_status.none;

                        if (!(mb == MouseButtons.ScrollDown || mb == MouseButtons.ScrollUp))
                            h.update_timer(state.result);
                    }
                } else {*/
                        s.result = determine_status(is_pressed(mb), was_pressed(mb));
                        s.special = h.special_status;

                        if (!(mb == MouseButtons.ScrollDown || mb == MouseButtons.ScrollUp))
                            h.update_timer(state.result);
                //}
                
            }

            public MouseButtonBind(MouseButtons button) {
                this.mb = button;
            }

            public MouseButtonBind(MouseButtons button, params string[] initial_binds) {
                this.mb = button;
                binds = new string[max_binds_per_button];

                int c = 0;
                if (initial_binds.Length > max_binds_per_button)
                    c = max_binds_per_button;
                else
                    c = initial_binds.Length;

                for (int i = 0; i < c; i++) {
                    binds[i] = initial_binds[i];
                }
            }
        }

        public class XInputButtonBind : IDigitalBind {
            public controller_type type => controller_type.xinput;

            XInputButtons xb;
            public XInputButtons button => xb;
            PlayerIndex player = PlayerIndex.One;

            //public string requires_other_bind { get; set; } = "";
            //public bool requires_other_bind_held = false;

            public string[] binds { get; set; }

            bind_state s;
            public bind_state state => s;

            hold_status h = new hold_status();
            public hold_status hold_state => h;

            public void update() {
                /*if (!string.IsNullOrWhiteSpace(requires_other_bind) && is_bound(requires_other_bind)) {
                    if (DigitalControlBindings.get_bind_result(requires_other_bind) == digital_bind_result.pressed) {
                        s.result = determine_status(is_pressed(xb, player), was_pressed(xb, player));
                        s.special = h.special_status;
                        h.update_timer(state.result);
                    } else {
                        s.result = digital_bind_result.released;
                        s.special = special_action_status.none;
                        h.update_timer(state.result);
                    }
                } else {*/
                    s.result = determine_status(is_pressed(xb, player), was_pressed(xb, player));
                    s.special = h.special_status;
                    h.update_timer(state.result);
                //}
            }

            public XInputButtonBind(XInputButtons button) {
                this.xb = button;
            }

            public XInputButtonBind(XInputButtons button, params string[] initial_binds) {
                this.xb = button;
                binds = new string[max_binds_per_button];

                int c = 0;
                if (initial_binds.Length > max_binds_per_button)
                    c = max_binds_per_button - 1;
                else
                    c = initial_binds.Length - 1;

                for (int i = 0; i < c; i++) {
                    binds[i] = initial_binds[i];
                }
            }
        }
    }
}
