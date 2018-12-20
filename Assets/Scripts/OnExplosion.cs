using UnityEngine;

public class OnExplosion : MonoBehaviour
{
    public int Damage;
    private bool _triggered;

    private void Start()
    {
        Destroy(GetComponent<SphereCollider>(), 2.0f);
    }

    private void OnTriggerStay(Collider other)
    {
        OnTriggerEnter(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_triggered && other.gameObject.GetComponent<PlayerController>())
        {
            other.gameObject.GetComponent<PlayerController>().LowerHealthPoint(Damage);

            _triggered = true;
        }
    }
}