using UnityEngine;

public class Lootbox : ServerBuilding
{
	[HideInInspector]
	public ItemContainer m_container;

	[HideInInspector]
	public int m_cid;

	private bool m_loadItemsFromDb;

	private void CreateCid()
	{
		Vector3 position = base.transform.position;
		float num = position.x + 10000f;
		Vector3 position2 = base.transform.position;
		float num2 = position2.z + 10000f;
		m_cid = (int)(num * 10.00001f + num2 * 20.00001f);
	}

	public override bool Use(ServerPlayer a_player)
	{
		if (m_loadItemsFromDb)
		{
			m_sql.RequestContainer(m_cid);
			m_loadItemsFromDb = false;
		}
		a_player.m_persistentContainer = m_container;
		a_player.m_updateContainersFlag = true;
		return base.Use(a_player);
	}

	protected override void Awake()
	{
		CreateCid();
		base.Awake();
	}

	public override void Init(LidServer a_server, int a_type, int a_ownerPid = 0, int a_health = 100, bool a_isNew = true)
	{
		CreateCid();
		m_sql = a_server.GetSql();
		if (null != m_sql)
		{
			m_container = new ItemContainer(4, 4, 6, m_cid, m_sql);
			m_container.m_position = base.transform.position;
			if (!a_isNew)
			{
				m_loadItemsFromDb = true;
			}
		}
		base.Init(a_server, a_type, a_ownerPid, a_health, a_isNew);
	}

	private void OnDestroy()
	{
		if (m_buildingIsDead && Global.isServer && null != m_sql && m_container != null)
		{
			Debug.Log("Lootbox.OnDestroy() has been called ... deleting items in box");
			m_container.DeleteItems();
		}
	}
}
