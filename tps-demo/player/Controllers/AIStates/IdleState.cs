using Godot;
using Godot.Collections;
using GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine;

namespace GodotThirdPersonShooterDemoWithCSharp.Player.Controllers.AIStates
{
    public class IdleState: State
    {
        private AIController _aiController;
        
        public override void Ready()
        {
            _aiController = Owner as AIController;
        }

        public override void Enter(Dictionary payload = null)
        {
            _aiController.Player.SetPlayerAimAmount(0);
            _aiController.Player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Walk);
            _aiController.Player.SetPlayerWalkBlendPosition(Vector2.Zero);
        }

        public override void PhysicsProcess(float delta)
        {
            if (_aiController.HasPlayerOnSight(out _))
            {
                StateMachine.TransitionTo("TargetState");
                return;
            }

            _aiController.UpdateRootMotion();
        }
    }
}