using Godot;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Menu
{
    public class Menu : Spatial
    {
        private ResourceInteractiveLoader _res_loader = null;
        private Thread _loading_thread = null;

        [Signal] public delegate void replace_main_scene(PackedScene scene);
        [Signal] public delegate void quit(); // Useless, but needed as there is no clean way to check if a node exposes a signal

        private Control _ui;
        private Control _main;
        private TextureButton _playButton;
        private TextureButton _settingsButton;
        private TextureButton _quitButton;

        private Control _settingsMenu;
        private Control _settingsActions;
        private Control _settingsActionApply;
        private Control _settingsActionCancel;

        private Control _giMenu;
        private Button _giHigh;
        private Button _giLow;
        private Button _giDisabled;

        private Control _aaMenu;
        private Button _aa8x;
        private Button _aa4x;
        private Button _aa2x;
        private Button _aaDisabled;

        private Control _ssaoMenu;
        private Button _ssaoHigh;
        private Button _ssaoLow;
        private Button _ssaoDisabled;

        private Control _bloomMenu;
        private Button _bloomHigh;
        private Button _bloomLow;
        private Button _bloomDisabled;

        private Control _resolutionMenu;
        private Button _resolutionNative;
        private Button _resolution1080;
        private Button _resolution720;
        private Button _resolution540;

        private Control _fullscreenMenu;
        private Button _fullscreenYes;
        private Button _fullscreenNo;

        private Control _loading;
        private ProgressBar _loadingProgress;
        private Timer _loadingDoneTimer;

        public override void _Ready()
        {
            _ui = GetNode<Control>("UI");
            _main = _ui.GetNode<Control>(@"Main");
            _playButton = _main.GetNode<TextureButton>(@"Play");
            _settingsButton = _main.GetNode<TextureButton>(@"Settings");
            _quitButton = _main.GetNode<TextureButton>(@"Quit");

            _settingsMenu = _ui.GetNode<Control>(@"Settings");
            _settingsActions = _settingsMenu.GetNode<Control>(@"Actions");
            _settingsActionApply = _settingsActions.GetNode<Control>(@"Apply");
            _settingsActionCancel = _settingsActions.GetNode<Control>(@"Cancel");

            _giMenu = _settingsMenu.GetNode<Control>(@"GI");
            _giHigh = _giMenu.GetNode<Button>(@"High");
            _giLow = _giMenu.GetNode<Button>(@"Low");
            _giDisabled = _giMenu.GetNode<Button>(@"Disabled");

            _aaMenu = _settingsMenu.GetNode<Control>(@"AA");
            _aa8x = _aaMenu.GetNode<Button>(@"8X");
            _aa4x = _aaMenu.GetNode<Button>(@"4X");
            _aa2x = _aaMenu.GetNode<Button>(@"2X");
            _aaDisabled = _aaMenu.GetNode<Button>(@"Disabled");

            _ssaoMenu = _settingsMenu.GetNode<Control>(@"SSAO");
            _ssaoHigh = _ssaoMenu.GetNode<Button>(@"High");
            _ssaoLow = _ssaoMenu.GetNode<Button>(@"Low");
            _ssaoDisabled = _ssaoMenu.GetNode<Button>(@"Disabled");

            _bloomMenu = _settingsMenu.GetNode<Control>(@"Bloom");
            _bloomHigh = _bloomMenu.GetNode<Button>(@"High");
            _bloomLow = _bloomMenu.GetNode<Button>(@"Low");
            _bloomDisabled = _bloomMenu.GetNode<Button>(@"Disabled");

            _resolutionMenu = _settingsMenu.GetNode<Control>(@"Resolution");
            _resolutionNative = _resolutionMenu.GetNode<Button>(@"Native");
            _resolution1080 = _resolutionMenu.GetNode<Button>(@"1080");
            _resolution720 = _resolutionMenu.GetNode<Button>(@"720");
            _resolution540 = _resolutionMenu.GetNode<Button>(@"540");

            _fullscreenMenu = _settingsMenu.GetNode<Control>(@"Fullscreen");
            _fullscreenYes = _fullscreenMenu.GetNode<Button>(@"Yes");
            _fullscreenNo = _fullscreenMenu.GetNode<Button>(@"No");

            _loading = _ui.GetNode<Control>(@"Loading");
            _loadingProgress = _loading.GetNode<ProgressBar>(@"Progress");
            _loadingDoneTimer = _loading.GetNode<Timer>(@"DoneTimer");

            GetTree().SetScreenStretch(SceneTree.StretchMode.Mode2d, SceneTree.StretchAspect.Keep, new Vector2(1920, 1080));
            _playButton.GrabFocus();
        }

        private void interactive_load(ResourceInteractiveLoader loader)
        {
            while (true)
            {
                var status = loader.Poll();
                if (status == Error.Ok)
                {
                    _loadingProgress.Value = (loader.GetStage() * 100) / loader.GetStageCount();
                    continue;
                }
                else if (status == Error.FileEof)
                {
                    _loadingProgress.Value = 100;
                    _loadingDoneTimer.Start();
                    break;
                }
                else
                {
                    GD.Print($"Error while loading level: {status}");
                    _main.Show();
                    _loading.Hide();
                    break;
                }
            }
        }

        private void loading_done(ResourceInteractiveLoader loader)
        {
            _loading_thread.WaitToFinish();
            EmitSignal(nameof(replace_main_scene), loader.GetResource());
            // Issue: https://github.com/godotengine/godot/issues/33809
            _res_loader.Dispose();
            _res_loader = null;
        }

        private void _on_loading_done_timer_timeout()
        {
            loading_done(_res_loader);
        }

        private void _on_play_pressed()
        {
            _main.Hide();
            _loading.Show();
            var path = "res://level/level.tscn";

            if (ResourceLoader.HasCached(path))
            {
                EmitSignal(nameof(replace_main_scene), ResourceLoader.Load<PackedScene>(path));
            }
            else
            {
                _res_loader = ResourceLoader.LoadInteractive(path);
                _loading_thread = new Thread();
        		_loading_thread.Start(this, "interactive_load", _res_loader);
            }
        }

        private void _on_settings_pressed()
        {
            _main.Hide();
            _settingsMenu.Show();
            _settingsActionCancel.GrabFocus();

            var settings = GetNode<Settings>("/root/Settings");

            if (settings.GIQuality == Settings.GIQualityEnum.High)
                _giHigh.Pressed = true;
            else if (settings.GIQuality == Settings.GIQualityEnum.Low)
                _giLow.Pressed = true;
            else if (settings.GIQuality == Settings.GIQualityEnum.Disabled)
                _giDisabled.Pressed = true;

            if (settings.AAQuality == Settings.AAQualityEnum.AA_8x)
                _aa8x.Pressed = true;
            else if (settings.AAQuality == Settings.AAQualityEnum.AA_4x)
                _aa4x.Pressed = true;
            else if (settings.AAQuality == Settings.AAQualityEnum.AA_2x)
                _aa2x.Pressed = true;
            else if (settings.AAQuality == Settings.AAQualityEnum.Disabled)
                _aaDisabled.Pressed = true;

            if (settings.SSAOQuality == Settings.SSAOQualityEnum.High)
                _ssaoHigh.Pressed = true;
            else if (settings.SSAOQuality == Settings.SSAOQualityEnum.Low)
                _ssaoLow.Pressed = true;
            else if (settings.SSAOQuality == Settings.SSAOQualityEnum.Disabled)
                _ssaoDisabled.Pressed = true;

            if (settings.BloomQuality == Settings.BloomQualityEnum.High)
                _bloomHigh.Pressed = true;
            else if (settings.BloomQuality == Settings.BloomQualityEnum.Low)
                _bloomLow.Pressed = true;
            else if (settings.BloomQuality == Settings.BloomQualityEnum.Disabled)
                _bloomDisabled.Pressed = true;

            if (settings.Resolution == Settings.ResolutionEnum.Native)
                _resolutionNative.Pressed = true;
            else if (settings.Resolution == Settings.ResolutionEnum.Res_1080)
                _resolution1080.Pressed = true;
            else if (settings.Resolution == Settings.ResolutionEnum.Res_720)
                _resolution720.Pressed = true;
            else if (settings.Resolution == Settings.ResolutionEnum.Res_540)
                _resolution540.Pressed = true;

            if (settings.Fullscreen)
                _fullscreenYes.Pressed = true;
            else
                _fullscreenNo.Pressed = true;
        }

        private void _on_quit_pressed()
        {
            GetTree().Quit();
        }

        private void _on_apply_pressed()
        {
            _main.Show();
            _playButton.GrabFocus();
            _settingsMenu.Hide();

            var settings = GetNode<Settings>("/root/Settings");

            if (_giHigh.Pressed)
                settings.GIQuality = Settings.GIQualityEnum.High;
            else if (_giLow.Pressed)
                settings.GIQuality = Settings.GIQualityEnum.Low;
            else if (_giDisabled.Pressed)
                settings.GIQuality = Settings.GIQualityEnum.Disabled;

            if (_aa8x.Pressed)
                settings.AAQuality = Settings.AAQualityEnum.AA_8x;
            else if (_aa4x.Pressed)
                settings.AAQuality = Settings.AAQualityEnum.AA_4x;
            else if (_aa2x.Pressed)
                settings.AAQuality = Settings.AAQualityEnum.AA_2x;
            else if (_aaDisabled.Pressed)
                settings.AAQuality = Settings.AAQualityEnum.Disabled;


            if (_ssaoHigh.Pressed)
                settings.SSAOQuality = Settings.SSAOQualityEnum.High;
            if (_ssaoLow.Pressed)
                settings.SSAOQuality = Settings.SSAOQualityEnum.Low;
            if (_ssaoDisabled.Pressed)
                settings.SSAOQuality = Settings.SSAOQualityEnum.Disabled;

            if (_bloomHigh.Pressed)
                settings.BloomQuality = Settings.BloomQualityEnum.High;
            else if (_bloomLow.Pressed)
                settings.BloomQuality = Settings.BloomQualityEnum.Low;
            else if (_bloomDisabled.Pressed)
                settings.BloomQuality = Settings.BloomQualityEnum.Disabled;

            if (_resolutionNative.Pressed)
                settings.Resolution = Settings.ResolutionEnum.Native;
            else if (_resolution1080.Pressed)
                settings.Resolution = Settings.ResolutionEnum.Res_1080;
            else if (_resolution720.Pressed)
                settings.Resolution = Settings.ResolutionEnum.Res_720;
            else if (_resolution540.Pressed)
                settings.Resolution = Settings.ResolutionEnum.Res_540;

            settings.Fullscreen = _fullscreenYes.Pressed;

            // Apply the setting directly
            OS.WindowFullscreen = settings.Fullscreen;

            settings.SaveSettings();
        }

        private void _on_cancel_pressed()
        {
            _main.Show();
            _playButton.GrabFocus();
            _settingsMenu.Hide();
        }
    }
}
