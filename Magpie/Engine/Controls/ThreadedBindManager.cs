using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace Magpie.Engine {
    class thread_bind {
        //add playerindex system at some point, have static binds in enginestate
        //and whichever playerindex is selected here is also checked (alongside static control binds)
        PlayerIndex player_index = PlayerIndex.One;

        string bind;

        public bool pressed = false;
        public bool was_pressed = false;

        public bool just_pressed => pressed && !was_pressed;
        public bool just_released => !pressed && was_pressed;

        public bool held => StaticControlBinds.held(bind);
        public bool was_held => _was_held; bool _was_held = false;

        public bool just_held => held && !was_held;

        public double held_time => StaticControlBinds.held_time(bind);
        public double pressed_time => StaticControlBinds.pressed_time(bind);

        public thread_bind(string bind) { this.bind = bind; }

        public thread_bind() { }
        public thread_bind(PlayerIndex player_index) { this.player_index = player_index; }

        public void update() {
            was_pressed = pressed;
            _was_held = held;
            pressed = StaticControlBinds.pressed(bind);                        
        }
    }

    public class ThreadedBindManager {
        Dictionary<string, thread_bind> binds = new Dictionary<string, thread_bind>();  
        
        public void update() {
            foreach (string key in binds.Keys) {
                binds[key].update();
            }
        }
        void maybe_new_bind(string bind) {
            if (!binds.ContainsKey(bind)) {
                if (StaticControlBinds.bind_exists(bind)) {
                    binds.Add(bind, new thread_bind(bind));
                }
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
