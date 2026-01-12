namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class EnemyDamageSystem : SystemMainThreadFilter<EnemyDamageSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();
            if (filter.Health->IsDead)
            {
                return;
            }

            var playerFilter = frame.Filter<Player, Transform3D, Health>();
            while (playerFilter.NextUnsafe(out var playerEntity, out var player, out var playerTransform,
                       out var playerHealth))
            {
                if (playerHealth->IsDead) continue;

                FPVector3 enemyPos = filter.Transform->Position;
                FPVector3 playerPos = playerTransform->Position;
                FPVector3 diff = playerPos - enemyPos;
                diff.Y = FP._0;
                FP distance = diff.Magnitude;

                if (distance <= filter.Enemy->AttackRange)
                {
                    FP damageThisFrame = filter.Enemy->DamagePerSecond * frame.DeltaTime;

                    filter.Enemy->DamageAccumulator += damageThisFrame;

                    if (filter.Enemy->DamageAccumulator >= FP._1)
                    {
                        FP damageToApply = FPMath.Floor(filter.Enemy->DamageAccumulator);
                        filter.Enemy->DamageAccumulator -= damageToApply;

                        playerHealth->Current -= damageToApply;

                        if (playerHealth->Current <= FP._0)
                        {
                            playerHealth->Current = FP._0;
                            playerHealth->IsDead = true;
                            session->IsGameOver = true;
                            frame.Events.OnGameOver();
                        }
                    }
                }
                else
                {
                    filter.Enemy->DamageAccumulator = FP._0;
                }

                break;
            }
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public Enemy* Enemy;
            public Health* Health;
            public Transform3D* Transform;
        }
    }
}