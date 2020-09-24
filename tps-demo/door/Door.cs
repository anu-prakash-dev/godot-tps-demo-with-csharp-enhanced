using Godot;
using GodotThirdPersonShooterDemoWithCSharp.Player;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Door
{
    public class Door : Area
    {
        private bool _open;

        private AnimationPlayer _animationPlayer;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("DoorModel/AnimationPlayer");
        }

        private void _on_door_body_entered(PlayerEntity body)
        {
            if (!_open)
            {
                _animationPlayer.Play("doorsimple_opening");
                _open = true;
            }
        }
    }
}
