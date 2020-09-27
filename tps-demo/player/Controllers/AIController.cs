using Godot;
using System;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers
{
    public class AIController : Node
    {
        public const float RotationInterpolateSpeed = 10;

        public PlayerEntity Player { get => _player; }

        private PlayerEntity _player;

        private StateMachine _stateMachine;

        public Transform Orientation { get; set; } = Transform.Identity;

        private Transform _rootMotion = Transform.Identity;
        private Vector2 _motion = new Vector2();
        private Vector3 _velocity = new Vector3();
        private Vector3 _initialPosition;
        private Vector3 _gravity;

        public override void _Ready()
        {
            _player = GetParent<PlayerEntity>();
            _stateMachine = GetNode<StateMachine>("StateMachine");

            _initialPosition = _player.Transform.origin;
            _gravity =
            Convert.ToSingle(ProjectSettings.GetSetting("physics/3d/default_gravity")) *
            (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

            var orientation = _player.GetPlayerModel().GlobalTransform;
            orientation.origin = new Vector3();
            Orientation = orientation;

            _player.Connect("ready", _stateMachine, "ParentReady");
        }

        public void UpdateRootMotion() => _rootMotion = _player.GetRootMotionTransform();

        public override void _PhysicsProcess(float delta)
        {
            Orientation = Orientation * _rootMotion;

            var hVelocity = Orientation.origin / delta;
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity += _gravity * delta;
            _velocity = _player.MoveAndSlide(_velocity, Vector3.Up);

            var orientation = Orientation;
            orientation.origin = new Vector3();
            Orientation = orientation.Orthonormalized();

            var transform = _player.PlayerModel.GlobalTransform;
            transform.basis = Orientation.basis;
            _player.PlayerModel.GlobalTransform = transform;
        }
    }
}
