using Godot;
using GodotThirdPersonShooterDemoWithCSharp.Player;
using System;

namespace GodotThirdPersonShooterDemoWithCSharp.Enemies
{
    public class RedRobot : KinematicBody
    {
        private enum StateEnum
        {
            Approach = 0,
            Aim = 1,
            Shooting = 2
        }

        private const float PlayerAimToleranceDegrees = 15f;

        private const float ShootWait = 6.0f;
        private const float AimTime = 1f;

        private const float AimPrepareTime = 0.5f;
        private const float BlendAimSpeed = 0.05f;

        private StateEnum _state = StateEnum.Approach;

        private float _shootCountdown = AimTime;
        private float _aimCountdown = AimTime;
        private float _aimPreparing = AimPrepareTime;
        private int _health = 5;
        private bool _dead = false;
        private bool _test_shoot = false;

        private PlayerEntity _player = null;
        private Vector3 _velocity = new Vector3();
        private Transform _orientation = Transform.Identity;

        private AnimationTree _animationTree;
        private AnimationPlayer _shootAnimation;

        private Spatial _model;
        private BoneAttachment _rayFrom;
        private MeshInstance _rayMesh;
        private CPUParticles _explosionParticles;

        private AudioStreamPlayer3D _explosionSound;
        private AudioStreamPlayer3D _hitSound;

        private Spatial _death;
        private RigidBody _shield1;
        private RigidBody _shield2;
        private RigidBody _shield3;

        private Vector3 _gravity;


        public override void _Ready()
        {
            _animationTree = GetNode<AnimationTree>("AnimationTree");
            _shootAnimation = GetNode<AnimationPlayer>("ShootAnimation");

            _model = GetNode<Spatial>("RedRobotModel");
            _rayFrom = _model.GetNode<BoneAttachment>(@"Armature/Skeleton/RayFrom");
            _rayMesh = _rayFrom.GetNode<MeshInstance>(@"RayMesh");
            _explosionParticles = _rayFrom.GetNode<CPUParticles>(@"ExplosionParticles");
            _explosionSound = GetNode<AudioStreamPlayer3D>("SoundEffects/Explosion");
            _hitSound = GetNode<AudioStreamPlayer3D>("SoundEffects/Hit");
            _death = GetNode<Spatial>("Death");
            _shield1 = _death.GetNode<RigidBody>(@"PartShield1");
            _shield2 = _death.GetNode<RigidBody>(@"PartShield2");
            _shield3 = _death.GetNode<RigidBody>(@"PartShield3");

            _gravity =
            Convert.ToSingle(ProjectSettings.GetSetting("physics/3d/default_gravity")) *
            (Vector3)ProjectSettings.GetSetting("physics/3d/default_gravity_vector");

            _orientation = GlobalTransform;
            _orientation.origin = new Vector3();
        }

        public override void _PhysicsProcess(float delta)
        {
            if (_test_shoot)
            {
                Shoot();
                _test_shoot = false;
            }

            if (_dead) return;

            if (_player == null)
            {
                _animationTree.Set("arameters/state/current", 0); //Go idle.
                return;
            }

            if (_state == StateEnum.Approach)
            {
                if (_aimPreparing > 0.0)
                {
                    _aimPreparing -= delta;
                    if (_aimPreparing < 0.0)
                        _aimPreparing = 0;
                    _animationTree.Set("parameters/aiming/blend_amount", _aimPreparing / AimPrepareTime);
                }

                var toPlayerLocal = GlobalTransform.XformInv(_player.GlobalTransform.origin);
                // The front of the robot is +Z, and atan2 is zero at +X, so we need to use the Z for the X parameter (second one).
                var angleToPlayer = Mathf.Atan2(toPlayerLocal.x, toPlayerLocal.z);
                var tolerance = Mathf.Deg2Rad(PlayerAimToleranceDegrees);
                if (angleToPlayer > tolerance)
                {
                    _animationTree.Set("parameters/state/current", 1);
                }
                else if (angleToPlayer < -tolerance)
                {
                    _animationTree.Set("parameters/state/current", 2);
                }
                else
                {
                    _animationTree.Set("parameters/state/current", 3);
                    // Facing player, try to shoot.

                    _shootCountdown -= delta;
                    if (_shootCountdown < 0.0)
                    {
                        // See if player can be killed because in they're sight.
                        var rayOrigin = _rayFrom.GlobalTransform.origin;
                        var rayTo = _player.GlobalTransform.origin + Vector3.Up; // Above middle of player.
                        var col = GetWorld().DirectSpaceState.IntersectRay(rayOrigin, rayTo, new Godot.Collections.Array() { this });
                        if (col.Count > 0 && col["collider"] is PlayerEntity)
                        {
                            _state = StateEnum.Aim;
                            _aimCountdown = AimTime;
                            _aimPreparing = 0;
                            _animationTree.Set("parameters/state/current", 0);
                        }
                        else
                        {
                            _shootCountdown = ShootWait;
                        }
                    }
                }

            }
            else if (_state == StateEnum.Aim || _state == StateEnum.Shooting)
            {
                if (_aimPreparing < AimPrepareTime)
                {
                    _aimPreparing += delta;
                    if (_aimPreparing > AimPrepareTime)
                        _aimPreparing = AimPrepareTime;

                }

                _animationTree.Set("parameters/aiming/blend_amount", Mathf.Clamp(_aimPreparing / AimPrepareTime, 0, 1));
                _aimCountdown -= delta;
                if (_aimCountdown < 0 && _state == StateEnum.Aim)
                {
                    var rayOrigin = _rayFrom.GlobalTransform.origin;
                    var rayTo = _player.GlobalTransform.origin + Vector3.Up; // Above middle of player.
                    var col = GetWorld().DirectSpaceState.IntersectRay(rayOrigin, rayTo, new Godot.Collections.Array() { this });
                    if (col.Count > 0 && col["collider"] is PlayerEntity)
                    {
                        _state = StateEnum.Shooting;
                        _shootAnimation.Play("shoot");
                    }
                    else
                        ResumeApproach();
                }

                if (_animationTree.Active)
                {
                    var toCannonLocal = _rayMesh.GlobalTransform.XformInv(_player.GlobalTransform.origin + Vector3.Up);
                    var hAngle = Mathf.Rad2Deg(Mathf.Atan2(toCannonLocal.x, -toCannonLocal.z));
                    var vAngle = Mathf.Rad2Deg(Mathf.Atan2(toCannonLocal.y, -toCannonLocal.z));

                    var blendPos = (Vector2)_animationTree.Get("parameters/aim/blend_position");
                    var hMotion = BlendAimSpeed * delta * -hAngle;
                    blendPos.x += hMotion;
                    blendPos.x = Mathf.Clamp(blendPos.x, -1, 1);

                    var vMotion = BlendAimSpeed * delta * vAngle;
                    blendPos.y += vMotion;
                    blendPos.y = Mathf.Clamp(blendPos.y, -1, 1);

                    _animationTree.Set("parameters/aim/blend_position", blendPos);
                }
            }

            _orientation *= _animationTree.GetRootMotionTransform();

            var hVelocity = _orientation.origin / delta;
            _velocity.x = hVelocity.x;
            _velocity.z = hVelocity.z;
            _velocity += _gravity * delta;
            _velocity = MoveAndSlide(_velocity, Vector3.Up);

            _orientation.origin = new Vector3();
            _orientation = _orientation.Orthonormalized();

            var gt = GlobalTransform;
            gt.basis = _orientation.basis;
            GlobalTransform = gt;
        }

        private void ResumeApproach()
        {
            _state = StateEnum.Approach;
            _aimPreparing = AimPrepareTime;
            _shootCountdown = ShootWait;
        }

        public void Hit()
        {
            if (_dead) return;

            _animationTree.Set($"parameters/hit{GD.Randi() % 3 + 1}/active", true);
            _hitSound.Play();
            _health -= 1;
            if (_health == 0)
            {
                _dead = true;
                var baseXf = GlobalTransform.basis;
                _animationTree.Active = false;
                _model.Visible = false;
                _death.Visible = true;
                GetNode<CollisionShape>("CollisionShape").Disabled = true;
                _death.GetNode<CPUParticles>("Particles").Emitting = true;
                _shield1.GetNode<CollisionShape>("Col1").Disabled = false;
                _shield1.GetNode<CollisionShape>("Col2").Disabled = false;
                _shield1.Mode = RigidBody.ModeEnum.Rigid;
                _shield2.GetNode<CollisionShape>("Col1").Disabled = false;
                _shield2.GetNode<CollisionShape>("Col2").Disabled = false;
                _shield2.Mode = RigidBody.ModeEnum.Rigid;
                _shield3.GetNode<CollisionShape>("Col1").Disabled = false;
                _shield3.GetNode<CollisionShape>("Col2").Disabled = false;
                _shield3.Mode = RigidBody.ModeEnum.Rigid;

                _shield2.LinearVelocity = 3 * (Vector3.Up + baseXf.x).Normalized();
                _shield3.LinearVelocity = 3 * (Vector3.Up).Normalized();
                _shield1.LinearVelocity = 3 * (Vector3.Up - baseXf.x).Normalized();
                _shield2.AngularVelocity = (new Vector3(GD.Randf(), GD.Randf(), GD.Randf()).Normalized() * 2 - Vector3.One) * 10;
                _shield1.AngularVelocity = (new Vector3(GD.Randf(), GD.Randf(), GD.Randf()).Normalized() * 2 - Vector3.One) * 10;
                _shield3.AngularVelocity = (new Vector3(GD.Randf(), GD.Randf(), GD.Randf()).Normalized() * 2 - Vector3.One) * 10;
                _explosionSound.Play();
            }
        }

        private void Shoot()
        {
            var gt = _rayFrom.GlobalTransform;
            var rayOrigin = _rayFrom.GlobalTransform.origin;
            var rayDir = -gt.basis.z;
            float maxDist = 1000f;

            var col = GetWorld().DirectSpaceState.IntersectRay(rayOrigin, rayOrigin + rayDir * maxDist, new Godot.Collections.Array() { this });
            if (col.Count > 0)
            {
                maxDist = rayOrigin.DistanceTo((Vector3)col["position"]);
                if (col["collider"] is PlayerEntity)
                {
                    // Kill
                }
            }

            // Clip ray in shader
            ((ShaderMaterial)_rayMesh.GetSurfaceMaterial(0)).SetShaderParam("clip", maxDist);
            // Position explosion.
            var t = _explosionParticles.Transform;
            t.origin.z = -maxDist;
            _explosionParticles.Transform = t;
        }

        private void ShootCheck() => _test_shoot = true;

        private void _on_area_body_entered(PlayerEntity body)
        {
            _player = body;
            _shootCountdown = ShootWait;
        }

        private void _on_area_body_exited(PlayerEntity body)
        {
            _player = null;
        }
    }
}
