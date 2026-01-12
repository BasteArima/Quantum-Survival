using UnityEngine;
using TMPro;
using Quantum;

public class EnemyHealthDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private string _format = "{0}";
    [SerializeField] private bool _faceCamera = true;

    private Transform _cameraTransform;
    private QuantumEntityView _entityView;
    private QuantumGame _game;

    private void Awake()
    {
        if (_healthText == null)
        {
            _healthText = GetComponent<TMP_Text>();
        }

        _entityView = GetComponentInParent<QuantumEntityView>();
    }

    private void Start()
    {
        if (Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        _game = QuantumRunner.Default?.Game;
    }

    private void LateUpdate()
    {
        if (_cameraTransform == null && Camera.main != null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_faceCamera && _cameraTransform != null)
        {
            transform.rotation = _cameraTransform.rotation;
        }

        UpdateHealthText();
    }

    private void UpdateHealthText()
    {
        if (_healthText == null) return;
        if (_entityView == null || !_entityView.EntityRef.IsValid) return;

        if (_game == null)
        {
            _game = QuantumRunner.Default?.Game;
            if (_game == null) return;
        }

        var frame = _game.Frames.Verified;
        if (frame == null) return;

        if (frame.TryGet<Health>(_entityView.EntityRef, out var health))
        {
            int currentHealth = (int)health.Current.AsFloat;
            _healthText.text = string.Format(_format, currentHealth);

            if (health.IsDead)
            {
                _healthText.enabled = false;
            }
        }
    }
}