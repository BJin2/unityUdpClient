using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
	public string Network_ID { get; private set; }
	public void SetNetworkID(string id)
	{
		Network_ID = id;
	}
}
