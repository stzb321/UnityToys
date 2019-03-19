using System;
using Rasterizer;
// using System.Threading.Tasks;
using System.Drawing;

namespace Rasterizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            int width = 1024, height = 768;
            Rasterizer.Render render = new Rasterizer.Render(width, height);
            render.SetFrustum((float)Math.PI / 2, (float)width / (float)height, 0.1f, 1000);
            render.SetCamera(new Rasterizer.Vector4(0, 3, 5), Rasterizer.Vector4.Zero, new Rasterizer.Vector4(0,1,0,0));
            render.SetLight(new Rasterizer.Vector4(-10.0f, 30.0f, 30.0f, 1.0f), new Rasterizer.Vector4(0.5f, 0.0f, 0.0f, 0.0f), new Rasterizer.Vector4(0.8f, 0.8f, 0.8f, 0.0f), new Rasterizer.Vector4(0.5f, 0.5f, 0.5f, 0.0f));

            //方块
            Rasterizer.Model cube = new Rasterizer.Model("res/cube", new Rasterizer.Vector4(-2.0f, 0.0f, 2.0f), new Rasterizer.Material(0.3f, 0.8f, 0.8f));
            render.DrawModel(cube);

            //球体
            Rasterizer.Model sphere = new Rasterizer.Model("res/sphere", new Rasterizer.Vector4(2.5f, 0.5f, 1.5f), new Rasterizer.Material(0.1f, 1.0f, 0.5f));
            render.DrawModel(sphere);

            //兔子
            Rasterizer.Model bunny = new Rasterizer.Model("res/bunny", new Rasterizer.Vector4(0.0f, 0.0f, 0.0f), new Rasterizer.Material(0.1f, 0.8f, 0.7f));
            render.DrawModel(bunny);

            render.SaveBitMap("output.bmp");

            watch.Stop();
            var time = watch.ElapsedMilliseconds;
            Console.WriteLine("execution time {0}", time);

            Console.ReadLine();
        }
    }
}
