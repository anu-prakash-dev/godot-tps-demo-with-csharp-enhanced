using Godot;
using Godot.Collections;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers.AIStates
{
    public class TargetState : State
    {
        AIController _aiController;
        float _totalOutOfSight;

        public override void Ready()
        {
            _aiController = Owner as AIController;
        }

        public override void Enter(Dictionary payload = null)
        {
            _totalOutOfSight = 0;
            _aiController.Player.SetPlayerAimAmount(0);
            _aiController.Player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Strafe);
            _aiController.Player.SetPlayerWalkBlendPosition(Vector2.Zero);
        }

        public override void PhysicsProcess(float delta)
        {
            if (!_aiController.HasPlayerTarget() || _totalOutOfSight > 5f)
            {
                StateMachine.TransitionTo("IdleState");
                return;
            }

            if (_aiController.HasPlayerOnSight())
            {
                _totalOutOfSight = 0;
                var target = _aiController.GetPlayerTarget().GlobalTransform.LookingAt(_aiController.Player.GlobalTransform.origin, Vector3.Up);

                var orientation = _aiController.Orientation;
                orientation.basis = orientation.basis.Slerp(target.basis, delta * AIController.RotationInterpolateSpeed);
                _aiController.Orientation = orientation;
            }
            else
            {
                _totalOutOfSight += delta;
            }

            _aiController.UpdateRootMotion();
        }
    }
}