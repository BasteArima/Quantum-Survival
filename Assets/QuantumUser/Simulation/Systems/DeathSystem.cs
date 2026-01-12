namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class DeathSystem : SystemMainThreadFilter<DeathSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Health->IsDead) return;

            if (frame.Has<Player>(filter.Entity))
            {
                if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(filter.Entity, out var body))
                {
                    body->Velocity = FPVector3.Zero;
                    body->AngularVelocity = FPVector3.Zero;
                }

                return;
            }

            if (frame.Has<Enemy>(filter.Entity))
            {
                TrySpawnCoin(frame, filter.Transform->Position);
                frame.Destroy(filter.Entity);
            }
        }

        private void TrySpawnCoin(Frame frame, FPVector3 position)
        {
            var config = frame.RuntimeConfig;

            if (!config.CoinPrototype.IsValid) return;

            FP roll = frame.RNG->Next();
            if (roll > config.CoinDropChance) return;

            var coinEntity = frame.Create(config.CoinPrototype);

            if (frame.Unsafe.TryGetPointer<Transform3D>(coinEntity, out var transform))
            {
                position.Y = FP._0_50;
                transform->Position = position;
                transform->Rotation = FPQuaternion.Identity;
            }
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public Health* Health;
            public Transform3D* Transform;
        }
    }
}