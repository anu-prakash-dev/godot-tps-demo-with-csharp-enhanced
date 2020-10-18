using Godot;
using GodotTPSSharpCommon.Network.Message;
using GodotTPSSharpServer.Level;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GodotTPSSharpServer.Network
{
    public class NetworkMapController : Node
    {
        private Vector3[] _spawnPositions = new Vector3[]
        {
            new Vector3(10, 1, -18)
        };
        // private LevelMap _level;
        private Dictionary<int, PlayerMessage> _players = new Dictionary<int, PlayerMessage>();
        private Dictionary<int, DateTime> _joiningPlayers = new Dictionary<int, DateTime>();

        public override void _Ready()
        {
            // _level = GetParent<LevelMap>();
        }

        [Master]
        private void MapLoaded()
        {
            var senderId = Multiplayer.GetRpcSenderId();

            // Todo: if it's false disconnect the player
            if (!_joiningPlayers.ContainsKey(senderId)) return;
            _joiningPlayers.Remove(senderId);

            JoinPlayer(senderId);

            var levelData = new LevelMessage()
            {
                Players = _players.Values.ToList()
            };

            byte[] bytes = MessageSerializer.Serialize(levelData);

            RpcId(senderId, "InitSyncMap", bytes);
        }

        [Master]
        private void UpdateCurrentPlayer(byte[] bytes)
        {
            var senderId = Multiplayer.GetRpcSenderId();
            var playerData = MessageSerializer.Deserialize<PlayerMessage>(bytes);
            playerData.PlayerId = senderId;
            _players[senderId] = playerData;
        }

        public void RemovePlayer(int playerId)
        {
            _players.Remove(playerId);
            foreach (var key in _players.Keys)
            {
                RpcId(key, "RemovePlayer", playerId);
            }
        }

        public void AddPlayer(int playerId)
        {
            // Todo: Get Better Time laster, maybe from the engine
            _joiningPlayers.Add(playerId, DateTime.Now);
        }

        public void JoinPlayer(int playerId)
        {
            var playerMessage = new PlayerMessage()
            {
                PlayerId = playerId
            };

            var o = _spawnPositions[0];

            playerMessage.OriginX = o.x;
            playerMessage.OriginY = o.y;
            playerMessage.OriginZ = o.z;

            var b = Basis.Identity;

            playerMessage.BasisXX = b.x.x;
            playerMessage.BasisXY = b.x.y;
            playerMessage.BasisXZ = b.x.z;
            playerMessage.BasisYX = b.y.x;
            playerMessage.BasisYY = b.y.y;
            playerMessage.BasisYZ = b.y.z;
            playerMessage.BasisZX = b.z.x;
            playerMessage.BasisZY = b.z.y;
            playerMessage.BasisZZ = b.z.z;

            byte[] bytes = MessageSerializer.Serialize(playerMessage);

            foreach (var key in _players.Keys)
            {
                RpcId(key, "AddPlayer", bytes);
            }
            _players.Add(playerId, playerMessage);
        }

        public override void _PhysicsProcess(float delta)
        {
            var levelMessage = new LevelMessage()
            {
                Players = _players.Values.ToList()
            };

            byte[] bytes = MessageSerializer.Serialize(levelMessage);

            foreach (var kv in _players)
            {
                RpcId(kv.Key, "UpdatePlayers", bytes);
            }
        }
    }
}
