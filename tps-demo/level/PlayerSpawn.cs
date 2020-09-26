using Godot;
using GodotThirdPersonShooterDemoWithCSharp.Player;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Level
{
    public class PlayerSpawn : Spatial
    {
        private PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://player/player.tscn");

        public void Spawn()
        {
            var player = (PlayerEntity)PlayerScene.Instance();
            player.GlobalTransform = GetChild<Position3D>(0).GlobalTransform;
            AddChild(player);
        }
    }
}
