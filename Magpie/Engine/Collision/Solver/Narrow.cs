using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using static Magpie.Engine.Collision.Collision2D;

namespace Magpie.Engine.Collision.Solver {


    internal class NarrowPhaseSolver {
        public volatile ConcurrentQueue<narrow_queue_obj> queue = new ConcurrentQueue<narrow_queue_obj>();

        Thread solver_thread = null;

        BroadPhaseSolver parent_solver;

        public NarrowPhaseSolver(ref BroadPhaseSolver parent_solver) {
            this.parent_solver = parent_solver;

            this.solver_thread = new Thread(thread_loop);
            this.solver_thread.Start();
        }
        public bool working = false;
        void thread_loop() {

            while (EngineState.running) {

                if (queue.Count > 0) {
                    working = true;
                    narrow_queue_obj nqo;
                    if (!queue.TryDequeue(out nqo)) continue;
                    if (nqo.A == 0) continue;

                    World.internal_frame_probe.set("solving nqo " + nqo.A.ToString());

                    Shape3D shape_a = EngineState.world.current_map.game_objects[nqo.A].collision.movebox;

                    if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan()) {
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    }

                    float shortest_B = float.MaxValue;
                    collision_result shortest_hit = new collision_result();
                    int shortest_id = -1;

                    iterate:
                    bool hit = false;
                    bool halving_required = false;

                    foreach (int target in nqo.B) {
                        bool tmphalv=false;
                        Shape3D shape_b = EngineState.world.current_map.game_objects[target].collision.movebox;
                        collision_result 
                            result = GJK.gjk_intersects(
                                   shape_a, shape_b,
                                   EngineState.world.current_map.game_objects[nqo.A].world,
                                   EngineState.world.current_map.game_objects[target].world,
                                   EngineState.world.current_map.game_objects[nqo.A].wants_movement,
                                   EngineState.world.current_map.game_objects[target].wants_movement);

                            //tmphalv = true;
                        

                        if (result.intersects) {
                            hit = true;

                            var dist = (result.closest_B - EngineState.world.current_map.game_objects[target].position).Length();
                            if (dist < shortest_B) {
                                shortest_B = dist;
                                shortest_hit = result;
                                shortest_id = target;
                                halving_required = tmphalv;
                            }

                            if (Vector3.Dot(EngineState.world.current_map.game_objects[nqo.A].wants_movement, result.penetration_scalar) < Math3D.epsilon) {
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement += result.penetration_scalar;
                            } else {
                                result = GJK.swept_gjk_intersects_with_halving(
                                   shape_a, shape_b,
                                   EngineState.world.current_map.game_objects[nqo.A].world,
                                   EngineState.world.current_map.game_objects[target].world,
                                   EngineState.world.current_map.game_objects[nqo.A].wants_movement,
                                   EngineState.world.current_map.game_objects[target].wants_movement);

                                EngineState.world.current_map.game_objects[nqo.A].wants_movement = result.sweep_end + result.penetration_scalar + result.sweep_slide;
                            }
                            //EngineState.world.current_map.game_objects[nqo.A].wants_movement -= result.penetration_scalar;
                            //EngineState.world.current_map.game_objects[nqo.A].wants_movement = result.sweep_end;

                            EngineState.world.current_map.game_objects[nqo.A].collision.contact_points.Add(new contact_point() {
                                id = target,
                                contact = result.closest_A + result.penetration_scalar,
                                normal = result.penetration_normal
                            });

                            if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan() ||
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement.Length() < Math3D.epsilon) {
                                EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                                break;
                            }
                        }
                    }
                    
                    EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations++;

                    if (hit && EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations < 2)
                        goto iterate;
                    
                    //if (hit) {

                    EngineState.world.current_map.game_objects[nqo.A].position += EngineState.world.current_map.game_objects[nqo.A].wants_movement;
                    EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    //}


                    /*
                    if (shortest_id >= 0 && halving_required) {
                        EngineState.world.current_map.game_objects[nqo.A].position +=
                            shortest_hit.sweep_end + ((shortest_hit.penetration) * shortest_hit.penetration_normal);
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement =
                            shortest_hit.sweep_slide; //+ (shortest_hit.penetration * shortest_hit.penetration_normal);

                        if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan() ||
                            EngineState.world.current_map.game_objects[nqo.A].wants_movement.Length() < Math3D.epsilon) {
                            EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                        }

                        EngineState.world.current_map.game_objects[nqo.A].post_solve();

                        if (EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations < 3) {
                            parent_solver.working = true;
                            parent_solver.queue.Enqueue(nqo.A);
                            parent_solver.working = true;
                        } else {
                            EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                            parent_solver.queue.Enqueue(nqo.A);
                        }
                    } else if (shortest_id > 0) {
                        EngineState.world.current_map.game_objects[nqo.A].position += EngineState.world.current_map.game_objects[nqo.A].wants_movement + ((shortest_hit.penetration + Math3D.big_epsilon) * shortest_hit.penetration_normal);
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                        
                    
                    } else {
                        EngineState.world.current_map.game_objects[nqo.A].position += EngineState.world.current_map.game_objects[nqo.A].wants_movement;
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    }
                    */
                    /*

                    if (shortest_id >= 0) {
                        EngineState.world.current_map.game_objects[nqo.A].position +=
                            shortest_hit.sweep_end;
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement =
                            shortest_hit.sweep_slide; //+ (shortest_hit.penetration * shortest_hit.penetration_normal);


                        if (EngineState.world.current_map.game_objects[nqo.A].collision.solve.solver_iterations < 8)
                            parent_solver.queue.Enqueue(nqo.A);
                        else {
                            EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                        }
                    }  else {


                        EngineState.world.current_map.game_objects[nqo.A].position += EngineState.world.current_map.game_objects[nqo.A].wants_movement;
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;

                    }

                    if (EngineState.world.current_map.game_objects[nqo.A].wants_movement.contains_nan() ||
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement.Length() < Math3D.epsilon) {
                        EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;
                    }
                    */


                    EngineState.world.current_map.game_objects[nqo.A].post_solve();
                    EngineState.world.current_map.game_objects[nqo.A].wants_movement = Vector3.Zero;

                } else { working = false; }
            }
        }

    }
}
