
namespace UnityTools.Math
{
    using Unity.Mathematics;
    using UnityEngine;
    using UnityTools.Debuging;
    using static Unity.Mathematics.math;
    public class SVD
    {

        //NOTE
        //WARNING
        //unity float2x2[0][1] will return col0 row 1 value
        //which is different than hlsl float2x2[0][1] will return row0 col1 value
        private const float EPSILON = float.Epsilon;

        private static float2x2 G2(float c, float s)
        {
            return float2x2(c, s, -s, c);
        }

        private static float2 GetGivensConventionalCS(float a, float b)
        {
            float d = a * a + b * b;
            float c = 1;
            float s = 0;
            if (abs(d) > 0)
            {
                float t = rsqrt(d);
                c = a * t;
                s = -b * t;
            }

            return float2(c, s);
        }

        private static float2 GetGivensUnConventionalCS(float a, float b)
        {

            float d = a * a + b * b;
            float c = 0;
            float s = 1;
            if (abs(d) > 0)
            {
                float t = rsqrt(d);
                s = a * t;
                c = b * t;
            }

            return float2(c, s);
        }

        private static float3x3 G3_12(float c, float s, bool use_conventional = true)
        {
            float2 cs = use_conventional ? GetGivensConventionalCS(c, s) : GetGivensUnConventionalCS(c, s);
            c = cs.x;
            s = cs.y;
            return float3x3(c, s, 0,
                -s, c, 0,
                0, 0, 1);
        }

        private static float3x3 G3_12_Direct(float c, float s)
        {
            return float3x3(c, s, 0,
                -s, c, 0,
                0, 0, 1);
        }


        private static float3x3 G3_23(float c, float s, bool use_conventional = true)
        {
            float2 cs = use_conventional ? GetGivensConventionalCS(c, s) : GetGivensUnConventionalCS(c, s);
            c = cs.x;
            s = cs.y;
            return float3x3(1, 0, 0,
                0, c, s,
                0, -s, c);
        }
        private static float3x3 G3_23_Direct(float c, float s)
        {
            return float3x3(1, 0, 0,
                0, c, s,
                0, -s, c);
        }


        private static float3x3 G3_13(float c, float s, bool use_conventional = true)
        {
            float2 cs = use_conventional ? GetGivensConventionalCS(c, s) : GetGivensUnConventionalCS(c, s);
            c = cs.x;
            s = cs.y;
            return float3x3(c, 0, s,
                0, 1, 0,
                -s, 0, c);
        }

        private static float3x3 G3_13_Direct(float c, float s)
        {
            return float3x3(c, 0, s,
                0, 1, 0,
                -s, 0, c);
        }

        public static void GetPolarDecomposition2D(float2x2 A, out float2x2 R, out float2x2 S)
        {
            R = float2x2(0, 0, 0, 0);
            S = float2x2(0, 0, 0, 0);

            float x = A[0][0] + A[1][1];
            float y = A[0][1] - A[1][0];

            float d = sqrt(x * x + y * y);

            float c = 1;
            float s = 0;

            R = G2(c, s);

            if (abs(d) > EPSILON)
            {
                d = 1.0f / d;
                R = G2(x * d, -y * d);
            }

            S = mul(transpose(R), A);
        }


        public static void GetSVD2D(float2x2 A, out float2x2 U, out float2 D, out float2x2 V)
        {
            U = float2x2(0, 0, 0, 0);
            D = float2(0, 0);
            V = float2x2(0, 0, 0, 0);

            float2x2 R = float2x2(0, 0, 0, 0);
            float2x2 S = float2x2(0, 0, 0, 0);

            GetPolarDecomposition2D(A, out R, out S);

            float c = 1f;
            float s = 0f;

            if (abs(S[1][0]) < EPSILON)
            {
                D[0] = S[0][0];
                D[1] = S[1][1];
            }
            else
            {
                float taw = 0.5f * (S[0][0] - S[1][1]);
                float w = sqrt(taw * taw + S[1][0] * S[1][0]);
                float t = taw > 0 ? S[1][0] / (taw + w) : S[1][0] / (taw - w);

                c = rsqrt(t * t + 1f);
                s = -t * c;

                D[0] = c * c * S[0][0] - 2f * c * s * S[1][0] + s * s * S[1][1];
                D[1] = s * s * S[0][0] + 2f * c * s * S[1][0] + c * c * S[1][1];

            }

            if (D[0] < D[1])
            {
                float temp = D[0];
                D[0] = D[1];
                D[1] = temp;

                V = G2(-s, c);
            }
            else
            {
                V = G2(c, s);
            }

            U = mul(R, V);
        }

        private static void CodeZerochasing(ref float3x3 U, ref float3x3 A, ref float3x3 V)
        {
            float3x3 G = G3_12(A[0][0], A[0][1]);
            A = mul(transpose(G), A);
            U = mul(U, G);
            //checked

            float c = A[1][0];
            float s = A[2][0];
            if (abs(A[0][1]) > EPSILON)
            {
                c = A[0][0] * A[1][0] + A[0][1] * A[1][1];
                s = A[0][0] * A[2][0] + A[0][1] * A[2][1];
            }

            G = G3_23(c, s);
            A = mul(A, G);
            V = mul(V, G);
            //checked;

            G = G3_23(A[1][1], A[1][2]);
            A = mul(transpose(G), A);
            U = mul(U, G);
            //checked
        }

        private static void Zerochasing(ref float3x3 U, ref float3x3 A, ref float3x3 V)
        {
            float3x3 G = G3_23(A[1][0], A[2][0]);
            A = mul(A, G);
            U = mul(transpose(G), U);

            G = G3_23(A[1][0], A[2][0]);
            A = mul(transpose(G), A);
            V = mul(transpose(G), V);

            G = G3_23(A[1][1], A[1][2]);
            A = mul(transpose(G), A);
            U = mul(U, G);
        }

        private static void Bidiagonalize(ref float3x3 U, ref float3x3 A, ref float3x3 V)
        {
            float3x3 G = G3_23(A[0][1], A[0][2]);
            A = mul(transpose(G), A);
            U = mul(U, G);
            //checked

            CodeZerochasing(ref U, ref A, ref V);
        }

        private static float FrobeniusNorm(float3x3 input)
        {
            float ret = 0;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    ret += input[i][j] * input[i][j];
                }
            }

            return sqrt(ret);
        }

        private static void FlipSign(int index, ref float3x3 mat, ref float3 sigma)
        {
            mat[index] = -mat[index];
            sigma[index] = -sigma[index];
        }

        private static void FlipSignColumn(ref float3x3 mat, int col)
        {
            mat[col] = -mat[col];
        }

        private static void SwapFloat(ref float3 v, int a, int b)
        {
            float temp = v[a];
            v[a] = v[b];
            v[b] = temp;
        }


        private static void Swap(ref float3 a, ref float3 b)
        {
            float3 temp = a;
            a = b;
            b = temp;
        }

        private static void SwapColumn(ref float3x3 a, int col_a, int col_b)
        {
            float3 temp = a[col_a];
            a[col_a] = a[col_b];
            a[col_b] = temp;
        }

        private static void SortWithTopLeftSub(ref float3x3 U, ref float3 sigma, ref float3x3 V)
        {
            if (abs(sigma[1]) >= abs(sigma[2]))
            {
                if (sigma[1] < 0)
                {
                    FlipSign(1, ref U, ref sigma);
                    FlipSign(2, ref U, ref sigma);
                }
                return;
            }
            if (sigma[2] < 0)
            {
                FlipSign(1, ref U, ref sigma);
                FlipSign(2, ref U, ref sigma);
            }
            SwapFloat(ref sigma, 1, 2);
            SwapColumn(ref U, 1, 2);
            SwapColumn(ref V, 1, 2);

            if (sigma[1] > sigma[0])
            {
                SwapFloat(ref sigma, 0, 1);
                SwapColumn(ref U, 0, 1);
                SwapColumn(ref V, 0, 1);
            }
            else
            {
                FlipSignColumn(ref U, 2);
                FlipSignColumn(ref V, 2);
            }
        }

        private static void SortWithBotRightSub(ref float3x3 U, ref float3 sigma, ref float3x3 V)
        {
            if (abs(sigma[0]) >= abs(sigma[1]))
            {
                if (sigma[0] < 0)
                {
                    FlipSign(0, ref U, ref sigma);
                    FlipSign(2, ref U, ref sigma);
                }
                return;
            }
            SwapFloat(ref sigma, 0, 1);
            SwapColumn(ref U, 0, 1);
            SwapColumn(ref V, 0, 1);

            if (abs(sigma[1]) < abs(sigma[2]))
            {
                SwapFloat(ref sigma, 1, 2);
                SwapColumn(ref U, 1, 2);
                SwapColumn(ref V, 1, 2);
            }
            else
            {
                FlipSignColumn(ref U, 2);
                FlipSignColumn(ref V, 2);
            }

            if (sigma[1] < 0)
            {
                FlipSign(1, ref U, ref sigma);
                FlipSign(2, ref U, ref sigma);
            }
        }

        private static void SolveReducedTopLeft(ref float3x3 B, ref float3x3 U, ref float3 sigma, ref float3x3 V)
        {
            float s3 = B[2][2];
            //float2x2 u = G2(1, 0);
            //float2x2 v = G2(1, 0);

            float2x2 top_left = float2x2(B[0][0], B[1][0], B[0][1], B[1][1]);

            float2x2 A2 = top_left;
            float2x2 U2 = float2x2(0, 0, 0, 0);
            float2 D2 = float2(0, 0);
            float2x2 V2 = float2x2(0, 0, 0, 0);
            GetSVD2D(A2, out U2, out D2, out V2);

            float3x3 u3 = G3_12_Direct(U2[0][0], U2[1][0]);
            float3x3 v3 = G3_12_Direct(V2[0][0], V2[1][0]);

            U = mul(U, u3);
            V = mul(V, v3);

            sigma = float3(D2, s3);
        }


        private static void SolveReducedBotRight(ref float3x3 B, ref float3x3 U, ref float3 sigma, ref float3x3 V)
        {
            float s1 = B[0][0];
            //float2x2 u = G2(1, 0);
            //float2x2 v = G2(1, 0);

            float2x2 bot_right = float2x2(B[1][1], B[2][1], B[1][2], B[2][2]);

            float2x2 A2 = bot_right;
            float2x2 U2 = float2x2(0, 0, 0, 0);
            float2 D2 = float2(0, 0);
            float2x2 V2 = float2x2(0, 0, 0, 0);
            GetSVD2D(A2, out U2, out D2, out V2);

            float3x3 u3 = G3_23_Direct(U2[0][0], U2[1][0]);
            float3x3 v3 = G3_23_Direct(V2[0][0], V2[1][0]);

            U = mul(U, u3);
            V = mul(V, v3);
            sigma = float3(s1, D2);
        }

        private static void PostProcess(float3x3 B, ref float3x3 U, ref float3x3 V, float3 alpha, float2 beta, ref float3 sigma, float tao)
        {
            if (abs(beta[1]) <= tao)
            {
                SolveReducedTopLeft(ref B, ref U, ref sigma, ref V);
                //checked
                SortWithTopLeftSub(ref U, ref sigma, ref V);
                //checked
            }
            else if (abs(beta[0]) <= tao)
            {
                SolveReducedBotRight(ref B, ref U, ref sigma, ref V);
                SortWithBotRightSub(ref U, ref sigma, ref V);
                //checked once
            }
            else if (abs(alpha[1]) <= tao)
            {
                //UnConventional G here
                float3x3 G = G3_23(B[2][1], B[2][2], false);
                B = mul(transpose(G), B);
                U = mul(U, G);

                SolveReducedTopLeft(ref B, ref U, ref sigma, ref V);
                SortWithTopLeftSub(ref U, ref sigma, ref V);
            }
            else if (abs(alpha[2]) <= tao)
            {
                float3x3 G = G3_23(B[1][1], B[2][1]);
                B = mul(B, G);
                V = mul(V, G);

                G = G3_13(B[0][0], B[2][0]);
                B = mul(B, G);
                V = mul(V, G);

                //checked
                SolveReducedTopLeft(ref B, ref U, ref sigma, ref V);
                //checked
                SortWithTopLeftSub(ref U, ref sigma, ref V);
                //checked
            }
            else if (abs(alpha[0]) <= tao)
            {
                //UnConventional G here
                float3x3 G = G3_12(B[1][0], B[1][1], false);
                B = mul(transpose(G), B);
                U = mul(U, G);

                //UnConventional G here
                G = G3_13(B[2][0], B[2][2], false);
                B = mul(transpose(G), B);
                U = mul(U, G);

                SolveReducedBotRight(ref B, ref U, ref sigma, ref V);
                SortWithBotRightSub(ref U, ref sigma, ref V);
            }
        }


        public static void GetSVD3D(float3x3 A, out float3x3 U, out float3 D, out float3x3 V)
        {
            UnityEngine.Assertions.Assert.IsTrue(false, "Not tested");
            U = float3x3(1, 0, 0,
                         0, 1, 0,
                         0, 0, 1);
            D = float3(0, 0, 0);
            V = float3x3(1, 0, 0,
                         0, 1, 0,
                         0, 0, 1);

            float3x3 B = A;

            Bidiagonalize(ref U, ref B, ref V);
            //chekced

            float3 alpha = float3(B[0][0], B[1][1], B[2][2]);
            float2 beta = float2(B[1][0], B[2][1]);
            float2 gamma = float2(alpha[0] * beta[0], alpha[1] * beta[1]);

            float tol = 128 * EPSILON;
            float tao = tol * max(0.5f * FrobeniusNorm(B), 1.0f);

            int count = 0;

            while (abs(beta[1]) > tao && abs(beta[0]) > tao &&
                abs(alpha[0]) > tao && abs(alpha[1]) > tao && abs(alpha[2]) > tao)
            {
                float a1 = alpha[1] * alpha[1] + beta[0] * beta[0];
                float a2 = alpha[2] * alpha[2] + beta[1] * beta[1];
                float b1 = gamma[1];


                float d = (a1 - a2) * 0.5f;
                float mu = (b1 * b1) / (abs(d) + sqrt(d * d + b1 * b1));
                //copy sign from d to mu
                float d_sign = sign(d);
                mu = d_sign > 0 ? mu : -mu;


                //code not in the paper
                mu = a2 - mu;
                //----------------

                float3x3 G = G3_12((alpha[0] * alpha[0]) - mu, gamma[0]);
                B = mul(B, G);
                V = mul(V, G);

                CodeZerochasing(ref U, ref B, ref V);

                alpha = float3(B[0][0], B[1][1], B[2][2]);
                beta = float2(B[1][0], B[2][1]);
                gamma = float2(alpha[0] * beta[0], alpha[1] * beta[1]);

                count++;

            }

            PostProcess(B, ref U, ref V, alpha, beta, ref D, tao);
        }
    }


    public class SVDTest : Test.ITest
    {
        public string Name => this.ToString();

        public void Report()
        {
        }

        public void RunTest()
        {
            var input = new float2x2(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
            var input3x3 = new float3x3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value,
                                        UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value
                                        , UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);


            var U = new float2x2();
            var d = new float2();
            var V = new float2x2();

            SVD.GetSVD2D(input, out U, out d, out V);
            var Vt = math.transpose(V);

            var D = float2x2(d.x, 0, 0, d.y);
            var output = mul(mul(U, D), Vt);
            var delta = input - output;
            LogTool.Log(delta.ToString());

            var U3 = new float3x3();
            var d3 = new float3();
            var V3 = new float3x3();
            
            SVD.GetSVD3D(input3x3, out U3, out d3, out V3);
            var Vt3 = math.transpose(V3);
            var D3 = float3x3(d3[0], 0, 0,
                              0, d3[1], 0,
                              0, 0, d3[2]);
            var output3 = mul(mul(U3, D3), Vt3);
            var delta3 = input3x3-output3;
            LogTool.Log(delta3.ToString());

            // var test = new float2x2(1,2,3,4);
            // Debug.Log(test[0][1]);


            // var ma = new Matrix4x4();
            // ma.m00 = input[0][0];
            // ma.m01 = input[0][1];
            // ma.m10 = input[1][0];
            // ma.m11 = input[1][1];

            // var mu = new Matrix4x4();
            // mu.m00 = U[0][0];
            // mu.m01 = U[0][1];
            // mu.m10 = U[1][0];
            // mu.m11 = U[1][1];


            // var md = new Matrix4x4();
            // md.m00 = d.x;
            // md.m11 = d.y;

            // var mvt = new Matrix4x4();
            // mvt.m00 = Vt[0][0];
            // mvt.m01 = Vt[0][1];
            // mvt.m10 = Vt[1][0];
            // mvt.m11 = Vt[1][1];

            // Matrix4x4 delta = mu * md * mvt;
            // delta.SetRow(0, ma.GetRow(0) - delta.GetRow(0));
            // delta.SetRow(1, ma.GetRow(1) - delta.GetRow(1));
            // delta.SetRow(2, ma.GetRow(2) - delta.GetRow(2));
            // delta.SetRow(3, ma.GetRow(3) - delta.GetRow(3));

            // Debug.Log(delta.ToString());


            var R = new float2x2();
            var S = new float2x2();
            SVD.GetPolarDecomposition2D(input, out R, out S);
            LogTool.Log((input - mul(R, S)).ToString());
            // LogTool.AssertIsTrue(math.length(delta) < 0.0001f);
        }
    }

}