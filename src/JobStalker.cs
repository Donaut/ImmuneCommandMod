using UnityEngine;

public class JobStalker : JobBase
{
	public bool m_pickupThings = true;

	private float m_nextPickupTime;

	private LidServer m_server;

	private float m_resetAttackTime;

	private void Start()
	{
		Init();
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
	}

	public void SetAggressor(Transform a_aggressor)
	{
		m_body.Attack(a_aggressor, true);
		m_resetAttackTime = Time.time + Random.Range(3f, 10f);
	}

	public override void Execute(float deltaTime)
	{
		if ((m_body.GetState() == eBodyBaseState.none || 600f < Time.time - m_nextPickupTime) && null != m_server)
		{
			PickupThings();
		}
		if (m_resetAttackTime > 0f && Time.time > m_resetAttackTime)
		{
			m_body.Attack(null, false);
		}
	}

	private void PickupThings()
	{
		if (!(Time.time > m_nextPickupTime))
		{
			return;
		}
		m_nextPickupTime = Time.time + Random.Range(10f, 30f);
		Vector3 vector = Vector3.zero;
		DatabaseItem databaseItem = new DatabaseItem(0);
		if (m_pickupThings)
		{
			m_server.PickupItem(null, m_brain);
			databaseItem = m_server.GetRandomFreeWorldItem();
			if (databaseItem.type != 0 && 600f < Time.time - databaseItem.dropTime)
			{
				vector = new Vector3(databaseItem.x, 0f, databaseItem.y);
				if (m_server.IsInSpecialArea(vector, eAreaType.noPvp))
				{
					vector = Vector3.zero;
				}
			}
		}
		if (Vector3.zero == vector)
		{
			vector = base.transform.position + new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f));
		}
		m_body.GoTo(vector);
	}
}
