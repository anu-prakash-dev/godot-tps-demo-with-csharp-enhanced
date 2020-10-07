using Godot;
using GodotTPSSharpEnhanced.Enemies;
using System;

namespace GodotTPSSharpEnhanced.Player
{
    public class Bullet : KinematicBody
    {
        private const float BulletVelocity = 50f;

        public Vector3 Direction { get; set; }

        private float _timeAlive = 5f;
        private bool _hit = false;

        private AnimationPlayer _animationPlayer;
        private CollisionShape _collisionShape;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
            _collisionShape = GetNode<CollisionShape>("CollisionShape");
        }

        public override void _Process(float delta)
        {
            if (_hit) return;

            _timeAlive -= delta;
            if (_timeAlive < 0)
            {
                _hit = true;
                _animationPlayer.Play("explode");
            }
            var col = MoveAndCollide(Direction * BulletVelocity * delta);
            if (col != null)
            {
                if (col.Collider is RedRobot _rebRobot)
                {
                    _rebRobot.Hit();
                }
                _collisionShape.Disabled = true;
                _animationPlayer.Play("explode");
                _hit = true;
            }
        }
    }
}
