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
    private Dictionary<string, GameObject> connectedPlayers;
    private Queue<string> newPlayers;
    

    // Start is called before the first frame update
    void Start()
    {
        connectedPlayers = new Dictionary<string, GameObject>();
        connectedPlayers.Clear();
        newPlayers = new Queue<string>();
        newPlayers.Clear();

        udp = new UdpClient();
        
        udp.Connect("54.90.62.70", 12345);

        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");
      
        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy()
    {
        udp.Dispose();
        connectedPlayers.Clear();
        newPlayers.Clear();
    }


    public enum commands
    {
        NEW_CLIENT,
        UPDATE,
        CLIENT_DROPPED,
        CLIENT_LIST
    };
    
    [Serializable]
    public class Message
    {
        public commands cmd;
    }
    
    [Serializable]
    public class Player
    {
        public string id;
        [Serializable]
        public struct receivedColor
        {
            public float R;
            public float G;
            public float B;
        }
        public receivedColor color;
        public override string ToString()
        {
            string result = "Player : \n";
            result += "id : " + id + "\n";
            result += "R : " + color.R.ToString() + ", ";
            result += "G : " + color.G.ToString() + ", ";
            result += "B : " + color.B.ToString() + "\n";

            return result;
        }
    }

    [Serializable]
    public class NewPlayer
    {
        [Serializable]
        public class SingleClient
        {
            public string id;
            public override string ToString()
            {
                return id;
            }
        }

        public SingleClient[] player;
        public override string ToString()
        {
            string result = "player : \n";
            foreach (var p in player)
            {
                result += p.ToString() + "\n";
            }

            return result;
        }
    }

    [Serializable]
    public class GameState
    {
        public Player[] players;

        public override string ToString()
        {
            string result = "players : \n";
            foreach (var p in players)
            {
                result += p.ToString();
            }
            return result;
        }
    }

    public Message latestMessage;
    public GameState lastestGameState;
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
        
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try
        {
            switch(latestMessage.cmd)
            {
                
                case commands.NEW_CLIENT:
                    Debug.Log("New Client");
                    NewPlayer newPlayer = JsonUtility.FromJson<NewPlayer>(returnData);
                    foreach(var p in newPlayer.player)
                        newPlayers.Enqueue(p.ToString());
                    Debug.Log(newPlayer.ToString());
                    break;
                case commands.UPDATE:
                    Debug.Log("Update");
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    Debug.Log(lastestGameState.ToString());
                    
                    break;
                case commands.CLIENT_DROPPED:
                    DestroyPlayers();
                    break;
                case commands.CLIENT_LIST:
                    Debug.Log("Client List");
                    NewPlayer clientList = JsonUtility.FromJson<NewPlayer>(returnData);
                    foreach (var p in clientList.player)
                        newPlayers.Enqueue(p.ToString());
                    Debug.Log(clientList.ToString());
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

    void SpawnPlayers(string networkID)
    {
        if (connectedPlayers.ContainsKey(networkID))
        {
            Debug.Log("Already exists");
            return;
        }
        Debug.Log("Spawn new player");
        //Spawn actual game object
        GameObject newPlayerGameObject = (GameObject)Instantiate(playerPrefab);

        //Add network id
        newPlayerGameObject.AddComponent<PlayerInfo>();
        newPlayerGameObject.GetComponent<PlayerInfo>().SetNetworkID(networkID);
        connectedPlayers.Add(networkID, newPlayerGameObject);
        Debug.Log("Spawned " + networkID);
    }

    void UpdatePlayers()
    {
        int numPlayer = connectedPlayers.Count;
        if (numPlayer < lastestGameState.players.Length)
        {
            Debug.Log("Some player is not spawned yet");
        }
        else if (numPlayer > lastestGameState.players.Length)
        {
            numPlayer = lastestGameState.players.Length;
            Debug.Log("Some player is not destroyed");
        }

        for (int i = 0; i < numPlayer; i++)
        {
            
            Player p = lastestGameState.players[i];

            if (!connectedPlayers.ContainsKey(p.id))
            {
                Debug.Log(p.id + " not spawned yet");
                continue;
            }

            //player existance is guaranteed
            Debug.Log("Changing color " + p.id);
            GameObject player = connectedPlayers[p.id];
            Color newColor = new Color(p.color.R, p.color.G, p.color.B);
            player.GetComponent<Renderer>().material.SetColor("_Color", newColor);
            Debug.Log("Changed color" + p.id);
        }
    }

    void DestroyPlayers()
    {

    }
    
    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    private void Update()
    {
        if (newPlayers.Count > 0)
        {
            for (int i = 0; i < newPlayers.Count; i++)
            {
                string newPlayer = newPlayers.Dequeue();
                SpawnPlayers(newPlayer);
            }
        }
        UpdatePlayers();
    }
}