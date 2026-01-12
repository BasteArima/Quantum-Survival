namespace Quantum
{
    using System.Collections.Generic;

    public static partial class DeterministicSystemSetup
    {
        static partial void AddSystemsUser(ICollection<SystemBase> systems, RuntimeConfig gameConfig, SimulationConfig simulationConfig, SystemsConfig systemsConfig)
        {
            systems.Add(new GameSessionSystem());
            
            systems.Add(new PlayerSpawnSystem());
            systems.Add(new EnemySpawnSystem());
            
            systems.Add(new PlayerMovementSystem());
            systems.Add(new EnemyMovementSystem());
            
            systems.Add(new FlowFieldSystem());
            systems.Add(new FlowFieldMovementSystem());
            
            systems.Add(new EnemyDamageSystem());
            systems.Add(new PlayerShootingSystem());
            systems.Add(new BulletSystem());
            
            systems.Add(new CoinCollectionSystem());
            
            systems.Add(new DeathSystem());
        }
    }
}
