using UnityEngine;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private Text _gameOverTitle;

    [SerializeField] private Text _spellFired;

    [SerializeField] private Text _spellLanded;

    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_gameOverTitle)
        {
            switch (_gameManager.GetState())
            {
                case GameState.Won:
                    _gameOverTitle.text = "You won !";
                    break;
                case GameState.Lost:
                    _gameOverTitle.text = "You lost !";
                    break;
                case GameState.Playing:
                    _gameOverTitle.text = "You were disconnected, it seems...";
                    break;
                default:
                    _gameOverTitle.text = "Unexpected error";
                    break;
            }
        }

        if (_spellFired && _spellLanded)
        {
            _spellFired.text = "Spell Fired : " + _gameManager.GetSpellFired();
            _spellLanded.text = "Spell Landed : " + _gameManager.GetSpellLanded();
        }

        _gameManager.SetState(GameState.Offline);
    }
}