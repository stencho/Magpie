using Magpie.Engine.Physics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Collision {
    public struct Collision {
        Shape3D shape;
        Intersection intersections;
        Matrix world;
        //some way to link bone model goes here
    }

    public class CollisionList {

        Dictionary<string, Collision> collisions = new Dictionary<string, Collision>();

        public void add() {

        }
        public void remove() {

        }



        public void update() {
        }
        public void draw() {

        }
    }
}
