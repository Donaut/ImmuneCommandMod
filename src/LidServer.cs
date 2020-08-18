using Lidgren.Network;
using MyMod;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LidServer : LidgrenPeer
{
	private const float c_updateXradius = 22f;

	private const float c_updateZradius = 19f;

	private const float c_playerPickupRadius = 17.5f;

	private const float c_playerVehicleRadius = 17.5f;

	public GameObject m_controlledCharPrefab;

	public float m_updateIntervall = 0.2f;

	public float m_playerDbWriteIntervall = 10.5f;

	public float m_serverRestartTime = 86399f;

	public bool m_shutdownIfEmpty;

	private int m_restartMinutes = 5;

	private int m_maxPartyId = 1;

	private float m_dayNightCycleSpeed = 0.001f;

	private float m_nextPlayerDbWriteTime;

	private float m_nextServerListUpdate = 5f;

	private float m_nextItemUpdate = 5f;

	private NetServer m_server;

	private SQLThreadManager m_sql;

	private BuildingManager m_buildingMan;

	private MissionManager m_missionMan;

	private string m_serverName = string.Empty;

	private bool m_inited;

	private ServerPlayer[] m_players = new ServerPlayer[50];

	private ServerNpc[] m_npcs;

	private ServerVehicle[] m_vehicles;

	private ShopContainer[] m_shopContainers;

	private ServerBuilding[] m_staticBuildings;

	private SpawnPos[] m_spawnPoints;

	private SpecialArea[] m_specialAreas;

	private RepairingNpc[] m_repairNpcs;

    private ServerTutorial m_tutorial;

	private List<DatabaseItem> m_freeWorldItems = new List<DatabaseItem>();

	private Hashtable m_freeWorldContainers = new Hashtable();

	private Hashtable m_partys = new Hashtable();

	private float m_dayNightCycle;

	private Transform[] m_playerEntities = new Transform[36];

	private List<CharData> m_charData = new List<CharData>();
	private LidClient m_client;
	private void OnEnable()
	{
		Global.isServer = true;
		QualitySettings.SetQualityLevel(0);
		QualitySettings.vSyncCount = 0;
		m_sql = (SQLThreadManager)UnityEngine.Object.FindObjectOfType(typeof(SQLThreadManager));
		m_sql.enabled = true;
		Application.LoadLevel(1);
	}

	private void StartServer()
	{
		if (m_server == null)
		{
			NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration("immune");
			netPeerConfiguration.Port = 8844;
			netPeerConfiguration.MaximumConnections = 50;
			netPeerConfiguration.ConnectionTimeout = 10f;
			netPeerConfiguration.PingInterval = 1f;
			m_server = new NetServer(netPeerConfiguration);
			m_server.Start();
			SetPeer(m_server);
			base.Connected += onConnected;
			base.Disconnected += onDisconnected;
			RegisterMessageHandler(MessageIds.Auth, onAuth);
			RegisterMessageHandler(MessageIds.Input, onInput);
			RegisterMessageHandler(MessageIds.Craft, onCraft);
			RegisterMessageHandler(MessageIds.Chat, onChat);
			RegisterMessageHandler(MessageIds.ChatLocal, onChatLocal);
			RegisterMessageHandler(MessageIds.SpecialRequest, onSpecialRequest);
			RegisterMessageHandler(MessageIds.SetLook, onSetLook);
			RegisterMessageHandler(MessageIds.PartyControl, onPartyControl);
		}
	}

	private void Init()
	{
		m_npcs = (ServerNpc[])UnityEngine.Object.FindObjectsOfType(typeof(ServerNpc));
		m_shopContainers = (ShopContainer[])UnityEngine.Object.FindObjectsOfType(typeof(ShopContainer));
		m_vehicles = (ServerVehicle[])UnityEngine.Object.FindObjectsOfType(typeof(ServerVehicle));
		m_spawnPoints = (SpawnPos[])UnityEngine.Object.FindObjectsOfType(typeof(SpawnPos));
		m_specialAreas = (SpecialArea[])UnityEngine.Object.FindObjectsOfType(typeof(SpecialArea));
		m_tutorial = (ServerTutorial)UnityEngine.Object.FindObjectOfType(typeof(ServerTutorial));
		m_repairNpcs = (RepairingNpc[])UnityEngine.Object.FindObjectsOfType(typeof(RepairingNpc));
		m_missionMan = (MissionManager)UnityEngine.Object.FindObjectOfType(typeof(MissionManager));
		m_buildingMan = (BuildingManager)UnityEngine.Object.FindObjectOfType(typeof(BuildingManager));
		Renderer[] array = (Renderer[])UnityEngine.Object.FindObjectsOfType(typeof(Renderer));
		Renderer[] array2 = array;
		foreach (Renderer renderer in array2)
		{
			renderer.enabled = false;
		}
		SkinnedMeshRenderer[] array3 = (SkinnedMeshRenderer[])UnityEngine.Object.FindObjectsOfType(typeof(SkinnedMeshRenderer));
		SkinnedMeshRenderer[] array4 = array3;
		foreach (Renderer renderer2 in array4)
		{
			renderer2.enabled = false;
		}
		m_staticBuildings = (ServerBuilding[])UnityEngine.Object.FindObjectsOfType(typeof(ServerBuilding));
		StartServer();
		m_maxPartyId = m_sql.GetMaxPartyId();
		m_sql.RequestBuildings();
		if (m_vehicles != null)
		{
			for (int k = 0; k < m_vehicles.Length; k++)
			{
				m_vehicles[k].m_id = k;
			}
		}
		if (!Environment.CommandLine.Contains("-name"))
		{
			return;
		}
		string[] array5 = Environment.CommandLine.Split('-');
		int num = 0;
		while (true)
		{
			if (num < array5.Length)
			{
				if (array5[num].StartsWith("name"))
				{
					break;
				}
				num++;
				continue;
			}
			return;
		}
		m_serverName = array5[num].Substring(4);
	}

	private void OnApplicationQuit()
	{
		QualitySettings.SetQualityLevel(5);
		if (m_server != null)
		{
			if (!string.IsNullOrEmpty(m_serverName))
			{
				StartCoroutine(WebRequest.DeleteServer(m_server.Configuration.Port));
			}
			m_server.Shutdown("Server has shutdown.");
		}
	}

	private void Update()
	{
		if (Application.loadedLevel != 1)
		{
			return;
		}
		if (!m_inited)
		{
			Init();
			m_inited = true;
		}
		m_dayNightCycle += Time.deltaTime * m_dayNightCycleSpeed;
		if (m_dayNightCycle > 1f)
		{
			m_dayNightCycle -= 1f;
		}
		HandleDatabaseAnswers();
		if (Time.time > m_nextItemUpdate)
		{
			UpdateItems();
			m_nextItemUpdate = Time.time + 1.17f;
		}
		UpdatePlayers();
		if (Time.time > m_nextPlayerDbWriteTime)
		{
			if (m_server.ConnectionsCount > 0)
			{
				m_sql.SavePlayers(m_players, m_server.ConnectionsCount);
			}
			m_nextPlayerDbWriteTime = Time.time + m_playerDbWriteIntervall;
		}
		if (Time.time > m_nextServerListUpdate)
		{
			if (!string.IsNullOrEmpty(m_serverName))
			{
				StartCoroutine(WebRequest.UpdateServer(m_server.Configuration.Port, m_serverName, m_server.ConnectionsCount));
			}
			m_nextServerListUpdate = Time.time + 60.34f;
		}
		if (Time.time > m_serverRestartTime)
		{
			Application.Quit();
			m_serverRestartTime = Time.time + 999f;
		}
		else if (Time.time > m_serverRestartTime - (float)m_restartMinutes * 60f)
		{
			SendNotification(LNG.Get("SERVER_RESTART_X_MINUTES").Replace("{1}", m_restartMinutes.ToString()));
			m_restartMinutes--;
		}
	}

	private void onConnected(NetIncomingMessage a_msg)
	{
		a_msg.SenderConnection.Tag = -1;
	}

	private void onDisconnected(NetIncomingMessage a_msg)
	{
		bool flag = m_shutdownIfEmpty && 0 == m_server.ConnectionsCount;
		if (a_msg != null && a_msg.SenderConnection != null && a_msg.SenderConnection.Tag != null)
		{

			ServerPlayer player = GetPlayer((int)a_msg.SenderConnection.Tag);
			if (player != null)
			{
				if (Time.time > player.m_cantLogoutTime || flag)
				{
					DisconnectPlayer(player);

				}
				else
				{
					player.m_disconnectTime = player.m_cantLogoutTime;
				}
				
			}
		}
		if (flag)
		{
			Application.Quit();
		}
	}

	private void DisconnectPlayer(ServerPlayer a_player)
	{
		if (a_player == null)
		{
			return;
		}
		if (a_player.m_partyId != 0 && (!m_partys.Contains(a_player.m_partyId) || 2 > ((List<DatabasePlayer>)m_partys[a_player.m_partyId]).Count))
		{
			a_player.m_partyId = 0;
			a_player.m_partyRank = 0;
			if (m_partys.Contains(a_player.m_partyId))
			{
				m_partys.Remove(a_player.m_partyId);
			}
		}
		m_sql.SavePlayer(a_player);
		ServerVehicle vehicle = a_player.GetVehicle();
		if (null != vehicle && !a_player.ExitVehicle())
		{
			vehicle.DestroyCarAndForceExitPassengers();
		}
		NetOutgoingMessage netOutgoingMessage = m_server.CreateMessage();
		netOutgoingMessage.Write(MessageIds.RemoveClient);
		netOutgoingMessage.Write((byte)a_player.m_onlineId);
		m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered);
		DeletePlayer(a_player.m_onlineId);
		
	}

	private void onAuth(NetIncomingMessage msg)
	{
		string a_name = msg.ReadString();
		string a = msg.ReadString();
		ulong num = msg.ReadUInt64();
		string text = msg.ReadString();
		eCharType eCharType = (eCharType)msg.ReadByte();
		if (eCharType != 0 && eCharType != eCharType.ePlayerFemale)
		{
			eCharType = eCharType.ePlayer;
		}
		if (text != "1.0.1")
		{
			msg.SenderConnection.Disconnect("Version conflict! Client version: " + text + " Server version: 1.0.1");
		}
		else if (0 < num && a == Util.Md5(num + "Version_0_4_8_B"))
		{
			bool flag = false;
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i] != null && num == m_players[i].m_accountId)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				msg.SenderConnection.Disconnect("Auth Problem: Error 2");
				return;
			}
			bool flag2 = false;
			for (int j = 0; j < m_players.Length; j++)
			{
				if (m_players[j] == null)
				{
					m_players[j] = new ServerPlayer(a_name, num, j, eCharType, msg.SenderConnection, m_sql, this, m_buildingMan, m_missionMan);
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				m_sql.RequestPlayer(num);
				return;
			}
			Debug.Log("LidServer.cs: ERROR: Couldn't find free array element to place new player on?!");
			msg.SenderConnection.Disconnect("Server is full.");
		}
		else
		{
			msg.SenderConnection.Disconnect("Auth Problem: Error 1");
		}
	}

	private void onInput(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		if (player == null || !player.IsSpawned())
		{
			return;
		}
		player.m_cantLogoutTime = Time.time + 0.5f;
		int num = msg.ReadInt32();
		int num2 = msg.ReadInt16();
		float a_buildRotation = (float)(int)msg.ReadByte() / 255f * 360f;
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (msg.LengthBytes > 8)
		{
			zero.x = (int)msg.ReadByte();
			zero.z = (int)msg.ReadByte();
			zero2.x = (int)msg.ReadByte();
			zero2.z = (int)msg.ReadByte();
		}
		bool a_use = 1 == (num & 1);
		bool flag = 1 == ((num >> 1) & 1);
		bool flag2 = 1 == ((num >> 2) & 1);
		bool a_targetIsPlayer = 1 == ((num >> 3) & 1);
		bool flag3 = 1 == ((num >> 4) & 1);
		float num3 = (float)((num >> 8) & 0xFF) * 0.01f - 1f;
		float num4 = (float)((num >> 16) & 0xFF) * 0.01f - 1f;
		if (Mathf.Abs(num3) < 0.01f)
		{
			num3 = 0f;
		}
		if (Mathf.Abs(num4) < 0.01f)
		{
			num4 = 0f;
		}
		float a_attackRot = -1f;
		if (!flag2)
		{
			if (flag3)
			{
				a_attackRot = (float)num2 / 255f * 360f;
			}
			num2 = -1;
		}
		if ((num3 != 0f || num4 != 0f || flag) && (player.m_freeWorldContainer != null || player.m_persistentContainer != null || null != player.m_shopContainer))
		{
			player.ResetContainers();
		}
		player.AssignInput(num3, num4, a_use, flag, a_buildRotation);
		HandlePlayerInput(player, a_use, flag, a_targetIsPlayer, num2, a_attackRot, zero, zero2);
	}

	private void onChat(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		if (player != null && player.IsSpawned())
		{
			string text = msg.ReadString();
			if (!string.IsNullOrEmpty(text))
			{
				text = ((!text.StartsWith(":#~")) ? (player.m_name + "§ " + text.Replace("<", string.Empty).Replace(">", string.Empty)) : (player.m_name + " just opened a case and received: \n<color=\"red\">" + text.Substring(3) + "</color>"));
				NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
				netOutgoingMessage.Write(MessageIds.Chat);
				netOutgoingMessage.Write(text);
				m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.Unreliable);
			}
		}
	}

	public void SendMessageToPlayerLocal(string text, ServerPlayer player, NetIncomingMessage msg)
    {
		NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.ChatLocal);
		netOutgoingMessage.Write((byte)player.m_onlineId);
		netOutgoingMessage.Write(text);
		m_server.SendMessage(netOutgoingMessage, player.m_connection, NetDeliveryMethod.Unreliable);
	}

	public void SendMessageToPlayerChat(string text, ServerPlayer player, NetIncomingMessage msg)
	{
		NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.Chat);
		netOutgoingMessage.Write((byte)player.m_onlineId);
		netOutgoingMessage.Write(text);
		m_server.SendMessage(netOutgoingMessage, player.m_connection, NetDeliveryMethod.Unreliable);
	}


	private void onChatLocal(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		if (player == null || !player.IsSpawned())
		{
			return;
		}
		string text = msg.ReadString();
		if (!string.IsNullOrEmpty(text))
		{
			if (text.StartsWith("/"))
			{
				HandleChatCommand(text, player, msg);
				ChatCommandExtensions.HandleChatCommand(text, player, msg);
				return;
			}
			NetOutgoingMessage netOutgoingMessage = msg.SenderConnection.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.Chat);
			netOutgoingMessage.Write((byte)player.m_onlineId);
			netOutgoingMessage.Write(text);
			m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.Unreliable);
			//m_server.SendMessage(netOutgoingMessage, player.m_connection, NetDeliveryMethod.Unreliable);
		}
	}

	private void onSpecialRequest(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		eSpecialRequest eSpecialRequest = (eSpecialRequest)msg.ReadByte();
		if (player == null || !player.IsSpawned())
		{
			return;
		}
		switch (eSpecialRequest)
		{
		case eSpecialRequest.repairItem:
		{
			DatabaseItem itemFromPos = player.m_inventory.GetItemFromPos(0f, 0f);
			int num = 0;
			while (true)
			{
				if (num < m_repairNpcs.Length)
				{
					Vector3 position = m_repairNpcs[num].transform.position;
					Vector3 position2 = player.GetPosition();
					if (Mathf.Abs(position.x - position2.x) < 1.4f && Mathf.Abs(position.z - position2.z) < 1.4f && itemFromPos.type != 0 && Items.HasCondition(itemFromPos.type) && itemFromPos.amount < 100)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			int num2 = (int)(1f + Items.GetValue(itemFromPos.type, 100) * 0.01f * (float)(100 - itemFromPos.amount));
			num2 = (int)((float)num2 * m_repairNpcs[num].m_priceMultip + 0.5f);
			if (num2 <= player.m_inventory.GetItemAmountByType(254))
			{
				player.m_inventory.DeclineItemAmountByType(254, num2);
				player.m_inventory.RepairHandItem();
				player.m_updateContainersFlag = true;
				SendMoneyUpdate(player);
			}
			break;
		}
		case eSpecialRequest.acceptMission:
			m_missionMan.AcceptMission(player);
			break;
		case eSpecialRequest.solveMission:
			m_missionMan.SolveMission(player);
			break;
		case eSpecialRequest.acceptPartyInvite:
			if (m_partys.Contains(player.m_partyInviteId))
			{
				List<DatabasePlayer> list = (List<DatabasePlayer>)m_partys[player.m_partyInviteId];
				if (5 > list.Count)
				{
					player.m_partyId = player.m_partyInviteId;
					player.m_partyInviteId = 0;
					player.m_partyRank = 0;
					m_sql.SavePlayer(player, true);
					DatabasePlayer item = new DatabasePlayer(player.m_accountId, player.m_name, player.m_pid);
					item.partyId = player.m_partyInviteId;
					item.partyRank = player.m_partyRank;
					list.Add(item);
					UpdateParty(list);
				}
				else
				{
					SendPartyFeedback(player, ePartyFeedback.partyFull, string.Empty);
				}
			}
			break;
		}
	}

	private void onSetLook(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		int num = msg.ReadByte();
		string a = msg.ReadString();
		int num2 = msg.ReadByte();
		string a2 = msg.ReadString();
		if (player != null && player.IsSpawned() && a == Util.GetItemDefHash(num, player.m_accountId) && a2 == Util.GetItemDefHash(num2, player.m_accountId))
		{
			player.m_lookIndex = num;
			player.m_skinIndex = num2;
			player.m_updateInfoFlag = true;
		}
	}

	private void onPartyControl(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		ePartyControl ePartyControl = (ePartyControl)msg.ReadByte();
		ulong num = msg.ReadUInt64();
		if (ePartyControl == ePartyControl.invite)
		{
			if (player.m_partyId == 0)
			{
				player.m_partyId = ++m_maxPartyId;
				player.m_partyRank = 1;
				m_sql.SavePlayer(player, true);
				List<DatabasePlayer> list = new List<DatabasePlayer>(1);
				DatabasePlayer item = new DatabasePlayer(player.m_accountId, string.Empty);
				item.pid = player.m_pid;
				item.name = player.m_name;
				item.partyId = player.m_partyId;
				item.partyRank = player.m_partyRank;
				list.Add(item);
				m_partys[player.m_partyId] = list;
				SendPartyUpdate(player, list.ToArray());
			}
			ServerPlayer playerByAid = GetPlayerByAid(num);
			int partyId = playerByAid.m_partyId;
			if (playerByAid != null && (partyId == 0 || !m_partys.Contains(partyId) || 2 > ((List<DatabasePlayer>)m_partys[partyId]).Count))
			{
				playerByAid.m_partyInviteId = player.m_partyId;
				SendPartyFeedback(playerByAid, ePartyFeedback.invite, player.m_name);
			}
			else
			{
				SendPartyFeedback(player, ePartyFeedback.errorAlreadyInParty, playerByAid.m_name);
			}
		}
		else
		{
			if (player == null || !player.IsSpawned() || !(Time.time > player.m_nextPartyActionTime) || player.m_partyId == 0 || !m_partys.Contains(player.m_partyId) || (player.m_partyRank != 1 && (ePartyControl != ePartyControl.kick || player.m_accountId != num)))
			{
				return;
			}
			player.m_nextPartyActionTime = Time.time + 0.5f;
			List<DatabasePlayer> list2 = (List<DatabasePlayer>)m_partys[player.m_partyId];
			int num2 = 0;
			while (true)
			{
				if (num2 < list2.Count)
				{
					DatabasePlayer databasePlayer = list2[num2];
					if (num == databasePlayer.aid)
					{
						break;
					}
					num2++;
					continue;
				}
				return;
			}
			DatabasePlayer databasePlayer2 = list2[num2];
			ServerPlayer playerByAid2 = GetPlayerByAid(databasePlayer2.aid);
			switch (ePartyControl)
			{
			case ePartyControl.kick:
				databasePlayer2.partyId = 0;
				databasePlayer2.partyRank = 0;
				list2.RemoveAt(num2);
				if (playerByAid2 != null)
				{
					playerByAid2.m_partyId = 0;
					playerByAid2.m_partyRank = 0;
					SendPartyUpdate(playerByAid2, null);
					if (player.m_accountId != num)
					{
						SendPartyFeedback(playerByAid2, ePartyFeedback.kicked, player.m_name);
					}
				}
				break;
			case ePartyControl.prodemote:
				databasePlayer2.partyRank = ((databasePlayer2.partyRank == 0) ? 1 : 0);
				list2[num2] = databasePlayer2;
				if (playerByAid2 != null)
				{
					playerByAid2.m_partyRank = databasePlayer2.partyRank;
					if (player.m_accountId != num)
					{
						SendPartyFeedback(playerByAid2, ePartyFeedback.prodemoted, player.m_name);
					}
				}
				break;
			}
			m_sql.SavePartyPlayer(databasePlayer2);
			UpdateParty(list2);
		}
	}

	private void onCraft(NetIncomingMessage msg)
	{
		ServerPlayer player = GetPlayer((int)msg.SenderConnection.Tag);
		if (player != null && player.IsSpawned())
		{
			int num = player.m_inventory.CraftItem(msg.ReadByte(), msg.ReadByte());
			player.AddXp((int)((float)num * 1.0001f));
			player.ResetContainers();
		}
	}

	private void HandleDatabaseAnswers()
	{
		DatabaseBuilding[] array = m_sql.PopRequestedBuildings();
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				m_buildingMan.CreateBuilding(array[i].type, array[i].GetPos(), array[i].pid, array[i].rot, array[i].health, false);
			}
		}
		DatabasePlayer[] array2 = m_sql.PopRequestedPlayers();
		if (array2 != null)
		{
			for (int j = 0; j < array2.Length; j++)
			{
				ServerPlayer a_player = SpawnPlayer(array2[j]);
				if (0 >= array2[j].partyId)
				{
					continue;
				}
				if (m_partys.Contains(array2[j].partyId))
				{
					List<DatabasePlayer> list = (List<DatabasePlayer>)m_partys[array2[j].partyId];
					if (list != null && 0 < list.Count)
					{
						SendPartyUpdate(a_player, list.ToArray());
					}
				}
				else
				{
					m_sql.RequestParty(array2[j].partyId);
				}
			}
		}
		array2 = m_sql.PopRequestedParty();
		if (array2 != null && 0 < array2.Length)
		{
			List<DatabasePlayer> list2 = new List<DatabasePlayer>(array2.Length);
			list2.AddRange(array2);
			m_partys[array2[0].partyId] = list2;
			UpdateParty(list2);
		}
		DatabaseItem[] array3 = m_sql.PopRequestedItems();
		if (array3 == null)
		{
			return;
		}
		for (int k = 0; k < array3.Length; k++)
		{
			if (array3[k].cid == 0)
			{
				m_freeWorldItems.Add(array3[k]);
				continue;
			}
			if (SQLThreadManager.IsInventoryContainer(array3[k].cid))
			{
				for (int l = 0; l < m_players.Length; l++)
				{
					if (m_players[l] != null && array3[k].cid == SQLThreadManager.PidToCid(m_players[l].m_pid))
					{
						m_players[l].m_inventory.UpdateOrCreateItem(array3[k]);
						m_players[l].m_updateInfoFlag |= (array3[k].x < 1f);
						m_players[l].m_updateContainersFlag = true;
						break;
					}
				}
				continue;
			}
			Lootbox lootbox = m_buildingMan.AddItemToLootContainer(array3[k]);
			if (!(null != lootbox))
			{
				continue;
			}
			for (int m = 0; m < m_players.Length; m++)
			{
				if (m_players[m] != null && m_players[m].m_persistentContainer == lootbox.m_container)
				{
					m_players[m].m_updateContainersFlag = true;
				}
			}
		}
	}

	private void UpdateParty(List<DatabasePlayer> a_party)
	{
		if (a_party == null || 0 >= a_party.Count)
		{
			return;
		}
		DatabasePlayer databasePlayer = a_party[0];
		int partyId = databasePlayer.partyId;
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && m_players[i].m_partyId == partyId)
			{
				SendPartyUpdate(m_players[i], a_party.ToArray());
			}
		}
	}

	private void HandlePlayerInput(ServerPlayer a_player, bool a_use, bool a_attack, bool a_targetIsPlayer, int a_targetOnlineId, float a_attackRot, Vector3 a_dragPos, Vector3 a_dropPos)
	{
		Transform victim = null;
		if (!a_player.IsDead())
		{
			bool flag = a_use && a_player.HasUseChanged();
			if (null == a_player.GetVehicle())
			{
				if (flag && !TryEnterVehicle(a_player))
				{
					DatabaseItem databaseItem = PickupItem(a_player, null);
					if (databaseItem.type == 0 && !OpenShopContainer(a_player) && !m_buildingMan.UseBuilding(a_player, a_player.GetPosition() + a_player.GetForward()) && !m_missionMan.RequestMission(a_player))
					{
						m_missionMan.SolveMission(a_player, true);
					}
				}
				if (a_attack && a_targetOnlineId != -1)
				{
					if (a_targetIsPlayer)
					{
						ServerPlayer player = GetPlayer(a_targetOnlineId);
						victim = ((player == null || !player.IsSpawned()) ? null : player.GetTransform());
					}
					else
					{
						ServerNpc npc = GetNpc(a_targetOnlineId);
						victim = ((!(null != npc)) ? null : npc.transform);
					}
				}
			}
			else if (flag)
			{
				a_player.ExitVehicle();
			}
			if (a_dragPos != a_dropPos)
			{
				HandlePlayerDragDrop(a_player, a_dragPos, a_dropPos);
			}
		}
		a_player.SetVictim(victim);
		a_player.SetRotation(a_attackRot);
	}

	private void HandlePlayerDragDrop(ServerPlayer a_player, Vector3 a_dragPos, Vector3 a_dropPos)
	{
		bool flag = false;
		Vector3 vector = a_dragPos;
		Vector3 a_dropPos2 = a_dropPos;
		if ((a_dropPos - new Vector3(252f, 0f, 252f)).sqrMagnitude < 0.1f)
		{
			flag = BuySellItem(vector, a_player);
		}
		else
		{
			bool flag2 = (a_dropPos - new Vector3(253f, 0f, 253f)).sqrMagnitude < 0.1f;
			bool flag3 = (a_dropPos - new Vector3(254f, 0f, 254f)).sqrMagnitude < 0.1f;
			ItemContainer itemContainer = a_player.m_inventory;
			ItemContainer itemContainer2 = null;
			ItemContainer itemContainer3 = null;
			bool flag4 = false;
			if (a_player.m_persistentContainer != null)
			{
				itemContainer3 = a_player.m_persistentContainer;
				flag4 = false;
			}
			else if (a_player.m_freeWorldContainer != null)
			{
				itemContainer3 = a_player.m_freeWorldContainer;
				flag4 = true;
			}
			if (itemContainer3 != null)
			{
				if (a_dragPos.x > 5f)
				{
					itemContainer = itemContainer3;
					itemContainer2 = a_player.m_inventory;
				}
				else
				{
					itemContainer2 = itemContainer3;
				}
				for (int i = 0; i < m_players.Length; i++)
				{
					if (m_players[i] != null && ((flag4 && m_players[i].m_freeWorldContainer == a_player.m_freeWorldContainer) || (!flag4 && m_players[i].m_persistentContainer == a_player.m_persistentContainer)))
					{
						m_players[i].m_updateContainersFlag = true;
					}
				}
			}
			if (flag3)
			{
				itemContainer.SplitItem(vector);
			}
			else if (flag2)
			{
				flag = itemContainer.EatItem(vector, a_player);
			}
			else
			{
				DatabaseItem item = itemContainer.DragDrop(vector, a_dropPos2, itemContainer2, a_player.GetPosition());
				if (0 < item.type)
				{
					m_freeWorldItems.Add(item);
				}
				else if (itemContainer2 != null)
				{
					SendMoneyUpdate(a_player);
				}
				flag = true;
			}
			if (flag4 && itemContainer3 != null && itemContainer3.m_items.Count < 1)
			{
				DeleteFreeWorldItem(itemContainer3.m_position, true);
			}
		}
		if (flag && (vector.x < 1f || a_dropPos2.x < 1f))
		{
			SendPlayerInfo(a_player);
		}
		a_player.m_updateContainersFlag = true;
	}

	private bool BuySellItem(Vector3 a_pos, ServerPlayer a_player)
	{
		bool flag = false;
		if (a_player != null && a_player.m_inventory != null && null != a_player.m_shopContainer)
		{
			ShopContainer shopContainer = a_player.m_shopContainer;
			if (null != shopContainer && shopContainer.m_container != null)
			{
				if (a_pos.x < 6f)
				{
					DatabaseItem itemFromPos = a_player.m_inventory.GetItemFromPos(a_pos.x, a_pos.z);
					if (0 < itemFromPos.type)
					{
						a_player.m_inventory.DeleteItem(a_pos.x, a_pos.z);
						int num = (int)(Items.GetValue(itemFromPos.type, itemFromPos.amount) * shopContainer.m_sellPriceMuliplier + 0.5f);
						while (num > 0)
						{
							int a_amount = (num <= 254) ? num : 254;
							num -= 254;
							DatabaseItem a_item = new DatabaseItem(254, 0f, 0f, a_amount);
							if (!a_player.m_inventory.CollectItem(a_item, true))
							{
								CreateFreeWorldItem(254, a_amount, a_player.GetPosition());
							}
						}
						if (shopContainer.m_container.HasFreeSlots())
						{
							shopContainer.m_container.CollectItem(itemFromPos, false);
						}
						flag = true;
					}
				}
				else
				{
					DatabaseItem itemFromPos2 = shopContainer.m_container.GetItemFromPos(a_pos.x, a_pos.z);
					if (0 < itemFromPos2.type)
					{
						int num2 = (int)(Items.GetValue(itemFromPos2.type, itemFromPos2.amount) * shopContainer.m_buyPriceMuliplier + 0.5f);
						if (num2 <= a_player.m_inventory.GetItemAmountByType(254))
						{
							a_player.m_inventory.DeclineItemAmountByType(254, num2);
							shopContainer.m_container.DeleteItem(a_pos.x, a_pos.z);
							if (!a_player.m_inventory.CollectItem(itemFromPos2, true))
							{
								CreateFreeWorldItem(itemFromPos2.type, itemFromPos2.amount, a_player.GetPosition());
							}
							flag = true;
						}
					}
				}
			}
		}
		if (flag)
		{
			SendMoneyUpdate(a_player);
		}
		return flag;
	}

	private void SendPlayerInfo(ServerPlayer a_player)
	{
		NetOutgoingMessage netOutgoingMessage = m_server.CreateMessage();
		netOutgoingMessage.Write(MessageIds.SetPlayerInfo);
		netOutgoingMessage.Write((byte)a_player.m_onlineId);
		AddPlayerInfoToMsg(netOutgoingMessage, a_player);
		m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.Unreliable);
	}

	private ServerPlayer SpawnPlayer(DatabasePlayer a_dbplayer)
	{
		ServerPlayer serverPlayer = null;
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].m_accountId == a_dbplayer.aid)
			{
				serverPlayer = m_players[i];
				break;
			}
		}
		if (serverPlayer != null && serverPlayer.m_onlineId != -1)
		{
			serverPlayer.Spawn(m_controlledCharPrefab, a_dbplayer);
			SendPlayerAndNpcData(serverPlayer);
			m_missionMan.UpdatePlayer(serverPlayer);
			NetOutgoingMessage netOutgoingMessage = m_server.CreateMessage();
			netOutgoingMessage.Write(MessageIds.SetPlayerName);
			netOutgoingMessage.Write((byte)serverPlayer.m_onlineId);
			netOutgoingMessage.Write(serverPlayer.m_name);
			netOutgoingMessage.Write(serverPlayer.m_accountId);
			m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered);
			m_sql.RequestContainer(SQLThreadManager.PidToCid(serverPlayer.m_pid));
			SendNotification(LNG.Get("WELCOME_PLAYER").Replace("{p_name}", serverPlayer.m_name.ToString()));
		}
		else
		{
			Debug.Log("LidServer.cs: Error: Can't init/spawn player because it's null or onlineId is -1.");
		}
		return serverPlayer;
	}

	private void UpdatePlayers()
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] == null || !m_players[i].IsSpawned())
			{
				continue;
			}
			if (0f < m_players[i].m_disconnectTime)
			{
				if (Time.time > m_players[i].m_disconnectTime)
				{
					DisconnectPlayer(m_players[i]);
				}
				continue;
			}
			if (m_players[i].m_nextUpdate < Time.time)
			{
				m_players[i].Progress(m_updateIntervall);
				bool flag = 0 == ++m_players[i].m_updateCount % 2;
				NetOutgoingMessage netOutgoingMessage = m_players[i].m_connection.Peer.CreateMessage();
				netOutgoingMessage.Write((!flag) ? MessageIds.UpdateB : MessageIds.UpdateA);
				AddOwnPlayerToMsg(netOutgoingMessage, m_players[i]);
				AddVehiclesToMsg(netOutgoingMessage, m_players[i]);
				if (flag)
				{
					AddPlayersToMsg(netOutgoingMessage, m_players[i]);
				}
				else
				{
					UpdatePlayerInventory(m_players[i]);
					AddNpcsItemsBuildingsToMsg(netOutgoingMessage, m_players[i]);
				}
				m_players[i].m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
				m_players[i].m_nextUpdate = Time.time + m_updateIntervall;
			}
			if (m_players[i].m_updateInfoFlag)
			{
				m_players[i].m_updateInfoFlag = false;
				SendPlayerInfo(m_players[i]);
			}
		}
	}

	private void UpdatePlayerInventory(ServerPlayer a_player)
	{
		if (a_player == null || a_player.m_inventory == null)
		{
			return;
		}
		int num = 0;
		while (true)
		{
			if (num >= a_player.m_inventory.m_items.Count)
			{
				return;
			}
			DatabaseItem databaseItem = a_player.m_inventory.m_items[num];
			if (databaseItem.flag != eDbAction.delete)
			{
				DatabaseItem databaseItem2 = a_player.m_inventory.m_items[num];
				if (databaseItem2.flag != eDbAction.update)
				{
					goto IL_00dc;
				}
			}
			a_player.m_updateContainersFlag = true;
			m_sql.SaveItem(a_player.m_inventory.m_items[num]);
			DatabaseItem databaseItem3 = a_player.m_inventory.m_items[num];
			if (databaseItem3.flag == eDbAction.delete)
			{
				break;
			}
			DatabaseItem value = a_player.m_inventory.m_items[num];
			value.flag = eDbAction.none;
			a_player.m_inventory.m_items[num] = value;
			goto IL_00dc;
			IL_00dc:
			num++;
		}
		a_player.m_inventory.m_items.RemoveAt(num);
	}

	private void UpdateItems()
	{
		float time = Time.time;
		for (int i = 0; i < m_freeWorldItems.Count; i++)
		{
			DatabaseItem databaseItem = m_freeWorldItems[i];
			if (!(time > databaseItem.dropTime + 6f))
			{
				continue;
			}
			DatabaseItem databaseItem2 = m_freeWorldItems[i];
			if (databaseItem2.amount == 1)
			{
				int num = -1;
				DatabaseItem databaseItem3 = m_freeWorldItems[i];
				if (databaseItem3.type == 2)
				{
					num = 60;
				}
				DatabaseItem databaseItem4 = m_freeWorldItems[i];
				if (databaseItem4.type == 1)
				{
					num = 61;
				}
				if (num != -1 && m_buildingMan.CreateBuilding(num, m_freeWorldItems[i].GetPos()))
				{
					DeleteFreeWorldItem(i);
					i--;
				}
			}
		}
	}

	private void AddVehiclesToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		Vector3 position = a_player.GetPosition();
		for (int i = 0; i < m_vehicles.Length; i++)
		{
			if (!(null != m_vehicles[i]))
			{
				continue;
			}
			Vector3 position2 = m_vehicles[i].transform.position;
			if (Mathf.Abs(position2.x - position.x) < 22f)
			{
				Vector3 position3 = m_vehicles[i].transform.position;
				if (Mathf.Abs(position3.z - position.z) < 19f)
				{
					AddVehicleToMsg(a_msg, i, m_vehicles[i]);
				}
			}
		}
		a_msg.Write(byte.MaxValue);
	}

	private void AddPlayersToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		if (a_player != null && a_player.m_connection != null)
		{
			Vector3 position = a_player.GetPosition();
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i] == null || !m_players[i].IsSpawned() || !(null == m_players[i].GetVehicle()) || a_player.m_pid == m_players[i].m_pid || m_players[i].m_onlineId == -1)
				{
					continue;
				}
				Vector3 position2 = m_players[i].GetPosition();
				if (Mathf.Abs(position2.x - position.x) < 22f)
				{
					Vector3 position3 = m_players[i].GetPosition();
					if (Mathf.Abs(position3.z - position.z) < 19f)
					{
						int onlineId = m_players[i].m_onlineId;
						Vector3 position4 = m_players[i].GetPosition();
						float x = position4.x;
						Vector3 position5 = m_players[i].GetPosition();
						AddPlayerOrNpcToMsg(a_msg, onlineId, x, position5.z, m_players[i].GetRotation(), m_players[i].IsAttacking(), m_players[i].GetHealth());
					}
				}
			}
		}
		a_msg.Write(short.MaxValue);
	}

	private void AddNpcsItemsBuildingsToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		if (a_player == null || a_player.m_connection == null)
		{
			return;
		}
		Vector3 position = a_player.GetPosition();
		for (int i = 0; i < m_npcs.Length; i++)
		{
			if (!(null != m_npcs[i]))
			{
				continue;
			}
			Vector3 position2 = m_npcs[i].transform.position;
			if (Mathf.Abs(position2.x - position.x) < 22f)
			{
				Vector3 position3 = m_npcs[i].transform.position;
				if (Mathf.Abs(position3.z - position.z) < 19f)
				{
					int a_id = i;
					Vector3 position4 = m_npcs[i].transform.position;
					float x = position4.x;
					Vector3 position5 = m_npcs[i].transform.position;
					float z = position5.z;
					Vector3 eulerAngles = m_npcs[i].transform.rotation.eulerAngles;
					AddPlayerOrNpcToMsg(a_msg, a_id, x, z, eulerAngles.y, eBodyBaseState.attacking == m_npcs[i].GetBodyState(), m_npcs[i].GetHealth());
				}
			}
		}
		a_msg.Write(short.MaxValue);
		for (int j = 0; j < m_buildingMan.m_buildings.Count; j++)
		{
			if (!(null != m_buildingMan.m_buildings[j]))
			{
				continue;
			}
			Vector3 position6 = m_buildingMan.m_buildings[j].transform.position;
			if (Mathf.Abs(position6.x - position.x) < 22f)
			{
				Vector3 position7 = m_buildingMan.m_buildings[j].transform.position;
				if (Mathf.Abs(position7.z - position.z) < 19f)
				{
					AddBuildingToMsg(a_msg, m_buildingMan.m_buildings[j], a_player.m_pid == m_buildingMan.m_buildings[j].GetOwnerId());
				}
			}
		}
		a_msg.Write(byte.MaxValue);
		for (int k = 0; k < m_freeWorldItems.Count; k++)
		{
			DatabaseItem databaseItem = m_freeWorldItems[k];
			if (Mathf.Abs(databaseItem.x - position.x) < 22f)
			{
				DatabaseItem databaseItem2 = m_freeWorldItems[k];
				if (Mathf.Abs(databaseItem2.y - position.z) < 19f)
				{
					AddItemToMsg(a_msg, m_freeWorldItems[k]);
				}
			}
		}
		a_msg.Write(byte.MaxValue);
		AddRequestedItemsToMsg(a_msg, a_player);
		a_msg.Write(byte.MaxValue);
	}

	private void AddRequestedItemsToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		bool flag = false;
		if (!a_player.m_updateContainersFlag)
		{
			return;
		}
		for (int i = 0; i < a_player.m_inventory.m_items.Count; i++)
		{
			DatabaseItem databaseItem = a_player.m_inventory.m_items[i];
			if (0 < databaseItem.iid)
			{
				AddItemToMsg(a_msg, a_player.m_inventory.m_items[i]);
				flag = true;
			}
		}
		ItemContainer itemContainer = null;
		if (a_player.m_freeWorldContainer != null)
		{
			itemContainer = a_player.m_freeWorldContainer;
		}
		else if (a_player.m_persistentContainer != null)
		{
			itemContainer = a_player.m_persistentContainer;
		}
		else if (null != a_player.m_shopContainer)
		{
			itemContainer = a_player.m_shopContainer.m_container;
		}
		if (itemContainer != null)
		{
			if (0 < itemContainer.m_items.Count)
			{
				for (int j = 0; j < itemContainer.m_items.Count; j++)
				{
					AddItemToMsg(a_msg, itemContainer.m_items[j]);
				}
			}
			else
			{
				DatabaseItem a_item = new DatabaseItem(0);
				a_item.x += 6f;
				AddItemToMsg(a_msg, a_item);
			}
			flag = true;
		}
		if (!flag)
		{
			DatabaseItem a_item2 = new DatabaseItem(0);
			AddItemToMsg(a_msg, a_item2);
		}
		a_player.m_updateContainersFlag = false;
	}

	private void AddOwnPlayerToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		int num = (a_player.IsAttacking() ? 128 : 0) | ((int)a_player.GetHealth() & 0x7F);
		int num2 = ((null != a_player.GetVehicle()) ? 128 : 0) | ((int)a_player.GetEnergy() & 0x7F);
		a_msg.Write((byte)num);
		a_msg.Write((byte)num2);
		if (null == a_player.GetVehicle())
		{
			Vector3 position = a_player.GetPosition();
			a_msg.Write((short)(position.x * 10.00001f));
			Vector3 position2 = a_player.GetPosition();
			a_msg.Write((short)(position2.z * 10.00001f));
			a_msg.Write((byte)(a_player.GetRotation() / 360f * 255f));
		}
	}

	private void AddPlayerOrNpcToMsg(NetOutgoingMessage a_msg, int a_id, float a_x, float a_z, float a_rotation, bool a_isAttacking, float a_health)
	{
		int num = (a_isAttacking ? 128 : 0) | ((int)a_health & 0x7F);
		a_msg.Write((short)a_id);
		a_msg.Write((short)(a_x * 10.00001f));
		a_msg.Write((short)(a_z * 10.00001f));
		a_msg.Write((byte)(a_rotation / 360f * 255f));
		a_msg.Write((byte)num);
	}

	private void AddItemToMsg(NetOutgoingMessage a_msg, DatabaseItem a_item)
	{
		a_msg.Write((byte)a_item.type);
		a_msg.Write((short)(a_item.x * 10.00001f));
		a_msg.Write((short)(a_item.y * 10.00001f));
		a_msg.Write((byte)a_item.amount);
	}

	private void AddBuildingToMsg(NetOutgoingMessage a_msg, ServerBuilding a_building, bool a_isPlayersBuilding)
	{
		Vector3 eulerAngles = a_building.transform.rotation.eulerAngles;
		float num = eulerAngles.y / 360f;
		int num2 = (((int)(a_building.GetState() * 3f + 0.5f) << 5) & 0x60) | ((int)(num * 31f) & 0x1F);
		if (a_isPlayersBuilding)
		{
			num2 |= 0x80;
		}
		a_msg.Write((byte)a_building.m_type);
		Vector3 position = a_building.transform.position;
		a_msg.Write((short)(position.x * 10.00001f));
		Vector3 position2 = a_building.transform.position;
		a_msg.Write((short)(position2.z * 10.00001f));
		a_msg.Write((byte)num2);
	}

	private void AddVehicleToMsg(NetOutgoingMessage a_msg, int a_id, ServerVehicle a_vehicle)
	{
		int num = (a_vehicle.IsNpcControlled() ? 128 : 0) | ((int)a_vehicle.GetHealth() & 0x7F);
		a_msg.Write((byte)a_id);
		Vector3 position = a_vehicle.transform.position;
		a_msg.Write((short)(position.x * 10.00001f));
		Vector3 position2 = a_vehicle.transform.position;
		a_msg.Write((short)(position2.z * 10.00001f));
		Vector3 eulerAngles = a_vehicle.transform.rotation.eulerAngles;
		a_msg.Write((byte)(eulerAngles.y / 360f * 255f));
		a_msg.Write((byte)num);
		for (int i = 0; i < 4; i++)
		{
			a_msg.Write((byte)(a_vehicle.m_data.passengerIds[i] + 1));
		}
	}

	private void AddPlayerInfoToMsg(NetOutgoingMessage a_msg, ServerPlayer a_player)
	{
		int num;
		if (a_player.m_inventory != null)
		{
			DatabaseItem itemFromPos = a_player.m_inventory.GetItemFromPos(0f, 0f);
			num = itemFromPos.type;
		}
		else
		{
			num = 0;
		}
		int num2 = num;
		int lookIndex = a_player.m_lookIndex;
		int skinIndex = a_player.m_skinIndex;
		int num3;
		if (a_player.m_inventory != null)
		{
			DatabaseItem itemFromPos2 = a_player.m_inventory.GetItemFromPos(0f, 2f);
			num3 = itemFromPos2.type;
		}
		else
		{
			num3 = 0;
		}
		int num4 = num3;
		a_msg.Write((byte)num2);
		a_msg.Write((byte)lookIndex);
		a_msg.Write((byte)skinIndex);
		a_msg.Write((byte)num4);
		a_msg.Write((byte)a_player.GetRank());
		a_msg.Write((byte)a_player.GetKarma());
		a_msg.Write((byte)a_player.m_charType);
	}

	public ServerNpc GetNpc(int a_onlineId)
	{
		if (a_onlineId < 0 || a_onlineId > m_npcs.Length)
		{
			return null;
		}
		return m_npcs[a_onlineId];
	}

	public ServerPlayer GetPlayer(int a_onlineId)
	{
		if (a_onlineId < 0 || a_onlineId > m_players.Length)
		{
			return null;
		}
		return m_players[a_onlineId];
	}

	public ServerPlayer GetPlayer(string a_playerName, int a_onlineId)
	{
		ServerPlayer currentPlayer;
		currentPlayer = GetPlayer(a_onlineId);
		var p_name = currentPlayer.m_name.ToString();
		if (currentPlayer != null)
		{
			currentPlayer.m_name.ToString();
		}
		return null; //wdhwduhwduhw
		
	}

	public void DeletePlayer(int a_onlineId)
	{
		ServerPlayer currentPlayer;
		currentPlayer = GetPlayer(a_onlineId);

		var playerLeave = currentPlayer.m_name.ToString();
		SendNotification(playerLeave + " left the server!");

		ServerPlayer player = null;
		
		if (a_onlineId >= 0 && a_onlineId < m_players.Length)
		{

			m_players[a_onlineId].Remove();
			m_players[a_onlineId] = null;
		}
	}

	public void DeleteFreeWorldItem(Vector3 a_pos, bool a_containerOnly = false)
	{
		int num = 0;
		while (true)
		{
			if (num >= m_freeWorldItems.Count)
			{
				return;
			}
			if ((m_freeWorldItems[num].GetPos() - a_pos).sqrMagnitude < 0.1f)
			{
				if (!a_containerOnly)
				{
					break;
				}
				DatabaseItem databaseItem = m_freeWorldItems[num];
				if (Items.IsContainer(databaseItem.type))
				{
					break;
				}
			}
			num++;
		}
		DeleteFreeWorldItem(num);
	}

	private void DeleteFreeWorldItem(int a_index)
	{
		DatabaseItem databaseItem = m_freeWorldItems[a_index];
		if (Items.IsContainer(databaseItem.type))
		{
			string key = m_freeWorldItems[a_index].GetPos().ToString();
			ItemContainer itemContainer = (ItemContainer)m_freeWorldContainers[key];
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i] != null && itemContainer != null && m_players[i].m_freeWorldContainer == itemContainer)
				{
					m_players[i].ResetContainers();
				}
			}
			m_freeWorldContainers.Remove(key);
		}
		m_freeWorldItems.RemoveAt(a_index);
	}

	private void SendPlayerAndNpcData(ServerPlayer a_player)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.Init);
		netOutgoingMessage.Write((byte)a_player.m_onlineId);
		netOutgoingMessage.Write((byte)(a_player.GetRankProgress() * 255f));
		netOutgoingMessage.Write(a_player.m_gold);
		netOutgoingMessage.Write(m_dayNightCycle);
		netOutgoingMessage.Write(m_dayNightCycleSpeed);
		int num = 0;
		for (int i = 0; i < m_server.ConnectionsCount; i++)
		{
			int a_id = (int)m_server.Connections[i].Tag;
			if (IsValidPlayer(a_id))
			{
				num++;
			}
		}
		netOutgoingMessage.Write((byte)num);
		for (int j = 0; j < m_server.ConnectionsCount; j++)
		{
			int num2 = (int)m_server.Connections[j].Tag;
			if (IsValidPlayer(num2))
			{
				netOutgoingMessage.Write((byte)num2);
				netOutgoingMessage.Write(m_players[num2].m_name);
				netOutgoingMessage.Write(m_players[num2].m_accountId);
				AddPlayerInfoToMsg(netOutgoingMessage, m_players[num2]);
			}
		}
		netOutgoingMessage.Write((short)m_npcs.Length);
		for (int k = 0; k < m_npcs.Length; k++)
		{
			if (null != m_npcs[k])
			{
				netOutgoingMessage.Write((byte)m_npcs[k].GetHandItem());
				netOutgoingMessage.Write((byte)m_npcs[k].GetLookItem());
				netOutgoingMessage.Write((byte)m_npcs[k].GetBodyItem());
				netOutgoingMessage.Write((byte)m_npcs[k].m_npcType);
			}
		}
		if (m_staticBuildings != null && 0 < m_staticBuildings.Length)
		{
			for (int l = 0; l < m_staticBuildings.Length; l++)
			{
				if (null != m_staticBuildings[l] && m_staticBuildings[l].m_type > 0 && m_staticBuildings[l].m_type < 255)
				{
					AddBuildingToMsg(netOutgoingMessage, m_staticBuildings[l], false);
				}
			}
		}
		netOutgoingMessage.Write(byte.MaxValue);
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
	}

	private bool IsValidPlayer(int a_id)
	{
		return -1 < a_id && a_id < m_players.Count() && null != m_players[a_id];
	}



	public void HandleChatCommand(string a_chatText, ServerPlayer a_player, NetIncomingMessage msg)
	{
		string[] array = a_chatText.Split(' ');
		var a_pname = a_player.m_name.ToString();
		var p_skin = a_player.m_skinIndex;
		if ("/suicide" == array[0] || "/kill" == array[0] || "/k" == array[0])
		{
			a_player.ChangeHealthBy(-10000f);
			SendNotification("[p_name] just commited toaster bath.".Replace("[p_name]", a_pname));
			Debug.Log(a_player.m_name + " Commited not alive!");
		}
		else if ("/login" == array[0] && array.Length > 1 && ConfigFile.GetVar("adminpassword", "!@!Not_This_Time!@!") == array[1])
		{
			a_player.m_isAdmin = true;
			SendMessageToPlayerLocal("<b><color='#008000ff'>Admin commands unlocked.</color></b>", a_player, msg);
			Debug.Log(a_player.m_name + " (Steam ID: " + a_player.m_accountId + ") just logged in as admin");
		}
		else if ("/tp-p" == array[0])
		{
			var re_name = GetPlayerByName(array[1].ToString()).m_name.ToString();
			var re_pos = GetPlayerByName(array[1].ToString()).GetPosition();
			if (array[1] == re_name)
			{
				a_player.SetPosition(re_pos);
				SendMessageToPlayerLocal("Teleported to player: <color='#ffa500ff'>"+re_name.ToString()+"</color>.", a_player, msg);
				Debug.Log(a_player.m_name + " Teleported to: " + array[1].ToString() + "!");
			}

		}
		else if (("/skin" == array[0] && array.Length > 1) && (a_player.m_isAdmin = true))
		{
			var skinResponse = "Changed Skin to <b><color='#ffa500ff'>" + array[1].ToString() + "</color></b>.";
			var skin = a_player.m_skinIndex;
			if (array[1] == "0" || array[1] == "Player" || array[1] == "Default")
			{
				a_player.m_skinIndex = 0;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
				Debug.Log(a_player.m_name + " Changed Skin to: " + array[1].ToString() + "!");
			}
			else if (array[1] == "Chad" || array[1] == "chad" || array[1] == "CHAD" || array[1] == "Yakuza" || array[1] == "yakuza" || array[1] == "YAKUZA")
			{
				a_player.m_skinIndex = 1;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
				Debug.Log(a_player.m_name + " Changed Skin to: " + array[1].ToString() + "!");
			}
			else if (array[1] == "Chuck" || array[1] == "Chad".ToUpper() || array[1] == "Chad".ToLower() || array[1] == "LumberJack" || array[1] == "LumberJack".ToUpper() || array[1] == "LumberJack".ToLower())
			{
				a_player.m_skinIndex = 2;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
				Debug.Log(a_player.m_name + " Changed Skin to: " + array[1].ToString() + "!");
			}
			else if (array[1] == "Rollins" || array[1] == "Business" || array[1] == "Rollins".ToUpper() || array[1] == "Business".ToUpper() || array[1] == "Rollins".ToLower() || array[1] == "Business".ToLower())
			{
				a_player.m_skinIndex = 3;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
				Debug.Log(a_player.m_name + " Changed Skin to: " + array[1].ToString() + "!");
			}
			else if (array[1] == "Vince" || array[1] == "Latino" || array[1] == "Vince".ToLower() || array[1] == "Latino".ToLower() || array[1] == "Vince".ToUpper() || array[1] == "Latino".ToUpper())
			{
				a_player.m_skinIndex = 4;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Jenkins" || array[1] == "Elder" || array[1] == "Jenkins".ToLower() || array[1] == "Elder".ToLower() || array[1] == "Jenkins".ToUpper() || array[1] == "Elder".ToUpper())
			{
				a_player.m_skinIndex = 5;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Andrew" || array[1] == "Soldier" || array[1] == "Andrew".ToLower() || array[1] == "Soldier".ToLower() || array[1] == "Andrew".ToUpper() || array[1] == "Soldier".ToUpper())
			{
				a_player.m_skinIndex = 6;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Boxer" || array[1] == "Boxer".ToLower() || array[1] == "Boxer".ToUpper())
			{
				a_player.m_skinIndex = 7;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Pirate" || array[1] == "Viking" || array[1] == "Pirate".ToLower() || array[1] == "Viking".ToLower() || array[1] == "Pirate".ToUpper() || array[1] == "Viking".ToUpper())
			{
				a_player.m_skinIndex = 8;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Thief" || array[1] == "Warrior" || array[1] == "Thief".ToLower() || array[1] == "Warrior".ToLower() || array[1] == "Thief".ToUpper() || array[1] == "Warrior".ToUpper())
			{
				a_player.m_skinIndex = 9;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
			else if (array[1] == "Psyco" || array[1] == "Occult" || array[1] == "Psyco".ToLower() || array[1] == "Occult".ToLower() || array[1] == "Psyco".ToUpper() || array[1] == "Occult".ToUpper())
			{
				a_player.m_skinIndex = 10;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(skinResponse, a_player, msg);
			}
		}
		else if (("/hat" == array[0] && array.Length > 1) && (a_player.m_isAdmin = true))
		{
			var textResponse = "Changed Hat to <b><color='#ffa500ff'>" + array[1].ToString() + "</color></b>.";

			if (array[1] == "0" || array[1] == "ArmyCap" || array[1] == "ArmyCap".ToLower() || array[1] == "ArmyCap".ToUpper())
			{
				a_player.m_lookIndex = 0;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "1" || array[1] == "ArmyCap")
			{
				a_player.m_lookIndex = 1;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "2" || array[1] == "WoolHat")
			{
				a_player.m_lookIndex = 2;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "3" || array[1] == "Pirate")
			{
				a_player.m_lookIndex = 3;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "4" || array[1] == "BunnyEars")
			{
				a_player.m_lookIndex = 4;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "5" || array[1] == "BaseBall")
			{
				a_player.m_lookIndex = 5;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "6" || array[1] == "Santa")
			{
				a_player.m_lookIndex = 6;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "7" || array[1] == "Cowboy")
			{
				a_player.m_lookIndex = 7;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "8" || array[1] == "Ushanka")
			{
				a_player.m_lookIndex = 8;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "9" || array[1] == "TopHat")
			{
				a_player.m_lookIndex = 9;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "10" || array[1] == "Rasta")
			{
				a_player.m_lookIndex = 10;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "11" || array[1] == "Cone" || array[1] == "RoadCone")
			{
				a_player.m_lookIndex = 11;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "12" || array[1] == "Bucket")
			{
				a_player.m_lookIndex = 12;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "13" || array[1] == "Biker")
			{
				a_player.m_lookIndex = 13;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "14" || array[1] == "Horse")
			{
				a_player.m_lookIndex = 14;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "15" || array[1] == "Veteran")
			{
				a_player.m_lookIndex = 15;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "16" || array[1] == "Bandana")
			{
				a_player.m_lookIndex = 16;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "17" || array[1] == "Beanie-Blue")
			{
				a_player.m_lookIndex = 17;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "18" || array[1] == "Irish")
			{
				a_player.m_lookIndex = 18;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "19" || array[1] == "ACap")
			{
				a_player.m_lookIndex = 19;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "20" || array[1] == "Pot" || array[1] == "Pan")
			{
				a_player.m_lookIndex = 20;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "21" || array[1] == "ArmyBoonie")
			{
				a_player.m_lookIndex = 21;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "22" || array[1] == "Glasses" || array[1] == "Sunnies")
			{
				a_player.m_lookIndex = 22;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "23" || array[1] == "Wizard")
			{
				a_player.m_lookIndex = 23;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "24" || array[1] == "Football")
			{
				a_player.m_lookIndex = 24;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "25" || array[1] == "Bandana-Skull")
			{
				a_player.m_lookIndex = 25;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "26" || array[1] == "Party")
			{
				a_player.m_lookIndex = 26;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "27" || array[1] == "Derby")
			{
				a_player.m_lookIndex = 27;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "28" || array[1] == "Miner")
			{
				a_player.m_lookIndex = 28;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "29" || array[1] == "Headphones")
			{
				a_player.m_lookIndex = 29;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "30" || array[1] == "Sherif")
			{
				a_player.m_lookIndex = 30;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "31" || array[1] == "Police")
			{
				a_player.m_lookIndex = 31;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "32" || array[1] == "Green-Beret")
			{
				a_player.m_lookIndex = 32;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "33" || array[1] == "Halo")
			{
				a_player.m_lookIndex = 33;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "34" || array[1] == "Crown")
			{
				a_player.m_lookIndex = 34;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "35" || array[1] == "Skull")
			{
				a_player.m_lookIndex = 35;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "36" || array[1] == "Fedora")
			{
				a_player.m_lookIndex = 36;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "37" || array[1] == "SkiMask")
			{
				a_player.m_lookIndex = 37;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "38" || array[1] == "Russia")
			{
				a_player.m_lookIndex = 38;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "39" || array[1] == "USA")
			{
				a_player.m_lookIndex = 39;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "40" || array[1] == "Brazil")
			{
				a_player.m_lookIndex = 40;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "41" || array[1] == "Germany")
			{
				a_player.m_lookIndex = 41;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "42" || array[1] == "Turkey")
			{
				a_player.m_lookIndex = 42;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "43" || array[1] == "Poland")
			{
				a_player.m_lookIndex = 43;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "44" || array[1] == "Canada")
			{
				a_player.m_lookIndex = 44;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "45" || array[1] == "France")
			{
				a_player.m_lookIndex = 45;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "46" || array[1] == "UK")
			{
				a_player.m_lookIndex = 46;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "47" || array[1] == "Thailand")
			{
				a_player.m_lookIndex = 47;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "48" || array[1] == "Spain")
			{
				a_player.m_lookIndex = 48;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "49" || array[1] == "Australia")
			{
				a_player.m_lookIndex = 49;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
			}
			else if (array[1] == "50" || array[1] == "Italy")
			{
				a_player.m_lookIndex = 50;
				a_player.m_updateInfoFlag = true;
				SendMessageToPlayerLocal(textResponse, a_player, msg);
				
			}
		}
		else if (("/model" == array[0] && array.Length > 1) && (a_player.m_isAdmin = true || a_player.m_gold > 0))
		{
			var mdlTextResponse = "Changed Model to <b><color='#ffa500ff'>" + array[1].ToString() + "</color></b>.";
			eCharType eCharType = eCharType.ePlayer;
			var eMutant = eCharType.eMutant;

			if (array[1] == "Car")
			{
				eCharType = (eCharType)(1);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "GasMask-Guy")
			{
				eCharType = (eCharType)(2);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Zombie")
			{
				eCharType = (eCharType)(3);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Chicken")
			{
				eCharType = (eCharType)(4);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Raven" || array[1] == "Crow")
			{
				eCharType = (eCharType)(5);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Deer")
			{
				eCharType = (eCharType)(6);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Bull")
			{
				eCharType = (eCharType)(7);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Pig")
			{
				eCharType = (eCharType)(8);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Wolf")
			{
				eCharType = (eCharType)(9);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Survivor")
			{
				eCharType = (eCharType)(10);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Survivor-F")
			{
				eCharType = (eCharType)(11);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Player-F")
			{
				eCharType = (eCharType)(12);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Fox")
			{
				eCharType = (eCharType)(13);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Sheep")
			{
				eCharType = (eCharType)(14);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Eagle")
			{
				eCharType = (eCharType)(15);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Cow")
			{
				eCharType = (eCharType)(16);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "SurvivorMutant")
			{
				eCharType = (eCharType)(17);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Spider")
			{
				eCharType = (eCharType)(18);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}
			else if (array[1] == "Poison-Spider")
			{
				eCharType = (eCharType)(19);
				SendMessageToPlayerLocal(mdlTextResponse, a_player, msg);
			}

			if ((a_player.m_isAdmin || eCharType == eCharType.ePlayer || eCharType == eCharType.ePlayerFemale) && a_player.m_charType != eCharType)
			{
				a_player.m_charType = eCharType;
				a_player.m_updateInfoFlag = true;
			}

		}
		else if ("/dropgold" == array[0] && array.Length > 1)
		{
			int itemAmountByType = a_player.m_inventory.GetItemAmountByType(254);
			if (0 < itemAmountByType)
			{
				int num = 0;
				try
				{
					num = int.Parse(array[1]);
				}
				catch (Exception)
				{
					num = 0;
				}
				if (0 < num)
				{
					num = Mathf.Min(itemAmountByType, num);
					a_player.m_inventory.DeclineItemAmountByType(254, num);
					CreateFreeWorldItem(254, num, a_player.GetPosition());
					SendMoneyUpdate(a_player);
				}
			}
		}


		if (!a_player.m_isAdmin)
		{
			return;
		}
		if ("/stats" == array[0])
		{
			NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.ChatLocal);
			netOutgoingMessage.Write("[SERVER]: fps_cur: " + (int)(1f / Time.smoothDeltaTime) + "\n fps_alltime: " + (int)((float)Time.frameCount / Time.time) + " \nbuildings: " + m_buildingMan.m_buildings.Count + " \nitems: " + m_freeWorldItems.Count + " \ntempcontainers: " + m_freeWorldContainers.Count + "POS:" + a_player.GetPosition().ToString());
			a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
		}
		else if ("/airdrop" == array[0])
		{
			Invoke("CreateAirdrop", 3f);
		}
		else if ("/shutdown" == array[0])
		{
			m_serverRestartTime = Time.time + 300f;
		}
		else if ("/shutdownnow" == array[0])
		{
			m_serverRestartTime = Time.time;
		}
		else if ("/message" == array[0])
		{
			SendNotification(a_chatText.Substring(8));
		}
		else if ("/addxp" == array[0] && array.Length > 1)
		{
			int num2 = 0;
			try
			{
				num2 = int.Parse(array[1]);

			}
			catch (Exception)
			{


				num2 = 0;
				if (num2 == 0)
				{
					SendMessageToPlayerLocal(LNG.Get("CMD_RETURN_USAGE_XP"), a_player, msg);
				}

			}
			SendMessageToPlayerLocal("Added " + num2.ToString() + "XP!", a_player, msg);
			a_player.AddXp(num2);

		}
		else if ("/addkarma" == array[0] && array.Length > 1)
		{
			int num3 = 0;
			try
			{
				num3 = int.Parse(array[1]);
			}
			catch (Exception)
			{
				SendMessageToPlayerLocal(LNG.Get("CMD_RETURN_USAGE_KARMA"), a_player, msg);
				num3 = 0;

			}
			SendMessageToPlayerLocal("Added " + num3.ToString() + " Karma Points!", a_player, msg);
			a_player.ChangeKarmaBy(num3);
		}
		else if ("/addhp" == array[0] && array.Length > 1)
		{
			int num4 = 0;
			try
			{
				num4 = int.Parse(array[1]);
			}
			catch (Exception)
			{
				SendMessageToPlayerLocal(LNG.Get("CMD_RETURN_USAGE_HP"), a_player, msg);
				num4 = 0;

			}
			SendMessageToPlayerLocal("Added " + num4.ToString() + "HP!", a_player, msg);
			a_player.ChangeHealthBy(num4);
		}
		else if ("/addenergy" == array[0] && array.Length > 1)
		{
			int num5 = 0;
			try
			{
				num5 = int.Parse(array[1]);
			}
			catch (Exception)
			{
				SendMessageToPlayerLocal(LNG.Get("CMD_RETURN_USAGE_ENERGY"), a_player, msg);
				num5 = 0;

			}
			SendMessageToPlayerLocal("Added " + num5.ToString() + " Energy!", a_player, msg);
			a_player.ChangeEnergyBy(num5);

		}
		else if ("/buff" == array[0] && array.Length > 1) // BROKEN AS FUCK LMAO!! FIX ASAP!!!!
		{

			eCondition eCondition = eCondition.none;
			var eMutant = eCharType.eMutant;
			if (array[1] == "Starve" || array[1] == "Hunger" || array[1] == "Food" || array[1] == "Starve".ToLower() || array[1] == "Hunger".ToLower() || array[1] == "Food".ToLower() || array[1] == "Starve".ToUpper() || array[1] == "Hunger".ToUpper() || array[1] == "Food".ToUpper())
			{
				a_player.SetCondition(eCondition.starvation, true);
			}
			else if (array[1] == "Infected")
			{
				a_player.SetCondition(eCondition.infection, true);
			}
			else if (array[1] == "Bleeding")
			{
				a_player.SetCondition(eCondition.bleeding, true);
			}
			else if (array[1] == "Freezing")
			{
				a_player.SetCondition(eCondition.freezing, true);
			}
			else if (array[1] == "Radiation")
			{
				a_player.SetCondition(eCondition.radiation, true);
			}
			else if (array[1] == "Pain")
			{
				a_player.SetCondition(eCondition.pain, true);
			}
			else if (array[1] == "Clear")
			{
				a_player.SetCondition(eCondition.none, true);
				a_player.SetCondition(eCondition.pain, false);
				a_player.SetCondition(eCondition.radiation, false);
				a_player.SetCondition(eCondition.freezing, false);
				a_player.SetCondition(eCondition.bleeding, false);
				a_player.SetCondition(eCondition.infection, false);
				a_player.ChangeHealthBy(100f);
				a_player.ChangeEnergyBy(100f);
				SendMessageToPlayerLocal("Cleared All effects on your player!", a_player, msg);
			}
		}
		else if ("/spawn" == array[0] && array.Length > 1)
		{
			int num7 = 0;
			int num8 = 1;
			string i_name;
			string i_amount;
			try
			{
				num7 = int.Parse(array[1]);
				num8 = ((array.Length <= 2) ? 1 : int.Parse(array[2]));
			}
			catch (Exception)
			{
				num7 = 0;
				num8 = 1;
			}
			if (num7 != 0)
			{
				ItemDef itemDef = Items.GetItemDef(num7);
				if (itemDef.ident != null)
				{
					i_name = itemDef.ident.ToString();
					CreateFreeWorldItem(num7, num8, a_player.GetPosition());
					i_amount = num8.ToString();
					SendMessageToPlayerLocal("Spawned item: "+i_name+" X:"+i_amount, a_player, msg);
				}
			}
		}
		else if ("/tp" == array[0] && "HomeTown" == array[1])
		{
			var loc_ht = new Vector3(-909, 0, 612);
			a_player.SetPosition(loc_ht);
		}
		else if ("/tp" == array[0] && "Terminus" == array[1])
		{
			var loc = new Vector3(-925, 0, 869);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Garbage" == array[1])
		{
			var loc = new Vector3(-646, 0, 1054);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Castle" == array[1])
		{
			var loc = new Vector3(-267, 0, 178);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Area42" == array[1])
		{
			var loc = new Vector3(-1030, 0, -274);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Fort-B" == array[1])
		{
			var loc = new Vector3(-1043, 0, 14);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "GasTown" == array[1])
		{
			var loc = new Vector3(-1054, 0, 310);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "West-Port" == array[1])
		{
			var loc = new Vector3(1113, 0, 109);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Madison" == array[1])
		{
			var loc = new Vector3(-643, 0, -1033);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Venmore" == array[1])
		{
			var loc = new Vector3(-407, 0, 647);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Valley" == array[1])
		{
			var loc = new Vector3(-1004, 0, -1044);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Alexandria" == array[1])
		{
			var loc = new Vector3(341, 0, 161);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Riverside" == array[1])
		{
			var loc = new Vector3(430, 0, 735);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Tallahassee" == array[1])
		{
			var loc = new Vector3(94, 0, 1014);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Airport" == array[1])
		{
			var loc = new Vector3(-355, 0, 1053);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "PowePlant" == array[1])
		{
			var loc = new Vector3(1192, 0, 1184);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "GasRanch" == array[1])
		{
			var loc = new Vector3(681, 0, 1158);
			a_player.SetPosition(loc);
		}
		else if ("/tp" == array[0] && "Riverside-SZ" == array[1])
		{
			var loc = new Vector3(628, 0, 631);
			a_player.SetPosition(loc);
			
		}
		else if ("/tp" == array[0] && array.Length > 2)
		{
			int num9 = 0;
			int num10 = 0;
			string final_dest = "X:" + num9.ToString() + " Z:" + num10.ToString();
			try
			{
				num9 = int.Parse(array[1]);
				num10 = int.Parse(array[2]);
			}
			catch (Exception)
			{
				if (num9 != 1 - 12000 && num10 != 1 - 12000)
				{
					SendMessageToPlayerLocal("Please use correctly EG: /tp 123 123", a_player, msg);
				}
			}
			if (num9 != 1 - 12000 && num10 != 1 - 12000)
			{
				SendMessageToPlayerLocal("Please use correctly EG: /tp 123 123", a_player, msg);
			}
			if (num9 != 0 && num10 != 0)
			{
				a_player.SetPosition(new Vector3(num9, 0f, num10));
				num9.ToString();
				num10.ToString();
				SendMessageToPlayerLocal(LNG.Get("TO_POS").Replace("[dest]", num9.ToString() + " " + num10.ToString()), a_player, msg);
			}
		}

	}



	private void CreateAirdrop()
	{
		int a_containerType = 121;
		Vector3 a_pos = new Vector3(UnityEngine.Random.Range(-345f, -335f), 0f, UnityEngine.Random.Range(-240f, -230f));
		int num = UnityEngine.Random.Range(60, 66);
		CreateTempContainerItem(num, 1, a_pos, a_containerType);
		ItemDef itemDef = Items.GetItemDef(num);
		int ammoItemType = itemDef.ammoItemType;
		CreateTempContainerItem(ammoItemType, 100, a_pos, a_containerType);
		int a_newItemType = UnityEngine.Random.Range(90, 94);
		CreateTempContainerItem(a_newItemType, 1, a_pos, a_containerType);
		int a_newItemType2 = UnityEngine.Random.Range(210, 220);
		CreateTempContainerItem(a_newItemType2, 1, a_pos, a_containerType);
		CreateTempContainerItem(2, 10, a_pos, a_containerType);
		CreateTempContainerItem(3, 10, a_pos, a_containerType);
		CreateTempContainerItem(130, 200, a_pos, a_containerType);
		CreateTempContainerItem(131, 200, a_pos, a_containerType);
		CreateTempContainerItem(132, 200, a_pos, a_containerType);
		CreateTempContainerItem(133, 200, a_pos, a_containerType);
		SendNotification("An airdrop has spawned, be the first there for GREAT LOOT!!!");
	}

	private bool OpenShopContainer(ServerPlayer a_player)
	{
		if (a_player != null && m_shopContainers != null)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < m_shopContainers.Length; i++)
			{
				zero = m_shopContainers[i].transform.position;
				float x = zero.x;
				Vector3 position = a_player.GetPosition();
				if (Mathf.Abs(x - position.x) < 1.4f)
				{
					float z = zero.z;
					Vector3 position2 = a_player.GetPosition();
					if (Mathf.Abs(z - position2.z) < 1.4f)
					{
						a_player.m_shopContainer = m_shopContainers[i];
						a_player.m_updateContainersFlag = true;
						SendShopInfo(a_player, m_shopContainers[i].m_buyPriceMuliplier, m_shopContainers[i].m_sellPriceMuliplier);
						return true;
					}
				}
			}
		}
		return false;
	}

	private bool TryEnterVehicle(ServerPlayer a_player)
	{
		bool result = false;
		if (a_player != null && a_player.CanEnterExitVehicle() && m_vehicles != null && null == a_player.GetVehicle() && a_player.m_onlineId != -1)
		{
			Vector3 zero = Vector3.zero;
			for (int i = 0; i < m_vehicles.Length; i++)
			{
				zero = m_vehicles[i].transform.position;
				float x = zero.x;
				Vector3 position = a_player.GetPosition();
				if (Mathf.Abs(x - position.x) < 2.5f)
				{
					float z = zero.z;
					Vector3 position2 = a_player.GetPosition();
					if (Mathf.Abs(z - position2.z) < 2.5f && m_vehicles[i].AddPassenger(a_player.m_onlineId))
					{
						result = a_player.SetVehicle(m_vehicles[i]);
						break;
					}
				}
			}
		}
		return result;
	}

	public void Dig(Vector3 a_pos)
	{
		m_sql.RequestHiddenItems(a_pos);
		for (int i = 0; i < m_freeWorldItems.Count; i++)
		{
			DatabaseItem databaseItem = m_freeWorldItems[i];
			if (Items.IsContainer(databaseItem.type))
			{
				continue;
			}
			DatabaseItem databaseItem2 = m_freeWorldItems[i];
			if (Mathf.Abs(databaseItem2.x - a_pos.x) < 0.5f)
			{
				DatabaseItem databaseItem3 = m_freeWorldItems[i];
				if (Mathf.Abs(databaseItem3.y - a_pos.z) < 0.5f)
				{
					DatabaseItem a_item = m_freeWorldItems[i];
					a_item.hidden = true;
					a_item.flag = eDbAction.insert;
					m_sql.SaveItem(a_item);
					DeleteFreeWorldItem(i);
					i--;
				}
			}
		}
	}

	public void CreateFreeWorldItem(int a_newItemType, int a_amount, Vector3 a_pos) // need to mirror this method for entites possible /summon /spawn commands etc.
	{
		if (Items.IsContainer(a_newItemType))
		{
			return;
		}
		bool flag = false;
		if (Items.IsStackable(a_newItemType))
		{
			for (int i = 0; i < m_freeWorldItems.Count; i++)
			{
				DatabaseItem databaseItem = m_freeWorldItems[i];
				if (a_newItemType != databaseItem.type)
				{
					continue;
				}
				DatabaseItem databaseItem2 = m_freeWorldItems[i];
				if (databaseItem2.amount + a_amount > 254)
				{
					continue;
				}
				DatabaseItem databaseItem3 = m_freeWorldItems[i];
				if (Mathf.Abs(databaseItem3.x - a_pos.x) < 0.2f)
				{
					DatabaseItem databaseItem4 = m_freeWorldItems[i];
					if (Mathf.Abs(databaseItem4.y - a_pos.z) < 0.2f)
					{
						DatabaseItem value = m_freeWorldItems[i];
						value.amount += a_amount;
						m_freeWorldItems[i] = value;
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			m_freeWorldItems.Add(new DatabaseItem(a_newItemType, a_pos.x, a_pos.z, a_amount));
		}
	}

	public void CreateFreeWorldItem(int a_newItemType, int a_amount, Vector3 a_pos, int quality)
	{
		if (Items.IsContainer(a_newItemType))
		{
			return;
		}
		bool flag = false;
		if (Items.IsStackable(a_newItemType))
		{
			for (int i = 0; i < m_freeWorldItems.Count; i++)
			{
				DatabaseItem databaseItem = m_freeWorldItems[i];
				if (a_newItemType != databaseItem.type)
				{
					continue;
				}
				DatabaseItem databaseItem2 = m_freeWorldItems[i];
				if (databaseItem2.amount + a_amount > 254)
				{
					continue;
				}
				DatabaseItem databaseItem3 = m_freeWorldItems[i];
				if (Mathf.Abs(databaseItem3.x - a_pos.x) < 0.2f)
				{
					DatabaseItem databaseItem4 = m_freeWorldItems[i];
					if (Mathf.Abs(databaseItem4.y - a_pos.z) < 0.2f)
					{
						DatabaseItem value = m_freeWorldItems[i];
						value.amount += a_amount;
						m_freeWorldItems[i] = value;
						flag = true;
						break;
					}
				}
			}
		}
		if (!flag)
		{
			m_freeWorldItems.Add(new DatabaseItem(a_newItemType, a_pos.x, a_pos.z, quality));
		}
	}

	public void CreateTempContainerItem(int a_newItemType, int a_amount, Vector3 a_pos, int a_containerType = 120)
	{
		a_pos.y = 0f;
		string key = a_pos.ToString();
		ItemContainer itemContainer = (!m_freeWorldContainers.Contains(key)) ? null : ((ItemContainer)m_freeWorldContainers[key]);
		if (itemContainer == null)
		{
			m_freeWorldItems.Add(new DatabaseItem(a_containerType, a_pos.x, a_pos.z));
			itemContainer = new ItemContainer(4, 4, 6);
			itemContainer.m_position = a_pos;
			m_freeWorldContainers.Add(key, itemContainer);
		}
		itemContainer.CollectItem(new DatabaseItem(a_newItemType, 0f, 0f, a_amount), true);
	}

	public DatabaseItem PickupItem(ServerPlayer a_player, BrainBase a_npc)
	{
		DatabaseItem databaseItem = new DatabaseItem(0);
		for (int i = 0; i < m_freeWorldItems.Count; i++)
		{
			if (a_player != null)
			{
				DatabaseItem databaseItem2 = m_freeWorldItems[i];
				float x = databaseItem2.x;
				Vector3 position = a_player.GetPosition();
				if (!(Mathf.Abs(x - position.x) < 1.1f))
				{
					continue;
				}
				DatabaseItem databaseItem3 = m_freeWorldItems[i];
				float y = databaseItem3.y;
				Vector3 position2 = a_player.GetPosition();
				if (!(Mathf.Abs(y - position2.z) < 1.1f))
				{
					continue;
				}
				databaseItem = m_freeWorldItems[i];
				if (Items.IsContainer(databaseItem.type))
				{
					string key = m_freeWorldItems[i].GetPos().ToString();
					if (m_freeWorldContainers.Contains(key))
					{
						a_player.m_freeWorldContainer = (ItemContainer)m_freeWorldContainers[key];
					}
				}
				else if (a_player.m_inventory.CollectItem(databaseItem, true))
				{
					DeleteFreeWorldItem(i);
					if (databaseItem.type == 254)
					{
						SendMoneyUpdate(a_player);
					}
				}
				a_player.m_updateContainersFlag = true;
				if (databaseItem.x < 1f)
				{
					SendPlayerInfo(a_player);
				}
				break;
			}
			if (!(null != a_npc))
			{
				continue;
			}
			float num = 1.6f;
			DatabaseItem databaseItem4 = m_freeWorldItems[i];
			float x2 = databaseItem4.x;
			Vector3 position3 = a_npc.transform.position;
			if (Mathf.Abs(x2 - position3.x) < num)
			{
				DatabaseItem databaseItem5 = m_freeWorldItems[i];
				float y2 = databaseItem5.y;
				Vector3 position4 = a_npc.transform.position;
				if (Mathf.Abs(y2 - position4.z) < num)
				{
					databaseItem = m_freeWorldItems[i];
					DeleteFreeWorldItem(i);
					break;
				}
			}
		}
		return databaseItem;
	}

	public DatabaseItem GetRandomFreeWorldItem()
	{
		if (m_freeWorldItems.Count > 0)
		{
			return m_freeWorldItems[UnityEngine.Random.Range(0, m_freeWorldItems.Count)];
		}
		return new DatabaseItem(0);
	}

	public int GetPlayerCount()
	{
		return m_server.ConnectionsCount;
	}

	public List<DatabaseItem> GetFreeWorldItems()
	{
		return m_freeWorldItems;
	}

	public ServerPlayer GetPlayerByTransform(Transform a_t)
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && a_t == m_players[i].GetTransform())
			{
				return m_players[i];
			}
		}
		return null;
	}

	public int GetFreeSlots()
	{
		int num = 0;
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] == null)
			{
				num++;
			}
		}
		return num;
	}

	public ServerPlayer GetPlayerByPid(int a_pid)
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && a_pid == m_players[i].m_pid)
			{
				return m_players[i];
			}
		}
		return null;
	}

	public ServerPlayer GetPlayerByAid(ulong a_aid)
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && a_aid == m_players[i].m_accountId)
			{
				return m_players[i];
			}
		}
		return null;
	}
	public ServerPlayer GetPlayerByName(string a_name)
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && a_name == m_players[i].m_name)
			{
				return m_players[i];
			}
		}
		return null;
	}

	public ServerPlayer GetPlayerByOnlineid(int a_oid)
	{
		if (-1 < a_oid && a_oid < m_players.Length && m_players[a_oid] != null && m_players[a_oid].IsSpawned())
		{
			return m_players[a_oid];
		}
		return null;
	}

	public ServerPlayer GetNearestPlayer(Vector3 a_pos)
	{
		float num = 9999999f;
		ServerPlayer result = null;
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && !m_players[i].IsDead())
			{
				float sqrMagnitude = (a_pos - m_players[i].GetPosition()).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = m_players[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public DatabaseItem GetNearestItem(Vector3 a_pos, bool a_petFoodOnly = false)
	{
		float num = 9999999f;
		DatabaseItem result = default(DatabaseItem);
		for (int i = 0; i < m_freeWorldItems.Count; i++)
		{
			if (a_petFoodOnly)
			{
				DatabaseItem databaseItem = m_freeWorldItems[i];
				if (!Items.IsEatableForPet(databaseItem.type))
				{
					continue;
				}
			}
			float sqrMagnitude = (a_pos - m_freeWorldItems[i].GetPos()).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				result = m_freeWorldItems[i];
				num = sqrMagnitude;
			}
		}
		return result;
	}

	public int GetNearbyItemCount(Vector3 a_pos)
	{
		int num = 0;
		for (int i = 0; i < m_freeWorldItems.Count; i++)
		{
			float sqrMagnitude = (a_pos - m_freeWorldItems[i].GetPos()).sqrMagnitude;
			if (sqrMagnitude < 1f)
			{
				num++;
			}
		}
		return num;
	}

	public bool PartyContainsPid(int a_partyId, int a_pid)
	{
		if (m_partys.Contains(a_partyId))
		{
			List<DatabasePlayer> list = (List<DatabasePlayer>)m_partys[a_partyId];
			if (list != null && 0 < list.Count)
			{
				for (int i = 0; i < list.Count; i++)
				{
					DatabasePlayer databasePlayer = list[i];
					if (a_pid == databasePlayer.pid)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool PartyContainsAid(int a_partyId, ulong a_aid)
	{
		if (m_partys.Contains(a_partyId))
		{
			List<DatabasePlayer> list = (List<DatabasePlayer>)m_partys[a_partyId];
			if (list != null && 0 < list.Count)
			{
				for (int i = 0; i < list.Count; i++)
				{
					DatabasePlayer databasePlayer = list[i];
					if (a_aid == databasePlayer.aid)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public float GetDayLight()
	{
		float a_pip = 0f;
		return Util.GetLightIntensity(m_dayNightCycle, out a_pip);
	}

	public SQLThreadManager GetSql()
	{
		return m_sql;
	}

	public void BroadcastStaticBuildingChange(ServerBuilding a_building)
	{
		NetOutgoingMessage netOutgoingMessage = m_server.CreateMessage();
		netOutgoingMessage.Write(MessageIds.StaticBuildingUpdate);
		AddBuildingToMsg(netOutgoingMessage, a_building, false);
		m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.ReliableUnordered);
	}

	public static void SendPartyUpdate(ServerPlayer a_player, DatabasePlayer[] a_party)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.PartyUpdate);
		netOutgoingMessage.Write((byte)((a_party != null) ? a_party.Length : 0));
		if (a_party != null)
		{
			for (int i = 0; i < a_party.Length; i++)
			{
				netOutgoingMessage.Write(a_party[i].name);
				netOutgoingMessage.Write(a_party[i].aid);
				netOutgoingMessage.Write((byte)a_party[i].partyRank);
			}
		}
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public static void SendPartyFeedback(ServerPlayer a_player, ePartyFeedback a_type, string a_otherPlayerName)
	{
		if (15 < a_otherPlayerName.Length)
		{
			a_otherPlayerName = a_otherPlayerName.Substring(0, 15) + "...";
		}
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.PartyFeedback);
		netOutgoingMessage.Write((byte)a_type);
		netOutgoingMessage.Write(a_otherPlayerName);
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public static void SendRankUpdate(ServerPlayer a_player, int a_addedXp)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.RankUpdate);
		netOutgoingMessage.Write((byte)(a_player.GetRankProgress() * 255f));
		netOutgoingMessage.Write((short)Mathf.Max(a_addedXp, 0));
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public static void SendConditionUpdate(ServerPlayer a_player)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.ConditionUpdate);
		netOutgoingMessage.Write(a_player.GetConditions());
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public static void SendMoneyUpdate(ServerPlayer a_player)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.MoneyUpdate);
		netOutgoingMessage.Write(a_player.m_gold);
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public void SendShopInfo(ServerPlayer a_player, float a_buyMultip, float a_sellMultip)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.ShopInfo);
		netOutgoingMessage.Write(a_buyMultip);
		netOutgoingMessage.Write(a_sellMultip);
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public void SendSpecialEvent(ServerPlayer a_player, eSpecialEvent a_event)
	{
		if (a_event > eSpecialEvent.none)
		{
			NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.SpecialEvent);
			netOutgoingMessage.Write((byte)a_event);
			a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendMissionPropose(ServerPlayer a_player, Mission a_mission)
	{
		if (a_mission != null)
		{
			NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.MissionPropose);
			netOutgoingMessage.Write((byte)a_mission.m_type);
			netOutgoingMessage.Write((byte)a_mission.m_objPerson);
			netOutgoingMessage.Write((byte)a_mission.m_objObject);
			netOutgoingMessage.Write((byte)a_mission.m_location);
			netOutgoingMessage.Write((short)a_mission.m_xpReward);
			a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendMissionUpdate(ServerPlayer a_player, List<Mission> a_missions)
	{
		NetOutgoingMessage netOutgoingMessage = a_player.m_connection.Peer.CreateMessage();
		netOutgoingMessage.Write(MessageIds.MissionUpdate);
		if (a_missions != null)
		{
			for (int i = 0; i < a_missions.Count; i++)
			{
				if (a_missions[i] != null)
				{
					netOutgoingMessage.Write((byte)a_missions[i].m_type);
					netOutgoingMessage.Write((byte)a_missions[i].m_objPerson);
					netOutgoingMessage.Write((byte)a_missions[i].m_objObject);
					netOutgoingMessage.Write((byte)a_missions[i].m_location);
					netOutgoingMessage.Write((short)a_missions[i].m_xpReward);
					netOutgoingMessage.Write((short)(a_missions[i].m_dieTime - Time.time));
				}
			}
		}
		a_player.m_connection.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
	}

	public void SendNotification(string a_text)
	{
		NetOutgoingMessage netOutgoingMessage = m_server.CreateMessage();
		netOutgoingMessage.Write(MessageIds.Notification);
		netOutgoingMessage.Write(a_text);
		m_server.SendToAll(netOutgoingMessage, NetDeliveryMethod.Unreliable);
	}

	public SpawnPos[] GetSpawnPoints()
	{
		return m_spawnPoints;
	}

	public ServerTutorial GetTutorial()
	{
		return m_tutorial;
	}

	public bool IsInSpecialArea(Vector3 a_pos, eAreaType a_area)
	{
		SpecialArea[] specialAreas = m_specialAreas;
		foreach (SpecialArea specialArea in specialAreas)
		{
			if (!(null != specialArea) || a_area != specialArea.m_type)
			{
				continue;
			}
			Vector3 vector = specialArea.transform.position - a_pos;
			if (specialArea.m_type == eAreaType.noPvp)
			{
				if (Mathf.Abs(vector.x) < specialArea.m_radius && Mathf.Abs(vector.z) < specialArea.m_radius)
				{
					return true;
				}
			}
			else if (vector.sqrMagnitude < specialArea.m_radius * specialArea.m_radius)
			{
				return true;
			}
		}
		return false;
	}

	public bool RepairVehicle(ServerPlayer a_player, Vector3 a_distCheckPos)
	{
		bool result = false;
		float num = 9999999f;
		int num2 = -1;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < m_vehicles.Length; i++)
		{
			if (null != m_vehicles[i] && !m_vehicles[i].IsDead())
			{
				zero = m_vehicles[i].transform.position;
				float sqrMagnitude = (zero - a_distCheckPos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num2 = i;
					num = sqrMagnitude;
				}
			}
		}
		if (10f > num)
		{
			m_vehicles[num2].ChangeHealthBy(5f);
			result = true;
		}
		return result;
	}

	public void DealExplosionDamage(Vector3 a_pos, float a_damage, float a_radius)
	{
		for (int i = 0; i < m_players.Length; i++)
		{
			if (m_players[i] != null && m_players[i].IsSpawned() && !m_players[i].IsDead())
			{
				float sqrMagnitude = (a_pos - m_players[i].GetPosition()).sqrMagnitude;
				if (sqrMagnitude < a_radius * a_radius)
				{
					float a_delta = (0f - a_damage) * (1f - sqrMagnitude / (a_radius * a_radius));
					m_players[i].ChangeHealthBy(a_delta);
				}
			}
		}
		for (int j = 0; j < m_buildingMan.m_buildings.Count; j++)
		{
			if (null != m_buildingMan.m_buildings[j])
			{
				float sqrMagnitude2 = (a_pos - m_buildingMan.m_buildings[j].transform.position).sqrMagnitude;
				if (sqrMagnitude2 < a_radius * a_radius)
				{
					float a_dif = (0f - a_damage) * (1f - sqrMagnitude2 / (a_radius * a_radius));
					m_buildingMan.m_buildings[j].ChangeHealthBy(a_dif);
					m_buildingMan.m_buildings[j].SetAggressor(base.transform);
				}
			}
		}
		for (int k = 0; k < m_npcs.Length; k++)
		{
			if (null != m_npcs[k])
			{
				float sqrMagnitude3 = (a_pos - m_npcs[k].transform.position).sqrMagnitude;
				if (sqrMagnitude3 < a_radius * a_radius)
				{
					float a_delta2 = (0f - a_damage) * (1f - sqrMagnitude3 / (a_radius * a_radius));
					m_npcs[k].ChangeHealthBy(a_delta2);
				}
			}
		}
		if (m_vehicles == null)
		{
			return;
		}
		for (int l = 0; l < m_vehicles.Length; l++)
		{
			float sqrMagnitude4 = (a_pos - m_vehicles[l].transform.position).sqrMagnitude;
			if (sqrMagnitude4 < a_radius * a_radius)
			{
				float a_delta3 = (0f - a_damage) * (1f - sqrMagnitude4 / (a_radius * a_radius));
				m_vehicles[l].ChangeHealthBy(a_delta3);
			}
		}
	}
}
