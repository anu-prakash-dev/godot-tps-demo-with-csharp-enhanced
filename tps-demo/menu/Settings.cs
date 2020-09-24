using Godot;
using Godot.Collections;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Menu
{
    public class Settings : Node
    {
        public enum GIQualityEnum : int
        {
            Disabled = 0,
            Low = 1,
            High = 2,
        }

        public enum AAQualityEnum
        {
            Disabled = 0,
            AA_2x = 1,
            AA_4x = 2,
            AA_8x = 3,
        }

        public enum SSAOQualityEnum
        {
            Disabled = 0,
            Low = 1,
            High = 2,
        }

        public enum ResolutionEnum
        {
            Res_540 = 0,
            Res_720 = 1,
            Res_1080 = 2,
            Native = 3,
        }

        public GIQualityEnum GIQuality { get; set; } = GIQualityEnum.Low;
        public AAQualityEnum AAQuality { get; set; } = AAQualityEnum.AA_2x;
        public SSAOQualityEnum SSAOQuality { get; set; } = SSAOQualityEnum.Disabled;
        public ResolutionEnum Resolution { get; set; } = ResolutionEnum.Native;
        public bool Fullscreen { get; set; } = true;

        public override void _Ready()
        {
            LoadSettings();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("toggle_fullscreen"))
            {
                OS.WindowFullscreen = !OS.WindowFullscreen;
                GetTree().SetInputAsHandled();
            }
        }

        public void LoadSettings()
        {
            var f = new File();
            var error = f.Open("user://settings.json", File.ModeFlags.Read);
            if (error != Error.Ok)
            {
                GD.Print("There are no settings to load.");
                return;
            }

            var d = JSON.Parse(f.GetAsText());
            f.Close();

            if (!(d.Result is Dictionary _dict))
            {
                GD.Print("Error it's not a dictionary");
                return;
            }

            if (_dict.Contains("gi"))
                GIQuality = (GIQualityEnum)Convert.ToInt32(_dict["gi"]);

            if (_dict.Contains("aa"))
                AAQuality = (AAQualityEnum)Convert.ToInt32(_dict["aa"]);

            if (_dict.Contains("ssao"))
                SSAOQuality = (SSAOQualityEnum)Convert.ToInt32(_dict["ssao"]);

            if (_dict.Contains("resolution"))
                Resolution = (ResolutionEnum)Convert.ToInt32(_dict["resolution"]);

            if (_dict.Contains("fullscreen"))
                Fullscreen = (bool)_dict["fullscreen"];
        }

        public void SaveSettings()
        {
            var f = new File();
            var error = f.Open("user://settings.json", File.ModeFlags.Write);
            if (error != Error.Ok)
            {
                GD.PrintErr("Error opening file to write.");
                return;
            }

            var d = new Dictionary()
            {
                {"gi", (int)GIQuality},
                {"aa", (int)AAQuality},
                {"ssao", (int)SSAOQuality},
                {"resolution", (int)Resolution},
                {"fullscreen",  Fullscreen}
            };
            f.StoreLine(JSON.Print(d));
            f.Close();
        }
    }
}
