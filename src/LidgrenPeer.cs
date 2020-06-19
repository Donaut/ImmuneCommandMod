using Lidgren.Network;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LidgrenPeer : MonoBehaviour
{
	private NetPeer peer;

	private Dictionary<int, Action<NetIncomingMessage>> messageHandlers = new Dictionary<int, Action<NetIncomingMessage>>();

	protected event Action<NetIncomingMessage> Connected;

	protected event Action<NetIncomingMessage> Disconnected;

	protected void SetPeer(NetPeer peer)
	{
		this.peer = peer;
	}

	protected void RegisterMessageHandler(byte id, Action<NetIncomingMessage> message)
	{
		messageHandlers.Add(id, message);
	}

	protected void ReadMessages()
	{
		if (peer == null)
		{
			return;
		}
		NetIncomingMessage netIncomingMessage;
		while ((netIncomingMessage = peer.ReadMessage()) != null)
		{
			switch (netIncomingMessage.MessageType)
			{
			case NetIncomingMessageType.Data:
			{
				byte b = netIncomingMessage.ReadByte();
				Action<NetIncomingMessage> value = null;
				if (messageHandlers.TryGetValue(b, out value))
				{
					value(netIncomingMessage);
				}
				else
				{
					Debug.LogWarning("No handler for message id " + b);
				}
				break;
			}
			case NetIncomingMessageType.StatusChanged:
				switch (netIncomingMessage.SenderConnection.Status)
				{
				case NetConnectionStatus.Connected:
					if (this.Connected != null)
					{
						this.Connected(netIncomingMessage);
					}
					break;
				case NetConnectionStatus.Disconnected:
					if (this.Disconnected != null)
					{
						this.Disconnected(netIncomingMessage);
					}
					break;
				}
				Debug.Log(string.Concat("Status on ", netIncomingMessage.SenderConnection, " changed to ", netIncomingMessage.SenderConnection.Status));
				break;
			case NetIncomingMessageType.VerboseDebugMessage:
			case NetIncomingMessageType.DebugMessage:
			case NetIncomingMessageType.WarningMessage:
			case NetIncomingMessageType.ErrorMessage:
				Debug.Log("Lidgren: " + netIncomingMessage.ReadString());
				break;
			}
			peer.Recycle(netIncomingMessage);
		}
	}

	private void FixedUpdate()
	{
		ReadMessages();
	}

	private void Update()
	{
		ReadMessages();
	}

	private void LateUpdate()
	{
		ReadMessages();
	}
}
