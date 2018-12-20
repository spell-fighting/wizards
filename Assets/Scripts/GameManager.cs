using UnityEngine;

public enum GameState
{
    Offline,
    Playing,
    Won,
    Lost
}

public class GameManager : MonoBehaviour
{
    private GameState _gameState = GameState.Offline;
    private int _spellLanded;
    private int _spellFired;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void SetState(GameState s)
    {
        if (s == GameState.Playing)
        {
            _spellLanded = 0;
            _spellFired = 0;
        }
        _gameState = s;
    }

    public GameState GetState()
    {
        return _gameState;
    }

    public void IncrementSpellLanded()
    {
        _spellLanded++;
    }
    
    public void IncrementSpellFired()
    {
        _spellFired++;
    }

    public int GetSpellLanded()
    {
        return _spellLanded;
    }
    
    public int GetSpellFired()
    {
        return _spellFired;
    }
}
