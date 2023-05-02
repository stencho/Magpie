using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support2D;
using Magpie.Engine;
using Microsoft.Xna.Framework.Graphics;
using static Magpie.Engine.Collision.Collision2D;
using static Magpie.Engine.Controls;
using Microsoft.Xna.Framework;

namespace Magpie.Graphics.UI {
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

        public Dictionary<string, Shape2D> collision => _collision;
        Dictionary<string, Shape2D> _collision = new Dictionary<string, Shape2D>();

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

        public IUIForm parent_form { get; set; }

        bool is_child => parent_form != null;

        public string list_subforms() {
            return UIStandard.list_subforms(subforms);
        }

        public void add_subform(IUIForm subform) {
            subform.parent_form = this;
            
            subforms.Add(subform);
        }

        public UIWindow(IUIForm parent_form = null) {
            parent_form = parent_form;
            setup();
        }

        public UIWindow(XYPair position, XYPair size, IUIForm parent_form = null) {
            this.position = position;
            this.size = size;
            parent_form = parent_form;

            setup();
        }

        public void hide() { _visible = false; }
        public void show() { _visible = true; }
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


        static Shape2D _mouse_coll_obj_child;
        XYPair parent_pos => parent_form.position;

        public virtual void update() {
            test_mouse();

            if (is_child) {
                ((BoundingBox2D)_collision["form"]).position = (position + parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["form"]).SetSize(size.ToVector2());

                ((BoundingBox2D)_collision["top_bar"]).position = (position + parent_form.client_top_left).ToVector2();
                ((BoundingBox2D)_collision["top_bar"]).SetSize((Vector2.UnitX * size.X) + (Vector2.UnitY * top_bar_height));

                ((BoundingBox2D)_collision["resize_handle_R"]).set(
                    ((position + parent_form.client_top_left) + (size - (XYPair.UnitX * resize_handle_thickness)) - (XYPair.UnitY * size.Y) + (XYPair.UnitX * (resize_handle_thickness / 2))),
                    bottom_right + parent_form.client_top_left + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());

                ((BoundingBox2D)_collision["resize_handle_B"]).set(
                    ((position + parent_form.client_top_left) + (size - (XYPair.UnitY * resize_handle_thickness)) - (XYPair.UnitX * size.X) + (XYPair.UnitY * (resize_handle_thickness / 2))),
                    bottom_right + parent_form.client_top_left + (XYPair.One * (resize_handle_thickness / 2)).ToVector2());


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
                lock(subform)
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
}
