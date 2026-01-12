namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class EnemyMovementSystem : SystemMainThreadFilter<EnemyMovementSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            if (frame.RuntimeConfig.UseFlowField) return;
            
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();

            if (filter.Health->IsDead || session->IsGameOver)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            filter.PhysicsBody->AngularVelocity = FPVector3.Zero;

            FPVector3 playerPosition = FPVector3.Zero;
            bool foundPlayer = false;

            var playerFilter = frame.Filter<Player, Transform3D, Health>();
            while (playerFilter.NextUnsafe(out var playerEntity, out var player, out var playerTransform,
                       out var playerHealth))
            {
                if (playerHealth->IsDead) continue;

                playerPosition = playerTransform->Position;
                foundPlayer = true;
                break;
            }

            if (!foundPlayer)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            FPVector3 enemyPosition = filter.Transform->Position;
            FPVector3 toPlayer = playerPosition - enemyPosition;

            toPlayer.Y = FP._0;
            FP distanceToPlayer = toPlayer.Magnitude;

            FP attackRange = filter.Enemy->AttackRange;
            if (distanceToPlayer <= attackRange)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;

                if (distanceToPlayer > FP._0_10)
                {
                    FPVector3 direction = toPlayer.Normalized;
                    FPQuaternion targetRotation = FPQuaternion.LookRotation(direction);
                    filter.Transform->Rotation = targetRotation;
                }

                return;
            }

            FPVector3 moveDirection = toPlayer.Normalized;

            FPQuaternion currentRotation = filter.Transform->Rotation;
            FPQuaternion targetRot = FPQuaternion.LookRotation(moveDirection);

            FP rotationSpeed = filter.Movement->RotationSpeed > FP._0
                ? filter.Movement->RotationSpeed
                : FP._5;
            FP rotationT = rotationSpeed * frame.DeltaTime;
            if (rotationT > FP._1) rotationT = FP._1;

            filter.Transform->Rotation = FPQuaternion.Slerp(currentRotation, targetRot, rotationT);

            FPVector3 velocity = moveDirection * filter.Movement->MoveSpeed;
            filter.PhysicsBody->Velocity = velocity;
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public Enemy* Enemy;
            public Health* Health;
            public Movement* Movement;
            public Transform3D* Transform;
            public PhysicsBody3D* PhysicsBody;
        }
    }
}