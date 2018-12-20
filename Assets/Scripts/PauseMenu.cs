using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private static bool _gameIsPaused;

    public GameObject PauseMenuUi;

    private void Start()
    {
        Resume();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        PauseMenuUi.SetActive(false);
        _gameIsPaused = false;
    }

    private void Pause()
    {
        PauseMenuUi.SetActive(true);
        _gameIsPaused = true;
    }

    public void LoadMenu()
    {
        NetworkManager.singleton.StopHost();
        SceneManager.LoadScene("Menu");
    }
    
    public void Quit()
    {
        Application.Quit();
    }
}