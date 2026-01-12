using UnityEngine;
using TMPro;
using Quantum;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _coinsText;
    [SerializeField] private string _healthFormat = "HP: {0}";
    [SerializeField] private string _coinsFormat = "Coins: {0}";
    
    private QuantumGame _game;

    private void Start()
    {
        _game = QuantumRunner.Default?.Game;
    }

    private void Update()
    {
        if (_game == null)
        {
            _game = QuantumRunner.Default?.Game;
            if (_game == null) return;
        }
        
        var frame = _game.Frames.Verified;
        if (frame == null) return;
        
        var playerFilter = frame.Filter<Player, Health>();
        while (playerFilter.Next(out var entity, out var player, out var health))
        {
            if (_healthText != null)
            {
                int currentHealth = (int)health.Current.AsFloat;
                _healthText.text = string.Format(_healthFormat, currentHealth);
            }
            
            if (_coinsText != null)
            {
                _coinsText.text = string.Format(_coinsFormat, player.CoinsCollected);
            }
            
            break;
        }
    }
}
