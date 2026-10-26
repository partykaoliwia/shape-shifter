using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using ShapeShifter.Controllers;
using ShapeShifter.Rendering;
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
            var panel = new EditorPanel();
            var lineAlgorithm = LineAlgorithm.Library;

            
            Raylib.InitWindow(screenWidth,screenHeight,"ShapeShifter");
            Raylib.SetTargetFPS(60);

            rlImGui.Setup(true);

            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.White);

                rlImGui.Begin();
                var io = ImGui.GetIO();
                bool focused = Raylib.IsWindowFocused();
                bool pointerAvailable = focused && !io.WantCaptureMouse;
                bool keyboardAvailable = focused && !io.WantCaptureKeyboard;
                var mouse = Raylib.GetMousePosition();
                var isLeftPressed = focused && Raylib.IsMouseButtonPressed(MouseButton.Left);
                var isLeftDown = focused && Raylib.IsMouseButtonDown(MouseButton.Left);
                var addAnother = keyboardAvailable && (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));
                controller.UpdatePointer(mouse, isLeftPressed, isLeftDown, pointerAvailable, addAnother);
                if (pointerAvailable && keyboardAvailable)
                {
                    if (Raylib.IsKeyPressed(KeyboardKey.Insert))
                    {
                        controller.InsertVertexAtHoveredEdge();
                    }
                    else if (Raylib.IsKeyPressed(KeyboardKey.Delete))
                    {
                        controller.DeleteHoveredVertex();
                    }
                }
                panel.Draw(controller, ref lineAlgorithm);
                renderer.Draw(scene, controller.HoveredVertex, controller.HoveredEdge, controller.HoveredPolygon,lineAlgorithm,controller.SelectedEdges);
                rlImGui.End();
                Raylib.EndDrawing();
            }
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}