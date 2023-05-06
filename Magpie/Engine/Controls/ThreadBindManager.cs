using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Magpie.Engine {
    class local_bind {
        //add playerindex system at some point, have static binds in enginestate
        //and whichever playerindex is selected here is also checked (alongside static control binds)
        PlayerIndex player_index = PlayerIndex.One;

        string bind;

        public bool pressed = false;
        public bool was_pressed = false;

        public bool just_pressed => pressed && !was_pressed;
        public bool just_released => !pressed && was_pressed;

        public bool held => StaticControlBinds.held(bind) || (parent.using_custom_binds && parent.custom_control_binds.held(bind));
        public bool was_held => _was_held; bool _was_held = false;

        public bool just_held => held && !was_held;

        double cht,scht;
        public double held_time {
            get {
                cht = 0; scht = 0;
                if (parent.using_custom_binds) cht = parent.custom_control_binds.held_time(bind);
                scht = StaticControlBinds.held_time(bind);
                if (scht >= cht) return scht;
                else return cht;
            }
        }

        double cpt, scpt;
        public double pressed_time {
            get {
                cpt = 0; scpt = 0;
                if (parent.using_custom_binds) cpt = parent.custom_control_binds.pressed_time(bind);
                scpt = StaticControlBinds.pressed_time(bind);
                if (scpt >= cpt) return scpt;
                else return cpt;
            }
        }

        ThreadBindManager parent;
        bool static_control_bind = false;
        public local_bind(string bind, ThreadBindManager parent, bool staticCB) { 
            this.bind = bind; this.parent = parent; this.static_control_bind = staticCB;
        }

        public local_bind() { }
        public local_bind(PlayerIndex player_index) { this.player_index = player_index; }

        public void update() {
            was_pressed = pressed; pressed = false;
            if (static_control_bind)
                pressed = pressed | StaticControlBinds.pressed(bind);
            else 
                pressed = pressed | parent.custom_control_binds.pressed(bind); 
            _was_held = held;                  
        }
    }

    public class ThreadBindManager {
        Dictionary<string, local_bind> binds = new Dictionary<string, local_bind>();
        internal ControlBinds custom_control_binds = null;

        internal bool using_custom_binds = false;

        public ThreadBindManager() { }
        public ThreadBindManager(ControlBinds custom_control_binds) {

            this.custom_control_binds = custom_control_binds;
            using_custom_binds = true;

        }

        public string info() {
            StringBuilder sb = new StringBuilder();
            foreach (var k in binds.Keys)
                sb.AppendLine($"{k}: {binds[k].pressed} {binds[k].just_pressed} {binds[k].pressed_time.ToString()}");

            return sb.ToString();
        }

        public void update() {
            if (using_custom_binds) custom_control_binds.update();
            foreach (string key in binds.Keys) {
                binds[key].update();
            }
        }
        void maybe_new_bind(string bind) {
            if (!binds.ContainsKey(bind)) {
                if (StaticControlBinds.bind_exists(bind)) 
                    binds.Add(bind, new local_bind(bind, this, true));                
                else if (using_custom_binds && custom_control_binds.bind_exists(bind)) 
                    binds.Add(bind, new local_bind(bind, this, false));
                
            }
        }
        public double pressed_time(string bind) {
            maybe_new_bind(bind);
            return binds[bind].pressed_time;
        }
        public double held_time(string bind) {
            maybe_new_bind(bind);
            return binds[bind].held_time;
        }

        public bool pressed(string bind) {
            maybe_new_bind(bind);
            return binds[bind].pressed;
        }

        public bool just_pressed(string bind) {
            maybe_new_bind(bind);
            return binds[bind].just_pressed;
        }
        public bool just_released(string bind) {
            maybe_new_bind(bind);            
            return binds[bind].just_released;
        }

    }
}