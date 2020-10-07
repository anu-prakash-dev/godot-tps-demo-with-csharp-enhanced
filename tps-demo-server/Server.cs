using Godot;
using System;

namespace GodotTPSSharpServer
{
    public class Server : Node
    {
        public override void _Ready()
        {
            GD.Print("Hellooo");
            var enet = new NetworkedMultiplayerENet();
            var err = enet.CreateServer(9000);
            GD.Print(err);
            GetTree().NetworkPeer = enet;
        }
    }
}
