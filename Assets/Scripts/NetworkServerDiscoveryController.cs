using UnityEngine;
using UnityEngine.Networking;

public class NetworkServerDiscoveryController : MonoBehaviour
{
	[SerializeField] private NetworkDiscovery _networkDiscovery;
	
	private void Start ()
	{
		_networkDiscovery.Initialize();
		_networkDiscovery.StartAsServer();
	}
}
