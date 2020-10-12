using Godot;
using GodotTPSSharpServer.Autoloads;
using System;

namespace GodotTPSSharpServer.Network
{
    public class NetworkServer : Node
    {
        public override void _Ready()
        {
            GetTree().Connect("network_peer_connected", this, nameof(OnNetwork_peer_connected));
            GetTree().Connect("network_peer_disconnected", this, nameof(OnNetwork_peer_disconnected));

            var peer = new NetworkedMultiplayerENet();

            if (peer.CreateServer(9000) == Error.Ok)
            {
                GetTree().NetworkPeer = peer;
            }

        }

        private void OnNetwork_peer_connected(int id)
        {
            GD.Print($"peer_connected {id}");
            var map = Main.Instance?.AddPlayer(id);
            RpcId(id, "LoadMap", map);
        }

        private void OnNetwork_peer_disconnected(int id)
        {
            GD.Print($"peer_disconnected {id}");
            Main.Instance?.RemovePlayer(id);
        }
    }
}
