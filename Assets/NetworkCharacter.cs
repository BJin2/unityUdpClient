using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkCharacter : MonoBehaviour
{
	public NetworkMan netMan;
	public string network_id { get; private set; }
	public bool controllable { get; private set; }
	private Vector3 position;
	private Vector3 euler;
	private Quaternion rotation;

	private void Start()
	{
		if(controllable)
			InvokeRepeating("NetUpdateTransform", 1,0.03f);
		position = transform.position;
		rotation = transform.rotation;
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
			position += transform.TransformDirection(Vector3.forward) * Time.deltaTime;
			//transform.Translate(Vector3.forward * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			position += transform.TransformDirection(Vector3.back) * Time.deltaTime;
			//transform.Translate(-Vector3.forward * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.D))
		{
			euler += Vector3.up * Time.deltaTime * 90;
			rotation = Quaternion.Euler(euler);
			//transform.Rotate(Vector3.up * Time.deltaTime * 90);
		}
		if (Input.GetKey(KeyCode.A))
		{
			euler += Vector3.down * Time.deltaTime * 90;
			rotation = Quaternion.Euler(euler);
			//transform.Rotate(-Vector3.up * Time.deltaTime * 90);
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
		netMan.SendTransform(position, rotation, network_id);
	}
}
