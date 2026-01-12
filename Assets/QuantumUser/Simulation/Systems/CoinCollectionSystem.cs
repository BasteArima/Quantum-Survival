namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class CoinCollectionSystem : SystemMainThreadFilter<CoinCollectionSystem.Filter>
    {
        private static readonly FP CollectionRadius = FP._0_50 + FP._0_25;

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Health->IsDead) return;

            FPVector3 playerPos = filter.Transform->Position;

            var coinFilter = frame.Filter<Coin, Transform3D>();
            while (coinFilter.NextUnsafe(out var coinEntity, out var coin, out var coinTransform))
            {
                FPVector3 coinPos = coinTransform->Position;
                FPVector3 diff = coinPos - playerPos;
                diff.Y = FP._0;
                FP distanceSqr = diff.SqrMagnitude;

                if (distanceSqr <= CollectionRadius * CollectionRadius)
                {
                    filter.Player->CoinsCollected += 1;
                    frame.Destroy(coinEntity);
                }
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