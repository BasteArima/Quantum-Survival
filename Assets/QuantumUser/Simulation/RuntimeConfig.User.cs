namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public partial class RuntimeConfig
    {
        [Header("Player Settings")]
        public FP PlayerMaxHealth = 100;
        public FP PlayerMoveSpeed = 5;
        
        [Header("Enemy Settings")]
        public FP EnemyMaxHealth = 100;
        public FP EnemyMoveSpeed = 3;
        public FP EnemyDamagePerSecond = 5;
        public FP EnemyAttackRange = FP._1_50;
        public FP EnemySpawnInterval = 7;
        public FP EnemySpawnDistance = 15;
        
        [Header("Enemy Prototype")]
        public AssetRef<EntityPrototype> EnemyPrototype;
        
        [Header("Coin Settings")]
        public AssetRef<EntityPrototype> CoinPrototype;
        [Range(0, 100)] public int CoinDropChance = 50;
        
        [Header("Bullet Settings")]
        public AssetRef<EntityPrototype> BulletPrototype;
        public FP BulletSpeed = 15;
        public FP BulletDamage = 25;
        public FP BulletLifetime = 5;
        public FP PlayerShootCooldown = 1;
        
        [Header("Pathfinding")]
        public bool UseFlowField = true;
        
        [Range(20, 100)]
        public int FlowFieldGridSize = 50;
        public FP FlowFieldCellSize = FP._1;
        public FP FlowFieldUpdateInterval = FP._0_50;
    }
}
