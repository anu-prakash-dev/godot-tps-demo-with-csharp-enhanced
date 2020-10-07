using Godot;
using System;

namespace GodotTPSSharpEnhanced.Autoloads
{
    public class Main : Node
    {
        public static Main Instance { get; private set; }

        private Node _current_scene ;
        public override void _Ready()
        {
            Instance = this;
            OS.WindowFullscreen = GetNode<Settings>("/root/Settings").Fullscreen;
            
            var root = GetTree().Root;
            _current_scene = root.GetChild<Node>(root.GetChildCount() - 1);
        }

        public void GoToMainMenu()
        {
            var menu = ResourceLoader.Load<PackedScene>("res://menu/menu.tscn");
            GoToScene(menu);
        }

        public void GoToScene(PackedScene scene)
        {
            CallDeferred(nameof(DeferredGoToScene), scene);
        }

        private void DeferredGoToScene(PackedScene scene)
        {
            _current_scene.Free();
            _current_scene = scene.Instance();
            GetTree().Root.AddChild(_current_scene);
            GetTree().CurrentScene = _current_scene;
        }
    }
}
