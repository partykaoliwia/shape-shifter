using System.Numerics;
using System.Text.Json;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using ShapeShifter.Controllers;
using ShapeShifter.Editing.Interaction;
using ShapeShifter.Models;
using ShapeShifter.SaveLoad;
using ShapeShifter.Setup;
using ShapeShifter.Views;
using ShapeShifter.Views.Rendering;
using Forms = System.Windows.Forms;

namespace ShapeShifter
{
    /// <summary>
    /// Starts the Raylib window and connects scene editing with the ImGui interface.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Creates the demo scene, processes input and renders frames until the window closes.
        /// Releases graphics resources after the main loop ends.
        /// </summary>
        [STAThread]
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
                // Keep sidebar clicks from selecting geometry hidden beneath the panel.
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
                    if (Raylib.IsKeyPressed(KeyboardKey.Insert) || Raylib.IsKeyPressed(KeyboardKey.I))
                    {
                        controller.InsertVertexAtHoveredEdge();
                    }
                    else if (Raylib.IsKeyPressed(KeyboardKey.Delete))
                    {
                        controller.DeleteHoveredVertex();
                    }
                }
                panel.Draw(controller, ref lineAlgorithm);
                try
                {
                    if (panel.SaveRequested)
                    {
                        using var dialog = new Forms.SaveFileDialog
                        {
                            Title = "Save scene",
                            Filter = "Scene JSON (*.json)|*.json",
                            DefaultExt = "json",
                            AddExtension = true,
                            FileName = "scene.json",
                            OverwritePrompt = true,
                            RestoreDirectory = true
                        };

                        if (dialog.ShowDialog() == Forms.DialogResult.OK)
                        {
                            SceneFile.Save(scene, dialog.FileName);
                            panel.FileStatus = "Scene saved.";
                        }
                    }
                    else if (panel.LoadRequested)
                    {
                        using var dialog = new Forms.OpenFileDialog
                        {
                            Title = "Load scene",
                            Filter = "Scene JSON (*.json)|*.json",
                            CheckFileExists = true,
                            Multiselect = false,
                            RestoreDirectory = true
                        };

                        if (dialog.ShowDialog() == Forms.DialogResult.OK)
                        {
                            var loadedScene = SceneFile.Load(dialog.FileName);
                            var loadedController = new EditorController(loadedScene);

                            scene = loadedScene;
                            controller = loadedController;

                            panel.FileStatus = "Scene loaded.";
                        }
                    }
                }
                catch (Exception ex) when (
                    ex is IOException
                    or UnauthorizedAccessException
                    or JsonException
                    or ArgumentException
                    or NotSupportedException)
                {
                    panel.FileStatus = $"File error: {ex.Message}";
                }
                int sidebarWidth = (int)EditorPanel.SidebarWidth;
                // Reserve the sidebar area without changing the scene's coordinate system.
                Raylib.BeginScissorMode(
                    sidebarWidth,
                    0,
                    Math.Max(0, Raylib.GetScreenWidth() - sidebarWidth),
                    Raylib.GetScreenHeight());
                renderer.Draw(scene,controller.GetViewState(),lineAlgorithm);
                Raylib.EndScissorMode();
                // Submit the interface last so panels and Help stay above the scene.
                rlImGui.End();
                Raylib.EndDrawing();
            }
            rlImGui.Shutdown();
            Raylib.CloseWindow();
        }
    }
}
