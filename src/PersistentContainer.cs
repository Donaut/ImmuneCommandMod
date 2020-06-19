using UnityEngine;

public class PersistentContainer : MonoBehaviour
{
	public bool m_dropOnEmpty = true;

	[HideInInspector]
	public ItemContainer m_container;

	private float m_nextDropOnEmptyTime;

	private int m_cid;

	private SQLThreadManager m_sql;

	private void Awake()
	{
		if (Global.isServer)
		{
			Vector3 position = base.transform.position;
			float num = position.x * 10f;
			Vector3 position2 = base.transform.position;
			m_cid = (int)(num + position2.z * 20f);
			m_sql = (SQLThreadManager)Object.FindObjectOfType(typeof(SQLThreadManager));
			m_container = new ItemContainer(4, 4, 6, m_cid, m_sql);
			m_container.m_position = base.transform.position;
			SetNextDropTime();
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (Global.isServer && m_container != null && Time.time > m_nextDropOnEmptyTime)
		{
			if (m_container.Count() == 0 && Random.Range(0, 5) == 0)
			{
				DatabaseItem a_item = new DatabaseItem(Items.GetRandomType(70f));
				a_item.amount = ((!Items.HasCondition(a_item.type)) ? 1 : Random.Range(1, 50));
				m_container.CollectItem(a_item, true);
			}
			SetNextDropTime();
		}
	}

	private void SetNextDropTime()
	{
		m_nextDropOnEmptyTime = Time.time + Random.Range(600f, 7200f);
	}

	private void OnDestroy()
	{
		if (Global.isServer && null != m_sql)
		{
			m_container.DeleteItems();
		}
	}
}
