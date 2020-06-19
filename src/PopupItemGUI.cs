using SimpleJSON;
using Steamworks;
using UnityEngine;

public class PopupItemGUI : MonoBehaviour
{
	public GameObject m_guiParent;

	public GameObject m_steamItemPrefab;

	public TextMesh m_reviewText;

	private GUI3dMaster m_guimaster;

	private InventoryGUI m_inventory;

	private TextMesh m_itemText;

	private MeshCollider m_itemRenderer;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_inventory = (InventoryGUI)Object.FindObjectOfType(typeof(InventoryGUI));
		GameObject gameObject = (GameObject)Object.Instantiate(m_steamItemPrefab);
		m_itemText = gameObject.GetComponentInChildren<TextMesh>();
		m_itemRenderer = gameObject.GetComponentInChildren<MeshCollider>();
		gameObject.transform.parent = m_guiParent.transform;
		gameObject.transform.localPosition = new Vector3(-0.29f, 0.245f, -0.04f);
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one * 0.8f;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ShowGui(false);
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.K))
		{
			ShowGui(true, 2000);
		}
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster) || !m_guiParent.activeSelf)
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		if (!(string.Empty != clickedButtonName))
		{
			return;
		}
		if ("btn_writereview" == clickedButtonName)
		{
			string text = "http://store.steampowered.com/recommended/recommendgame/348670";
			if (Global.isSteamActive)
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Debug.Log("DEBUG: " + text);
			}
		}
		else
		{
			if ("btn_open_inv" == clickedButtonName)
			{
				m_inventory.OpenSteamInventory();
			}
			ShowGui(false);
		}
	}

	public void ShowGui(bool a_show, int a_itemDefId = 0)
	{
		if (a_show)
		{
			JSONNode item = JsonItems.GetItem(a_itemDefId);
			if (null != item && null != m_itemText && null != m_itemRenderer)
			{
				m_itemText.text = string.Concat("<color=#", item["name_color"], ">", item["market_name"], "</color>");
				m_itemRenderer.renderer.material.mainTexture = Resources.Load<Texture>("inventory_steam/inventory_s_" + a_itemDefId);
			}
			base.audio.Play();
		}
		m_guiParent.SetActive(a_show);
		int @int = PlayerPrefs.GetInt("prefSteamDropCount", 0);
		PlayerPrefs.SetInt("prefSteamDropCount", @int + 1);
		m_reviewText.text = LNG.Get((@int % 2 != 1) ? "STEAM_BLUE_ICON" : "STEAM_PLEASE_REVIEW");
	}

	public bool IsActive()
	{
		return m_guiParent.activeSelf;
	}
}
