using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
    }

    public void LoadScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void Host()
    {
        NetworkManager.singleton.StartHost();
    }

    public void Join(InputField input)
    {
        NetworkManager.singleton.networkAddress = input.text;
        NetworkManager.singleton.StartClient();
    }
}