namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class EnemySpawnSystem : SystemMainThread
    {
        private int _lastSpawnFrame = -1;

        public override void Update(Frame frame)
        {
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();
            if (session->IsGameOver) return;

            var config = frame.RuntimeConfig;

            if (!config.EnemyPrototype.IsValid)
            {
                return;
            }

            int spawnIntervalFrames = (int)(config.EnemySpawnInterval * frame.SessionConfig.UpdateFPS);

            if (frame.Number - _lastSpawnFrame < spawnIntervalFrames)
            {
                return;
            }

            _lastSpawnFrame = frame.Number;

            FPVector3 playerPosition = FPVector3.Zero;
            bool foundAlivePlayer = false;

            var playerFilter = frame.Filter<Player, Transform3D, Health>();
            while (playerFilter.NextUnsafe(out var entity, out var player, out var transform, out var healthComponent))
            {
                if (healthComponent->IsDead)
                {
                    continue;
                }

                playerPosition = transform->Position;
                foundAlivePlayer = true;
                break;
            }

            if (!foundAlivePlayer)
            {
                return;
            }

            if (_lastSpawnFrame < 0)
            {
                _lastSpawnFrame = frame.Number;
                return;
            }

            FPVector3 spawnPosition = GetSpawnPosition(frame, playerPosition, config.EnemySpawnDistance);

            var enemyEntity = frame.Create(config.EnemyPrototype);

            if (frame.Unsafe.TryGetPointer<Transform3D>(enemyEntity, out var enemyTransform))
            {
                enemyTransform->Position = spawnPosition;
                enemyTransform->Rotation = FPQuaternion.Identity;
            }

            if (frame.Unsafe.TryGetPointer<Health>(enemyEntity, out var health))
            {
                health->Max = config.EnemyMaxHealth;
                health->Current = config.EnemyMaxHealth;
                health->IsDead = false;
            }

            if (frame.Unsafe.TryGetPointer<Movement>(enemyEntity, out var movement))
            {
                movement->MoveSpeed = config.EnemyMoveSpeed;
                movement->MaxSpeed = config.EnemyMoveSpeed * FP._1_50;
                movement->RotationSpeed = FP._5;
            }

            if (frame.Unsafe.TryGetPointer<Enemy>(enemyEntity, out var enemy))
            {
                enemy->DamagePerSecond = config.EnemyDamagePerSecond;
                enemy->AttackRange = config.EnemyAttackRange;
                enemy->DamageAccumulator = FP._0;
            }

            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(enemyEntity, out var body))
            {
                body->Drag = FP._0;
                body->AngularDrag = FP._100;
                body->GravityScale = FP._0;
                body->AngularVelocity = FPVector3.Zero;
            }
        }

        private FPVector3 GetSpawnPosition(Frame frame, FPVector3 playerPosition, FP spawnDistance)
        {
            var rng = frame.RNG;
            FP angle = rng->Next() * FP.PiTimes2;
            FP x = playerPosition.X + FPMath.Cos(angle) * spawnDistance;
            FP z = playerPosition.Z + FPMath.Sin(angle) * spawnDistance;
            return new FPVector3(x, playerPosition.Y, z);
        }
    }
}