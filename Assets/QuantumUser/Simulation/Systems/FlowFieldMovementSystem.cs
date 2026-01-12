namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class FlowFieldMovementSystem : SystemMainThreadFilter<FlowFieldMovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Enemy* Enemy;
            public Health* Health;
            public Movement* Movement;
            public PhysicsBody3D* PhysicsBody;
            public Transform3D* Transform;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            filter.PhysicsBody->AngularVelocity = FPVector3.Zero;

            if (filter.Health->IsDead)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            if (frame.Unsafe.TryGetPointerSingleton<GameSession>(out var session) && session->IsGameOver)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            if (!frame.Unsafe.TryGetPointerSingleton<FlowFieldData>(out var flowData) || !flowData->IsValid)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            FPVector3 enemyPos = filter.Transform->Position;

            FP distToTargetSqr = (flowData->TargetPosition - enemyPos).SqrMagnitude;
            FP attackRangeSqr = filter.Enemy->AttackRange * filter.Enemy->AttackRange;

            if (distToTargetSqr <= attackRangeSqr)
            {
                filter.PhysicsBody->Velocity = FPVector3.Zero;
                return;
            }

            FPVector2 flowDir = SampleFlowField(enemyPos, flowData);

            if (flowDir.SqrMagnitude < FP._0_01)
            {
                FPVector3 directDir = (flowData->TargetPosition - enemyPos);
                if (directDir.SqrMagnitude > FP.Epsilon)
                {
                    directDir = directDir.Normalized;
                    flowDir = new FPVector2(directDir.X, directDir.Z);
                }
            }

            FPVector3 velocity = new FPVector3(flowDir.X, FP._0, flowDir.Y) * filter.Movement->MoveSpeed;

            velocity.Y = filter.PhysicsBody->Velocity.Y;
            filter.PhysicsBody->Velocity = velocity;

            if (velocity.XZ.SqrMagnitude > FP._0_01)
            {
                FPVector3 lookDir = new FPVector3(velocity.X, 0, velocity.Z).Normalized;
                FPQuaternion targetRot = FPQuaternion.LookRotation(lookDir);

                FP rotT = filter.Movement->RotationSpeed * frame.DeltaTime;
                filter.Transform->Rotation = FPQuaternion.Slerp(filter.Transform->Rotation, targetRot, rotT);
            }
        }

        private FPVector2 SampleFlowField(FPVector3 worldPos, FlowFieldData* flowData)
        {
            FP fx = (worldPos.X - flowData->GridOrigin.X) / flowData->CellSize;
            FP fz = (worldPos.Z - flowData->GridOrigin.Z) / flowData->CellSize;

            int gridSize = flowData->GridSize;
            if (gridSize <= 1) return FPVector2.Zero;

            int x0 = fx.AsInt;
            int z0 = fz.AsInt;

            if (x0 < 0) x0 = 0;
            if (x0 > gridSize - 2) x0 = gridSize - 2;
            if (z0 < 0) z0 = 0;
            if (z0 > gridSize - 2) z0 = gridSize - 2;

            int x1 = x0 + 1;
            int z1 = z0 + 1;

            // Веса для Lerp
            FP tx = fx - (FP)x0;
            FP tz = fz - (FP)z0;

            // Clamp weights 0..1
            if (tx < 0) tx = 0;
            else if (tx > 1) tx = 1;
            if (tz < 0) tz = 0;
            else if (tz > 1) tz = 1;

            // Получаем векторы из 4 соседних клеток
            FPVector2 v00 = GetVectorFromByte(flowData, z0 * gridSize + x0);
            FPVector2 v10 = GetVectorFromByte(flowData, z0 * gridSize + x1);
            FPVector2 v01 = GetVectorFromByte(flowData, z1 * gridSize + x0);
            FPVector2 v11 = GetVectorFromByte(flowData, z1 * gridSize + x1);

            // Интерполируем
            FPVector2 top = FPVector2.Lerp(v00, v10, tx);
            FPVector2 bottom = FPVector2.Lerp(v01, v11, tx);
            return FPVector2.Lerp(top, bottom, tz).Normalized;
        }

        private FPVector2 GetVectorFromByte(FlowFieldData* data, int index)
        {
            byte dirIndex = data->Grid[index];

            // 255 = Invalid
            if (dirIndex >= 8) return FPVector2.Zero;

            // Берем готовый вектор из статического массива другой системы
            return FlowFieldSystem.DirectionLookup[dirIndex];
        }
    }
}