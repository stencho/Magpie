using Magpie.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics.UI.ContextMenu {

    public interface IContextMenuItem {
        XYPair position { get; }
        XYPair size { get; }

        void test_mouse();
        void update();
        void draw();
    }

    public class CMButton : IContextMenuItem {
        public XYPair position => _position;
        XYPair _position = XYPair.Zero;
        public XYPair size => _size;
        XYPair _size = XYPair.One;

        public CMButton() {

        }

        public void test_mouse() {

        }

        public void update() {

        }

        public void draw() {

        }
    }

    public class CMSlider : IContextMenuItem {
        public XYPair position => _position;
        XYPair _position = XYPair.Zero;
        public XYPair size => _size;
        XYPair _size = XYPair.One;

        public CMSlider() {

        }

        public void test_mouse() {

        }

        public void update() {

        }

        public void draw() {

        }
    }

    public class ContextMenu {
        List<IContextMenuItem> items = new List<IContextMenuItem>();

        public ContextMenu(params IContextMenuItem[] items) {

        }

        public void show() {

        }
        public void close() {

        }


        public void test_mouse() {

        }

        public void update() {

        }

        public void draw() {            
            foreach (IContextMenuItem context_menu_item in items) {
                context_menu_item.draw();
            }
        }
    }

}
