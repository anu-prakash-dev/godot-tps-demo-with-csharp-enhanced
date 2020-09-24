using Godot;
using GodotThirdPersonShooterDemoWithCSharp.Player;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Level
{
    public class ShaderCache : Node
    {
        private float _fadeInFrameCounter;

        public override void _Ready()
        {
            GetNode<AudioStreamPlayer3D>("Bullet/ExplosionAudio").UnitDb = -Mathf.Inf;
        }

        public override void _Process(float delta)
        {
            _fadeInFrameCounter -= 1;
            // Fade in progressively to hide artifacts
            if (_fadeInFrameCounter == 20)
                // Hide after a few frames to be sure the shaders compiled.
                GetNode<Bullet>("Bullet").Hide();
            
            if (_fadeInFrameCounter == 0)
                // This node has served its purpose, and now it's time to stop existing.
                QueueFree();
        }
    }
}
