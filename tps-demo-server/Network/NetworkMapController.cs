using Godot;
using GodotTPSSharpCommon.Network.Message;
using GodotTPSSharpServer.Level;
using System;

namespace GodotTPSSharpServer.Network
{
    public class NetworkMapController : Node
    {
        private LevelMap _level;

        public override void _Ready()
        {
            _level = GetParent<LevelMap>();
        }

        [Master]
        private void MapLoaded()
        {
            var levelData = new LevelMessage()
            {
                Players = new System.Collections.Generic.List<PlayerMessage>()
            };

            foreach(Spatial player in _level.PlayerSpawn.GetChildren())
            {
                var playerData = new PlayerMessage()
                {
                    // Todo: Get id from the right property
                    PlayerId = Convert.ToInt32(player.Name)
                };

                levelData.Players.Add(playerData);
            }
            
            byte[] bytes = MessageSerializer.Serialize(levelData);

            var senderId = Multiplayer.GetRpcSenderId();
            RpcId(senderId, "InitSyncMap", bytes);
        }
    }
}
