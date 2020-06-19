using Lidgren.Network;
using System.Linq;
using UnityEngine;

public class LidgrenServer : LidgrenPeer
{
	private int clientCounter;

	private NetServer server;

	[SerializeField]
	private int port = 10000;

	private void Start()
	{
		Object.DontDestroyOnLoad(this);
		Object.DontDestroyOnLoad(base.gameObject);
		NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration("LidgrenDemo");
		netPeerConfiguration.Port = port;
		server = new NetServer(netPeerConfiguration);
		server.Start();
		SetPeer(server);
		base.Connected += onConnected;
		base.Disconnected += onDisconnected;
		RegisterMessageHandler(LidgrenMessageHeaders.RequestSpawn, onRequestSpawn);
		RegisterMessageHandler(LidgrenMessageHeaders.Movement, onMovement);
		RegisterMessageHandler(LidgrenMessageHeaders.Position, onPosition);
	}

	private void spawnOn(LidgrenGameObject go, NetConnection c)
	{
		NetOutgoingMessage netOutgoingMessage = c.Peer.CreateMessage();
		netOutgoingMessage.Write(LidgrenMessageHeaders.Spawn);
		netOutgoingMessage.Write(go.Id);
		c.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
	}

	private void onMovement(NetIncomingMessage msg)
	{
		LidgrenPlayer lidgrenPlayer = (LidgrenPlayer)msg.SenderConnection.Tag;
		NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(msg);
		server.SendToAll(netOutgoingMessage, msg.SenderConnection, NetDeliveryMethod.ReliableOrdered, 1);
		msg.ReadInt32();
		lidgrenPlayer.GameObject.GetComponent<PlayerAnimator>().OnPlayerMovement(msg.ReadByte());
	}

	private void onPosition(NetIncomingMessage msg)
	{
		LidgrenPlayer lidgrenPlayer = (LidgrenPlayer)msg.SenderConnection.Tag;
		NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(msg);
		server.SendToAll(netOutgoingMessage, msg.SenderConnection, NetDeliveryMethod.Unreliable, 0);
		msg.ReadInt32();
		Vector3 position = new Vector3(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
		lidgrenPlayer.GameObject.transform.position = position;
		Quaternion rotation = new Quaternion(msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat(), msg.ReadFloat());
		lidgrenPlayer.GameObject.transform.GetChild(0).rotation = rotation;
	}

	private void onRequestSpawn(NetIncomingMessage msg)
	{
		LidgrenPlayer lidgrenPlayer = (LidgrenPlayer)msg.SenderConnection.Tag;
		if (lidgrenPlayer.GameObject == null)
		{
			lidgrenPlayer.GameObject = LidgrenGameObject.Spawn(-1, lidgrenPlayer.Id, msg.SenderConnection);
			foreach (NetConnection connection in server.Connections)
			{
				spawnOn(lidgrenPlayer.GameObject, connection);
			}
		}
	}

	private void onConnected(NetIncomingMessage a_msg)
	{
		NetOutgoingMessage netOutgoingMessage = a_msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(LidgrenMessageHeaders.Hello);
		netOutgoingMessage.Write(++clientCounter);
		a_msg.SenderConnection.Tag = new LidgrenPlayer(clientCounter);
		a_msg.SenderConnection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
		foreach (LidgrenGameObject item in Object.FindObjectsOfType(typeof(LidgrenGameObject)).Cast<LidgrenGameObject>())
		{
			spawnOn(item, a_msg.SenderConnection);
		}
		Debug.Log("Client connected");
	}

	private void onDisconnected(NetIncomingMessage a_msg)
	{
		LidgrenPlayer lidgrenPlayer = (LidgrenPlayer)a_msg.SenderConnection.Tag;
		NetOutgoingMessage netOutgoingMessage = server.CreateMessage();
		netOutgoingMessage.Write(LidgrenMessageHeaders.Despawn);
		netOutgoingMessage.Write(lidgrenPlayer.Id);
		server.SendToAll(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered);
		if (lidgrenPlayer.GameObject != null)
		{
			Object.Destroy(lidgrenPlayer.GameObject);
		}
		Debug.Log("Client disconnected");
	}
}
