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
    private Dictionary<string, GameObject> players;

    // Start is called before the first frame update
    void Start()
    {
        players = new Dictionary<string, GameObject>();

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
        public struct receivedColor
        {
            public float R;
            public float G;
            public float B;
        }
        public receivedColor color;        
    }

    [Serializable]
    public class NewPlayer
    {
        
    }

    [Serializable]
    public class GameState
    {
        public Player[] players;
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
        try{
            switch(latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    SpawnPlayers();
                    break;
                case commands.UPDATE:
                    lastestGameState = JsonUtility.FromJson<GameState>(returnData);
                    UpdatePlayers();
                    break;
                case commands.CLIENT_DROPPED:
                    DestroyPlayers();
                    break;
                case commands.CLIENT_LIST:
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e){
            Debug.Log(e.ToString());
        }
        
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers()
    {
        //int numPlayers = lastestGameState.players.Length;
        //Instantiate(playerPrefab, new Vector3(numPlayers, 0, 0), playerPrefab.transform.rotation);
        //Add to dictionary named players(NetworkMan class member dictionary). Key is id from received data
    }

    void UpdatePlayers()
    {
        //find player gameobject using id for lastestGameState
    }

    void DestroyPlayers()
    {

    }
    
    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }
}