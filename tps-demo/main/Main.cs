using Godot;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Main
{
    public class Main : Node
    {
        public override void _Ready()
        {
            OS.WindowFullscreen = GetNode<Menu.Settings>("/root/Settings").Fullscreen;
            GoToMainMenu();
        }

        private void GoToMainMenu()
        {
            var menu = ResourceLoader.Load<PackedScene>("res://menu/menu.tscn");
            ChangeScene(menu);
        }

        private void ReplaceMainScene(PackedScene scene)
        {
            CallDeferred(nameof(ChangeScene), scene);
        }

        private void ChangeScene(PackedScene scene)
        {
            var node = scene.Instance();

            foreach (Node child in GetChildren())
            {
                RemoveChild(child);
                child.QueueFree();
            }

            AddChild(node);
            // Todo: Verify those signals
            node.Connect("quit", this, nameof(GoToMainMenu));
            node.Connect("replace_main_scene", this, nameof(ReplaceMainScene));
        }
    }
}
