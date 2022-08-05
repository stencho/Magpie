using Magpie.Engine.Brushes;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.Stages;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine.Physics {
    interface ISolver {
        //order of events for solving physics
        //find the initial final positions for each of the objects via the solver inherited classes        
        //check if each object needs to be swept, create sweep hitshapes for ones that do

        //do time-related tests to check if swept objects are intersecting at the same time, though this is probably unnecessary, 
        //for the most part objects will not be moving fast enough that they will, say, form a big, this is mostly all just 
        //a faster way to make sure that objects don't enter each other at any point and less to prevent actual full fuck sake tunnelling 

        //use the solver classes again to do things like bounciness, weight tests, etc
    }

    public enum physics_type {
        STATIC,
        BASIC_PHYS,
        GRAVITY_ONLY,
        CUSTOM
    }
    public enum intersection_level {
        NONE,
        AABB,
        COLLIDING
    }

    public class PhysicsInfo {
        public float gravity;
        public Vector3 gravity_direction;
        public physics_type phys_type;
        public bool stick_to_ground;
        public Brush current_ground;
               
        public static PhysicsInfo default_static() {
            return new PhysicsInfo() {
                gravity = 9.81f,
                gravity_direction = Vector3.Down,

                phys_type = physics_type.STATIC,
                stick_to_ground = false

            };
        }
        public static PhysicsInfo default_phys() {
            return new PhysicsInfo() {
                gravity = 9.81f,
                gravity_direction = Vector3.Down,

                phys_type = physics_type.BASIC_PHYS,
                stick_to_ground = false
            };
        }
        public static PhysicsInfo default_grav_only() {
            return new PhysicsInfo() {
                gravity = 9.81f,
                gravity_direction = Vector3.Down,

                phys_type = physics_type.GRAVITY_ONLY,
                stick_to_ground = true
            };
        }

        public static void do_base_physics(PhysicsInfo info, out Vector3 result_position_delta) {
            result_position_delta = Vector3.Zero;

            switch (info.phys_type) {
                case physics_type.STATIC:
                    break;
                case physics_type.BASIC_PHYS:
                    break;
                case physics_type.GRAVITY_ONLY:
                    result_position_delta += info.gravity_direction * info.gravity * Clock.frame_time_delta;
                    break;
                case physics_type.CUSTOM:
                    break;
            }
        }
    }


    public class Intersection {
        public Shape3D A, B;
        public intersection_level level = intersection_level.NONE;
        public Vector3 position;
        public int id;

        public GJK.gjk_result gjkr;

        public object parent_a, parent_b;
        public string tag_a, tag_b;

        public Intersection(Shape3D A, Shape3D B, Vector3 pos, intersection_level level, object parent_a=null, object parent_b=null, string tag_a=null, string tag_b=null) {
            this.A = A;
            this.B = B;
            this.position = pos;
            this.level = level;

            this.tag_a = tag_a;
            this.tag_b = tag_b;

            this.parent_a = parent_a;
            this.parent_b = parent_b;

            id = 0;
        }        

        public override string ToString() {
            if (level == intersection_level.COLLIDING) {
                return string.Format(
                    "{0} => {1}\n  {2}\n\n{3}",
                    A.shape.ToString(), B.shape.ToString(), level.ToString(), gjkr.ToString());
            } else {
                return string.Format(
                    "{0} => {1}\n  {2}\n",
                    A.shape.ToString(), B.shape.ToString(), level.ToString());
            }
        }
    }

    public class PhysicsSolver {
        //find final movement for each of the map's elements
        //first brushes, then actors, then objects
        public static void do_movement(Map map) {
            foreach (Brush brush in map.brushes.Values) {
                //do brush movement
                if (!brush.movement_vector.contains_nan()) {
                    brush.final_position = brush.position + brush.movement_vector;
                }

            }

            //map.player_actor find movement goes here

            if (map.player_actor.wants_movement != Vector3.Zero) {
                map.player_actor.position += map.player_actor.wants_movement;

                map.player_actor.wants_movement = Vector3.Zero;
            }

            foreach (Actor actor in map.actors.Values) {
                //actor.final_position = 
            }

            foreach(GameObject gobject in map.objects.Values) {

            }

        }
        private static List<int> intersection_ids = new List<int>();
        public static List<Intersection> intersections = new List<Intersection>();
        public static string list_intersections() {
            string s = "";
            foreach (Intersection i in intersections) {
                s += i.ToString();
            }
            return s;
        }

        static bool abs = false;
        public static void do_base_physics_and_ground_interaction(Map map) {
            foreach (Actor actor in map.actors.Values) {
                intersections.Clear();
                intersection_ids.Clear();

                if (actor.collision.shape != shape_type.dummy) {
                    
                    var pos_delta = Vector3.Zero;
                    PhysicsInfo.do_base_physics(actor.phys_info, out pos_delta);
                    actor.wants_movement += pos_delta;

                    if (actor.request_absolute_move) {
                        abs = true;

                        if (!actor.sweep_absolute_move) {
                            actor.position = actor.wants_absolute_movement;
                            actor.collision.position = actor.wants_absolute_movement;
                            actor.sweep_collision = actor.collision;
                            actor.sweep_collision.position = actor.wants_absolute_movement;
                        } else {
                            switch (actor.collision.shape) {
                                case shape_type.capsule:
                                    actor.sweep_collision = new Quad(
                                        actor.collision.position + ((Capsule)actor.collision).A,
                                        actor.wants_absolute_movement + ((Capsule)actor.collision).A,
                                        actor.wants_absolute_movement + ((Capsule)actor.collision).B,
                                        actor.collision.position + ((Capsule)actor.collision).B
                                        );

                                    break;

                                default:break;
                            }


                            actor.sweep_collision.radius = actor.collision.radius;
                            actor.sweep_collision.position = Vector3.Zero;
                        }

                        actor.request_absolute_move = false;
                        actor.wants_absolute_movement = Vector3.Zero;

                    } else {
                        abs = false;
                    }

                    if (abs) goto skip_sweep;
                    switch (actor.collision.shape) {
                        case shape_type.cube:
                            break;
                        case shape_type.polyhedron:
                            //find most extreme points on shape in all 360 degrees, perpendicular to sweep direction, 
                            //then remove any duplicates and add in all the points from the original shape
                            //which are in the opposite direction of the sweep
                            //I think
                            //ultimately this probably won't need to be swept like ever and would be kind of slow
                            //but might be useful for, say, boss hitboxes
                            //if a big dude swings a big arm at you and it hits you but shouldn't have, that feels bad
                            //so actual large fitted hitshapes are probs preferred here
                            //regular hitshapes should be preferred for everything else tho
                            break;
                        case shape_type.quad:
                            //cube
                            break;
                        case shape_type.tri:
                            //will need to make a specific class for this
                            break;
                        case shape_type.capsule:
                            actor.sweep_collision = new Quad(
                                ((Capsule)actor.collision).A + actor.collision.position,
                                actor.collision.position + actor.wants_movement + ((Capsule)actor.collision).A,
                                actor.collision.position + actor.wants_movement + ((Capsule)actor.collision).B,
                                ((Capsule)actor.collision).B + actor.collision.position
                                );

                            break;
                        case shape_type.line:
                            //quad w/ no radius
                            break;
                        case shape_type.sphere:
                            //capsule
                            break;
                        case shape_type.dummy:
                            break;
                    }


                    actor.sweep_collision.radius = actor.collision.radius;
                    actor.sweep_collision.position = Vector3.Zero;

                    skip_sweep:

                    foreach (Brush brush in map.brushes.Values) {
                        if (brush.collision.shape != shape_type.dummy) {
                            var intersection = new Intersection(actor.sweep_collision, brush.collision, Vector3.Zero, intersection_level.NONE, actor, brush, "actor", "brush");

                            if (actor.sweep_collision.find_bounding_box().Contains(brush.collision.find_bounding_box()) != ContainmentType.Disjoint) {
                                intersection.level = intersection_level.AABB;

                                int id = 0;
                                id = RNG.rng_int();
                                while (intersection_ids.Contains(id)) {
                                    id = RNG.rng_int();
                                }
                                intersection.id = id;

                                intersection.gjkr = GJK.gjk_intersects(actor.sweep_collision, brush.collision, Matrix.Identity, brush.world, actor.sweep_collision.radius, brush.collision.radius);
                                
                                if (intersection.gjkr.hit) {
                                    intersection.level = intersection_level.COLLIDING;

                                    if (actor.phys_info.stick_to_ground) {
                                        actor.phys_info.current_ground = brush;
                                    }

                                }


                            }

                            intersections.Add(intersection);
                        }
                    }


                }





            }

            foreach (GameObject gobject in map.objects.Values) {

            }

        }



        public static void finalize_collisions(Map map) {

            foreach (Actor actor in map.actors.Values) {

                if (actor.wants_movement != Vector3.Zero) {
                    actor.position += actor.wants_movement;
                    actor.collision.position += actor.wants_movement;

                    actor.wants_movement = Vector3.Zero;
                }
            }
        }
    }
}
