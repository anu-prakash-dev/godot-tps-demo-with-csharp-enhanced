using Godot;
using Godot.Collections;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine
{
    public class StateMachine : Node
    {
        [Export]
        private NodePath _entry_state = null;

        private State _state;

        public override void _Ready()
        {
            if (_entry_state != null && HasNode(_entry_state))
            {
                _state = GetNode<State>(_entry_state);
            }
            else if (GetChildCount() > 0)
            {
                _state = GetChild<State>(0);
            }
            else
            {
                GD.PushWarning("Entry state required");
            }

            foreach (State node in GetChildren())
            {
                // Controller
                node.Owner = GetParent();
            }
        }
        
        public override void _Input(InputEvent @event) => _state?.InputProcess(@event);
        
        public override void _PhysicsProcess(float delta) => _state?.PhysicsProcess(delta);
        
        public void TransitionTo(string stateName, Dictionary payload = null)
        {
            if (!HasNode(stateName))
            {
                GD.PushWarning($"Invalid state {stateName}");
                return;
            }
            GD.Print($"TransitionTo {stateName}");
            _state?.Exit();
            _state = GetNode<State>(stateName);
            _state?.Enter(payload);
        }
       
        public void ParentReady()
        {
            foreach (State node in GetChildren())
            {
                node.Ready();
            }
            _state?.Enter();
        }
    }
}
