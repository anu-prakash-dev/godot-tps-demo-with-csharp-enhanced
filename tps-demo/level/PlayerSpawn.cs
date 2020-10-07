using Godot;
using GodotTPSSharpEnhanced.Player;
using GodotTPSSharpEnhanced.Player.Controllers;
using System;

namespace GodotTPSSharpEnhanced.Level
{
    public class PlayerSpawn : Spatial
    {
        private PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://player/player.tscn");

        public void Spawn()
        {
            var player = (PlayerEntity)PlayerScene.Instance();
            player.GlobalTransform = GetChild<Position3D>(0).GlobalTransform;
            player.CurrentPlayer = true;
            AddChild(player);

            player.AddChild(new PlayerController() { Name = "PlayerController" });
        }
    }
}
