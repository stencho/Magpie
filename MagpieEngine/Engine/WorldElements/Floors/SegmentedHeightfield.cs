using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Magpie.Engine.Floors {
    public class SegmentedHeightfield : Floor {
        public FloorType type => FloorType.SEGMENTED_HEIGHTFIELD;

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
        }

        HFSegment[,] segments;

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
                } else {
                    throw new Exception("input segment size must be divisions of overall heightfield width and height size");
                }
            }
        }

        XYPair segment_count = XYPair.Zero;

        public Vector3 position { get; set; } = Vector3.Zero;

        public Vector3 top_left => position - size_over_2.ToVector3XZ();
        public Vector3 top_right => position + new Vector3(size_over_2.X, 0, -size_over_2.Y);

        public Vector3 bottom_left => position + new Vector3(-size_over_2.X, 0, size_over_2.Y);
        public Vector3 bottom_right => position + size_over_2.ToVector3XZ();

        public Matrix orientation { get; set; } = Matrix.Identity;
        public Matrix world => orientation * Matrix.CreateTranslation(position);

        float _highest_value = float.MinValue;
        float _lowest_value = float.MaxValue;

        public BoundingBox bounds { get; set; }
        
        public string texture { get; set; } = "zerocool_sharper";

        public SegmentedHeightfield(Vector3 position, XYPair size, XYPair segment_size) {
            this.position = position;
            this.size = size;
            this.segment_size = segment_size;

            this.segment_count = size / segment_size;

            initialize_segments();
        }
        
        public SegmentedHeightfield(Vector3 position, XYPair size, int segments_per_axis) {
            this.position = position;
            this.size = size;
            
            if (size.X % segments_per_axis != 0 || size.Y % segments_per_axis != 0) {
                throw new Exception("input segment size must be divisions of overall heightfield width and height size");
            }

            this.segment_size = size / segments_per_axis;
            this.segment_count = new XYPair(segments_per_axis);

            initialize_segments();
        }

        private void initialize_segments() {
            segments = new HFSegment[segment_count.X, segment_count.Y];
            //iterate over each of the segments
            for (int sy = 0; sy < segment_count.Y; sy++) {
                for (int sx = 0; sx < segment_count.X; sx++) {
                    Vector2 offset = new Vector2(sx * segment_size.X, sy * segment_size.Y);
                    var current_segment = new HFSegment(this, offset);

                    current_segment.data = new float[segment_size.X+1, segment_size.Y+1];
                    fill_segment_with_random(current_segment);

                    //the reason why all the segment_sizes have +1 on them is because there 
                    //needs to be an extra row and column of data on each segment to hold the 
                    //left and top sides of the segments to the right of them and below them
                    //including the data for the final right and bottom rows which would otherwise not exist
                    

                    if (sx>0) {
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
                    /*
                    for (int y = 0; y < segment_size.Y; y++) {
                        for (int x = 0; x < segment_size.Y; x++) {
                            current_segment.data[x, y] = 0f;
                        }
                    }
                    */
                }
            }
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
        public void Draw() {
        }
        public void DrawDebug() {
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;

            RenderTargetBinding[] rtb = EngineState.graphics_device.GetRenderTargets();
            EngineState.graphics_device.SetRenderTarget(EngineState.buffer.rt_2D);


            Draw3D.xyz_cross(position, 1f, Color.Red);

            Draw3D.xyz_cross(top_left, 5f, Color.Red);
            Draw3D.xyz_cross(top_right, 5f, Color.Green);
            Draw3D.xyz_cross(bottom_right, 5f, Color.Blue);
            Draw3D.xyz_cross(bottom_left, 5f, Color.Yellow);

            for (int sy = 0; sy < 2/*segment_count.Y*/; sy++) {
                for (int sx = 0; sx < 2/*segment_count.X*/; sx++) {
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

                            Draw3D.xyz_cross(s.top_left + new Vector3(x, s.data[x, y], y), 0.5f, Color.FromNonPremultiplied(col));
                        }
                    }
                }
            }
            

            Draw3D.lines(Color.White, top_left, top_right, bottom_right, bottom_left, top_left);

            EngineState.graphics_device.SetRenderTargets(rtb);
            //Draw3D.line()

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
    }
}
