using System.Collections;
using UnityEngine;

public class InventoryGUI : MonoBehaviour
{
	public GameObject m_btnOpen;

	public GameObject m_btnClose;

	public GameObject m_btnCraft;

	public GameObject m_btnSteamInventory;

	public GameObject m_guiSteamInventory;

	public GameObject m_guiBig;

	public GameObject m_guiContainer;

	public QmunicatorGUI m_communicator;

	public GameObject m_guiShopInfo;

	public GameObject m_guiItemInfo;

	public GameObject m_guiHintBuilding;

	public TextMesh m_guiInfoName;

	public TextMesh m_guiInfoDesc;

	public TextMesh m_guiInfoStats;

	public TextMesh m_guiGold;

	private float m_shopBuyMultiplier = -1f;

	private float m_shopSellMultiplier = -1f;

	private LidClient m_client;

	private GUI3dMaster m_guimaster;

	private RemoteItem[] m_items;

	private QuitGameGUI m_quitGameGui;

	private RemoteItem m_handItem;

	private float m_hideInfoTime;

	private Hashtable m_resourceCounts = new Hashtable();

	private void ClearInventory(bool a_onlyContainer = false)
	{
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && (!a_onlyContainer || !m_items[i].m_isInventoryItem))
				{
					Object.Destroy(m_items[i].gameObject);
				}
			}
		}
		if (!a_onlyContainer)
		{
			m_items = null;
		}
	}

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_quitGameGui = (QuitGameGUI)Object.FindObjectOfType(typeof(QuitGameGUI));
	}

	private void Update()
	{
		if (Input.GetButtonDown("Exit") || Input.GetButtonDown("Communicator") || Input.GetButtonDown("Map") || Input.GetButtonDown("Help") || Input.GetButtonDown("Global Chat") || Input.GetButtonDown("Crafting"))
		{
			m_guiSteamInventory.SetActive(false);
			m_guiBig.SetActive(false);
			m_guiItemInfo.SetActive(false);
		}
		else if (Input.GetButtonDown("Inventory"))
		{
			m_guiSteamInventory.SetActive(false);
			m_guiBig.SetActive(!m_guiBig.activeSelf);
			m_guiItemInfo.SetActive(false);
		}
		if (m_hideInfoTime > 0f && Time.time > m_hideInfoTime)
		{
			m_guiItemInfo.SetActive(false);
			m_hideInfoTime = 0f;
		}
		if (IsShopActive() != m_guiShopInfo.activeSelf)
		{
			m_guiShopInfo.SetActive(IsShopActive());
		}
		bool flag = !(null != m_client) || m_client.IsTutorialActive();
		if (m_guiHintBuilding.activeSelf != flag)
		{
			m_guiHintBuilding.SetActive(flag);
		}
		if (null != m_client)
		{
			m_guiGold.text = m_client.GetGoldCount().ToString();
		}
		m_quitGameGui.m_openWithEsc = (!m_communicator.IsActive() && !m_guiBig.activeSelf);
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster))
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		if (!(string.Empty != clickedButtonName))
		{
			return;
		}
		if (null != m_btnOpen && m_btnOpen.name == clickedButtonName)
		{
			m_guiBig.SetActive(true);
		}
		else if ((null != m_btnClose && m_btnClose.name == clickedButtonName) || (null != m_btnCraft && m_btnCraft.name == clickedButtonName))
		{
			if (null != m_btnCraft && m_btnCraft.name == clickedButtonName)
			{
				m_communicator.OpenCrafting();
				m_guiBig.SetActive(false);
				m_guiSteamInventory.SetActive(false);
			}
			else if (m_guiSteamInventory.activeSelf)
			{
				m_guiSteamInventory.SetActive(false);
			}
			else
			{
				m_guiBig.SetActive(false);
			}
		}
		else if (null != m_btnSteamInventory && m_btnSteamInventory.name == clickedButtonName)
		{
			m_guiSteamInventory.SetActive(!m_guiSteamInventory.activeSelf);
		}
	}

	private void DisableShadows()
	{
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>(true);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.castShadows = false;
			renderer.receiveShadows = false;
		}
	}

	private void UpdateResourceCounts()
	{
		m_resourceCounts.Clear();
		for (int i = 0; i < m_items.Length; i++)
		{
			if (null != m_items[i] && m_items[i].m_isInventoryItem && Items.IsResource(m_items[i].m_type))
			{
				if (m_resourceCounts.Contains(m_items[i].m_type))
				{
					m_resourceCounts[m_items[i].m_type] = (int)m_resourceCounts[m_items[i].m_type] + m_items[i].m_amountOrCond;
				}
				else
				{
					m_resourceCounts.Add(m_items[i].m_type, m_items[i].m_amountOrCond);
				}
			}
		}
	}

	private Vector3 ToInventoryPos(Transform a_t, Vector3 a_pos)
	{
		a_t.parent = m_guiBig.transform;
		float num = 0f;
		float num2 = (!(a_pos.x > 0f)) ? 0f : 0.15f;
		a_pos *= 0.3f;
		a_pos.x += num2;
		a_pos.y = a_pos.z * -1f + num;
		a_pos.z = 0f;
		return a_pos;
	}

	public void UpdateInventory(RemoteItem[] a_items)
	{
		if (a_items == null)
		{
			return;
		}
		ClearInventory();
		if (a_items.Length != 1 || !(null == a_items[0]) || a_items[0].m_type != 0 || !m_items[0].m_isInventoryItem)
		{
			m_items = a_items;
			bool flag = false;
			for (int i = 0; i < m_items.Length; i++)
			{
				if (!(null != m_items[i]))
				{
					continue;
				}
				if (m_items[i].m_isInventoryOrContainerItem)
				{
					flag |= !m_items[i].m_isInventoryItem;
					if (m_items[i].m_type == 0)
					{
						Object.Destroy(m_items[i].gameObject);
						m_items[i] = null;
					}
					else if (!m_items[i].IsVisible())
					{
						if (IsShopActive() && m_items[i].m_type != 254)
						{
							Vector3 localPosition = m_items[i].transform.localPosition;
							float num = (!(localPosition.x < 6f)) ? GetShopBuyMultiplier() : GetShopSellMultiplier();
							int num2 = (int)(Items.GetValue(m_items[i].m_type, m_items[i].m_amountOrCond) * num + 0.5f);
							m_items[i].CreateLabel(m_items[i].m_labelPricePrefab, m_items[i].m_priceLabelOffset, num2 + "G");
						}
						if (m_items[i].transform.position.sqrMagnitude < 0.1f)
						{
							m_handItem = m_items[i];
						}
						m_items[i].transform.localPosition = ToInventoryPos(m_items[i].transform, m_items[i].transform.position);
						m_items[i].transform.localRotation = Quaternion.identity;
						m_items[i].SwitchVisibility();
					}
				}
				else
				{
					Object.Destroy(m_items[i].gameObject);
					m_items[i] = null;
				}
			}
			if (flag != m_guiContainer.activeSelf)
			{
				if (!m_guiContainer.activeSelf && flag)
				{
					m_guiBig.SetActive(true);
				}
				m_guiContainer.SetActive(flag);
			}
		}
		else
		{
			Object.Destroy(a_items[0].gameObject);
		}
		DisableShadows();
		UpdateResourceCounts();
	}

	public RemoteItem GetHandItem()
	{
		return m_handItem;
	}

	public RemoteItem GetItemFromPos(float a_x, float a_y)
	{
		if (m_items != null)
		{
			a_x = Mathf.Round(a_x);
			a_y = Mathf.Round(a_y);
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i])
				{
					zero = ToWorldPos(m_items[i].transform.localPosition);
					if (a_x == Mathf.Round(zero.x) && a_y == Mathf.Round(zero.z))
					{
						return m_items[i];
					}
				}
			}
		}
		return null;
	}

	public void SetShop(float a_buy, float a_sell)
	{
		m_shopBuyMultiplier = a_buy;
		m_shopSellMultiplier = a_sell;
	}

	public bool IsShopActive()
	{
		return m_shopBuyMultiplier != -1f || -1f != m_shopSellMultiplier;
	}

	public float GetShopBuyMultiplier()
	{
		return m_shopBuyMultiplier;
	}

	public float GetShopSellMultiplier()
	{
		return m_shopSellMultiplier;
	}

	public Vector3 ToWorldPos(Vector3 a_pos)
	{
		if (a_pos.x > 0f)
		{
			a_pos.x -= 0.15f;
		}
		a_pos *= 3.33333325f;
		a_pos.x = Mathf.Round(a_pos.x);
		a_pos.z = Mathf.Round(a_pos.y * -1f);
		a_pos.y = 0f;
		return a_pos;
	}

	public int GetInventoryItemCount()
	{
		int num = 0;
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && m_items[i].m_isInventoryItem)
				{
					num++;
				}
			}
		}
		return num;
	}

	public bool DragDrop(ref Vector3 a_startPos, ref Vector3 a_endPos)
	{
		if (m_items == null)
		{
			return false;
		}
		a_startPos = ToWorldPos(a_startPos);
		a_endPos = ToWorldPos(a_endPos);
		return true;
	}

	public void ShowInfo(Vector3 a_pos)
	{
		if (Vector3.zero == a_pos)
		{
			return;
		}
		m_hideInfoTime = Time.time + 8f;
		int num = -1;
		int a_amount = 0;
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && a_pos == m_items[i].transform.position)
				{
					num = m_items[i].m_type;
					a_amount = m_items[i].m_amountOrCond;
					break;
				}
			}
		}
		if (num != -1)
		{
			ItemDef itemDef = Items.GetItemDef(num);
			m_guiInfoName.text = LNG.Get(itemDef.ident);
			m_guiInfoDesc.text = LNG.Get(itemDef.ident + "_DESC");
			m_guiInfoStats.text = Items.GetStatsText(num, a_amount, true);
			m_guiItemInfo.SetActive(true);
		}
		else
		{
			m_guiItemInfo.SetActive(false);
		}
	}

	public int GetResourceCount(int a_resType)
	{
		return m_resourceCounts.Contains(a_resType) ? ((int)m_resourceCounts[a_resType]) : 0;
	}

	public int GetFreeSlots()
	{
		int num = 20;
		return num - GetInventoryItemCount();
	}

	public bool HasItemType(int a_type)
	{
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && a_type == m_items[i].m_type)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasFood()
	{
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && m_items[i].m_isInventoryItem && Items.IsEatable(m_items[i].m_type))
				{
					ItemDef itemDef = Items.GetItemDef(m_items[i].m_type);
					if (itemDef.healing > 0f)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasBuilding()
	{
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				if (null != m_items[i] && m_items[i].m_isInventoryItem)
				{
					ItemDef itemDef = Items.GetItemDef(m_items[i].m_type);
					if (itemDef.buildingIndex > 0)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsVisible()
	{
		return m_guiBig.activeSelf;
	}

	public void OpenSteamInventory()
	{
		m_guiBig.SetActive(true);
		m_guiSteamInventory.SetActive(true);
	}
}
