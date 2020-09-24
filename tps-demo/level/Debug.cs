using Godot;

namespace GodotThirdPersonShooterDemoWithCSharp.Level
{
    public class Debug : Label
    {
        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("toggle_debug"))
                if (Visible)
                    Hide();
                else
                    Show();

            Text = $"FPS: {Engine.GetFramesPerSecond()}";
            Text += $"\nVSync: {((bool)ProjectSettings.GetSetting("display/window/vsync/use_vsync") ? "on" : "off")}";
            Text += $"\nMemory: {(OS.GetStaticMemoryUsage() / 1048576.0):F2} MiB";
        }
    }
}
