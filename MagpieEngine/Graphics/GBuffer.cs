﻿using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public struct GBuffer {
        public RenderTarget2D rt_diffuse;
        public RenderTarget2D rt_normal;
        public RenderTarget2D rt_depth;
        public RenderTarget2D rt_lighting;

        public RenderTarget2D rt_final_half;
        public RenderTarget2D rt_final;
        public RenderTarget2D rt_2D;

        public RenderTarget2D rt_fxaa;

        private bool FXAA;
        public bool FXAA_enabled => FXAA;

        private int _width;
        private int _height;

        public XYPair position;

        public int width => _width;
        public int height => _height;

        public int width_scaled => (int)(_width * resolution_scale);
        public int height_scaled => (int)(_height * resolution_scale);

        public float width_scaled_f => _width * resolution_scale;
        public float height_scaled_f => _height * resolution_scale;

        public Vector2 shader_position_offset;
        public Vector2 shader_size_scale;

        public XYPair resolution => (XYPair.UnitX * _width) + (XYPair.UnitY * _height);
        public XYPair resolution_super => (XYPair.UnitX * width_scaled) + (XYPair.UnitY * width_scaled);

        public float aspect_ratio => (float)width / (float)height;

        private float _resolution_scale;
        public float resolution_scale => _resolution_scale;

        public RenderTargetBinding[] buffer_targets { get; private set; }
        public RenderTargetBinding[] buffer_targets_dl { get; private set; }
        public RenderTargetBinding[] buffer_targets_dln { get; private set; }

        public Viewport viewport;

        public void change_resolution(GraphicsDevice gd, int W, int H) {
            _width = W;
            _height = H;

            CreateInPlace(gd, W, H, 1, false);
        }

        public void CreateInPlace(GraphicsDevice gd, int width, int height, float res_scale = 1.0f, bool fxaa = false) {
            buffer_targets = new RenderTargetBinding[4];
            buffer_targets_dl = new RenderTargetBinding[2];
            buffer_targets_dln = new RenderTargetBinding[3];

            FXAA = fxaa;

            position = XYPair.Zero;

            if (res_scale != 1.0f)
                FXAA = false;

            this._width = width; this._height = height;

            _resolution_scale = res_scale;

            viewport = new Viewport(position.X, position.Y, width, height);

            shader_position_offset = Vector2.Zero;
            shader_size_scale = Vector2.One;

            rt_diffuse = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            rt_normal = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Vector4, DepthFormat.None);
            rt_depth = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            //TODO IMPLEMENT ALPHA TEST PASS
            //THIS COULD BE USED ALONGSIDE LIGHT SHADERS TO GET FREE SHADOWS EVEN WITH POINT LIGHTS
            //rt_alpha = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Vector4, DepthFormat.Depth24Stencil8);
            rt_lighting = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);

            rt_final = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            rt_final_half = new RenderTarget2D(gd, (int)(width / 2), (int)(height / 2), false, SurfaceFormat.Color, DepthFormat.None);

            rt_2D = new RenderTarget2D(gd, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            buffer_targets[0] = rt_diffuse;
            buffer_targets[1] = rt_normal;
            buffer_targets[2] = rt_depth;
            buffer_targets[3] = rt_lighting;

            buffer_targets_dl[0] = rt_depth;
            buffer_targets_dl[1] = rt_lighting;

            buffer_targets_dl[0] = rt_depth;
            buffer_targets_dln[1] = rt_lighting;
            buffer_targets_dln[2] = rt_normal;

            if (FXAA) {
                rt_fxaa = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            }
        }

        public void EnableFXAA(GraphicsDevice gd, bool enable = true) {
            if (enable && !FXAA && resolution_scale == 1.0f) {
                rt_fxaa = new RenderTarget2D(gd, _width, _height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
                FXAA = true;
            }
            else if (!enable) {
                rt_fxaa.Dispose();
                rt_fxaa = null;
                FXAA = false;
            }
        }

        public static GBuffer Create(GraphicsDevice gd, int width, int height, float res_scale = 1.0f, bool fxaa = false) {
            GBuffer gbuffer = new GBuffer();

            gbuffer.buffer_targets = new RenderTargetBinding[4];

            gbuffer.FXAA = fxaa;

            if (res_scale != 1.0f)
                gbuffer.FXAA = false;

            gbuffer._width = width; gbuffer._height = height;
            gbuffer._resolution_scale = res_scale;

            gbuffer.shader_position_offset = Vector2.Zero;
            gbuffer.shader_size_scale = Vector2.One;
            
            gbuffer.rt_diffuse = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
            gbuffer.rt_normal = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            gbuffer.rt_depth = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Single, DepthFormat.Depth24Stencil8);
            gbuffer.rt_lighting = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);

            gbuffer.rt_final = new RenderTarget2D(gd, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None);
            gbuffer.rt_final_half = new RenderTarget2D(gd, (int)(width / 2), (int)(height / 2), false, SurfaceFormat.Color, DepthFormat.None);

            gbuffer.rt_2D = new RenderTarget2D(gd, (int)(width), (int)(height), false, SurfaceFormat.Color, DepthFormat.None);

            gbuffer.buffer_targets[0] = gbuffer.rt_diffuse;
            gbuffer.buffer_targets[1] = gbuffer.rt_normal;
            gbuffer.buffer_targets[2] = gbuffer.rt_depth;
            gbuffer.buffer_targets[3] = gbuffer.rt_lighting;

            if (gbuffer.FXAA) {
                gbuffer.rt_fxaa = new RenderTarget2D(gd, (int)(width * res_scale), (int)(height * res_scale), false, SurfaceFormat.Color, DepthFormat.None);
            }

            return gbuffer;
        }
    }
}
