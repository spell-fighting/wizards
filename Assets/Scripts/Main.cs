 using UnityEngine;
using UnityEngine.Networking;

public class Main : MonoBehaviour
{
    public SceneReference MenuScene;
    
    private void Awake()
    {
        NetworkManager.singleton.ServerChangeScene(MenuScene);
    }
}