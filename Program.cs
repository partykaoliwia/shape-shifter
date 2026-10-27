using System.Numerics;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using ShapeShifter.Controllers;
using ShapeShifter.Editing.Interaction;
using ShapeShifter.Models;
using ShapeShifter.Setup;
using ShapeShifter.Views;
using ShapeShifter.Views.Rendering;

namespace ShapeShifter
{
    public class Program
    {
        static void Main()
        {
            const int screenWidth = 1000;
            const int screenHeight = 700;

            var scene = DemoSceneFactory.Create();
            var renderer = new SceneRenderer();
            var controller = new EditorController(scene);
            var panel = new EditorPanel();
            var lineAlgorithm = LineAlgorithm.Library;
            Raylib.InitWindow(screenWidth,screenHeight,"ShapeShifter");
            var iconPath = Path.Combine(AppContext.BaseDirectory,"Assets","shapeshifter-icon.png");
            var icon = Raylib.LoadImage(iconPath);
            Raylib.SetWindowIcon(icon);
            Raylib.UnloadImage(icon);
            Raylib.SetTargetFPS(60);

            rlImGui.Setup(true);
            EditorTheme.Apply();

            while(!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.White);

                rlImGui.Begin();
                var io = ImGui.GetIO();
                bool focused = Raylib.IsWindowFocused();
                var mousePosition = Raylib.GetMousePosition();
                bool isMouseAvailable = focused
                    && !io.WantCaptureMouse
                    && mousePosition.X >= EditorPanel.SidebarWidth;
                bool keyboardAvailable = focused && !io.WantCaptureKeyboard;
                var isLeftPressed = focused && Raylib.IsMouseButtonPressed(MouseButton.Left);
                var isLeftDown = focused && Raylib.IsMouseButtonDown(MouseButton.Left);
                var extendSelection = keyboardAvailable && (Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl));
                bool preferControlPoints = keyboardAvailable && !extendSelection && (Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift));
                var input = new MouseInput(Position: mousePosition, LeftPressed: isLeftPressed, LeftDown: isLeftDown, CanInteractWithScene:isMouseAvailable,ExtendSelection: extendSelection, PreferControlPoints: preferControlPoints);
                controller.UpdateMouse(input);
                if (isMouseAvailable && keyboardAvailable)
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
                int sidebarWidth = (int)EditorPanel.SidebarWidth;
                Raylib.BeginScissorMode(
                    sidebarWidth,
                    0,
                    Math.Max(0, Raylib.GetScreenWidth() - sidebarWidth),
                    Raylib.GetScreenHeight());
                renderer.Draw(scene,controller.GetViewState(),lineAlgorithm);
                Raylib.EndScissorMode();
                rlImGui.End();
                Raylib.EndDrawing();
            }
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}
