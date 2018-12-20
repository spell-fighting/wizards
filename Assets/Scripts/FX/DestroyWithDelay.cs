using UnityEngine;

public class DestroyWithDelay : MonoBehaviour {

    [SerializeField] private float _delay = 3;

    private void OnEnable ()
    {
        Invoke(nameof(DestroyThis), _delay);
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
