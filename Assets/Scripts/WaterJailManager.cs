using UnityEngine;

public class WaterJailManager : MonoBehaviour
{
    public int Damage;

    private void Start()
    {
        Destroy(gameObject, 30);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>())
        {
            other.gameObject.GetComponent<PlayerController>().LowerHealthPoint(Damage);
        }
        Destroy(gameObject);
    }
}
