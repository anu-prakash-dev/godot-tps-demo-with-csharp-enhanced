using System;
using Godot;
using Godot.Collections;

namespace GodotThirdPersonShooterDemoWithCSharp.Components.StateMachine
{
    public class State : Node
    {
        protected StateMachine StateMachine { get; private set; }

        public override void _Ready()
        {
            StateMachine = GetParent<StateMachine>();
        }

        public virtual void Ready() { }
        public virtual void Enter(Dictionary payload = null) { }
        public virtual void Exit() { }
        public virtual void InputProcess(InputEvent @event) { }
        public virtual void PhysicsProcess(float delta) { }
    }
}
