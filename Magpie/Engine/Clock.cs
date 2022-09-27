using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Engine {
    public class frame_probe {
        volatile List<(string name, probe probe)> probes = new List<(string name, probe probe)>();

        volatile bool ignore_this_frame = false;

        public volatile string last_probe_name = "";
        public class probe {
            public DateTime dt = DateTime.Now;
            public void setup() {
                dt = DateTime.Now;
            }

            public double since => (DateTime.Now - dt).TotalMilliseconds;
            public float sinceF => (float)(DateTime.Now - dt).TotalMilliseconds;

            public bool set_this_frame = false;
            public double frame_pos = 0;
        }

        public int probe_index(string name) {
            lock(probes)
                return probes.FindIndex(a => a.name == name);
        }
        public bool probe_exists(string name) {
            lock (probes)
                return (probe_index(name) > -1);
        }

        int ins = 0;
        public void false_set(string name) {
            lock (probes) {
                if (probe_exists(name)) {
                    probes[probe_index(name)].probe.set_this_frame = true;

                    last_probe_name = name;
                }
            }
        }

        public void set(string name) {
            lock (probes) {
                if (ignore_this_frame) return;

                if (probe_exists(name)) {
                    probes[probe_index(name)].probe.setup();
                } else {
                    probes.Add((name, new probe()));
                }
                probes[probe_index(name)].probe.frame_pos = (probes[probe_index(name)].probe.dt - frame_dt).TotalMilliseconds;
                probes[probe_index(name)].probe.set_this_frame = true;

                last_probe_name = name;

                ins++;
            }
        }
        public double since(string name) {
            lock (probes) {
                if (probe_exists(name)) {
                    return probes[probe_index(name)].probe.since;
                } else {
                    return 1;
                }
            }
        }
        public float sinceF(string name) {
            lock (probes) {
                if (probe_exists(name)) {
                    return probes[probe_index(name)].probe.sinceF;
                } else {
                    return 1;
                }
            }
        }
        
        public double since_frame_start() {
            return (DateTime.Now - frame_start_dt).TotalMilliseconds;
        }
        public double since_frame_startF() {
            return (float)(DateTime.Now - frame_start_dt).TotalMilliseconds;
        }

        public DateTime frame_start_dt = DateTime.Now;
        public DateTime frame_dt = DateTime.Now;
        public double frame_time = 0;
        public double since_start_of_frame => frame_time > -1 ? frame_time : (DateTime.Now - frame_dt).TotalMilliseconds;

        public void start_of_frame() {
            ins = 0;
            frame_start_dt = DateTime.Now;
            //ignore_this_frame = (Clock.frame_count % profile_every_n_frames != 0);
                        
            if (ignore_this_frame) return;
            lock (probes) {
                foreach ((string name, probe probe) v in probes) {
                    v.probe.set_this_frame = false;
                }
            }
        }


        public void end_of_frame() {
            frame_time = (DateTime.Now - frame_dt).TotalMilliseconds;
            frame_dt = DateTime.Now;
            if (ignore_this_frame) return;

            lock (probes)
                probes.RemoveAll(a => !a.probe.set_this_frame);
        }
        public void end_of_frame(double time) {
            frame_time = time;
            frame_dt = DateTime.Now;
            if (ignore_this_frame) return;
            lock (probes)
                probes.RemoveAll(a => !a.probe.set_this_frame);
        }


        public void draw(int x, int y, int graph_width, out int total_width, out int total_height) {
            total_width = 0;
            total_height = 0;
            if (probes.Count <= 0) return;

            int xx = 0;
            int yy = 0;
            int y_tot = 0;
            int count = 0;

            int border = 4;

            var ms1 = Math2D.measure_string("pf", "a");

            lock (probes) {
                foreach ((string name, probe) v in probes) {
                    var ms = Math2D.measure_string("pf", v.name);
                    if (graph_width + ms.X + (ms1.X * 3) > xx)
                        xx = graph_width + ms.X + (ms1.X * 3);
                    y_tot += ms.Y + 2;
                }
            }

            y_tot += ms1.Y + 2;

            Draw2D.fill_square(x - border, y - border, xx + (border * 2), y_tot + (border * 2), Color.FromNonPremultiplied(0, 0, 0, 128));

            total_width = xx + (border * 2);
            total_height = y_tot + (border * 2);

            Draw2D.line(x + (ms1.X * 2), y + y_tot - (ms1.Y), x + graph_width, y + y_tot - (ms1.Y), 1, Color.DeepPink);

            Draw2D.fill_square(x + (ms1.X * 2)-2, y + y_tot - (int)(ms1.Y) - 1, 2, (int)(ms1.Y) + 2, Color.White);
            Draw2D.fill_square(x + graph_width - 1, y + y_tot - (int)(ms1.Y) - 1, 2, (int)(ms1.Y) + 2, Color.White);

            Draw2D.text_shadow("pf", "0", new Vector2(x, y + y_tot - (ms1.Y)), Color.White);
            Draw2D.text_shadow("pf", string.Format("{0:F2}", since_start_of_frame), new Vector2(x + ms1.X + graph_width, y + y_tot - (ms1.Y)), Color.White);

            int last_x = x + (ms1.X * 2);

            Color c = Color.Transparent;
            Color last_c = Color.Transparent;


            lock (probes) {
                foreach ((string name, probe probe) v in probes) {
                    c = Draw2D.ColorRandomFromString(v.name);
                    var ms = Math2D.measure_string("pf", v.name);

                    Draw2D.fill_circle(
                        new Vector2(graph_width + x + (ms1.X * 0.5f) + (ms1.X * 1), y + (ms1.Y * 0.5f) + yy + 1),
                        ms1.X / 2, c);

                    Draw2D.text_shadow("pf", v.name,
                        new Vector2(graph_width + x + (ms1.X * 3), y + yy + 1),
                        Color.White);

                    var f = (v.probe.frame_pos / since_start_of_frame);

                    if (f > 1) f = 1;
                    if (f < 0) f = 0;

                    var current_x = (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f));
                    if (count == probes.Count - 1) {
                        Draw2D.fill_square(current_x, y + y_tot - (ms1.Y), (graph_width - (ms1.X * 2)) - (int)((graph_width - (ms1.X * 2)) * f), ms1.Y, c);
                    }

                    Draw2D.fill_square(last_x, y + y_tot - (ms1.Y), current_x - last_x, ms1.Y, last_c);

                    if (f >= 0 && f <= 1) {

                        //top
                        Draw2D.line(
                            (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f)),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            (int)(x + (ms1.X * 2) + (graph_width - (ms1.X * 2))),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            1, c);

                        Draw2D.line(
                            (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f)),
                            (int)(y + (ms1.Y * 0.5f) + yy + 1),
                            (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f)),
                            (int)(y + (ms1.Y * 0.5f) + y_tot - border - 2),
                            1, c);

                        Draw2D.line(
                            (int)(x + (ms1.X * 2) + (graph_width - (ms1.X * 2))),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            (int)(x + (ms1.X * 2) + (graph_width - (ms1.X * 2)) + ms1.X),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            1, c);
                    } else {
                        Draw2D.line(
                            (int)(x + (ms1.X * 2) + (graph_width - (ms1.X * 2))),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            (int)(x + (ms1.X * 2) + (graph_width - (ms1.X * 2)) + ms1.X),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            1, c);


                        Draw2D.line(
                            (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f)),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + yy + 1),
                            (int)(x + (ms1.X * 2) + ((graph_width - (ms1.X * 2)) * f)),
                            (int)Math.Round(y + (ms1.Y * 0.5f) + y_tot - border - 1),
                            1, c);
                    }

                    last_c = c;
                    last_x = current_x;
                    yy += ms1.Y + 2;
                    count++;
                }
            }            
        }
    }

    public static class Clock {
        private static GameTime _gt = new GameTime();
        public static GameTime game_time => _gt;

        public static int frame_rate { get; set; } = 0;
        public static int frame_rate_immediate { get; set; } = 0;

        private static double _frame_rate_timer = 0;
        private static double _frame_rate_timer_i = 0;
                
        public static double total_ms { get; private set; } = 0;
        public static double total_ms_ignore_pause { get; private set; } = 0;

        public static float internal_frame_time_delta_ms = 0;
        public static float internal_frame_time_delta = 0;

        public static bool paused { get; set; } = false;

        public static long frame_count { get; private set; } = 0;

        public static int frame_limit = 144;
        public static int internal_frame_limit = 60;

        public static double frame_limit_ms => 1000.0 / Clock.frame_limit;
        public static double internal_frame_limit_ms => 1000.0 / Clock.internal_frame_limit;

        public static int internal_frame_rate_immediate;

        public static volatile frame_probe frame_probe = new frame_probe();

        //public static float minimum_tolerated_fps = (frame_limiter_fps > 0 ? frame_limiter_fps / 3 : 20);

        public static TimeSpan frame_time_delta_ts => TimeSpan.FromMilliseconds(_gt.ElapsedGameTime.TotalMilliseconds);
                        
        //(float)(_gt.ElapsedGameTime.TotalMilliseconds <= 1000f / //minimum_tolerated_fps 
        //: 1000f / minimum_tolerated_fps / 1000f);

        public static float frame_time_delta => paused ? 0 :
                (float)_gt.ElapsedGameTime.TotalSeconds;
        // (float)(_gtr.ElapsedGameTime.TotalMilliseconds <= //1000f / minimum_tolerated_fps
        // : 1000f / minimum_tolerated_fps / 1000f);

        public static float frame_time_delta_ms => paused ? 0 :
                     (float)_gt.ElapsedGameTime.TotalMilliseconds;
        //(float)(_gt.ElapsedGameTime.TotalMilliseconds <= 1000f / minimum_tolerated_fps 
        //  : 1000f / minimum_tolerated_fps);


        public static double d_frame_time_delta => paused ? 0 :
                _gt.ElapsedGameTime.TotalSeconds;
        public static double d_frame_time_delta_ms => paused ? 0 :
                     _gt.ElapsedGameTime.TotalMilliseconds;
        public static void set_gametime(GameTime gt) {
            _gt = gt;
        }


        public static void update(GameTime gt, Game game) {            
            set_gametime(gt);            

            if (frame_limit > 0) {
                game.IsFixedTimeStep = true;
                game.TargetElapsedTime = TimeSpan.FromMilliseconds(frame_limit_ms);
            } else {
                game.IsFixedTimeStep = false;
                game.TargetElapsedTime = TimeSpan.FromMilliseconds(1000.0 / 16.0);
            }

            total_ms_ignore_pause += _gt.ElapsedGameTime.TotalMilliseconds;

            if (paused ){
                return;
            }
            
            total_ms += _gt.ElapsedGameTime.TotalMilliseconds;
        }

        public static int FPS_buffer_length = 60;
        public static int delta_buffer_length = 60 * 5;

        public static double[] FPS_immediate_buffer = new double[FPS_buffer_length];
        public static double[] FPS_immediate_buffer_internal = new double[FPS_buffer_length];

        public static double[] delta_buffer = new double[delta_buffer_length];
        public static double delta_buffer_average = 0;

        public static void update_fps() {           
            _frame_rate_timer += _gt.ElapsedGameTime.TotalMilliseconds;
            _frame_rate_timer_i++;
            

            for (int i = 0; i < delta_buffer_length-1;i++) {
                delta_buffer[i] = delta_buffer[i + 1];
                delta_buffer_average += delta_buffer[i];
            }
            delta_buffer[delta_buffer_length - 1] = Clock.d_frame_time_delta_ms;
            delta_buffer_average += delta_buffer[delta_buffer_length - 1];
            delta_buffer_average /= delta_buffer_length;

            frame_rate_immediate = (int)(FPS_immediate_buffer.Aggregate((a, b) => a + b) / (double)FPS_buffer_length);
            internal_frame_rate_immediate = (int)(Math.Round(World.last_fps[World.last_fps.Length - 1]));


            if (_frame_rate_timer >= 200.0) {
                frame_rate = (int)(_frame_rate_timer_i );
                
                _frame_rate_timer -= 1000.0;
                _frame_rate_timer_i = 0;
                for (int i = 0; i < FPS_buffer_length - 1; i++) {
                    FPS_immediate_buffer[i] = FPS_immediate_buffer[i + 1];
                }

                for (int i = 0; i < FPS_buffer_length - 1; i++) {
                    FPS_immediate_buffer_internal[i] = FPS_immediate_buffer_internal[i + 1];
                }

                FPS_immediate_buffer[FPS_buffer_length - 1] = frame_rate;// (1000 / Clock.frame_time_delta_ms);
                FPS_immediate_buffer_internal[FPS_buffer_length - 1] = frame_rate;// (1000 / Clock.frame_time_delta_ms);

            }

            frame_count++;

        }
    }
}
