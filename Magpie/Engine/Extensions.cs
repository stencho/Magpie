using Magpie.Graphics;
using Magpie.Graphics.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public static class Extensions {
        public static Vector2 XZ(this Vector3 pos) {
            return new Vector2(pos.X, pos.Z);
        }

        public static string simple_vector3_string(this Vector3 input) {
            return string.Format("{0:F2}, {1:F2}, {2:F2}", input.X, input.Y, input.Z);
        }
        public static string simple_vector3_string_full_acc(this Vector3 input) {
            return string.Format("{0}, {1}, {2}", input.X, input.Y, input.Z);
        }
        public static string simple_vector2_string(this Vector2 input) {
            return string.Format("{0:F2}, {1:F2}", input.X, input.Y);
        }
        public static string simple_vector2_string_full_acc(this Vector2 input) {
            return string.Format("{0}, {1}", input.X, input.Y);
        }
        public static string simple_vector2_x_string(this Vector2 input) {
            return string.Format("{0:F2}x{1:F2}", input.X, input.Y);
        }
        public static string simple_vector2_x_string_full_acc(this Vector2 input) {
            return string.Format("{0}x{1}", input.X, input.Y);
        }


        public static string simple_vector2_x_string_no_dec(this Vector2 input) {
            return string.Format("{0:F0}x{1:F0}", input.X, input.Y);
        }

        public static string simple_vector3_string_brackets(this Vector3 input) {
            return string.Format("[{0:F2}, {1:F2}, {2:F2}]", input.X, input.Y, input.Z);
        }
        public static string simple_vector2_string_brackets(this Vector2 input) {
            return string.Format("[{0:F2}, {1:F2}]", input.X, input.Y);
        }

        public static Vector3 ToVector3XZ(this Vector2 v) {
            return new Vector3(v.X, 0, v.Y);
        }

        public static bool contains_nan(this Vector3 a) { return (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z)); }

        public static Vector3 dimensions (this BoundingBox bb) {
            return bb.Max - bb.Min;
        }
        public static Vector3 half_size(this BoundingBox bb) {
            return bb.Max - ((bb.Min + bb.Max) / 2f);
        }

        public static Vector3 center(this BoundingBox bb) {
            return (bb.Min + bb.Max) / 2f;
        }

        public static void draw_debug(this BoundingBox bb, Color color) {
            Draw3D.cube(A(bb), B(bb), C(bb), D(bb), E(bb), F(bb), G(bb), H(bb), color);
        }

        public static Vector3 A (this BoundingBox bb) { return bb.center() + bb.half_size(); }
        public static Vector3 B (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2)); }

        public static Vector3 C (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitY * 2)); }
        public static Vector3 D (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2)); }

        public static Vector3 E (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2)); }
        public static Vector3 F (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitZ * 2)); }
                                
        public static Vector3 G (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitZ * 2) - (Vector3.UnitY * 2)); }
        public static Vector3 H (this BoundingBox bb) { return bb.center() + (bb.half_size() * Vector3.One - (Vector3.UnitX * 2) - (Vector3.UnitY * 2) - (Vector3.UnitZ * 2)); }

        public static bool Intersects(this Rectangle rect, XYPair point) {

            if (rect.Right > point.X && rect.Left < point.X && rect.Top < point.Y && rect.Bottom > rect.Y) return true;
            return false;
        }

        static List<IUIForm> tmp_list = new List<IUIForm>();
        static List<IUIForm> tmp_top = new List<IUIForm>();
        static List<IUIForm> tmp_floating = new List<IUIForm>();
        static List<IUIForm> tmp_bottom = new List<IUIForm>();

        public static string Ellipsis(this string str, int ellipsis_end) {
            if (str.Length > 3) {
                string s = "";
                s = str.Substring(0, ellipsis_end - 3);
                s += "...";
                return s;
            } else
                return new string('.', str.Length);
        }

        public static void SortWindows(this List<IUIForm> list) {
            /*
            tmp_top.Clear();
            tmp_floating.Clear();
            tmp_bottom.Clear();
            tmp_list.Clear();

            foreach (IUIForm f in list) {
                if (f.layer_state == ui_layer_state.on_bottom) {
                    tmp_bottom.Add(f);
                } else if (f.layer_state == ui_layer_state.on_top) {
                    tmp_top.Add(f);
                } else {
                    tmp_floating.Add(f);
                }
            }

            foreach (IUIForm f in tmp_bottom) {
                tmp_list.Add(f);
            }

            foreach (IUIForm f in tmp_floating) {
                tmp_list.Add(f);
            }

            foreach (IUIForm f in tmp_top) {
                tmp_list.Add(f);
            }

            return tmp_list;
            */

            int c = 0;
            int low = 0;
            int norm = 0;
            int high = 0;

            tmp_list.Clear();

            for (int i = 0; i < list.Count; i++) {
                tmp_list.Add(list[i]);
            }

            list.Clear();

            for (int i = 0; i < tmp_list.Count; i++) {

                if (tmp_list[i].layer_state == ui_layer_state.on_bottom) {
                    list.Add(tmp_list[i]);
                }
                
            }

            for (int i = 0; i < tmp_list.Count; i++) {

                if (tmp_list[i].layer_state == ui_layer_state.floating && !(tmp_list[i].has_focus || tmp_list[i].top_of_mouse_stack)) {
                    list.Add(tmp_list[i]);
                }

            }
            for (int i = 0; i < tmp_list.Count; i++) {
                if (tmp_list[i].layer_state == ui_layer_state.floating && tmp_list[i].top_of_mouse_stack && !tmp_list[i].has_focus) {
                    list.Add(tmp_list[i]);
                }
            }
            for (int i = 0; i < tmp_list.Count; i++) {
                if (tmp_list[i].layer_state == ui_layer_state.floating && (tmp_list[i].has_focus && tmp_list[i].top_of_mouse_stack)) {
                    list.Add(tmp_list[i]);
                }

            }
            for (int i = 0; i < tmp_list.Count; i++) {
                if (tmp_list[i].layer_state == ui_layer_state.floating && tmp_list[i].has_focus && !tmp_list[i].top_of_mouse_stack) {
                    list.Add(tmp_list[i]);
                }
            }
            
            for (int i = 0; i < tmp_list.Count; i++) {
                if (tmp_list[i].layer_state == ui_layer_state.on_top) {
                    list.Add(tmp_list[i]);
                }
            }


            /*
            for (int i = 0; i < list.Count; i++) {
                IUIForm tmp = list[i];

                switch (list[i].layer_state) {

                    case ui_layer_state.on_bottom:
                        list.Remove(list[i]);
                        if (i < low) 
                            list.Insert(low-1, tmp);
                        else
                            list.Insert(low, tmp);
                        low++;
                        break;

                    case ui_layer_state.floating:
                        list.Remove(list[i]);
                        if (i < low+norm)
                            list.Insert(low+norm- 1, tmp);
                        else
                            list.Insert(low + norm, tmp);
                        norm++;
                        break;
                    case ui_layer_state.on_top:
                        list.Remove(list[i]);
                        if (i < low + norm + high)
                            list.Insert(low + norm + high - 1, tmp);
                        else
                            list.Insert(low + norm + high, tmp);
                        high++;
                        break;
                }

                c++;
            }
            for (int i = list.Count - 1; i >= 0; i--) {
                IUIForm tmp = list[i];
                if (list[i].top_of_mouse_stack && list[i].layer_state == ui_layer_state.floating) {
                    list.Remove(list[i]);
                    if (i < low + norm)
                        list.Insert(low + norm + 1, tmp);
                    else
                        list.Insert(low + norm, tmp);
                    break;
                }

            }

            for (int i = list.Count-1; i >=0; i--) {
                IUIForm tmp = list[i];
                if (list[i].has_focus && list[i].layer_state == ui_layer_state.floating) {
                    list.Remove(list[i]);
                    list.Insert(low + norm, tmp);
                }

            }*/
        }
        public static void BringToFront(this List<IUIForm> list, IUIForm window) {
            if (list.Count <= 1) { list[0].has_focus = true;  return; }
            if (window == list[list.Count - 1]) {
                
                for (int i = 0; i < list.Count-1; i++) {
                    list[i].has_focus = false;
                }

                list[list.Count - 1].has_focus = true;
                return;
            }

            int found = 0;
            for (int i = 0; i < list.Count; i++) {
                if (list[i] == window) {

                    found = i;
                    var tmp = window;

                    list[i].has_focus = false;

                    for (i = found; i < list.Count - 1; i++) {
                        list[i] = list[i + 1];
                        list[i].has_focus = false;
                    }

                    list[list.Count - 1] = tmp;
                    list[list.Count - 1].has_focus = true;
                    /*
                    for (i = found; i > 0; i--) {
                        list[i] = list[i-1];
                    }
                    list[0] = tmp;
                    */
                    return;
                }
            }
        }
    }
}
