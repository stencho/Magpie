
using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Math2D;
using static Magpie.Engine.Collision.Collision2D;
using Magpie.Graphics.UI;
using static Magpie.Engine.Controls;

namespace Magpie.Graphics {
    public class UIWindow : UIForm {
        public override form_type type => form_type.WINDOW;
        public readonly int top_handle_height = 16;

        public string title_text = "title";

        private XYPair get_txt_size(string txt) => measure_string("pf", txt);
        public XYPair center_pos => position + (size / 2);

        public Dictionary<string, UIForm> sub_forms = new Dictionary<string, UIForm>();

        internal XYPair size_p = XYPair.Zero;

        public RenderTarget2D form_rt;
        internal GBuffer gbuffer;

        public XYPair aabb_mouse_sub_local;

        public XYPair mouse_pos_local;

        public bool selected = false;
        bool locked_size;
        public bool locked_movement = false;

        public bool hide_close_button = false;

        public XYPair minimum_size = XYPair.One * 25;
        public XYPair maximum_size = XYPair.One * 666;

        public void lock_size(XYPair size) {
            this.size = size;
            minimum_size = size;
            maximum_size = size;
        }

        public ui_delegate draw_background_3D_delegate { get => _draw_background_3D_delegate; set { _draw_background_3D_delegate = value; } }
        public ui_delegate draw_foreground_3D_delegate { get => _draw_foreground_3D_delegate; set { _draw_foreground_3D_delegate = value; } }

        private ui_delegate _draw_background_3D_delegate;
        public ui_delegate draw_background_2D_delegate;

        private ui_delegate _draw_foreground_3D_delegate;
        public ui_delegate draw_foreground_2D_delegate;
        // RenderTarget2D double_buffer;

        public override void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            if (this.hidden) return;
            foreach (UIForm sf in sub_forms.Values) {
                if (((!sf.hidden) && DigitalControlBindings.bind_released("ui_alt")) || (sf.name == "close_button" && !sf.hidden)) {
                    aabb_mouse_sub_local = new XYPair(mouse_local.X - sf.position.X, mouse_local.Y - sf.position.Y);
                    sf.mouse_update(mouse, aabb_mouse_sub_local, mouse_over_by_update);
                }
            }
        }

        public override bool mouse_hit(XYPair mouse, XYPair mouse_local) {
            if (this.hidden) return false;

            foreach (UIForm sf in sub_forms.Values) {
                if (((!sf.hidden) && DigitalControlBindings.bind_released("ui_alt")) || (sf.name == "close_button" && !sf.hidden && !hide_close_button) && Math2D.point_within_square(hover_aabb.top_left, hover_aabb.bottom_right, mouse_local)) {
                    aabb_mouse_sub_local = new XYPair(mouse_local.X - sf.position.X, mouse_local.Y - sf.position.Y);
                    sf.mouse_hit(mouse, aabb_mouse_sub_local); 
                }
            }

            hide_all_subforms = false;

            if (((DigitalControlBindings.bind_just_pressed("ui_select") || DigitalControlBindings.bind_pressed("ui_select")) || (DigitalControlBindings.bind_just_pressed("ui_context_menu") || DigitalControlBindings.bind_pressed("ui_context_menu"))) && Math2D.point_within_square(hover_aabb.top_left, hover_aabb.bottom_right, mouse)) {

                //collidin w/ overall window
                if (DigitalControlBindings.bind_pressed("ui_alt")) {

                    //moving window
                    if (DigitalControlBindings.bind_pressed("ui_select")) {
                        this.position += mouse_delta;
                        setup(this.position.X, this.position.Y, this.size.X, this.size.Y);


                    } else if (DigitalControlBindings.bind_pressed("ui_context_menu") && !locked_size) {

                        if (mouse_state.Position != mouse_state_prev.Position) {
                            gbuffer.change_resolution(EngineState.graphics_device, size.X, size.Y);
                            GC.Collect();
                            form_rt = gbuffer.rt_diffuse;
                        }

                        hide_all_subforms = true;

                        
                        if (mouse.X > position.X && mouse.Y > position.Y) {
                            //bottom right
                            this.size += mouse_delta;
                        }

                        //forbidden resize methods
                        /*
                         if (mouse.X < center_pos.X && mouse.Y < center_pos.Y) {
                            //top left
                            this.position += Controls.mouse_delta;
                            this.size -= Controls.mouse_delta;


                        } else
                         
                         
                        
                         else if (mouse.position.X < center_pos.X && mouse.position.Y > center_pos.Y) {
                            //bottom left
                            this.size.X -= (int)Controls.mouse_delta.X;
                            this.position.X += (int)Controls.mouse_delta.X;
                            this.size.Y += (int)Controls.mouse_delta.Y;

                        }*/ /* else if (mouse.position.X > center_pos.X && mouse.position.Y < center_pos.Y) {
                            //top right
                            this.position.Y += (int)Controls.mouse_delta.Y;


                            this.size.Y -= (int)Controls.mouse_delta.Y;
                            this.size.X += (int)Controls.mouse_delta.X;

                        }*/

                        this.size = clamp(this.size, minimum_size.X, minimum_size.Y, maximum_size.X, maximum_size.Y);

                        setup(this.position.X, this.position.Y, this.size.X, this.size.Y);
                    }
                }

                return true;
            }


            if (DigitalControlBindings.bind_released("ui_context_menu")) {
                hide_all_subforms = false;
            }

            if (DigitalControlBindings.bind_just_released("ui_context_menu")) {
                //form_rt = new RenderTarget2D(Renderer.gd, size.X, size.Y);
                gbuffer.CreateInPlace(EngineState.graphics_device, size.X, size.Y);
                GC.Collect();

                form_rt = gbuffer.rt_diffuse;

                hide_all_subforms = false;

            }
            return false;

        }
        /*

        public override void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) {
            gd.SetRenderTarget(form_rt);
            gd.Clear(Color.Transparent);

            sb.Begin();

            foreach (UIForm f in sub_forms.OrderBy(a => a.z)) {
                if ((!f.hidden && !hide_all_subforms) || (f.name == "close_button" && hide_all_subforms)) continue;
                f.draw();
            }

            //Draw2D.fill_square(0, 0, size.X, size.Y, Color.Green);
            sb.End();
        }
        */
        public override void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) {
            foreach (UIForm f in sub_forms.Values) {
                if (((!f.hidden && !hide_all_subforms) || (f.name == "close_button" && !hide_close_button)) && (f.type == form_type.PANEL)) continue;
                f.internal_stateless_draw(EngineState.graphics_device, EngineState.spritebatch);
            }

            if (hide_all_subforms) return;

            gd.SetRenderTargets(gbuffer.buffer_targets);
            Scene.clear_buffer();

            gd.DepthStencilState = DepthStencilState.Default;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            gd.BlendState = BlendState.AlphaBlend;
            draw_background_3D_delegate?.Invoke();

            sb.Begin();
            draw_background_2D_delegate?.Invoke();

            foreach (UIForm f in sub_forms.Values.OrderBy(a => a.z)) {
                if (f.hidden) continue;
                f.draw();
            }

            draw_foreground_2D_delegate?.Invoke();
            sb.End();

            gd.DepthStencilState = DepthStencilState.Default;
            gd.RasterizerState = RasterizerState.CullCounterClockwise;
            gd.BlendState = BlendState.AlphaBlend;
            draw_foreground_3D_delegate?.Invoke();

        }

        public bool mouse_over_by_update = false;
        bool hide_all_subforms = false;

        public override void update() {
            mouse_pos_local = new XYPair(mouse_state.Position.X - position.X, mouse_state.Position.Y - position.Y);

            if (mouse_pos_local.X < size.X && mouse_pos_local.Y < size.Y && mouse_pos_local.X > 0 && mouse_pos_local.Y > 0) {
                mouse_over_by_update = true;
            } else mouse_over_by_update = false;
            
            if (form_rt == null && EngineState.graphics_device != null && !hide_all_subforms) {
                //form_rt = new RenderTarget2D(Renderer.gd, size.X, size.Y);
                gbuffer = new GBuffer();
                GC.Collect();
                gbuffer.CreateInPlace(EngineState.graphics_device, size.X, size.Y);
                form_rt = gbuffer.rt_diffuse;
                //form_rt_normal = new RenderTarget2D(Renderer.gd, size.X, size.Y);
                //form_rt_depth = new RenderTarget2D(Renderer.gd, size.X, size.Y);
            }

            hover_aabb.top_left.X = position.X;
            hover_aabb.top_left.Y = position.Y;
            hover_aabb.bottom_right.X = position.X + size.X;
            hover_aabb.bottom_right.Y = position.Y + size.Y;

            locked_size = false;
            if (minimum_size == maximum_size) {
                locked_size = true;
            }

            if (EngineState.graphics_device != null) {
                if (DigitalControlBindings.bind_pressed("ui_alt")) {
                    //hide_all_subforms = true;
                    //sub_forms[0].hidden = false;
                } else if (DigitalControlBindings.bind_released("ui_alt")) {
                    // = false;
                    //sub_forms[0].hidden = true;
                }

                sub_forms["close_button"].position.X = size.X - sub_forms["close_button"].size.X;

                foreach (UIForm f in sub_forms.Values) {
                    if (((!f.hidden && !hide_all_subforms) || (f.name == "close_button" && hide_all_subforms && !hide_close_button)) && DigitalControlBindings.bind_released("ui_alt")) continue;
                    f.update();
                    if ((f.name == "close_button" && hide_close_button)) {
                        f.hidden = true;
                    }
                }
            }
        }

        public bool draw_external_title = false;
        Vector2 title_offset = (Vector2.One * 2f) + (Vector2.UnitX * -15f);
        XYPair text_margin = new XYPair(4, 2);

        public override void draw() {

            if ((DigitalControlBindings.bind_released("ui_select") || DigitalControlBindings.bind_released("ui_context_menu")) && DigitalControlBindings.bind_pressed("ui_alt"))
                Draw2D.fill_square(position, size, Color.FromNonPremultiplied(240, 180, 195, 160));
            else
                Draw2D.fill_square(position, size, Color.Pink);


            //Draw2D.line(position + (Vector2.UnitY * top_handle_height), position + (Vector2.UnitY * top_handle_height) + (Vector2.UnitX * size.X), 1f, Color.Red);

            //we do this here instead of in update to ensure that it comes much later, to allow for inheritance to work
            if (form_rt != null)
                Draw2D.image(gbuffer.rt_diffuse, position.X, position.Y, size.X, size.Y, Color.White);


            if (draw_external_title) {
                var tl = position - title_offset - (text_margin);
                var size = get_txt_size(title_text) + text_margin + (XYPair.UnitX * 7);

                Draw2D.square(position, position + this.size, 1f, selected ? Color.Red : Color.Black);

                if (title_text != "") {
                    if (selected) {
                        Draw2D.fill_square(tl, size, Color.HotPink);
                    } else {
                        Draw2D.fill_square(tl, size, Color.LightPink);
                    }

                    Draw2D.square(tl, tl + size, 1f, Color.Red);
                    Draw2D.text_shadow("pf", title_text, tl + (XYPair.UnitX * 5), Vector2.One, Color.White, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None);
                }
            } else {
                if (title_text != "") {
                    if (selected) {
                        //Draw2D.image("gradient_vertical", position, (XYPair.UnitX * (size.X - 1)) + (XYPair.UnitY * 21), Color.DeepPink * 0.75f, SpriteEffects.FlipVertically);
                    } else {
                        //Draw2D.image("gradient_vertical", position, (XYPair.UnitX * (size.X - 1)) + (XYPair.UnitY * 21), Color.Pink, SpriteEffects.FlipVertically);
                    }
                }

                Draw2D.text_shadow("pf", title_text, position.ToVector2() + (Vector2.One * 2f) + (Vector2.UnitX * 4f), Vector2.One, Color.White, Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None);
                Draw2D.square(position, position + size, 1f, selected ? Color.Red : Color.Black);
            }

           // skip_title_draw:

            //if (DigitalControlBindings.bind_pressed("ui_alt"))
                //Draw2D.cross(center_pos, size.X, size.Y, Color.DarkSlateGray * 0.75f);
        }

        public void setup(int X, int Y, int W, int H) {
            this.position = new XYPair(X, Y);
            this.size = new XYPair(W, H);

            hover_aabb = new AABB2D(position, position + size);
            //top_handle = new AABB2D(position, position + (Vector2.UnitX * W) + (Vector2.UnitY * top_handle_height));

            collisions["aabb"] = hover_aabb;

            //keep close button far right (punch a nazi)
            sub_forms["close_button"].position = (XYPair.UnitX * (W - 16));

        }

        public void set_sub_button_click_delegate(string sub_form_name, ui_delegate click_delegate) {
            if (get_sub_form(sub_form_name).type == form_type.BUTTON)
                get_sub_form(sub_form_name).collisions["button"].click = click_delegate;
        }

        public override void add_sub_form(string name, UIForm form) {
            if (form.type != form_type.WINDOW)
                sub_forms.Add(name, form);
        }

        public override UIForm get_sub_form(string name) {
            if (sub_forms.ContainsKey(name))
                return sub_forms[name];
            else
                return null;
        }

        public UIWindow(int X, int Y, int W, int H, string name) : base(X, Y, -1) {
            // this.position = new XYPair(X, Y);
            //this.size = new XYPair(W, H);
            this.name = name;
            this.title_text = name;

            this.collisions.Add("aabb", new AABB2D(new Vector2(X, Y), Vector2.One * (position + (XYPair.UnitX * W))));


            sub_forms.Add("close_button", new UIButton((XYPair.UnitX * (W - 16)), XYPair.One * 16, "close_button", "X", true));
            sub_forms["close_button"].collisions["button"].click = hide;

            setup(X, Y, W, H);
            //this.collisions.Add("bottom_right_handle", hover_aabb);
        }
    }
}
