using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public UdpClient udp;
    public GameObject playerPrefab;
    private string ownID;

    private Dictionary<string, GameObject> connectedPlayers;
    private Queue<NetworkMessage.Player> newPlayers;
    private Queue<string> disconnectedPlayers;

    public NetworkMessage.UpdatedPlayer latestUpdates;

    // Start is called before the first frame update
    void Start()
    {
        connectedPlayers = new Dictionary<string, GameObject>();
        connectedPlayers.Clear();
        newPlayers = new Queue<NetworkMessage.Player>();
        newPlayers.Clear();
        disconnectedPlayers = new Queue<string>();
        disconnectedPlayers.Clear();

        udp = new UdpClient();
        
        udp.Connect("3.209.132.25", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy()
    {
        udp.Dispose();
        connectedPlayers.Clear();
        disconnectedPlayers.Clear();
        newPlayers.Clear();
    }

    void OnReceived(IAsyncResult result)
    {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);

        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref source);
        
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);
        Debug.Log("Got this: " + returnData);

        NetworkMessage.State latestState = JsonUtility.FromJson<NetworkMessage.State>(returnData);
        try
        {
            switch(latestState.cmd)
            {
                case NetworkMessage.Commands.NEW_CLIENT:
                    Debug.Log("New Client");
                    NetworkMessage.NewPlayer newPlayer = JsonUtility.FromJson<NetworkMessage.NewPlayer>(returnData);
                    newPlayers.Enqueue(newPlayer.player);
                    break;
                case NetworkMessage.Commands.UPDATE:
                    Debug.Log("Update");
                    latestUpdates = JsonUtility.FromJson<NetworkMessage.UpdatedPlayer>(returnData);
                    break;
                case NetworkMessage.Commands.CLIENT_DROPPED:
                    Debug.Log("Client Dropped");
                    NetworkMessage.DisconnectedPlayer dropped = JsonUtility.FromJson<NetworkMessage.DisconnectedPlayer>(returnData);
                    foreach (var p in dropped.players)
                        disconnectedPlayers.Enqueue(p.id);
                    break;
                case NetworkMessage.Commands.CLIENT_LIST:
                    Debug.Log("Client List");

                    NetworkMessage.ConnectedPlayer clientList = JsonUtility.FromJson<NetworkMessage.ConnectedPlayer>(returnData);
                    if (clientList.players.Length > 0)
                    {
                        foreach (var p in clientList.players)
                            newPlayers.Enqueue(p);
                    }
                    break;
                case NetworkMessage.Commands.OWN_ID:
                    Debug.Log("Connected");
                    NetworkMessage.NewPlayer player = JsonUtility.FromJson<NetworkMessage.NewPlayer>(returnData);
                    ownID = player.player.id;
                    newPlayers.Enqueue(player.player);
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers(NetworkMessage.Player player)
    {
        if (connectedPlayers.ContainsKey(player.id))
        {
            Debug.Log("Already exists");
            return;
        }

        //Spawn actual game object
        GameObject newPlayerGameObject = Instantiate(playerPrefab);

        //Every players will have different color from the server
        Color newColor = new Color(player.color.R, player.color.G, player.color.B);
        newPlayerGameObject.GetComponent<Renderer>().material.SetColor("_Color", newColor);

        //Add network id
        NetworkCharacter character = newPlayerGameObject.GetComponent<NetworkCharacter>();
        character.SetNetMan(this);
        character.SetNetworkID(player.id);
        character.SetControllable(player.id == ownID);
        connectedPlayers.Add(player.id, newPlayerGameObject);
    }

    void UpdatePlayers()
    {
        for (int i = 0; i < connectedPlayers.Count; i++)
        {
            if (latestUpdates.players.Length <= 0)
                return;

            if (i >= latestUpdates.players.Length)
                return;
            NetworkMessage.Player p = latestUpdates.players[i];
            if (p.id == ownID || !connectedPlayers.ContainsKey(p.id))
                continue;

            GameObject player = connectedPlayers[p.id];
            player.transform.position = p.position;
            player.transform.rotation = p.rotation;
        }
    }

    void DestroyPlayers(string networkID)
    {
        Debug.Log("Destroyed player");
        Destroy(connectedPlayers[networkID]);
        connectedPlayers.Remove(networkID);
    }
    
    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }
    public void SendTransform(Transform t, string id)
    {
        NetworkMessage.PlayerData p = new NetworkMessage.PlayerData();
        p.position = t.position;
        p.rotation = t.rotation;
        string jsonString = JsonUtility.ToJson(p);
        Debug.Log(jsonString);
        Byte[] sendBytes = Encoding.ASCII.GetBytes(jsonString);
        udp.Send(sendBytes, sendBytes.Length);
    }

    private void Update()
    {
        if (newPlayers.Count > 0)
        {
            for (int i = 0; i < newPlayers.Count; i++)
            {
                NetworkMessage.Player newPlayer = newPlayers.Dequeue();
                SpawnPlayers(newPlayer);
            }
        }
        if (disconnectedPlayers.Count > 0)
        {
            for (int i = 0; i < disconnectedPlayers.Count; i++)
            {
                string diconnectedPlayerID = disconnectedPlayers.Dequeue();
                DestroyPlayers(diconnectedPlayerID);
            }
        }
        UpdatePlayers();
    }
}