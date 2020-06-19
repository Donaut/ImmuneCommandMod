using UnityEngine;

public class RemoteItem : MonoBehaviour
{
	public GameObject m_labelAmountPrefab;

	public GameObject m_labelPricePrefab;

	public Vector3 m_amountLabelOffset = new Vector3(0.45f, -0.45f, 0.2f);

	public Vector3 m_priceLabelOffset = new Vector3(0.45f, 0.45f, 0.2f);

	public float m_inventoryScale = 0.26f;

	public float m_worldScale = 1.3f;

	[HideInInspector]
	public int m_type;

	[HideInInspector]
	public int m_amountOrCond;

	[HideInInspector]
	public bool m_isInventoryItem;

	[HideInInspector]
	public bool m_isInventoryOrContainerItem;

	private Renderer[] m_renderers;

	private GameObject m_riseItemEffect;

	private bool m_visible;

	private float m_lastUpdate;

	private float m_disappearTime = 0.5f;

	private float m_dieTime = 10f;

	public void Refresh()
	{
		if (!m_visible && !m_isInventoryOrContainerItem)
		{
			SwitchVisibility();
		}
		m_lastUpdate = Time.time;
	}

	public void Init(Vector3 a_pos, int a_type, int a_amount, bool a_isContainerItem)
	{
		m_type = a_type;
		bool flag = Items.IsStackable(m_type);
		a_pos.y = 0.1f;
		base.transform.position = a_pos;
		m_amountOrCond = a_amount;
		m_isInventoryOrContainerItem = a_isContainerItem;
		int isInventoryItem;
		if (m_isInventoryOrContainerItem)
		{
			Vector3 position = base.transform.position;
			isInventoryItem = ((position.x < 5f) ? 1 : 0);
		}
		else
		{
			isInventoryItem = 0;
		}
		m_isInventoryItem = ((byte)isInventoryItem != 0);
		InstantiateItem();
		if (m_isInventoryOrContainerItem)
		{
			base.transform.localScale = Vector3.one * m_inventoryScale;
			if (Items.HasAmountOrCondition(m_type))
			{
				CreateLabel(m_labelAmountPrefab, m_amountLabelOffset, a_amount + ((!flag) ? "%" : string.Empty));
			}
		}
		else
		{
			base.transform.localScale = Vector3.one * m_worldScale;
		}
		if (null != base.audio && Time.timeSinceLevelLoad > 5f)
		{
			base.audio.Play();
		}
		m_lastUpdate = Time.time;
	}

	public void CreateLabel(GameObject a_go, Vector3 a_offset, string a_caption)
	{
		if (null != a_go)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(a_go);
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = a_offset;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			EasyFontTextMesh component = gameObject.GetComponent<EasyFontTextMesh>();
			if (null != component)
			{
				component.Text = a_caption;
			}
		}
	}

	private void Update()
	{
		if (!m_isInventoryOrContainerItem)
		{
			if (m_visible)
			{
				if (m_lastUpdate + m_disappearTime < Time.time)
				{
					SwitchVisibility();
				}
			}
			else if (m_lastUpdate + m_dieTime < Time.time)
			{
				Object.Destroy(base.gameObject);
			}
		}
		if (null != m_riseItemEffect)
		{
			float num = 20f * Time.deltaTime;
			m_riseItemEffect.transform.position += new Vector3(0f, num, num * -0.3f);
			Vector3 position = m_riseItemEffect.transform.position;
			if (position.y > 6f)
			{
				Object.Destroy(m_riseItemEffect);
			}
		}
	}

	private GameObject InstantiateItem(bool a_justForEffect = false)
	{
		GameObject gameObject = (GameObject)Resources.Load("items/item_" + m_type);
		GameObject gameObject2 = null;
		if (null != gameObject)
		{
			float x = 90f;
			Quaternion rotation = (!m_isInventoryOrContainerItem) ? Quaternion.Euler(x, Random.Range(0f, 360f), 0f) : Quaternion.identity;
			gameObject2 = (GameObject)Object.Instantiate(gameObject, base.transform.position, rotation);
			gameObject2.transform.parent = base.transform;
			Util.SetLayerRecursively(base.transform, (!m_isInventoryOrContainerItem) ? 10 : 17);
			if (!a_justForEffect)
			{
				m_renderers = gameObject2.GetComponentsInChildren<Renderer>();
			}
			Transform transform = gameObject2.transform.FindChild("Particles");
			if (null != transform)
			{
				transform.gameObject.SetActive(false);
			}
			Transform transform2 = gameObject2.transform.FindChild("Point light");
			if (null != transform2)
			{
				transform2.gameObject.SetActive(false);
			}
		}
		else
		{
			gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gameObject2.transform.position = base.transform.position;
			gameObject2.transform.localScale = Vector3.one * 0.5f;
			gameObject2.transform.parent = base.transform;
		}
		return gameObject2;
	}

	public bool IsVisible()
	{
		return m_visible;
	}

	public void SwitchVisibility()
	{
		m_visible = !m_visible;
		if (m_renderers != null)
		{
			Renderer[] renderers = m_renderers;
			foreach (Renderer renderer in renderers)
			{
				if (null != renderer)
				{
					renderer.enabled = m_visible;
				}
			}
		}
		if (!m_visible && !m_isInventoryOrContainerItem)
		{
			m_riseItemEffect = InstantiateItem(true);
		}
	}
}
