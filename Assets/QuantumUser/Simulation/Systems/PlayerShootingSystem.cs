namespace Quantum
{
    using Photon.Deterministic;
    using System;
    
    public unsafe class PlayerShootingSystem : SystemMainThreadFilter<PlayerShootingSystem.Filter>
    {
        private const int MaxEnemyCandidates = 32;

        private struct EnemyCandidate : IComparable<EnemyCandidate>
        {
            public FPVector3 Position;
            public FP DistanceSqr;

            public int CompareTo(EnemyCandidate other)
            {
                if (DistanceSqr < other.DistanceSqr) return -1;
                if (DistanceSqr > other.DistanceSqr) return 1;
                return 0;
            }
        }

        private EnemyCandidate[] _candidates = new EnemyCandidate[MaxEnemyCandidates];

        public override void Update(Frame frame, ref Filter filter)
        {
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();
            if (session->IsGameOver) return;

            var config = frame.RuntimeConfig;

            if (!config.BulletPrototype.IsValid)
            {
                return;
            }

            if (filter.Health->IsDead)
            {
                return;
            }

            filter.Player->ShootCooldownTimer -= frame.DeltaTime;

            if (filter.Player->ShootCooldownTimer > FP._0)
            {
                return;
            }

            FPVector3 playerPos = filter.Transform->Position;
            FPVector3? targetPos = FindNearestVisibleEnemy(frame, playerPos, filter.Entity);

            if (!targetPos.HasValue)
            {
                return;
            }

            filter.Player->ShootCooldownTimer = config.PlayerShootCooldown;

            FPVector3 direction = (targetPos.Value - playerPos).Normalized;
            SpawnBullet(frame, playerPos, direction, config);
        }

        private FPVector3? FindNearestVisibleEnemy(Frame frame, FPVector3 fromPosition, EntityRef playerEntity)
        {
            int candidateCount = 0;

            var enemyFilter = frame.Filter<Enemy, Transform3D, Health>();
            while (enemyFilter.NextUnsafe(out var entity, out var enemy, out var transform, out var health))
            {
                if (health->IsDead) continue;

                if (candidateCount >= MaxEnemyCandidates) break;

                FPVector3 enemyPos = transform->Position;

                _candidates[candidateCount++] = new EnemyCandidate
                {
                    Position = enemyPos,
                    DistanceSqr = (enemyPos - fromPosition).SqrMagnitude
                };
            }

            if (candidateCount == 0) return null;

            Array.Sort(_candidates, 0, candidateCount);

            for (int i = 0; i < candidateCount; i++)
            {
                FPVector3 enemyPos = _candidates[i].Position;

                if (HasLineOfSight(frame, fromPosition, enemyPos, playerEntity))
                {
                    return enemyPos;
                }
            }

            return null;
        }

        private bool HasLineOfSight(Frame frame, FPVector3 from, FPVector3 to, EntityRef ignoreEntity)
        {
            FPVector3 direction = to - from;
            FP distance = direction.Magnitude;

            if (distance <= FP._0) return true;

            direction = direction / distance;

            var hit = frame.Physics3D.Raycast(
                from,
                direction,
                distance,
                layerMask: -1,
                QueryOptions.HitAll
            );

            if (!hit.HasValue) return true;

            var hitEntity = hit.Value.Entity;

            if (hitEntity == ignoreEntity) return true;

            if (frame.Has<Enemy>(hitEntity)) return true;

            if (frame.Has<Bullet>(hitEntity) || frame.Has<Coin>(hitEntity)) return true;

            return false;
        }

        private void SpawnBullet(Frame frame, FPVector3 position, FPVector3 direction, RuntimeConfig config)
        {
            FPVector3 spawnPos = position + direction * FP._0_50;
            spawnPos.Y = position.Y;

            var bulletEntity = frame.Create(config.BulletPrototype);

            if (frame.Unsafe.TryGetPointer<Transform3D>(bulletEntity, out var transform))
            {
                transform->Position = spawnPos;
                transform->Rotation = FPQuaternion.LookRotation(direction);
            }

            if (frame.Unsafe.TryGetPointer<Bullet>(bulletEntity, out var bullet))
            {
                bullet->Damage = config.BulletDamage;
                bullet->Speed = config.BulletSpeed;
                bullet->Direction = direction;
                bullet->Lifetime = config.BulletLifetime;
                bullet->DestroyOnHit = true;
            }
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public Health* Health;
            public Transform3D* Transform;
        }
    }
}