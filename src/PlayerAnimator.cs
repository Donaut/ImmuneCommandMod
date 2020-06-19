using Lidgren.Network;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	[SerializeField]
	private Animation target;

	private byte state;

	private Quaternion rotation = Quaternion.identity;

	private LidgrenGameObject lgo;

	private void Start()
	{
		target.wrapMode = WrapMode.Loop;
		target["Jump"].wrapMode = WrapMode.Once;
		target["Jump"].layer = 1;
		target["Land"].wrapMode = WrapMode.Once;
		target["Land"].layer = 1;
		target["Run"].speed = 1.75f;
		target["Walk"].speed = -1.25f;
		target.Play("Idle");
		lgo = GetComponent<LidgrenGameObject>();
	}

	private void Update()
	{
		if (lgo.IsMine)
		{
			switch (state)
			{
			case 1:
				rotate(0f);
				break;
			case 2:
				rotate(0f);
				break;
			case 7:
				rotate(-90f);
				break;
			case 8:
				rotate(90f);
				break;
			case 9:
				rotate(-45f);
				break;
			case 10:
				rotate(45f);
				break;
			case 11:
				rotate(45f);
				break;
			case 12:
				rotate(-45f);
				break;
			}
		}
		switch (state)
		{
		case 6:
			target.CrossFade("Idle");
			break;
		case 4:
			target.CrossFade("Fall");
			break;
		case 1:
		case 7:
		case 8:
		case 9:
		case 10:
			target.CrossFade("Run");
			break;
		case 2:
		case 11:
		case 12:
			target.CrossFade("Walk");
			break;
		}
		if (!lgo.IsMine)
		{
			return;
		}
		if (state == 6)
		{
			if (Input.GetMouseButtonDown(1))
			{
				target.transform.rotation = Quaternion.LookRotation(base.transform.forward);
			}
		}
		else
		{
			target.transform.rotation = Quaternion.Slerp(target.transform.rotation, rotation, Time.deltaTime * 10f);
		}
	}

	private void rotate(float yaw)
	{
		rotation = Quaternion.LookRotation(Quaternion.Euler(0f, yaw, 0f) * base.transform.forward, Vector3.up);
	}

	public void OnPlayerMovement(byte newState)
	{
		if (lgo.IsMine)
		{
			NetOutgoingMessage netOutgoingMessage = lgo.Connection.Peer.CreateMessage();
			netOutgoingMessage.Write(LidgrenMessageHeaders.Movement);
			netOutgoingMessage.Write(lgo.Id);
			netOutgoingMessage.Write(newState);
			lgo.Connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
		}
		state = newState;
		switch (state)
		{
		case 4:
			break;
		case 5:
			target.CrossFade("Land");
			break;
		case 3:
			target.CrossFade("Jump");
			break;
		}
	}
}
