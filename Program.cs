using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using ShapeShifter.Controllers;
using ShapeShifter.Setup;
using ShapeShifter.Views;

namespace ShapeShifter
{
    public class Program
    {
        static void Main()
        {
            const int screenWidth = 800;
            const int screenHeight = 600;

            var scene = DemoSceneFactory.Create();
            var renderer = new SceneRenderer();
            var controller = new EditorController(scene);

            
            Raylib.InitWindow(screenWidth,screenHeight,"ShapeShifter");
            Raylib.SetTargetFPS(60);

            rlImGui.Setup(true);

            while(!Raylib.WindowShouldClose())
            {
                var mouse = Raylib.GetMousePosition();
                var hoveredVertex = controller.FindVertexAt(mouse);
                Raylib.BeginDrawing();

                Raylib.ClearBackground(Color.White);
                renderer.Draw(scene,hoveredVertex);

                
                Raylib.EndDrawing();
            }
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}