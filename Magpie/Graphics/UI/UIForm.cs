using ConsoleInput;
using Magpie.Engine;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;
using static Magpie.Engine.Controls;
using static Magpie.Engine.Log;

namespace Magpie.Graphics.UI {
    public interface IUIForm {
        string name { get; }
        string text { get; }

        ui_layer_state layer_state { get; }

        XYPair position { get; set; }
        XYPair size { get; set; }

        XYPair client_top_left { get; }
        XYPair client_size { get; }
        XYPair client_bottom_right { get; }

        bool mouse_over { get; } 
        bool has_focus { get; set; }
        bool top_of_mouse_stack { get; set; }
        bool visible { get; }

        List<IUIForm> subforms { get; set; }
        Dictionary<string, ISupport2D> collision { get; }

        List<string> mouse_interactions { get; }

        bool test_mouse();

        void update();
        void render_internal();
        void draw();

        string list_subforms();

    }

    public class Console : IUIForm {
        public string name => "text_display";
        public string text => _text;
        string _text = "";//@"According to all known laws of #c:Red# aviation, there is no way that a bee should be #c:128,128,128# able to fly. Its wings are too small to get its fat little body off the ground. The bee, of course, flies anyway. Because bees don’t care what humans think is impossible.” SEQ. 75 - “INTRO TO BARRY” INT. BENSON HOUSE - DAY ANGLE ON: Sneakers on the ground. Camera PANS UP to reveal BARRY BENSON’S BEDROOM ANGLE ON: Barry’s hand flipping through different sweaters in his closet. BARRY Yellow black, yellow black, yellow black, yellow black, yellow black, yellow black...oohh, black and yellow... ANGLE ON: Barry wearing the sweater he picked, looking in the mirror. BARRY (CONT’D) Yeah, let’s shake it up a little. He picks the black and yellow one. He then goes to the sink, takes the top off a CONTAINER OF HONEY, and puts some honey into his hair. He squirts some in his mouth and gargles. Then he takes the lid off the bottle, and rolls some on like deodorant. CUT TO: INT. BENSON HOUSE KITCHEN - CONTINUOUS Barry’s mother, JANET BENSON, yells up at Barry. JANET BENSON Barry, break";
        string _buffer = "";

        public bool has_focus { get; set; } = false;
        public bool visible => _visible;
        bool _visible = true;

        public XYPair position { get; set; } = XYPair.Zero;
        public XYPair size { get; set; } = XYPair.One * 10;

        public XYPair top_left => position;
        public XYPair bottom_right => position + size;
        public XYPair bottom_left => position + (XYPair.UnitY * client_size.Y);
        public XYPair top_right => position + (XYPair.UnitX * client_size.X);

        public XYPair client_top_left => top_left;
        public XYPair client_size => size;
        public XYPair client_bottom_right => bottom_right;

        public bool mouse_over => (mouse_interactions.Count > 0);

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, ISupport2D> collision => _collision;

        public ui_layer_state layer_state => ui_layer_state.floating;

        Dictionary<string, ISupport2D> _collision = new Dictionary<string, ISupport2D>();

        public float text_line_gap = 1;
        public float message_gap = 3;

        public Color text_color = Color.White;
        public Color text_shadow_color = Color.Black;

        public bool text_shadow = false;

        public int scroll_bar_width = 15;

        public int text_box_height = 18;
        public int text_box_edge_margin = 4;

        public int bottom_scroll_pos = 25;

        IUIForm parent_form;

        ConsoleInputHandler cih;

        public Console(int X, int Y, int width, int height, IUIForm parent_form = null) {
            this.parent_form = parent_form;

            position = new XYPair(X, Y);
            size = new XYPair(width, height);

            cih = new ConsoleInputHandler(110,10, width - 1);
            
            //Logging.log_with_namespace("test", this.GetType());
            //Logging.log(Logging.log_level.ERROR, "test 2");
            //Logging.log(Logging.log_level.WARNING, "test 3");
            //Logging.log("be #c:128,128,128# jhgjkhfg", log_data.default_format_text_header_source);
            //Logging.log("I am farting #b_on#very#b_off# hard in this moment #b_on#so#b_off# hard in fact that my cheeks of ass are sort of lioke this: (#c:SaddleBrown#,(#c# ) !!", "FARTING!!", this.GetType().ToString(), log_data.default_format_text_header_source,log_level.CUSTOM, "LightGray", "SaddleBrown", "SaddleBrown", "SaddleBrown");
            
        }

        public void change_text(string text) {
            _text = text;
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }

        public void update() {

            cih.has_focus = parent_form.has_focus;

            cih.update(cih.has_focus, parent_form.client_top_left);

            //_text = Logging.last_n_messages(200);

        }

        public void render_internal() {

        }

        float default_x_position = 4;
        float notch_width = 3;

        public Color default_color = Color.LightGray;

        float scroll_position = 0;

        Vector2 pos = Vector2.Zero;
        public void draw() {
            Color current_color = default_color;

            bool bold = false;

            float msx = Math2D.measure_string_x("pf", "a");
            float msx_single = Math2D.measure_string_x("pf", "a");

            float msy = Math2D.measure_string_y("pf", "a");

            float msy_overall = 0;
            
            var c = 0;
            var x = 0;
            float last_msy = 0;

            bool skipdraw = false;

            Draw2D.fill_square(0, 0, size.X, size.Y, Color.FromNonPremultiplied(25,25,25,255));

            for (int d = 0; d < Log.data.Count; d++) {
                x = Log.data.Count - 1 - c;

                if (x < 0) break;

                skipdraw = false;

                string edited = Log.data[x].print();

                edited = edited.Replace("\n", " \\n ");

                var rm = Regex.Match(edited, UIStandard.color_string_pattern);

                edited = Regex.Replace(edited, UIStandard.color_string_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.color_string_reset_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.color_string_RGB_pattern, " $1 ");

                edited = Regex.Replace(edited, UIStandard.bold_string_disable_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.bold_string_enable_pattern, " $1 ");
                edited = Regex.Replace(edited, UIStandard.bold_string_toggle_pattern, " $1 ");

                var split = edited.Split(' ');


                pos = Vector2.Zero;
                pos.X = default_x_position;

                for (int i = 0; i < split.Length; i++) {
                    skipdraw = false;
                    // TURN ALL THIS INTO A UISTANDARD FUNCTION
                    // THEN MAKE A SIMPLE XYPair MEASURE_STRING_FANCY(string font, string text, int max_width);
                    // then that can just run all this shit on a string, act like it's inside a text box, and measure the entire list that way
                    // from there it's not too hard to add scrolling, and from there it's also not too hard to add keep-scroll-at-bottom

                    if (Regex.IsMatch(split[i], UIStandard.color_string_pattern)) {
                        var k = split[i].Replace("#", "").Replace("c:", "");
                        if (Draw2D.string_colors.ContainsKey(k)) {
                            current_color = Draw2D.string_colors[k];
                        } else {
                            current_color = Color.White;
                        }

                        pos.X -= msx_single;
                        skipdraw = true;

                    } else if (Regex.IsMatch(split[i], UIStandard.color_string_reset_pattern)) {
                        current_color = default_color;

                        pos.X -= msx_single;
                        skipdraw = true;


                    } else if (Regex.IsMatch(split[i], UIStandard.color_string_RGB_pattern)) {

                        var s = Regex.Split(split[i].Replace("#", "").Replace("c:", ""), "[,/:;-\\|x]");
                        int r, g, b;

                        if (int.TryParse(s[0], out r) && int.TryParse(s[1], out g) && int.TryParse(s[2], out b)) {
                            current_color = Color.FromNonPremultiplied(r, g, b, 255);
                        } else {
                            current_color = default_color;
                        }

                        pos.X -= msx_single;
                        skipdraw = true;

                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_enable_pattern)) {
                        bold = true;
                        pos.X -= msx_single;
                        skipdraw = true;
                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_disable_pattern)) {
                        bold = false;
                        pos.X -= msx_single;
                        skipdraw = true;
                    } else if (Regex.IsMatch(split[i], UIStandard.bold_string_toggle_pattern)) {
                        bold = !bold;
                        pos.X -= msx_single;
                        skipdraw = true;
                    }

                    if (skipdraw) continue;

                    msx = Math2D.measure_string_x("pf", split[i]);
                    
                    if (msx + pos.X + scroll_bar_width >= client_size.X || split[i] == "\\n") {
                        pos.X = default_x_position;
                        pos.Y += (msy + text_line_gap);

                        if (split[i] == "\\n")
                            skipdraw = true;
                    }

                    msx += msx_single;
                    
                    if (skipdraw) continue;
                    
                    if (text_shadow)
                        Draw2D.text("pf", split[i], top_left + XYPair.One + pos + (XYPair.UnitY *  msy_overall), text_shadow_color);

                    Draw2D.text("pf", split[i], top_left + pos + (XYPair.UnitY *  msy_overall), current_color);

                    if (bold)
                        Draw2D.text("pf", split[i], top_left + pos + (XYPair.UnitY * msy_overall) + XYPair.UnitX, current_color);
                    
                    pos.X += msx;
                }                

                last_msy = msy_overall;
                msy_overall += pos.Y + msy + message_gap + 2;
                

                //Draw2D.line((int)pos.X, (int)msy_overall, client_size.X, (int)msy_overall, 1f, Color.Red);
                Draw2D.fill_square(new XYPair(1, last_msy+1), new XYPair(notch_width, msy_overall - last_msy - 1), data[x].side_notch_color);

                c++;
            }


            //Console.WriteLine(edited);

            //scroll bar
            // Draw2D.fill_square(new XYPair(client_size.X - scroll_bar_width, 0), new XYPair(client_size.X, client_size.Y - (text_box_edge_margin) - text_box_height), Color.Red);

            //text box
            Draw2D.image("gradient_vertical", new XYPair(0, client_size.Y - (text_box_height * 1.8f)), new XYPair(client_size.X, (text_box_height * 2f)), Color.Black);
            //Draw2D.fill_square(new XYPair(text_box_edge_margin/2, client_size.Y - text_box_height), new XYPair(client_size.X - (text_box_edge_margin/2), text_box_height), Color.Red);

            cih.set_width_lock_to_char_width(this.size.X - (text_box_edge_margin * 2) - (cih.Width / 40 / 2));
            cih.set_position(new XYPair( (this.size.X / 2) - (cih.Width / 2) + 1, client_size.Y - text_box_height + 3));
            cih.draw();


            //bottom scroll marker
            //Draw2D.line(0, client_size.Y - bottom_scroll_pos, client_size.X, client_size.Y - bottom_scroll_pos, 1f, (msy_overall > client_size.Y - bottom_scroll_pos) ? Color.HotPink : Color.Purple);
        }

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }
    }

    public class UIButton : IUIForm {
        public string name => "button";
        public string text => _text;

        public ui_layer_state layer_state => ui_layer_state.on_top;

        public void change_text(string text) {
            this._text = text;
        }

        public bool has_focus { get; set; } = false;
        public bool visible => _visible;
        bool _visible = true;

        public XYPair position { get; set; } = XYPair.Zero;
        public XYPair size { get; set; } = XYPair.One * 10;

        public XYPair top_left => position;
        public XYPair bottom_right => position + size;

        public XYPair client_top_left => top_left;
        public XYPair client_size => size;
        public XYPair client_bottom_right => bottom_right;

        public bool mouse_over => (mouse_interactions.Count > 0);

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, ISupport2D> collision => _collision;
        Dictionary<string, ISupport2D> _collision = new Dictionary<string, ISupport2D>();
        
        string _text = "";

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        Color _text_color                   = Color.White;
        Color _border_color                 = Color.White;
        Color _background_color             = Color.Black;

        Color _hover_text_color             = Color.HotPink;
        Color _hover_border_color           = Color.HotPink;
        Color _hover_background_color       = Color.Black;

        Color _clicked_text_color           = Color.White;
        Color _clicked_border_color         = Color.White;
        Color _clicked_background_color     = Color.Black;

        float molerp = 0;
        
        IUIForm parent_form;

        public UIButton(int X, int Y, int width, int height, string text) {
            position = new XYPair(X, Y);
            size = new XYPair(width, height);
            _text = text;
            
            _collision.Add("button", new BoundingBox2D(top_left, bottom_right));
        }

        public UIButton(int X, int Y, int width, int height, string text, IUIForm parent_form) {
            position = new XYPair(X, Y);
            size = new XYPair(width, height);
            _text = text;

            this.parent_form = parent_form;

            _collision.Add("button", new BoundingBox2D(top_left,bottom_right));            
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }


        [Flags]
        public enum button_mouse_status {
            none = 0,
            mouse_over = 1 << 0,
            mouse_down = 1 << 1,
            mouse_up = 1 << 2
        }

        button_mouse_status current_flags = button_mouse_status.none;
        button_mouse_status previous_flags = button_mouse_status.none;

        Action button_click;

        public void set_action(Action action) {
            button_click = action;
        }

        bool _clicked = false;
        public bool clicked => _clicked;
        bool mdown = false;
        bool is_child => parent_form != null;

        public void update() {
            if (is_child) {
                ((BoundingBox2D)_collision["button"]).set(top_left + parent_form.client_top_left, parent_form.client_top_left + bottom_right);
            } else {
                ((BoundingBox2D)_collision["button"]).set(top_left, bottom_right);
            }

            var mo = test_mouse();

            mdown = Controls.is_pressed(MouseButtons.Left);
                       
            if (mo && (top_of_mouse_stack && is_child)) {
                current_flags = button_mouse_status.mouse_over;

                if (Controls.is_pressed(MouseButtons.Left)) {
                    current_flags = button_mouse_status.mouse_over | button_mouse_status.mouse_down;
                }

                
                if (molerp < 1) molerp += 0.05f;
                else if (molerp > 1) molerp = 1;
                

                if (current_flags.HasFlag(button_mouse_status.mouse_down) && !previous_flags.HasFlag(button_mouse_status.mouse_down)) {
                    //mouse just clicked
                }

            } else if (!mo || !(top_of_mouse_stack && is_child)) {
                if (molerp > 0) molerp -= 0.05f;
                else if (molerp < 0) molerp = 0;

                if (Controls.is_pressed(MouseButtons.Left)) {
                    current_flags = button_mouse_status.mouse_down;
                } else {
                    current_flags = button_mouse_status.none;
                }
            }
             
            if (!current_flags.HasFlag(button_mouse_status.mouse_down) && previous_flags.HasFlag(button_mouse_status.mouse_down) && !_clicked ) {
                //mouse just released
                if (mo && !mdown && previous_flags.HasFlag(button_mouse_status.mouse_down) && (top_of_mouse_stack)) {
                    //do stuff
                    if (button_click != null )
                        button_click.Invoke();

                    _clicked = true;
                }
            }

            if (_clicked) {
                _clicked = false;
            }

            previous_flags = current_flags;
        }
        
        public void render_internal() {}

        public void draw() {
            var ms = Math2D.measure_string("pf", text);
            Draw2D.fill_square(top_left, size, Draw2D.ColorInterpolate(_background_color, _hover_background_color, molerp));
            Draw2D.text("pf", text, position + (size / 2) - (ms / 2), Draw2D.ColorInterpolate(_text_color, _hover_text_color, molerp));
            Draw2D.square(top_left, bottom_right - XYPair.One, 2, Draw2D.ColorInterpolate(_border_color, _hover_border_color, molerp));
        }

    }

    /*
     *** all of these need doing to some extent ***
    public class UIButton : IUIForm{}
    public class UILabel : IUIForm{}
    public class UITextDisplay : IUIForm{}
    public class UITextBox : IUIForm{}
    public class UIDropDown : IUIForm{}
    public class UIContextMenu : IUIForm{} <- maybe not this one
    */
    public class UIWindow : IUIForm {
        public string name => "window";
        public string text => _text;
        string _text = "a window";
        public void change_text(string text) { _text = text; }

        public ui_layer_state layer_state => ui_layer_state.floating;

        public XYPair position { get; set; } = XYPair.Zero;
        public XYPair size { get; set; } = XYPair.One * 250;

        public XYPair top_left => position;
        public XYPair bottom_right => position + size;
        public XYPair top_right => position + (XYPair.UnitX * size.X);
        public XYPair bottom_left => position + (XYPair.UnitY * size.Y);

        public XYPair client_top_left => position + (XYPair.UnitY * top_bar_height);
        public XYPair client_size => size - (XYPair.UnitY * top_bar_height);
        public XYPair client_bottom_right => client_top_left + client_size;

        public XYPair top_bar_size => new XYPair(client_size.X, top_bar_height);

        public XYPair min_window_size = new XYPair(40, 40);
        public XYPair max_window_size = new XYPair(600, 600);

        float top_bar_height = 12f;

        public List<IUIForm> subforms { get; set; } = new List<IUIForm>();

        public Dictionary<string, ISupport2D> collision => _collision;
        Dictionary<string, ISupport2D> _collision = new Dictionary<string, ISupport2D>();

        RenderTarget2D client_render_target;
        RenderTarget2D top_bar_render_target;

        public bool mouse_over => (mouse_interactions.Count > 0);

        public bool has_focus { get; set; } = true;

        bool _draw_collision = false;

        public bool visible => _visible;
        bool _visible = true;

        bool _update_render_targets = true;
        bool _draw_render_targets = true;
        bool _render_targets_need_resize = false;

        public int top_hit_subform { get; set; } = -1;
        public bool top_of_mouse_stack { get; set; } = false;

        public List<string> mouse_interactions => _mouse_interactions;
        List<string> _mouse_interactions = new List<string>();

        bool _resize_handle_R_mo = false;
        bool _resize_handle_B_mo = false;
        bool _resize_handle_both_mo => _resize_handle_R_mo && _resize_handle_B_mo;

        bool _resize_handle_R_grabbed = false;
        bool _resize_handle_B_grabbed = false;
        bool _resize_handle_both_grabbed => _resize_handle_R_grabbed && _resize_handle_B_grabbed;

        bool _grabbed_bar = false;
        Vector2 _bar_mouse_offset = Vector2.Zero;

        bool mdown = false;
        bool mdown_p = false;
        XYPair last_mouse_pos = XYPair.Zero;

        int resize_handle_thickness = 10;

        IUIForm _parent_form;
        bool is_child => _parent_form != null;

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        public UIWindow(IUIForm parent_form = null) {
            _parent_form = parent_form;
            setup();
        }

        public UIWindow(XYPair position, XYPair size, IUIForm parent_form = null) {
            this.position = position;
            this.size = size;
            _parent_form = parent_form;

            setup();
        }

        public void hide() { _visible = false; }
        public void show() { _visible = true;  }
        public void toggle_vis() { _visible = !_visible; }
        public void toggle_vis(bool toggle) { _visible = toggle; }

        public void setup() {
            _collision.Add("form", new BoundingBox2D(XYPair.Zero, size));
            _collision.Add("top_bar", new BoundingBox2D(position, position + (XYPair.UnitX * size.X) + (XYPair.UnitY * top_bar_height)));

            _collision.Add("resize_handle_R", new BoundingBox2D(
                position + (size - (XYPair.UnitX * 6)) - (XYPair.UnitY * size.Y) + (XYPair.UnitX * 3), 
                bottom_right + (XYPair.One * 3)));
            _collision.Add("resize_handle_B", new BoundingBox2D(
                position + (size - (XYPair.UnitY * 6)) - (XYPair.UnitX * size.X) + (XYPair.UnitY * 3),
                bottom_right + (XYPair.One * 3)));

            client_render_target = new RenderTarget2D(EngineState.graphics_device, client_size.X, client_size.Y);
            top_bar_render_target = new RenderTarget2D(EngineState.graphics_device, top_bar_size.X, top_bar_size.Y);
        }

        public bool test_mouse() {
            return UIStandard.test_mouse(ref _collision, ref _mouse_interactions);
        }


        static ISupport2D _mouse_coll_obj_child;
        XYPair parent_pos => _parent_form.position;

        public virtual void update() {
            test_mouse();

            if (is_child) {
                ((BoundingBox2D)_collision["form"]).position = (position + _parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["form"]).SetSize(size.ToVector2());

                ((BoundingBox2D)_collision["top_bar"]).position = (position + _parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["top_bar"]).SetSize((Vector2.UnitX * size.X) + (Vector2.UnitY * top_bar_height));

                ((BoundingBox2D)_collision["resize_handle_R"]).set(
                    ((position + _parent_form.client_top_left) + (size - (XYPair.UnitX * resize_handle_thickness)) - (XYPair.UnitY * size.Y) + (XYPair.UnitX * (resize_handle_thickness / 2))),
                    bottom_right + _parent_form.client_top_left + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());

                ((BoundingBox2D)_collision["resize_handle_B"]).set(
                    ((position + _parent_form.client_top_left) + (size - (XYPair.UnitY * resize_handle_thickness)) - (XYPair.UnitX * size.X) + (XYPair.UnitY * (resize_handle_thickness / 2))),
                    bottom_right + _parent_form.client_top_left + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());


                mdown = Controls.is_pressed(MouseButtons.Left) && EngineState.is_active && Controls.mouse_in_bounds;

                _mouse_coll_obj_child = new Circle2D(Controls.mouse_position_float, 1f);


                _resize_handle_R_mo = GJK2D.test_shapes_simple(_collision["resize_handle_R"], _mouse_coll_obj_child, out _);
                _resize_handle_B_mo = GJK2D.test_shapes_simple(_collision["resize_handle_B"], _mouse_coll_obj_child, out _);

            } else {
                ((BoundingBox2D)_collision["form"]).position = (position).ToVector2();
                ((BoundingBox2D)_collision["form"]).SetSize(size.ToVector2());

                ((BoundingBox2D)_collision["top_bar"]).position = position.ToVector2();
                ((BoundingBox2D)_collision["top_bar"]).SetSize((Vector2.UnitX * size.X) + (Vector2.UnitY * top_bar_height));

                ((BoundingBox2D)_collision["resize_handle_R"]).set(
                    (position + (size - (XYPair.UnitX * resize_handle_thickness)) - (XYPair.UnitY * size.Y) + (XYPair.UnitX * (resize_handle_thickness / 2))),
                    bottom_right + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());

                ((BoundingBox2D)_collision["resize_handle_B"]).set(
                    (position + (size - (XYPair.UnitY * resize_handle_thickness)) - (XYPair.UnitX * size.X) + (XYPair.UnitY * (resize_handle_thickness / 2))),
                    bottom_right + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());


                mdown = Controls.is_pressed(MouseButtons.Left) && EngineState.is_active && Controls.mouse_in_bounds;

                _resize_handle_R_mo = GJK2D.test_shapes_simple(_collision["resize_handle_R"], Controls.mouse_collision_object, out _);
                _resize_handle_B_mo = GJK2D.test_shapes_simple(_collision["resize_handle_B"], Controls.mouse_collision_object, out _);
            }

            //do resize stuff here
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //switch from mouseover to grabbed
                if (_resize_handle_R_mo && _resize_handle_B_mo) {
                    _resize_handle_R_grabbed = true;
                    _resize_handle_B_grabbed = true;
                } else if (_resize_handle_R_mo) {
                    _resize_handle_R_grabbed = true;
                } else if (_resize_handle_B_mo) {
                    _resize_handle_B_grabbed = true;
                }
                
                if (_resize_handle_R_mo || _resize_handle_B_mo) {
                    EngineState.game.IsMouseVisible = false;
                }

            }

            //mouse down, something held
            if (mdown && (_resize_handle_R_grabbed || _resize_handle_B_grabbed || _resize_handle_both_grabbed)) {
                //disable drawing while resizing
                _draw_render_targets = false;

                //size change is basically just mouse delta
                var size_change = Controls.mouse_delta;

                if (size_change.X > 0) {
                    Log.log("fuck");
                }
                var sizefit = size;
                if (size.X > EngineState.resolution.X)
                    sizefit = new XYPair(EngineState.resolution.X, sizefit.Y);
                if (size.Y > EngineState.resolution.Y)
                    sizefit = new XYPair(sizefit.X, EngineState.resolution.Y);
                size = sizefit;

                if (_resize_handle_both_grabbed) {
                    size += size_change;
                } else if (_resize_handle_R_grabbed) {
                    size += (Vector2.UnitX * size_change.X);
                } else if (_resize_handle_B_grabbed) {
                    size += (Vector2.UnitY * size_change.Y);
                }

                float tmpX = size.X;
                float tmpY = size.Y;

                if (Controls.mouse_position.X > EngineState.resolution.X)
                    tmpX = EngineState.resolution.X - top_left.X;
                
                if (Controls.mouse_position.Y > EngineState.resolution.Y) 
                    tmpY = EngineState.resolution.Y - top_left.Y;
                
                size = new XYPair(tmpX, tmpY);
            }

            if (!mdown && mdown_p && (_resize_handle_R_grabbed || _resize_handle_B_grabbed)) {
                _render_targets_need_resize = true;
                _draw_render_targets = true;
                _resize_handle_R_grabbed = false;
                _resize_handle_B_grabbed = false;          
                EngineState.game.IsMouseVisible = true;
            }


            if (_resize_handle_R_grabbed || _resize_handle_B_grabbed) {
                last_mouse_pos = Controls.mouse_position;
                mdown_p = mdown;
                return;
            }




            //below here is window movement
            //mouse just clicked
            if (mdown && !mdown_p && top_of_mouse_stack) {
                //if clicking top bar, grab the top bad
                if (GJK2D.test_shapes_simple(_collision["top_bar"], Controls.mouse_collision_object, out _))
                    _grabbed_bar = true;                
            }

            //mouse down and bar grabbed, position needs to change according to mouse delta
            if (mdown && _grabbed_bar) {
                this.position += Controls.mouse_delta;
            }

            //mouse released, release bar
            if (!mdown && mdown_p) {
                _grabbed_bar = false;

                var tmp = this.position;

                if (this.top_left.X < 0)
                    tmp = new XYPair(0, tmp.Y);
                if (this.top_left.Y < 0)
                    tmp = new XYPair(tmp.X, 0);

                if (this.bottom_right.X > EngineState.resolution.X)
                    tmp = new XYPair(EngineState.resolution.X - size.X, tmp.Y);
                if (this.bottom_right.Y > EngineState.resolution.Y)
                    tmp = new XYPair(tmp.X, EngineState.resolution.Y - size.Y);

                position = tmp;
            }


            //subform updates
            for (int i = 0; i < subforms.Count; i++) {
                subforms[i].update();
            }

            last_mouse_pos = Controls.mouse_position;
            mdown_p = mdown;

            if (_render_targets_need_resize) {
                top_bar_render_target = new RenderTarget2D(EngineState.graphics_device, top_bar_size.X, top_bar_size.Y);
                client_render_target = new RenderTarget2D(EngineState.graphics_device, client_size.X, client_size.Y);
                _render_targets_need_resize = false;
            }

        }



        public Color color_borders = Color.HotPink;
        public Color color_bg = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_header = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_header_selected = Color.HotPink;
        public Color color_header_text = Color.HotPink;
        public Color color_header_text_selected = Color.FromNonPremultiplied(25,25,25,255);
        public Color color_drag_border = Color.DeepPink;

        public Action internal_draw_action;
        public Action draw_action;

        public void render_internal() {
            if (!_update_render_targets || !_visible) return;
            
            EngineState.graphics_device.SetRenderTarget(top_bar_render_target);

            if (has_focus)
                EngineState.graphics_device.Clear(color_header_selected);
            else
                EngineState.graphics_device.Clear(color_header);

            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);

            if (has_focus)
                Draw2D.text("pf", text, (XYPair.Right * 4f), color_header_text_selected);
            else
                Draw2D.text("pf", text, (XYPair.Right * 4f), color_header_text);

            //Draw2D.line(size.X - 45, 0, size.X - 45, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 30, 0, size.X - 30, size.Y, 1f, Color.Black);
            //Draw2D.line(size.X - 15, 0, size.X - 15, size.Y, 1f, Color.Black);

            EngineState.spritebatch.End();

            foreach (IUIForm subform in subforms) {
                subform.render_internal();
            }

            EngineState.graphics_device.SetRenderTarget(client_render_target);
            EngineState.graphics_device.Clear(color_bg);

            EngineState.spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None);
            
            foreach (IUIForm subform in subforms) {
                subform.draw();
            }

            internal_draw_action?.Invoke();

            EngineState.spritebatch.End();
        }

        public void draw() {
            if (!_visible) return;
            Draw2D.fill_square(top_left, top_bar_size, Color.FromNonPremultiplied(color_header.R, color_header.G, color_header.B, 128));
            Draw2D.fill_square(client_top_left, client_size, Color.FromNonPremultiplied(color_bg.R, color_bg.G, color_bg.B, 128));

            if (_draw_render_targets) {
                Draw2D.image(top_bar_render_target, top_left, top_bar_size, Color.White);
                Draw2D.image(client_render_target, client_top_left, client_size, Color.White);
            }

            Draw2D.square(top_left, bottom_right, 1f, color_borders);
            Draw2D.square(top_left, top_left + top_bar_size, 1f, color_borders);

            if ((_resize_handle_R_mo || _resize_handle_R_grabbed) && top_of_mouse_stack) { Draw2D.line(top_right, bottom_right, 3f, color_drag_border); }
            if ((_resize_handle_B_mo || _resize_handle_B_grabbed) && top_of_mouse_stack) { Draw2D.line(bottom_left, bottom_right, 3f, color_drag_border); }

            draw_action?.Invoke();

            /*
            if (_draw_collision) {
                foreach (string is2d in collision.Keys) {
                    _collision[is2d].Draw(_mouse_interactions.Contains(is2d) ? Color.MediumVioletRed : Color.MediumPurple);
                }
            }

            foreach (IUIForm subform in subforms) {

                foreach (string is2d in subform.collision.Keys) {
                    subform.collision[is2d].Draw(_mouse_interactions.Contains(is2d) ? Color.MediumVioletRed : Color.MediumPurple);
                }
            }
            */
        }

    }

    public class ConsoleWindow : UIWindow {
        public ConsoleWindow(IUIForm parent_form = null) : base(parent_form) {
            subforms.Add(new Console(3, 3, client_size.X - 3, client_size.Y - 3, this));
            change_text("console");
        }

        public ConsoleWindow(XYPair position, XYPair size, IUIForm parent_form = null) : base(position, size, parent_form) {
            subforms.Add(new Console(3, 3, client_size.X - 3, client_size.Y - 3, this));
            change_text("console");
        }

        public override void update() {
            base.update();

            subforms[0].size = client_size - 3;
        }
    }
}
