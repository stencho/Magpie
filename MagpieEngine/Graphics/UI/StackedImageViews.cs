using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Graphics.UI {
    public class StackedImageViews {
        static int max_viewports = 8;

        Point2D mouse_collision;

        public enum stack_method {
            HORIZONTAL,
            VERTICAL,
            SPIRAL,
            FREE
        }

        int enabled_viewports_count => viewport_toggles.Count((a) => a == true);
        int active_viewport = 0;

        bool[] viewport_toggles = new bool[max_viewports];

        AABB2D[] viewport_collisions;
        ImageViewer[] viewports;

        public RenderTarget2D output_rt;

        Vector2 size = Vector2.One * 800;
        Vector2 top_left = Vector2.Zero;

        Vector2 top_right => top_left + (Vector2.UnitX * size.X);
        Vector2 bottom_left => top_left + (Vector2.UnitY * size.Y);
        Vector2 bottom_right => top_left + (Vector2.UnitY * size.Y) + (Vector2.UnitX * size.X);
        
        public StackedImageViews(Vector2 screen_position, Vector2 form_size) {
            top_left = screen_position;
            size = form_size;

            output_rt = new RenderTarget2D(EngineState.graphics_device, EngineState.resolution.X, EngineState.resolution.Y);

            viewport_collisions = new AABB2D[max_viewports];
            viewports = new ImageViewer[max_viewports];

            for (int id = 0; id < max_viewports; id++) {
                viewport_toggles[id] = false;
                viewports[id] = new ImageViewer(0,0,size.X, size.Y);
                viewport_collisions[id] = new AABB2D(0, 0, size.X, size.Y);

                viewports[id].change_position(screen_position.X, screen_position.Y);
                viewports[id].change_size(size.X, size.Y);
            }

            enable_viewport(0);
            configure_viewport(0, 0, 0, size.X,size.Y);
            enable_viewport(1);
            configure_viewport(1, 200, 200, 200, 200);
        }

        public void enable_viewport(int id) {
            viewport_toggles[id] = true;
        }
        public void disable_viewport(int id) {
            viewport_toggles[id] = false;
        }
        public void toggle_viewport(int id) {
            viewport_toggles[id] = !viewport_toggles[id];
        }

        public void set_viewport_image(int id, ref Texture2D image) {
            viewports[id].image = image;
        }

        public void move_viewport(int id, float x, float y) {
            viewports[id].change_position(x, y);
        }
        public void resize_viewport(int id, float w, float h) {
            viewports[id].change_size(w, h);
        }
        public void configure_viewport(int id, float x, float y, float w, float h) {            
            move_viewport(id, x,y); resize_viewport(id, w, h);
            viewport_collisions[id] = new AABB2D(x, y, w, h);

        }

        public void update() {
            for (int id = 0; id < max_viewports; id++) {
                viewports[id].active = (id == active_viewport);
                if (viewport_toggles[id]) {

                    viewports[id].update();
                }
            }
        }

        public void draw() {
            for (int id = 0; id < max_viewports; id++) {
                if (viewport_toggles[id]) {
                    var viewport = viewports[id];

                    viewport.draw();
                    /*
                    EngineState.spritebatch.Begin(SpriteSortMode.Immediate);
                    Draw2D.text_shadow("pf", Vector2.One * 5f + viewport.viewport_position, Color.White,
@"[{0}]",
                        id.ToString());
                    EngineState.spritebatch.End();*/
                }
            }
        }
    }
}
