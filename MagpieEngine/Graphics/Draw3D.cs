using Magpie.Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Graphics {
    public class Draw3D {
        static VertexPositionColor[] verts = new VertexPositionColor[2];
        public static Effect line_effect;
        public static BasicEffect basic_effect;
        public static Texture2D onePXWhite;
        public static Texture2D testing_gradient;

        public static bool vector3_contains_nan(Vector3 a) { return (float.IsNaN(a.X) || float.IsNaN(a.Y) || float.IsNaN(a.Z)); }

        public static Vector3 find_any_line_perpendicular(Vector3 A, Vector3 B) {
            Vector3 AB = B - A;
            Vector3 dir = Vector3.Normalize(B - A);

            var cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(dir.X, dir.Z, -dir.Y)));
            if (vector3_contains_nan(cross))
                cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(-dir.Z, dir.Y, dir.X)));
            if (vector3_contains_nan(cross))
                cross = Vector3.Cross(dir, Vector3.Cross(dir, new Vector3(dir.Y, -dir.X, dir.Z)));

            return Vector3.Normalize(cross);
        }


        public static void line(Vector3 A, Vector3 B, Color color) {
            line(A, B, color, EngineState.camera.view, EngineState.camera.projection);
        }
        public static void line(Vector3 A, Vector3 B, Color color, Matrix view, Matrix projection) {
            
            line_effect = ContentHandler.resources["fill_gbuffer"].value_fx;
            //ContentHandler.resources["diffuse"].value_fx. = color.ToVector3();

            line_effect.Parameters["World"].SetValue(Matrix.Identity);
            line_effect.Parameters["View"].SetValue(view);
            line_effect.Parameters["Projection"].SetValue(projection);
            line_effect.Parameters["DiffuseMap"].SetValue(onePXWhite);
            line_effect.Parameters["tint"].SetValue(color.ToVector3());
            //line_effect.Parameters["FarClip"].SetValue(2000f);
            //line_effect.Parameters["opacity"].SetValue(-1f);

            verts[0] = new VertexPositionColor(A, color);
            verts[1] = new VertexPositionColor(B, color);

            //line_effect.DiffuseColor = color.ToVector3();

            for (int i = 0; i < line_effect.CurrentTechnique.Passes.Count; i++) {
                line_effect.CurrentTechnique.Passes[i].Apply();
                EngineState.graphics_device.DrawUserPrimitives(PrimitiveType.LineList, verts, 0, 1);
            }

            line_effect.Parameters["tint"].SetValue(Color.White.ToVector3());
        }

        public static void lines(Color color, params Vector3[] points) {
            lines(color, EngineState.camera.view, EngineState.camera.projection, points);
        }
        public static void lines(Color color, Matrix view, Matrix projection, params Vector3[] points) {

            line_effect = ContentHandler.resources["fill_gbuffer"].value_fx;
            //ContentHandler.resources["diffuse"].value_fx. = color.ToVector3();
            line_effect.Parameters["World"].SetValue(Matrix.Identity);
            line_effect.Parameters["View"].SetValue(view);
            line_effect.Parameters["Projection"].SetValue(projection);
            line_effect.Parameters["DiffuseMap"].SetValue(onePXWhite);
            line_effect.Parameters["tint"].SetValue(color.ToVector3());
            //line_effect.Parameters["FarClip"].SetValue(2000f);
            //line_effect.Parameters["opacity"].SetValue(-1f);

            VertexPositionColor[] verts = new VertexPositionColor[points.Length];

            for (int i = 0; i < points.Length; i++) {
                verts[i].Position = points[i];
            }

            EngineState.graphics_device.BlendState = BlendState.Opaque;

            for (int i = 0; i < line_effect.CurrentTechnique.Passes.Count; i++) {
                line_effect.CurrentTechnique.Passes[i].Apply();
                EngineState.graphics_device.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, points.Length - 1);
            }

            line_effect.Parameters["tint"].SetValue(Color.White.ToVector3());
        }

        public static void swept_capsule(float radius, Vector3 AA, Vector3 AB, Vector3 BA, Vector3 BB, Color color, Matrix view, Matrix projection) {
            Vector3 AAAB = Vector3.Normalize(AB - AA);
            Vector3 BABB = Vector3.Normalize(BB - BA);

            capsule(AA, AB, radius, color, view, projection);
            capsule(BA, BB, radius, color, view, projection);

            lines(color, view, projection,
                AA - (AAAB * radius), AB + (AAAB * radius),
                BA - (BABB * radius), BB + (BABB * radius),
                AA - (AAAB * radius)
            );

            lines(color, view, projection,
                AA - (AAAB * radius),
                AB + (AAAB * radius),
                BA - (BABB * radius),
                BB + (BABB * radius),
                AA - (AAAB * radius)
            );

            Vector3 C = Vector3.Normalize(Vector3.Cross(AAAB, (AA - BA)));

            Vector3 ABH = ((AA + AB) / 2f);
            Vector3 BBH = ((BA + BB) / 2f);

            line(ABH - (C * radius), BBH - (C * radius), color, view, projection);
            line(ABH + (C * radius), BBH + (C * radius), color, view, projection);

            lines(color, view, projection,
                AA - (C * radius),
                AB - (C * radius),
                BA - (C * radius),
                BB - (C * radius),
                AA - (C * radius)
            );

            lines(color, view, projection,
                AA + (C * radius),
                AB + (C * radius),
                BA + (C * radius),
                BB + (C * radius),
                AA + (C * radius)
            );

        }

        public static void xyz_cross(Vector3 P, float line_distance, Color color) {
            line(P - (Vector3.UnitX * (line_distance / 2)), P + (Vector3.UnitX * (line_distance / 2)), color, EngineState.camera.view, EngineState.camera.projection);
            line(P - (Vector3.UnitY * (line_distance / 2)), P + (Vector3.UnitY * (line_distance / 2)), color, EngineState.camera.view, EngineState.camera.projection);
            line(P - (Vector3.UnitZ * (line_distance / 2)), P + (Vector3.UnitZ * (line_distance / 2)), color, EngineState.camera.view, EngineState.camera.projection);
        }
        public static void xyz_cross(Vector3 P, float line_distance, Color color, Matrix view, Matrix projection) {
            line(P - (Vector3.UnitX * (line_distance / 2)), P + (Vector3.UnitX * (line_distance / 2)), color, view, projection);
            line(P - (Vector3.UnitY * (line_distance / 2)), P + (Vector3.UnitY * (line_distance / 2)), color, view, projection);
            line(P - (Vector3.UnitZ * (line_distance / 2)), P + (Vector3.UnitZ * (line_distance / 2)), color, view, projection);
        }

        public static void circle(Vector3 p, float radius, Vector3 normal, int subdivs, Color color, Matrix view, Matrix projection) {
            if (subdivs < 6) return;
            VertexPositionColor[] verts = new VertexPositionColor[subdivs];

            normal = Vector3.Normalize(normal);

            var cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.X, normal.Z, -normal.Y))));
            if (float.IsNaN(cross.X) || float.IsNaN(cross.Y) || float.IsNaN(cross.Z)) {
                cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(-normal.Z, normal.Y, normal.X))));
            }
            if (float.IsNaN(cross.X) || float.IsNaN(cross.Y) || float.IsNaN(cross.Z)) {
                cross = Vector3.Normalize(Vector3.Cross(normal, Vector3.Cross(normal, new Vector3(normal.Y, -normal.X, normal.Z))));
            }

            for (int i = 0; i < subdivs; i++) {
                verts[i].Position = p + (Vector3.Transform(cross, Matrix.CreateFromAxisAngle(normal, MathHelper.ToRadians(((float)i / (subdivs - 1)) * 360f))) * (radius));
                verts[i].Color = color;
            }

            //ContentHandler.resources["diffuse"].value_fx. = color.ToVector3();
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["World"].SetValue(Matrix.Identity);
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["View"].SetValue(view);
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["Projection"].SetValue(projection);
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["DiffuseMap"].SetValue(onePXWhite);
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["tint"].SetValue(color.ToVector3());
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["FarClip"].SetValue(2000f);
            ContentHandler.resources["fill_gbuffer"].value_fx.Parameters["opacity"].SetValue(-1f);


            for (int i = 0; i < ContentHandler.resources["fill_gbuffer"].value_fx.CurrentTechnique.Passes.Count; i++) {
                ContentHandler.resources["fill_gbuffer"].value_fx.CurrentTechnique.Passes[i].Apply();
                EngineState.graphics_device.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, verts.Length - 1);
            }
        }

        public static void sphere(Vector3 P, float radius, Color color) {
            sphere(P, radius, color, EngineState.camera.view, EngineState.camera.projection);
        }

        public static void sphere(Vector3 P, float radius, Color color, Matrix view, Matrix projection) {
            Draw3D.circle(P, radius, Vector3.Up, 32, color, view, projection);
            Draw3D.circle(P, radius, Vector3.Right, 32, color, view, projection);
            Draw3D.circle(P, radius, Vector3.Forward, 32, color, view, projection);
        }

        public static void capsule(Vector3 A, Vector3 B, float radius, Color color) {
            capsule(A, B, radius, color, EngineState.camera.view, EngineState.camera.projection);
        }
        public static void capsule(Vector3 A, Vector3 B, float radius, Color color, Matrix view, Matrix projection) {
            line_effect.Parameters["World"].SetValue(Matrix.Identity);

            Vector3 AB = B - A;
            Vector3 normal = Vector3.Normalize(B - A);
            Vector3 origin = (A + B) / 2f;

            var cross = find_any_line_perpendicular(A, B);
            var criss = Vector3.Normalize(Vector3.Cross(normal, cross));

            Draw3D.line(A - (normal * radius), B + (normal * radius), color, view, projection);

            Draw3D.circle(origin, radius, AB, 19, color, view, projection);
            Draw3D.circle(A, radius, AB, 19, color, view, projection);
            Draw3D.circle(B, radius, AB, 19, color, view, projection);

            Draw3D.circle(A, radius, cross, 19, color, view, projection);
            Draw3D.circle(B, radius, cross, 19, color, view, projection);

            Draw3D.line(A + (cross * radius), B + (cross * radius), color, view, projection);
            Draw3D.line(A - (cross * radius), B - (cross * radius), color, view, projection);

            Draw3D.circle(A, radius, criss, 19, color, view, projection);
            Draw3D.circle(B, radius, criss, 19, color, view, projection);

            Draw3D.line(A + (criss * radius), B + (criss * radius), color, view, projection);
            Draw3D.line(A - (criss * radius), B - (criss * radius), color, view, projection);
        }

        public static void cube(Vector3 center, Vector3 size, Color color, Matrix world, Matrix view, Matrix projection) {
            cube(
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Forward), world),     //A
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Forward), world),     //B
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Forward), world),     //D
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Forward), world),     //C

                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Backward), world),     //E
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Up) + ((size.Z) * Vector3.Backward), world),     //F
                Vector3.Transform(center + ((size.X) * Vector3.Right) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Backward), world),     //H
                Vector3.Transform(center + ((size.X) * Vector3.Left) + ((size.Y) * Vector3.Down) + ((size.Z) * Vector3.Backward), world),     //G

            color, view, projection);
        }

        public static void square(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color, Matrix view, Matrix projection) {
            lines(color, view, projection, A, B, C, D, A);
        }

        public static void cube(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Vector3 E, Vector3 F, Vector3 G, Vector3 H, Color color, Matrix view, Matrix projection) {
            //top
            square(A, E, F, B, color, view, projection);
            //right
            square(A, C, G, E, color, view, projection);
            //left
            square(F, H, D, B, color, view, projection);
            //bottom
            square(D, H, G, C, color, view, projection);
        }

        public static Texture2D tum;
        public static Effect light_depth;

        public static void init() {

            if (onePXWhite == null) {
                onePXWhite = new Texture2D(EngineState.graphics_device, 1, 1);
                onePXWhite.SetData<Color>(new Color[1] { Color.White });

                tum = Texture2D.FromFile(EngineState.graphics_device, @"C:\Users\nat\source\repos\Magpie\MagpieDemo\Content\tex\zerocool_sharper.jpg");
                testing_gradient = new Texture2D(EngineState.graphics_device, 256, 256);

                Color[] glowData = new Color[256 * 256];
                for (var i = 0; i < 256; i++) {
                    for (var x = 0; x < 256; x++) {
                        glowData[(i * 256) + x] = Color.FromNonPremultiplied(i, 256 - i, i, 256);
                    }
                }
                testing_gradient.SetData(glowData);

                light_depth = ContentHandler.resources["light_depth"].value_fx;

                text_effect = new BasicEffect(EngineState.graphics_device);
            }
        }

        public static BasicEffect text_effect;
        public static void text_3D(SpriteBatch sb, string text, string fontname, Vector3 offset, Vector3? normal, float scale, Matrix view, Matrix projection, Color color, bool always_visible = false) {
            Vector2 origin = ContentHandler.resources[fontname].value_ft.MeasureString(text) / 2f;
            text_effect.World = Matrix.CreateScale(scale, -scale, 0) * Matrix.CreateLookAt(Vector3.Zero, view.Forward, Vector3.Up) * Matrix.CreateTranslation(offset);
            text_effect.View = view;
            text_effect.Projection = projection;
            text_effect.DiffuseColor = color.ToVector3();
            text_effect.TextureEnabled = true;
            sb.Begin(0, null, SamplerState.PointWrap, (always_visible ? DepthStencilState.None : DepthStencilState.DepthRead), RasterizerState.CullNone, text_effect);
            sb.DrawString(ContentHandler.resources[fontname].value_ft, text, Vector2.Zero, Color.White, 0, origin, 0.015f, SpriteEffects.None, 1);
            sb.End();
        }

        public static void draw_buffers_diffuse_color(VertexBuffer vb, IndexBuffer ib, Color color, Matrix world, Matrix view, Matrix projection) {
            Effect e_diffuse = ContentHandler.resources["fill_gbuffer"].value_fx;

            //ContentHandler.resources["diffuse"].value_fx. = color.ToVector3();
            e_diffuse.Parameters["World"].SetValue(world);
            e_diffuse.Parameters["View"].SetValue(view);
            e_diffuse.Parameters["Projection"].SetValue(projection);
            e_diffuse.Parameters["DiffuseMap"].SetValue(onePXWhite);
            e_diffuse.Parameters["tint"].SetValue(color.ToVector3());
            e_diffuse.Parameters["FarClip"].SetValue(2000f);
            e_diffuse.Parameters["opacity"].SetValue(-1f);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;
            EngineState.graphics_device.SetVertexBuffer(vb);
            EngineState.graphics_device.Indices = ib;

            foreach (EffectTechnique t in e_diffuse.Techniques) {
                foreach (EffectPass p in t.Passes) {
                    p.Apply();
                }
            }

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount));


        }

        public static void draw_buffers_diffuse_texture(VertexBuffer vb, IndexBuffer ib, Texture2D texture, Color color, Matrix world, Matrix view, Matrix projection) {
            Effect e_diffuse = ContentHandler.resources["fill_gbuffer"].value_fx;

            //ContentHandler.resources["diffuse"].value_fx. = color.ToVector3();
            e_diffuse.Parameters["World"].SetValue(world);
            e_diffuse.Parameters["View"].SetValue(view);
            e_diffuse.Parameters["Projection"].SetValue(projection);
            e_diffuse.Parameters["DiffuseMap"].SetValue(texture);
            e_diffuse.Parameters["tint"].SetValue(color.ToVector3());
            e_diffuse.Parameters["FarClip"].SetValue(2000f);
            e_diffuse.Parameters["opacity"].SetValue(-1f);

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            EngineState.graphics_device.BlendState = BlendState.AlphaBlend;
            EngineState.graphics_device.DepthStencilState = DepthStencilState.Default;
            EngineState.graphics_device.SetVertexBuffer(vb);
            EngineState.graphics_device.Indices = ib;

            e_diffuse.Techniques["BasicColorDrawing"].Passes[0].Apply();

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount));
        }


        public static void draw_buffers(VertexBuffer vb, IndexBuffer ib, Matrix world, Color color, Matrix view, Matrix projection) {
            init();

            if (basic_effect == null) {
                basic_effect = new BasicEffect(EngineState.graphics_device);
                basic_effect.World = Matrix.Identity;
            }

            EngineState.graphics_device.RasterizerState = RasterizerState.CullCounterClockwise;
            //gd.RasterizerState = RasterizerState.CullNone;

            basic_effect.DiffuseColor = color.ToVector3();
            basic_effect.TextureEnabled = true;
            basic_effect.Texture = tum;

            basic_effect.World = world;
            basic_effect.View = view;
            basic_effect.Projection = projection;
            basic_effect.EnableDefaultLighting();

            EngineState.graphics_device.SetVertexBuffer(vb, 0);
            EngineState.graphics_device.Indices = ib;

            foreach (EffectTechnique t in basic_effect.Techniques) {
                foreach (EffectPass p in t.Passes) {
                    p.Apply();
                }
            }

            EngineState.graphics_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, (vb.VertexCount));

            basic_effect.World = Matrix.Identity;
        }


        public static VertexPositionNormalTexture[] quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(new Vector3(-1, 1, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(1, 1, 0), -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0), -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), -Vector3.UnitZ, new Vector2(0, 1))
            };

        static ushort[] q_indices = { 0, 1, 2, 2, 3, 0 };

        public static VertexPositionNormalTexture[] tri = new VertexPositionNormalTexture[3] {
                new VertexPositionNormalTexture(new Vector3(0, 1, 0), -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(new Vector3(-1, -1, 0), -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(new Vector3(1, -1, 0), -Vector3.UnitZ, new Vector2(1, 1))
            };
        static ushort[] t_indices = { 0, 2, 1 };

        static VertexBuffer t_vertex_buffer;
        static IndexBuffer t_index_buffer;
        static VertexBuffer q_vertex_buffer;
        static IndexBuffer q_index_buffer;

        static string[] q_textures = new string[] { "OnePXWhite" };

        public static void fill_tri(Matrix world, Vector3 A, Vector3 B, Vector3 C, Color color, Matrix view, Matrix projection) {
            if (t_index_buffer == null) {
                t_index_buffer = new IndexBuffer(EngineState.graphics_device, IndexElementSize.SixteenBits, t_indices.Length, BufferUsage.None);
                t_index_buffer.SetData<ushort>(t_indices);
            }
            tri = new VertexPositionNormalTexture[3] {
                new VertexPositionNormalTexture(A, -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(B, -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(C, -Vector3.UnitZ, new Vector2(1, 1))
            };

            t_vertex_buffer = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, tri.Length, BufferUsage.None);
            t_vertex_buffer.SetData<VertexPositionNormalTexture>(tri);

            draw_buffers(t_vertex_buffer, t_index_buffer, world, color, view, projection);
        }

        public static void fill_tris_big_buffer(Matrix world, (Vector3 A, Vector3 B, Vector3 C)[] tris, Color color, Matrix view, Matrix projection) {
            if (t_index_buffer == null) {
                t_index_buffer = new IndexBuffer(EngineState.graphics_device, IndexElementSize.SixteenBits, t_indices.Length, BufferUsage.None);
                t_index_buffer.SetData<ushort>(t_indices);
            }


            t_vertex_buffer = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, tri.Length, BufferUsage.None);
            t_vertex_buffer.SetData<VertexPositionNormalTexture>(tri);

            draw_buffers(t_vertex_buffer, t_index_buffer, world, color, view, projection);
        }

        public static void fill_quad(Matrix world, Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color, Matrix view, Matrix projection) {

            if (q_index_buffer == null) {
                q_index_buffer = new IndexBuffer(EngineState.graphics_device, IndexElementSize.SixteenBits, q_indices.Length, BufferUsage.None);
                q_index_buffer.SetData<ushort>(q_indices);
            }
            quad = new VertexPositionNormalTexture[4] {
                new VertexPositionNormalTexture(A, -Vector3.UnitZ, new Vector2(0, 0)),
                new VertexPositionNormalTexture(B, -Vector3.UnitZ, new Vector2(1, 0)),
                new VertexPositionNormalTexture(C, -Vector3.UnitZ, new Vector2(1, 1)),
                new VertexPositionNormalTexture(D, -Vector3.UnitZ, new Vector2(0, 1))
            };
            EngineState.graphics_device.RasterizerState = RasterizerState.CullNone;
            q_vertex_buffer = new VertexBuffer(EngineState.graphics_device, VertexPositionNormalTexture.VertexDeclaration, quad.Length, BufferUsage.None);
            q_vertex_buffer.SetData<VertexPositionNormalTexture>(quad);
            draw_buffers_diffuse_texture(q_vertex_buffer, q_index_buffer, tum, Color.White, world, view, projection);
            //draw_buffers(gd, q_vertex_buffer, q_index_buffer, world, color, view, projection);
        }


        public static void arrow(Vector3 A, Vector3 B, float chevron_distance_percent, Vector3 color, Matrix view, Matrix projection) { arrow(A, B, chevron_distance_percent, Color.FromNonPremultiplied(new Vector4(color, 1.0f)), view, projection); }

        public static void arrow(Vector3 A, Vector3 B, float chevron_distance_percent, Vector4 color, Matrix view, Matrix projection) { arrow(A, B, chevron_distance_percent, Color.FromNonPremultiplied(color), view, projection); }

        public static void arrow(Vector3 A, Vector3 B, float chevron_distance_percent, Color color, Matrix view, Matrix projection) {
            line(A, B, color, view, projection);
            Vector3 BA = (A - B) * chevron_distance_percent;
            
            line_effect.Parameters["View"].SetValue(view);
            line_effect.Parameters["Projection"].SetValue(projection);
            
            VertexPositionColor[] verts = new VertexPositionColor[9];

            verts[0] = new VertexPositionColor(A, color);
            verts[1] = new VertexPositionColor(B, color);
            verts[2] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Up), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[3] = new VertexPositionColor(B, color);
            verts[4] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Down), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[5] = new VertexPositionColor(B, color);
            verts[6] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Left), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            verts[7] = new VertexPositionColor(B, color);
            verts[8] = new VertexPositionColor(B + (Vector3.Cross(Vector3.Cross(BA, Vector3.Right), BA) * chevron_distance_percent) + (BA * chevron_distance_percent), color);

            line_effect.Parameters["tint"].SetValue(color.ToVector3());

            for (int i = 0; i < line_effect.CurrentTechnique.Passes.Count; i++) {
                line_effect.CurrentTechnique.Passes[i].Apply();
                EngineState.graphics_device.DrawUserPrimitives(PrimitiveType.LineStrip, verts, 0, 8);
            }

            line_effect.Parameters["tint"].SetValue(Color.White.ToVector3());
        }

    }
}
