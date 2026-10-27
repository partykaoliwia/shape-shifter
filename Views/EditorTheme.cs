using System.Numerics;
using ImGuiNET;

namespace ShapeShifter.Views;

/// <summary>
/// Defines the shared plum and lavender palette for the sidebar and Help window.
/// </summary>
public static class EditorTheme
{
    public static readonly Vector4 Accent = Rgb(181, 154, 197);
    public static readonly Vector4 MutedText = Rgb(191, 174, 201);

    /// <summary>
    /// Applies interface colors, spacing and rounding after ImGui has been initialized.
    /// Affects ImGui widgets; the scene canvas is drawn separately by Raylib.
    /// </summary>
    public static void Apply()
    {
        var style = ImGui.GetStyle();
        style.WindowPadding = new Vector2(18f, 16f);
        style.FramePadding = new Vector2(10f, 7f);
        style.ItemSpacing = new Vector2(8f, 9f);
        style.ItemInnerSpacing = new Vector2(7f, 5f);
        style.WindowRounding = 10f;
        style.FrameRounding = 6f;
        style.GrabRounding = 6f;
        style.ScrollbarRounding = 6f;
        style.ScrollbarSize = 12f;

        var colors = style.Colors;
        colors[(int)ImGuiCol.Text] = Rgb(244, 238, 247);
        colors[(int)ImGuiCol.TextDisabled] = MutedText;
        colors[(int)ImGuiCol.WindowBg] = Rgb(48, 38, 56);
        colors[(int)ImGuiCol.PopupBg] = Rgb(48, 38, 56);
        colors[(int)ImGuiCol.Border] = Rgb(100, 80, 111);
        colors[(int)ImGuiCol.BorderShadow] = Vector4.Zero;
        colors[(int)ImGuiCol.TitleBg] = Rgb(40, 31, 47);
        colors[(int)ImGuiCol.TitleBgActive] = Rgb(70, 54, 79);
        colors[(int)ImGuiCol.TitleBgCollapsed] = Rgb(48, 38, 56);
        colors[(int)ImGuiCol.FrameBg] = Rgb(65, 49, 76);
        colors[(int)ImGuiCol.FrameBgHovered] = Rgb(88, 66, 101);
        colors[(int)ImGuiCol.FrameBgActive] = Rgb(107, 80, 122);
        colors[(int)ImGuiCol.Button] = Rgb(96, 71, 111);
        colors[(int)ImGuiCol.ButtonHovered] = Rgb(120, 90, 137);
        colors[(int)ImGuiCol.ButtonActive] = Rgb(144, 114, 158);
        colors[(int)ImGuiCol.Header] = Rgb(73, 54, 85);
        colors[(int)ImGuiCol.HeaderHovered] = Rgb(100, 74, 115);
        colors[(int)ImGuiCol.HeaderActive] = Rgb(124, 94, 139);
        colors[(int)ImGuiCol.CheckMark] = Accent;
        colors[(int)ImGuiCol.SliderGrab] = Accent;
        colors[(int)ImGuiCol.SliderGrabActive] = Rgb(207, 185, 219);
        colors[(int)ImGuiCol.Separator] = Rgb(94, 73, 105);
        colors[(int)ImGuiCol.SeparatorHovered] = Accent;
        colors[(int)ImGuiCol.SeparatorActive] = Accent;
        colors[(int)ImGuiCol.ScrollbarBg] = Rgb(40, 31, 47);
        colors[(int)ImGuiCol.ScrollbarGrab] = Rgb(101, 79, 113);
        colors[(int)ImGuiCol.ScrollbarGrabHovered] = Rgb(127, 101, 140);
        colors[(int)ImGuiCol.ScrollbarGrabActive] = Accent;
        colors[(int)ImGuiCol.ResizeGrip] = Rgb(100, 74, 115);
        colors[(int)ImGuiCol.ResizeGripHovered] = Accent;
        colors[(int)ImGuiCol.ResizeGripActive] = Accent;
        colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.71f, 0.60f, 0.77f, 0.35f);
        colors[(int)ImGuiCol.NavCursor] = Accent;
    }

    private static Vector4 Rgb(byte red, byte green, byte blue)
    {
        return new Vector4(red / 255f, green / 255f, blue / 255f, 1f);
    }
}
