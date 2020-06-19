using UnityEngine;

public class Turret : ServerBuilding
{
	public float m_attackRadius = 5f;

	public float m_damage = 10f;

	public float m_damageIntervall = 2f;

	private float m_nextCheckTime;

	protected override void Awake()
	{
		m_nextCheckTime = Random.Range(4f, 8f);
		base.Awake();
	}

	protected override void Update()
	{
		if (Time.time > m_nextCheckTime)
		{
			float num = m_damageIntervall;
			if (null == m_server)
			{
				m_server = Object.FindObjectOfType<LidServer>();
			}
			if (null != m_server)
			{
				Vector3 position = base.transform.position;
				ServerPlayer nearestPlayer = m_server.GetNearestPlayer(position);
				if (nearestPlayer != null && nearestPlayer.m_pid != m_ownerPid && !m_server.PartyContainsPid(nearestPlayer.m_partyId, m_ownerPid))
				{
					float sqrMagnitude = (nearestPlayer.GetPosition() - position).sqrMagnitude;
					if (m_attackRadius * m_attackRadius > sqrMagnitude)
					{
						nearestPlayer.ChangeHealthBy(0f - m_damage);
					}
					num += ((!(sqrMagnitude > 2500f)) ? 0f : 4f);
				}
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
			m_nextCheckTime = Time.time + num;
		}
		base.Update();
	}
}
