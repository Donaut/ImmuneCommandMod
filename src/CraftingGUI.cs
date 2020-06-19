using System.Collections.Generic;
using UnityEngine;

public class CraftingGUI : MonoBehaviour
{
	public TextMesh m_txtItems;

	public TextMesh m_txtItem;

	public TextMesh m_txtRes;

	public TextMesh m_txtResNeed;

	public TextMesh m_txtResHaveEnough;

	public TextMesh m_txtResHaveTooLess;

	public TextMesh m_txtAmount;

	public TextMesh m_txtTimeLeft;

	public TextMesh m_txtCraft;

	public TextMesh m_txtInfoStats;

	public GameObject m_btnItems;

	public GameObject m_btnCraft;

	public GameObject m_btnMore;

	public GameObject m_btnLess;

	public GameObject[] m_btnPages;

	public Transform m_itemDisplayParent;

	public float m_minClickHeight = 0.21f;

	public float m_maxClickHeight = 0.75f;

	public AudioClip m_buildClip;

	public AudioClip m_failClip;

	private GameObject m_itemForDisplay;

	private GUI3dMaster m_guimaster;

	private InventoryGUI m_inventory;

	private LidClient m_client;

	private int m_activePage;

	private List<ItemDef>[] m_craftableItems;

	private List<int>[] m_craftableItemTypes;

	private string[] m_itemList;

	private int m_amount = 1;

	private int m_selectedItem;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_inventory = (InventoryGUI)Object.FindObjectOfType(typeof(InventoryGUI));
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_craftableItems = new List<ItemDef>[m_btnPages.Length];
		m_craftableItemTypes = new List<int>[m_btnPages.Length];
		m_itemList = new string[m_btnPages.Length];
		for (int i = 0; i < m_btnPages.Length; i++)
		{
			m_craftableItems[i] = new List<ItemDef>();
			m_craftableItemTypes[i] = new List<int>();
			m_itemList[i] = string.Empty;
		}
		m_txtItems.text = string.Empty;
		for (int j = 0; j < 254; j++)
		{
			ItemDef itemDef = Items.GetItemDef(j);
			if (Items.IsCraftable(j) && itemDef.ident != null)
			{
				m_craftableItems[itemDef.rankReq].Add(itemDef);
				m_craftableItemTypes[itemDef.rankReq].Add(j);
				string reference = m_itemList[itemDef.rankReq];
				reference = reference + LNG.Get(itemDef.ident) + "\n";
			}
		}
		ActivatePage(0);
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
		if (null != m_btnCraft && m_btnCraft.name == clickedButtonName)
		{
			int num = m_craftableItemTypes[m_activePage][m_selectedItem];
			int num2 = Items.IsStackable(num) ? 1 : m_amount;
			if (UpdateNeedHave())
			{
				if (m_inventory.GetFreeSlots() >= num2)
				{
					m_client.SendCraftRequest(num, m_amount);
					base.audio.clip = m_buildClip;
					base.audio.Play();
				}
				else
				{
					Debug.Log("too less space in inventory " + m_inventory.GetFreeSlots() + " < " + num2);
					base.audio.clip = m_failClip;
					base.audio.Play();
				}
			}
			else
			{
				base.audio.clip = m_failClip;
				base.audio.Play();
			}
			return;
		}
		if (null != m_btnMore && m_btnMore.name == clickedButtonName)
		{
			SetCraftAmount(m_amount + 1);
			return;
		}
		if (null != m_btnLess && m_btnLess.name == clickedButtonName)
		{
			SetCraftAmount(m_amount - 1);
			return;
		}
		if (null != m_btnItems && m_btnItems.name == clickedButtonName)
		{
			Vector3 mousePosition = Input.mousePosition;
			float num3 = (mousePosition.y / (float)Screen.height - m_minClickHeight) / (m_maxClickHeight - m_minClickHeight);
			int a_index = (int)((1f - num3) * 8f);
			ChooseItem(a_index);
			return;
		}
		for (int i = 0; i < m_btnPages.Length; i++)
		{
			if (null != m_btnPages[i] && m_btnPages[i].name == clickedButtonName)
			{
				ActivatePage(i);
			}
		}
	}

	private void ActivatePage(int a_index)
	{
		m_activePage = a_index;
		m_txtItems.text = m_itemList[m_activePage];
		ChooseItem(0);
	}

	private void FixedUpdate()
	{
		UpdateNeedHave();
	}

	private void ChooseItem(int a_index)
	{
		if (a_index < 0 || m_craftableItems[m_activePage].Count <= a_index)
		{
			return;
		}
		m_selectedItem = a_index;
		int num = m_craftableItemTypes[m_activePage][m_selectedItem];
		TextMesh txtItem = m_txtItem;
		ItemDef itemDef = m_craftableItems[m_activePage][m_selectedItem];
		txtItem.text = LNG.Get(itemDef.ident);
		SetCraftAmount(1);
		if (null != m_itemForDisplay)
		{
			Object.Destroy(m_itemForDisplay);
		}
		GameObject gameObject = (GameObject)Resources.Load("items/item_" + num);
		if (null != gameObject)
		{
			m_itemForDisplay = (GameObject)Object.Instantiate(gameObject, m_itemDisplayParent.position, Quaternion.identity);
			m_itemForDisplay.transform.parent = m_itemDisplayParent;
			m_itemForDisplay.transform.localScale = Vector3.one * 2f;
			m_itemForDisplay.transform.localRotation = Quaternion.identity;
			Renderer[] componentsInChildren = m_itemForDisplay.GetComponentsInChildren<Renderer>();
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				renderer.gameObject.layer = 17;
			}
			Transform transform = m_itemForDisplay.transform.FindChild("Particles");
			if (null != transform)
			{
				transform.gameObject.SetActive(false);
			}
			Transform transform2 = m_itemForDisplay.transform.FindChild("Point light");
			if (null != transform2)
			{
				transform2.gameObject.SetActive(false);
			}
			m_txtInfoStats.text = Items.GetStatsText(num, -1);
			ItemDef itemDef2 = Items.GetItemDef(num);
			if (itemDef2.ident != null && itemDef2.ident.Length > 0 && string.Empty == m_txtInfoStats.text)
			{
				m_txtInfoStats.text = LNG.Get(itemDef2.ident + "_DESC");
			}
		}
		UpdateNeedHave();
	}

	private bool UpdateNeedHave()
	{
		if (m_selectedItem < 0 || m_craftableItems[m_activePage].Count <= m_selectedItem)
		{
			return false;
		}
		m_txtRes.text = string.Empty;
		m_txtResNeed.text = string.Empty;
		m_txtResHaveEnough.text = string.Empty;
		m_txtResHaveTooLess.text = string.Empty;
		int num = 0;
		int num2 = 0;
		ItemDef itemDef = m_craftableItems[m_activePage][m_selectedItem];
		if (0 < itemDef.wood)
		{
			TextMesh txtRes = m_txtRes;
			string text = txtRes.text;
			ItemDef itemDef2 = Items.GetItemDef(130);
			txtRes.text = text + LNG.Get(itemDef2.ident) + "\n";
			num2 = m_inventory.GetResourceCount(130);
			ItemDef itemDef3 = m_craftableItems[m_activePage][m_selectedItem];
			num = itemDef3.wood * m_amount;
			TextMesh txtResNeed = m_txtResNeed;
			txtResNeed.text = txtResNeed.text + num + "\n";
			if (num > num2)
			{
				m_txtResHaveTooLess.text += num2;
			}
			else
			{
				m_txtResHaveEnough.text += num2;
			}
			m_txtResHaveTooLess.text += "\n";
			m_txtResHaveEnough.text += "\n";
		}
		ItemDef itemDef4 = m_craftableItems[m_activePage][m_selectedItem];
		if (0 < itemDef4.metal)
		{
			TextMesh txtRes2 = m_txtRes;
			string text2 = txtRes2.text;
			ItemDef itemDef5 = Items.GetItemDef(131);
			txtRes2.text = text2 + LNG.Get(itemDef5.ident) + "\n";
			num2 = m_inventory.GetResourceCount(131);
			ItemDef itemDef6 = m_craftableItems[m_activePage][m_selectedItem];
			num = itemDef6.metal * m_amount;
			TextMesh txtResNeed2 = m_txtResNeed;
			txtResNeed2.text = txtResNeed2.text + num + "\n";
			if (num > num2)
			{
				m_txtResHaveTooLess.text += num2;
			}
			else
			{
				m_txtResHaveEnough.text += num2;
			}
			m_txtResHaveTooLess.text += "\n";
			m_txtResHaveEnough.text += "\n";
		}
		ItemDef itemDef7 = m_craftableItems[m_activePage][m_selectedItem];
		if (0 < itemDef7.stone)
		{
			TextMesh txtRes3 = m_txtRes;
			string text3 = txtRes3.text;
			ItemDef itemDef8 = Items.GetItemDef(132);
			txtRes3.text = text3 + LNG.Get(itemDef8.ident) + "\n";
			num2 = m_inventory.GetResourceCount(132);
			ItemDef itemDef9 = m_craftableItems[m_activePage][m_selectedItem];
			num = itemDef9.stone * m_amount;
			TextMesh txtResNeed3 = m_txtResNeed;
			txtResNeed3.text = txtResNeed3.text + num + "\n";
			if (num > num2)
			{
				m_txtResHaveTooLess.text += num2;
			}
			else
			{
				m_txtResHaveEnough.text += num2;
			}
			m_txtResHaveTooLess.text += "\n";
			m_txtResHaveEnough.text += "\n";
		}
		ItemDef itemDef10 = m_craftableItems[m_activePage][m_selectedItem];
		if (0 < itemDef10.cloth)
		{
			TextMesh txtRes4 = m_txtRes;
			string text4 = txtRes4.text;
			ItemDef itemDef11 = Items.GetItemDef(133);
			txtRes4.text = text4 + LNG.Get(itemDef11.ident) + "\n";
			num2 = m_inventory.GetResourceCount(133);
			ItemDef itemDef12 = m_craftableItems[m_activePage][m_selectedItem];
			num = itemDef12.cloth * m_amount;
			TextMesh txtResNeed4 = m_txtResNeed;
			txtResNeed4.text = txtResNeed4.text + num + "\n";
			if (num > num2)
			{
				m_txtResHaveTooLess.text += num2;
			}
			else
			{
				m_txtResHaveEnough.text += num2;
			}
			m_txtResHaveTooLess.text += "\n";
			m_txtResHaveEnough.text += "\n";
		}
		int resourceCount = m_inventory.GetResourceCount(130);
		ItemDef itemDef13 = m_craftableItems[m_activePage][m_selectedItem];
		int num3;
		if (resourceCount >= itemDef13.wood * m_amount)
		{
			int resourceCount2 = m_inventory.GetResourceCount(131);
			ItemDef itemDef14 = m_craftableItems[m_activePage][m_selectedItem];
			if (resourceCount2 >= itemDef14.metal * m_amount)
			{
				int resourceCount3 = m_inventory.GetResourceCount(132);
				ItemDef itemDef15 = m_craftableItems[m_activePage][m_selectedItem];
				if (resourceCount3 >= itemDef15.stone * m_amount)
				{
					int resourceCount4 = m_inventory.GetResourceCount(133);
					ItemDef itemDef16 = m_craftableItems[m_activePage][m_selectedItem];
					num3 = ((resourceCount4 >= itemDef16.cloth * m_amount) ? 1 : 0);
					goto IL_0621;
				}
			}
		}
		num3 = 0;
		goto IL_0621;
		IL_06aa:
		bool flag;
		m_txtCraft.characterSize = ((!flag) ? 0.012f : 0.018f);
		return flag;
		IL_0621:
		flag = ((byte)num3 != 0);
		if (null != m_client)
		{
			int rank = m_client.GetRank();
			ItemDef itemDef17 = m_craftableItems[m_activePage][m_selectedItem];
			if (rank < itemDef17.rankReq)
			{
				m_txtCraft.text = LNG.Get("TOO_LOW_RANK");
				flag = false;
				goto IL_06aa;
			}
		}
		m_txtCraft.text = ((!flag) ? LNG.Get("TOO_LESS_RES") : LNG.Get("CRAFT"));
		goto IL_06aa;
	}

	private void SetCraftAmount(int a_amount)
	{
		m_amount = Mathf.Clamp(a_amount, 1, 99);
		m_txtAmount.text = m_amount.ToString();
		if (m_txtAmount.text.Length == 1)
		{
			m_txtAmount.text = "0" + m_txtAmount.text;
		}
		UpdateNeedHave();
	}
}
