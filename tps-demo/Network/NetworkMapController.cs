using Godot;
using GodotTPSSharpEnhanced.Level;
using GodotTPSSharpCommon.Network.Message;
using System;
using System.Linq;

namespace GodotTPSSharpEnhanced.Network
{
    public class NetworkMapController : Node
    {
        private LevelMap _level;

        public override void _Ready()
        {
            SetPhysicsProcess(false);

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
            var json = MessagePack.MessagePackSerializer.ConvertToJson(bytes);
            GD.Print(Multiplayer.GetNetworkUniqueId(), json);
            if (levelDataPack.Players != null)
            {
                foreach (var playerMessage in levelDataPack.Players)
                {
                    var globalTransform = new Transform(
                    new Vector3(
                        playerMessage.BasisXX,
                        playerMessage.BasisXY,
                        playerMessage.BasisXZ
                    ), new Vector3(
                        playerMessage.BasisYX,
                        playerMessage.BasisYY,
                        playerMessage.BasisYZ
                    ), new Vector3(
                        playerMessage.BasisZX,
                        playerMessage.BasisZY,
                        playerMessage.BasisZZ
                    ), new Vector3(
                        playerMessage.OriginX,
                        playerMessage.OriginY,
                        playerMessage.OriginZ
                    ));
                    _level.PlayerSpawn.AddPlayer(playerMessage.PlayerId, playerMessage.PlayerId == Multiplayer.GetNetworkUniqueId(), globalTransform);
                }
            }

            SetPhysicsProcess(true);
        }

        [Puppet]
        private void UpdatePlayers(byte[] bytes)
        {
            var levelMessage = MessageSerializer.Deserialize<LevelMessage>(bytes);

            if (levelMessage.Players != null)
            {
                foreach (var playerMessage in levelMessage.Players.Where(p => p.PlayerId != Multiplayer.GetNetworkUniqueId()))
                {
                    var globalTransform = new Transform(
                    new Vector3(
                        playerMessage.BasisXX,
                        playerMessage.BasisXY,
                        playerMessage.BasisXZ
                    ), new Vector3(
                        playerMessage.BasisYX,
                        playerMessage.BasisYY,
                        playerMessage.BasisYZ
                    ), new Vector3(
                        playerMessage.BasisZX,
                        playerMessage.BasisZY,
                        playerMessage.BasisZZ
                    ), new Vector3(
                        playerMessage.OriginX,
                        playerMessage.OriginY,
                        playerMessage.OriginZ
                    ));

                    if(_level.PlayerSpawn.HasNode($"{playerMessage.PlayerId}"))
                        _level.PlayerSpawn.GetNode<Player.PlayerEntity>($"{playerMessage.PlayerId}").GlobalTransform = globalTransform;
                    else
                        GD.PushWarning($"P: {playerMessage.PlayerId} not found - NID{Multiplayer.GetNetworkUniqueId()}");
                }
            }
        }

        [Puppet]
        private void RemovePlayer(int playerId)
        {
            _level.PlayerSpawn.RemovePlayer(playerId);
        }

        [Puppet]
        private void AddPlayer(byte[] bytes)
        {
            var playerMessage = MessageSerializer.Deserialize<PlayerMessage>(bytes);
            var globalTransform = new Transform(
                    new Vector3(
                        playerMessage.BasisXX,
                        playerMessage.BasisXY,
                        playerMessage.BasisXZ
                    ), new Vector3(
                        playerMessage.BasisYX,
                        playerMessage.BasisYY,
                        playerMessage.BasisYZ
                    ), new Vector3(
                        playerMessage.BasisZX,
                        playerMessage.BasisZY,
                        playerMessage.BasisZZ
                    ), new Vector3(
                        playerMessage.OriginX,
                        playerMessage.OriginY,
                        playerMessage.OriginZ
                    ));

            _level.PlayerSpawn.AddPlayer(playerMessage.PlayerId, false, globalTransform);
        }

        public override void _PhysicsProcess(float delta)
        {
            var player = _level.PlayerSpawn.GetCurrentPlayer();
            var playerMessage = new PlayerMessage();

            var o = player.GlobalTransform.origin;

            playerMessage.OriginX = o.x;
            playerMessage.OriginY = o.y;
            playerMessage.OriginZ = o.z;

            var b = player.GlobalTransform.basis;

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

            RpcId(1, "UpdateCurrentPlayer", bytes);
        }

        public override void _ExitTree()
        {
            NetworkClient.Instance.DisconnectFromServer();
        }
    }
}
