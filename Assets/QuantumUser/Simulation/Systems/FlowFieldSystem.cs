namespace Quantum
{
    using Photon.Deterministic;
    using System.Collections.Generic;

    public unsafe class FlowFieldSystem : SystemMainThread, ISignalOnComponentAdded<FlowFieldData>
    {
        // Смещения соседей (N, NE, E, SE, S, SW, W, NW)
        private static readonly int[] DirX = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static readonly int[] DirZ = { 1, 1, 0, -1, -1, -1, 0, 1 };

        public static readonly FPVector2[] DirectionLookup = new FPVector2[]
        {
            new FPVector2(0, 1), // 0: N
            new FPVector2(1, 1).Normalized, // 1: NE
            new FPVector2(1, 0), // 2: E
            new FPVector2(1, -1).Normalized, // 3: SE
            new FPVector2(0, -1), // 4: S
            new FPVector2(-1, -1).Normalized, // 5: SW
            new FPVector2(-1, 0), // 6: W
            new FPVector2(-1, 1).Normalized // 7: NW
        };

        private int[] _costField;
        private Queue<int> _openList;
        private int _lastAllocatedSize = -1;

        public override void OnInit(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointerSingleton<FlowFieldData>(out var data))
            {
                var e = frame.Create();
                frame.Add<FlowFieldData>(e);
            }
        }

        public void OnAdded(Frame f, EntityRef entity, FlowFieldData* component)
        {
            component->IsValid = false;
            component->LastUpdateTick = -9999;
        }

        private void InitBuffers(int gridSize)
        {
            if (_lastAllocatedSize == gridSize && _costField != null) return;

            int maxCells = gridSize * gridSize;
            _costField = new int[maxCells];
            _openList = new Queue<int>(maxCells);
            _lastAllocatedSize = gridSize;
        }

        public override void Update(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointerSingleton<GameSession>(out var session) || session->IsGameOver) return;
            if (!frame.Unsafe.TryGetPointerSingleton<FlowFieldData>(out var flowData)) return;

            var config = frame.RuntimeConfig;
            if (!config.UseFlowField) return;

            FPVector3 playerPos = FPVector3.Zero;
            bool foundPlayer = false;

            var playerFilter = frame.Filter<Player, Transform3D, Health>();
            while (playerFilter.NextUnsafe(out _, out _, out var transform, out var health))
            {
                if (health->IsDead) continue;
                playerPos = transform->Position;
                foundPlayer = true;
                break;
            }

            if (!foundPlayer)
            {
                flowData->IsValid = false;
                return;
            }

            int updateInterval = (int)(config.FlowFieldUpdateInterval * frame.SessionConfig.UpdateFPS).AsInt;
            if (updateInterval < 1) updateInterval = 1;

            int framesSince = frame.Number - flowData->LastUpdateTick;
            FP targetMovedSqr = (playerPos - flowData->TargetPosition).SqrMagnitude;

            bool needsUpdate = !flowData->IsValid
                               || framesSince >= updateInterval
                               || targetMovedSqr > FP._1;

            if (!needsUpdate) return;

            InitBuffers(config.FlowFieldGridSize);
            CalculateFlowField(frame, flowData, playerPos, config);
        }

        private void CalculateFlowField(Frame frame, FlowFieldData* flowData, FPVector3 targetPos, RuntimeConfig config)
        {
            int gridSize = config.FlowFieldGridSize;
            FP cellSize = config.FlowFieldCellSize;

            FP halfGridWorld = (FP)gridSize * cellSize * FP._0_50;
            FPVector3 gridOrigin = new FPVector3(
                targetPos.X - halfGridWorld,
                targetPos.Y,
                targetPos.Z - halfGridWorld
            );

            flowData->GridOrigin = gridOrigin;
            flowData->GridSize = gridSize;
            flowData->CellSize = cellSize;
            flowData->TargetPosition = targetPos;
            flowData->LastUpdateTick = frame.Number;

            int totalCells = gridSize * gridSize;

            for (int i = 0; i < totalCells; i++) _costField[i] = int.MaxValue;

            int targetX = WorldToGrid(targetPos.X, gridOrigin.X, cellSize, gridSize);
            int targetZ = WorldToGrid(targetPos.Z, gridOrigin.Z, cellSize, gridSize);
            int targetIndex = targetZ * gridSize + targetX;

            _openList.Clear();
            _costField[targetIndex] = 0;
            _openList.Enqueue(targetIndex);

            while (_openList.Count > 0)
            {
                int idx = _openList.Dequeue();
                int cx = idx % gridSize;
                int cz = idx / gridSize;
                int currentCost = _costField[idx];

                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + DirX[d];
                    int nz = cz + DirZ[d];

                    if (nx < 0 || nx >= gridSize || nz < 0 || nz >= gridSize) continue;

                    int nIdx = nz * gridSize + nx;

                    if (_costField[nIdx] < int.MaxValue) continue;

                    FPVector3 cellCenter = GridToWorld(nx, nz, gridOrigin, cellSize);
                    if (IsBlocked(frame, cellCenter, cellSize))
                    {
                        _costField[nIdx] = int.MaxValue - 1;
                        continue;
                    }

                    int moveCost = (DirX[d] == 0 || DirZ[d] == 0) ? 10 : 14;

                    _costField[nIdx] = currentCost + moveCost;
                    _openList.Enqueue(nIdx);
                }
            }

            for (int i = 0; i < totalCells; i++)
            {
                if (_costField[i] >= int.MaxValue - 1)
                {
                    flowData->Grid[i] = 255;
                    continue;
                }

                int myCost = _costField[i];
                int bestCost = myCost;
                int bestDirIndex = -1;

                int cx = i % gridSize;
                int cz = i / gridSize;

                for (int d = 0; d < 8; d++)
                {
                    int nx = cx + DirX[d];
                    int nz = cz + DirZ[d];

                    if (nx < 0 || nx >= gridSize || nz < 0 || nz >= gridSize) continue;

                    int nCost = _costField[nz * gridSize + nx];
                    if (nCost < bestCost)
                    {
                        bestCost = nCost;
                        bestDirIndex = d;
                    }
                }

                if (bestDirIndex != -1)
                {
                    flowData->Grid[i] = (byte)bestDirIndex;
                }
                else
                {
                    flowData->Grid[i] = 255;
                }
            }

            flowData->IsValid = true;
        }

        private int WorldToGrid(FP worldCoord, FP originCoord, FP cellSize, int gridSize)
        {
            int val = ((worldCoord - originCoord) / cellSize).AsInt;
            if (val < 0) return 0;
            if (val >= gridSize) return gridSize - 1;
            return val;
        }

        private FPVector3 GridToWorld(int x, int z, FPVector3 origin, FP cellSize)
        {
            return new FPVector3(
                origin.X + ((FP)x + FP._0_50) * cellSize,
                origin.Y,
                origin.Z + ((FP)z + FP._0_50) * cellSize
            );
        }

        private bool IsBlocked(Frame frame, FPVector3 pos, FP cellSize)
        {
            var hits = frame.Physics3D.OverlapShape(
                pos,
                FPQuaternion.Identity,
                Shape3D.CreateSphere(cellSize * FP._0_33),
                layerMask: -1
            );

            for (int i = 0; i < hits.Count; i++)
            {
                var e = hits[i].Entity;
                if (frame.Has<Enemy>(e) || frame.Has<Player>(e) || frame.Has<Bullet>(e) || frame.Has<Coin>(e)) continue;
                return true;
            }

            return false;
        }
    }
}