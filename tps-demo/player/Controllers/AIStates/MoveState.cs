using Godot;
using Godot.Collections;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers.AIStates
{
    public class MoveState : State
    {
        AIController _aiController;
        private Vector2 _motion = new Vector2();
        private Vector3 _destination;

        private Vector3[] _path;
        private int _currentPath = 0;

        public override void Ready()
        {
            _aiController = Owner as AIController;
        }

        public override void Enter(Dictionary payload = null)
        {
            if (payload == null || !payload.Contains("destination"))
            {
                StateMachine.TransitionTo("IdleState");
                return;
            }
            _destination = (Vector3)payload["destination"];
            _currentPath = 0;
            _path = _aiController
            .Navigation.GetSimplePath(_aiController.Player.GlobalTransform.origin, _destination);

            _aiController.Player.SetPlayerAimAmount(0);
            _aiController.Player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Walk);
        }

        public override void PhysicsProcess(float delta)
        {
            if (_currentPath < _path.Length)
            {
                Vector3 direction = _path[_currentPath] - _aiController.Player.GlobalTransform.origin;
                if (direction.Length() < 1f)
                {
                    _currentPath++;
                }
                else
                {
                    _motion = _motion.LinearInterpolate(Vector2.Right, AIController.MotionInterpolateSpeed * delta);

                    _aiController.Player.SetPlayerWalkBlendPosition(_motion);
                    _aiController.UpdateRootMotion();

                    var t = Transform.Identity.Rotated(Vector3.Up, Mathf.Atan2(direction.x, direction.z));

                    var orientation = _aiController.Orientation;
                    orientation.basis = orientation.basis.Slerp(t.basis, delta * AIController.RotationInterpolateSpeed);
                    _aiController.Orientation = orientation;
                }
            }
            else
            {
                StateMachine.TransitionTo("IdleState");
            }
        }
    }
}