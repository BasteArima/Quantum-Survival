namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class PlayerMovementSystem : SystemMainThreadFilter<PlayerMovementSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();

            filter.PhysicsBody->AngularVelocity = FPVector3.Zero;

            Input* input = frame.GetPlayerInput(filter.PlayerLink->Player);

            if (filter.Health->IsDead || session->IsGameOver)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            FPVector2 direction = input->Direction;
            bool hasInput = direction.SqrMagnitude > FP._0;

            if (hasInput)
            {
                direction = direction.Normalized;
                FPVector3 direction3D = new FPVector3(direction.X, 0, direction.Y);

                FPQuaternion currentRotation = filter.Transform->Rotation;
                FPQuaternion targetRotation = FPQuaternion.LookRotation(direction3D);

                FP rotationSpeed = filter.Movement->RotationSpeed > FP._0
                    ? filter.Movement->RotationSpeed
                    : FP._10;
                FP rotationT = rotationSpeed * frame.DeltaTime;

                if (rotationT > FP._1) rotationT = FP._1;

                filter.Transform->Rotation = FPQuaternion.Slerp(currentRotation, targetRotation, rotationT);

                FPVector3 targetVelocity = direction3D * filter.Movement->MoveSpeed;
                filter.PhysicsBody->Velocity = targetVelocity;
            }
            else
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
            }

            FPVector3 finalVelocity = filter.PhysicsBody->Velocity;
            FP maxSpeed = filter.Movement->MaxSpeed > FP._0
                ? filter.Movement->MaxSpeed
                : filter.Movement->MoveSpeed * FP._1_50;

            FP horizontalSpeedSqr = finalVelocity.XZ.SqrMagnitude;
            if (horizontalSpeedSqr > maxSpeed * maxSpeed)
            {
                FPVector2 horizontalVelocity = finalVelocity.XZ.Normalized * maxSpeed;
                filter.PhysicsBody->Velocity = new FPVector3(
                    horizontalVelocity.X,
                    finalVelocity.Y,
                    horizontalVelocity.Y
                );
            }
        }
        
        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public Player* Player;
            public Health* Health;
            public Movement* Movement;
            public PhysicsBody3D* PhysicsBody;
            public Transform3D* Transform;
        }
    }
}