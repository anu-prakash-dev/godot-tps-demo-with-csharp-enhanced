using Godot;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Player
{
    // Temporally till it gotta fixed
    public static class BasisEx
    {
        static public Quat RotationQuat(this Basis b)
        {
            Basis orthonormalizedBasis = b.Orthonormalized();
            float det = orthonormalizedBasis.Determinant();
            if (det < 0)
            {
                // Ensure that the determinant is 1, such that result is a proper
                // rotation matrix which can be represented by Euler angles.
                orthonormalizedBasis = orthonormalizedBasis.Scaled(-Vector3.One);
            }

            return orthonormalizedBasis.Quat();
        }
    }

    public class PlayerEntity : KinematicBody
    {
        private PackedScene BulletScene = ResourceLoader.Load<PackedScene>("res://player/bullet/bullet.tscn");

        private const float CameraMouseRotationSpeed = 0.001f;
        private const float CameraControllerRotationSpeed = 3.0f;
        private const float CameraXRotMin = -40;
        private const float CameraXRotMax = 30;

        private const float DirectionInterpolateSpeed = 1;
        private const float MotionInterpolateSpeed = 10;
        private const float RotationInterpolateSpeed = 10;

        private const float MinAirborneTime = 0.1f;
        private const float JumpSpeed = 5;

        private float _airborneTime = 100;

        private Transform _orientation = Transform.Identity;
        private Transform _orientationBkp = Transform.Identity;
        private Transform _rootMotion = Transform.Identity;
        private Vector2 _motion = new Vector2();
        private Vector3 _velocity = new Vector3();

        private bool _aiming = false;
        private float _cameraXRot = 0.0f;

        private Vector3 _initialPosition;
        private Vector3 _gravity;

        private AnimationTree _animationTree;
        private Spatial _playerModel;
        private Position3D _shootFrom;
        private ColorRect _colorRect;
        private TextureRect _crosshair;
        private Timer _fireCooldown;

        private Spatial _cameraBase;
        private AnimationPlayer _cameraAnimation;
        private Spatial _cameraRot;
        private SpringArm _cameraSpringArm;
        private CameraNoiseShakeEffect _cameraCamera;

        private Node _soundEffects;
        private AudioStreamPlayer _soundEffectJump;
        private AudioStreamPlayer _soundEffectLand;
        private AudioStreamPlayer _soundEffectShoot;

        public PlayerEntity()
        {
            if (!Engine.EditorHint) Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public override void _Ready()
        {
            _animationTree = GetNode<AnimationTree>("AnimationTree");
            _playerModel = GetNode<Spatial>("PlayerModel");
            _shootFrom = _playerModel.GetNode<Position3D>(@"Robot_Skeleton/Skeleton/GunBone/ShootFrom");
            _colorRect = GetNode<ColorRect>("ColorRect");
            _crosshair = GetNode<TextureRect>("Crosshair");
            _fireCooldown = GetNode<Timer>("FireCooldown");

            _cameraBase = GetNode<Spatial>("CameraBase");
            _cameraAnimation = _cameraBase.GetNode<AnimationPlayer>(@"Animation");
            _cameraRot = _cameraBase.GetNode<Spatial>(@"CameraRot");
            _cameraSpringArm = _cameraRot.GetNode<SpringArm>(@"SpringArm");
            _cameraCamera = _cameraSpringArm.GetNode<CameraNoiseShakeEffect>(@"Camera");

            _soundEffects = GetNode<Node>("SoundEffects");
            _soundEffectJump = _soundEffects.GetNode<AudioStreamPlayer>(@"Jump");
            _soundEffectLand = _soundEffects.GetNode<AudioStreamPlayer>(@"Land");
            _soundEffectShoot = _soundEffects.GetNode<AudioStreamPlayer>(@"Shoot");

            _initialPosition = Transform.origin;
            _gravity =
            Convert.ToSingle(ProjectSettings.GetSetting("physics/3d/default_gravity")) *
            (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

            // Pre-initialize orientation transform.
            _orientation = _playerModel.GlobalTransform;
            _orientation.origin = new Vector3();
            _orientationBkp = _orientation;
        }

        public override void _Process(float delta)
        {
            if (Transform.origin.y < -17)
            {
                var color = _colorRect.Modulate;
                color.a = Mathf.Min((-17 - Transform.origin.y) / 15, 1);
                _colorRect.Modulate = color;

                if (Transform.origin.y < -40)
                {
                    color.a = 0;
                    _colorRect.Modulate = color;

                    Transform = new Transform(Transform.basis, _initialPosition);
                }
            }
        }

        public override void _PhysicsProcess(float delta)
        {
            var camera_move = new Vector2(
                    Input.GetActionStrength("view_right") - Input.GetActionStrength("view_left"),
                    Input.GetActionStrength("view_up") - Input.GetActionStrength("view_down"));
            var cameraSpeedThisFrame = delta * CameraControllerRotationSpeed;
            if (_aiming)
                cameraSpeedThisFrame *= 0.5f;
            RotateCamera(camera_move * cameraSpeedThisFrame);

            var motion_target = new Vector2(
                    Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
                    Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward"));

            _motion = _motion.LinearInterpolate(motion_target, MotionInterpolateSpeed * delta);

            var cameraBasis = _cameraRot.GlobalTransform.basis;
            var cameraZ = cameraBasis.z;
            var cameraX = cameraBasis.x;

            cameraZ.y = 0f;
            cameraZ = cameraZ.Normalized();
            cameraX.y = 0;
            cameraX = cameraX.Normalized();

            var currentAim = Input.IsActionPressed("aim");

            if (_aiming != currentAim)
            {
                _aiming = currentAim;
                if (_aiming)
                    _cameraAnimation.Play("shoot");
                else
                    _cameraAnimation.Play("far");
            }

            // Jump/in-air logic.
            _airborneTime += delta;
            if (IsOnFloor())
            {
                if (_airborneTime > 0.5)
                {
                    _soundEffectLand.Play();
                }

                _airborneTime = 0;
            }

            var onAir = _airborneTime > MinAirborneTime;

            if (!onAir && Input.IsActionJustPressed("jump"))
            {
                _velocity.y = JumpSpeed;
                onAir = true;
                // Increase airborne time so next frame on_air is still true
                _airborneTime = MinAirborneTime;
                _animationTree.Set("parameters/state/current", 2);
                _soundEffectJump.Play();
            }

            if (onAir)
            {
                if (_velocity.y > 0)
                    _animationTree.Set("parameters/state/current", 2);
                else
                    _animationTree.Set("parameters/state/current", 3);
            }
            else if (_aiming)
            {
                // Change state to strafe
                _animationTree.Set("parameters/state/current", 0);

                // Change aim according to camera rotation.
                if (_cameraXRot >= 0) // Aim up.
                    _animationTree.Set("parameters/aim/add_amount", -_cameraXRot / Mathf.Deg2Rad(CameraXRotMax));
                else
                    _animationTree.Set("parameters/aim/add_amount", _cameraXRot / Mathf.Deg2Rad(CameraXRotMin));

                // Convert orientation to quaternions for interpolating rotation.
                var qFrom = _orientation.basis.RotationQuat();
                var qTo = _cameraBase.GlobalTransform.basis.RotationQuat();
                // Interpolate current rotation with desired one.
                _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));

                // The animation's forward/backward axis is reversed.
                _animationTree.Set("parameters/strafe/blend_position", new Vector2(_motion.x, -_motion.y));

                _rootMotion = _animationTree.GetRootMotionTransform();

                if (Input.IsActionPressed("shoot") && _fireCooldown.TimeLeft == 0)
                {
                    var shootOrigin = _shootFrom.GlobalTransform.origin;

                    var chPos = _crosshair.RectPosition + _crosshair.RectSize * 0.5f;
                    var rayFrom = _cameraCamera.ProjectRayOrigin(chPos);
                    var rayDir = _cameraCamera.ProjectRayNormal(chPos);

                    Vector3 shootTarget;
                    // 0b11 -> 0b is binary, like 0x is hex; 11 means first and second bytes on.
                    var col = GetWorld().DirectSpaceState.IntersectRay(rayFrom, rayFrom + rayDir * 1000, new Godot.Collections.Array() { this }, 0b11);
                    if (col.Count == 0)
                        shootTarget = rayFrom + rayDir * 1000;
                    else
                        shootTarget = (Vector3)col["position"];
                    var shootDir = (shootTarget - shootOrigin).Normalized();

                    var bullet = (Bullet)BulletScene.Instance();
                    GetParent().AddChild(bullet);
                    bullet.GlobalTransform = new Transform(Basis.Identity, shootOrigin);
                    bullet.Direction = shootDir;
                    bullet.AddCollisionExceptionWith(this);
                    _fireCooldown.Start();
                    _soundEffectShoot.Play();
                    _cameraCamera.AddTrauma(0.35f);
                }
            }
            else // Not in air or aiming, idle
            {
                // Convert orientation to quaternions for interpolating rotation
                var target = cameraX * _motion.x + cameraZ * _motion.y;
                if (target.Length() > 0.001)
                {
                    var qFrom = _orientation.basis.RotationQuat();
                    var qTo = Transform.Identity.LookingAt(target, Vector3.Up).basis.RotationQuat();

                    _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));
                }

                // Aim to zero (no aiming while walking).
                _animationTree.Set("parameters/aim/add_amount", 0);
                // Change state to walk.
                _animationTree.Set("parameters/state/current", 1);
                // Blend position for walk speed based on motion.
                _animationTree.Set("parameters/walk/blend_position", new Vector2(_motion.Length(), 0));

                _rootMotion = _animationTree.GetRootMotionTransform();
            }

            // Apply root motion to orientation.
            _orientation = _orientation * _rootMotion;

            var hVelocity = _orientation.origin / delta;
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity += _gravity * delta;
            _velocity = MoveAndSlide(_velocity, Vector3.Up);

            _orientation.origin = new Vector3(); // Clear accumulated root motion displacement (was applied to speed).
            _orientation = _orientation.Orthonormalized(); // Orthonormalize orientation.

            var transform = _playerModel.GlobalTransform;
            transform.basis = _orientation.basis;
            _playerModel.GlobalTransform = transform;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion _mouseMotion)
            {
                var cameraSpeedThisFrame = CameraMouseRotationSpeed;
                if (_aiming)
                    cameraSpeedThisFrame *= 0.75f;

                RotateCamera(_mouseMotion.Relative * cameraSpeedThisFrame);
            }
        }

        private void RotateCamera(Vector2 move)
        {
            _cameraBase.RotateY(-move.x);
            // After relative transforms, camera needs to be renormalized.
            _cameraBase.Orthonormalize();

            _cameraXRot += move.y;
            _cameraXRot = Mathf.Clamp(_cameraXRot, Mathf.Deg2Rad(CameraXRotMin), Mathf.Deg2Rad(CameraXRotMax));

            var rotation = _cameraRot.Rotation;
            rotation.x = _cameraXRot;
            _cameraRot.Rotation = rotation;
        }
    }
}
