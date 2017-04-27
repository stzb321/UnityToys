/* 一个简单的光栅化渲染器，支持模型坐标转换，单个点光源
 * 参考资料：
 * 1. http://www.scratchapixel.com/lessons/3d-basic-rendering/rasterization-practical-implementation/rasterization-stage
 * 2. http://www.opengl-tutorial.org/cn/beginners-tutorials/tutorial-3-matrices/
 * 3. https://www.google.com.hk/
*/

using System;
using System.Collections.Generic;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Drawing;

namespace MiniRasterizer
{
    class MiniRasterizer
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

            public Vector4(float x, float y, float z, float w = 0f)
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
                return new Vector4(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
            }

            public static Vector4 operator -(Vector4 a)
            {
                return new Vector4(-a.x, -a.y, -a.z, -a.w);
            }

            public static Vector4 operator -(Vector4 a, Vector4 b)
            {
                return new Vector4(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
            }

            public static Vector4 operator *(Vector4 a, Vector4 b)
            {
                return new Vector4(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
            }

            public static Vector4 operator *(Vector4 a, float b)
            {
                return new Vector4(a.x * b, a.y * b, a.z * b, a.w * b);
            }

            public static Vector4 Cross(Vector4 a, Vector4 b)
            {
                return new Vector4(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
            }

            public static float Dot(Vector4 a, Vector4 b)
            {
                return a.x * b.x + a.y * b.y + a.z * b.z;
            }

            public Vector4 Normalize()
            {
                var invlen = 1.0f / (float)Math.Sqrt(x * x + y * y + z * z);
                return new Vector4(x * invlen, y * invlen, z = z * invlen);
            }
        }

        struct Matrix4
        {
            public float m00, m01, m02, m03, m10, m11, m12, m13, m20, m21, m22, m23, m30, m31, m32, m33;

            public float this[int row, int column]   //TODO: 为了能像数组那样调用，这里用了反射，是否有更简洁且不使用反射的方法？
            {
                get{
                    if (row < 0 || row >= 4 || column < 0 || column >= 4)
                    {
                        throw new Exception("out of matrix range");
                    }
                    FieldInfo field = this.GetType().GetField(string.Format("m{0}{1}", row, column));
                    return (float)field.GetValueDirect(__makeref(this));
                }
                set
                {
                    if (row < 0 || row >= 4 || column < 0 || column >= 4)
                    {
                        throw new Exception("out of matrix range");
                    }
                    FieldInfo field = this.GetType().GetField(string.Format("m{0}{1}", row, column));
                    field.SetValueDirect(__makeref(this), (float)value);
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

            public static readonly Matrix4 Zero = new Matrix4
            (
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f,
                0f, 0f, 0f, 0f
            );

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
                tmp[0] = temm.m22 * temm.m33;
                tmp[1] = temm.m32 * temm.m23;
                tmp[2] = temm.m12 * temm.m33;
                tmp[3] = temm.m32 * temm.m13;
                tmp[4] = temm.m12 * temm.m23;
                tmp[5] = temm.m22 * temm.m13;
                tmp[6] = temm.m02 * temm.m33;
                tmp[7] = temm.m32 * temm.m03;
                tmp[8] = temm.m02 * temm.m23;
                tmp[9] = temm.m22 * temm.m03;
                tmp[10] = temm.m02 * temm.m13;
                tmp[11] = temm.m12 * temm.m03;

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
                Matrix4 t = Matrix4.identity;
                Matrix4 o = this;
                o.Invert();
                t.m00 = o.m00; t.m01 = o.m10; t.m02 = o.m20; t.m03 = o.m30;
                t.m10 = o.m01; t.m11 = o.m11; t.m12 = o.m21; t.m13 = o.m31;
                t.m20 = o.m02; t.m21 = o.m12; t.m22 = o.m22; t.m23 = o.m32;
                t.m30 = o.m03; t.m31 = o.m13; t.m32 = o.m23; t.m33 = o.m33;
                return t;
            }
        }

        static Vector4 TransformPoint(Vector4 point, Matrix4 mat)
        {
            Vector4 p = Vector4.Zero;
            p.w = mat.m03 * point.x + mat.m13 * point.y + mat.m23 * point.z + mat.m33;
            p.x = (mat.m00 * point.x + mat.m10 * point.y + mat.m20 * point.z + mat.m30) / p.w;
            p.y = (mat.m01 * point.x + mat.m11 * point.y + mat.m21 * point.z + mat.m31) / p.w;
            p.z = (mat.m02 * point.x + mat.m12 * point.y + mat.m22 * point.z + mat.m32) / p.w;
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
            Matrix4 mat = Matrix4.identity;
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

        struct Texture
        {
            public int width, height;
            public float smax, tmax;
            public List<Vector4> data;

            //public Texture(int a)
            //{
            //    width = 0;
            //    height = 0;
            //    smax = 0f;
            //    tmax = 0f;
            //    data = new List<Vector4>();
            //}
        }

        struct Material
        {
            public float ka, kd, ks;
            public Texture texture;

            public Material(float a, float d, float s)
            {
                ka = a;
                kd = d;
                ks = s;
                texture = new Texture();
            }
        }

        struct Light
        {
            public Vector4 pos, viewPos, ambientColor, diffuseColor, specularColor;
        }

        struct Model
        {
            public Material material;
            public List<Vector4> posBuffer, uvBuffer, normalBuffer;
            public List<Index> indexBuffer;
            public Matrix4 WorldMatrix;
            

            public Model(string objName, Vector4 pos, Material mat)
            {
                posBuffer = uvBuffer = normalBuffer = new List<Vector4>();
                indexBuffer = new List<Index>();
                material = mat;
                WorldMatrix = CreateModelMatrix(pos);
                LoadObj(objName + ".obj");
                if (uvBuffer.Count > 1) // load texture only if the model has uv data.
                    LoadBmp(ref material.texture, objName + ".bmp");
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
                                string[] s = arr[i].Split(new string[] {"//"}, StringSplitOptions.RemoveEmptyEntries);
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

                foreach (var index in indexBuffer)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (index.pos[i] < 0) index.pos[i] += posBuffer.Count;
                        if (index.uv[i] < 0) index.uv[i] += uvBuffer.Count;
                        if (index.normal[i] < 0) index.normal[i] += normalBuffer.Count;
                    }
                }
            }


            void LoadBmp(ref Texture texture, string file)
            {
                texture.data = new List<Vector4>();
                return;
            } // load bmp into texture
        }


        struct Render
        {
            int width, height;
            Vector4[] frameBuffer;
            float[] depthBuffer;
            Matrix4 projMat, viewMat, mvMat, mvpMat, nmvMat;
            Light light;

            public Render(int width, int height)
            {
                this.width = width;
                this.height = height;
                light = new Light();
                projMat = viewMat = mvMat = mvpMat = nmvMat = Matrix4.identity;

                frameBuffer = new Vector4[width * height];
                depthBuffer = new float[width * height];

                //Init frameBuffer
                for (int i = 0; i < frameBuffer.Length; i++)
                {
                    frameBuffer[i] = new Vector4(0, 0, 0.34f, 1f);
                }

                //Init depthBuffer
                for (int i = 0; i < depthBuffer.Length; i++)
                {
                    depthBuffer[i] = float.MaxValue;
                }
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

            public void SetLight(Vector4 pos, Vector4 ambi, Vector4 diff, Vector4 spec)
            {
                light.pos = pos; light.ambientColor = ambi; light.diffuseColor = diff; light.specularColor = spec;
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
                return Vector4.Dot(p1, Vector4.Cross((p2 - p1), (p3 - p1))) >= 0;  //右手法则
            }

            public void DrawModel(Model model, bool drawTexture = true, bool drawWireFrame = false)
            {
                mvMat = model.WorldMatrix * viewMat;
                mvpMat = mvMat * projMat;
                nmvMat = mvMat.InvertTranspose();

                for (int k = 0; k < model.indexBuffer.Count; k++)
                {
                    Index idx = model.indexBuffer[k];
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
            }

            private Vector4 PixelShader(Model model, Vertex v)
            {
                Vector4 ldir = (light.viewPos - v.viewPos).Normalize();
                float lambertian = Math.Max(0f, Vector4.Dot(ldir, v.normal));
                float specular = 0f;
                if (lambertian > 0)
                {
                    Vector4 viewDir = (-v.viewPos).Normalize();
                    Vector4 half = (ldir + viewDir).Normalize ();
                    float angle = Math.Max(0f, Vector4.Dot(half, v.normal));
                    specular = (float)Math.Pow(angle, 16.0f);
                }
                return ( TextureLookup(model.material.texture, v.uv.x, v.uv.y) * (light.ambientColor * model.material.ka + light.diffuseColor * lambertian * model.material.kd) + light.specularColor * specular * model.material.ks);
            }

            private void FillTriangle(Model model, Vertex v1, Vertex v2, Vertex v3)
            {
                Vector4 weight = new Vector4(0, 0, 0, EdgeFunc(v1.pos, v2.pos, v3.pos));
                int x0 = Math.Max(0, (int)Math.Floor (Math.Min (v1.pos.x, Math.Min (v2.pos.x, v3.pos.x))));
		        int y0 = Math.Max (0, (int)Math.Floor (Math.Min (v1.pos.y, Math.Min (v2.pos.y, v3.pos.y))));
		        int x1 = Math.Min (width - 1, (int)Math.Floor (Math.Max (v1.pos.x, Math.Max (v2.pos.x, v3.pos.x))));
		        int y1 = Math.Min (height - 1, (int)Math.Floor (Math.Max (v1.pos.y, Math.Max (v3.pos.y, v3.pos.y))));
                for (int y = y0; y <= y1; y++) //       only check for points that are inside the screen
                {
                    for (int x = x0; x <= x1; x++)
                    {
                        Vertex v = new Vertex();
                        v.pos = new Vector4(x + 0.5f, y + 0.5f, 0, 0);

                        // v is outside the triangle
                        if (TriangleCheck(v1, v2, v3, v, ref weight)) continue;

                        // perspective correct interpolation
                        Interpolate(v1, v2, v3, ref v, weight);

                        // z test
                        if (v.pos.z >= depthBuffer[x + y * width]) continue;

                        DrawPoint(x, y, PixelShader(model, v), v.pos.z);
                    }
                }
            }

            bool TriangleCheck (Vertex v0, Vertex v1, Vertex v2, Vertex v, ref Vector4 w) {
		        w.x = EdgeFunc (v1.pos, v2.pos, v.pos) * v0.pos.w / w.w; // pos.w == 1 / pos.z . we did that in Ndc2Screen()
		        w.y = EdgeFunc (v2.pos, v0.pos, v.pos) * v1.pos.w / w.w;
		        w.z = EdgeFunc (v0.pos, v1.pos, v.pos) * v2.pos.w / w.w;
		        return (w.x < 0 || w.y < 0 || w.z < 0);
	        }

            void Interpolate (Vertex v0, Vertex v1, Vertex v2, ref Vertex v, Vector4 w) {
		        v.pos.z = 1.0f / (w.x + w.y + w.z);
		        v.viewPos = (v0.viewPos * w.x + v1.viewPos * w.y + v2.viewPos * w.z) * v.pos.z;
		        v.normal = (v0.normal * w.x + v1.normal * w.y + v2.normal * w.z) * v.pos.z;
		        v.color = (v0.color * w.x + v1.color * w.y + v2.color * w.z) * v.pos.z;
		        v.uv = (v0.uv * w.x + v1.uv * w.y + v2.uv * w.z) * v.pos.z;
	        }

            float EdgeFunc (Vector4 p0, Vector4 p1, Vector4 p2) {
		        return ((p2.x - p0.x) * (p1.y - p0.y) - (p2.y - p0.y) * (p1.x - p0.x));
	        }


            public Vector4 TextureLookup(Texture texture, float s, float t)
            {
                Vector4 color = new Vector4(0.87f, 0.87f, 0.87f, 1f);
                if (texture.data.Count > 0)
                {
                    s = Saturate(s);
                    t = Saturate(t);
                    color = BilinearFiltering(texture, s * (texture.width - 1), t * (texture.height - 1));
                }
                return color;
            }

            public float Saturate(float n)
            {
                return Math.Min(1.0f, Math.Max(0f, n));
            }

            Vector4 BilinearFiltering (Texture texture, float s, float t) {
		        if (s <= 0.5f || s >= texture.smax) return LinearFilteringV (texture, s, t);
		        if (t <= 0.5f || t >= texture.tmax) return LinearFilteringH (texture, s, t);
                float supper = s + 0.5f, fs = (float)Math.Floor(supper), ws = supper - fs, tupper = t + 0.5f, ts = (float)Math.Floor(tupper), wt = tupper - ts;
		        return (NearestNeighbor (texture, fs, ts) * ws * wt +
			        NearestNeighbor (texture, fs, ts - 1.0f) * ws * (1.0f - wt) +
			        NearestNeighbor (texture, fs - 1.0f, ts) * (1.0f - ws) * wt +
			        NearestNeighbor (texture, fs - 1.0f, ts - 1.0f) * (1.0f - ws) * (1.0f - wt));
	        }

            public Vector4 LinearFilteringH (Texture texture, float s, float t) {
		        if (s <= 0.5f || s >= texture.smax) return NearestNeighbor (texture, s, t);
		        float supper = s + 0.5f, fs = (float)Math.Floor(supper), ws = supper - fs;
		        return (NearestNeighbor (texture, fs, t) * ws + NearestNeighbor (texture, fs - 1.0f, t) * (1.0f - ws));
	        }

            public Vector4 LinearFilteringV (Texture texture, float s, float t) {
		        if (t <= 0.5f || t >= texture.tmax) return NearestNeighbor (texture, s, t);
		        float tupper = t + 0.5f, ts = (float)Math.Floor(tupper), wt = tupper - ts;
		        return (NearestNeighbor (texture, s, ts) * wt + NearestNeighbor (texture, s, ts - 1.0f) * (1.0f - wt));
	        }

            public Vector4 NearestNeighbor (Texture texture, float s, float t) {
                return texture.data[(int)Math.Round(s) + (int)Math.Round(t) * texture.width];
	        }



            void DrawPoint (int x, int y, Vector4 color, float z) {
		        if (x >= 0 && x < width && y >= 0 && y < height) {
			        frameBuffer[x + y * width] = color; // write frame buffer
			        depthBuffer[x + y * width] = z; // write z buffer
		        }
	        }


            public void SaveBitMap(string name)
            {
                Bitmap bmp = new Bitmap(width, height);
                for (int i = 0; i < frameBuffer.Length; i++)
                {
                    int x = i % width;
                    int y = i / width;
                    Vector4 color = frameBuffer[i];
                    int r = Math.Min(255, (int)(color.x * 255));
                    int g = Math.Min(255, (int)(color.y * 255));
                    int b = Math.Min(255, (int)(color.z * 255));
                    int a = Math.Min(255, (int)(color.w * 255));
                    bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                }
                bmp.Save(name);
                Console.Out.WriteLine("finish");
            }

        }
        

        static void Main(string[] args)
        {
            int width = 1024, height = 768;
            Render render = new Render(width, height);
            render.SetFrustum((float)Math.PI / 2, (float)width / (float)height, 0.1f, 1000);
            render.SetCamera(new Vector4(0, 3, 5), Vector4.Zero);
            render.SetLight(new Vector4(-10.0f, 30.0f, 30.0f ), new Vector4(0.5f, 0.0f, 0.0f, 1f), new Vector4(0.8f, 0.8f, 0.8f, 1f), new Vector4(0.5f, 0.5f, 0.5f, 1f));

            Model cube = new Model("res/cube", new Vector4(-2f, 0, 2f), new Material(0.3f, 0.8f, 0.8f));
            render.DrawModel(cube, true, false);

            Model sphere = new Model("res/sphere", new Vector4(1, 1, 1), new Material(0.1f, 1.0f, 0.5f));
            render.DrawModel(sphere, true, false);

            //Model bunny = new Model("res/bunny", new Vector4(0.0f, 0.0f, 0.0f), new Material(0.1f, 0.8f, 0.7f));
            //render.DrawModel(bunny, true, false);

            render.SaveBitMap("output.bmp");

            Console.In.ReadLine();
            
        }
    }
}
