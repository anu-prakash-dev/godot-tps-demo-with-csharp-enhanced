using Godot;
using GodotThirdPersonShooterDemoWithCSharp.Menu;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Level
{
    public class Level : Spatial
    {
        [Signal] public delegate void replace_main_scene(PackedScene scene);
        [Signal] public delegate void quit(); // Useless, but needed as there is no clean way to check if a node exposes a signal

        private WorldEnvironment _worldEnvironment;

        public override void _Ready()
        {
            _worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
            var settings = GetNode<Settings>("/root/Settings");

            if (settings.GIQuality == Settings.GIQualityEnum.High)
                ProjectSettings.SetSetting("rendering/quality/voxel_cone_tracing/high_quality", true);
            else if (settings.GIQuality == Settings.GIQualityEnum.Low)
                ProjectSettings.SetSetting("rendering/quality/voxel_cone_tracing/high_quality", false);
            else
            {
                GetNode<GIProbe>("GIProbe").Hide();
                GetNode<Spatial>("ReflectionProbes").Show();
            }

            if (settings.AAQuality == Settings.AAQualityEnum.AA_8x)
                GetTree().Root.Msaa = Viewport.MSAA.Msaa8x;
            else if (settings.AAQuality == Settings.AAQualityEnum.AA_4x)
                GetTree().Root.Msaa = Viewport.MSAA.Msaa4x;
            else if (settings.AAQuality == Settings.AAQualityEnum.AA_2x)
                GetTree().Root.Msaa = Viewport.MSAA.Msaa2x;
            else
                GetTree().Root.Msaa = Viewport.MSAA.Disabled;

            if (settings.SSAOQuality == Settings.SSAOQualityEnum.High)
                _worldEnvironment.Environment.SsaoQuality = Godot.Environment.SSAOQuality.High;
            else if (settings.SSAOQuality == Settings.SSAOQualityEnum.Low)
                _worldEnvironment.Environment.SsaoQuality = Godot.Environment.SSAOQuality.Low;
            else
                _worldEnvironment.Environment.SsaoEnabled = false;

            if (settings.Resolution == Settings.ResolutionEnum.Native) { }
            else if (settings.Resolution == Settings.ResolutionEnum.Res_1080)
            {
                var minsize = new Vector2(OS.WindowSize.x * 1080 / OS.WindowSize.y, 1080.0f);
                GetTree().SetScreenStretch(SceneTree.StretchMode.Viewport, SceneTree.StretchAspect.KeepHeight, minsize);
            }
            else if (settings.Resolution == Settings.ResolutionEnum.Res_720)
            {
                var minsize = new Vector2(OS.WindowSize.x * 720 / OS.WindowSize.y, 720.0f);
                GetTree().SetScreenStretch(SceneTree.StretchMode.Viewport, SceneTree.StretchAspect.KeepHeight, minsize);
            }
            else if (settings.Resolution == Settings.ResolutionEnum.Res_540)
            {
                var minsize = new Vector2(OS.WindowSize.x * 540 / OS.WindowSize.y, 540.0f);
                GetTree().SetScreenStretch(SceneTree.StretchMode.Viewport, SceneTree.StretchAspect.KeepHeight, minsize);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("quit"))
            {
                Input.SetMouseMode(Input.MouseMode.Visible);
                EmitSignal(nameof(quit));
            }
        }
    }
}
