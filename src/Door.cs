using UnityEngine;

public class Door : ServerBuilding
{
	private bool m_doorOpen;

	private Quaternion m_startRot = Quaternion.identity;

	public override bool Use(ServerPlayer a_player)
	{
		bool result = false;
		base.Use(a_player);
		if (a_player.m_pid == m_ownerPid || m_server.PartyContainsPid(a_player.m_partyId, m_ownerPid))
		{
			SwitchDoorState(a_player.GetPosition());
			result = true;
		}
		return result;
	}

	protected override void Awake()
	{
		m_startRot = base.transform.rotation;
		base.Awake();
	}

	private void SwitchDoorState(Vector3 a_pos)
	{
		float y = (!(Vector3.Dot((a_pos - base.transform.position).normalized, base.transform.forward) < 0f)) ? 90f : (-90f);
		m_doorOpen = !m_doorOpen;
		if (m_doorOpen)
		{
			base.transform.rotation = m_startRot * Quaternion.Euler(0f, y, 0f);
		}
		else
		{
			base.transform.rotation = m_startRot;
		}
	}
}
