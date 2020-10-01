using Godot;
using System;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers
{
    public class AIController : Node
    {
        [Export] public NodePath NavigationPath { get; set; }

        public const float MotionInterpolateSpeed = 10;
        public const float RotationInterpolateSpeed = 10;

        public PlayerEntity Player { get => _player; }

        private PlayerEntity _player;
        private PlayerEntity _playerTarget;

        private StateMachine _stateMachine;

        public Navigation Navigation { get; private set; }

        private Transform _orientation = Transform.Identity;
        public Transform Orientation { get => _orientation; set => _orientation = value; }

        private Transform _rootMotion = Transform.Identity;
        private Vector2 _motion = new Vector2();

        private Vector3 _velocity = new Vector3();
        public Vector3 InitialPosition { get; private set; }


        private Vector3 _gravity;

        public override void _Ready()
        {
            _player = GetParent<PlayerEntity>();
            _player.PerceptionEnabled = true;

            _stateMachine = GetNode<StateMachine>("StateMachine");
            Navigation = GetNode<Navigation>(NavigationPath);

            InitialPosition = _player.Transform.origin;
            _gravity =
            Convert.ToSingle(ProjectSettings.GetSetting("physics/3d/default_gravity")) *
            (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

            var orientation = _player.GetPlayerModel().GlobalTransform;
            orientation.origin = new Vector3();
            Orientation = orientation;

            _player.Connect("ready", _stateMachine, "ParentReady");
            _player.GetPerceptionArea().Connect("body_entered", this, nameof(OnPerceptionArea_body_entered));
            _player.GetPerceptionArea().Connect("body_exited", this, nameof(OnPerceptionArea_body_exited));
        }

        public void ShootPlayer(Vector3 shootTarget)
        {
            _player.Shoot(shootTarget);
        }

        public void UpdateRootMotion() => _rootMotion = _player.GetRootMotionTransform();

        public bool HasPlayerTarget() => IsInstanceValid(_playerTarget);

        public bool HasPlayerOnSight(out Godot.Collections.Dictionary col)
        {
            if (!HasPlayerTarget())
            {
                col = null;
                return false;
            }

            var rayOrigin = _player.GlobalTransform.origin;
            var rayTo = _playerTarget.GlobalTransform.origin + Vector3.Up; // Above middle of player.
            col = _player.GetWorld().DirectSpaceState.IntersectRay(rayOrigin, rayTo, new Godot.Collections.Array() { _player });
            return col.Count > 0 && col["collider"] == _playerTarget;
        }

        public PlayerEntity GetPlayerTarget() => _playerTarget;

        public override void _PhysicsProcess(float delta)
        {
            Orientation = Orientation * _rootMotion;

            var hVelocity = Orientation.origin / delta;
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity += _gravity * delta;
            _velocity = _player.MoveAndSlide(_velocity, Vector3.Up);

            _orientation.origin = new Vector3(); // Clear accumulated root motion displacement (was applied to speed).
            _orientation = _orientation.Orthonormalized(); // Orthonormalize orientation.

            var transform = _player.PlayerModel.GlobalTransform;
            transform.basis = Orientation.basis;
            _player.PlayerModel.GlobalTransform = transform;
        }

        private void OnPerceptionArea_body_entered(Node body)
        {
            if (body is PlayerEntity player && player.CurrentPlayer)
            {
                _playerTarget = player;
            }
        }

        private void OnPerceptionArea_body_exited(Node body)
        {
            if (body == _playerTarget)
            {
                _playerTarget = null;
            }
        }
    }
}
