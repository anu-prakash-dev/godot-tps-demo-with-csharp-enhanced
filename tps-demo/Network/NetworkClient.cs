using System;
using Godot;
using GodotTPSSharpEnhanced.Autoloads;

namespace GodotTPSSharpEnhanced.Network
{
    public class NetworkClient : Node
    {
        public static NetworkClient Instance { get; private set; }

        [Signal] public delegate void LoadMapRequested(string map);

        public override void _Ready()
        {
            GetTree().Connect("connected_to_server", this, nameof(OnConnected_to_server));
            GetTree().Connect("connection_failed", this, nameof(OnConnection_failed));
            GetTree().Connect("server_disconnected", this, nameof(OnServer_disconnected));

            Instance = this;
        }

        public void ConnectToServer()
        {
            var peer = new NetworkedMultiplayerENet();
            if (peer.CreateClient("localhost", 9000) == Error.Ok)
            {
                GetTree().NetworkPeer = peer;
            }
        }

        private void OnConnected_to_server()
        {
            GD.Print("OnConnected_to_server");
        }

        private void OnConnection_failed()
        {
            GD.Print("OnConnection_failed");
        }

        private void OnServer_disconnected()
        {
            GD.Print("OnServer_disconnected");
        }

        [Puppet]
        private void LoadMap(string map)
        {
            EmitSignal(nameof(LoadMapRequested), map);
        }
    }
}
