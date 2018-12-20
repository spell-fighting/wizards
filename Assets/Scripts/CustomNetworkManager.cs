using System;
using UnityEngine;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    [HideInInspector] public GameObject Player;
    [HideInInspector] public GameObject Enemy;

    private GameObject InstantiatePlayer()
    {
        if (Player)
        {
            throw new Exception("SpawnPlayer has been called twice.");
        }

        var spawnPoint = GameObject.Find("Player Spawn Point").transform;
        
        Player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);

        return Player;
    }

    private GameObject InstantiateEnemy()
    {
        if (Enemy)
        {
            throw new Exception("SpawnEnemy has been called twice.");
        }
        
        var spawnPoint = GameObject.Find("Enemy Spawn Point").transform;
        
        Enemy = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        
        return Enemy;
    }
    
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject.Find("GameManager").GetComponent<GameManager>().SetState(GameState.Playing);
        NetworkServer.AddPlayerForConnection(conn, !Player ? InstantiatePlayer() : InstantiateEnemy(), playerControllerId);
    }
}