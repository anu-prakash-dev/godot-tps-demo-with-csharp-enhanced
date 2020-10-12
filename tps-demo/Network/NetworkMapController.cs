using Godot;
using GodotTPSSharpEnhanced.Level;
using GodotTPSSharpCommon.Network.Common.Message;
using System;

namespace GodotTPSSharpEnhanced.Network
{
    public class NetworkMapController : Node
    {
        private LevelMap _level;

        public override void _Ready()
        {
            _level = GetParent<LevelMap>();
            _level.Connect("ready", this, nameof(MapLoaded));
        }

        private void MapLoaded()
        {
            RpcId(1, "MapLoaded");
        }

        [Puppet]
        private void InitSyncMap(byte[] bytes)
        {
            var levelDataPack = MessageSerializer.Deserialize<LevelMessage>(bytes);

            if (levelDataPack.Players != null)
            {
                foreach(var playerData in levelDataPack.Players)
                {
                    _level.PlayerSpawn.AddPlayer(playerData.PlayerId, playerData.PlayerId == Multiplayer.GetNetworkUniqueId());
                }
            }
        }
    }
}
