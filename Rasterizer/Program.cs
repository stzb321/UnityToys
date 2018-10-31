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
            int width = 1024, height = 768;
            Rasterizer.Render render = new Rasterizer.Render(width, height);
            render.SetFrustum((float)Math.PI / 2, (float)width / (float)height, 0.1f, 1000);
            render.SetCamera(new Rasterizer.Vector4(0, 3, 5), Rasterizer.Vector4.Zero);
            render.SetLight(new Rasterizer.Vector4(-10.0f, 30.0f, 30.0f ), new Rasterizer.Vector4(1.0f, 0.0f, 0.0f, 1f), new Rasterizer.Vector4(0.8f, 0.8f, 0.8f, 1f), new Rasterizer.Vector4(0.5f, 0.5f, 0.5f, 1f));

            //方块
            Rasterizer.Model cube = new Rasterizer.Model("res/cube", new Rasterizer.Vector4(0.0f, 0.0f, -3.0f), new Rasterizer.Material(0.1f, 1.0f, 0.5f));
            render.DrawModel(cube, true, false);

            ////球体
            Rasterizer.Model sphere = new Rasterizer.Model("res/sphere", new Rasterizer.Vector4(1, 1, 1), new Rasterizer.Material(0.1f, 1.0f, 0.5f));
            render.DrawModel(sphere, true, false);

            //兔子
            // Rasterizer.Model bunny = new Rasterizer.Model("res/bunny", new Rasterizer.Vector4(0.0f, 0.0f, 5.0f), new Rasterizer.Material(0.1f, 0.8f, 0.7f));
            // render.DrawModel(bunny, true, false);

            render.SaveBitMap("output.bmp");

            System.Diagnostics.ProcessStartInfo Info = new System.Diagnostics.ProcessStartInfo(Environment.CurrentDirectory + "\\output.bmp");
            Console.WriteLine(Environment.CurrentDirectory + "\\output.bmp");
            System.Diagnostics.Process Pro = System.Diagnostics.Process.Start(Info);
            //Console.In.ReadLine();
        }
    }
}
