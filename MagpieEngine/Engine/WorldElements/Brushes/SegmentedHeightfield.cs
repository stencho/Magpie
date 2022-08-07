using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Engine.Brushes {
    public class SegmentedHeightfield : Brush {
        public BrushType type => BrushType.SEGMENTED_HEIGHTFIELD;

        public class HFSegment {
            public float[,] data;

            public XYPair size;

            public SegmentedHeightfield parent { get; }

            public Vector2 offset { get; }

            public Vector3 top_left => (parent.top_left + offset.ToVector3XZ());
            public Vector3 top_right => top_left + new Vector3(size.X, 0, 0);

            public Vector3 bottom_left => top_left + new Vector3(0, 0, size.Y);
            public Vector3 bottom_right => top_left + size.ToVector3XZ();

            public int LOD_count = 4;
            public int LOD_level = 0;

            public Vector2 overall_UV_position = Vector2.Zero;

            public VertexBuffer[] LOD_vertex_buffers;
            public IndexBuffer[] LOD_index_buffers;

            public HFSegment(SegmentedHeightfield parent, Vector2 offset) {
                this.parent = parent;
                this.offset = offset;
            }

            public void build_from_data(float[,] data) {

                build_buffers();
            }

            public void build_from_data(float[] data, int stride) {

                build_buffers();
            }


            private void build_buffers() {

            }

            public Vector3 data_v3_pos(int x, int y) {

                return top_left + new Vector3(x, data[x, y], y);
            }
        }

        public HFSegment[,] segments;

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

        public Vector3 position { get; set; } = Vector3.Zero;

        public Vector3 top_left => position - size_over_2.ToVector3XZ();
        public Vector3 top_right => position + new Vector3(size_over_2.X, 0, -size_over_2.Y);

        public Vector3 bottom_left => position + new Vector3(-size_over_2.X, 0, size_over_2.Y);
        public Vector3 bottom_right => position + size_over_2.ToVector3XZ();

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Matrix world => orientation * Matrix.CreateTranslation(position);

        float _highest_value = float.MinValue;
        float _lowest_value = float.MaxValue;

        public Shape3D collision { get; set; }

        public string texture { get; set; } = "zerocool_sharper";

        public Vector3 movement_vector { get; set; } = Vector3.Zero;
        public Vector3 final_position { get; set; }

        public SegmentedHeightfield(Vector3 position, XYPair size, XYPair segment_size) {
            this.position = position;

            this.size = size;
            this.segment_size = segment_size;

            this.segment_count = size / segment_size;

            collision = new DummySupport();

            initialize_segments();
        }
        
        public SegmentedHeightfield(Vector3 position, int size, int segments_per_axis) {
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
            segments = new HFSegment[segment_count.X, segment_count.Y];

            //iterate over each of the segments
            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    Vector2 offset = new Vector2(sx * segment_size.X, sy * segment_size.Y);
                    var current_segment = new HFSegment(this, offset);

                    current_segment.data = new float[segment_size.X + 1, segment_size.Y + 1];
                    fill_segment_with_random(current_segment);

                    //the reason why all the segment_sizes have +1 on them is because there 
                    //needs to be an extra row and column of data on each segment to hold the 
                    //left and top sides of the segments to the right of them and below them
                    //including the data for the final right and bottom rows which would otherwise not exist


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

                    current_segment.size = segment_size;

                    current_segment.overall_UV_position = offset / size.ToVector2();


                    segments[sx, sy] = current_segment;
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
                    build_segment_buffers(ref segments[sx, sy]);
                }
            }
        }


        private void build_segment_buffers(ref HFSegment seg) {
            seg.LOD_vertex_buffers = new VertexBuffer[1];
            seg.LOD_vertex_buffers[0] = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, (segment_size.Y + 1) * (segment_size.X + 1) * 2, BufferUsage.None);

            seg.LOD_index_buffers = new IndexBuffer[1];
            seg.LOD_index_buffers[0] = new IndexBuffer(EngineState.graphics_device, IndexElementSize.ThirtyTwoBits, (segment_size.Y + 1) * (segment_size.X + 1) * 6, BufferUsage.None);

            VertexPositionNormalTexture[] data = new VertexPositionNormalTexture[(segment_size.Y + 1) * (segment_size.X + 1)];
            int[] indices = new int[(segment_size.Y + 1) * (segment_size.X + 1) * 6];

            Vector2 segment_uv_frac = new Vector2(1f / segment_count.X, 1f / segment_count.Y);

            int linear = 0;
            for (int y = 0; y < segment_size.Y+1; y++) {
                for (int x = 0; x < segment_size.X+1; x++) {

                    Vector2 segment_uv_pos = new Vector2((float)x / segment_size.X, (float)y / segment_size.Y);

                    data[linear] = new VertexPositionNormalTexture(seg.data_v3_pos(x, y), Vector3.Up,
                            seg.overall_UV_position + (segment_uv_frac * segment_uv_pos)
                        );
                    linear++;
                }
            }

            linear = 0;
            for (int y = 0; y < segment_size.Y+1; y++) {
                for (int x = 0; x < segment_size.X+1; x++) {
                    if (y < segment_size.Y && x < segment_size.X && ((y % 2 == 0 && x % 2 == 0) || (y % 2 == 1 && x % 2 == 1))) {
                        indices[linear + 0] = x + (y * (segment_size.X + 1));
                        indices[linear + 1] = (x + 1) + (y * (segment_size.X + 1));
                        indices[linear + 2] = (x) + ((y + 1) * (segment_size.X + 1));

                        indices[linear + 3] = (x + 1) + ((y + 1) * (segment_size.X + 1));
                        indices[linear + 4] = (x) + ((y + 1) * (segment_size.X + 1));
                        indices[linear + 5] = (x + 1) + ((y) * (segment_size.X + 1));

                        linear += 6;
                    } else if (y < segment_size.Y && x < segment_size.X) {
                        indices[linear + 0] = x + (y * (segment_size.X+1));
                        indices[linear + 1] = (x + 1) + (y * (segment_size.X + 1));
                        indices[linear + 2] = (x + 1) + ((y + 1) * (segment_size.X + 1));

                        indices[linear + 3] = x + (y * (segment_size.X + 1));
                        indices[linear + 4] = (x + 1) + ((y + 1) * (segment_size.X + 1));
                        indices[linear + 5] = (x) + ((y + 1) * (segment_size.X + 1));

                        linear += 6;
                    }
                }

            }



            seg.LOD_vertex_buffers[0].SetData(data);
            seg.LOD_index_buffers[0].SetData(indices);
        }

        private void fill_segment_with_random(HFSegment seg, float max_height = 0.3f) {
            for (int y = 0; y < segment_size.Y + 1; y++) {
                for (int x = 0; x < segment_size.X + 1; x++) {
                    seg.data[x, y] = RNG.rng_float * max_height;
                }
            }
        }
        private void fill_segment_with_random(int X, int Y, float max_height = 0.3f) {
            var seg = segments[X, Y];

            for (int y = 0; y < segment_size.Y + 1; y++) {
                for (int x = 0; x < segment_size.X + 1; x++) {
                    seg.data[x, y] = RNG.rng_float * max_height;
                }
            }
        }

        private void fill_all_segments_with_random(float max_height = 0.3f) {
            segments = new HFSegment[segment_count.X, segment_count.Y];
            //iterate over each of the segments
            for (int sy = 0; sy < segment_count.Y+1; sy++) {
                for (int sx = 0; sx < segment_count.X+1; sx++) {
                    var current_segment = segments[sx, sy];
                                        
                    for (int y = 0; y < segment_size.Y; y++) {
                        for (int x = 0; x < segment_size.Y; x++) {
                            current_segment.data[x, y] = RNG.rng_float * max_height;
                        }
                    }
                    
                }
            }
        }
               
        public void Update() {

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

        public void debug_draw() {
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            RenderTargetBinding[] rtb = EngineState.graphics_device.GetRenderTargets();

            //EngineState.graphics_device.SetRenderTargets(EngineState.buffer.buffer_targets);
            //Draw3D.draw_buffers(segments[0, 0].LOD_vertex_buffers[0], segments[0, 0].LOD_index_buffers[0], world, Color.White);


            EngineState.graphics_device.SetRenderTarget(EngineState.buffer.rt_2D);


            Draw3D.xyz_cross(position, 1f, Color.Red);

            Draw3D.xyz_cross(top_left, 5f, Color.Red);
            Draw3D.xyz_cross(top_right, 5f, Color.Green);
            Draw3D.xyz_cross(bottom_right, 5f, Color.Blue);
            Draw3D.xyz_cross(bottom_left, 5f, Color.Yellow);

            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    var s = segments[sx, sy];
                    Draw3D.sphere(s.top_left, 0.1f, Color.White);



                    Vector4 col = new Vector4(1, 0, s.overall_UV_position.X, 1f);
                    Draw3D.line(s.top_left, s.top_right, Color.FromNonPremultiplied(col));

                    col = new Vector4(s.overall_UV_position.Y, 0, 1, 1f);

                    Draw3D.line(s.top_left, s.bottom_left, Color.FromNonPremultiplied(col));

                    col = Color.HotPink.ToVector4();

                    for (int y = 0; y < segment_size.Y + 1; y++) {
                        for (int x = 0; x < segment_size.X + 1; x++) {
                            if (x == 0 || y == 0 || x == segment_size.X || y == segment_size.Y) {
                                col = new Vector4(1, 0, 0, 1);
                            } else {
                                col = Color.HotPink.ToVector4();
                            }

                            //Draw3D.xyz_cross(s.top_left + new Vector3(x, s.data[x, y], y), 0.5f, Color.FromNonPremultiplied(col));
                        }
                    }
                }
            }


            Draw3D.lines(Color.White, top_left, top_right, bottom_right, bottom_left, top_left);
            
            EngineState.graphics_device.SetRenderTargets(rtb);
            //Draw3D.line()
        }
    }
}
