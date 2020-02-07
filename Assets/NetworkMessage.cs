using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessage : MonoBehaviour
{
    public enum Commands
    {
        NEW_CLIENT,
        UPDATE,
        CLIENT_DROPPED,
        CLIENT_LIST,
        OWN_ID
    }

    [Serializable]
    public class State
    {
        public Commands cmd;
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
        public Vector3 position;
        public Quaternion rotation;

        public override string ToString()
        {
            string result = "Player : \n";
            result += "id : " + id + "\n";
            result += "R : " + color.R.ToString() + ", ";
            result += "G : " + color.G.ToString() + ", ";
            result += "B : " + color.B.ToString() + "\n";
            result += "position : " + position.ToString() + "\n";
            result += "rotation : " + rotation.ToString() + "\n";

            return result;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class NewPlayer
    {
        public Player player;
    }

    [Serializable]
    public class ConnectedPlayer
    {
        public Player[] players;
    }

    [Serializable]
    public class DisconnectedPlayer
    {
        public Player[] players;
    }

    [Serializable]
    public class UpdatedPlayer
    {
        public Player[] players;
    }
}
