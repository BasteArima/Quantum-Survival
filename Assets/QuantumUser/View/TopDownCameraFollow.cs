using Quantum;
using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{ 
    [SerializeField] private float _height = 15f;
    [SerializeField] private Vector3 _offset = Vector3.zero;
    [SerializeField] private float _pitchAngle = 75f;
    [SerializeField] private float _followSpeed = 10f;
    [SerializeField] private bool _smoothFollow = true;

    private bool _initialized;
    private Transform _target;

    private void LateUpdate()
    {
        if (_target == null)
        {
            TryFindPlayer();
        }

        if (_target == null) return;

        Vector3 targetPosition = _target.position + _offset;
        targetPosition.y = _target.position.y + _height;

        float angleRad = (90f - _pitchAngle) * Mathf.Deg2Rad;
        targetPosition.z -= Mathf.Tan(angleRad) * _height * 0.5f;

        if (_smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, _followSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }

        transform.rotation = Quaternion.Euler(_pitchAngle, 0f, 0f);
    }

    private void TryFindPlayer()
    {
        var entityViews = FindObjectsByType<QuantumEntityView>(FindObjectsSortMode.None);

        foreach (var view in entityViews)
        {
            if (view == null || !view.EntityRef.IsValid) continue;

            var game = QuantumRunner.Default?.Game;
            if (game == null) continue;

            var frame = game.Frames.Verified;
            if (frame == null) continue;

            if (frame.Has<Player>(view.EntityRef) && frame.Has<PlayerLink>(view.EntityRef))
            {
                _target = view.transform;
                _initialized = true;
                Debug.Log($"[TopDownCameraFollow] Found player entity: {view.name}");
                break;
            }
        }
    }
}