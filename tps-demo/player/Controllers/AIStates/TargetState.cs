using Godot;
using Godot.Collections;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers.AIStates
{
    public class TargetState : State
    {

        AIController _aiController;
        float _cooldown;
        float _totalOutOfSight;
        private Vector3? _playerTargetLastOrigin;

        public override void Ready()
        {
            _aiController = Owner as AIController;
        }

        public override void Enter(Dictionary payload = null)
        {
            _playerTargetLastOrigin = null;
            _totalOutOfSight = 0;
            _aiController.Player.SetPlayerAimAmount(0);
            _aiController.Player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Strafe);
            _aiController.Player.SetPlayerWalkBlendPosition(Vector2.Zero);
            _cooldown = 0.500f;
        }

        public override void PhysicsProcess(float delta)
        {
            _cooldown -= delta;
            if (_cooldown <= 0) _cooldown = 0;

            if (!_aiController.HasPlayerTarget() || _totalOutOfSight > 1f)
            {
                if (_playerTargetLastOrigin.HasValue)
                {
                    StateMachine.TransitionTo("ChaseState",
                    new Dictionary() {
                    { "playerTargetLastOrigin", _playerTargetLastOrigin }
                    });
                }
                else
                {
                    StateMachine.TransitionTo("IdleState");
                }
                return;
            }

            if (_aiController.HasPlayerOnSight(out Dictionary col))
            {
                _totalOutOfSight = 0;
                _playerTargetLastOrigin = _aiController.GetPlayerTarget().GlobalTransform.origin;
                var target = _aiController.GetPlayerTarget().GlobalTransform.LookingAt(_aiController.Player.GlobalTransform.origin, Vector3.Up);

                var orientation = _aiController.Orientation;
                orientation.basis = orientation.basis.Slerp(target.basis, delta * AIController.RotationInterpolateSpeed);
                _aiController.Orientation = orientation;

                if (_cooldown == 0 && _aiController.Player.CanFire())
                    _aiController.ShootPlayer((Vector3)col["position"]);
            }
            else
            {
                _totalOutOfSight += delta;
            }

            _aiController.UpdateRootMotion();
        }
    }
}