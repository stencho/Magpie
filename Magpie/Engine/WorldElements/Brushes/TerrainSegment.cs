using Magpie.Engine.Brushes;
using Magpie.Engine.Collision;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Magpie.Engine.Collision.Raycasting;
using static Magpie.Engine.Controls;

namespace Magpie.Engine.WorldElements.Brushes {
    public class TerrainQuad {
        TerrainSegment parent;
        int x, y;

        public int X => x;
        public int Y => y;
        public (int, int) XY => (x, y);

        public bool x_even => x % 2 == 0; public bool x_odd => x % 2 == 1;
        public bool y_even => y % 2 == 0; public bool y_odd => y % 2 == 1;

        public bool even => x_even && y_even; public bool odd => x_odd && y_odd;

        public Vector3 A => parent.data_v3_pos(x, y);
        public Vector3 B => parent.data_v3_pos(x + 1, y);
        public Vector3 C => parent.data_v3_pos(x + 1, y + 1);
        public Vector3 D => parent.data_v3_pos(x, y + 1);

        public Vector3 center_on_line => get_actual_center();
        public Vector3 center_average => (A + B + C + D) / 4f;

        public float height_A => parent.data[x, y];
        public float height_B => parent.data[x + 1, y];
        public float height_C => parent.data[x + 1, y + 1];
        public float height_D => parent.data[x, y + 1];

        public BoundingBox aabb => CollisionHelper.BoundingBox_around_points(A, B, C, D);

        public Vector3 get_actual_center() {
            if (even || odd) {
                //B -> D
                var BD = D - B;
                float len = BD.Length();
                return B + (Vector3.Normalize(BD) * (len * 0.5f));
            } else {
                //A -> C
                var AC = C - A;
                float len = AC.Length();
                return A + (Vector3.Normalize(AC) * (len * 0.5f));
            }
        }

        public TerrainQuad(TerrainSegment parent, int x, int y) {
            this.parent = parent;
            this.x = x; this.y = y;
        }
    }

    public class OctreeArea {
        TerrainSegment parent { get; }

        public XYPair offset { get; }
        public XYPair size { get; }

        public OctreeArea[,] octree;

        public Vector3 top_left => (parent.top_left + offset.ToVector3XZ());
        public Vector3 top_right => top_left + new Vector3(size.X, 0, 0);

        public Vector3 bottom_left => top_left + new Vector3(0, 0, size.Y);
        public Vector3 bottom_right => top_left + size.ToVector3XZ();

        public BoundingBox aabb => new BoundingBox(
                top_left + (Vector3.Up * lowest_point),
                bottom_right + (Vector3.Up * highest_point));

        public float highest_point, lowest_point;

        public OctreeArea(TerrainSegment parent, XYPair offset, XYPair size) {
            this.parent = parent;
            this.offset = offset;
            this.size = size;

            highest_point = float.MinValue;
            lowest_point = float.MaxValue;

            for (int y = offset.Y; y < offset.Y + size.Y + 1; y++) {
                for (int x = offset.X; x < offset.X + size.X + 1; x++) {
                    if (parent.data[x, y] > highest_point)
                        highest_point = parent.data[x, y];
                    if (parent.data[x, y] < lowest_point)
                        lowest_point = 0;
                }
            }
        }
    }

    public class TerrainSegment {
        public float[,] data;
        public TerrainQuad[,] quads;

        public XYPair size;

        public SegmentedTerrain parent { get; }

        public XYPair offset { get; }

        public Vector3 top_left => (parent.top_left + offset.ToVector3XZ());
        public Vector3 top_right => top_left + new Vector3(size.X, 0, 0);

        public Vector3 bottom_left => top_left + new Vector3(0, 0, size.Y);
        public Vector3 bottom_right => top_left + size.ToVector3XZ();

        public int LOD_count = 4;
        public int LOD_level = 0;

        public Vector2 overall_UV_position = Vector2.Zero;

        public VertexBuffer[] LOD_vertex_buffers;
        public IndexBuffer[] LOD_index_buffers;

        public float highest_point, lowest_point;

        string heightmap = "terrain_test";

        public BoundingBox aabb => new BoundingBox(
                top_left + (Vector3.Up * lowest_point),
                bottom_right + (Vector3.Up * highest_point));

        public (int, int) index { get; }

        public float distance_to_camera => Vector3.Distance(CollisionHelper.closest_point_on_AABB(EngineState.camera.position, aabb.Min, aabb.Max), EngineState.camera.position);

        public SceneRenderInfo render_info { get; set; }

        public bool built { get; set; } = false;

        public TerrainSegment(SegmentedTerrain parent, (int, int) index, XYPair offset, Vector2 UV_pos) {
            this.parent = parent;
            this.offset = offset;
            this.size = parent.segment_size;
            this.overall_UV_position = UV_pos;
            this.index = index;

            render_info = new SceneRenderInfo() {
                textures = new string[1] { parent.texture },
                vertex_buffers = LOD_vertex_buffers,
                index_buffers = LOD_index_buffers,
                draw_buffers = new bool[3] { true, false, false },
                tint = Color.White,
                render = true
            };

        }

        public void debug_draw(Color color, bool draw_all_quads) {
            if (draw_all_quads) {
                foreach (TerrainQuad q in quads) {
                    if (q.even || q.odd) {
                        Draw3D.triangle(q.A, q.B, q.D, color);
                        Draw3D.triangle(q.C, q.D, q.B, color);
                    } else {
                        Draw3D.triangle(q.A, q.B, q.C, color);
                        Draw3D.triangle(q.A, q.C, q.D, color);
                    }
                }
            }

            Draw3D.cube(aabb, Color.LightGreen);
        }

        public void debug_draw_quad((int, int) index, Color color) {
            var q = quads[index.Item1, index.Item2];

            if (q.even || q.odd) {
                Draw3D.triangle(q.A, q.B, q.D, color);
                Draw3D.triangle(q.C, q.D, q.B, color);
            } else {
                Draw3D.triangle(q.A, q.B, q.C, color);
                Draw3D.triangle(q.A, q.C, q.D, color);
            }
                       

        }

        public void build_from_data(float[,] data) {
            if (data.Length != (size.X + 1) * (size.Y + 1)) throw new Exception("Incorrect data layout for terrain segment");

            for (int y = 0; y < size.Y + 1; y++) {
                for (int x = 0; x < size.X + 1; x++) {

                }
            }
        }

        public void build_from_data(float[] data) {
            if (data.Length != (size.X + 1) * (size.Y + 1)) throw new Exception("Incorrect data layout for terrain segment");


        }

        public void build_from_texture(string texture_name, float max_height = 25f) { 
            Color[] tex_data = new Color[(size.X) * (size.Y)];
            ContentHandler.resources[heightmap].value_tx.GetData<Color>(tex_data);
            if (tex_data.Length != (size.X) * (size.Y)) throw new Exception("Incorrect data layout for terrain segment");

            highest_point = float.MinValue;
            lowest_point = float.MaxValue;
            max_height *= RNG.rng_float;
            for (int y = 0; y < size.Y; y++) {
                for (int x = 0; x < size.X; x++) {
                    var t = tex_data[x + (y * size.X)].ToVector3();
                    data[x, y] = ((t.X + t.Y + t.Z) / 3f) * (max_height);

                    if (data[x, y] > highest_point)
                        highest_point = data[x, y];

                    if (data[x, y] < lowest_point)
                        lowest_point = data[x, y];
                }
            }
        }

        public void fill_with_random(float max_height = 3f)  {
            highest_point = float.MinValue;
            lowest_point = float.MaxValue;

            for (int y = 0; y < size.Y + 1; y++) {
                for (int x = 0; x < size.X + 1; x++) {
                    data[x, y] = RNG.rng_float * max_height;

                    if (data[x, y] > highest_point)
                        highest_point = data[x, y];

                    if (data[x, y] < lowest_point)
                        lowest_point = data[x, y];
                }
            }
        }

        public void build_quads() {
            for (int y = 0; y < size.Y; y++) {
                for (int x = 0; x < size.Y; x++) {
                    quads[x, y] = new TerrainQuad(this, x, y);
                }
            }
        }

        Triangle tri_a = new Triangle();
        Triangle tri_b = new Triangle();


        private class dist_comp : Comparer<(int, int, float)> {
            public override int Compare((int, int, float) x, (int, int, float) y) {
                return x.Item3.CompareTo(y.Item3);
            }
        }

        public bool raycast(Vector3 start, Vector3 end, out (int, int) quad_index, out raycast_result result) {
            quad_index = (-1, -1);
            result = new raycast_result() {
                hit = false,
                distance = float.MaxValue,
                hit_normal = Vector3.Zero,
                point = Vector3.Zero
            };

            //check AABB for entire segment
            if (!Raycasting.ray_intersects_BoundingBox(start,end, aabb.Min, aabb.Max, out _)) { return false; }

            //check first octree layer
            List<(OctreeArea, float)> octree_hits = new List<(OctreeArea, float)>();
            foreach (OctreeArea oa in octree) {
                if (Raycasting.ray_intersects_BoundingBox(start, end, oa.aabb.Min, oa.aabb.Max, out _)) {
                    octree_hits.Add((oa, Vector3.Distance(EngineState.camera.position, (oa.top_left + oa.bottom_right / 2))));
                }
            }

            octree_hits.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            //check second octree layer within first layer octree hits
            List<(int, int, float)> hit_terrain = new List<(int, int, float)>();
            foreach ((OctreeArea, float) oa in octree_hits) {
                //check second layer within first layer hits
                foreach (OctreeArea oa2 in oa.Item1.octree) {
                    if (Raycasting.ray_intersects_BoundingBox(start, end, oa2.aabb.Min, oa2.aabb.Max, out _)) {

                        //third layer within second
                        foreach (OctreeArea oa3 in oa2.octree) {
                            if (Raycasting.ray_intersects_BoundingBox(start, end, oa3.aabb.Min, oa3.aabb.Max, out _)) {

                                //success, add terrain within octree to hit test list
                                for (int y = oa3.offset.Y; y < oa3.offset.Y + oa3.size.Y; y++) {
                                    for (int x = oa3.offset.X; x < oa3.offset.X + oa3.size.X; x++) {
                                        hit_terrain.Add((x, y, Vector3.Distance(EngineState.camera.position, quads[x, y].center_on_line)));
                                    }
                                }
                            }
                        }
                    }
                }                    
            }

            //check each of the quads within each of the octrees
            hit_terrain.Sort(new dist_comp());
            List<(raycast_result, int, int)> hits = new List<(raycast_result, int, int)>();

            foreach ((int, int, float) xy in hit_terrain) {
                var q = quads[xy.Item1, xy.Item2];
                raycast_result res_a, res_b;

                if (q.even || q.odd) {
                    if (Vector3.Distance((q.A + q.B + q.D) / 3f, EngineState.camera.position) < Vector3.Distance((q.C + q.D + q.B) / 3f, EngineState.camera.position)) {
                        ray_intersects_triangle(start, end, q.A, q.B, q.D, out res_a);
                        ray_intersects_triangle(start, end, q.C, q.D, q.B, out res_b);
                    } else {
                        ray_intersects_triangle(start, end, q.C, q.D, q.B, out res_a);
                        ray_intersects_triangle(start, end, q.A, q.B, q.D, out res_b);
                    }

                } else {
                    if (Vector3.Distance((q.A + q.B + q.C) / 3f, EngineState.camera.position) < Vector3.Distance((q.A + q.C + q.D) / 3f, EngineState.camera.position)) {
                        ray_intersects_triangle(start, end, q.A, q.B, q.C, out res_a);
                        ray_intersects_triangle(start, end, q.A, q.C, q.D, out res_b);
                    } else {
                        ray_intersects_triangle(start, end, q.A, q.C, q.D, out res_a);
                        ray_intersects_triangle(start, end, q.A, q.B, q.C, out res_b);
                    }
                }
                if (res_a.hit || res_b.hit) {
                    quad_index = (xy.Item1, xy.Item2);

                    if (res_a.hit) {
                        //result = res_a;
                        hits.Add((res_a, xy.Item1, xy.Item2));
                    } else {
                        //result = res_b;
                        hits.Add((res_b, xy.Item1, xy.Item2));
                    }

                    //return true;
                }

            }

            float c = float.MaxValue;
            foreach((raycast_result, int, int) r in hits) {
                if (r.Item1.distance < c) {
                    c = r.Item1.distance;
                    result = r.Item1;
                    quad_index = (r.Item2, r.Item3);
                }
            }

            if (result.hit)
                return true;

            return false;
        }

        public void create_segment_LOD_buffers() {

        }

        public void build_buffers() {
            LOD_vertex_buffers = new VertexBuffer[3];
            LOD_vertex_buffers[0] = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, (size.Y + 1) * (size.X + 1) * 2, BufferUsage.None);

            LOD_index_buffers = new IndexBuffer[3];
            LOD_index_buffers[0] = new IndexBuffer(EngineState.graphics_device, IndexElementSize.ThirtyTwoBits, (size.Y + 1) * (size.X + 1) * 6, BufferUsage.None);

            VertexPositionNormalTexture[] data = new VertexPositionNormalTexture[(size.Y + 1) * (size.X + 1)];
            int[] indices = new int[(size.Y + 1) * (size.X + 1) * 6];

            Vector2 segment_uv_frac = new Vector2(1f / parent.segment_count.X, 1f / parent.segment_count.Y);


            int linear = 0;
            for (int y = 0; y < size.Y + 1; y++) {
                for (int x = 0; x < size.X + 1; x++) {

                    Vector2 segment_uv_pos = new Vector2((float)x / size.X, (float)y / size.Y);

                    data[linear] = new VertexPositionNormalTexture(data_v3_pos(x, y), Vector3.Zero,
                            overall_UV_position + (segment_uv_frac * segment_uv_pos)
                        );
                    linear++;
                }
            }


            linear = 0;
            for (int y = 0; y < size.Y + 1; y++) {
                for (int x = 0; x < size.X + 1; x++) {
                    if (y < size.Y && x < size.X && ((y % 2 == 0 && x % 2 == 0) || (y % 2 == 1 && x % 2 == 1))) {
                        indices[linear + 0] = x + (y * (size.X + 1));
                        indices[linear + 1] = (x + 1) + (y * (size.X + 1));
                        indices[linear + 2] = (x) + ((y + 1) * (size.X + 1));

                        indices[linear + 3] = (x + 1) + ((y + 1) * (size.X + 1));
                        indices[linear + 4] = (x) + ((y + 1) * (size.X + 1));
                        indices[linear + 5] = (x + 1) + ((y) * (size.X + 1));

                        linear += 6;
                    } else if (y < size.Y && x < size.X) {
                        indices[linear + 0] = x + (y * (size.X + 1));
                        indices[linear + 1] = (x + 1) + (y * (size.X + 1));
                        indices[linear + 2] = (x + 1) + ((y + 1) * (size.X + 1));

                        indices[linear + 3] = x + (y * (size.X + 1));
                        indices[linear + 4] = (x + 1) + ((y + 1) * (size.X + 1));
                        indices[linear + 5] = (x) + ((y + 1) * (size.X + 1));

                        linear += 6;
                    }
                }
            }
            for (int i = 0; i < indices.Length / 3; i++) {
                Vector3 AB = data[indices[i * 3 + 1]].Position - data[indices[i * 3]].Position;
                Vector3 CA = data[indices[i * 3]].Position - data[indices[i * 3 + 2]].Position;
                Vector3 norm = Vector3.Cross(AB, CA);
                norm.Normalize();
                data[indices[i * 3]].Normal = norm;
                data[indices[i * 3+1]].Normal = norm;
                data[indices[i * 3+2]].Normal = norm;
                data[indices[i * 3]].Normal.Normalize();
                data[indices[i * 3 + 1]].Normal.Normalize();
                data[indices[i * 3 + 2]].Normal.Normalize();
            }

            //X = 0, Y != 0, left side, get corner data from above
            if (index.Item1 == 0 && index.Item2 != 0) {

            //Y = 0, X != 0, top, get corner data from left
            } else if (index.Item2 == 0 && index.Item1 != 0) {

            //top left segment, set to up
            } else if (index.Item1 == 0 && index.Item2 == 0) {
                data[0].Normal = Vector3.Up;

            //everywhere else, doesn't matter
            } else {
                data[0].Normal = Vector3.Up;

                //data[0].Normal = parent.segments[index.Item1, index.Item2].data_v3_pos[parent.segment_size.X, parent.segment_size.Y]
            }
            
            //for (int i = 0; i < data.Length; i++) {
            //    if (data[i].Normal != Vector3.Zero) data[i].Normal.Normalize();
            //}

            LOD_vertex_buffers[0].SetData(data);
            LOD_index_buffers[0].SetData(indices);
            Thread.Sleep(2);
            render_info = new SceneRenderInfo() {
                textures = new string[1] { parent.texture },
                vertex_buffers = LOD_vertex_buffers,
                index_buffers = LOD_index_buffers,
                draw_buffers = new bool[3] { true, false, false },
                tint = Color.White,
                render = true
            };

            built = true;
        }

        public const int octree_depth = 3;
        public const int octree_regions_per_axis = 2;

        public OctreeArea[,] octree;

        public void build_octrees() {
            XYPair octree_size = (this.size+XYPair.One) / octree_regions_per_axis;
            XYPair octree_inner_size = octree_size / octree_regions_per_axis;

            //create first layer
            octree = new OctreeArea[octree_regions_per_axis, octree_regions_per_axis];

            //fill first layer
            for (int y = 0; y < octree_regions_per_axis; y++) {
                for (int x = 0; x < octree_regions_per_axis; x++) {
                    octree[x, y] = new OctreeArea(this, octree_size * new XYPair(x, y), octree_size);
                    octree[x, y].octree = new OctreeArea[octree_regions_per_axis, octree_regions_per_axis];

                    //fill second layer
                    for (int y2 = 0; y2 < octree_regions_per_axis; y2++) {
                        for (int x2 = 0; x2 < octree_regions_per_axis; x2++) {
                            octree[x, y].octree[x2, y2] = new OctreeArea(this, (octree_size * new XYPair(x, y)) + (octree_inner_size * new XYPair(x2, y2)), octree_inner_size);
                            octree[x, y].octree[x2, y2].octree = new OctreeArea[octree_regions_per_axis, octree_regions_per_axis];

                            //fill third layer
                            for (int y3 = 0; y3 < octree_regions_per_axis; y3++) {
                                for (int x3 = 0; x3 < octree_regions_per_axis; x3++) {
                                    octree[x, y].octree[x2, y2].octree[x3, y3] = 
                                        new OctreeArea(this, 
                                        (octree_size * new XYPair(x, y)) + ((octree_inner_size) * new XYPair(x2, y2)) + ((octree_inner_size / 2) * new XYPair(x3, y3)), 
                                        (octree_inner_size / 2));

                                }
                            }
                        }
                    }
                }
            }
        }

        public Vector3 data_v3_pos(int x, int y) {

            return top_left + new Vector3(x, data[x, y], y);
        }

    }

    
}
