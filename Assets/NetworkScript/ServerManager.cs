using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    [Header("Settings")]

    [SerializeField] private string characterSelectSceneName = "Character Select";
    [SerializeField] private string gameplaySceneName = "Gameplay";

    public static ServerManager instance;
    private bool gameHasStarted;
    public Dictionary<ulong, ClientData> ClientData { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Another instance of ServerManager already exists! Destroying this one.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("ServerManager instance initialized.");
        }
    }


    public void StartServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
        ClientData = new Dictionary<ulong, ClientData>();
        NetworkManager.Singleton.StartServer();
    }

    public void StartHost()
    {
        try
        {
            Debug.Log("Attempting to start Host...");
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            ClientData = new Dictionary<ulong, ClientData>();

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started successfully!");
            }
            else
            {
                Debug.LogError("Failed to start Host. NetworkManager.StartHost() returned false.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception when starting Host: {ex.Message}\n{ex.StackTrace}");
        }
    }



    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientData.Count >= 8 || gameHasStarted)
        {
            response.Approved = false;
            return;
        }
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId);
        Debug.Log($"Adding Player {request.ClientNetworkId}");
    }
    private void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientData.ContainsKey(clientId))
        {
            if (ClientData.Remove(clientId))
            {
                Debug.Log($"Removed Player {clientId}");
            }
        }
    }

    public void SetCharacter(ulong clientId, int characterId)
    {
        if (ClientData.TryGetValue(clientId, out ClientData data))
        {
            data.characterId = characterId;
        }
    }

    public void StartGame()
    {
        gameHasStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

}
