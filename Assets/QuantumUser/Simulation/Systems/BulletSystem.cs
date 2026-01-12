namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class BulletSystem : SystemMainThreadFilter<BulletSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            filter.Bullet->Lifetime -= frame.DeltaTime;

            if (filter.Bullet->Lifetime <= FP._0)
            {
                frame.Destroy(filter.Entity);
                return;
            }

            FPVector3 movement = filter.Bullet->Direction * filter.Bullet->Speed * frame.DeltaTime;
            FPVector3 newPosition = filter.Transform->Position + movement;

            filter.Transform->Position = newPosition;

            CheckCollisions(frame, ref filter, newPosition);
        }

        private void CheckCollisions(Frame frame, ref Filter filter, FPVector3 position)
        {
            var hits = frame.Physics3D.OverlapShape(
                position,
                FPQuaternion.Identity,
                Shape3D.CreateSphere(FP._0_25),
                layerMask: -1
            );

            for (int i = 0; i < hits.Count; i++)
            {
                var hit = hits[i];

                if (hit.Entity == filter.Entity) continue;

                if (frame.Has<Bullet>(hit.Entity) ||
                    frame.Has<Player>(hit.Entity) ||
                    frame.Has<Coin>(hit.Entity))
                    continue;

                if (frame.Has<Enemy>(hit.Entity))
                {
                    if (frame.Unsafe.TryGetPointer<Health>(hit.Entity, out var health))
                    {
                        if (!health->IsDead)
                        {
                            health->Current -= filter.Bullet->Damage;

                            if (health->Current <= FP._0)
                            {
                                health->Current = FP._0;
                                health->IsDead = true;
                            }

                            if (filter.Bullet->DestroyOnHit)
                            {
                                frame.Destroy(filter.Entity);
                                return;
                            }
                        }
                    }

                    continue;
                }

                if (filter.Bullet->DestroyOnHit)
                {
                    frame.Destroy(filter.Entity);
                    return;
                }
            }
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public Bullet* Bullet;
            public Transform3D* Transform;
        }
    }
}