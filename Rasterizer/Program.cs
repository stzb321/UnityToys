using System;
using Rasterizer;

namespace Rasterizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int width = 1024, height = 768;
            Rasterizer.Render render = new Rasterizer.Render(width, height);
            render.SetMultisample(1);
            render.SetFrustum((float)Math.PI / 2f, (float)width / (float)height, 0.1f, 1000);
            render.SetCamera(Vector4.Zero, new Vector4(0, 3, 5), new Vector4(0,1,0,0));
            render.SetLight(new Vector4(-10.0f, 30.0f, 30.0f, 1.0f), new Vector4(0.5f, 0.0f, 0.0f, 0.0f), new Vector4(0.8f, 0.8f, 0.8f, 0.0f), new Vector4(0.5f, 0.5f, 0.5f, 0.0f));

            //方块
            Rasterizer.Model cube = new Rasterizer.Model("res/cube", new Vector4(-2.0f, 0.0f, 2.0f), new Rasterizer.Material(0.3f, 0.8f, 0.8f));
            render.DrawModel(ref cube);

            //球体
            Rasterizer.Model sphere = new Rasterizer.Model("res/sphere", new Vector4(2.5f, -0.5f, 1.5f), new Rasterizer.Material(0.1f, 1.0f, 0.5f));
            render.DrawModel(ref sphere);

            //兔子
            Rasterizer.Model bunny = new Rasterizer.Model("res/bunny", new Vector4(0.0f, 0.0f, 0.0f), new Rasterizer.Material(0.1f, 0.8f, 0.7f));
            render.DrawModel(ref bunny);

            //龙
            Rasterizer.Model dragon = new Rasterizer.Model("res/dragon", new Vector4(13.0f, -5.0f, -18.0f), new Rasterizer.Material(0.1f, 0.8f, 0.7f));
            render.DrawModel(ref dragon);

            render.SaveBitMap("output.bmp");

            watch.Stop();
            Console.WriteLine("calc time {0}", watch.ElapsedMilliseconds);

            Console.ReadLine();
        }
    }
}
