using Godot;
using Godot.Collections;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers.AIStates
{
    public class SampleState : State
    {
        AIController _aiController;
        float _totalDelta;
        int _x;

        public override void Ready()
        {
            _aiController = Owner as AIController;
        }

        public override void Enter(Dictionary payload = null)
        {
            _aiController.Player.SetPlayerAimAmount(0);
            _aiController.Player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Walk);
        }

        public override void PhysicsProcess(float delta)
        {
            _aiController.Player.SetPlayerWalkBlendPosition(new Vector2(1, 0));
            
            _aiController.UpdateRootMotion();

            _totalDelta += delta;
            if (_totalDelta > 2f)
            {
                _totalDelta = 0f;
                _x++;
            }

            var orientation = _aiController.Orientation;
            orientation.basis = orientation.basis.Slerp(Transform.Identity.Rotated(Vector3.Up, Mathf.Rad2Deg(90 * _x)).basis, delta * AIController.RotationInterpolateSpeed);
            _aiController.Orientation = orientation;
        }
    }
}