using Photon.Deterministic;
using Quantum;
using UnityEngine;

public class PlayerInputService : MonoBehaviour
{
    private FPVector2 _moveDirection;

    private void OnEnable()
    {
        QuantumCallback.Subscribe(this, (CallbackPollInput callback) => OnPollInput(callback));
    }

    private void Update()
    {
        _moveDirection = new Vector2
        {
            x = UnityEngine.Input.GetAxisRaw("Horizontal"),
            y = UnityEngine.Input.GetAxisRaw("Vertical")
        }.ToFPVector2();
    }

    private void OnPollInput(CallbackPollInput callback)
    {
#if DEBUG
        if (callback.IsInputSet)
        {
            Debug.LogWarning(
                $"{nameof(QuantumDebugInput)}.{nameof(OnPollInput)}: Input was already set by another user script, unsubscribing from the poll input callback. Please delete this component.",
                this);
            QuantumCallback.UnsubscribeListener(this);
            return;
        }
#endif

        var i = new Quantum.Input
        {
            Direction = _moveDirection
        };

        callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }
}