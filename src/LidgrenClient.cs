using Lidgren.Network;
using System.Collections.Generic;
using UnityEngine;

public class LidgrenClient : LidgrenPeer
{
	private int clientId;

	private NetClient client;

	private Dictionary<int, LidgrenGameObject> lgos = new Dictionary<int, LidgrenGameObject>();

	[SerializeField]
	private int port = 10000;

	[SerializeField]
	private string host = "127.0.0.1";

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		Object.DontDestroyOnLoad(base.gameObject);
		client = new NetClient(new NetPeerConfiguration("LidgrenDemo"));
		client.Start();
		client.Connect(host, port);
		SetPeer(client);
		base.Connected += onConnected;
		base.Disconnected += onDisconnected;
		RegisterMessageHandler(LidgrenMessageHeaders.Hello, onHello);
		RegisterMessageHandler(LidgrenMessageHeaders.Spawn, onSpawn);
		RegisterMessageHandler(LidgrenMessageHeaders.Despawn, onDespawn);
		RegisterMessageHandler(LidgrenMessageHeaders.Movement, onMovement);
		RegisterMessageHandler(LidgrenMessageHeaders.Position, onPosition);
	}

	private void onConnected(NetIncomingMessage msg)
	{
		Debug.Log("Connected to server");
	}

	private void onDisconnected(NetIncomingMessage msg)
	{
		Debug.Log("Disconnected from server");
	}

	private void onMovement(NetIncomingMessage msg)
	{
		Debug.Log("onMovement");
		int key = msg.ReadInt32();
		LidgrenGameObject value = null;
		if (lgos.TryGetValue(key, out value))
		{
			value.GetComponent<PlayerAnimator>().OnPlayerMovement(msg.ReadByte());
		}
	}

	private void onPosition(NetIncomingMessage msg)
	{
		Debug.Log("onPosition");
		int key = msg.ReadInt32();
		LidgrenGameObject value = null;
		if (lgos.TryGetValue(key, out value))
		{
			Vector3 position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
			value.transform.position = position;
			Quaternion rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
			value.transform.GetChild(0).rotation = rotation;
		}
	}

	private void onSpawn(NetIncomingMessage msg)
	{
		int num = msg.ReadInt32();
		lgos.Add(num, LidgrenGameObject.Spawn(clientId, num, msg.SenderConnection));
	}

	private void onDespawn(NetIncomingMessage msg)
	{
		try
		{
			int key = msg.ReadInt32();
			Object.Destroy(lgos[key]);
			lgos.Remove(key);
		}
		catch
		{
		}
	}

	private void onHello(NetIncomingMessage msg)
	{
		clientId = msg.ReadInt32();
		NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(LidgrenMessageHeaders.RequestSpawn);
		msg.SenderConnection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
	}
}
