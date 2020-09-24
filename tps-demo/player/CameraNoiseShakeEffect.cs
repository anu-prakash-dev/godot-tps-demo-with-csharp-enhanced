using Godot;
using System;

public class CameraNoiseShakeEffect : Camera
{
    // Constant values of the effect.
    private const float Speed = 1.0f;
    private const float DecayRate = 1.5f;
    private const float MaxYaw = 0.05f;
    private const float MaxPitch = 0.05f;
    private const float MaxRoll = 0.1f;
    private const float MaxTrauma = 0.6f;

    // Default values.
    private Vector3 _startRotation;
    private float _trauma = 0.0f;
    private float _time = 0.0f;
    private OpenSimplexNoise _noise = new OpenSimplexNoise();
    private int _noiseSeed = Convert.ToInt32(GD.Randi() % Int32.MaxValue);

    public override void _Ready()
    {
        _noise.Seed = _noiseSeed;
        _noise.Octaves = 1;
        _noise.Period = 256.0f;
        _noise.Persistence = 0.5f;
        _noise.Lacunarity = 1.0f;

        // This variable is reset if the camera position is changed by other scripts,
        // such as when zooming in/out or focusing on a different position.
        // This should NOT be done when the camera shake is happening.
        _startRotation = Rotation;
    }

    public override void _Process(float delta)
    {
        if (_trauma > 0.0f)
        {
            DecayTrauma(delta);
            ApplyShake(delta);
        }
    }

    // Add trauma to start/continue the shake.
    public void AddTrauma(float amount)
    {
        _trauma = Mathf.Min(_trauma + amount, MaxTrauma);
    }

    // Decay the trauma effect over time.
    private void DecayTrauma(float delta)
    {
        var change = DecayRate * delta;
        _trauma = Mathf.Max(_trauma - change, 0.0f);
    }

    // Apply the random shake accoring to delta time.
    private void ApplyShake(float delta)
    {
        // Using a magic number here to get a pleasing effect at SPEED 1.0.
        _time += delta * Speed * 5000.0f;
        var shake = _trauma * _trauma;
        var yaw = MaxYaw * shake * GetNoiseValue(_noiseSeed, _time);
        var pitch = MaxPitch * shake * GetNoiseValue(_noiseSeed + 1, _time);
        var roll = MaxRoll * shake * GetNoiseValue(_noiseSeed + 2, _time);

        Rotation = _startRotation + new Vector3(pitch, yaw, roll);
    }

    // Return a random float in range(-1, 1) using OpenSimplex noise.
    private float GetNoiseValue(int seedValue, float t)
    {
        _noise.Seed = seedValue;
        return _noise.GetNoise1d(t);
    }
}
