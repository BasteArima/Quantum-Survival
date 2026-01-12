using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 180f;
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    
    [SerializeField] private bool _enableBobbing = true;
    [SerializeField] private float _bobAmplitude = 0.1f;
    [SerializeField] private float _bobSpeed = 2f;
    
    private Transform _visualPivot;
    private float _bobTime;
    private float _currentRotation;

    private void Start()
    {
        _bobTime = Random.Range(0f, Mathf.PI * 2f);
        _currentRotation = Random.Range(0f, 360f);
    }

    private void Update()
    {
        _currentRotation += _rotationSpeed * Time.deltaTime;
        
        float bobOffset = 0f;
        if (_enableBobbing)
        {
            _bobTime += Time.deltaTime * _bobSpeed;
            bobOffset = Mathf.Sin(_bobTime) * _bobAmplitude;
        }
        
        transform.localPosition = Vector3.up * bobOffset;
        transform.localRotation = Quaternion.Euler(_rotationAxis * _currentRotation);
    }
}
