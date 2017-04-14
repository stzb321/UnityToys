using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MiniRasterize
{
    class MiniRasterize
    {
        struct Vector4
        {
            public float x, y, z, w;

            public static Vector4 Zero
            {
                get
                {
                    return new Vector4(0, 0, 0, 0);
                }
            }

            public Vector4(float x, float y, float z, float w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public static bool operator ==(Vector4 a, Vector4 b)
            {
                return a.x.Equals(b.x) && a.y.Equals(b.y) && a.z.Equals(b.z) && a.w.Equals(b.w);
            }

            public static bool operator !=(Vector4 a, Vector4 b)
            {
                return !a.x.Equals(b.x) || !a.y.Equals(b.y) || !a.z.Equals(b.z) || !a.w.Equals(b.w);
            }

            public static Vector4 operator +(Vector4 a, Vector4 b)
            {
                return new Vector4() {x = a.x + b.x, y = a.y + b.y, w = a.w + b.w, z = a.z + b.z };
            }

            public static Vector4 operator -(Vector4 a)
            {
                return new Vector4() { x = -a.x, y = -a.y, w = -a.w, z = -a.z };
            }

            public static Vector4 operator -(Vector4 a, Vector4 b)
            {
                return new Vector4() { x = a.x - b.x, y = a.y - b.y, w = a.w - b.w, z = a.z - b.z };
            }

            public static Vector4 operator *(Vector4 a, Vector4 b)
            {
                return new Vector4() { x = a.x * b.x, y = a.y * b.y, w = a.w * b.w, z = a.z * b.z };
            }

            public static Vector4 operator *(Vector4 a, float b)
            {
                return new Vector4() { x = a.x * b, y = a.y * b, w = a.w * b, z = a.z * b };
            }

            public static Vector4 Cross(Vector4 a, Vector4 b)
            {
                return new Vector4() {x = a.y * b.z - a.z * b.y, y = a.z * b.x - a.x * b.z, z = a.x * b.y - a.y * b.x };
            }

            public static float Dot(Vector4 a, Vector4 b)
            {
                return a.x * b.x + a.y * b.y + a.z * b.z;
            }

            public Vector4 Normalize()
            {
                var invlen = 1.0f / (float)Math.Sqrt(x * x + y * y + z * z);
                return new Vector4() {x = x * invlen, y = y * invlen, z = z * invlen, w = w * invlen};
            }
        }

        struct Matrix4
        {
            public float m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33;

            public float this[int row, int column]   //TODO: 为了能像数组那样调用，这里用了反射，是否有更好的不使用反射的方法？
            {
                get{
                    if (row < 0 || row >= 4 || column < 0 || column >= 4)
                    {
                        throw new Exception("out of matrix range");
                    }
                    FieldInfo field = this.GetType().GetField(string.Format("m{0}{1}", row, column));
                    return (float)field.GetValue(this);
                }
                set
                {
                    if (row < 0 || row >= 4 || column < 0 || column >= 4)
                    {
                        throw new Exception("out of matrix range");
                    }
                    FieldInfo field = this.GetType().GetField(string.Format("m{0}{1}", row, column));
                    field.SetValue(this, value);
                }
            }

            public static Matrix4 operator *(Matrix4 a, Matrix4 b)
            {
                Matrix4 mat = Matrix4.identity;
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        mat[i, j] = a[i, 0] * b[0, j] + a[i, 1] * b[1, j] + a[i, 2] * b[2, j] + a[i, 3] * b[3, j];
                    }
                }
                return mat;
            }

            public static Matrix4 Zero
            {
                get
                {
                    Matrix4 mat = Matrix4.identity;
                    mat.m00 = mat.m01 = mat.m02 = mat.m03 = mat.m10 = mat.m11 = mat.m12 = mat.m13 = mat.m20 = mat.m21 = mat.m22 = mat.m23 = mat.m30 = mat.m31 = mat.m32 = mat.m33 = 0;
                    return mat;
                }
            }

            private static readonly Matrix4 _identity = new Matrix4
            (
                1f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f,
                0f, 0f, 1f, 0f,
                0f, 0f, 0f, 1f
            );

            public static Matrix4 identity
            {
                get { return _identity; }
            }

            public Matrix4(float m00, float m01, float m02, float m03,
                float m10, float m11, float m12, float m13,
                float m20, float m21, float m22, float m23,
                float m30, float m31, float m32, float m33)
            {
                this.m00 = m00;
                this.m01 = m01;
                this.m02 = m02;
                this.m03 = m03;
                this.m10 = m10;
                this.m11 = m11;
                this.m12 = m12;
                this.m13 = m13;
                this.m20 = m20;
                this.m21 = m21;
                this.m22 = m22;
                this.m23 = m23;
                this.m30 = m30;
                this.m31 = m31;
                this.m32 = m32;
                this.m33 = m33;
            }

            public void Invert()
            {
                Matrix4 temm = this;
                float[] tmp = new float[12];
                tmp[0] = m22 * m33;
                tmp[1] = m32 * m23;
                tmp[2] = m12 * m33;
                tmp[3] = m32 * m13;
                tmp[4] = m12 * m23;
                tmp[5] = m22 * m13;
                tmp[6] = m02 * m33;
                tmp[7] = m32 * m03;
                tmp[8] = m02 * m23;
                tmp[9] = m22 * m03;
                tmp[10] = m02 * m13;
                tmp[11] = m12 * m03;

                m00 = tmp[0] * temm.m11 + tmp[3] * temm.m21 + tmp[4] * temm.m31;
                m00 -= tmp[1] * temm.m11 + tmp[2] * temm.m21 + tmp[5] * temm.m31;
                m01 = tmp[1] * temm.m01 + tmp[6] * temm.m21 + tmp[9] * temm.m31;
                m01 -= tmp[0] * temm.m01 + tmp[7] * temm.m21 + tmp[8] * temm.m31;
                m02 = tmp[2] * temm.m01 + tmp[7] * temm.m11 + tmp[10] * temm.m31;
                m02 -= tmp[3] * temm.m01 + tmp[6] * temm.m11 + tmp[11] * temm.m31;
                m03 = tmp[5] * temm.m01 + tmp[8] * temm.m11 + tmp[11] * temm.m21;
                m03 -= tmp[4] * temm.m01 + tmp[9] * temm.m11 + tmp[10] * temm.m21;
                m10 = tmp[1] * temm.m10 + tmp[2] * temm.m20 + tmp[5] * temm.m30;
                m10 -= tmp[0] * temm.m10 + tmp[3] * temm.m20 + tmp[4] * temm.m30;
                m11 = tmp[0] * temm.m00 + tmp[7] * temm.m20 + tmp[8] * temm.m30;
                m11 -= tmp[1] * temm.m00 + tmp[6] * temm.m20 + tmp[9] * temm.m30;
                m12 = tmp[3] * temm.m00 + tmp[6] * temm.m10 + tmp[11] * temm.m30;
                m12 -= tmp[2] * temm.m00 + tmp[7] * temm.m10 + tmp[10] * temm.m30;
                m13 = tmp[4] * temm.m00 + tmp[9] * temm.m10 + tmp[10] * temm.m20;
                m13 -= tmp[5] * temm.m00 + tmp[8] * temm.m10 + tmp[11] * temm.m20;

                tmp[0] = temm.m20 * temm.m31;
                tmp[1] = temm.m30 * temm.m21;
                tmp[2] = temm.m10 * temm.m31;
                tmp[3] = temm.m30 * temm.m11;
                tmp[4] = temm.m10 * temm.m21;
                tmp[5] = temm.m20 * temm.m11;
                tmp[6] = temm.m00 * temm.m31;
                tmp[7] = temm.m30 * temm.m01;
                tmp[8] = temm.m00 * temm.m21;
                tmp[9] = temm.m20 * temm.m01;
                tmp[10] = temm.m00 * temm.m11;
                tmp[11] = temm.m10 * temm.m01;

                m20 = tmp[0] * temm.m13 + tmp[3] * temm.m23 + tmp[4] * temm.m33;
                m20 -= tmp[1] * temm.m13 + tmp[2] * temm.m23 + tmp[5] * temm.m33;
                m21 = tmp[1] * temm.m03 + tmp[6] * temm.m23 + tmp[9] * temm.m33;
                m21 -= tmp[0] * temm.m03 + tmp[7] * temm.m23 + tmp[8] * temm.m33;
                m22 = tmp[2] * temm.m03 + tmp[7] * temm.m13 + tmp[10] * temm.m33;
                m22 -= tmp[3] * temm.m03 + tmp[6] * temm.m13 + tmp[11] * temm.m33;
                m23 = tmp[5] * temm.m03 + tmp[8] * temm.m13 + tmp[11] * temm.m23;
                m23 -= tmp[4] * temm.m03 + tmp[9] * temm.m13 + tmp[10] * temm.m23;
                m30 = tmp[2] * temm.m22 + tmp[5] * temm.m32 + tmp[1] * temm.m12;
                m30 -= tmp[4] * temm.m32 + tmp[0] * temm.m12 + tmp[3] * temm.m22;
                m31 = tmp[8] * temm.m32 + tmp[0] * temm.m02 + tmp[7] * temm.m22;
                m31 -= tmp[6] * temm.m22 + tmp[9] * temm.m32 + tmp[1] * temm.m02;
                m32 = tmp[6] * temm.m12 + tmp[11] * temm.m32 + tmp[3] * temm.m02;
                m32 -= tmp[10] * temm.m32 + tmp[2] * temm.m02 + tmp[7] * temm.m12;
                m33 = tmp[10] * temm.m22 + tmp[4] * temm.m02 + tmp[9] * temm.m12;
                m33 -= tmp[8] * temm.m12 + tmp[11] * temm.m22 + tmp[5] * temm.m02;
                float idet = 1.0f / (temm.m00 * m00 + temm.m10 * m01 + temm.m20 * m02 + temm.m30 * m03);
                m00 *= idet; m01 *= idet; m02 *= idet; m03 *= idet;
                m10 *= idet; m11 *= idet; m12 *= idet; m13 *= idet;
                m20 *= idet; m21 *= idet; m22 *= idet; m23 *= idet;
                m30 *= idet; m31 *= idet; m32 *= idet; m33 *= idet;
            }

            public Matrix4 InvertTranspose()
            {
                Matrix4 mat = Matrix4.Zero;
                mat.Invert();
                mat.m01 = m10; mat.m02 = m20; mat.m03 = m30;
                mat.m10 = m01; mat.m12 = m21; mat.m13 = m31;
                mat.m20 = m02; mat.m21 = m12; mat.m23 = m32;
                mat.m30 = m03; mat.m31 = m13; mat.m32 = m23;
                return mat;
            }
        }

        static Vector4 TransformPoint(Vector4 point, Matrix4 mat)
        {
            Vector4 p = Vector4.Zero;
            p.w = mat.m03 * point.x + mat.m13 * point.y + mat.m23 * point.z + mat.m30;  // 这里有疑问，为什么最后不用 *point.w ，是因为point.w默认是1？
            p.x = (mat.m00 * point.x + mat.m10 * point.y + mat.m20 * point.z ) / p.w;
            p.y = (mat.m01 * point.x + mat.m11 * point.y + mat.m21 * point.z) / p.w;
            p.z = (mat.m02 * point.x + mat.m12 * point.y + mat.m22 * point.z) / p.w;
            return p;
        }

        static Vector4 TransformDir(Vector4 dir, Matrix4 mat)
        {
            Vector4 d = Vector4.Zero;
            d.x = mat.m00 * dir.x + mat.m10 * dir.y + mat.m20 * dir.z;
            d.y = mat.m01 * dir.x + mat.m11 * dir.y + mat.m21 * dir.z;
            d.z = mat.m02 * dir.x + mat.m12 * dir.y + mat.m22 * dir.z;
            return d;
        }

        static Matrix4 CreateModelMatrix(Vector4 v)
        {
            Matrix4 mat = Matrix4.identity;
            mat.m30 = v.x;
            mat.m31 = v.y;
            mat.m32 = v.z;
            return mat;
        }

        // look : camera pos
        // at: camera view dirction
        static Matrix4 CreateViewMatrix(Vector4 look, Vector4 at, Vector4 up)
        {
            Vector4 zaxis = (look - at).Normalize();
            Vector4 xaxis = Vector4.Cross(up, zaxis).Normalize();
            Vector4 yaxis = Vector4.Cross(zaxis, xaxis);
            Matrix4 mat = Matrix4.identity;
            mat.m00 = xaxis.x; mat.m01 = xaxis.y; mat.m02 = xaxis.z; mat.m03 = 0.0f;
            mat.m10 = yaxis.x; mat.m11 = yaxis.y; mat.m12 = yaxis.z; mat.m13 = 0.0f;
            mat.m20 = zaxis.x; mat.m21 = zaxis.y; mat.m22 = zaxis.z; mat.m23 = 0.0f;
            mat.m30 = look.x; mat.m31 = look.y; mat.m32 = look.z; mat.m33 = 1.0f;
            mat.Invert();
            return mat;
        } // TODO： 要清楚视图矩阵的推导

        static Matrix4 CreateProjectionMatrix(float hfov, float ratio, float n, float f)
        {
            float r = n * (float)Math.Tan(hfov * 0.5f), l = -r, b = -r / ratio, t = r / ratio;
            Matrix4 mat;
            mat.m00 = 2 * n / (r - l); mat.m01 = 0.0f; mat.m02 = 0.0f; mat.m03 = 0.0f;
            mat.m10 = 0.0f; mat.m11 = 2 * n / (t - b); mat.m12 = 0.0f; mat.m13 = 0.0f;
            mat.m20 = (r + l) / (r - l); mat.m21 = (t + b) / (t - b); mat.m22 = -(f + n) / (f - n); mat.m23 = -1.0f;
            mat.m30 = 0.0f; mat.m31 = 0.0f; mat.m32 = (-2.0f * f * n) / (f - n); mat.m33 = 0.0f;
            return mat;
        }// TODO： 要清楚投影矩阵的推导

        struct Vertex
        {
            public Vector4 pos, uv, normal, viewPos, color;
        }

        struct Index
        {
            public int[] pos, uv, normal;

            public static Index Zero
            {
                get
                {
                    return new Index(0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
            }

            private Index( int pos1, int pos2, int pos3, int uv1, int uv2, int uv3, int n1, int n2, int n3 )
            {
                pos = new int[] { pos1, pos2, pos3 };
                uv = new int[] { uv1, uv2, uv3 };
                normal = new int[] { n1, n2, n3 };
            }
        }

        struct Light
        {
            Vector4 pos, viewPos, ambientColor, diffuseColor, specularColor;
        }

        struct Model
        {
            public List<Vector4> posBuffer, uvBuffer, normalBuffer;
            public List<Index> indexBuffer;
            public Matrix4 WorldMatrix;

            public Model(string objName, Vector4 pos)
            {
                posBuffer = uvBuffer = normalBuffer = new List<Vector4>();
                indexBuffer = new List<Index>();
                WorldMatrix = CreateModelMatrix(pos);
                LoadObj(objName + ".obj");
            }

            void LoadObj(string objPath)
            {
                float x, y, z;
                StreamReader sr = new StreamReader(objPath, Encoding.Default);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    Console.WriteLine(line.ToString());
                    string str = line.ToString();
                    if (str.StartsWith("vt"))
                    {
                        // load uv
                        string[] arr = str.Split(' ');
                        x = float.Parse(arr[1]);
                        y = float.Parse(arr[2]);
                        uvBuffer.Add(new Vector4(x,y,0,0));
                    }
                    else if (str.StartsWith("v"))
                    {
                        // load vertex
                        string[] arr = str.Split(' ');
                        x = float.Parse(arr[1]);
                        y = float.Parse(arr[2]);
                        z = float.Parse(arr[3]);
                        posBuffer.Add(new Vector4(x, y, z, 0));
                    }
                    else if (str.StartsWith("vn"))
                    {
                        // load normal
                        string[] arr = str.Split(' ');
                        x = float.Parse(arr[1]);
                        y = float.Parse(arr[2]);
                        z = float.Parse(arr[3]);
                        normalBuffer.Add(new Vector4(x, y, z, 0));
                    }
                    else if (str.StartsWith("f"))
                    {
                        // load index
                        if (str.Contains("//")) // pos//normal, no uv. "f 181//176 182//182 209//208"
                        {
                            string[] arr = str.Substring(2).Split(' ');
                            Index idx = Index.Zero;
                            for (int i = 0; i < arr.Length; i++)
                            {
                                string[] s = arr[i].Split(new[] {'/', '/'});
                                idx.pos[i] = int.Parse(s[0]);
                                idx.normal[i] = int.Parse(s[1]);
                            }
                            indexBuffer.Add(idx);
                        }
                        else
                        {
                            int count = 0;
                            foreach (var c in str)
                            {
                                if (c == '/')
                                {
                                    count++;
                                }
                            }
                            if (count == 6)    // pos/uv/normal, such as "f 181/292/176 182/250/182 209/210/208"
                            {
                                string[] arr = str.Substring(2).Split(' ');
                                Index idx = Index.Zero;
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    string[] s = arr[i].Split('/');
                                    idx.pos[i] = int.Parse(s[0]);
                                    idx.uv[i] = int.Parse(s[1]);
                                    idx.normal[i] = int.Parse(s[2]);
                                }
                                indexBuffer.Add(idx);
                            }
                            else if (count == 3)   // pos/uv, no normal. "f 181/176 182/182 209/208"
                            {
                                string[] arr = str.Substring(2).Split(' ');
                                Index idx = Index.Zero;
                                for (int i = 0; i < arr.Length; i++)
                                {
                                    string[] s = arr[i].Split('/');
                                    idx.pos[i] = int.Parse(s[0]);
                                    idx.uv[i] = int.Parse(s[1]);
                                }
                                indexBuffer.Add(idx);
                            }
                        }
                    }
                }
            }
        }


        struct Render
        {
            int width, height;
            List<Vector4> frameBuffer;
            List<float> depthBuffer;
            Matrix4 projMat, viewMat, mvMat, mvpMat, nmvMat;

            public Render(int width, int height)
            {
                this.width = width;
                this.height = height;
                frameBuffer = new List<Vector4>();
                depthBuffer = new List<float>();
                projMat = viewMat = mvMat = mvpMat = nmvMat = Matrix4.identity;
            }

            public void SetFrustum(float hfov, float ratio, float n, float f)
            {
                // 设置平截头体
                projMat = CreateProjectionMatrix(hfov, ratio, n, f);
            }

            public void SetCamera(Vector4 look, Vector4 at)
            {
                viewMat = CreateViewMatrix(look, at, new Vector4(0,1,0,0));
            }

            private Vertex VertexShader(Vector4 pos, Vector4 normal, Vector4 uv)
            {
                Vertex vert = new Vertex();
                vert.pos = TransformPoint(pos, mvpMat);
                vert.viewPos = TransformPoint(pos, mvMat);
                vert.normal = TransformDir(normal, nmvMat);
                vert.uv = uv;
                return vert;
            }

            private void Ndc2Screen(ref Vector4 pos)
            {
                pos.x = (pos.x + 1) * 0.5f * width;
                pos.y = (pos.y + 1) * 0.5f * height;
                pos.z = pos.w;
                pos.w = 1.0f / pos.w;
            }

            private bool BackFaceCulling(Vector4 p1, Vector4 p2, Vector4 p3)
            {
                return Vector4.Dot(p1, Vector4.Cross((p2 - p1), (p3 - p1))) >= 0;  //TODO: no idea what it is
            }

            public void DrawModel(Model model, bool drawTexture = true, bool drawWireFrame = false)
            {
                mvMat = model.WorldMatrix * viewMat;
                mvpMat = mvMat * projMat;
                nmvMat = mvMat.InvertTranspose();

                foreach (var idx in model.indexBuffer)
                {
                    // 一次取出3个顶点
                    Vertex[] outVertexes = new Vertex[3];
                    bool badTriangle = false;

                    for (int i = 0; i < 3; i++)
                    {
                        // 经过VertexShader，得到归一化的的顶点[-1,1]
                        outVertexes[i] = VertexShader(model.posBuffer[idx.pos[i]], model.normalBuffer[idx.normal[i]], model.uvBuffer[idx.uv[i]]);

                        if (outVertexes[i].pos.z < 0 || outVertexes[i].pos.z > 1)
                        { // 超出了视锥体的范围，剔除
                            badTriangle = true;
                            break;
                        }
                        Ndc2Screen(ref outVertexes[i].pos);
                    }

                    if (badTriangle || BackFaceCulling(outVertexes[0].viewPos, outVertexes[1].viewPos, outVertexes[2].viewPos))
                    {
                        continue;
                    }
                    if (drawTexture) FillTriangle(model, outVertexes[0], outVertexes[1], outVertexes[2]);
                }

                Console.Out.WriteLine("aa");
            }
        }

        

        static void Main(string[] args)
        {
            int width = 1024, height = 768;
            Render render = new Render(width, height);
            render.SetFrustum((float)Math.PI/2, (float)width/(float)height, 0.1f, 1000);
            render.SetCamera(new Vector4(0,3,5,0), Vector4.Zero);

            Model cube = new Model("res/cube",new Vector4() {x = 1, y = 1, z =1});

            render.DrawModel(cube, true, false);

            Console.In.ReadLine();
        }
    }
}
