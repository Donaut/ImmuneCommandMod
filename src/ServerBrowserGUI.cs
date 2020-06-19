using Steamworks;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ServerBrowserGUI : MonoBehaviour
{
	public bool m_testMode = true;

	public InputField m_inCustomIp;

	public Text m_txtMessage;

	public Text m_txtFootNote;

	public Text m_txtServerNames;

	public Text m_txtServerPlayers;

	public GameObject[] m_connectBtns;

	private List<string> m_serverIps = new List<string>();

	private LidClient m_client;

	private float m_lastListRefresh = -1000f;

	private string m_lastServerList = string.Empty;

	private string m_playerName = string.Empty;

	private ulong m_steamId;

	private string m_pwHash = string.Empty;

	private bool m_isVerbrecherVersion;

	public void OnBtnQuit()
	{
		if (!Application.isWebPlayer && !Application.isEditor)
		{
			Process.GetCurrentProcess().Kill();
		}
	}

	public void OnBtnRefresh()
	{
		RefreshServers();
	}

	public void OnBtnPlaySingleplayer()
	{
	}

	private void JoinPrivateServer()
	{
		Connect("127.0.0.1");
	}

	public void OnBtnConnect(int a_index)
	{
		if (a_index < m_serverIps.Count)
		{
			Connect(m_serverIps[a_index]);
		}
	}

	public void OnBtnCustomConnect()
	{
		Connect(m_inCustomIp.text);
	}

	public void OnBtnCommunityMarket()
	{
		OpenUrlInSteam("http://steamcommunity.com/market/search?appid=" + 348670);
	}

	public void OnBtnSteamInventory()
	{
		if (Global.isSteamActive)
		{
			object[] obj = new object[4]
			{
				"http://steamcommunity.com/profiles/",
				null,
				null,
				null
			};
			CSteamID steamID = SteamUser.GetSteamID();
			obj[1] = steamID.m_SteamID;
			obj[2] = "/inventory/#";
			obj[3] = 348670;
			OpenUrlInSteam(string.Concat(obj));
		}
	}

	private void OpenUrlInSteam(string a_url)
	{
		if (Global.isSteamActive)
		{
			SteamFriends.ActivateGameOverlayToWebPage(a_url);
		}
		else
		{
			UnityEngine.Debug.Log("DEBUG: " + a_url);
		}
	}

	private void Start()
	{
		m_inCustomIp.text = "127.0.0.1";
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_txtFootNote.text = "Immune version 1.0.1, please join the Community-Hub forums and give us feedback.";
		if (Application.isEditor && m_testMode)
		{
			m_steamId = 13376uL;
			m_pwHash = Util.Md5(m_steamId + "Version_0_4_8_B");
			m_playerName = "Hube'rt";
		}
		else if (SteamManager.Initialized)
		{
			Global.isSteamActive = true;
			CSteamID steamID = SteamUser.GetSteamID();
			m_steamId = steamID.m_SteamID;
			m_pwHash = Util.Md5(m_steamId + "Version_0_4_8_B");
			m_playerName = SteamFriends.GetPersonaName();
			SteamUserStats.RequestCurrentStats();
		}
		VerbrecherCheckStart();
		RedrawList();
		RefreshServers();
	}

	private void LateUpdate()
	{
		if (WebRequest.m_serverlist != m_lastServerList)
		{
			m_lastServerList = WebRequest.m_serverlist;
			RedrawList();
		}
	}

	private void Update()
	{
		if (null != m_client && string.Empty != m_client.m_disconnectMsg)
		{
			m_txtMessage.text = m_client.m_disconnectMsg;
			m_client.m_disconnectMsg = string.Empty;
		}
		VerbrecherCheckUpdate();
	}

	private void RefreshServers()
	{
		if (m_lastListRefresh < Time.time - 10f)
		{
			StartCoroutine(WebRequest.GetServers());
			m_lastListRefresh = Time.time;
		}
	}

	private void Connect(string a_ip)
	{
		if (m_steamId != 0L)
		{
			if (m_isVerbrecherVersion)
			{
				m_txtMessage.text = LNG.Get("INVALID_IP");
				return;
			}
			bool flag = m_client.Connect(m_playerName, m_pwHash, m_steamId, a_ip);
			m_txtMessage.text = ((!flag) ? LNG.Get("INVALID_IP") : LNG.Get("LOADING"));
		}
	}

	private void RedrawList()
	{
		m_txtServerNames.text = string.Empty;
		m_txtServerPlayers.text = string.Empty;
		m_serverIps.Clear();
		string empty = string.Empty;
		string[] array = m_lastServerList.Split(';');
		int num = -1;
		for (int i = 0; i < array.Length && !("STOP" == array[i]); i++)
		{
			if (-1 < num)
			{
				switch (num % 5)
				{
				case 0:
					m_serverIps.Add(array[i]);
					break;
				case 2:
				{
					empty = array[i];
					if (empty.StartsWith(" "))
					{
						empty = empty.Substring(1);
					}
					Text txtServerNames = m_txtServerNames;
					txtServerNames.text = txtServerNames.text + empty + "\n";
					break;
				}
				case 3:
				{
					Text txtServerPlayers = m_txtServerPlayers;
					txtServerPlayers.text = txtServerPlayers.text + GetServerPopulationString(int.Parse(array[i])) + "\n";
					break;
				}
				}
				num++;
			}
			if ("START" == array[i])
			{
				num = 0;
			}
		}
		for (int j = 0; j < m_connectBtns.Length; j++)
		{
			m_connectBtns[j].SetActive(j < m_serverIps.Count);
		}
		if (string.Empty == m_txtServerNames.text)
		{
			m_txtServerNames.text = LNG.Get("NO_SERVERS_FOUND");
		}
	}

	private string GetServerPopulationString(int a_playerCount)
	{
		string result = "<color=green>low</color>";
		if (a_playerCount == 50)
		{
			result = "<color=red>full</color>";
		}
		else if (19 < a_playerCount)
		{
			result = "<color=red>high</color>";
		}
		else if (4 < a_playerCount)
		{
			result = "<color=yellow>mid</color>";
		}
		return result;
	}

	private void VerbrecherCheckUpdate()
	{
		if (m_txtServerNames.text.ToLower().Contains("kortal") || m_txtFootNote.text.ToLower().Contains("kortal"))
		{
			m_isVerbrecherVersion = true;
		}
	}

	private void VerbrecherCheckStart()
	{
		if (m_txtServerNames.text.ToLower().Contains("kortal") || m_txtFootNote.text.ToLower().Contains("kortal") || m_steamId == 1685597 || m_steamId == 77484 || File.Exists("3dmgame.dll") || File.Exists("3DMGAME.ini") || File.Exists("Immune_Launcher.exe") || File.Exists("kortal.nfo"))
		{
			m_isVerbrecherVersion = true;
		}
	}
}
