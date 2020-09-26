using Godot;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers
{
    public class AIController : Node
    {
        private PlayerEntity _player;
        public override void _Ready()
        {
            _player = GetParent<PlayerEntity>();
        }
    }
}
