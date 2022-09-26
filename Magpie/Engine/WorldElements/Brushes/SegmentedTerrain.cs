using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Engine.WorldElements.Brushes;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using static Magpie.Engine.Collision.Raycasting;
using static Magpie.Engine.Controls;


//the reason why all the segment_sizes have +1 on them is because there 
//needs to be an extra row and column of data on each segment to hold the 
//left and top sides of the segments to the right of them and below them
//including the data for the final right and bottom rows which would otherwise not exist
//this is tedious and has to be done basically everywhere outside the quads but it does work


namespace Magpie.Engine.Brushes {
    public class SegmentedTerrain : Brush {
        public BrushType type => BrushType.SEGMENTED_TERRAIN;        
        
        public TerrainSegment[,] segments;
        public List<(TerrainSegment, int, int, float)> visible_terrain = new List<(TerrainSegment, int, int, float)>();

        public XYPair size = XYPair.One * 128;
        public XYPair size_over_2 => size / 2;

        private XYPair _segment_size;
        public XYPair segment_size {
            get {
                return _segment_size;
            }
            set {
                if (size.X % value.X == 0 && size.Y % value.Y == 0) {
                    _segment_size = value;
                    this.segment_count = size / _segment_size;
                } else {
                    throw new Exception("input segment size must be divisions of overall heightfield width and height size");
                }
            }
        }

        public XYPair segment_count = XYPair.Zero;

        public (int, int, float) highest_segment;
        public (int, int, float) lowest_segment;

        public Vector3 position { get; set; } = Vector3.Zero;

        public Vector3 top_left => position - size_over_2.ToVector3XZ();
        public Vector3 top_right => position + new Vector3(size_over_2.X, 0, -size_over_2.Y);

        public Vector3 bottom_left => position + new Vector3(-size_over_2.X, 0, size_over_2.Y);
        public Vector3 bottom_right => position + size_over_2.ToVector3XZ();

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Matrix world => orientation * Matrix.CreateTranslation(position);
        
        public Shape3D collision { get; set; }

        public string texture { get; set; } = "monkeymelon";

        public Vector3 movement_vector { get; set; } = Vector3.Zero;
        public Vector3 final_position { get; set; }

        public float distance_to_camera => visible_terrain.Count > 0 ? visible_terrain[0].Item4 : float.MaxValue;

        public SceneRenderInfo render_info { get; set; }

        public BoundingBox bounds => new BoundingBox(top_left, bottom_right + (Vector3.Up * highest_segment.Item3));

        public SegmentedTerrain(Vector3 position, XYPair size, XYPair segment_size) {
            this.position = position;

            this.size = size;
            this.segment_size = segment_size;

            this.segment_count = size / segment_size;

            collision = new DummySupport();

            initialize_segments();
        }
        
        public SegmentedTerrain(Vector3 position, int size, int segments_per_axis) {
            this.position = position;
            this.size = XYPair.One * size;
            
            if (size % segments_per_axis != 0) {
                throw new Exception("input segment size must be divisions of overall heightfield width and height size");
            }

            this.segment_size = XYPair.One * (size / segments_per_axis);
            this.segment_count = new XYPair(segments_per_axis);

            collision = new DummySupport();

            initialize_segments();
        }

        private void initialize_segments() {
            segments = new TerrainSegment[segment_count.X, segment_count.Y];

            highest_segment = (-1,-1,float.MinValue);
            lowest_segment = (-1,-1,float.MaxValue);

            //iterate over each of the segments
            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    XYPair offset = new XYPair(sx * segment_size.X, sy * segment_size.Y);
                    var current_segment = new TerrainSegment(this, (sx,sy), offset, (offset.ToVector2() / size.ToVector2()));

                    current_segment.data = new float[segment_size.X + 1, segment_size.Y + 1];
                    current_segment.quads = new TerrainQuad[segment_size.X, segment_size.Y];

                    current_segment.fill_with_random();
                    //current_segment.build_from_texture("terrain_test");
                    current_segment.build_octrees();

                    current_segment.build_quads();

                    if (sx > 0) {
                        //make the +1 data of the segment to the left into the left side of the current segment's data
                        for (int i = 0; i < segment_size.Y + 1; i++) {
                            segments[sx - 1, sy].data[segment_size.X, i] = current_segment.data[0, i];
                        }
                    }
                    if (sy > 0) {
                        //same as above but making the top row into the above segment's bottom row
                        for (int i = 0; i < segment_size.X + 1; i++) {
                            segments[sx, sy - 1].data[i, segment_size.Y] = current_segment.data[i, 0];
                        }
                    }

                    segments[sx, sy] = current_segment;

                    if (current_segment.highest_point > highest_segment.Item3) {
                        highest_segment = (sx, sy, current_segment.highest_point);
                    }

                    if (current_segment.lowest_point < lowest_segment.Item3) {
                        lowest_segment = (sx, sy, current_segment.lowest_point);
                    }
                }
            }


            for (int sy = 1; sy < segment_count.Y-1; sy++) {
                for (int sx = 1; sx < segment_count.X-1; sx++) {
                    float r = 5f;

                    r = segments[sx, sy].data[0, 0] 
                        + segments[sx - 1, sy - 1].data[segment_size.X, segment_size.Y] 
                        + segments[sx, sy - 1].data[0, segment_size.Y] 
                        + segments[sx - 1, sy].data[segment_size.X, 0];
                    r /= 4;

                    segments[sx, sy].data[0, 0] = r;
                    segments[sx-1, sy-1].data[segment_size.X, segment_size.Y] = r;
                    segments[sx, sy - 1].data[0, segment_size.Y] = r;
                    segments[sx - 1, sy].data[segment_size.X, 0] = r;
                        
                    r = segments[sx, sy].data[segment_size.X, 0] 
                        + segments[sx, sy - 1].data[segment_size.X, segment_size.Y] 
                        + segments[sx + 1, sy - 1].data[0, segment_size.Y] 
                        + segments[sx + 1, sy].data[0, 0];
                    r /= 4;

                    segments[sx, sy].data[segment_size.X, 0] = r;
                    segments[sx, sy - 1].data[segment_size.X, segment_size.Y] = r;
                    segments[sx + 1, sy - 1].data[0, segment_size.Y] = r;
                    segments[sx + 1, sy].data[0, 0] = r;

                    r = segments[sx, sy].data[segment_size.X, segment_size.Y] 
                        + segments[sx + 1, sy].data[0, segment_size.Y] 
                        + segments[sx + 1, sy + 1].data[0, 0] 
                        + segments[sx, sy + 1].data[segment_size.X, 0];
                    r /= 4;

                    segments[sx, sy].data[segment_size.X, segment_size.Y] = r;
                    segments[sx + 1, sy].data[0, segment_size.Y] = r;
                    segments[sx + 1, sy + 1].data[0, 0] = r;
                    segments[sx, sy + 1].data[segment_size.X, 0] = r;

                    r = segments[sx, sy].data[0, segment_size.Y] 
                        + segments[sx - 1, sy].data[segment_size.X, segment_size.Y] 
                        + segments[sx - 1, sy + 1].data[segment_size.X, 0] 
                        + segments[sx, sy + 1].data[0, 0];
                    r /= 4;

                    segments[sx, sy].data[0, segment_size.Y] = r;
                    segments[sx - 1, sy].data[segment_size.X, segment_size.Y] = r;
                    segments[sx - 1, sy + 1].data[segment_size.X, 0] = r;
                    segments[sx, sy + 1].data[0, 0] = r;
                    
                }
            }

            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    //build_segment_buffers(ref segments[sx, sy]);
                    segments[sx, sy].build_buffers();
                }
            }
        }
        
        private void fill_all_segments_with_random(float max_height = 0.3f) {
            segments = new TerrainSegment[segment_count.X, segment_count.Y];
            //iterate over each of the segments
            for (int sy = 0; sy < segment_count.Y+1; sy++) {
                for (int sx = 0; sx < segment_count.X+1; sx++) {
                    segments[sx, sy].fill_with_random();                    
                }
            }
        }

        public void update_visible_terrain() {
            visible_terrain.Clear();

            //we do a little culling            
            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    var bb = segments[sx, sy].aabb;
                    if (bb.Intersects(EngineState.camera.frustum)) {
                        visible_terrain.Add((segments[sx, sy], sx, sy, Vector3.Distance(EngineState.camera.position, CollisionHelper.closest_point_on_AABB(EngineState.camera.position, bb.Min, bb.Max))));
                    }
                }
            }

            visible_terrain.Sort((a, b) => a.Item4.CompareTo(b.Item4));
        }

        public void Update() {
            
            /*
            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    var s = segments[sx, sy];
                    var bb = segments[sx, sy].aabb;
                    var cp = CollisionHelper.closest_point_on_AABB(EngineState.camera.position, bb.Min, bb.Max);
                    if (cp.X == EngineState.camera.position.X && cp.Z == EngineState.camera.position.Z) {
                        visible_terrain.Insert(0, (s, sx, sy, Vector3.Distance(EngineState.camera.position, cp)));
                        break;
                    }
                }
            }
            */
        }


        public Vector3 get_footing(float X, float Z) {
            throw new NotImplementedException();
        }
        public float get_footing_height(Vector3 pos) {
            throw new NotImplementedException();
        }
        public bool within_vertical_bounds(Vector3 pos) {
            throw new NotImplementedException();
        }

        (int, int) debug_mouseover;

        public bool raycast(Vector3 start, Vector3 end, out (int, int) quad_index, out (int,int) segment_index, out raycast_result result) {
            quad_index = (-1,-1);
            segment_index = (-1,-1);
            result = new raycast_result() {
                hit = false,
                distance = float.MaxValue,
                hit_normal = Vector3.Zero,
                point = Vector3.Zero
            };


            foreach ((TerrainSegment, int, int, float) xy in visible_terrain) {
                (int, int) current_index = (xy.Item2, xy.Item3);
                raycast_result current_result = new raycast_result();

                if (xy.Item1.raycast(start, end, out current_index, out current_result)) {
                    if (current_result.distance < result.distance) {
                        result = current_result;
                        quad_index = current_index;
                        segment_index = (xy.Item2, xy.Item3);
                    }
                }
            }

            if (quad_index != (-1, -1)) {
                return true;
            } else {
                return false;
            }
        }

        public raycast_result cursor_hit_result = new raycast_result();
        public (int, int) cursor_quad_index = (-1, -1);
        public (int, int) cursor_segment_index = (-1, -1);
        bool draw_hit_octrees = false;

        public void debug_draw() {
            RenderTargetBinding[] rtb = EngineState.graphics_device.GetRenderTargets();

            //EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);
            //Draw3D.draw_buffers(segments[0, 0].LOD_vertex_buffers[0], segments[0, 0].LOD_index_buffers[0], world, Color.White);

            /*
            Draw3D.xyz_cross(position, 1f, Color.Red);

            Draw3D.xyz_cross(top_left, 5f, Color.Red);
            Draw3D.xyz_cross(top_right, 5f, Color.Green);
            Draw3D.xyz_cross(bottom_right, 5f, Color.Blue);
            Draw3D.xyz_cross(bottom_left, 5f, Color.Yellow);
            
            Draw3D.lines(Color.White, top_left, top_right, bottom_right, bottom_left, top_left);

            foreach ((TerrainSegment, int, int, float) xy in visible_terrain) {
                var s = xy.Item1;
                Draw3D.sphere(s.top_left, 0.1f, Color.White);

                Vector4 col = new Vector4(1, 0, s.overall_UV_position.X, 1f);
                Draw3D.line(s.top_left, s.top_right, Color.FromNonPremultiplied(col));

                col = new Vector4(s.overall_UV_position.Y, 0, 1, 1f);

                Draw3D.line(s.top_left, s.bottom_left, Color.FromNonPremultiplied(col));

                col = Color.HotPink.ToVector4();
            }
            */


            if (StaticControlBinds.pressed("click_right")) {
                //do test raycast @ crosshair
                raycast(picker_raycasts.crosshair_ray.start, picker_raycasts.crosshair_ray.direction, out cursor_quad_index, out cursor_segment_index, out cursor_hit_result);

                //draw octree AABBs which collide with the same ray as above
                if (draw_hit_octrees)
                    foreach ((TerrainSegment, int, int, float) xy in visible_terrain) {
                        if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.crosshair_ray.start, picker_raycasts.crosshair_ray.direction, xy.Item1.aabb.Min, xy.Item1.aabb.Max, out _)) {
                            Draw3D.cube(xy.Item1.aabb, Color.HotPink);
                            for (int y = 0; y < 2; y++) {
                                for (int x = 0; x < 2; x++) {
                                    if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.crosshair_ray.start, picker_raycasts.crosshair_ray.direction, xy.Item1.octree[x, y].aabb.Min, xy.Item1.octree[x, y].aabb.Max, out _)) {
                                        Draw3D.cube(xy.Item1.octree[x, y].aabb, Color.Red);
                                        foreach (OctreeArea oa in xy.Item1.octree[x, y].octree) {
                                            if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.crosshair_ray.start, picker_raycasts.crosshair_ray.direction, oa.aabb.Min, oa.aabb.Max, out _)) {
                                                Draw3D.cube(oa.aabb, Color.ForestGreen);
                                                foreach (OctreeArea oa2 in oa.octree) {
                                                    if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.crosshair_ray.start, picker_raycasts.crosshair_ray.direction,
                                                        oa2.aabb.Min, oa2.aabb.Max, out _)) {
                                                        Draw3D.cube(oa2.aabb, Color.Blue);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
            
            } else if (mouse_in_bounds) {
                //do test raycast @ mouse cursor
                raycast(picker_raycasts.mouse_pick_ray.start, picker_raycasts.mouse_pick_ray.direction, out cursor_quad_index, out cursor_segment_index, out cursor_hit_result);

                //draw octree AABBs which collide with the same ray as above
                if (draw_hit_octrees)
                foreach ((TerrainSegment, int, int, float) xy in visible_terrain) {
                    if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.mouse_pick_ray.start, picker_raycasts.mouse_pick_ray.direction, xy.Item1.aabb.Min, xy.Item1.aabb.Max, out _)) {
                        Draw3D.cube(xy.Item1.aabb, Color.HotPink);
                        for (int y = 0; y < 2; y++) {
                            for (int x = 0; x < 2; x++) {
                                if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.mouse_pick_ray.start, picker_raycasts.mouse_pick_ray.direction, xy.Item1.octree[x, y].aabb.Min, xy.Item1.octree[x, y].aabb.Max, out _)) {
                                    Draw3D.cube(xy.Item1.octree[x, y].aabb, Color.Red);
                                    foreach (OctreeArea oa in xy.Item1.octree[x, y].octree) {
                                        if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.mouse_pick_ray.start, picker_raycasts.mouse_pick_ray.direction, oa.aabb.Min, oa.aabb.Max, out _)) {
                                            Draw3D.cube(oa.aabb, Color.ForestGreen);
                                            foreach (OctreeArea oa2 in oa.octree) {
                                                if (Raycasting.ray_intersects_BoundingBox(picker_raycasts.mouse_pick_ray.start, picker_raycasts.mouse_pick_ray.direction,
                                                    oa2.aabb.Min, oa2.aabb.Max, out _)) {
                                                    Draw3D.cube(oa2.aabb, Color.Blue);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //draw test raycast result
            if (cursor_hit_result.hit == true) {
                Draw3D.line(cursor_hit_result.point, cursor_hit_result.point + (Vector3.Up * 1f), Color.Red);
                Draw3D.circle(cursor_hit_result.point + (Vector3.Up * 0.5f), 3f, Vector3.Up, 16, Color.Red);

                var mo_quad = segments[cursor_segment_index.Item1, cursor_segment_index.Item2].quads[cursor_quad_index.Item1, cursor_quad_index.Item2];

                if (mo_quad.odd || mo_quad.even) {
                    Draw3D.triangle(mo_quad.A, mo_quad.B, mo_quad.D, Color.Red);
                    Draw3D.triangle(mo_quad.C, mo_quad.D, mo_quad.B, Color.Red);
                } else {
                    Draw3D.triangle(mo_quad.A, mo_quad.B, mo_quad.C, Color.Red);
                    Draw3D.triangle(mo_quad.A, mo_quad.C, mo_quad.D, Color.Red);
                }

            }
            
            //unjiggle render targets
            EngineState.graphics_device.SetRenderTargets(rtb);
        }
    }
}
