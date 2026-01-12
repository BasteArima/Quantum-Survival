namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime)
        {
            var playerData = frame.GetPlayerData(player);
            var playerEntity = frame.Create(playerData.PlayerAvatar);

            PlayerLink* playerLink = frame.Unsafe.GetPointer<PlayerLink>(playerEntity);
            playerLink->Player = player;

            var config = frame.RuntimeConfig;
            Health* health = frame.Unsafe.GetPointer<Health>(playerEntity);
            health->Max = config.PlayerMaxHealth;
            health->Current = config.PlayerMaxHealth;
            health->IsDead = false;

            Movement* movement = frame.Unsafe.GetPointer<Movement>(playerEntity);
            movement->MoveSpeed = config.PlayerMoveSpeed;
            movement->MaxSpeed = config.PlayerMoveSpeed * FP._1_50;
            movement->RotationSpeed = FP._10;

            Player* playerComp = frame.Unsafe.GetPointer<Player>(playerEntity);
            playerComp->CoinsCollected = 0;

            if (frame.Unsafe.TryGetPointer<PhysicsBody3D>(playerEntity, out var body))
            {
                body->Drag = FP._0;
                body->AngularDrag = FP._100;
                body->GravityScale = FP._0;
                body->AngularVelocity = FPVector3.Zero;
            }

            if (frame.Unsafe.TryGetPointer<Transform3D>(playerEntity, out var transform))
            {
                transform->Rotation = FPQuaternion.Identity;
            }
        }
    }
}