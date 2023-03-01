using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    class thread_bind {
        public bool pressed = false;
        public bool was_pressed = false;
        public bool just_pressed = false;
        public bool just_released = false;

        string bind;

        public thread_bind(string bind) { this.bind = bind; }

        public void update() {
            was_pressed = pressed;
            pressed = StaticControlBinds.pressed(bind);

            just_pressed
                = !was_pressed && pressed;

           just_released
                = was_pressed && !pressed;
        }
    }

    public class ThreadedBindManager {
        Dictionary<string, thread_bind> binds = new Dictionary<string, thread_bind>();  
        
        public void update() {
            foreach (string key in binds.Keys) {
                binds[key].update();
            }
        }

        public bool pressed(string bind) {
            if (!binds.ContainsKey(bind)) {
                if (StaticControlBinds.bind_exists(bind)) {
                    binds.Add(bind, new thread_bind(bind));
                }
            }

            return binds[bind].pressed;
        }

        public bool just_pressed(string bind) {
            if (!binds.ContainsKey(bind)) {
                if (StaticControlBinds.bind_exists(bind)) {
                    binds.Add(bind, new thread_bind(bind));
                }
            }

            return binds[bind].just_pressed;
        }
        public bool just_released(string bind) {
            if (!binds.ContainsKey(bind)) {
                if (StaticControlBinds.bind_exists(bind)) {
                    binds.Add(bind, new thread_bind(bind));
                }
            }

            return binds[bind].just_released;
        }

    }
}
