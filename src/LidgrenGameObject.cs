using Lidgren.Network;
using UnityEngine;

public class LidgrenGameObject : MonoBehaviour
{
	public int Id;

	public bool IsMine;

	public NetConnection Connection
	{
		get;
		set;
	}

	private void FixedUpdate()
	{
		if (IsMine)
		{
			NetOutgoingMessage netOutgoingMessage = Connection.Peer.CreateMessage();
			netOutgoingMessage.Write(LidgrenMessageHeaders.Position);
			netOutgoingMessage.Write(Id);
			Vector3 position = base.transform.position;
			netOutgoingMessage.Write(position.x);
			Vector3 position2 = base.transform.position;
			netOutgoingMessage.Write(position2.y);
			Vector3 position3 = base.transform.position;
			netOutgoingMessage.Write(position3.z);
			Transform child = base.transform.GetChild(0);
			Quaternion rotation = child.rotation;
			netOutgoingMessage.Write(rotation.x);
			Quaternion rotation2 = child.rotation;
			netOutgoingMessage.Write(rotation2.y);
			Quaternion rotation3 = child.rotation;
			netOutgoingMessage.Write(rotation3.z);
			Quaternion rotation4 = child.rotation;
			netOutgoingMessage.Write(rotation4.w);
			Connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public static LidgrenGameObject Spawn(int myId, int id, NetConnection con)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(Resources.Load("Player"), new Vector3(45f, 10f, 45f), Quaternion.identity);
		LidgrenGameObject component = gameObject.GetComponent<LidgrenGameObject>();
		component.Id = id;
		component.IsMine = (myId == id);
		component.Connection = con;
		return component;
	}
}
