using Magpie.Engine;
using Magpie.Engine.Collision.Support3D;
using Magpie.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//this is based on Vurtun's implementation, found here: https://gist.github.com/vurtun/29727217c269a2fbf4c0ed9a1d11cb40

namespace Magpie {
    public class GJK {
        public static int max_iterations = 50;
        public const float epsilon = 0.0001f;

        public struct support {
            public int vert_ID_A, vert_ID_B;

            public Vector3 A;
            public Vector3 B;

            public Vector3 DA;
            public Vector3 DB;
        }
        
        public struct simplex {
            public int max_iterations;
            public int iterations;
            public int count;
            public bool hit; 

            public float[] bc; //always 4 floats
            public float distance;

            public struct vert {
                public Vector3 A;
                public Vector3 B;

                public Vector3 P;

                public int A_ID, B_ID;
            }

            public vert[] verts; //always 4 verts
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

            public simplex last_simplex;

            public Shape3D shape_A;
            public Shape3D shape_B;
            internal Vector3 last_un_hit_A;
            internal Vector3 last_un_hit_B;

            public override string ToString() {
                return string.Format(
@"##{0} | {1}## 
closest points:
A: {2} B: {3}

",
                    hit, iterations,
                    closest_point_A.simple_vector3_string(),
                    closest_point_B.simple_vector3_string()
                );
            }
        }

        static Vector3 
            A, B, C, D, E, F, 
            T, N;

        static Vector3 
            AB, BA, CB, BC, CA, AC,
            DB, BD, DC, CD, DA, AD,
            DD, B0;

        static float U, V;
        static float AS, BS, CS, PP;
        static float 
            U_AB, V_AB, U_BC, V_BC, U_CA, V_CA,
            U_BD, V_BD, U_DC, V_DC, U_AD, V_AD;

        static float 
            U_ABC, V_ABC, W_ABC, U_ADB, V_ADB, W_ADB,
            U_ACD, V_ACD, W_ACD, U_CBD, V_CBD, W_CBD;

        static float U_ABCD, V_ABCD, W_ABCD, X_ABCD;

        static float denom;
        static float volume;

        public static float box(Vector3 A, Vector3 B, Vector3 C) {
            Vector3 N = Vector3.Cross(B, A);
            return Vector3.Dot(N, C);
        }
        
        public static int gjk(ref support sup, ref simplex s) {
            //drop out if we've passed the max iterations
            if (s.max_iterations > 0 && s.iterations >= max_iterations)
                return 0;

            //just starting this test, set things up
            if (s.count == 0) {
                s = new simplex {
                    iterations = 0,
                    hit = false,
                    count = 0,
                    max_iterations = GJK.max_iterations,
                    distance = float.MaxValue,
                    bc = new float[4],
                    verts = new simplex.vert[4]
                };
            }

            for (int i = 0; i < s.count; ++i) {
                if (sup.vert_ID_A != s.verts[i].A_ID) continue;
                if (sup.vert_ID_B != s.verts[i].B_ID) continue;
                return 0;
            }
            
            s.verts[s.count].A = sup.A;
            s.verts[s.count].B = sup.B;
            s.verts[s.count].P = sup.B - sup.A;

            s.verts[s.count].A_ID = sup.vert_ID_A;
            s.verts[s.count].B_ID = sup.vert_ID_B;

            s.bc[s.count] = 1f;
            
            s.count++;

            switch (s.count) {
                case 1: break;

                case 2:
                    //line simplex

                    A = s.verts[0].P;
                    B = s.verts[1].P;

                    AB = A - B; BA = B - A;

                    U = Vector3.Dot(B, BA);
                    V = Vector3.Dot(A, AB);

                    //voronoi region A
                    if (V <= 0f) {
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }
                    //region B
                    if (U <= 0f) {
                        s.verts[0] = s.verts[1];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    s.bc[0] = U;
                    s.bc[1] = V;

                    s.count = 2;

                    break;

                case 3:
                    A = s.verts[0].P;
                    B = s.verts[1].P;
                    C = s.verts[2].P;
                    
                    AB = A - B;  BA = B - A;
                    BC = B - C;  CB = C - B;
                    CA = C - A;  AC = A - C;

                    U_AB = Vector3.Dot(B, BA);  V_BC = Vector3.Dot(B, BC);
                    V_AB = Vector3.Dot(A, AB);  U_CA = Vector3.Dot(A, AC);
                    U_BC = Vector3.Dot(C, CB);  V_CA = Vector3.Dot(C, CA);
                    
                    //region A
                    if (V_AB <= 0f && U_CA <= 0f) {
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //region B
                    if (U_AB <= 0f && V_BC <= 0f) {
                        s.verts[0] = s.verts[1];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //region C
                    if (U_BC <= 0f && V_CA <= 0f) {
                        s.verts[0] = s.verts[2];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //fractional area calculation
                    N = Vector3.Cross(BA, CA);
                    
                    U_ABC = Vector3.Dot(Vector3.Cross(B,C),N);
                    V_ABC = Vector3.Dot(Vector3.Cross(C,A),N);
                    W_ABC = Vector3.Dot(Vector3.Cross(A,B),N);

                    //region AB
                    if (U_AB > 0f && V_AB > 0f && W_ABC <= 0f) {
                        s.bc[0] = U_AB;
                        s.bc[1] = V_AB;
                        s.count = 2;
                        break;
                    }

                    //region BC
                    if (U_BC > 0f && V_BC > 0f && U_ABC <= 0f) {
                        s.verts[0] = s.verts[1];
                        s.verts[1] = s.verts[2];
                        s.bc[0] = U_BC;
                        s.bc[1] = V_BC;
                        s.count = 2;
                        break;
                    }

                    //region CA
                    if (U_CA > 0f && V_CA > 0f && V_ABC <= 0f) {
                        s.verts[1] = s.verts[0];
                        s.verts[0] = s.verts[2];
                        s.bc[0] = U_CA;
                        s.bc[1] = V_CA;
                        s.count = 2;
                        break;
                    }

                    //region ABC
                    if (U_ABC > 0f && V_ABC > 0f && W_ABC > 0f) {

                        //throw new Exception("what the fuck man");
                        //s.count = 2;
                        //break;
                    } 

                    s.bc[0] = U_ABC;
                    s.bc[1] = V_ABC;
                    s.bc[2] = W_ABC;

                    s.count = 3;
                    break;
                    //}

                case 4:
                    //tetrahedron simplex
                    A = s.verts[0].P;
                    B = s.verts[1].P;
                    C = s.verts[2].P;
                    D = s.verts[3].P;

                    AB = A - B;   BA = B - A;
                    BC = B - C;   CB = C - B;
                    CA = C - A;   AC = A - C;
                    DB = D - B;   BD = B - D;
                    DC = D - C;   CD = C - D;
                    DA = D - A;   AD = A - D;

                    U_AB = Vector3.Dot(B, BA); V_AB = Vector3.Dot(A, AB);
                    U_BC = Vector3.Dot(C, CB); V_BC = Vector3.Dot(B, BC);   
                    U_CA = Vector3.Dot(A, AC); V_CA = Vector3.Dot(C, CA);

                    U_BD = Vector3.Dot(D, DB); V_BD = Vector3.Dot(B, BD);
                    U_DC = Vector3.Dot(C, CD); V_DC = Vector3.Dot(D, DC);
                    U_AD = Vector3.Dot(D, DA); V_AD = Vector3.Dot(A, AD);
                    //region A
                    if (V_AB <= 0f && U_CA <= 0f && V_AD <= 0f) {
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //region B
                    if (U_AB <= 0f && V_BC <= 0f && V_BD <= 0f) {
                        s.verts[0] = s.verts[1];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //region C
                    if (U_BC <= 0f && V_CA <= 0f && U_DC <= 0f) {
                        s.verts[0] = s.verts[2];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //region D
                    if (U_BD <= 0f && V_DC <= 0f && U_AD <= 0f) {
                        s.verts[0] = s.verts[3];
                        s.bc[0] = 1f; s.count = 1;
                        break;
                    }

                    //fractional area calculation
                    N = Vector3.Cross(DA, BA);

                    U_ADB = Vector3.Dot(N, Vector3.Cross(D, B));
                    V_ADB = Vector3.Dot(N, Vector3.Cross(B, A));
                    W_ADB = Vector3.Dot(N, Vector3.Cross(A, D));

                    N = Vector3.Cross(CA, DA);

                    U_ACD = Vector3.Dot(N, Vector3.Cross(C, D));
                    V_ACD = Vector3.Dot(N, Vector3.Cross(D, A));
                    W_ACD = Vector3.Dot(N, Vector3.Cross(A, C));

                    N = Vector3.Cross(BC, DC);

                    U_CBD = Vector3.Dot(N, Vector3.Cross(B, D));
                    V_CBD = Vector3.Dot(N, Vector3.Cross(D, C));
                    W_CBD = Vector3.Dot(N, Vector3.Cross(C, B));

                    N = Vector3.Cross(BA, CA);

                    U_ABC = Vector3.Dot(N, Vector3.Cross(B, C));
                    V_ABC = Vector3.Dot(N, Vector3.Cross(C, A));
                    W_ABC = Vector3.Dot(N, Vector3.Cross(A, B));

                    //test edges

                    //AB
                    if (W_ABC <= 0f && V_ADB <= 0f && U_AB > 0f && V_AB > 0f) {
                        s.bc[0] = U_AB;
                        s.bc[1] = V_AB;
                        s.count = 2;
                        break;
                    }

                    //BC
                    if (U_ABC <= 0f && W_CBD <= 0f && U_BC > 0f && V_BC > 0f) {
                        s.verts[0] = s.verts[1];
                        s.verts[1] = s.verts[2];
                        s.bc[0] = U_BC;
                        s.bc[1] = V_BC;
                        s.count = 2;
                        break;
                    }

                    //CA
                    if (V_ABC <= 0f && W_CBD <= 0f && U_CA > 0f && V_CA > 0f) {
                        s.verts[1] = s.verts[0];
                        s.verts[0] = s.verts[2];
                        s.bc[0] = U_CA;
                        s.bc[1] = V_CA;
                        s.count = 2;
                        break;
                    }

                    //DC
                    if (V_CBD <= 0f && U_ACD <= 0f && U_DC > 0f && V_DC > 0f) {
                        s.verts[0] = s.verts[3];
                        s.verts[1] = s.verts[2];
                        s.bc[0] = U_DC;
                        s.bc[1] = V_DC;
                        s.count = 2;
                        break;
                    }

                    //AD
                    if (V_ACD <= 0f && W_ADB <= 0f && U_AD > 0f && V_AD > 0f) {
                        s.verts[1] = s.verts[3];
                        s.bc[0] = U_AD;
                        s.bc[1] = V_AD;
                        s.count = 2;
                        break;
                    }
                    
                    //BD
                    if (U_CBD <= 0f && U_ADB <= 0f && U_BD > 0f && V_BD > 0f) {
                        s.verts[0] = s.verts[1];
                        s.verts[1] = s.verts[3];
                        s.bc[0] = U_BD;
                        s.bc[1] = V_BD;
                        s.count = 2;
                        break;
                    }

                    //fractional volume calc
                    denom = box(CB, AB, DB);
                    volume = (denom == 0) ? 1f : 1f / denom;

                    U_ABCD = box(C, D, B) * volume;
                    V_ABCD = box(C, A, D) * volume;
                    W_ABCD = box(D, A, B) * volume;
                    X_ABCD = box(B, A, C) * volume;

                    
                    Vector3 PA, PB, PC, PD;
                    PA = s.verts[0].P * (denom * U_ABCD);
                    PB = s.verts[1].P * (denom * V_ABCD);
                    PC = s.verts[2].P * (denom * W_ABCD);
                    PD = s.verts[3].P * (denom * X_ABCD);
                    /*
                    Vector3 point = PA + PB + PC + PD;

                    if (Vector3.Dot(point, point) >= epsilon * epsilon) {
                        s.bc[0] = U_ABCD;
                        s.bc[1] = V_ABCD;
                        s.bc[2] = W_ABCD;
                        s.bc[3] = X_ABCD;
                        s.count = 4;
                        //break;
                        return 1;
                    }*/
                    

                    //ABC
                    if (X_ABCD <= 0f && U_ABC > 0f && V_ABC > 0f && W_ABC > 0f) {
                        s.bc[0] = U_ABC;
                        s.bc[1] = V_ABC;
                        s.bc[2] = W_ABC;
                        s.count = 3;
                        break;
                    }
                    
                    //CBD
                    if (U_ABCD <= 0f && U_CBD > 0f && V_CBD > 0f && W_CBD > 0F) {
                        s.verts[0] = s.verts[2];
                        s.verts[2] = s.verts[3];
                        s.bc[0] = U_CBD;
                        s.bc[1] = V_CBD;
                        s.bc[2] = W_CBD;
                        s.count = 3;
                        break;
                    }

                    //ACD
                    if (V_ABCD <= 0f && U_ACD > 0f && V_ACD > 0f && W_ACD > 0f) {
                        s.verts[1] = s.verts[2];
                        s.verts[2] = s.verts[3];
                        s.bc[0] = U_ACD;
                        s.bc[1] = V_ACD;
                        s.bc[2] = W_ACD;
                        s.count = 3;
                        break;
                    }

                    //ADB
                    if (W_ABCD <= 0f && U_ADB > 0f && V_ADB > 0f && W_ACD > 0f) {
                        s.verts[2] = s.verts[1];
                        s.verts[1] = s.verts[3];
                        s.bc[0] = U_ADB;
                        s.bc[1] = V_ADB;
                        s.bc[2] = W_ADB;
                        s.count = 3;
                        break;
                    }

                    //ABCD
                    if (U_ABCD > 0f && V_ABCD > 0f && W_ABCD > 0f && X_ABCD > 0f) {
                        //throw new Exception("what the fuck man");
                        //s.count =3;
                        //break;
                    }

                    s.bc[0] = U_ABCD;
                    s.bc[1] = V_ABCD;
                    s.bc[2] = W_ABCD;
                    s.bc[3] = X_ABCD;

                    s.count = 4;

                    break;
            }

            //test whether origin is enclosed by tetrahedron
            if (s.count == 4) {
                s.hit = true;
                return 0;
            }

            Vector3 P = Vector3.Zero;
            denom = 0f;

            for (int i = 0; i < s.count; ++i) {
                denom += s.bc[i];                
            }
            denom = 1f / denom;

            switch(s.count) {
                //point
                case 1:
                    P = s.verts[0].P;
                    break;

                //line
                case 2:
                    A = s.verts[0].P * (denom * s.bc[0]);
                    B = s.verts[1].P * (denom * s.bc[1]);
                    P = A + B;
                    break;
                
                //triangle
                case 3:
                    A = s.verts[0].P * (denom * s.bc[0]);
                    B = s.verts[1].P * (denom * s.bc[1]);
                    C = s.verts[2].P * (denom * s.bc[2]);
                    P = A + B + C;
                    break;
            }

            PP = Vector3.Dot(P, P);
            
            if (PP >= s.distance) return 0;
            s.distance = PP;

            //change search direction
            DD = Vector3.Zero;

            switch (s.count) {
                case 1:
                    //point
                    DD = s.verts[0].P * -1;
                    break;
                case 2:
                    //line
                    BA = s.verts[1].P - s.verts[0].P;
                    B0 = s.verts[1].P * -1;
                    T = Vector3.Cross(BA, B0);
                    DD = Vector3.Cross(T, BA);
                    break;
                case 3:
                    //tri
                    AB = s.verts[1].P - s.verts[0].P;
                    AC = s.verts[2].P - s.verts[0].P;
                    N = Vector3.Cross(AB, AC);
                    if (Vector3.Dot(N, s.verts[0].P) <= 0f) {
                        DD = N;
                    } else {
                        DD = N * -1;
                    }
                    break;
            }
            
            if (Vector3.Dot(DD, DD) < epsilon)
                return 0;

            sup.DA = DD * -1f;
            sup.DB = DD;
            
            return 1;
        }

        public static void gjk_check(ref simplex s, ref gjk_result res) {
            //res = new result();
            res.iterations = s.iterations;
            res.hit = s.hit;

            denom = 0f;
            for (int i = 0; i < s.count; ++i) {
                denom += s.bc[i];
            }
            denom = 1f / denom;

            switch (s.count) {
                default: throw new Exception(); 

                //you know the drill by now, point, line, tri, tet
                case 1:
                    res.closest_point_A = s.verts[0].A;
                    res.closest_point_B = s.verts[0].B;
                    break;

                case 2:
                    AS = denom * s.bc[0];
                    BS = denom * s.bc[1];

                    A = s.verts[0].A * AS;
                    B = s.verts[1].A * BS;
                    C = s.verts[0].B * AS;
                    D = s.verts[1].B * BS;

                    res.closest_point_A = A + B;
                    res.closest_point_B = C + D;
                    break;

                case 3:
                    AS = denom * s.bc[0];
                    BS = denom * s.bc[1];
                    CS = denom * s.bc[2];

                    A = s.verts[0].A * AS;
                    B = s.verts[1].A * BS;
                    C = s.verts[2].A * CS;

                    D = s.verts[0].B * AS;
                    E = s.verts[1].B * BS;
                    F = s.verts[2].B * CS;

                    res.closest_point_A = A + B + C;
                    res.closest_point_B = D + E + F;
                    break;

                case 4:
                    A = s.verts[0].A * (denom * s.bc[0]);
                    B = s.verts[1].A * (denom * s.bc[1]);
                    C = s.verts[2].A * (denom * s.bc[2]);
                    D = s.verts[3].A * (denom * s.bc[3]);

                    res.closest_point_A = A + B + C + D;
                    res.closest_point_B = res.closest_point_A;
                    break;
            }

            if (!res.hit) {
                res.distance_squared = Vector3.DistanceSquared(res.closest_point_A, res.closest_point_B);
                res.distance = Vector3.Distance(res.closest_point_A, res.closest_point_B);
            } else {
                res.distance_squared = 0;
                res.distance = 0;
            }
                       
            res.last_simplex = s;
        }

        // you have been visited by the Fast Inverse Square Root of Greg Walsh
        // may all your game dev be happy and may your math be approximate and fast
       public  static float InvSqrt(float x) {
            float xhalf = 0.5f * x;
            int i = BitConverter.ToInt32(BitConverter.GetBytes(x), 0);
            i = 0x5f3759df - (i >> 1);
            x = BitConverter.ToSingle(BitConverter.GetBytes(i), 0);
            x = x * (1.5f - xhalf * x * x);
            return x;
        }
               
        //this is purely for things like spheres and capsules, 
        //it allows you to treat the closest point which is output by the support function as though it has a radius around it,
        //so a sphere is simply a point that has this applied to it, and a capsule is a line        
        public static void gjk_quadratic_distance_solve(float a_radius, float b_radius, ref gjk_result res) {
            float radius = a_radius + b_radius;
            float radius2 = radius * radius;

            if (res.distance_squared > epsilon && res.distance_squared > radius2) {
                
                res.distance_squared -= radius2;

                N = res.closest_point_B - res.closest_point_A;

                float L2 = Vector3.Dot(N, N);
                if (L2 != 0f) {
                    N *= InvSqrt(L2);
                }

                DA = N * a_radius;
                DB = N * b_radius;

                res.closest_point_A += DA;
                res.closest_point_B -= DB;
                
            } else {
                
                Vector3 P = res.closest_point_A + res.closest_point_B;

                res.closest_point_A = P * 0.5f;
                res.closest_point_B = res.closest_point_A;

                res.distance_squared = 0;
                
                res.hit = true;
            }
        }

        static List<gjk_result> results = new List<gjk_result>();
        public static Vector3 cda = Vector3.Zero; public static Vector3 cdb = Vector3.Zero;

        public static gjk_result gjk_intersects(Shape3D shape_A, Shape3D shape_B, Matrix w_a, Matrix w_b, float radius_a = 0f, float radius_b = 0f) {
            if (shape_A == null || shape_B == null) throw new Exception();
            support s = new support();
            simplex si = new simplex();
            gjk_result res = new gjk_result();
            Vector3 sa = Vector3.Zero;
            Vector3 sb = Vector3.Zero;
            results.Clear();

            int t = 0;

            //world matrices
            //Matrix w_a = shape_A.orientation * Matrix.CreateTranslation(shape_A.position);
            //Matrix w_b = shape_B.orientation * Matrix.CreateTranslation(shape_B.position);


            s.A = Vector3.Transform(shape_A.start_point, w_a);
            s.B = Vector3.Transform(shape_B.start_point, w_b);

            res.shape_A = shape_A;
            res.shape_B = shape_B;

            do {
                t = GJK.gjk(ref s, ref si);

                switch (shape_A.shape) {
                    case shape_type.cube:
                        s.vert_ID_A = Supports.Cube(ref sa, Vector3.Transform(s.DA, Matrix.Invert(w_a)), ((Cube)shape_A));
                        break;
                    case shape_type.polyhedron:
                        s.vert_ID_A = Supports.Polyhedron(ref sa, Vector3.Transform(s.DA, Matrix.Invert(w_a)), ((Polyhedron)shape_A).verts.ToArray());
                        break;
                    case shape_type.quad:
                        s.vert_ID_A = Supports.Quad(ref sa, s.DA, ((Quad)shape_A).A, ((Quad)shape_A).B, ((Quad)shape_A).C, ((Quad)shape_A).D, (Quad)shape_A);
                        break;
                    case shape_type.tri:
                        s.vert_ID_A = Supports.Tri(ref sa, Vector3.Transform(s.DA, Matrix.Invert(w_a)), ((Triangle)shape_A).A, ((Triangle)shape_A).B, ((Triangle)shape_A).C);
                        break;
                    case shape_type.capsule:
                        s.vert_ID_A = Supports.Line(ref sa, Vector3.Transform(s.DA, Matrix.Invert(w_a)), ((Capsule)shape_A).A, ((Capsule)shape_A).B);
                        break;
                    case shape_type.line:
                        s.vert_ID_A = Supports.Line(ref sa, s.DA, ((Line3D)shape_A).A, ((Line3D)shape_A).B);
                        break;
                    case shape_type.sphere:
                        s.vert_ID_A = Supports.Point(ref sa, s.DA, ((Sphere)shape_A).P);
                        break;                    
                }

                switch (shape_B.shape) {
                    case shape_type.cube:

                        s.vert_ID_B = Supports.Cube(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w_b)), ((Cube)shape_B) );
                        break;
                    case shape_type.polyhedron:
                        s.vert_ID_B = Supports.Polyhedron(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w_b)), ((Polyhedron)shape_B).verts.ToArray());
                        break;
                    case shape_type.quad:
                        s.vert_ID_B = Supports.Quad(ref sb, s.DB, ((Quad)shape_B).A, ((Quad)shape_B).B, ((Quad)shape_B).C, ((Quad)shape_B).D, (Quad)shape_B);
                        break;
                    case shape_type.tri:
                        s.vert_ID_B = Supports.Tri(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w_b)), ((Triangle)shape_B).A, ((Triangle)shape_B).B, ((Triangle)shape_B).C);
                        break;
                    case shape_type.capsule:
                        s.vert_ID_B = Supports.Line(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w_b)), ((Capsule)shape_B).A, ((Capsule)shape_B).B);
                        break;
                    case shape_type.line:
                        s.vert_ID_B = Supports.Line(ref sb, s.DB, ((Line3D)shape_B).A, ((Line3D)shape_B).B);
                        break;
                    case shape_type.sphere:
                        s.vert_ID_B = Supports.Point(ref sb, s.DB, ((Sphere)shape_B).P);
                        break;
                }
                

                s.A = Vector3.Transform(sa, w_a);
                s.B = Vector3.Transform(sb, w_b);

            } while (t == 1);


            GJK.gjk_check(ref si, ref res);
            
            float a_rad = 0, b_rad = 0;
            switch (shape_A.shape) {
                case shape_type.polyhedron:
                case shape_type.quad:
                case shape_type.tri:
                case shape_type.line:
                    //I sleep
                    break;

                // REAL SHIT?
                case shape_type.capsule:
                    a_rad = ((Capsule)shape_A).radius;
                    break;
                case shape_type.sphere:
                    a_rad = ((Sphere)shape_A).radius;
                    break;
            }

            switch (shape_B.shape) {
                case shape_type.polyhedron:
                case shape_type.quad:
                case shape_type.tri:
                case shape_type.line:
                    break;

                case shape_type.capsule:
                    b_rad = ((Capsule)shape_B).radius;
                    break;
                case shape_type.sphere:
                    b_rad = ((Sphere)shape_B).radius;
                    break;
            }

            if (radius_a > 0f)
                a_rad += radius_a;
            if (radius_b > 0f)
                b_rad += radius_b;

            if ((a_rad != 0 || b_rad != 0))
                GJK.gjk_quadratic_distance_solve(a_rad, b_rad, ref res);

            return res;
        }


        public static bool gjk_raycast(Vector3 start, Vector3 end, Shape3D shape, out gjk_result res) {
            if (shape == null) throw new Exception();

            support s = new support();
            simplex si = new simplex();

            Vector3 sa = Vector3.Zero;
            Vector3 sb = Vector3.Zero;

            results.Clear();
            int t = 0;

            Matrix w = shape.orientation * Matrix.CreateTranslation(shape.position);

            s.A = Vector3.Transform(start, Matrix.Identity);
            s.B = Vector3.Transform(shape.start_point, w);

            float rad = 0;

            do {
                t = GJK.gjk(ref s, ref si);

                s.vert_ID_A = Supports.Line(ref sa, s.DA, start, end);

                switch (shape.shape) {
                    case shape_type.cube:
                        s.vert_ID_B = Supports.Cube(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w)), ((Cube) shape));
                        break;
                    case shape_type.polyhedron:
                        s.vert_ID_B = Supports.Polyhedron(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w)), ((Polyhedron) shape).verts.ToArray());
                        break;
                    case shape_type.quad:
                        s.vert_ID_B = Supports.Quad(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w)), ((Quad) shape).A, ((Quad) shape).B, ((Quad) shape).C, ((Quad) shape).D, (Quad)shape);
                        break;
                    case shape_type.tri:
                        s.vert_ID_B = Supports.Tri(ref sb, s.DB, ((Triangle) shape).A, ((Triangle) shape).B, ((Triangle) shape).C);
                        break;
                    case shape_type.capsule:
                        s.vert_ID_B = Supports.Line(ref sb, Vector3.Transform(s.DB, Matrix.Invert(w)), ((Capsule) shape).A, ((Capsule) shape).B);
                        rad = ((Capsule)shape).radius;
                        break;
                    case shape_type.line:
                        s.vert_ID_B = Supports.Line(ref sb, s.DB, ((Line3D) shape).A, ((Line3D) shape).B);
                        break;
                    case shape_type.sphere:
                        s.vert_ID_B = Supports.Point(ref sb, s.DB, ((Sphere) shape).P);
                        rad = ((Sphere)shape).radius;
                        break;
                }


                s.A = Vector3.Transform(sa, w);
                s.B = Vector3.Transform(sb, w);

            } while (t == 1);

            res = new gjk_result();
            GJK.gjk_check(ref si, ref res);

            if (rad != 0)
              GJK.gjk_quadratic_distance_solve(0, rad, ref res);

            return res.hit;
        }
    }
}