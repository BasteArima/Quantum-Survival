namespace Quantum
{
    using Photon.Deterministic;
    
    public unsafe class GameSessionSystem : SystemMainThread, ISignalOnComponentAdded<GameSession>
    {
        public override void OnInit(Frame frame)
        {
            if (!frame.Unsafe.TryGetPointerSingleton<GameSession>(out var session))
            {
                var e = frame.Create();
                frame.Add<GameSession>(e); 
            }
        }

        public void OnAdded(Frame frame, EntityRef entity, GameSession* component)
        {
            component->IsGameOver = false;
        }
        
        public override void Update(Frame frame)
        {
            var input = frame.GetPlayerCommand(0);
            if (input is RestartCommand)
            {
                ResetGame(frame);
            }
        }

        private void ResetGame(Frame frame)
        {
            var session = frame.Unsafe.GetPointerSingleton<GameSession>();
            session->IsGameOver = false;

            foreach (var (entity, enemy) in frame.Unsafe.GetComponentBlockIterator<Enemy>())
            {
                frame.Destroy(entity);
            }

            foreach (var (entity, bullet) in frame.Unsafe.GetComponentBlockIterator<Bullet>())
            {
                frame.Destroy(entity);
            }

            foreach (var (entity, coin) in frame.Unsafe.GetComponentBlockIterator<Coin>())
            {
                frame.Destroy(entity);
            }

            foreach (var (entity, player) in frame.Unsafe.GetComponentBlockIterator<Player>())
            {
                var health = frame.Unsafe.GetPointer<Health>(entity);

                health->Current = frame.RuntimeConfig.PlayerMaxHealth;
                player->CoinsCollected = 0;
                health->IsDead = false;

                if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var t))
                {
                    t->Position = FPVector3.Zero;
                }
            }
        }
    }
}