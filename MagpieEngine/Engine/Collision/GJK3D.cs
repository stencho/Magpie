using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this is based on Vurtun's implementation, found here: https://gist.github.com/vurtun/29727217c269a2fbf4c0ed9a1d11cb40

namespace Magpie.Engine.Collision {
    public class GJK3D {
        public static int max_iterations = 50;

        static Vector3
            A, B, C, D, E, F,
            T, N,
            AB, BA, CB, BC, CA, AC,
            DB, BD, DC, CD, DA, AD,
            DD, B0, PA, PB, PC, PD,
            point;
                
        static float AS, BS, CS, PP, denom, volume;

        static Vector2 UV, UV_AB, UV_BC, UV_CA, UV_BD, UV_DC, UV_AD;        
        static Vector3 UVW_ABC, UVW_ADB, UVW_ACD, UVW_CBD;
        static Vector4 UVWX_ABCD;

        public static float box(Vector3 A, Vector3 B, Vector3 C) {
            Vector3 N = Vector3.Cross(B, A);
            return Vector3.Dot(N, C);
        }

        public static bool all_positive(Vector3 v) {
            return (v.X > 0f && v.Y > 0f && v.Z > 0f);
        }
        public static bool all_positive(Vector2 v) {
            return (v.X > 0f && v.Y > 0f);
        }


        public interface shape3D {
            Vector3 find_point_in_direction(Vector3 direction, out int vert_ID);
            void draw();
        }

        public struct gjk_support {
            public int vert_ID_A, vert_ID_B;

            public Vector3 A;
            public Vector3 B;

            public Vector3 DA;
            public Vector3 DB;
        }

        public enum simplex_stage {
            none,
            point,
            line,
            tri,
            tetrahedron
        }

        public struct gjk_simplex {
            public int iterations;
            public int count;
            public bool hit;

            public float[] bc; //always 4 floats
            public float distance;

            public struct gjk_vertex {
                public Vector3 A;
                public Vector3 B;

                public Vector3 P;

                public int A_ID, B_ID;
            }

            public gjk_vertex[] verts; //always 4 sets of verts
        }

        public struct gjk_result {
            public bool hit;
            public int iterations;

            public Vector3 closest_point_A;
            public Vector3 closest_point_B;

            public float distance_squared;
            public float distance;
            public float margin_penetration_distance;

            public bool margin_hit;
            public float pen_margin_A;
            public float pen_margin_B;

            public gjk_simplex last_simplex;

            public shape3D shape_A;
            public shape3D shape_B;
        }
        

        private static int gjk(ref gjk_support support, ref gjk_simplex simplex) {
            if (simplex.iterations >= max_iterations)
                return 0;

            if (simplex.count == 0) {
                simplex = new gjk_simplex {
                    iterations = 0,
                    hit = false,
                    count = 0,
                    distance = float.MaxValue,
                    bc = new float[4],
                    verts = new gjk_simplex.gjk_vertex[4]
                };                
            }

            for (int i = 0; i < simplex.count; i++) {
                if (support.vert_ID_A != simplex.verts[i].A_ID) continue;
                if (support.vert_ID_B != simplex.verts[i].B_ID) continue;
                return 0;
            }

            simplex.verts[simplex.count].A = support.A;
            simplex.verts[simplex.count].B = support.B;
            simplex.verts[simplex.count].P = support.B - support.A;

            simplex.verts[simplex.count].A_ID = support.vert_ID_A;
            simplex.verts[simplex.count].B_ID = support.vert_ID_B;

            simplex.bc[simplex.count] = 1f;

            simplex.count++;

            switch (simplex.count) {
                case 1: break;

                case 2:
                    A = simplex.verts[0].P;
                    B = simplex.verts[1].P;

                    AB = B - A; BA = A - B;

                    //test voronoi regions
                    UV = new Vector2(
                        Vector3.Dot(B, AB),
                        Vector3.Dot(A, BA)
                        );

                    //single point simplex if these are true
                    if (UV.Y <= 0f) {
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }
                    if (UV.X <= 0f) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //line simplex otherwise
                    simplex.bc[0] = UV.X;
                    simplex.bc[1] = UV.Y;

                    simplex.count = 2;

                    break;

                case 3:
                    A = simplex.verts[0].P;
                    B = simplex.verts[1].P;
                    C = simplex.verts[2].P;

                    AB = B - A; BA = A - B;
                    BC = C - B; CB = B - C;
                    CA = A - C; AC = C - A;

                    UV_AB = new Vector2(Vector3.Dot(B, AB), Vector3.Dot(B, BA));
                    UV_BC = new Vector2(Vector3.Dot(C, BC), Vector3.Dot(C, CB));
                    UV_CA = new Vector2(Vector3.Dot(A, CA), Vector3.Dot(C, AC));

                    //region A
                    if (UV_AB.Y <= 0f && UV_CA.X <= 0f) {
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //region B
                    if (UV_AB.X <= 0f && UV_BC.Y <= 0f) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //region C
                    if (UV_BC.X <= 0f && UV_CA.Y <= 0f) {
                        simplex.verts[0] = simplex.verts[2];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }
                    

                    N = Vector3.Cross(AB, AC);

                    UVW_ABC = new Vector3(
                        Vector3.Dot(Vector3.Cross(B, C), N),
                        Vector3.Dot(Vector3.Cross(C, A), N),
                        Vector3.Dot(Vector3.Cross(A, B), N)
                        );

                    //region AB
                    if (UV_AB.X > 0f && UV_AB.Y > 0f && UVW_ABC.Z <= 0f) {
                        simplex.bc[0] = UV_AB.X;
                        simplex.bc[1] = UV_AB.Y;
                        simplex.count = 2;
                        break;
                    }

                    //region BC
                    if (UV_BC.X > 0f && UV_BC.Y > 0f && UVW_ABC.X <= 0f) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.verts[1] = simplex.verts[2];
                        simplex.bc[0] = UV_BC.X;
                        simplex.bc[1] = UV_BC.Y;
                        simplex.count = 2;
                        break;
                    }
                    
                    //region CA
                    if (UV_CA.X > 0f && UV_CA.Y > 0f && UVW_ABC.Y <= 0f) {
                        simplex.verts[1] = simplex.verts[0];
                        simplex.verts[0] = simplex.verts[2];
                        simplex.bc[0] = UV_CA.X;
                        simplex.bc[1] = UV_CA.Y;
                        simplex.count = 2;
                        break;
                    }

                    //this shouldn't happen
                    if (UVW_ABC.X > 0f && UVW_ABC.Y > 0f && UVW_ABC.Z > 0f) {
                        //throw new Exception("All ABC UVW values greater than 0");
                    }

                    simplex.bc[0] = UVW_ABC.X;
                    simplex.bc[1] = UVW_ABC.Y;
                    simplex.bc[2] = UVW_ABC.Z;

                    simplex.count = 3;
                    break;

                case 4:
                    A = simplex.verts[0].P;
                    B = simplex.verts[1].P;
                    C = simplex.verts[2].P;
                    D = simplex.verts[3].P;

                    AB = B - A; BA = A - B;
                    BC = C - B; CB = B - C;
                    CA = A - C; AC = C - A;
                    DB = B - D; BD = D - B;
                    DC = C - D; CD = D - C;
                    DA = A - D; AD = D - A;

                    UV_AB = new Vector2(Vector3.Dot(B, AB), Vector3.Dot(A, BA));
                    UV_BC = new Vector2(Vector3.Dot(C, BC), Vector3.Dot(B, CB));
                    UV_CA = new Vector2(Vector3.Dot(A, CA), Vector3.Dot(C, AC));

                    UV_BD = new Vector2(Vector3.Dot(D, BD), Vector3.Dot(B, DB));
                    UV_DC = new Vector2(Vector3.Dot(C, DC), Vector3.Dot(D, CD));
                    UV_AD = new Vector2(Vector3.Dot(D, AD), Vector3.Dot(A, DA));

                    //region A
                    if (UV_AB.Y <= 0f && UV_CA.X <= 0f && UV_AD.Y <= 0f) {
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //region B
                    if (UV_AB.X <= 0f && UV_BC.Y <= 0f && UV_BD.Y <= 0f) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //region C
                    if (UV_BC.X <= 0f && UV_CA.Y <= 0f && UV_DC.X <= 0f) {
                        simplex.verts[0] = simplex.verts[2];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    //region D
                    if (UV_BD.X <= 0f && UV_DC.Y <= 0f && UV_AD.X <= 0f) {
                        simplex.verts[0] = simplex.verts[3];
                        simplex.bc[0] = 1f;
                        simplex.count = 1;
                        break;
                    }

                    N = Vector3.Cross(AD, AB);
                    UVW_ADB = new Vector3(
                        Vector3.Dot(N, Vector3.Cross(D, B)),
                        Vector3.Dot(N, Vector3.Cross(B, A)),
                        Vector3.Dot(N, Vector3.Cross(A, D)));

                    N = Vector3.Cross(AC, AD);
                    UVW_ACD = new Vector3(
                        Vector3.Dot(N, Vector3.Cross(C, D)),
                        Vector3.Dot(N, Vector3.Cross(D, A)),
                        Vector3.Dot(N, Vector3.Cross(A, C)));

                    N = Vector3.Cross(CB, CD);
                    UVW_CBD = new Vector3(
                        Vector3.Dot(N, Vector3.Cross(B, D)),
                        Vector3.Dot(N, Vector3.Cross(D, C)),
                        Vector3.Dot(N, Vector3.Cross(C, B)));

                    N = Vector3.Cross(AB, AC);
                    UVW_ABC = new Vector3(
                        Vector3.Dot(N, Vector3.Cross(B, C)),
                        Vector3.Dot(N, Vector3.Cross(C, A)),
                        Vector3.Dot(N, Vector3.Cross(A, B)));

                    //test edges

                    //AB
                    if (UVW_ABC.Z <= 0f && UVW_ADB.Y <= 0f && all_positive(UV_AB)) {
                        simplex.bc[0] = UV_AB.X;
                        simplex.bc[1] = UV_AB.Y;
                        simplex.count = 2;
                        break;
                    }

                    //BC
                    if (UVW_ABC.X <= 0f && UVW_CBD.Y <= 0f && all_positive(UV_BC)) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.verts[1] = simplex.verts[2];
                        simplex.bc[0] = UV_BC.X;
                        simplex.bc[1] = UV_BC.Y;
                        simplex.count = 2;
                        break;
                    }

                    //CA
                    if (UVW_ABC.Y <= 0f && UVW_CBD.Z <= 0f && all_positive(UV_CA)) {
                        simplex.verts[1] = simplex.verts[0];
                        simplex.verts[0] = simplex.verts[2];
                        simplex.bc[0] = UV_CA.X;
                        simplex.bc[1] = UV_CA.Y;
                        simplex.count = 2;
                        break;
                    }

                    //DC
                    if (UVW_CBD.Y <= 0f && UVW_ACD.X <= 0f && all_positive(UV_DC)) {
                        simplex.verts[0] = simplex.verts[3];
                        simplex.verts[1] = simplex.verts[2];
                        simplex.bc[0] = UV_DC.X;
                        simplex.bc[1] = UV_DC.Y;
                        simplex.count = 2;
                        break;
                    }

                    //AD
                    if (UVW_ACD.Y <= 0f && UVW_ADB.Z <= 0f && all_positive(UV_AD)) {
                        simplex.verts[1] = simplex.verts[2];
                        simplex.bc[0] = UV_AD.X;
                        simplex.bc[1] = UV_AD.Y;
                        simplex.count = 2;
                        break;
                    }

                    //BD
                    if (UVW_CBD.X <= 0f && UVW_ADB.X <= 0f && all_positive(UV_BD)) {
                        simplex.verts[0] = simplex.verts[1];
                        simplex.verts[1] = simplex.verts[3];
                        simplex.bc[0] = UV_BD.X;
                        simplex.bc[1] = UV_BD.Y;
                        simplex.count = 2;
                        break;
                    }

                    denom = box(BC, BA, BD);
                    volume = (denom == 0) ? 1f : 1f / denom;

                    UVWX_ABCD = new Vector4(
                        box(C, D, B) * volume,
                        box(C, A, D) * volume,
                        box(D, A, B) * volume,
                        box(B, A, C) * volume);

                    PA = simplex.verts[0].P * (denom * UVWX_ABCD.X);
                    PB = simplex.verts[1].P * (denom * UVWX_ABCD.Y);
                    PC = simplex.verts[2].P * (denom * UVWX_ABCD.Z);
                    PD = simplex.verts[3].P * (denom * UVWX_ABCD.W);

                    //is this early exit necessary???
                    //
                    point = PA + PB + PC + PD;
                    
                    if (Vector3.Dot(point,point) >= CollisionHelper.epsilon * CollisionHelper.epsilon) {
                        simplex.bc[0] = UVWX_ABCD.X;
                        simplex.bc[1] = UVWX_ABCD.Y;
                        simplex.bc[2] = UVWX_ABCD.Z;
                        simplex.bc[3] = UVWX_ABCD.W;
                        simplex.count = 4;
                        //return 1;
                        break;
                    }
                    //

                    //ABC
                    if (UVWX_ABCD.W <= 0f && all_positive(UVW_ABC)) {
                        simplex.bc[0] = UVW_ABC.X;
                        simplex.bc[1] = UVW_ABC.Y;
                        simplex.bc[2] = UVW_ABC.Z;
                        simplex.count = 3;
                        break;
                    }

                    //CBD
                    if (UVWX_ABCD.X <= 0f && all_positive(UVW_CBD)) {
                        simplex.verts[0] = simplex.verts[2];
                        simplex.verts[2] = simplex.verts[3];
                        simplex.bc[0] = UVW_CBD.X;
                        simplex.bc[1] = UVW_CBD.Y;
                        simplex.bc[2] = UVW_CBD.Z;
                        simplex.count = 3;
                        break;
                    }

                    //ACD
                    if (UVWX_ABCD.Y <= 0f && all_positive(UVW_ACD)) {
                        simplex.verts[1] = simplex.verts[2];
                        simplex.verts[2] = simplex.verts[3];
                        simplex.bc[0] = UVW_ACD.X;
                        simplex.bc[1] = UVW_ACD.Y;
                        simplex.bc[2] = UVW_ACD.Z;
                        simplex.count = 3;
                        break;
                    }

                    //ADB
                    if (UVWX_ABCD.Z <= 0f && all_positive(UVW_ADB)) {
                        simplex.verts[2] = simplex.verts[1];
                        simplex.verts[1] = simplex.verts[3];
                        simplex.bc[0] = UVW_ADB.X;
                        simplex.bc[1] = UVW_ADB.Y;
                        simplex.bc[2] = UVW_ADB.Z;
                        simplex.count = 3;
                        break;
                    }


                    simplex.bc[0] = UVWX_ABCD.X;
                    simplex.bc[1] = UVWX_ABCD.Y;
                    simplex.bc[2] = UVWX_ABCD.Z;
                    simplex.bc[3] = UVWX_ABCD.W;
                    simplex.count = 4;

                    break;
            }

            if (simplex.count == 4) {
                simplex.hit = true;
                return 0;
            }

            point = Vector3.Zero;

            denom = 0f;

            for (int i = 0; i < simplex.count; i++) {
                denom += simplex.bc[i];
            }
            denom = 1f / denom;

            switch(simplex.count) {
                case 1:
                    point = simplex.verts[0].P;
                    break;

                case 2:
                    A = simplex.verts[0].P * (denom * simplex.bc[0]);
                    B = simplex.verts[1].P * (denom * simplex.bc[1]);
                    point = A + B;
                    break;

                case 3:
                    A = simplex.verts[0].P * (denom * simplex.bc[0]);
                    B = simplex.verts[1].P * (denom * simplex.bc[1]);
                    C = simplex.verts[2].P * (denom * simplex.bc[2]);
                    point = A + B + C;
                    break;
            }

            PP = Vector3.Dot(point, point);

            if (PP >= simplex.distance) return 0;
            simplex.distance = PP;

            DD = Vector3.Zero;

            switch (simplex.count) {
                case 1:
                    DD = simplex.verts[0].P * -1;
                    break;

                case 2:
                    BA = simplex.verts[1].P - simplex.verts[0].P;
                    B0 = simplex.verts[1].P * -1;
                    DD = Vector3.Cross(Vector3.Cross(AB, B0), AB);
                    break;

                case 3:
                    N = Vector3.Cross(simplex.verts[1].P - simplex.verts[0].P, simplex.verts[2].P - simplex.verts[0].P);
                    if (Vector3.Dot(N, simplex.verts[0].P) <= 0f) {
                        DD = N;
                    } else {
                        DD = N * -1;
                    }
                    break;
            }

            if (Vector3.Dot(DD,DD) < CollisionHelper.epsilon)
                return 0;

            support.DA = DD * -1f;
            support.DB = DD;

            return 1;
        }

        private static void gjk_check(ref gjk_simplex simplex, ref gjk_result result) {
            result.iterations = simplex.iterations;
            result.hit = simplex.hit;

            denom = 0f;
            for (int i = 0; i < simplex.count; i++) {
                denom += simplex.bc[i];
            }
            denom = 1f / denom;

            switch (simplex.count) {
                case 1:
                    result.closest_point_A = simplex.verts[0].A;
                    result.closest_point_B = simplex.verts[0].B;
                    break;

                case 2:
                    AS = denom * simplex.bc[0];
                    BS = denom * simplex.bc[1];

                    A = simplex.verts[0].A * AS;
                    B = simplex.verts[1].A * BS;
                    C = simplex.verts[0].B * AS;
                    D = simplex.verts[1].B * BS;

                    result.closest_point_A = A + B;
                    result.closest_point_B = C + D;
                    break;

                case 3:
                    AS = denom * simplex.bc[0];
                    BS = denom * simplex.bc[1];
                    CS = denom * simplex.bc[2];

                    A = simplex.verts[0].A * AS;
                    B = simplex.verts[1].A * BS;
                    C = simplex.verts[2].A * CS;

                    D = simplex.verts[0].B * AS;
                    E = simplex.verts[1].B * BS;
                    F = simplex.verts[2].B * CS;

                    result.closest_point_A = A + B + C;
                    result.closest_point_B = D + E + F;
                    break;

                case 4:
                    A = simplex.verts[0].A * (denom * simplex.bc[0]);
                    B = simplex.verts[1].A * (denom * simplex.bc[1]);
                    C = simplex.verts[2].A * (denom * simplex.bc[2]);
                    D = simplex.verts[3].A * (denom * simplex.bc[3]);

                    result.closest_point_A = A + B + C + D;
                    result.closest_point_B = result.closest_point_A;
                    break;
            }

            if (!result.hit) {
                result.distance_squared = Vector3.DistanceSquared(result.closest_point_A, result.closest_point_B);
                result.distance = Vector3.Distance(result.closest_point_A, result.closest_point_B);
            } else {
                result.distance = 0f;
                result.distance_squared = 0f;
            }

            result.last_simplex = simplex;
        }

        static List<gjk_result> results = new List<gjk_result>();
        public static gjk_result intersects(shape3D shape_A, shape3D shape_B) {
            if (shape_A == null) throw new Exception("Shape A was null!");
            if (shape_B == null) throw new Exception("Shape B was null!");

            gjk_support support = new gjk_support();
            gjk_simplex simplex = new gjk_simplex();
            gjk_result  result = new gjk_result();
            results.Clear();

            int t = 0;

            support.A = shape_A.find_point_in_direction(Vector3.Down, out _);
            support.B = shape_B.find_point_in_direction(Vector3.Down, out _);

            result.shape_A = shape_A;
            result.shape_B = shape_B;

            do {
                t = gjk(ref support, ref simplex);

                

                support.A = shape_A.find_point_in_direction(support.DA, out support.vert_ID_A);
                support.B = shape_B.find_point_in_direction(support.DB, out support.vert_ID_B);

            } while (t == 1);

            gjk_check(ref simplex, ref result);

            return result;
            
        }
    }
}

