using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;

namespace ShapeShifter
{
    public class Program
    {
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;
            
            Raylib.InitWindow(screenWidth,screenHeight,"ShapeShifter");
            Raylib.SetTargetFPS(60);

            rlImGui.Setup(true);

            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();

                Raylib.ClearBackground(Color.White);
                var mouse = Raylib.GetMousePosition();

                
                Raylib.EndDrawing();
            }
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}