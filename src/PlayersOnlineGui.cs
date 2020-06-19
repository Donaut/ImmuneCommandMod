using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersOnlineGui : MonoBehaviour
{
	private const string c_btnIdent = "btnpe_";

	private const string c_btnPage = "btn_page_";

	private const int c_playerEntitiesPerPage = 36;

	private const int c_maxNameLength = 16;

	public GameObject m_playerEntityTemplate;

	public TextMesh m_pageText;

	public TextMesh m_descriptionText;

	private bool m_invitePartyMode;

	private LidClient m_client;

	private GUI3dMaster m_guimaster;

	private Transform[] m_playerEntities = new Transform[36];

	private List<CharData> m_charData = new List<CharData>();

	private int m_page;

	private List<ulong> m_mutedSteamIds = new List<ulong>();

	private List<ulong> m_invitedSteamIds = new List<ulong>();

	private void Start()
	{
		m_guimaster = UnityEngine.Object.FindObjectOfType<GUI3dMaster>();
		m_playerEntityTemplate.SetActive(false);
	}

	private int UpdateList()
	{
		if (null != m_playerEntityTemplate)
		{
			for (int i = 0; i < 36; i++)
			{
				if (null != m_playerEntities[i])
				{
					UnityEngine.Object.Destroy(m_playerEntities[i].gameObject);
					m_playerEntities[i] = null;
				}
			}
			for (int j = 0; j < 36; j++)
			{
				int num = j + m_page * 36;
				if (num >= m_charData.Count)
				{
					break;
				}
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(m_playerEntityTemplate);
				gameObject.SetActive(true);
				m_playerEntities[j] = gameObject.transform;
				gameObject.transform.parent = m_playerEntityTemplate.transform.parent;
				gameObject.transform.localPosition = new Vector3(-0.216f + (float)(j / 12) * 0.19f, 0.15f - (float)(j % 12) * 0.028f, 0f);
				gameObject.transform.localRotation = Quaternion.identity;
				TextMesh componentInChildren = gameObject.GetComponentInChildren<TextMesh>();
				if (null != componentInChildren)
				{
					CharData charData = m_charData[num];
					componentInChildren.text = charData.name;
					if (16 < componentInChildren.text.Length)
					{
						componentInChildren.text = componentInChildren.text.Substring(0, 16) + "...";
					}
					List<ulong> invitedSteamIds = m_invitedSteamIds;
					CharData charData2 = m_charData[num];
					if (invitedSteamIds.Contains(charData2.aid))
					{
						componentInChildren.text = "<color=\"#66bb66\">" + componentInChildren.text + "</color>";
					}
					else
					{
						List<ulong> mutedSteamIds = m_mutedSteamIds;
						CharData charData3 = m_charData[num];
						if (mutedSteamIds.Contains(charData3.aid))
						{
							componentInChildren.text = "<color=\"#bb6666\">" + componentInChildren.text + "</color>";
						}
					}
				}
				GUI3dButton componentInChildren2 = gameObject.GetComponentInChildren<GUI3dButton>();
				if (null != componentInChildren2)
				{
					CharData charData4 = m_charData[num];
					componentInChildren2.name = "btnpe_" + charData4.aid;
				}
			}
		}
		return m_charData.Count;
	}

	public bool IsMuted(string a_name)
	{
		for (int i = 0; i < m_mutedSteamIds.Count; i++)
		{
			for (int j = 0; j < m_charData.Count; j++)
			{
				ulong num = m_mutedSteamIds[i];
				CharData charData = m_charData[j];
				if (num == charData.aid)
				{
					CharData charData2 = m_charData[j];
					if (a_name == charData2.name)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void OpenAsPlayerInvitation()
	{
		m_invitePartyMode = true;
		base.gameObject.SetActive(true);
	}

	private void OnEnable()
	{
		m_page = 0;
		m_invitedSteamIds.Clear();
		m_client = UnityEngine.Object.FindObjectOfType<LidClient>();
		if (null != m_client)
		{
			m_charData.Clear();
			for (int i = 0; i < m_client.m_playerData.Length; i++)
			{
				if (m_client.m_playerData[i].name != null && 1 < m_client.m_playerData[i].name.Length)
				{
					m_charData.Add(m_client.m_playerData[i]);
				}
			}
		}
		else if (m_charData.Count == 0)
		{
			for (int j = 0; j < 90; j++)
			{
				CharData item = default(CharData);
				item.name = "Ethan " + j + " the very very great!";
				item.aid = (ulong)(1337 + j);
				m_charData.Add(item);
			}
		}
		UpdateList();
		if (null != m_pageText)
		{
			m_pageText.text = "Page: <color=\"white\">";
			int num = (m_charData.Count - 1) / 36 + 1;
			for (int k = 0; k < num; k++)
			{
				TextMesh pageText = m_pageText;
				pageText.text = pageText.text + " " + (k + 1);
			}
			m_pageText.text += "</color>";
		}
		m_descriptionText.text = ((!m_invitePartyMode) ? LNG.Get("PLAYERS_ONLINE_DESC") : LNG.Get("PLAYERS_INVITE_DESC"));
	}

	private void OnDisable()
	{
		m_invitePartyMode = false;
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster))
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		string rightClickedButtonName = m_guimaster.GetRightClickedButtonName();
		string text = (!(string.Empty != clickedButtonName)) ? rightClickedButtonName : clickedButtonName;
		if (!(string.Empty != text))
		{
			return;
		}
		if (text.StartsWith("btnpe_") && null != m_client)
		{
			ulong num = 0uL;
			try
			{
				num = ulong.Parse(text.Substring("btnpe_".Length));
			}
			catch (Exception)
			{
			}
			if (0 >= num)
			{
				return;
			}
			if (m_invitePartyMode)
			{
				if (!m_invitedSteamIds.Contains(num) && m_client.GetSteamId() != num)
				{
					m_client.SendPartyRequest(ePartyControl.invite, num);
					m_invitedSteamIds.Add(num);
					UpdateList();
					m_descriptionText.text = LNG.Get("PARTY_SENT_INVITE");
				}
			}
			else if (string.Empty != clickedButtonName)
			{
				if (Global.isSteamActive)
				{
					SteamFriends.ActivateGameOverlayToUser("steamid", new CSteamID(num));
				}
			}
			else if (m_client.GetSteamId() != num)
			{
				if (m_mutedSteamIds.Contains(num))
				{
					m_mutedSteamIds.Remove(num);
				}
				else
				{
					m_mutedSteamIds.Add(num);
				}
				UpdateList();
			}
		}
		else if (text.StartsWith("btn_page_"))
		{
			try
			{
				m_page = int.Parse(text.Substring("btn_page_".Length)) - 1;
				UpdateList();
			}
			catch (Exception)
			{
			}
		}
	}
}
