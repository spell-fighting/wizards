using UnityEngine;
using UnityEngine.Networking;

public class NetworkClientDiscoveryController : MonoBehaviour
{
    [SerializeField] private NetworkDiscovery _networkDiscovery;
	
    private void Start ()
    {
        _networkDiscovery.Initialize();
        _networkDiscovery.StartAsClient();
    }

    public void Stop()
    {
        _networkDiscovery.StopBroadcast();
    }
}
