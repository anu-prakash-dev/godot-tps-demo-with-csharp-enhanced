using Godot;
using System;

namespace GodotTPSSharpServer.Level
{
    public class PlayerSpawn : Spatial
    {
        public void Spawn(int playerId)
        {
            var player = new Spatial()
            {
                Name = $"{playerId}"
            };
            
            // Todo: Set Network Position
            // player.GlobalTransform = GetChild<Position3D>(0).GlobalTransform;
            AddChild(player);

            // Todo: Add Network Controller
            // player.AddChild(new PlayerController() { Name = "PlayerController" });
        }

        public void Despawn(int playerId)
        {
            GetNode<Spatial>($"{playerId}").QueueFree();
        }
    }
}
