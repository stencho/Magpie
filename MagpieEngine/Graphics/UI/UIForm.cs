using Magpie.Engine;
using Magpie.Graphics;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Graphics.UI {
    public class UIForm {
        public XYPair position;
        public XYPair size;

        //some sort of collision here
        public float z;
        public string name;

        public bool mouse_over { get; set; } = false;
        public bool hidden { get; set; } = false;

        public virtual form_type type => form_type.NONE;

        public AABB2D hover_aabb;
        public virtual bool mouse_hover { get; set; }
        public bool mouse_down_prev = false;
        public bool mouse_down = false;

        public Dictionary<string, AABB2D> collisions = new Dictionary<string, AABB2D>();

        public virtual object value { get; set; }
        public string click_hit = "";
        public string hover_hit = "";
        public string right_click_hit = "";

        public UIForm(int X, int Y) {
            position = new XYPair(X, Y);
        }
        public UIForm(XYPair position) {
            this.position = position;
        }
        public UIForm(XYPair position, XYPair size) {
            this.position = position;
            this.size = size;
        }

        public UIForm(int X, int Y, float Z) {
            position = new XYPair(X, Y);
            this.z = Z;
        }

        public UIForm(XYPair position, float Z) {
            this.position = position;
            this.z = Z;
        }

        public virtual void update() { }
        public virtual void internal_stateless_draw(GraphicsDevice gd, SpriteBatch sb) { }
        public virtual void draw() { }

        public virtual bool mouse_hit(XYPair mouse, XYPair mouse_local) { return false; }
        public virtual void mouse_update(XYPair mouse, XYPair mouse_local, bool check_mouse) {
            mouse_hover = false;
            if ((mouse_local.X < size.X && mouse_local.Y < size.Y && mouse_local.X > 0 && mouse_local.Y > 0)) {
                mouse_hover = true;
            }
        }

        public virtual void add_sub_form(string name, UIForm form) { }

        public virtual UIForm get_sub_form(string name) { return null; }

        public void hide() => hidden = true;
        public void show() => hidden = false;

    }
}
