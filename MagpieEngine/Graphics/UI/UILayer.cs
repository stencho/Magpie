//#define GJK

using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;
using static Magpie.Engine.Controls;
using static Magpie.Engine.DigitalControlBindings;

namespace Magpie.Graphics.UI {
    public class UILayer {
        public Dictionary<string, UIForm> forms { get; internal set; } = new Dictionary<string, UIForm>();

        XYPair resolution;

        string[] window_layers;
        string[] layers_tmp;

        void move_to_top(string window_name) {
            /*
            if (window_layers.Length < 1) return;
            bool layer_hit = false;
            // if (window_layers.Contains(window_name)) {
            layers_tmp = new string[window_layers.Length];
            layers_tmp[0] = window_name;
            for (int i = 0; i < window_layers.Length - 1; i++) {
                ((UIWindow)forms[window_layers[i]]).selected = false;
                if (window_layers[i] == window_name) {
                    layer_hit = true;
                }
                layers_tmp[i + 1] = window_layers[(layer_hit ? i + 1 : i)];
            }


            window_layers = layers_tmp;
            ((UIWindow)forms[window_layers[0]]).selected = true;
            */
        }


        public bool hide { get; set; } = false;


        public UILayer(XYPair resolution) {
            this.resolution = resolution;
            window_layers = new string[0];
        }

        public void find_window() {

        }
        private void form_layers() {

        }
        private void move_to_top() {

        }

        private void window_count() {

        }

        public void add_form(string form_name, UIForm viewer) {
            viewer.name = form_name;
            forms.Add(form_name, viewer);
        }/*
        public void add_form(string form_name, UIMapViewer viewer) {
            viewer.name = form_name;
            forms.Add(form_name, viewer);
        }
        public void add_form(string form_name, UIButton button) {
            button.name = form_name;
            forms.Add(form_name, button);
        }
        public void add_form(string form_name, UILabel label) {
            label.name = form_name;
            forms.Add(form_name, label);
        }
        public void add_form(string form_name, UIPanel panel) {
            panel.name = form_name;
            forms.Add(form_name, panel);
        }
        public void add_form(string form_name, UIProgressSlider slider) {
            slider.name = form_name;
            forms.Add(form_name, slider);
        }*/
        
        public void add_form(string form_name, UIWindow bonetree) {
            bonetree.name = form_name;
            forms.Add(form_name, bonetree);

            Array.Resize(ref window_layers, window_layers.Length + 1);

            window_layers[window_layers.Length - 1] = form_name;
            move_to_top(form_name);
        }
        /*
        public void delete_form(string form_name) {
            int i = 0;
            int index = 
            for (i=0; i < window_layers.Length; i++) {
                if (window_layers[i] == form_name) {

                }
            }

            Array.Resize(ref window_layers, window_layers.Length + 1);
            window_layers[window_layers.Length - 1] = form_name;

            forms.Remove(form_name);
        }
        */

        public void show_form(string form_name) {
            forms[form_name].hidden = false;

            //Array.Resize(ref window_layers, window_layers.Length + 1);

            //window_layers[window_layers.Length - 1] = form_name;
            //move_to_top(form_name);
        }

        public void hide_form(string form_name) {
            forms[form_name].hidden = true;
            /*
            int k = 0;
            foreach (string f in window_layers) {
                if (f == form_name) {
                    window_layers[k] = null;
                    for (int i = 0; i < (window_layers.Length - k) - 1; i++) {
                        window_layers[k + i] = window_layers[k + i + 1];
                    }
                    break;
                }
                k++;
            }

            Array.Resize(ref window_layers, window_layers.Length - 1);

            window_layers[window_layers.Length - 1] = form_name;
            */
        }

        public void toggle_hidden(string form_name) {
            forms[form_name].hidden = !forms[form_name].hidden;
        }

        public void set_window_foreground_3d_delegate(string form_name, ui_delegate r_delegate) {
            if (forms.ContainsKey(form_name) && (forms[form_name].type == form_type.WINDOW))
                ((UIWindow)forms[form_name]).draw_foreground_3D_delegate = r_delegate;
        }

        public void set_window_foreground_2d_delegate(string form_name, ui_delegate r_delegate) {
            if (forms.ContainsKey(form_name) && (forms[form_name].type == form_type.WINDOW))
                ((UIWindow)forms[form_name]).draw_foreground_2D_delegate = r_delegate;
        }

        public void set_window_background_3d_delegate(string form_name, ui_delegate r_delegate) {
            if (forms.ContainsKey(form_name) && (forms[form_name].type == form_type.WINDOW))
                ((UIWindow)forms[form_name]).draw_background_3D_delegate = r_delegate;
        }

        public void set_window_background_2d_delegate(string form_name, ui_delegate r_delegate) {
            if (forms.ContainsKey(form_name) && (forms[form_name].type == form_type.WINDOW))
                ((UIWindow)forms[form_name]).draw_background_2D_delegate = r_delegate;
        }



        public void set_click_delegate(string form_name, string collision_name, ui_delegate cdelegate) {
            forms[form_name].collisions[collision_name].click = cdelegate;
        }

        bool mouse_down_left = false;
        bool mouse_down_right = false;

        bool mouse_down_start_left = false;
        bool mouse_down_start_right = false;

        bool mouse_down_end_left = false;
        bool mouse_down_end_right = false;

        bool mouse_scroll_up = false;
        bool mouse_scroll_down = false;

        public string focused_form_name {
            get {
                if (window_layers != null) {
                    foreach (string f in window_layers) {
                        if (!string.IsNullOrWhiteSpace(f)) {
                            return f;
                        }
                    }
                }
                return "";
            }
        }
        

        public string list_forms() {
            string s = "";
            //foreach (string f in forms.Keys)
            //s += f + " [" + forms[f].type + "]\n";                
            s += "  single forms\n";
            if (floating_forms != null) 
                foreach (string f in floating_forms)
                    if (!string.IsNullOrWhiteSpace(f))
                        s += "    " + f + " [" + forms[f].type + "]\n";

            s += "\n  windows\n";
            if (window_layers != null)
                foreach (string f in window_layers)
                    if (!string.IsNullOrWhiteSpace(f))
                        s += "    " + f + " [pos" + forms[f].position + " size" + forms[f].size + "]\n";

            return s;
        }



        string[] floating_forms;
        
        //instead of generating this every frame, we're gonna keep it updated and manage it properly
        public bool hit_scan(int mX, int mY) {
            mouse_down_left = DigitalControlBindings.bind_pressed("ui_select");
            mouse_down_right = DigitalControlBindings.bind_pressed("ui_context_menu");

            mouse_down_start_left = DigitalControlBindings.bind_just_pressed("ui_select");
            mouse_down_start_right = DigitalControlBindings.bind_just_pressed("ui_context_menu");

            mouse_down_end_left = DigitalControlBindings.bind_just_released("ui_select");
            mouse_down_end_right = DigitalControlBindings.bind_just_released("ui_context_menu");

            mouse_scroll_up = !DigitalControlBindings.bind_released("ui_scroll_up");
            mouse_scroll_down = !DigitalControlBindings.bind_released("ui_scroll_down");

            //FUCK IT MARK 2 BABY

            //forms are stored simply as their names in list order, sorted by depth
            floating_forms = new string[forms.Count];

            //SORT the goddamn forms
            int ff = 0;
            foreach (string f in forms.Keys.OrderBy(a => forms[a].z)) {
                if (forms[f].type != form_type.WINDOW) {
                    floating_forms[ff] = f;
                    ff++;
                }
            }

            // ff = 0;
            //foreach (string f in forms.Keys) {
            //     if (forms[f].type == form_type.WINDOW) {
            //         window_layers[ff] = f;
            //         ff++;
            //     }
            //  }


            bool hit = false;
            //if (window_layers.Length == 0) return false;

            /*var fone = window_layers[0];

            if (!string.IsNullOrWhiteSpace(fone)) {


                if (Math2D.point_within_square(forms[fone].hover_aabb.top_left, forms[fone].hover_aabb.bottom_right, mouse_position)) {


                    if (forms[fone].mouse_hit(mouse_position, new XYPair(mouse_position.X - forms[fone].position.X, mouse_position.Y - forms[fone].position.Y))) {
                        //move_to_top(fone, window_layers);
                        hit = true;
                    }

                }
            }
            

            foreach (string f in window_layers) {
                if (forms[f].hidden) continue;


                var laabb = new XYPair((mouse_position.X - forms[f].position.X), (mouse_position.Y - forms[f].position.Y));
                forms[f].mouse_update(mouse_position, laabb, true);



                //if (f != fone && !hit) {

                    if (Math2D.point_within_square(forms[f].hover_aabb.top_left, forms[f].hover_aabb.bottom_right, mouse_position)) {

                        if ((mouse_down_start_left || mouse_down_start_right || mouse_down_end_left || mouse_down_end_right || mouse_scroll_up || mouse_scroll_down)) {
                            //if (forms[f].mouse_hit(mouse_position, laabb)) {
                                move_to_top(f);
                                hit = true;
                            //}
                            //}
                        }
                    }
                //}
            }     */


            foreach (string f in floating_forms) {
                if (!string.IsNullOrWhiteSpace(f)) {
                    var laabb = new XYPair((mouse_position.X - forms[f].position.X), (mouse_position.Y - forms[f].position.Y));

                    forms[f].mouse_hover = false;
                    forms[f].mouse_update(mouse_position, laabb, true);
                    if (forms[f].hover_aabb != null && Math2D.point_within_square(forms[f].hover_aabb.top_left, forms[f].hover_aabb.bottom_right, mouse_position)) {
                        forms[f].mouse_hover = true;
                        // if (!hit) {
                        if (forms[f].mouse_hit(mouse_position, mouse_position)) {
                            //move_to_top(f, ref window_layers);
                            hit = true;
                        }
                    //}
                    } 
                }
            }

            return hit;
        }



        public void draw() {
            if (hide) return;

            if (window_layers != null) {
                foreach (string f in window_layers.Reverse()) {
                    if (!string.IsNullOrWhiteSpace(f) && !forms[f].hidden)
                        forms[f].draw();
                }
            }
            if (floating_forms != null) { 
                foreach (string f in floating_forms) {
                    if (!string.IsNullOrWhiteSpace(f) && !forms[f].hidden)
                        forms[f].draw();
                }
            }

        }

        public bool handled_right = false;
        public bool handled_ui_hit = false;
        public bool handled_right_p = false;
        public bool handled_ui_hit_p = false;
        public bool handled_left = false;
        public bool handled_left_p = false;
        public bool handled_alt = false;
        public bool handled_hold_focus = false;
        public bool handled_wheel = false;

        bool buttons_on_ui = false;
        public void update(out bool handle_mouse_clicks) {

            buttons_on_ui = hit_scan(mouse_state.Position.X, mouse_state.Position.Y);
            
            handle_mouse_clicks = buttons_on_ui;
            
            handled_left = bind_pressed("ui_select") || bind_just_pressed("ui_select");
            handled_right = bind_pressed("ui_context_menu") || bind_just_pressed("ui_context_menu");
            handled_wheel = bind_pressed("ui_scroll_up") || bind_just_pressed("ui_scroll_up") || bind_pressed("ui_scroll_down") || bind_just_pressed("ui_scroll_down");
            handled_alt = bind_pressed("ui_alt");

            if (handled_wheel && buttons_on_ui) {
                handle_mouse_clicks = true;
            }

            if ((handled_ui_hit || (handled_right && handled_alt)) && (!handled_hold_focus && (!handled_right_p && !handled_ui_hit_p)) || handled_wheel) {
                handled_hold_focus = true;
            }
            if ((handled_ui_hit || (handled_left && handled_alt)) && (!handled_hold_focus && (!handled_left_p && !handled_ui_hit_p)) || handled_wheel) {
                handled_hold_focus = true;
            }

            if (bind_released("ui_select") && bind_released("ui_context_menu"))
                handled_hold_focus = false;

            if (mouse_lock == true)
                handled_hold_focus = false;

            if (!handled_right) {

            }


            foreach (UIForm f in forms.Values) {
                f.update();
            }

        }


    }
}
