using UnityEngine;

public class DestroyableResource : ServerBuilding
{
	public int m_itemIndex = 130;

	public int m_quantity = 1000;

	public float m_respawnDur = 120f;

	private float m_respawnTime;

	private int m_curQuantity;

	public override bool Use(ServerPlayer a_player)
	{
		return false;
	}

	public override float GetState()
	{
		return (!(m_respawnTime > 0f) || !(Time.time < m_respawnTime)) ? 1f : 0.4f;
	}

	protected override void Awake()
	{
		m_curQuantity = m_quantity;
		base.Awake();
	}

	protected override void Update()
	{
		if (m_gotDamage > 0f && m_curQuantity > 0)
		{
			int num = Mathf.Min(1 + (int)(m_gotDamage * 0.08f), m_curQuantity);
			if (null == m_server)
			{
				m_server = Object.FindObjectOfType<LidServer>();
			}
			if (null != m_server)
			{
				Vector3 a_pos = (!(null == m_gotAttacker)) ? m_gotAttacker.position : (base.transform.position + base.transform.forward);
				m_server.CreateFreeWorldItem(m_itemIndex, num, a_pos);
			}
			m_curQuantity -= num;
			if (m_curQuantity <= 0)
			{
				m_respawnTime = Time.time + m_respawnDur;
				if (m_isStatic)
				{
					SendStateToClients();
				}
			}
			m_gotAttacker = null;
			m_gotDamage = 0f;
		}
		if (m_respawnTime > 0f && Time.time > m_respawnTime)
		{
			m_curQuantity = m_quantity;
			m_respawnTime = 0f;
			m_gotDamage = 0f;
			if (m_isStatic)
			{
				SendStateToClients();
			}
		}
		base.Update();
	}

	private void SendStateToClients()
	{
		if (null == m_server)
		{
			m_server = Object.FindObjectOfType<LidServer>();
		}
		if (null != m_server)
		{
			m_server.BroadcastStaticBuildingChange(this);
		}
	}

	private void OnCollisionEnter(Collision a_col)
	{
		float num = Mathf.Clamp(a_col.relativeVelocity.sqrMagnitude - 10f, 0f, 10000f);
		if (num > 1f)
		{
			m_gotDamage = num;
		}
	}
}
