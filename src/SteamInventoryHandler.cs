using Steamworks;
using System.Collections.Generic;
using UnityEngine;

public class SteamInventoryHandler : MonoBehaviour
{
	private List<SteamItemDetails_t> m_itemDetails = new List<SteamItemDetails_t>();

	private SteamInventoryResult_t m_resultHandle;

	private bool m_waitForResult;

	private LidClient m_client;

	private void Start()
	{
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		if (Global.isSteamActive)
		{
			m_waitForResult = SteamInventory.GetAllItems(out m_resultHandle);
		}
	}

	private void Update()
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
			if (SteamInventory.GetResultItems(m_resultHandle, null, ref punOutItemsArraySize) && punOutItemsArraySize != 0)
			{
				SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
				SteamInventory.GetResultItems(m_resultHandle, array, ref punOutItemsArraySize);
				m_itemDetails.AddRange(array);
			}
			EquipSteamInventoryItems();
			break;
		}
		default:
			Debug.Log("SteamInventoryHandler.cs: Couldn't get inventory: " + resultStatus);
			break;
		}
		SteamInventory.DestroyResult(m_resultHandle);
		m_waitForResult = false;
	}

	private bool HasItemDef(int a_itemDef)
	{
		for (int i = 0; i < m_itemDetails.Count; i++)
		{
			SteamItemDetails_t steamItemDetails_t = m_itemDetails[i];
			if (steamItemDetails_t.m_iDefinition.m_SteamItemDef == a_itemDef)
			{
				return true;
			}
		}
		return false;
	}

	private void EquipSteamInventoryItems()
	{
		int num = PlayerPrefs.GetInt("prefLook", 0);
		int num2 = PlayerPrefs.GetInt("prefSkin", 0);
		if (num != 0 && !HasItemDef(num))
		{
			num = 0;
		}
		if (num2 != 0 && !HasItemDef(num2))
		{
			num2 = 0;
		}
		SetLook(num, num2);
	}

	public void SetLook(int a_hatItemId, int a_skinItemId)
	{
		int num = (a_hatItemId != 0) ? (a_hatItemId + 1 - 10000) : 0;
		int num2 = (a_skinItemId != 0) ? (a_skinItemId + 1 - 20000) : 0;
		string itemDefHash = Util.GetItemDefHash(num, m_client.GetSteamId());
		PlayerPrefs.SetInt("prefLook", a_hatItemId);
		PlayerPrefs.SetString("prefLookHash", itemDefHash);
		string itemDefHash2 = Util.GetItemDefHash(num2, m_client.GetSteamId());
		PlayerPrefs.SetInt("prefSkin", a_skinItemId);
		PlayerPrefs.SetString("prefSkinHash", itemDefHash2);
		m_client.SendSetLook(num, itemDefHash, num2, itemDefHash2);
	}

	public int GetLookItemDef()
	{
		return PlayerPrefs.GetInt("prefLook", 0);
	}

	public int GetSkinItemDef()
	{
		return PlayerPrefs.GetInt("prefSkin", 0);
	}
}
