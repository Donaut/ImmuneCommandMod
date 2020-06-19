using System.Collections.Generic;
using UnityEngine;

public class Fireplace : ServerBuilding
{
	private bool m_isOnFire;

	private float m_nextCookTime;

	public override bool Use(ServerPlayer a_player)
	{
		m_isOnFire = !m_isOnFire;
		return true;
	}

	public override float GetState()
	{
		return (!m_isOnFire) ? 1f : 0.4f;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Update()
	{
		if (m_isOnFire)
		{
			float time = Time.time;
			m_decayTime -= Time.deltaTime * 9f;
			if (time > m_nextCookTime)
			{
				List<DatabaseItem> freeWorldItems = m_server.GetFreeWorldItems();
				for (int i = 0; i < freeWorldItems.Count; i++)
				{
					DatabaseItem databaseItem = freeWorldItems[i];
					if (!Items.IsCookable(databaseItem.type))
					{
						continue;
					}
					DatabaseItem databaseItem2 = freeWorldItems[i];
					if (time > databaseItem2.dropTime + 3f)
					{
						float sqrMagnitude = (base.transform.position - freeWorldItems[i].GetPos()).sqrMagnitude;
						if (sqrMagnitude < 3f)
						{
							DatabaseItem value = freeWorldItems[i];
							value.type++;
							freeWorldItems[i] = value;
						}
					}
				}
				m_nextCookTime = Time.time + 4f;
			}
		}
		base.Update();
	}
}
