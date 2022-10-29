using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Math2D;

namespace Magpie.Engine {
    public enum gvar_data_type {
        BOOL,
        INT,
        FLOAT,
        DOUBLE,
        XYPAIR,
        VECTOR2,
        VECTOR3,
        STRING,
        UNKNOWN
    }

    public static class command_parser {
        public static void parse_string(string input) {
            input = input.Replace("=", " = ");
            input = input.Replace("[", " [ ");
            input = input.Replace("]", " ] ");
            input = input.Replace("(", " ( ");
            input = input.Replace(")", " ) ");

            string[] split = input.Split(' ');
        }
    }


    public class gvar {
        public string name;
        
        public gvar_data_type data_type;
        
        public object data;

        public Action changed;



        public gvar(string name, gvar_data_type data_type, object data) {
            this.name = name; this.data_type = data_type; this.data = data;
        }

        public string to_string() {
            switch (data_type) {
                case gvar_data_type.BOOL:
                    return ((bool)data).ToString().ToLower();

                case gvar_data_type.INT:
                    return ((int)data).ToString();

                case gvar_data_type.FLOAT:
                    return ((float)data).ToString();

                case gvar_data_type.DOUBLE:
                    return ((double)data).ToString();

                case gvar_data_type.XYPAIR:
                    return ((XYPair)data).ToXString();

                case gvar_data_type.VECTOR2:
                    return ((Vector2)data).simple_vector2_string_full_acc();

                case gvar_data_type.VECTOR3:
                    return ((Vector3)data).simple_vector3_string_full_acc();

                case gvar_data_type.STRING:
                    return (string)data;

                default:
                    return "";
            }
        }
    }

    public static class gvars {
        static Dictionary<string, gvar> _gvars = new Dictionary<string, gvar>();

        public static void add_gvar(string name, gvar_data_type data_type, object data) {
            _gvars.Add(name, new gvar(name, data_type, data));            
        }
        public static void remove_gvar(string name) { _gvars.Remove(name); }
        
        public static string list_all() {
            var s = "";
            int c = 0;
            foreach(string gvar_key in _gvars.Keys) {
                var gvar = _gvars[gvar_key];
                
                s += string.Format("[{0}] {1} :: {2}{3}", gvar.data_type.ToString(), gvar.name, get_string(gvar_key, 2), c < _gvars.Count-1 ? "\n" : "");
                c++;
            }

            return s;
        }

        public static bool contains_bool(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.BOOL);
        public static bool contains_int(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.INT);
        public static bool contains_float(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.FLOAT);
        public static bool contains_double(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.DOUBLE);
        public static bool contains_string(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.STRING);
        public static bool contains_xy(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.XYPAIR);
        public static bool contains_v2(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.VECTOR2);
        public static bool contains_v3(string name) => (_gvars.ContainsKey(name) && _gvars[name].data_type == gvar_data_type.VECTOR3);

        public static gvar_data_type detect_type_from_string(string input) {
            var l = input.ToLower();
            if (l == "true" || l == "false" || l == "yes" || l == "no" || l == "y" || l == "n") {
                return gvar_data_type.BOOL;
            }

            if (input.StartsWith("\"") && input.EndsWith("\"")) {
                return gvar_data_type.STRING;
            }

            if (input.EndsWith("f") && float.TryParse(input, out _)) {
                return gvar_data_type.FLOAT;
            } 
            if (double.TryParse(input, out _)) {
                return gvar_data_type.DOUBLE;
            }
            if (int.TryParse(input, out _)) {
                return gvar_data_type.INT;
            }

            //if (input.StartsWith("[") && input.EndsWith("]")) {
                if (XYPair.TryParse(input, out _)) {
                    return gvar_data_type.XYPAIR;
                }
                if (Vector2TryParse(input, out _)) {
                    return gvar_data_type.VECTOR2;
                }
                if (Vector3TryParse(input, out _)) {
                    return gvar_data_type.VECTOR3;
                }
            //}
            return gvar_data_type.UNKNOWN;
        }

        public static void add_change_action(string name, Action action) {
            _gvars[name].changed = action;
        }

        public static void set(string name, bool value) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");                
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, int value) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, float value) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, double value) {
            if (!contains_double(name)) throw new Exception("no such double with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, XYPair value) {
            if (!contains_xy(name)) throw new Exception("no such xypair with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, Vector2 value) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, Vector3 value) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }
        public static void set(string name, string value) {
            if (!contains_string(name)) throw new Exception("no such string with name \"" + name + "\"");
            _gvars[name].data = value;
            _gvars[name].changed?.Invoke();
        }                     

        public static void get(string name, out bool result ) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");
            result = (bool)_gvars[name].data;            
        }
        public static void get(string name, out int result) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            result = (int)_gvars[name].data;
        }
        public static void get(string name, out float result) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            result = (float)_gvars[name].data;
        }
        public static void get(string name, out double result) {
            if (!contains_double(name)) throw new Exception("no such double with name \"" + name + "\"");
            result = (double)_gvars[name].data;
        }
        public static void get(string name, out XYPair result) {
            if (!contains_xy(name)) throw new Exception("no such xypair with name \"" + name + "\"");
            result = (XYPair)_gvars[name].data;           
        }
        public static void get(string name, out Vector2 result) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            result = (Vector2)_gvars[name].data;
        }
        public static void get(string name, out Vector3 result) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            result = (Vector3)_gvars[name].data;
        }
        public static void get(string name, out string result) {
            if (!contains_string(name)) throw new Exception("no such string with name \"" + name + "\"");
            result = (string)_gvars[name].data;
        }


        public static bool get_bool(string name) {
            if (!contains_bool(name)) throw new Exception("no such bool with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.BOOL) {
                return (bool)_gvars[name].data;
            } else {
                throw new Exception("get_bool is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static int get_int(string name) {
            if (!contains_int(name)) throw new Exception("no such int with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.INT || _gvars[name].data_type == gvar_data_type.FLOAT || _gvars[name].data_type == gvar_data_type.DOUBLE) {
                return (int)_gvars[name].data;
            } else {
                throw new Exception("get_int is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static float get_float(string name) {
            if (!contains_float(name)) throw new Exception("no such float with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.FLOAT || _gvars[name].data_type == gvar_data_type.INT || _gvars[name].data_type == gvar_data_type.DOUBLE) {
                return (float)_gvars[name].data;
            } else {
                throw new Exception("get_float is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static double get_double(string name) {
            if (!contains_double(name)) throw new Exception("no such double with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.DOUBLE || _gvars[name].data_type == gvar_data_type.INT || _gvars[name].data_type == gvar_data_type.FLOAT) {
                return (double)_gvars[name].data;
            } else {
                throw new Exception("get_double is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static XYPair get_xypair(string name) {
            if (!contains_xy(name)) return XYPair.Zero; // throw new Exception("no such xypair with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.XYPAIR) {
                return (XYPair)_gvars[name].data;
            } else {
                throw new Exception("get_xypair is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static Vector2 get_vector2(string name) {
            if (!contains_v2(name)) throw new Exception("no such vector2 with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.VECTOR2) {
                return (Vector2)_gvars[name].data;
            } else {
                throw new Exception("get_vector2 is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }
        public static Vector3 get_vector3(string name) {
            if (!contains_v3(name)) throw new Exception("no such vector3 with name \"" + name + "\"");
            if (_gvars[name].data_type == gvar_data_type.VECTOR3) {
                return (Vector3)_gvars[name].data;
            } else {
                throw new Exception("get_vector3 is not suitable for use on " + _gvars[name].data_type.ToString());
            }
        }

        public static string get_string(string name) {
            string s = "";
            gvar_data_type gdt = _gvars[name].data_type;

            switch (gdt) {
                case gvar_data_type.BOOL:
                    s = ((bool)_gvars[name].data).ToString().ToLower();
                    break;
                case gvar_data_type.INT:
                    s = string.Format("{0}", (int)_gvars[name].data);
                    break;
                case gvar_data_type.FLOAT:
                    s = string.Format("{0}", (float)_gvars[name].data);
                    break;
                case gvar_data_type.DOUBLE:
                    s = string.Format("{0}", (double)_gvars[name].data);
                    break;
                case gvar_data_type.STRING:
                    s = (string)_gvars[name].data;
                    break;
                case gvar_data_type.XYPAIR:
                    s = XYPair.simple_string_brackets((XYPair)_gvars[name].data);
                    break;
                case gvar_data_type.VECTOR2:
                    s = ((Vector2)_gvars[name].data).simple_vector2_string_brackets();
                    break;
                case gvar_data_type.VECTOR3:
                    s =  ((Vector3)_gvars[name].data).simple_vector3_string_brackets();
                    break;
            }
            return s;
        }
        public static string get_string(string name, int decimal_places) {
            string s = "";
            gvar_data_type gdt = _gvars[name].data_type;

            switch (gdt) {
                case gvar_data_type.BOOL:
                    s = ((bool)_gvars[name].data).ToString().ToLower();
                    break;
                case gvar_data_type.INT:
                    s = string.Format("{0}", (int)_gvars[name].data);
                    break;
                case gvar_data_type.FLOAT:
                    s = string.Format("{0:F" + decimal_places + "}", (float)_gvars[name].data);
                    break;
                case gvar_data_type.DOUBLE:
                    s = string.Format("{0:F" + decimal_places + "}", (double)_gvars[name].data);
                    break;
                case gvar_data_type.STRING:
                    s = (string)_gvars[name].data;
                    break;
                case gvar_data_type.XYPAIR:
                    s = XYPair.simple_string_brackets((XYPair)_gvars[name].data);
                    break;
                case gvar_data_type.VECTOR2:
                    s = ((Vector2)_gvars[name].data).simple_vector2_string_brackets();
                    break;
                case gvar_data_type.VECTOR3:
                    s = ((Vector3)_gvars[name].data).simple_vector3_string_brackets();
                    break;
            }
            return s;
        }
    }
    
}