using UnityEngine;
using UnityEditor;
using Quantum;

/// <summary>
/// Editor utilities to automatically setup EntityPrototypes in RuntimeConfig.
/// </summary>
public static class SetupPrototypes
{
    [MenuItem("Quantum/Setup All Prototypes")]
    public static void SetupAll()
    {
        SetupEnemyPrototype();
        SetupCoinPrototype();
        SetupBulletPrototype();
        
        // Mark scene dirty and prompt to save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("Don't forget to save the scene! (Ctrl+S)");
    }
    
    [MenuItem("Quantum/Setup Enemy Prototype")]
    public static void SetupEnemyPrototype()
    {
        SetupPrototype("EnemyViewEntityPrototype", "EnemyPrototype", 
            (config, proto) => config.EnemyPrototype = proto);
    }
    
    [MenuItem("Quantum/Setup Coin Prototype")]
    public static void SetupCoinPrototype()
    {
        SetupPrototype("CoinViewEntityPrototype", "CoinPrototype", 
            (config, proto) => config.CoinPrototype = proto);
    }
    
    [MenuItem("Quantum/Setup Bullet Prototype")]
    public static void SetupBulletPrototype()
    {
        SetupPrototype("BulletViewEntityPrototype", "BulletPrototype", 
            (config, proto) => config.BulletPrototype = proto);
    }
    
    private static void SetupPrototype(string assetName, string propertyName, 
        System.Action<RuntimeConfig, EntityPrototype> setter)
    {
        // Find prototype asset
        string[] guids = AssetDatabase.FindAssets($"{assetName} t:EntityPrototype");
        if (guids.Length == 0)
        {
            Debug.LogWarning($"{assetName} not found!");
            return;
        }
        
        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
        var prototype = AssetDatabase.LoadAssetAtPath<EntityPrototype>(assetPath);
        
        if (prototype == null)
        {
            Debug.LogError($"Failed to load EntityPrototype from {assetPath}");
            return;
        }
        
        int updated = 0;
        
        // Update QuantumRunnerLocalDebug
        var runner = Object.FindFirstObjectByType<QuantumRunnerLocalDebug>();
        if (runner != null)
        {
            setter(runner.RuntimeConfig, prototype);
            EditorUtility.SetDirty(runner);
            updated++;
        }
        
        // Update QuantumStartUIConnection (if exists)
        var startUI = Object.FindFirstObjectByType<QuantumStartUIConnection>();
        if (startUI != null)
        {
            setter(startUI.RuntimeConfig, prototype);
            EditorUtility.SetDirty(startUI);
            updated++;
        }
        
        if (updated > 0)
        {
            Debug.Log($"✅ {propertyName} set to: {prototype.name} ({updated} config(s) updated)");
        }
        else
        {
            Debug.LogError("No QuantumRunnerLocalDebug or QuantumStartUIConnection found!");
        }
    }
}
