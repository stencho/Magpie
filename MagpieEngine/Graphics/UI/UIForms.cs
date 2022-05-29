using Magpie.Engine;
using Magpie.Engine.Collision.Support2D;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;
using static Magpie.Engine.Math2D;
//set up UI left/right click checking and passthrough
//   i.e. if an element was clicked, do the click here, if not then do the click in the editing world layer
//set up dialog focus system
//set up keybind tag system to group them for each different editor in the IDE


namespace Magpie.Graphics.UI {
    public enum form_type {
        NONE,
        BUTTON,
        LABEL,
        PANEL,
        WINDOW,
        SCROLL_BAR
    }

    public enum dock_position {
        FREE,
        LEFT,
        RIGHT,
        TOP,
        BOTTOM,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        CENTER
    }

    public enum text_align_h {
        CENTER,
        LEFT,
        RIGHT
    }

    public enum text_align_v {
        CENTER,
        TOP,
        BOTTOM
    }


    public class UIButton : UIForm {
        public override form_type type => form_type.BUTTON;

        public string text { get; set; } = "";
        public XYPair text_margin = new XYPair(5, 2);

        public text_align_h text_alignment_h = text_align_h.LEFT;

        public bool text_shadow = true;
        public bool clicking = false;

        public bool autosize { get; set; } = true;

        private XYPair get_txt_size() => measure_string("pf", text);

        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            base.mouse_update(mouse, mouse_local, check_mouse);

            if (!mouse_hover) {
                clicking = false;
            }
        }

        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {
            if (mouse == null) return false;
            if (DigitalControlBindings.bind_pressed("ui_select") && mouse_hover) {

                if (DigitalControlBindings.bind_just_pressed("ui_select")) {
                    this.collisions["button"].click?.Invoke();
                    this.collisions["button"].click_option?.Invoke(click_options);
                }

                clicking = true;
                return true;
            }

            if (DigitalControlBindings.bind_released("ui_select")) {
                clicking = false;
            }
            return false;
        }

        public void mouse_hit_force() {
            if (clicking == false) {             
                this.collisions["button"].click?.Invoke();
                this.collisions["button"].click_option?.Invoke(click_options);
            }

            clicking = true;
        }

        private void setup(XYPair XY, XYPair size, float z, string name, string text, bool autos) { setup(XY.X, XY.Y, size.X, size.Y, z, name, text, autos); }
        private void setup(XYPair XY, int size_x, int size_y, float z, string name, string text, bool autos) { setup(XY.X, XY.Y, size_x, size_y, z, name, text, autos); }
        private void setup(int X, int Y, int size_x, int size_y, float z, string name, string text, bool autos) {
            this.z = z;
            this.name = name;
            this.text = text;
            this.autosize = autos;


            //if we've got autosize on, this is ez mode
            //just find out how wide/tall it is and draw it centered w/ a margin
            if (autosize) {
                this.position = new XYPair(X, Y);
                var ms = get_txt_size();
                this.size = (text_margin * 2) + ms;

            //if we've got autosize off, we need to care about whether the text is wider than the button, what size the button actually is (min size, shape etc), the text alignment
            } else {
                this.position = new XYPair(X, Y);
                this.size = new XYPair(size_x, size_y);
            }

            hover_aabb = new BoundingBox2D(position, position + size); 
            if (this.collisions.Count == 0)
                this.collisions.Add("button", new BoundingBox2D(0, 0, size.X, size.Y));
        }
                
        public UIButton(int X, int Y, XYPair size, string name = "button", string text = "button", bool autosize = false) : base(X, Y) { setup(X, Y, size.X, size.Y, 0, name, text, autosize); }
        public UIButton(XYPair position, XYPair size, string name = "button", string text = "button", bool autosize = false) : base(position) { setup(position, size, 0, name, text, autosize); }
        public UIButton(int X, int Y, XYPair size, float Z, string name = "button", string text = "button", bool autosize = false) : base(X, Y, Z) { setup(X, Y, size.X, size.Y, Z, name, text, autosize); }
        public UIButton(XYPair position, XYPair size, float Z, string name = "button", string text = "button", bool autosize = false) : base(position, Z) { setup(position, size, Z, name, text, autosize); }

        public void set_click_delegate(ui_delegate click_delegate) {
            this.collisions["button"].click = click_delegate;
        }

        object click_options;
        public void set_click_delegate(ui_delegate_option click_delegate, object data) {
            click_options = data;
            this.collisions["button"].click_option = click_delegate;
        }

        public override void update() {
            hover_aabb.top_left.X = position.X;
            hover_aabb.top_left.Y = position.Y;
            hover_aabb.bottom_right.X = position.X + size.X;
            hover_aabb.bottom_right.Y = position.Y + size.Y;

            this.collisions["button"].bottom_right.X = size.X;
            this.collisions["button"].bottom_right.Y = size.Y;

        }

        float hover_color_lerp = 0f;
        public override void draw() {        
            //lerp and clamp a value to use for interpolating the background colour of the buttons when hovered
            if (mouse_hover) {
                if (hover_color_lerp < 1f) {
                    hover_color_lerp += 5f * Clock.frame_time_delta;
                } else if (hover_color_lerp > 1f) hover_color_lerp = 1f;
            } else {
                if (hover_color_lerp > 0f) {
                    hover_color_lerp -= 2.5f * Clock.frame_time_delta;
                } else if (hover_color_lerp < 0f) hover_color_lerp = 0f;
            }

            //draw main square + outline
            if (clicking) {
                Draw2D.fill_square(position, size, Color.Red);
            } else {
                Draw2D.fill_square(position, size, Color.Lerp(Color.Plum, Color.HotPink, hover_color_lerp));
            }

            Draw2D.square(position, position + size, 1f, Color.Red);
            
            //get the text size
            var ts = (get_txt_size()) ;
            
            //find text position, Y is always half way down, X should be positioned to left/right align
            var text_offset = XYPair.Zero;
            switch (text_alignment_h) {
                case text_align_h.CENTER:
                    text_offset.X = (size.X / 2);
                    break;
                case text_align_h.LEFT:
                    text_offset.X = text_margin.X + (ts.X / 2);
                    break;
                case text_align_h.RIGHT:
                    text_offset.X = size.X - text_margin.X - (ts.X / 2);
                    break;
            }

            text_offset.Y = (size.Y / 2);// - (ts.Y / 2);//text_margin.Y + (ts.Y / 2);

            if (text_shadow) {
                Draw2D.text_shadow("pf", text, position.ToVector2() + text_offset, Vector2.One, Color.White, Color.Black, 0f, (.5f * ts), 1f, SpriteEffects.None);

            } else {
                Draw2D.text("pf", text, position.ToVector2() + text_offset, Color.Black, Draw2D.text_rotation.NONE, (.5f * ts), 1f, SpriteEffects.None, 1);
            }
        }
    }

    public class UILabel : UIForm {
        public new form_type type => form_type.LABEL;

        public string text { get; set; } = "";
        XYPair text_margin = new XYPair(4, 2);

        public bool draw_background = true;
        public bool text_shadow = true;

        private XYPair get_txt_size() => measure_string("pf", text);

        public Color bg_color = Color.Plum;

        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {
            if (Math2D.point_within_square(hover_aabb.top_left, hover_aabb.bottom_right, mouse_local) && DigitalControlBindings.bind_just_pressed("ui_select")) {
                this.collisions["label"].click?.Invoke();
                return false;
            }
            return false;
        }

        private void setup(XYPair XY, XYPair size, float z, string name, string text, bool draw_bg) { setup(XY.X, XY.Y, size.X, size.Y, z, name, text, draw_bg); }
        private void setup(XYPair XY, int size_x, int size_y, float z, string name, string text, bool draw_bg) { setup(XY.X, XY.Y, size_x, size_y, z, name, text, draw_bg); }
        private void setup(int X, int Y, int size_x, int size_y, float z, string name, string text, bool draw_bg) {
            this.z = z;
            this.name = name;
            this.text = text;
            draw_background = draw_bg;

            //if we've got autosize on, this is ez mode
            //just find out how wide/tall it is and draw it centered w/ a margin
            this.position = new XYPair(X, Y);
            var ms = get_txt_size();
            this.size = (text_margin * 2) + ms;

            hover_aabb = new BoundingBox2D(position, position + size);
            if (this.collisions.Count == 0)
                this.collisions.Add("label", hover_aabb);
        }

        public UILabel(int X, int Y, XYPair size, string name = "label", string text = "label", bool draw_bg = false) : base(X, Y) { setup(X, Y, size.X, size.Y, 0, name, text, draw_bg); }
        public UILabel(XYPair position, XYPair size, string name = "label", string text = "label", bool draw_bg = false) : base(position) { setup(position, size, 0, name, text, draw_bg); }
        public UILabel(int X, int Y, XYPair size, float Z, string name = "label", string text = "label", bool draw_bg = false) : base(X, Y, Z) { setup(X, Y, size.X, size.Y, Z, name, text, draw_bg); }
        public UILabel(XYPair position, XYPair size, float Z, string name = "label", string text = "label", bool draw_bg = false) : base(position, Z) { setup(position, size, Z, name, text, draw_bg); }

        public void set_click_delegate(ui_delegate click_delegate) {
            this.collisions["label"].click = click_delegate;
        }

        public override void update() {
            size = Draw2D.get_txt_size_pf(text) + (text_margin * 2);

            hover_aabb.top_left.X = position.X;
            hover_aabb.top_left.Y = position.Y;
            hover_aabb.bottom_right.X = position.X + size.X;
            hover_aabb.bottom_right.Y = position.Y + size.Y;

            this.collisions["label"] = hover_aabb;
        }
        public text_align_h alignment = text_align_h.CENTER;
        public override void draw() {
            //get the text size
            var ts = (get_txt_size());
            
            //draw bg
            if (draw_background) {
                if (alignment == text_align_h.CENTER) {
                    Draw2D.fill_square(position - (ts / 2) - (text_margin), size, bg_color);
                    Draw2D.square(position - (ts / 2) - (text_margin), (position - (ts / 2)) + (size - (text_margin)), 1f, Color.Red);
                } else if (alignment == text_align_h.LEFT) {
                    Draw2D.fill_square(position - text_margin, size, bg_color);
                    Draw2D.square(position - text_margin, position + (size - (text_margin)), 1f, Color.Red);
                }
            }


            if (alignment == text_align_h.CENTER) {
                if (text_shadow) {
                    Draw2D.text_shadow("pf", text, position.ToVector2(), Vector2.One, Color.White, Color.Black, 0f, (.5f * ts), 1f, SpriteEffects.None);

                } else {
                    Draw2D.text("pf", text, position.ToVector2(), Color.Black, Draw2D.text_rotation.NONE, (.5f * ts), 1f, SpriteEffects.None, 1);
                }            
            } else if (alignment == text_align_h.LEFT) {

                if (text_shadow) {
                    Draw2D.text_shadow("pf", text, position.ToVector2(), Vector2.One, Color.White, Color.Black, 0f, (0f*ts), 1f, SpriteEffects.None);

                } else {
                    Draw2D.text("pf", text, position.ToVector2(), Color.Black, Draw2D.text_rotation.NONE, (0f * ts), 1f, SpriteEffects.None, 1);
                }
            }
        }
    }


    public class UIScrollBarVert : UIForm {
        public override form_type type => form_type.SCROLL_BAR;

        public const int button_height = 20;
        public const int bottom_offset = 16;

        public const int default_bar_width = 7;

        public int bar_width = default_bar_width;
        
        public float min_bar_height_multi = 0.5f;
        public float min_bar_height => size.Y * min_bar_height_multi;
        public float min_bar_height_half => min_bar_height / 2f;        

        public float bar_pos = 0.5f;
        public float bar_zoom = 0.5f;

        public UIScrollBarVert(int X, int Y, int W, int H) : base(X, Y, 0) {
            position = new XYPair(X, Y);
            size = new XYPair(W, H);
        }

        public override void draw() {
            //SLIDER
            Draw2D.fill_square(position + (XYPair.UnitY * (bar_pos * (size.Y))) - (XYPair.UnitY * min_bar_height_half) , (XYPair.UnitX * bar_width) + (XYPair.UnitY * min_bar_height), Color.Red);
            Draw2D.line(position + (XYPair.UnitY * (bar_pos * (size.Y))), position + (XYPair.UnitY * (bar_pos * (size.Y))) + (XYPair.UnitX * bar_width), 2f, Color.Purple);
            //OUTLINE
            Draw2D.square(position, position + size, 1f, Color.Red);
        }
        
        public void update(int X_right, int top_offset, int height, int bar_width = default_bar_width) {
            
            position.X = X_right - bar_width;
            position.Y = top_offset;

            size.X = bar_width;
            size.Y = height - top_offset - bottom_offset;

            base.update();
        }
    }

    public class UIPanel : UIForm {
        public override form_type type => form_type.PANEL;

        internal RenderTarget2D form_rt;

        public bool draw_border { get; set; } = true;

        public override void draw() {
            if (form_rt == null)
                form_rt = new RenderTarget2D(EngineState.graphics_device, size.X, size.Y);
            // if (mouse_hover)
            //     Draw2D.fill_square(position, size, Color.Red);
            // else
            Draw2D.fill_square(position, size, Color.Plum);

            Draw2D.image(form_rt, position, size, Color.White);

            if (draw_border)
                Draw2D.square(position, position + size, 1f, Color.Red);
        }
        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            base.mouse_update(mouse, mouse_local, check_mouse);
        }
        private void setup(XYPair XY, XYPair size, float z, string name) { setup(XY.X, XY.Y, size.X, size.Y, z, name); }
        private void setup(XYPair XY, int size_x, int size_y, float z, string name) { setup(XY.X, XY.Y, size_x, size_y, z, name); }
        private void setup(int X, int Y, int size_x, int size_y, float z, string name) {
            this.z = z;
            this.name = name;

            //if we've got autosize on, this is ez mode
            //just find out how wide/tall it is and draw it centered w/ a margin
            this.position = new XYPair(X, Y);
            this.size = new XYPair(size_x, size_y);

            hover_aabb = new BoundingBox2D(position, position + size);
            if (this.collisions.Count == 0)
                this.collisions.Add("panel", hover_aabb);
        }

        public UIPanel(int X, int Y, XYPair size, string name = "panel") : base(X, Y) { setup(X, Y, size.X, size.Y, 0, name); }
        public UIPanel(XYPair position, XYPair size, string name = "panel") : base(position) { setup(position, size, 0, name); }
        public UIPanel(int X, int Y, XYPair size, float Z, string name = "panel") : base(X, Y, Z) { setup(X, Y, size.X, size.Y, Z, name); }
        public UIPanel(XYPair position, XYPair size, float Z, string name = "panel") : base(position, Z) { setup(position, size, Z, name); }
    }
    
    public class UIImageView : UIForm {
        public override form_type type => form_type.PANEL;

        internal RenderTarget2D form_rt;
        public Texture2D image {
            get {
                return _image;
            }
            set {
                _image = value;
                image_resolution = new XYPair(_image.Bounds.Width, _image.Bounds.Height);
                //focus_pos = map_resolution / 2;
            }
        }
        bool image_loaded => _image != null;
        Texture2D _image;

        XYPair image_resolution;
        XYPair ui_center => this.size / 2;
        XYPair image_center => image_resolution / 2;
        float zoom_multi = 1f;

        float scroll_speed_base = .2f;
        float scroll_speed;
        Vector2 scroll_pos = Vector2.Zero;
        public Vector2 focus_pos = Vector2.Zero;
        bool scrolled_off_center = false;

        bool stop_draw = false;

        public bool draw_border { get; set; } = true;

        public override void draw() {
            if (form_rt == null)
                form_rt = new RenderTarget2D(EngineState.graphics_device, size.X, size.Y);
            Draw2D.fill_square(position, size, Color.Plum);
            Draw2D.image(form_rt, position, size, Color.White);
            if (draw_border)
                Draw2D.square(position, position + size, 1f, Color.Red);
        }
        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            base.mouse_update(mouse, mouse_local, check_mouse);
        }


        public override void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) {
            var rt = gd.GetRenderTargets();
            if (!stop_draw) {

                gd.SetRenderTarget(form_rt);
                gd.Clear(Color.Plum);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearWrap);

                if (image_loaded) {

                    Draw2D.image(
                        image,
                        ui_center,
                        new XYPair(image_resolution * zoom_multi),
                        Color.White, (scroll_pos * image_resolution));

                    if (scrolled_off_center)
                        Draw2D.cross(ui_center, 10, 10, Color.Red);

                }

                Draw2D.text_shadow("pf",
                   string.Format("{0:F1}", zoom_multi) + "x zoom | window res " + size + " | image res " + image_resolution + "\n" +
                    (scrolled_off_center ?
                        "scroll_pos: " + scroll_pos
                    :
                        "scroll pos|focus pos: " + focus_pos
                        ),

                    Vector2.Zero, Color.White);


                sb.End();

                gd.SetRenderTargets(rt);

            }
            base.internal_stateless_draw(gd, sb);



        }

        RenderTarget2D r_tmp;

        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {
            if ((DigitalControlBindings.bind_just_pressed("ui_select") || DigitalControlBindings.bind_pressed("ui_select")) && !DigitalControlBindings.bind_pressed("ui_alt")) {
               
                scrolled_off_center = true;
                if (Controls.mouse_delta != Vector2.Zero) {
                    if (Controls.mouse_delta.X != 0) {
                        scroll_pos.X -= (Controls.mouse_delta.X / image_resolution.X) / zoom_multi;
                    }
                    if (Controls.mouse_delta.Y != 0) {
                        scroll_pos.Y -= (Controls.mouse_delta.Y / image_resolution.Y) / zoom_multi;
                    }
                }
            }

            if ((DigitalControlBindings.bind_just_pressed("ui_scroll_up"))) {
                if (zoom_multi < 5f)
                    zoom_multi += 0.3f;
            }
            if ((DigitalControlBindings.bind_just_pressed("ui_scroll_down"))) {
                if (zoom_multi > 0f)
                    zoom_multi -= 0.3f;
            }

            if (DigitalControlBindings.bind_pressed("ui_context_menu")) {
                stop_draw = true;
            }

            if (DigitalControlBindings.bind_pressed("ui_context_menu") || DigitalControlBindings.bind_just_released("ui_context_menu")) {
                if (Controls.mouse_state.Position != Controls.mouse_state_prev.Position) {
                    r_tmp = new RenderTarget2D(EngineState.graphics_device, size.X, size.Y);
                    form_rt = r_tmp;
                    GC.Collect();
                }
            }

            if (zoom_multi < 0.1f)
                zoom_multi = 0.1f;
            if (zoom_multi > 5)
                zoom_multi = 5;

            if (DigitalControlBindings.bind_released("ui_context_menu")) {
                stop_draw = false;
            }

            return base.mouse_hit(mouse, mouse_local);
        }

        public override void update() {
            /*
            if (selected) {
                if ((DigitalControlBindings.bind_just_pressed("ui_select") || DigitalControlBindings.bind_pressed("ui_select"))) {
                    if (!DigitalControlBindings.bind_pressed("ui_alt")) {

                    }
                }
                else {

                    scroll_speed = (scroll_speed_base / zoom_multi) * Clock.frame_time_delta;
                    if (DigitalControlBindings.bind_pressed("ui_left")) {
                        scrolled_off_center = true;
                        scroll_pos.X -= scroll_speed;
                    }
                    if (DigitalControlBindings.bind_pressed("ui_right")) {
                        scrolled_off_center = true;
                        scroll_pos.X += scroll_speed;
                    }
                    if (DigitalControlBindings.bind_pressed("ui_up")) {
                        scrolled_off_center = true;
                        scroll_pos.Y -= scroll_speed;
                    }
                    if (DigitalControlBindings.bind_pressed("ui_down")) {
                        scrolled_off_center = true;
                        scroll_pos.Y += scroll_speed;
                    }

                    if (DigitalControlBindings.bind_pressed("ui_back")) {
                        scroll_pos = Vector2.Zero;
                        scrolled_off_center = false;
                    }
                }
            }
            */
            if (scrolled_off_center)
                focus_pos = scroll_pos;
            else
                scroll_pos = focus_pos;


            //focus_pos = parent_core.player_actor.position_xz / parent_core.s_heightfield.size;
            //scroll_delta = XYPair.Zero;

            base.update();
        }


        private void setup(XYPair XY, XYPair size, float z, string name) { setup(XY.X, XY.Y, size.X, size.Y, z, name); }
        private void setup(XYPair XY, int size_x, int size_y, float z, string name) { setup(XY.X, XY.Y, size_x, size_y, z, name); }
        private void setup(int X, int Y, int size_x, int size_y, float z, string name) {
            this.z = z;
            this.name = name;

            //if we've got autosize on, this is ez mode
            //just find out how wide/tall it is and draw it centered w/ a margin
            this.position = new XYPair(X, Y);
            this.size = new XYPair(size_x, size_y);

            hover_aabb = new BoundingBox2D(position, position + size);
            if (this.collisions.Count == 0)
                this.collisions.Add(name, hover_aabb);
        }

        public UIImageView(int X, int Y, XYPair size, string name = "image") : base(X, Y) { setup(X, Y, size.X, size.Y, 0, name); }
        public UIImageView(XYPair position, XYPair size, string name = "image") : base(position) { setup(position, size, 0, name); }
        public UIImageView(int X, int Y, XYPair size, float Z, string name = "image") : base(X, Y, Z) { setup(X, Y, size.X, size.Y, Z, name); }
        public UIImageView(XYPair position, XYPair size, float Z, string name = "image") : base(position, Z) { setup(position, size, Z, name); }
    }
    
    public class UIButtonList : UIPanel {

        public List<UIButton> buttons = new List<UIButton>();

        int text_height = Draw2D.get_txt_size_pf("test").Y;
        int spacing = 2;

        public XYPair mouse_pos_local;

        public UIButtonList(XYPair position, XYPair size, string name = "panel") : base(position, size, name) {
        }

        public UIButtonList(int X, int Y, XYPair size, string name = "panel") : base(X, Y, size, name) {
        }

        public UIButtonList(XYPair position, XYPair size, float Z, string name = "panel") : base(position, size, Z, name) {
        }

        public UIButtonList(int X, int Y, XYPair size, float Z, string name = "panel") : base(X, Y, size, Z, name) {
        }

        public override form_type type => base.type;

        public override void draw() {
            base.draw();

        }

        public UIButton get_button(int index) => buttons[index];
        public UIButton get_button(string name) => buttons[button_index_from_name(name)];

        public int button_index_from_name(string name) => buttons.FindIndex((a) => a.name == name);

        public override void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) {
            gd.SetRenderTarget(form_rt);
            gd.Clear(Color.Transparent);

            sb.Begin();
            foreach (UIButton b in buttons) {
                b.draw();
            }

            sb.End();
        }

        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            if (this.hidden) return;
            base.mouse_update(mouse, mouse_local, check_mouse);
            foreach (UIButton f in buttons) {
                if (((!f.hidden) && DigitalControlBindings.bind_released("ui_alt")) || (f.name == "close_button" && !f.hidden)) {
                    aabb_mouse_sub_local = new XYPair(mouse_local.X - f.position.X, mouse_local.Y - f.position.Y);
                    f.mouse_update(mouse, aabb_mouse_sub_local, check_mouse);
                }
            }
        }
        XYPair aabb_mouse_sub_local;
        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {
            if (this.hidden) return false;
            
            foreach (UIButton b in buttons) {
                if (mouse_hover) {
                    aabb_mouse_sub_local = new XYPair(mouse_local.X - b.position.X, mouse_local.Y - b.position.Y);
                    b.mouse_hit(mouse, aabb_mouse_sub_local);
                }
            }
            return false;
        }
        



        int current_widest = 0;

        void update_widths() {
            foreach (UIButton b in buttons) {
                b.autosize = false;
                b.size.X = current_widest + 12;
            }
        }

        public override void update() {
            this.size.Y = 0;
            int widest = 0;
            int t_wid = 0;
            int spacing_index = 0;


            foreach (UIButton b in buttons) {

                t_wid = Draw2D.get_txt_size_pf(b.text).X;

                b.position.Y = this.size.Y;

                if (t_wid > widest)
                    widest = t_wid;

                if (spacing_index < buttons.Count-1)
                    this.size.Y += b.size.Y + spacing;
                else
                    this.size.Y += b.size.Y;

                spacing_index++;

                b.update();
            }

            if (widest != current_widest) {
                current_widest = widest;
                this.size.X = current_widest + 12;
                update_widths();
            }

            hover_aabb.top_left.X = position.X;
            hover_aabb.top_left.Y = position.Y;
            hover_aabb.bottom_right.X = position.X + size.X;
            hover_aabb.bottom_right.Y = position.Y + size.Y;
        }
    }

    public class UIProgressSlider : UIForm {
        public float value_internal = 0.5f;
        public float value_external => min + ((float)value * (max - min));

        public bool value_changed = false;

        public override object value {
            get {
                return value_internal;
            }
            set {
                if (value.GetType() == typeof(float)) {
                    value_internal = (float)value;
                }
                value_changed = true;
            }
        }

        public float max = 1f;
        public float min = 0f;


        bool built_in_lerp = false;
        public bool allow_clicks = true;
        public bool draw_text = true;


        public UIProgressSlider(XYPair position, XYPair size, string name = "slider") : base(position, 0) {
            this.position = position;
            this.size = size;
            this.name = name;
            hover_aabb = new BoundingBox2D(position, position + size);
            if (this.collisions.Count == 0)
                this.collisions.Add("button", hover_aabb);
        }

        public UIProgressSlider(int X, int Y, int W, int H, string name = "slider") : base(X, Y, 1) {
            this.position.X = X; this.position.Y = Y;
            this.size.X = W; this.size.Y = H;
            this.name = name;
            hover_aabb = new BoundingBox2D(position, position + size);
            if (this.collisions.Count == 0)
                this.collisions.Add("button", hover_aabb);
        }

        public override form_type type => base.type;

        public override void draw() {
            value_changed = false;

            Draw2D.fill_square(position, size, Color.Plum);
            Draw2D.fill_square(position, (XYPair.UnitX * (int)(size.X * value_internal))+(XYPair.UnitY * size.Y), Color.Red);
            Draw2D.square(position, position + size, 1f,  Color.Red);
        }

        public override void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) {
            base.internal_stateless_draw(gd, sb);
        }

        bool mousedown = false;
        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {

            return base.mouse_hit(mouse, mouse_local);
        }
        bool clicking = false;
        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {

            
            base.mouse_update(mouse, mouse_local, check_mouse);
            if (DigitalControlBindings.bind_pressed("ui_select")) {
                mousedown = true;
            }

            if (mousedown && mouse_hover) {
                Console.WriteLine(name);
                //Console.WriteLine(mouse);
                //Console.WriteLine(mouse_local);
                Console.WriteLine();
            }
            if (DigitalControlBindings.bind_released("ui_select")) {
                mousedown = false;
            }


            if (!mouse_hover) {
                clicking = false;
            }
        }
        public override void update() {
            hover_aabb.top_left.X = position.X;
            hover_aabb.top_left.Y = position.Y;
            hover_aabb.bottom_right.X = position.X + size.X;
            hover_aabb.bottom_right.Y = position.Y + size.Y;

        }
    }
}
