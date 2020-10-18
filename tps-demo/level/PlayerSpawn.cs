using Godot;
using GodotTPSSharpEnhanced.Player;
using GodotTPSSharpEnhanced.Player.Controllers;
using System;

namespace GodotTPSSharpEnhanced.Level
{
    public class PlayerSpawn : Spatial
    {
        private PackedScene PlayerScene = ResourceLoader.Load<PackedScene>("res://player/player.tscn");

        public PlayerEntity _currentPlayer;
        public void Spawn()
        {
            _currentPlayer = (PlayerEntity)PlayerScene.Instance();
            _currentPlayer.GlobalTransform = GetChild<Position3D>(0).GlobalTransform;
            _currentPlayer.CurrentPlayer = true;
            AddChild(_currentPlayer);

            _currentPlayer.AddChild(new PlayerController() { Name = "PlayerController" });
        }

        public void AddPlayer(int playerId, bool currentPlayer, Transform globalTransform)
        {
            var player = (PlayerEntity)PlayerScene.Instance();
            player.Name = $"{playerId}";
            player.GlobalTransform = globalTransform; // new Transform(Basis.Identity, new Vector3(10, 1, -18));
            player.CurrentPlayer = currentPlayer;
            AddChild(player);

            if (currentPlayer)
            {
                player.AddChild(new PlayerController() { Name = "PlayerController" });
                _currentPlayer = player;
            }
        }

        public PlayerEntity GetCurrentPlayer() => _currentPlayer;

        public void RemovePlayer(int playerId)
        {
            GetNode($"{playerId}").QueueFree();
        }
    }
}
