using SimpleJSON;
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SteamInventoryGUI : MonoBehaviour
{
	public GameObject m_itemPrefab;

	public GameObject m_contextMenu;

	public TextMesh m_txtEquipOpen;

	public GameObject m_btnEquipOpen;

	public GameObject m_btnSell;

	public GameObject m_btnOpenCM;

	public GameObject m_btnBuyKey;

	public GameObject m_btnBuyKeys;

	public TextMesh m_txtPage;

	public TextMesh m_txtCaseCount;

	private GameObject[] m_items;

	private int m_curPage;

	private SteamInventoryResult_t m_resultHandle;

	private bool m_waitForResult;

	private bool m_completeRefresh;

	private bool m_caseOpenFlag;

	private int m_generatorDefId;

	private GUI3dMaster m_guimaster;

	private LidClient m_client;

	private CaseOpeningGUI m_caseOpenGui;

	private SteamInventoryHandler m_steamInventoryHandler;

	private List<SteamItemDetails_t> m_itemDetails = new List<SteamItemDetails_t>();

	private int m_contextMenuItemIndex = -1;

	private float m_nextPossibleRequestTime;

	private Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private void Start()
	{
		m_client = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
		m_guimaster = (GUI3dMaster)UnityEngine.Object.FindObjectOfType(typeof(GUI3dMaster));
		m_caseOpenGui = (CaseOpeningGUI)UnityEngine.Object.FindObjectOfType(typeof(CaseOpeningGUI));
		m_steamInventoryHandler = (SteamInventoryHandler)UnityEngine.Object.FindObjectOfType(typeof(SteamInventoryHandler));
		if (Global.isSteamActive)
		{
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
		RequestSteamInventory();
	}

	private void OnEnable()
	{
		m_contextMenu.SetActive(false);
		RequestSteamInventory();
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t a_callback)
	{
		if (a_callback.m_bActive == 0)
		{
			RequestSteamInventory();
		}
	}

	private void RequestSteamInventory()
	{
		if (Global.isSteamActive && Time.time > m_nextPossibleRequestTime)
		{
			m_waitForResult = SteamInventory.GetAllItems(out m_resultHandle);
			m_completeRefresh = true;
			m_nextPossibleRequestTime = Time.time + 10f;
		}
	}

	private void Update()
	{
		UpdateInventory();
		if (!m_waitForResult && !m_caseOpenGui.InProgress() && m_caseOpenFlag)
		{
			UpdateInventoryDisplay();
			m_caseOpenFlag = false;
		}
		if (!Input.GetKeyDown(KeyCode.P))
		{
			return;
		}
		if (Global.isSteamActive)
		{
			if ("vidiludi" == m_client.GetPlayerName() || "Ethan" == m_client.GetPlayerName() || "Editor" == m_client.GetPlayerName())
			{
				SteamItemDef_t[] array = new SteamItemDef_t[4];
				uint[] array2 = new uint[4];
				array[0].m_SteamItemDef = 2004;
				array[1].m_SteamItemDef = 2004;
				array[2].m_SteamItemDef = 3000;
				array[3].m_SteamItemDef = 3000;
				array2[0] = 1u;
				array2[1] = 1u;
				array2[2] = 1u;
				array2[3] = 1u;
				m_waitForResult = SteamInventory.GenerateItems(out m_resultHandle, array, null, 4u);
				m_completeRefresh = false;
			}
		}
		else
		{
			SteamItemDetails_t item = default(SteamItemDetails_t);
			item.m_iDefinition.m_SteamItemDef = 2004;
			item.m_unQuantity = 1;
			m_itemDetails.Add(item);
			for (int i = 0; i < 10; i++)
			{
				item.m_iDefinition.m_SteamItemDef = 20000 + i;
				item.m_unQuantity = 1;
				m_itemDetails.Add(item);
			}
			item.m_iDefinition.m_SteamItemDef = 2004;
			item.m_unQuantity = 1;
			m_itemDetails.Add(item);
			UpdateInventoryDisplay();
		}
	}

	private void UpdateInventory()
	{
		if (!Global.isSteamActive || !m_waitForResult)
		{
			return;
		}
		EResult resultStatus = SteamInventory.GetResultStatus(m_resultHandle);
		switch (resultStatus)
		{
		case EResult.k_EResultPending:
			return;
		case EResult.k_EResultOK:
		{
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(m_resultHandle, null, ref punOutItemsArraySize))
			{
				break;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			int num = 0;
			if (m_completeRefresh)
			{
				m_itemDetails.Clear();
			}
			if (punOutItemsArraySize != 0)
			{
				SteamInventory.GetResultItems(m_resultHandle, array, ref punOutItemsArraySize);
				if (m_completeRefresh)
				{
					m_itemDetails.AddRange(array);
				}
				else
				{
					for (int i = 0; i < array.Length; i++)
					{
						for (int j = 0; j < m_itemDetails.Count; j++)
						{
							ulong steamItemInstanceID = array[i].m_itemId.m_SteamItemInstanceID;
							SteamItemDetails_t steamItemDetails_t = m_itemDetails[j];
							if (steamItemInstanceID == steamItemDetails_t.m_itemId.m_SteamItemInstanceID && 0 < (array[i].m_unFlags & 0x100))
							{
								m_itemDetails.RemoveAt(j);
								break;
							}
						}
					}
					for (int k = 0; k < array.Length; k++)
					{
						if ((array[k].m_unFlags & 0x100) == 0)
						{
							m_itemDetails.Add(array[k]);
							num = array[k].m_iDefinition.m_SteamItemDef;
						}
					}
				}
				int lookItemDef = m_steamInventoryHandler.GetLookItemDef();
				int skinItemDef = m_steamInventoryHandler.GetSkinItemDef();
				bool flag = true;
				bool flag2 = true;
				int num2 = 0;
				for (int l = 0; l < m_itemDetails.Count; l++)
				{
					SteamItemDetails_t steamItemDetails_t2 = m_itemDetails[l];
					if (steamItemDetails_t2.m_iDefinition.m_SteamItemDef == lookItemDef)
					{
						flag = false;
					}
					else
					{
						SteamItemDetails_t steamItemDetails_t3 = m_itemDetails[l];
						if (steamItemDetails_t3.m_iDefinition.m_SteamItemDef == skinItemDef)
						{
							flag2 = false;
						}
					}
					SteamItemDetails_t steamItemDetails_t4 = m_itemDetails[l];
					if (3000 > steamItemDetails_t4.m_iDefinition.m_SteamItemDef)
					{
						num2++;
					}
				}
				if (flag2 || flag)
				{
					m_steamInventoryHandler.SetLook((!flag) ? lookItemDef : 0, (!flag2) ? skinItemDef : 0);
				}
				m_txtCaseCount.transform.parent.gameObject.SetActive(0 < num2);
				m_txtCaseCount.text = num2.ToString();
			}
			if (m_caseOpenFlag && num != 0)
			{
				m_caseOpenGui.Showtime(num, m_generatorDefId);
			}
			else
			{
				UpdateInventoryDisplay();
			}
			break;
		}
		default:
			Debug.Log("SteamInventoryGUI.cs: Couldn't get inventory: " + resultStatus);
			break;
		}
		SteamInventory.DestroyResult(m_resultHandle);
		m_waitForResult = false;
	}

	private void UpdateInventoryDisplay()
	{
		if (m_items != null)
		{
			for (int i = 0; i < m_items.Length; i++)
			{
				UnityEngine.Object.Destroy(m_items[i]);
			}
			m_items = null;
		}
		int num = m_curPage * 16;
		if (m_itemDetails != null && num < m_itemDetails.Count)
		{
			int num2 = num + 15;
			m_items = new GameObject[Mathf.Min(m_itemDetails.Count - num, 16)];
			for (int j = 0; j < m_itemDetails.Count; j++)
			{
				if (j >= num && j <= num2)
				{
					int num3 = j - num;
					SteamItemDetails_t steamItemDetails_t = m_itemDetails[j];
					JSONNode item = JsonItems.GetItem(steamItemDetails_t.m_iDefinition.m_SteamItemDef);
					if (null != item)
					{
						m_items[num3] = (GameObject)UnityEngine.Object.Instantiate(m_itemPrefab);
						m_items[num3].transform.parent = base.transform;
						m_items[num3].transform.localPosition = new Vector3(-0.01f + (float)(num3 % 4) * 0.278f, (float)(num3 / 4) * -0.278f, -0.01f);
						m_items[num3].transform.localRotation = Quaternion.identity;
						TextMesh componentInChildren = m_items[num3].GetComponentInChildren<TextMesh>();
						componentInChildren.text = string.Concat("<color=#", item["name_color"], ">", item["market_name"], "</color>");
						MeshCollider componentInChildren2 = m_items[num3].GetComponentInChildren<MeshCollider>();
						Material material = componentInChildren2.renderer.material;
						SteamItemDetails_t steamItemDetails_t2 = m_itemDetails[j];
						material.mainTexture = Resources.Load<Texture>("inventory_steam/inventory_s_" + steamItemDetails_t2.m_iDefinition.m_SteamItemDef);
						componentInChildren2.transform.name = "sii-" + j;
					}
				}
			}
		}
		m_txtPage.text = string.Empty;
		int num4 = (m_itemDetails.Count - 1) / 16 + 1;
		if (1 >= num4)
		{
			return;
		}
		for (int k = 1; k < num4 + 1; k++)
		{
			string text = k + " ";
			if (m_curPage + 1 == k)
			{
				text = "<color=\"#ffffff\">" + text + "</color>";
			}
			if (k < 10)
			{
				text = " " + text;
			}
			m_txtPage.text += text;
		}
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster))
		{
			return;
		}
		string text = m_guimaster.GetClickedButtonName();
		if (string.Empty == text)
		{
			text = m_guimaster.GetRightClickedButtonName();
		}
		if (!(string.Empty != text))
		{
			return;
		}
		m_contextMenu.SetActive(false);
		if (null != m_btnEquipOpen && text == m_btnEquipOpen.name)
		{
			if (EquipOrOpenItem(m_contextMenuItemIndex))
			{
				m_contextMenuItemIndex = -1;
			}
		}
		else if (null != m_btnSell && text == m_btnSell.name)
		{
			if (SellItem(m_contextMenuItemIndex))
			{
				m_contextMenuItemIndex = -1;
			}
		}
		else if (null != m_btnOpenCM && text == m_btnOpenCM.name)
		{
			OpenCommunityMarket();
		}
		else if (null != m_btnBuyKey && text == m_btnBuyKey.name)
		{
			BuyKey(1);
		}
		else if (null != m_btnBuyKeys && text == m_btnBuyKeys.name)
		{
			BuyKey(5);
		}
		else if (text.StartsWith("btn_page"))
		{
			int num = 0;
			while (true)
			{
				if (num < 14)
				{
					if ("btn_page" + (num + 1) == text)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			m_curPage = num;
			UpdateInventoryDisplay();
		}
		else
		{
			m_contextMenuItemIndex = GetClickedItemIndex(text);
		}
	}

	private bool EquipOrOpenItem(int a_index)
	{
		bool result = false;
		if (m_itemDetails != null && a_index > -1 && a_index < m_itemDetails.Count)
		{
			SteamItemDetails_t steamItemDetails_t = m_itemDetails[a_index];
			int steamItemDef = steamItemDetails_t.m_iDefinition.m_SteamItemDef;
			SteamItemDetails_t steamItemDetails_t2 = m_itemDetails[a_index];
			ulong steamItemInstanceID = steamItemDetails_t2.m_itemId.m_SteamItemInstanceID;
			if (10000 > steamItemDef)
			{
				if (Global.isSteamActive)
				{
					SteamItemDetails_t steamItemDetails_t3 = (steamItemDef != 3000) ? GetRandomItemFromInventory(3000, 3000) : GetRandomItemFromInventory(2000, 2999);
					ulong steamItemInstanceID2 = steamItemDetails_t3.m_itemId.m_SteamItemInstanceID;
					if (steamItemInstanceID2 != 0L)
					{
						m_generatorDefId = ((steamItemDef != 3000) ? steamItemDef : steamItemDetails_t3.m_iDefinition.m_SteamItemDef) - 1000;
						SteamItemInstanceID_t[] array = new SteamItemInstanceID_t[2];
						uint[] array2 = new uint[2];
						array[0].m_SteamItemInstanceID = steamItemInstanceID;
						array[1].m_SteamItemInstanceID = steamItemInstanceID2;
						array2[0] = 1u;
						array2[1] = 1u;
						SteamItemDef_t[] array3 = new SteamItemDef_t[1];
						uint[] array4 = new uint[1];
						array3[0].m_SteamItemDef = m_generatorDefId;
						array4[0] = 1u;
						m_waitForResult = SteamInventory.ExchangeItems(out m_resultHandle, array3, array4, 1u, array, array2, 2u);
						m_completeRefresh = false;
						m_caseOpenFlag = m_waitForResult;
					}
					else if (steamItemDef == 3000)
					{
						OpenCommunityMarket();
					}
					else
					{
						BuyKey(1);
					}
				}
				else
				{
					m_caseOpenGui.Showtime(steamItemDef, ((steamItemDef != 3000) ? steamItemDef : 2000) - 1000);
				}
			}
			else if (null != m_client)
			{
				int num = m_steamInventoryHandler.GetLookItemDef();
				int num2 = m_steamInventoryHandler.GetSkinItemDef();
				if (20000 <= steamItemDef)
				{
					num2 = ((num2 != steamItemDef) ? steamItemDef : 0);
				}
				else
				{
					num = ((num != steamItemDef) ? steamItemDef : 0);
				}
				m_steamInventoryHandler.SetLook(num, num2);
			}
			result = true;
		}
		return result;
	}

	private void BuyKey(int a_count)
	{
		OpenUrlInSteam("https://store.steampowered.com/buyitem/" + 348670 + "/" + 3000 + "/" + a_count + "/");
	}

	private void OpenCommunityMarket()
	{
		OpenUrlInSteam("http://steamcommunity.com/market/search?appid=" + 348670);
	}

	private void OpenUrlInSteam(string a_url)
	{
		if (Global.isSteamActive)
		{
			SteamFriends.ActivateGameOverlayToWebPage(a_url);
		}
		else
		{
			Debug.Log("DEBUG: " + a_url);
		}
	}

	private SteamItemDetails_t GetRandomItemFromInventory(int a_defIdFrom, int a_defIdTo)
	{
		if (m_itemDetails != null)
		{
			for (int i = 0; i < m_itemDetails.Count; i++)
			{
				SteamItemDetails_t steamItemDetails_t = m_itemDetails[i];
				if (steamItemDetails_t.m_iDefinition.m_SteamItemDef <= a_defIdTo)
				{
					SteamItemDetails_t steamItemDetails_t2 = m_itemDetails[i];
					if (steamItemDetails_t2.m_iDefinition.m_SteamItemDef >= a_defIdFrom)
					{
						return m_itemDetails[i];
					}
				}
			}
		}
		SteamItemDetails_t result = default(SteamItemDetails_t);
		result.m_itemId.m_SteamItemInstanceID = 0uL;
		return result;
	}

	private bool SellItem(int a_index)
	{
		bool result = false;
		if (m_itemDetails != null && a_index > -1 && a_index < m_itemDetails.Count)
		{
			ulong num = (!(null != m_client)) ? 12345678 : m_client.GetSteamId();
			object[] obj = new object[6]
			{
				"http://steamcommunity.com/profiles/",
				num,
				"/inventory#",
				348670,
				"_2_",
				null
			};
			SteamItemDetails_t steamItemDetails_t = m_itemDetails[a_index];
			obj[5] = steamItemDetails_t.m_itemId.m_SteamItemInstanceID;
			string text = string.Concat(obj);
			if (Global.isSteamActive)
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Debug.Log("DEBUG: " + text);
			}
			result = true;
		}
		return result;
	}

	private int GetClickedItemIndex(string a_clickedBtnName)
	{
		int result = -1;
		string[] array = a_clickedBtnName.Split('-');
		if (array != null && 1 < array.Length && "sii" == array[0])
		{
			int num = -1;
			try
			{
				num = int.Parse(array[1]);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("SteamInventoryGUI.cs: " + ex.ToString());
			}
			if (num > -1 && m_itemDetails != null && num < m_itemDetails.Count)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				m_contextMenu.transform.position = ray.GetPoint(3.5f);
				m_contextMenu.transform.localPosition += new Vector3(0.2f, -0.1f, 0f);
				SteamItemDetails_t steamItemDetails_t = m_itemDetails[num];
				int steamItemDef = steamItemDetails_t.m_iDefinition.m_SteamItemDef;
				string empty = string.Empty;
				if (10000 > steamItemDef)
				{
					empty = "STEAM_INV_OPEN_CASE";
				}
				else
				{
					int lookItemDef = m_steamInventoryHandler.GetLookItemDef();
					int skinItemDef = m_steamInventoryHandler.GetSkinItemDef();
					empty = ((lookItemDef != steamItemDef && skinItemDef != steamItemDef) ? "STEAM_INV_EQUIP" : "STEAM_INV_UNEQUIP");
				}
				m_txtEquipOpen.text = LNG.Get(empty);
				result = num;
				m_contextMenu.SetActive(true);
			}
		}
		return result;
	}
}
