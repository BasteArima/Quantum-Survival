using UnityEngine;
using Quantum;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private UnityEngine.UI.Button _restartButton;

    private void Start()
    {
        _gameOverPanel.SetActive(false);
        QuantumEvent.Subscribe<EventOnGameOver>(this, OnGameOver);
        _restartButton.onClick.AddListener(OnRestartClicked);
    }
    
    private void OnGameOver(EventOnGameOver e)
    {
        _gameOverPanel.SetActive(true);
    }

    private void OnRestartClicked()
    {
        _gameOverPanel.SetActive(false);
        var command = new RestartCommand();
        QuantumRunner.Default.Game.SendCommand(command);
    }
}
