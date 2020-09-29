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

        public Area _perceptionArea { get; private set; }
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

            _perceptionArea = GetNode<Area>("PerceptionArea");
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
    
        private void OnPerceptionArea_body_entered(Node body)
        {
            GD.Print($"{body.Name} entered");
        }

        private void OnPerceptionArea_body_exited(Node body)
        {
            GD.Print($"{body.Name} exited");
        }
    }
}
