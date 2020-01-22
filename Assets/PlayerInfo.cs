using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
	/*/
	public string Network_ID { get; private set; }
	/*/
	//To show network id on editor
	public string Network_ID;
	//*/
	public void SetNetworkID(string id)
	{
		Network_ID = id;
	}
}
