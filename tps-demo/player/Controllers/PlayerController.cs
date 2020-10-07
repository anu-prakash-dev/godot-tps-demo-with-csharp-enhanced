using Godot;
using System;

namespace GodotTPSSharpEnhanced.Player.Controllers
{
    public class PlayerController : Node
    {
        private const float CameraMouseRotationSpeed = 0.001f;
        private const float CameraControllerRotationSpeed = 3.0f;
        
        private const float DirectionInterpolateSpeed = 1;
        private const float MotionInterpolateSpeed = 10;
        private const float RotationInterpolateSpeed = 10;

        private const float MinAirborneTime = 0.1f;
        private const float JumpSpeed = 7;

        private float _airborneTime = 100;

        
        private PlayerEntity _player;

        private Transform _orientation = Transform.Identity;
        private Transform _rootMotion = Transform.Identity;
        private Vector2 _motion = new Vector2();
        private Vector3 _velocity = new Vector3();

        private bool _aiming = false;

        private Vector3 _initialPosition;
        private Vector3 _gravity;
        public override void _Ready()
        {
            if (!Engine.EditorHint) Input.SetMouseMode(Input.MouseMode.Captured);

            _player = GetParent<PlayerEntity>();
            _player.CurrentPlayer = true;

            _initialPosition = _player.Transform.origin;
            _gravity =
            Convert.ToSingle(ProjectSettings.GetSetting("physics/3d/default_gravity")) *
            (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

            // Pre-initialize orientation transform.
            _orientation = _player.GetPlayerModel().GlobalTransform;
            _orientation.origin = new Vector3();
        }

        public override void _Process(float delta)
        {
            if (_player.Transform.origin.y < -17)
            {

                if (_player.Transform.origin.y < -40)
                {
                    _player.DimVision(0);
                    _player.Transform = new Transform(_player.Transform.basis, _initialPosition);
                }
                else
                {
                    var a = Mathf.Min((-17 - _player.Transform.origin.y) / 15, 1);
                    _player.DimVision(a);
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
            _player.RotateCamera(camera_move * cameraSpeedThisFrame);

            var motion_target = new Vector2(
                    Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
                    Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward"));

            _motion = _motion.LinearInterpolate(motion_target, MotionInterpolateSpeed * delta);

            var cameraBasis = _player.CameraRot.GlobalTransform.basis;
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
                    _player.SetCameraView(PlayerEntity.PlayerCameraViewEnum.Shoot);
                else
                    _player.SetCameraView(PlayerEntity.PlayerCameraViewEnum.Far);
            }

            // Jump/in-air logic.
            _airborneTime += delta;
            if (_player.IsOnFloor())
            {
                if (_airborneTime > 0.5)
                {
                    _player.PlaySoundEffect(PlayerEntity.PlayerSoundEffectEnum.Land);
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
                _player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.JumpUp);
                // _animationTree.Set("parameters/state/current", 2);
                _player.PlaySoundEffect(PlayerEntity.PlayerSoundEffectEnum.Jump);
            }

            if (onAir)
            {
                if (_velocity.y > 0)
                _player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.JumpUp);
                    // _animationTree.Set("parameters/state/current", 2);
                else
                _player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.JumpDown);
                    // _animationTree.Set("parameters/state/current", 3);
            }
            else if (_aiming)
            {
                // Change state to strafe
                _player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Strafe);
                // _animationTree.Set("parameters/state/current", 0);

                // Change aim according to camera rotation.
                if (_player.CameraXRot >= 0) // Aim up.
                    _player.SetPlayerAimAmount(-_player.CameraXRot / Mathf.Deg2Rad(PlayerEntity.CameraXRotMax));
                else
                    _player.SetPlayerAimAmount(_player.CameraXRot / Mathf.Deg2Rad(PlayerEntity.CameraXRotMin));

                // Convert orientation to quaternions for interpolating rotation.
                var qFrom = _orientation.basis.RotationQuat();
                var qTo = _player.CameraBase.GlobalTransform.basis.RotationQuat();
                // Interpolate current rotation with desired one.
                _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));

                // The animation's forward/backward axis is reversed.
                _player.SetPlayerStrafeBlendPosition(new Vector2(_motion.x, -_motion.y));
                // Strafe Faster =D
                _player.SetPlayerStrafeScale(Input.IsActionPressed("run") ? 1.5f : 1f);

                _rootMotion = _player.GetRootMotionTransform();

                if (Input.IsActionPressed("shoot") && _player.CanFire())
                {
                    var chPos = _player.Crosshair.RectPosition + _player.Crosshair.RectSize * 0.5f;
                    var rayFrom = _player.Camera.ProjectRayOrigin(chPos);
                    var rayDir = _player.Camera.ProjectRayNormal(chPos);

                    Vector3 shootTarget;
                    // 0b11 -> 0b is binary, like 0x is hex; 11 means first and second bytes on.
                    var col = _player.GetWorld().DirectSpaceState.IntersectRay(rayFrom, rayFrom + rayDir * 1000, new Godot.Collections.Array() { this }, 0b11);
                    if (col.Count == 0)
                        shootTarget = rayFrom + rayDir * 1000;
                    else
                        shootTarget = (Vector3)col["position"];

                    _player.Shoot(shootTarget);
                    _player.Camera.AddTrauma(0.35f);
                }
            }
            else // Not in air or aiming, idle
            {
                // Convert orientation to quaternions for interpolating rotation
                var target = cameraX * _motion.x + cameraZ * _motion.y;
                if (target.Length() > 0.001 && motion_target.LengthSquared() > 0)
                {
                    var qFrom = _orientation.basis.RotationQuat();
                    var qTo = Transform.Identity.LookingAt(target, Vector3.Up).basis.RotationQuat();

                    _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));
                }

                // Aim to zero (no aiming while walking).
                _player.SetPlayerAimAmount(0);
                // Change state to walk.
                // _animationTree.Set("parameters/state/current", 1);
                _player.SetPlayerTreeState(PlayerEntity.PlayerTreeStateEnum.Walk);
                // Blend position for walk speed based on motion.
                _player.SetPlayerWalkBlendPosition(new Vector2(_motion.Length(), 0));
                // Run Faster =D
                _player.SetPlayerWalkScale(Input.IsActionPressed("run") ? 1.5f : 1f);

                _rootMotion = _player.GetRootMotionTransform();
            }

            // Apply root motion to orientation.
            _orientation = _orientation * _rootMotion;

            var hVelocity = _orientation.origin / delta;
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity += _gravity * delta;
            _velocity = _player.MoveAndSlide(_velocity, Vector3.Up);

            _orientation.origin = new Vector3(); // Clear accumulated root motion displacement (was applied to speed).
            _orientation = _orientation.Orthonormalized(); // Orthonormalize orientation.

            var transform = _player.PlayerModel.GlobalTransform;
            transform.basis = _orientation.basis;
            _player.PlayerModel.GlobalTransform = transform;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseMotion _mouseMotion)
            {
                var cameraSpeedThisFrame = CameraMouseRotationSpeed;
                if (_aiming)
                    cameraSpeedThisFrame *= 0.75f;

                _player.RotateCamera(_mouseMotion.Relative * cameraSpeedThisFrame);
            }
        }
    }
}
