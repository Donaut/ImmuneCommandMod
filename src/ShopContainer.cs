using UnityEngine;

public class ShopContainer : MonoBehaviour
{
	public float m_buyPriceMuliplier = 1f;

	public float m_sellPriceMuliplier = 1f;

	public int m_minItemCount = 8;

	public eShopType m_type;

	[HideInInspector]
	public ItemContainer m_container;

	private static int m_cid;

	private float m_nextUpdateTime;

	private void Awake()
	{
		if (Global.isServer)
		{
			m_cid++;
			m_container = new ItemContainer(4, 4, 6);
			m_container.m_position = base.transform.position;
		}
		else
		{
			Object.Destroy(this);
		}
	}

	private void Start()
	{
		for (int i = 0; i < m_minItemCount; i++)
		{
			AddRandomItem();
		}
	}

	private void Update()
	{
		if (Time.time > m_nextUpdateTime)
		{
			if (m_container.Count() < m_minItemCount)
			{
				AddRandomItem();
			}
			else
			{
				m_container.DeleteItem(Random.Range(0, m_container.Count()));
			}
			m_nextUpdateTime = Time.time + Random.Range(30f, 120f);
		}
	}

	private void AddRandomItem()
	{
		int num = 0;
		num = ((m_type == eShopType.eFood) ? Items.GetRandomFood() : ((m_type == eShopType.eRareAmmo) ? Random.Range(40, 45) : ((m_type == eShopType.ePharmacy) ? Random.Range(140, 143) : ((m_type != eShopType.eResources) ? ((Random.Range(0, 8) != 0) ? Items.GetRandomType(90f) : 92) : Random.Range(130, 134)))));
		DatabaseItem a_item = new DatabaseItem(num);
		if (Items.HasAmountOrCondition(a_item.type))
		{
			a_item.amount = ((!Items.HasCondition(a_item.type)) ? Random.Range(1, 10) : Random.Range(10, 100));
			if (Items.HasCondition(a_item.type))
			{
				a_item.amount = Random.Range(20, 100);
			}
			else if (Items.IsMedicine(a_item.type))
			{
				a_item.amount = 1;
			}
			else if (Items.IsResource(a_item.type))
			{
				a_item.amount = Random.Range(10, 50);
			}
			else if (Items.IsEatable(a_item.type))
			{
				a_item.amount = Random.Range(1, 5);
			}
			else
			{
				a_item.amount = Random.Range(1, 10);
			}
		}
		m_container.CollectItem(a_item, false);
	}
}
