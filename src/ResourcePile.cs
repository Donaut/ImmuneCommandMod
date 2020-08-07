using UnityEngine;

public class ResourcePile : ServerBuilding
{
	public int m_itemIndex = 130;

	public int m_quantity = 10;

	protected override void Update()
	{
		if (null != m_server && m_gotDamage > 0f && null != m_gotAttacker)
		{
			int num = 1 + (int)(m_gotDamage * 0.08f);
			m_server.CreateFreeWorldItem(m_itemIndex, num, m_gotAttacker.position);
			m_quantity -= num;
			if (m_quantity <= 0)
			{
				Object.Destroy(base.gameObject);
			}
			m_gotAttacker = null;
			m_gotDamage = 0f;
		}
		base.Update();
	}
}
