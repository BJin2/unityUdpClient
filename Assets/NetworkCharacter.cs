using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : MonoBehaviour
{
	public NetworkMan netMan;
	public string network_id { get; private set; }
	public bool controllable { get; private set; }

	private void Start()
	{
		if(controllable)
			InvokeRepeating("NetUpdateTransform", 1,0.03f);
	}
	private void Update()
	{
		Control();
	}
	private void Control()
	{
		if (!controllable)
			return;

		if (Input.GetKey(KeyCode.W))
		{
			transform.Translate(Vector3.forward * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			transform.Translate(-Vector3.forward * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.D))
		{
			transform.Rotate(Vector3.up * Time.deltaTime * 90);
		}
		if (Input.GetKey(KeyCode.A))
		{
			transform.Rotate(-Vector3.up * Time.deltaTime * 90);
		}
	}
	public void SetNetMan(NetworkMan netman)
	{
		netMan = netman;
	}
	public void SetNetworkID(string id)
	{
		network_id = id;
	}
	public void SetControllable(bool control)
	{
		controllable = control;
	}

	public void NetUpdateTransform()
	{
		netMan.SendTransform(transform, network_id);
	}
}
