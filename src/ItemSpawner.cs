using UnityEngine;

public class ItemSpawner : ServerBuilding
{
	public bool m_dropWithPlayerAround;

	public int m_maxItems = 1;

	public DropItem[] m_itemDrops;

	public int m_containerType;

	public float m_spawnDuration = 10f;

	private float m_nextSpawnTime;

	protected override void Awake()
	{
		m_nextSpawnTime = Time.time + m_spawnDuration;
		if (m_containerType == 0)
		{
			m_maxItems = 1;
		}
		base.Awake();
	}

	protected override void Update()
	{
		if (Time.time > m_nextSpawnTime)
		{
			if (null == m_server)
			{
				m_server = Object.FindObjectOfType<LidServer>();
			}
			if (null != m_server)
			{
				int nearbyItemCount = m_server.GetNearbyItemCount(base.transform.position);
				ServerPlayer serverPlayer = (!m_dropWithPlayerAround) ? m_server.GetNearestPlayer(base.transform.position) : null;
				if (m_maxItems > nearbyItemCount && (serverPlayer == null || (base.transform.position - serverPlayer.GetPosition()).sqrMagnitude > 2500f))
				{
					DropLoot();
				}
			}
			else
			{
				Object.Destroy(base.gameObject);
			}
			m_nextSpawnTime = Time.time + Random.Range(m_spawnDuration * 0.5f, m_spawnDuration * 1.5f);
		}
		base.Update();
	}

	private void DropLoot()
	{
		if (!(null != m_server) || m_itemDrops == null || m_itemDrops.Length <= 0)
		{
			return;
		}
		int num = Random.Range(0, m_itemDrops.Length);
		if (m_itemDrops[num] == null || Random.Range(0, 100) >= m_itemDrops[num].chance)
		{
			return;
		}
		int num2 = (m_itemDrops[num].typeFrom != m_itemDrops[num].typeTo) ? Random.Range(m_itemDrops[num].typeFrom, m_itemDrops[num].typeTo + 1) : m_itemDrops[num].typeFrom;
		ItemDef itemDef = Items.GetItemDef(num2);
		if (itemDef.ident != null && itemDef.ident.Length > 0)
		{
			int value = (!Items.HasAmountOrCondition(num2)) ? 1 : Random.Range(m_itemDrops[num].min, m_itemDrops[num].max + 1);
			value = Mathf.Clamp(value, 1, 254);
			if (m_containerType != 0)
			{
				m_server.CreateTempContainerItem(num2, value, base.transform.position, m_containerType);
				return;
			}
			int num3 = 10;
			Vector3 b = new Vector3((float)Random.Range(-num3, num3) * 0.1f, 0f, (float)Random.Range(-num3, num3) * 0.1f);
			m_server.CreateFreeWorldItem(num2, value, base.transform.position + b);
		}
	}
}
