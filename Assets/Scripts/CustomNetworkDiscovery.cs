using UnityEngine.Networking;

public class CustomNetworkDiscovery : NetworkDiscovery
{
	public override void OnReceivedBroadcast(string fromAddress, string data)
	{
		NetworkManager.singleton.networkAddress = fromAddress;
		NetworkManager.singleton.StartClient();
		FindObjectOfType<NetworkDiscovery>().StopBroadcast();
	}
}
