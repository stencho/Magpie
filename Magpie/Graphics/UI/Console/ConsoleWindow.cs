﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ConsoleInput;
using Magpie.Engine;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Graphics.UI {
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

        public Dictionary<string, Shape2D> collision => _collision;

        public ui_layer_state layer_state => ui_layer_state.floating;

        Dictionary<string, Shape2D> _collision = new Dictionary<string, Shape2D>();

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

            cih = new ConsoleInputHandler(110, 10, width - 1);

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

            Draw2D.fill_square(0, 0, size.X, size.Y, Color.FromNonPremultiplied(25, 25, 25, 255));

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
                        Draw2D.text("pf", split[i], top_left + XYPair.One + pos + (XYPair.UnitY * msy_overall), text_shadow_color);

                    Draw2D.text("pf", split[i], top_left + pos + (XYPair.UnitY * msy_overall), current_color);

                    if (bold)
                        Draw2D.text("pf", split[i], top_left + pos + (XYPair.UnitY * msy_overall) + XYPair.UnitX, current_color);

                    pos.X += msx;
                }

                last_msy = msy_overall;
                msy_overall += pos.Y + msy + message_gap + 2;


                //Draw2D.line((int)pos.X, (int)msy_overall, client_size.X, (int)msy_overall, 1f, Color.Red);
                lock(Log.data)
                    Draw2D.fill_square(new XYPair(1, last_msy + 1), new XYPair(notch_width, msy_overall - last_msy - 1), Log.data[x].side_notch_color);

                c++;
            }


            //Console.WriteLine(edited);

            //scroll bar
            // Draw2D.fill_square(new XYPair(client_size.X - scroll_bar_width, 0), new XYPair(client_size.X, client_size.Y - (text_box_edge_margin) - text_box_height), Color.Red);

            //text box
            Draw2D.image("gradient_vertical", new XYPair(0, client_size.Y - (text_box_height * 1.8f)), new XYPair(client_size.X, (text_box_height * 2f)), Color.Black);
            //Draw2D.fill_square(new XYPair(text_box_edge_margin/2, client_size.Y - text_box_height), new XYPair(client_size.X - (text_box_edge_margin/2), text_box_height), Color.Red);

            cih.set_width_lock_to_char_width(this.size.X - (text_box_edge_margin * 2) - (cih.Width / 40 / 2));
            cih.set_position(new XYPair((this.size.X / 2) - (cih.Width / 2) + 1, client_size.Y - text_box_height + 3));
            cih.draw();


            //bottom scroll marker
            //Draw2D.line(0, client_size.Y - bottom_scroll_pos, client_size.X, client_size.Y - bottom_scroll_pos, 1f, (msy_overall > client_size.Y - bottom_scroll_pos) ? Color.HotPink : Color.Purple);
        }

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }
    }

}
