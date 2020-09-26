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

        public enum PlayerTreeStateEnum
        {
            Strafe = 0,
            Walk,
            JumpUp,
            JumpDown
        }

        public enum PlayerSoundEffectEnum
        {
            Jump,
            Land,
            Shoot
        }

        public enum PlayerCameraViewEnum
        {
            Shoot,
            Far
        }


        public const float CameraXRotMin = -40;
        public const float CameraXRotMax = 30;

        public bool CurrentPlayer { get; set; }

        public float CameraXRot { get; private set; } = 0.0f;

        private AnimationTree _animationTree;

        public Spatial PlayerModel { get; private set; }
        public Position3D ShootFrom { get; private set; }
        private ColorRect _colorRect;
        public TextureRect Crosshair { get; private set; }
        private Timer _fireCooldown;

        public Spatial CameraBase { get; private set; }
        private AnimationPlayer _cameraAnimation;
        public Spatial CameraRot { get; private set; }
        private SpringArm _cameraSpringArm;
        public CameraNoiseShakeEffect Camera { get; private set; }

        private Node _soundEffects;
        private AudioStreamPlayer _soundEffectJump;
        private AudioStreamPlayer _soundEffectLand;
        private AudioStreamPlayer _soundEffectShoot;

        public Spatial GetPlayerModel() => GetNode<Spatial>("PlayerModel");

        public override void _Ready()
        {
            _animationTree = GetNode<AnimationTree>("AnimationTree");
            PlayerModel = GetNode<Spatial>("PlayerModel");
            ShootFrom = PlayerModel.GetNode<Position3D>(@"Robot_Skeleton/Skeleton/GunBone/ShootFrom");
            _colorRect = GetNode<ColorRect>("ColorRect");
            Crosshair = GetNode<TextureRect>("Crosshair");
            _fireCooldown = GetNode<Timer>("FireCooldown");

            CameraBase = GetNode<Spatial>("CameraBase");
            _cameraAnimation = CameraBase.GetNode<AnimationPlayer>(@"Animation");
            CameraRot = CameraBase.GetNode<Spatial>(@"CameraRot");
            _cameraSpringArm = CameraRot.GetNode<SpringArm>(@"SpringArm");
            Camera = _cameraSpringArm.GetNode<CameraNoiseShakeEffect>(@"Camera");

            _soundEffects = GetNode<Node>("SoundEffects");
            _soundEffectJump = _soundEffects.GetNode<AudioStreamPlayer>(@"Jump");
            _soundEffectLand = _soundEffects.GetNode<AudioStreamPlayer>(@"Land");
            _soundEffectShoot = _soundEffects.GetNode<AudioStreamPlayer>(@"Shoot");

            if (CurrentPlayer) Camera.Current = true;
        }

        public void Shoot(Vector3 shootTarget)
        {
            var shootOrigin = ShootFrom.GlobalTransform.origin;
            var shootDir = (shootTarget - shootOrigin).Normalized();

            var bullet = (Bullet)BulletScene.Instance();
            // Todo: emit signal instead
            GetParent().AddChild(bullet);
            bullet.GlobalTransform = new Transform(Basis.Identity, shootOrigin);
            bullet.Direction = shootDir;
            bullet.AddCollisionExceptionWith(this);
            StartFireCooldown();
            PlaySoundEffect(PlayerEntity.PlayerSoundEffectEnum.Shoot);
        }


        // public override void _PhysicsProcess(float delta)
        // {
        //     var camera_move = new Vector2(
        //             Input.GetActionStrength("view_right") - Input.GetActionStrength("view_left"),
        //             Input.GetActionStrength("view_up") - Input.GetActionStrength("view_down"));
        //     var cameraSpeedThisFrame = delta * CameraControllerRotationSpeed;
        //     if (_aiming)
        //         cameraSpeedThisFrame *= 0.5f;
        //     RotateCamera(camera_move * cameraSpeedThisFrame);

        //     var motion_target = new Vector2(
        //             Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"),
        //             Input.GetActionStrength("move_back") - Input.GetActionStrength("move_forward"));

        //     _motion = _motion.LinearInterpolate(motion_target, MotionInterpolateSpeed * delta);

        //     var cameraBasis = _cameraRot.GlobalTransform.basis;
        //     var cameraZ = cameraBasis.z;
        //     var cameraX = cameraBasis.x;

        //     cameraZ.y = 0f;
        //     cameraZ = cameraZ.Normalized();
        //     cameraX.y = 0;
        //     cameraX = cameraX.Normalized();

        //     var currentAim = Input.IsActionPressed("aim");

        //     if (_aiming != currentAim)
        //     {
        //         _aiming = currentAim;
        //         if (_aiming)
        //             _cameraAnimation.Play("shoot");
        //         else
        //             _cameraAnimation.Play("far");
        //     }

        //     // Jump/in-air logic.
        //     _airborneTime += delta;
        //     if (IsOnFloor())
        //     {
        //         if (_airborneTime > 0.5)
        //         {
        //             _soundEffectLand.Play();
        //         }

        //         _airborneTime = 0;
        //     }

        //     var onAir = _airborneTime > MinAirborneTime;

        //     if (!onAir && Input.IsActionJustPressed("jump"))
        //     {
        //         _velocity.y = JumpSpeed;
        //         onAir = true;
        //         // Increase airborne time so next frame on_air is still true
        //         _airborneTime = MinAirborneTime;
        //         SetPlayerTreeState(PlayerTreeStateEnum.JumpUp);
        //         // _animationTree.Set("parameters/state/current", 2);
        //         _soundEffectJump.Play();
        //     }

        //     if (onAir)
        //     {
        //         if (_velocity.y > 0)
        //         SetPlayerTreeState(PlayerTreeStateEnum.JumpUp);
        //             // _animationTree.Set("parameters/state/current", 2);
        //         else
        //         SetPlayerTreeState(PlayerTreeStateEnum.JumpDown);
        //             // _animationTree.Set("parameters/state/current", 3);
        //     }
        //     else if (_aiming)
        //     {
        //         // Change state to strafe
        //         SetPlayerTreeState(PlayerTreeStateEnum.Strafe);
        //         // _animationTree.Set("parameters/state/current", 0);

        //         // Change aim according to camera rotation.
        //         if (_cameraXRot >= 0) // Aim up.
        //             SetPlayerAimAmount(-_cameraXRot / Mathf.Deg2Rad(CameraXRotMax));
        //         else
        //             SetPlayerAimAmount(_cameraXRot / Mathf.Deg2Rad(CameraXRotMin));

        //         // Convert orientation to quaternions for interpolating rotation.
        //         var qFrom = _orientation.basis.RotationQuat();
        //         var qTo = _cameraBase.GlobalTransform.basis.RotationQuat();
        //         // Interpolate current rotation with desired one.
        //         _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));

        //         // The animation's forward/backward axis is reversed.
        //         SetPlayerStrafeBlendPosition(new Vector2(_motion.x, -_motion.y));
        //         // Strafe Faster =D
        //         SetPlayerStrafeScale(Input.IsActionPressed("run") ? 1.5f : 1f);

        //         _rootMotion = _animationTree.GetRootMotionTransform();

        //         if (Input.IsActionPressed("shoot") && _fireCooldown.TimeLeft == 0)
        //         {
        //             var shootOrigin = _shootFrom.GlobalTransform.origin;

        //             var chPos = _crosshair.RectPosition + _crosshair.RectSize * 0.5f;
        //             var rayFrom = _cameraCamera.ProjectRayOrigin(chPos);
        //             var rayDir = _cameraCamera.ProjectRayNormal(chPos);

        //             Vector3 shootTarget;
        //             // 0b11 -> 0b is binary, like 0x is hex; 11 means first and second bytes on.
        //             var col = GetWorld().DirectSpaceState.IntersectRay(rayFrom, rayFrom + rayDir * 1000, new Godot.Collections.Array() { this }, 0b11);
        //             if (col.Count == 0)
        //                 shootTarget = rayFrom + rayDir * 1000;
        //             else
        //                 shootTarget = (Vector3)col["position"];
        //             var shootDir = (shootTarget - shootOrigin).Normalized();

        //             var bullet = (Bullet)BulletScene.Instance();
        //             GetParent().AddChild(bullet);
        //             bullet.GlobalTransform = new Transform(Basis.Identity, shootOrigin);
        //             bullet.Direction = shootDir;
        //             bullet.AddCollisionExceptionWith(this);
        //             _fireCooldown.Start();
        //             _soundEffectShoot.Play();
        //             _cameraCamera.AddTrauma(0.35f);
        //         }
        //     }
        //     else // Not in air or aiming, idle
        //     {
        //         // Convert orientation to quaternions for interpolating rotation
        //         var target = cameraX * _motion.x + cameraZ * _motion.y;
        //         if (target.Length() > 0.001)
        //         {
        //             var qFrom = _orientation.basis.RotationQuat();
        //             var qTo = Transform.Identity.LookingAt(target, Vector3.Up).basis.RotationQuat();

        //             _orientation.basis = new Basis(qFrom.Slerp(qTo, delta * RotationInterpolateSpeed));
        //         }

        //         // Aim to zero (no aiming while walking).
        //         SetPlayerAimAmount(0);
        //         // Change state to walk.
        //         // _animationTree.Set("parameters/state/current", 1);
        //         SetPlayerTreeState(PlayerTreeStateEnum.Walk);
        //         // Blend position for walk speed based on motion.
        //         SetPlayerWalkBlendPosition(new Vector2(_motion.Length(), 0));
        //         // Run Faster =D
        //         SetPlayerWalkScale(Input.IsActionPressed("run") ? 1.5f : 1f);

        //         _rootMotion = _animationTree.GetRootMotionTransform();
        //     }

        //     // Apply root motion to orientation.
        //     _orientation = _orientation * _rootMotion;

        //     var hVelocity = _orientation.origin / delta;
        //     _velocity.x = hVelocity.x;
        //     _velocity.z = hVelocity.z;
        //     _velocity += _gravity * delta;
        //     _velocity = MoveAndSlide(_velocity, Vector3.Up);

        //     _orientation.origin = new Vector3(); // Clear accumulated root motion displacement (was applied to speed).
        //     _orientation = _orientation.Orthonormalized(); // Orthonormalize orientation.

        //     var transform = _playerModel.GlobalTransform;
        //     transform.basis = _orientation.basis;
        //     _playerModel.GlobalTransform = transform;
        // }

        public void RotateCamera(Vector2 move)
        {
            CameraBase.RotateY(-move.x);
            // After relative transforms, camera needs to be renormalized.
            CameraBase.Orthonormalize();

            CameraXRot += move.y;
            CameraXRot = Mathf.Clamp(CameraXRot, Mathf.Deg2Rad(CameraXRotMin), Mathf.Deg2Rad(CameraXRotMax));

            var rotation = CameraRot.Rotation;
            rotation.x = CameraXRot;
            CameraRot.Rotation = rotation;
        }

        public void DimVision(float a)
        {
            var color = _colorRect.Modulate;
            color.a = a;
            _colorRect.Modulate = color;
        }

        public void SetPlayerTreeState(PlayerTreeStateEnum state) => _animationTree.Set("parameters/state/current", (int)state);

        public void SetPlayerAimAmount(float amount) => _animationTree.Set("parameters/aim/add_amount", amount);

        public void SetPlayerStrafeBlendPosition(Vector2 position) => _animationTree.Set("parameters/strafe/blend_position", position);

        public void SetPlayerStrafeScale(float scale) => _animationTree.Set("parameters/strafe_scale/scale", scale);

        public void SetPlayerWalkBlendPosition(Vector2 position) => _animationTree.Set("parameters/walk/blend_position", position);

        public void SetPlayerWalkScale(float scale) => _animationTree.Set("parameters/walk_scale/scale", scale);

        public Transform GetRootMotionTransform() => _animationTree.GetRootMotionTransform();

        public bool CanFire() => _fireCooldown.TimeLeft == 0;

        public void StartFireCooldown() => _fireCooldown.Start();

        public void PlaySoundEffect(PlayerSoundEffectEnum soundEffect)
        {
            switch (soundEffect)
            {
                case PlayerSoundEffectEnum.Jump:
                    _soundEffectJump.Play();
                    break;
                case PlayerSoundEffectEnum.Land:
                    _soundEffectLand.Play();
                    break;
                case PlayerSoundEffectEnum.Shoot:
                    _soundEffectShoot.Play();
                    break;
            }
        }

        public void SetCameraView(PlayerCameraViewEnum cameraType)
        {
            switch (cameraType)
            {
                case PlayerCameraViewEnum.Shoot:
                    _cameraAnimation.Play("shoot");
                    break;
                case PlayerCameraViewEnum.Far:
                    _cameraAnimation.Play("far");
                    break;
            }
        }
    }
}
