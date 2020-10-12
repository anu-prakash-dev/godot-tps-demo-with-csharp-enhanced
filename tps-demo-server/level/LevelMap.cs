using System;
using Godot;
using GodotTPSSharpServer.Network;

namespace GodotTPSSharpServer.Level
{
    public class LevelMap : Node
    {
        public PlayerSpawn PlayerSpawn { get; private set; }
        public NetworkMapController _networkMapController;

        public override void _Ready()
        {
            PlayerSpawn = GetNode<PlayerSpawn>("PlayerSpawn");

            _networkMapController = GetNode<NetworkMapController>("NetworkMapController");
        }

        public void AddPlayer(int playerId)
        {
            PlayerSpawn.Spawn(playerId);
            // Todo: Notify _networkMapController
        }

        public void RemovePlayer(int playerId)
        {
            PlayerSpawn.Despawn(playerId);
            // Todo: Notify _networkMapController
        }
    }
}
