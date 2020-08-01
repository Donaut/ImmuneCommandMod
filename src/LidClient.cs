using Lidgren.Network;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class LidClient : LidgrenPeer
{
	public GameObject m_remoteCharPrefab;

	public GameObject m_itemPrefab;

	public GameObject m_buildingPrefab;

	[HideInInspector]
	public string m_disconnectMsg = string.Empty;

	[HideInInspector]
	public string m_notificationMsg = string.Empty;

	private NetClient m_client;

	private NetConnection m_serverCon;

	private string m_name = string.Empty;

	private string m_pwhash = string.Empty;

	private ulong m_id;

	private int m_myOnlineId = -1;

	private float m_rankProgress;

	private bool m_isInVehicle;

	private int m_condition;

	private int m_gold;

	private Hashtable m_worldItems = new Hashtable();

	private Hashtable m_buildings = new Hashtable();

	private List<RemoteItem> m_inventoryItems = new List<RemoteItem>();

	private List<Mission> m_missions;

	private ComChatGUI m_chat;

	private InventoryGUI m_inventory;

	private Hud m_hud;

	private MapGUI m_map;

	private PartyGUI m_partyGui;

	private PopupGUI m_popupGui;

	private int m_popupIdInvite = -1;

	private ClientInput m_clientInput;

	private RemoteCharacter[] m_npcs = new RemoteCharacter[1024];

	private RemoteCharacter[] m_players = new RemoteCharacter[50];

	private RemoteCharacter[] m_cars = new RemoteCharacter[15];

	private CharData[] m_npcData = new CharData[1024];

	[HideInInspector]
	public CharData[] m_playerData = new CharData[50];

	private CarData[] m_carData = new CarData[15];

	private SpecialArea[] m_specialAreas;

	private MissionObjective[] m_missionObjs;

	private Vector3[] m_carSeatOffsets = new Vector3[4];

	private float m_nextSecondUpdate;

	private int m_playerCount;

	private void OnEnable()
	{
		if (m_client == null)
		{
			Debug.Log("LidClient::OnEnable " + Time.time);
			Global.isServer = false;
			NetPeerConfiguration netPeerConfiguration = new NetPeerConfiguration("immune");
			netPeerConfiguration.ConnectionTimeout = 10f;
			netPeerConfiguration.PingInterval = 1f;
			m_client = new NetClient(netPeerConfiguration);
			m_client.Start();
			SetPeer(m_client);
			base.Connected += onConnected;
			base.Disconnected += onDisconnected;
			RegisterMessageHandler(MessageIds.Init, onInit);
			RegisterMessageHandler(MessageIds.UpdateA, onUpdatePlayers);
			RegisterMessageHandler(MessageIds.UpdateB, onUpdateNpcsItemsBuildings);
			RegisterMessageHandler(MessageIds.SetPlayerName, onSetPlayerName);
			RegisterMessageHandler(MessageIds.SetPlayerInfo, onSetPlayerInfo);
			RegisterMessageHandler(MessageIds.Chat, onChat);
			RegisterMessageHandler(MessageIds.ChatLocal, onChatLocal);
			RegisterMessageHandler(MessageIds.RankUpdate, onRankUpdate);
			RegisterMessageHandler(MessageIds.SpecialEvent, onSpecialEvent);
			RegisterMessageHandler(MessageIds.StaticBuildingUpdate, onStaticBuildingUpdate);
			RegisterMessageHandler(MessageIds.RemoveClient, onRemoveClient);
			RegisterMessageHandler(MessageIds.Notification, onNotification);
			RegisterMessageHandler(MessageIds.ConditionUpdate, onConditionUpdate);
			RegisterMessageHandler(MessageIds.ShopInfo, onShopInfo);
			RegisterMessageHandler(MessageIds.MissionPropose, onMissionPropose);
			RegisterMessageHandler(MessageIds.MissionUpdate, onMissionUpdate);
			RegisterMessageHandler(MessageIds.MoneyUpdate, onMoneyUpdate);
			RegisterMessageHandler(MessageIds.PartyUpdate, onPartyUpdate);
			RegisterMessageHandler(MessageIds.PartyFeedback, onPartyFeedback);
			InitCars();
		}
	}

	private void InitStaticBuildings()
	{
		ServerBuilding[] array = (ServerBuilding[])Object.FindObjectsOfType(typeof(ServerBuilding));
		for (int i = 0; i < array.Length; i++)
		{
			if (0 < array[i].m_type)
			{
				RemoteBuilding remoteBuilding = array[i].gameObject.AddComponent<RemoteBuilding>();
				remoteBuilding.Init(array[i].transform.position, array[i].m_type, false, true);
			}
			Object.Destroy(array[i]);
		}
		RemoteBuilding[] array2 = (RemoteBuilding[])Object.FindObjectsOfType(typeof(RemoteBuilding));
		string empty = string.Empty;
		for (int j = 0; j < array2.Length; j++)
		{
			empty = array2[j].m_type + array2[j].transform.position.ToString();
			m_buildings[empty] = array2[j];
		}
	}

	private void InitCars()
	{
		for (int i = 0; i < 15; i++)
		{
			m_carData[i].passengerIds = new int[4];
			for (int j = 0; j < 4; j++)
			{
				m_carData[i].passengerIds[j] = -1;
			}
		}
		m_carSeatOffsets[0] = new Vector3(-0.5f, 0.8f, 0.37f);
		m_carSeatOffsets[1] = new Vector3(0.5f, 0.8f, 0.37f);
		m_carSeatOffsets[2] = new Vector3(-0.45f, 1.15f, -0.82f);
		m_carSeatOffsets[3] = new Vector3(0.45f, 1.15f, -0.82f);
	}

	private void Update()
	{
		if (Time.time > m_nextSecondUpdate)
		{
			m_playerCount = 0;
			for (int i = 0; i < m_playerData.Length; i++)
			{
				if (m_playerData[i].name != null && 1 < m_playerData[i].name.Length)
				{
					m_playerCount++;
				}
			}
			m_nextSecondUpdate = Time.time + 1f;
		}
		if (m_missions != null)
		{
			for (int j = 0; j < m_missions.Count; j++)
			{
				if (m_missions[j] != null && m_missions[j].m_dieTime > 0f)
				{
					m_missions[j].m_dieTime -= Time.deltaTime;
				}
			}
		}
		HandlePopup();
	}

	private void HandlePopup()
	{
		if (null != m_popupGui && m_popupIdInvite == m_popupGui.GetSessionId() && m_popupGui.m_saidYesFlag)
		{
			SendSpecialRequest(eSpecialRequest.acceptPartyInvite);
			m_popupGui.m_saidYesFlag = false;
		}
	}

	private void OnApplicationQuit()
	{
		if (m_client != null)
		{
			m_client.Shutdown("Client has closed the application.");
		}
	}

	private void OnDestroy()
	{
		if (m_client != null)
		{
			m_client.Disconnect("Client has destroyed the one and only.");
		}
	}

	public float GetRoundTripTime()
	{
		return (m_serverCon == null) ? 0f : m_serverCon.AverageRoundtripTime;
	}

	public int GetPlayerCount()
	{
		return m_playerCount;
	}

	public ulong GetSteamId()
	{
		return m_id;
	}

	public string GetPlayerName()
	{
		return m_name;
	}

	public bool IsTutorialActive()
	{
		Vector3 pos = GetPos();
		return pos.x > -1177f && pos.x < -1027f && pos.z > 678f && pos.z < 760f;
	}

	public bool Connect(string a_name, string a_pwhash, ulong a_id, string a_ip)
	{
		m_name = a_name;
		m_pwhash = a_pwhash;
		m_id = a_id;
		IPAddress address = null;
		if (m_client != null && IPAddress.TryParse(a_ip, out address))
		{
			m_client.Connect(a_ip, 8844);
			return true;
		}
		return false;
	}

	public void SendInput(int a_input, int a_targetIdOrAtkRot, float a_buildRot, Vector3 a_dragPos, Vector3 a_dropPos)
	{
		if (m_serverCon != null)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.Input);
			netOutgoingMessage.Write(a_input);
			netOutgoingMessage.Write((short)a_targetIdOrAtkRot);
			netOutgoingMessage.Write((byte)(a_buildRot / 360f * 255f));
			if (a_dragPos != a_dropPos)
			{
				netOutgoingMessage.Write((byte)a_dragPos.x);
				netOutgoingMessage.Write((byte)a_dragPos.z);
				netOutgoingMessage.Write((byte)a_dropPos.x);
				netOutgoingMessage.Write((byte)a_dropPos.z);
			}
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendChatMsg(string a_chatmsg, bool a_local)
	{
		if (m_serverCon != null)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write((!a_local) ? MessageIds.Chat : MessageIds.ChatLocal);
			netOutgoingMessage.Write(a_chatmsg);
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendPartyRequest(ePartyControl a_eType, ulong a_id)
	{
		if (m_serverCon != null)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.PartyControl);
			netOutgoingMessage.Write((byte)a_eType);
			netOutgoingMessage.Write(a_id);
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendCraftRequest(int a_itemType, int a_amount)
	{
		if (m_serverCon != null)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.Craft);
			netOutgoingMessage.Write((byte)a_itemType);
			netOutgoingMessage.Write((byte)a_amount);
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendSpecialRequest(eSpecialRequest a_request)
	{
		if (m_serverCon != null && a_request > eSpecialRequest.none)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.SpecialRequest);
			netOutgoingMessage.Write((byte)a_request);
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public void SendSetLook(int a_lookIndex, string a_lookHash, int a_skinIndex, string a_skinHash)
	{
		if (m_serverCon != null)
		{
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.SetLook);
			netOutgoingMessage.Write((byte)a_lookIndex);
			netOutgoingMessage.Write(a_lookHash);
			netOutgoingMessage.Write((byte)a_skinIndex);
			netOutgoingMessage.Write(a_skinHash);
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.Unreliable, 0);
		}
	}

	public NetConnectionStatistics GetStats()
	{
		if (m_client != null && m_client.ServerConnection != null)
		{
			return m_client.ServerConnection.Statistics;
		}
		return null;
	}

	public Vector3 GetPos()
	{
		return (!IsSpawned()) ? Vector3.zero : m_players[m_myOnlineId].transform.position;
	}

	public RemoteCharacter GetPlayer()
	{
		return (!IsSpawned()) ? null : m_players[m_myOnlineId];
	}

	public float GetHealth()
	{
		return (!IsSpawned()) ? 100f : m_players[m_myOnlineId].m_health;
	}

	public float GetEnergy()
	{
		return (!IsSpawned()) ? 100f : m_players[m_myOnlineId].m_energy;
	}

	public int GetHandItem()
	{
		return IsSpawned() ? m_playerData[m_myOnlineId].handItem : 0;
	}

	public float GetKarma()
	{
		return (!IsSpawned()) ? 100f : ((float)m_playerData[m_myOnlineId].karma);
	}

	public float GetRankProgress()
	{
		return m_rankProgress;
	}

	public int GetCondition()
	{
		return m_condition;
	}

	public ulong GetSteamId(int a_onlineId)
	{
		if (a_onlineId > -1 && m_playerData.Length > a_onlineId)
		{
			return m_playerData[a_onlineId].aid;
		}
		return 0uL;
	}

	private void onConnected(NetIncomingMessage a_msg)
	{
		Debug.Log("Connected to server ... loading level ...");
		Application.LoadLevel(1);
		m_serverCon = a_msg.SenderConnection;
	}

	private void onDisconnected(NetIncomingMessage a_msg)
	{
		a_msg.ReadByte();
		m_disconnectMsg = "Disconnected: " + a_msg.ReadString();
		m_serverCon = null;
		if (Application.loadedLevel != 0)
		{
			Application.LoadLevel(0);
		}
	}

	private void OnLevelWasLoaded(int a_index)
	{
		if (a_index == 1)
		{
			SendAuth();
		}
	}

	private void SendAuth()
	{
		if (m_client != null && m_client.ConnectionStatus == NetConnectionStatus.Connected && m_serverCon != null)
		{
			m_chat = (ComChatGUI)Object.FindObjectOfType(typeof(ComChatGUI));
			m_inventory = (InventoryGUI)Object.FindObjectOfType(typeof(InventoryGUI));
			m_hud = (Hud)Object.FindObjectOfType(typeof(Hud));
			m_map = (MapGUI)Object.FindObjectOfType(typeof(MapGUI));
			m_partyGui = (PartyGUI)Object.FindObjectOfType(typeof(PartyGUI));
			m_popupGui = (PopupGUI)Object.FindObjectOfType(typeof(PopupGUI));
			m_clientInput = (ClientInput)Object.FindObjectOfType(typeof(ClientInput));
			m_missionObjs = (MissionObjective[])Object.FindObjectsOfType(typeof(MissionObjective));
			m_specialAreas = (SpecialArea[])Object.FindObjectsOfType(typeof(SpecialArea));
			Debug.Log("Connected to server ... loading level complete ... send AUTH at " + Time.time);
			NetOutgoingMessage netOutgoingMessage = m_serverCon.Peer.CreateMessage();
			netOutgoingMessage.Write(MessageIds.Auth);
			netOutgoingMessage.Write(m_name);
			netOutgoingMessage.Write(m_pwhash);
			netOutgoingMessage.Write(m_id);
			netOutgoingMessage.Write("1.0.1");//Change for mod forc
			netOutgoingMessage.Write((byte)PlayerPrefs.GetInt("prefAppearance", 0));
			m_serverCon.SendMessage(netOutgoingMessage, NetDeliveryMethod.ReliableOrdered, 1);
		}
	}

	private void onInit(NetIncomingMessage msg)
	{
		m_myOnlineId = msg.ReadByte();
		m_rankProgress = (float)(int)msg.ReadByte() / 255f;
		m_gold = msg.ReadInt32();
		float a_progress = msg.ReadFloat();
		float a_speed = msg.ReadFloat();
		DayNightCycle dayNightCycle = (DayNightCycle)Object.FindObjectOfType(typeof(DayNightCycle));
		if (null != dayNightCycle)
		{
			dayNightCycle.Init(a_progress, a_speed);
		}
		InitStaticBuildings();
		int num = msg.ReadByte();
		Debug.Log("Init: other players: " + (num - 1));
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 = msg.ReadByte();
			if (num2 < 0 && num2 > m_playerData.Length - 1)
			{
				num2 = m_playerData.Length - 1;
			}
			m_playerData[num2].name = msg.ReadString();
			m_playerData[num2].aid = msg.ReadUInt64();
			m_playerData[num2].handItem = msg.ReadByte();
			m_playerData[num2].look = msg.ReadByte();
			m_playerData[num2].skin = msg.ReadByte();
			m_playerData[num2].body = msg.ReadByte();
			m_playerData[num2].rank = msg.ReadByte();
			m_playerData[num2].karma = msg.ReadByte();
			m_playerData[num2].type = (eCharType)msg.ReadByte();
		}
		int num3 = msg.ReadInt16();
		Debug.Log("Init: npcs: " + num3);
		for (int j = 0; j < num3; j++)
		{
			num2 = j;
			if (num2 < 0 && num2 > m_npcData.Length - 1)
			{
				num2 = m_npcData.Length - 1;
			}
			m_npcData[num2].handItem = msg.ReadByte();
			m_npcData[num2].look = msg.ReadByte();
			m_npcData[num2].body = msg.ReadByte();
			m_npcData[num2].type = (eCharType)msg.ReadByte();
		}
		int num4 = 0;
		while (msg.PositionInBytes < msg.LengthBytes && GetAndUpdateBuilding(msg))
		{
			num4++;
		}
		Debug.Log("Init: static buildings: " + num4);
		DebugLogReadWriteMismatch(msg, "onInit");
	}

	private void onUpdatePlayers(NetIncomingMessage a_msg)
	{
		if (-1 < m_myOnlineId)
		{
			GetAndUpdateOwnPlayer(a_msg);
			GetAndUpdateCars(a_msg);
			while (a_msg.PositionInBytes < a_msg.LengthBytes && GetAndUpdatePlayerOrNpc(a_msg, true))
			{
			}
			DebugLogReadWriteMismatch(a_msg, "onUpdatePlayers");
		}
	}

	private void onUpdateNpcsItemsBuildings(NetIncomingMessage a_msg)
	{
		if (-1 >= m_myOnlineId)
		{
			return;
		}
		GetAndUpdateOwnPlayer(a_msg);
		GetAndUpdateCars(a_msg);
		while (a_msg.PositionInBytes < a_msg.LengthBytes && GetAndUpdatePlayerOrNpc(a_msg, false))
		{
		}
		while (a_msg.PositionInBytes < a_msg.LengthBytes && GetAndUpdateBuilding(a_msg))
		{
		}
		bool flag = false;
		bool flag2 = false;
		while (a_msg.PositionInBytes < a_msg.LengthBytes)
		{
			if (!GetAndUpdateItem(a_msg, flag))
			{
				if (flag)
				{
					break;
				}
				flag = true;
			}
			else if (flag && !flag2)
			{
				flag2 = true;
			}
		}
		if (flag2 && null != m_inventory)
		{
			m_inventory.UpdateInventory(m_inventoryItems.ToArray());
			m_inventoryItems.Clear();
		}
		DebugLogReadWriteMismatch(a_msg, "onUpdateNpcsItemsBuildings");
	}

	private void onSetPlayerName(NetIncomingMessage msg)
	{
		int num = msg.ReadByte();
		m_playerData[num].name = msg.ReadString();
		m_playerData[num].aid = msg.ReadUInt64();
		DebugLogReadWriteMismatch(msg, "onSetPlayerName");
	}

	private void onSetPlayerInfo(NetIncomingMessage msg)
	{
		int num = msg.ReadByte();
		m_playerData[num].handItem = msg.ReadByte();
		m_playerData[num].look = msg.ReadByte();
		m_playerData[num].skin = msg.ReadByte();
		m_playerData[num].body = msg.ReadByte();
		m_playerData[num].rank = msg.ReadByte();
		m_playerData[num].karma = msg.ReadByte();
		m_playerData[num].type = (eCharType)msg.ReadByte();
		if (null != m_players[num])
		{
			m_players[num].SetInfo(m_playerData[num]);
		}
		DebugLogReadWriteMismatch(msg, "onSetPlayerInfo");
	}

	private void onChat(NetIncomingMessage msg)
	{
		string a_str = msg.ReadString();
		if (null != m_chat)
		{
			m_chat.AddString(a_str);
		}
		DebugLogReadWriteMismatch(msg, "onChat");
	}

	private void onChatLocal(NetIncomingMessage msg)
	{
		int num = msg.ReadByte();
		string chatText = msg.ReadString();
		if (null != m_players[num])
		{
			m_players[num].SetChatText(chatText);
		}
		DebugLogReadWriteMismatch(msg, "onChatLocal");
	}

	private void onNotification(NetIncomingMessage msg)
	{
		m_notificationMsg = msg.ReadString();
	}

	private void onRankUpdate(NetIncomingMessage a_msg)
	{
		float rankProgress = (float)(int)a_msg.ReadByte() / 255f;
		int a_xp = a_msg.ReadInt16();
		if (IsSpawned())
		{
			m_rankProgress = rankProgress;
			m_players[m_myOnlineId].AddXp(a_xp);
		}
		DebugLogReadWriteMismatch(a_msg, "onRankUpdate");
	}

	private void onConditionUpdate(NetIncomingMessage a_msg)
	{
		m_condition = a_msg.ReadInt32();
		DebugLogReadWriteMismatch(a_msg, "onConditionUpdate");
	}

	private void onMoneyUpdate(NetIncomingMessage a_msg)
	{
		m_gold = a_msg.ReadInt32();
		DebugLogReadWriteMismatch(a_msg, "onMoneyUpdate");
	}

	private void onPartyUpdate(NetIncomingMessage a_msg)
	{
		int num = a_msg.ReadByte();
		DatabasePlayer[] array = null;
		if (0 < num)
		{
			array = new DatabasePlayer[num];
			for (int i = 0; i < num; i++)
			{
				array[i].name = a_msg.ReadString();
				array[i].aid = a_msg.ReadUInt64();
				array[i].partyRank = a_msg.ReadByte();
			}
		}
		m_partyGui.SetParty(array);
		DebugLogReadWriteMismatch(a_msg, "onPartyUpdate");
	}

	private void onPartyFeedback(NetIncomingMessage a_msg)
	{
		ePartyFeedback ePartyFeedback = (ePartyFeedback)a_msg.ReadByte();
		string str = a_msg.ReadString();
		switch (ePartyFeedback)
		{
		case ePartyFeedback.invite:
			m_popupIdInvite = m_popupGui.ShowGui(true, str + LNG.Get("PARTY_POPUP_INVITED"));
			break;
		case ePartyFeedback.errorAlreadyInParty:
			m_popupGui.ShowGui(true, str + LNG.Get("PARTY_POPUP_ALREADY_IN_PARTY"));
			break;
		case ePartyFeedback.kicked:
			m_popupGui.ShowGui(true, str + LNG.Get("PARTY_POPUP_KICKED"));
			break;
		case ePartyFeedback.prodemoted:
			m_popupGui.ShowGui(true, str + LNG.Get("PARTY_POPUP_PRODEMOTED"));
			break;
		case ePartyFeedback.partyFull:
			m_popupGui.ShowGui(true, LNG.Get("PARTY_POPUP_FULL"));
			break;
		}
		DebugLogReadWriteMismatch(a_msg, "onPartyFeedback");
	}

	private void onShopInfo(NetIncomingMessage a_msg)
	{
		float a_buy = a_msg.ReadFloat();
		float a_sell = a_msg.ReadFloat();
		m_inventory.SetShop(a_buy, a_sell);
		DebugLogReadWriteMismatch(a_msg, "onShopInfo");
	}

	private void onMissionPropose(NetIncomingMessage a_msg)
	{
		Mission mission = new Mission();
		mission.m_type = (eMissiontype)a_msg.ReadByte();
		mission.m_objPerson = (eObjectivesPerson)a_msg.ReadByte();
		mission.m_objObject = (eObjectivesObject)a_msg.ReadByte();
		mission.m_location = (eLocation)a_msg.ReadByte();
		mission.m_xpReward = a_msg.ReadInt16();
		m_clientInput.ShowMissionPopup(mission);
		DebugLogReadWriteMismatch(a_msg, "onMissionPropose");
	}

	private void onMissionUpdate(NetIncomingMessage a_msg)
	{
		if (m_missions != null)
		{
			m_missions.Clear();
		}
		m_missions = new List<Mission>();
		while (0 < a_msg.LengthBytes - a_msg.PositionInBytes)
		{
			Mission mission = new Mission();
			mission.m_type = (eMissiontype)a_msg.ReadByte();
			mission.m_objPerson = (eObjectivesPerson)a_msg.ReadByte();
			mission.m_objObject = (eObjectivesObject)a_msg.ReadByte();
			mission.m_location = (eLocation)a_msg.ReadByte();
			mission.m_xpReward = a_msg.ReadInt16();
			mission.m_dieTime = a_msg.ReadInt16();
			m_missions.Add(mission);
		}
		m_hud.UpdateMissions(m_missions);
		m_map.UpdateMissions(m_missions);
		UpdateMissionObjects();
		DebugLogReadWriteMismatch(a_msg, "onMissionUpdate");
	}

	private void UpdateMissionObjects()
	{
		for (int i = 0; i < m_missionObjs.Length; i++)
		{
			if (!(null != m_missionObjs[i]))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < m_missions.Count; j++)
			{
				if (m_missionObjs[i].IsMission(m_missions[j]))
				{
					m_missionObjs[i].m_objPerson = m_missions[j].m_objPerson;
					m_missionObjs[i].SetOnOff(true);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				m_missionObjs[i].SetOnOff(false);
			}
		}
	}

	private void onSpecialEvent(NetIncomingMessage a_msg)
	{
		eSpecialEvent a_event = (eSpecialEvent)a_msg.ReadByte();
		if (IsSpawned())
		{
			m_players[m_myOnlineId].OnSpecialEvent(a_event);
		}
		DebugLogReadWriteMismatch(a_msg, "onSpecialEvent");
	}

	private void onStaticBuildingUpdate(NetIncomingMessage msg)
	{
		GetAndUpdateBuilding(msg);
		DebugLogReadWriteMismatch(msg, "onStaticBuildingUpdate");
	}

	private void onRemoveClient(NetIncomingMessage msg)
	{
		int num = msg.ReadByte();
		if (null != m_players[num])
		{
			m_players[num].Remove();
			m_players[num] = null;
		}
		m_playerData[num] = new CharData(eCharType.ePlayer);
		DebugLogReadWriteMismatch(msg, "onRemoveClient");
	}

	private void DebugLogReadWriteMismatch(NetIncomingMessage a_msg, string a_identifier)
	{
		int num = a_msg.LengthBytes - a_msg.PositionInBytes;
		if (num > 0)
		{
			Debug.Log("WRITE READ MISMATCH: " + a_identifier + " bytes left: " + num + " " + Time.time);
		}
	}

	private void GetAndUpdateOwnPlayer(NetIncomingMessage a_msg)
	{
		byte b = a_msg.ReadByte();
		CharAnim2.ePose a_anim = ((b & 0x80) == 128) ? CharAnim2.ePose.eAttack : CharAnim2.ePose.eStand;
		float a_health = (int)(b = (byte)(b & 0x7F));
		byte b2 = a_msg.ReadByte();
		m_isInVehicle = (128 == (b2 & 0x80));
		float a_energy = (int)(b2 = (byte)(b2 & 0x7F));
		if (!m_isInVehicle)
		{
			Vector3 zero = Vector3.zero;
			zero.x = (float)a_msg.ReadInt16() * 0.1f;
			zero.z = (float)a_msg.ReadInt16() * 0.1f;
			float a_rot = (float)(int)a_msg.ReadByte() / 255f * 360f;
			UpdateOrSpawnCharacter(m_myOnlineId, zero, a_rot, eCharType.ePlayer, a_anim, a_health, a_energy);
		}
		else if (IsSpawned())
		{
			m_players[m_myOnlineId].RefreshStatus(a_health, a_energy);
		}
	}

	private void GetAndUpdateCars(NetIncomingMessage a_msg)
	{
		while (a_msg.PositionInBytes < a_msg.LengthBytes && GetAndUpdateCar(a_msg))
		{
		}
	}

	private bool GetAndUpdatePlayerOrNpc(NetIncomingMessage a_msg, bool a_isPlayer)
	{
		Vector3 zero = Vector3.zero;
		int num = a_msg.ReadInt16();
		if (num == 32767)
		{
			return false;
		}
		zero.x = (float)a_msg.ReadInt16() * 0.1f;
		zero.z = (float)a_msg.ReadInt16() * 0.1f;
		float a_rot = (float)(int)a_msg.ReadByte() / 255f * 360f;
		byte b = a_msg.ReadByte();
		int a_anim = ((b & 0x80) == 128) ? 1 : 0;
		float a_health = (int)(b = (byte)(b & 0x7F));
		UpdateOrSpawnCharacter(num, zero, a_rot, (!a_isPlayer) ? m_npcData[num].type : eCharType.ePlayer, (CharAnim2.ePose)a_anim, a_health);
		return true;
	}

	private bool GetAndUpdateCar(NetIncomingMessage a_msg)
	{
		bool flag = false;
		Vector3 zero = Vector3.zero;
		int num = a_msg.ReadByte();
		if (num == 255)
		{
			return false;
		}
		zero.x = (float)a_msg.ReadInt16() * 0.1f;
		zero.z = (float)a_msg.ReadInt16() * 0.1f;
		float num2 = (float)(int)a_msg.ReadByte() / 255f * 360f;
		byte b = a_msg.ReadByte();
		bool flag2 = 128 == (b & 0x80);
		float a_health = (int)(b = (byte)(b & 0x7F));
		for (int i = 0; i < 4; i++)
		{
			m_carData[num].passengerIds[i] = a_msg.ReadByte() - 1;
			flag |= (m_myOnlineId == m_carData[num].passengerIds[i]);
			if (-1 < m_carData[num].passengerIds[i])
			{
				UpdateOrSpawnCharacter(m_carData[num].passengerIds[i], zero + Quaternion.Euler(0f, num2, 0f) * m_carSeatOffsets[i], num2, flag2 ? m_npcData[m_carData[num].passengerIds[i]].type : eCharType.ePlayer, CharAnim2.ePose.eSit, 9999999f, 9999999f);
			}
		}
		UpdateOrSpawnCharacter(num, zero, num2, eCharType.eCar, CharAnim2.ePose.eStand, a_health);
		return true;
	}

	private bool GetAndUpdateItem(NetIncomingMessage a_msg, bool a_isInventory)
	{
		Vector3 zero = Vector3.zero;
		int num = a_msg.ReadByte();
		if (num == 255)
		{
			return false;
		}
		zero.x = (float)a_msg.ReadInt16() * 0.1f;
		zero.z = (float)a_msg.ReadInt16() * 0.1f;
		int a_amount = a_msg.ReadByte();
		UpdateOrSpawnItem(num, zero, a_amount, a_isInventory);
		return true;
	}

	private bool GetAndUpdateBuilding(NetIncomingMessage a_msg)
	{
		Vector3 zero = Vector3.zero;
		int num = a_msg.ReadByte();
		if (num == 255)
		{
			return false;
		}
		zero.x = (float)a_msg.ReadInt16() * 0.1f;
		zero.z = (float)a_msg.ReadInt16() * 0.1f;
		int num2 = a_msg.ReadByte();
		bool a_isMine = 128 == (num2 & 0x80);
		float a_health = Mathf.Clamp((float)((num2 >> 5) & 3) / 3f * 100f + 1f, 0f, 100f);
		float a_rot = (float)(num2 & 0x1F) / 31f * 360f;
		UpdateOrSpawnBuilding(num, zero, a_rot, a_health, a_isMine);
		return true;
	}

	private void UpdateOrSpawnCharacter(int a_onlineId, Vector3 a_pos, float a_rot, eCharType a_type, CharAnim2.ePose a_anim, float a_health, float a_energy = 100f)
	{
		RemoteCharacter[] array = null;
		switch (a_type)
		{
		case eCharType.ePlayer:
		case eCharType.ePlayerFemale:
			array = m_players;
			break;
		case eCharType.eCar:
			array = m_cars;
			break;
		default:
			array = m_npcs;
			break;
		}
		if (a_onlineId <= -1 || a_onlineId >= array.Length)
		{
			return;
		}
		bool flag = array != null && null == array[a_onlineId];
		if (flag && !(0f < a_health))
		{
			return;
		}
		CharData[] array2 = null;
		switch (a_type)
		{
		case eCharType.ePlayer:
		case eCharType.ePlayerFemale:
			array2 = m_playerData;
			break;
		default:
			array2 = m_npcData;
			break;
		case eCharType.eCar:
			break;
		}
		if (flag)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_remoteCharPrefab);
			array[a_onlineId] = gameObject.GetComponent<RemoteCharacter>();
			array[a_onlineId].Spawn(a_onlineId, a_type, (a_type == eCharType.ePlayer || a_type == eCharType.ePlayerFemale) && m_myOnlineId == a_onlineId);
			if (array2 != null)
			{
				array[a_onlineId].SetInfo(array2[a_onlineId]);
			}
			switch (a_type)
			{
			case eCharType.ePlayer:
			case eCharType.ePlayerFemale:
				gameObject.name = "player_" + array2[a_onlineId].name + "_" + a_onlineId;
				if (m_myOnlineId == a_onlineId)
				{
					BirdCam birdCam = (BirdCam)Object.FindObjectOfType(typeof(BirdCam));
					birdCam.m_target = gameObject.transform;
					AudioListener audioListener = (AudioListener)Object.FindObjectOfType(typeof(AudioListener));
					if (null != audioListener)
					{
						Object.Destroy(audioListener);
					}
					gameObject.AddComponent<AudioListener>();
				}
				break;
			default:
				gameObject.name = "npc_" + a_onlineId;
				break;
			case eCharType.eCar:
				gameObject.name = "car_" + a_onlineId;
				break;
			}
		}
		array[a_onlineId].Refresh(a_pos, a_rot, a_anim, a_health, a_energy);
	}

	private void UpdateOrSpawnItem(int a_type, Vector3 a_pos, int a_amount, bool a_isInventory)
	{
		string text = a_type + a_pos.ToString();
		RemoteItem remoteItem = (!m_worldItems.Contains(text)) ? null : ((RemoteItem)m_worldItems[text]);
		if (a_isInventory || null == remoteItem)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_itemPrefab);
			gameObject.name = "item_" + text;
			remoteItem = gameObject.GetComponent<RemoteItem>();
			remoteItem.Init(a_pos, a_type, a_amount, a_isInventory);
			if (a_isInventory)
			{
				a_pos.x = Mathf.Round(a_pos.x);
				a_pos.z = Mathf.Round(a_pos.z);
				m_inventoryItems.Add(remoteItem);
			}
			else
			{
				m_worldItems[text] = remoteItem;
			}
		}
		remoteItem.Refresh();
	}

	private void UpdateOrSpawnBuilding(int a_type, Vector3 a_pos, float a_rot, float a_health, bool a_isMine)
	{
		if (0f < a_health)
		{
			string text = a_type + a_pos.ToString();
			RemoteBuilding remoteBuilding = (!m_buildings.Contains(text)) ? null : ((RemoteBuilding)m_buildings[text]);
			if (null == remoteBuilding)
			{
				RemoveNullBuildingsFromList();
				GameObject gameObject = (GameObject)Object.Instantiate(m_buildingPrefab);
				gameObject.name = "building_" + text;
				remoteBuilding = gameObject.GetComponent<RemoteBuilding>();
				remoteBuilding.Init(a_pos, a_type, a_isMine, false);
				m_buildings[text] = remoteBuilding;
			}
			remoteBuilding.Refresh(a_rot, a_health);
		}
	}

	private void RemoveNullBuildingsFromList()
	{
		foreach (DictionaryEntry building in m_buildings)
		{
			RemoteBuilding y = (RemoteBuilding)building.Value;
			if (null == y)
			{
				m_buildings.Remove(building.Key);
				break;
			}
		}
	}

	public Vector3 GetNearbyExplosion(Vector3 a_pos)
	{
		foreach (DictionaryEntry building in m_buildings)
		{
			RemoteBuilding remoteBuilding = (RemoteBuilding)building.Value;
			if (null != remoteBuilding && remoteBuilding.IsExploding())
			{
				float sqrMagnitude = (a_pos - remoteBuilding.transform.position).sqrMagnitude;
				if (sqrMagnitude < 36f)
				{
					return remoteBuilding.transform.position;
				}
			}
		}
		return Vector3.zero;
	}

	public Mission GetMission(int a_index)
	{
		return (m_missions == null || m_missions.Count <= a_index || a_index <= -1) ? null : m_missions[a_index];
	}

	public RemoteItem GetNearestItem(Vector3 a_pos)
	{
		float num = 9999999f;
		RemoteItem result = null;
		foreach (DictionaryEntry worldItem in m_worldItems)
		{
			RemoteItem remoteItem = (RemoteItem)worldItem.Value;
			if (null != remoteItem)
			{
				float sqrMagnitude = (a_pos - remoteItem.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = remoteItem;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public RemoteBuilding GetNearestResource(Vector3 a_pos)
	{
		float num = 9999999f;
		RemoteBuilding result = null;
		foreach (DictionaryEntry building in m_buildings)
		{
			RemoteBuilding remoteBuilding = (RemoteBuilding)building.Value;
			if (null != remoteBuilding && Buildings.IsResource(remoteBuilding.m_type))
			{
				float sqrMagnitude = (a_pos - remoteBuilding.transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = remoteBuilding;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public RemoteCharacter GetNearestNpc(Vector3 a_pos)
	{
		float num = 9999999f;
		RemoteCharacter result = null;
		for (int i = 0; i < m_npcs.Length; i++)
		{
			if (null != m_npcs[i] && m_npcs[i].IsNpc() && m_npcs[i].IsVisible())
			{
				float sqrMagnitude = (a_pos - m_npcs[i].transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = m_npcs[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public RemoteCharacter GetNearestCharacter(Vector3 a_pos, bool a_cars = false)
	{
		RemoteCharacter[] array = (!a_cars) ? m_players : m_cars;
		float num = 9999999f;
		RemoteCharacter result = null;
		for (int i = 0; i < array.Length; i++)
		{
			if (null != array[i] && array[i].IsVisible())
			{
				float sqrMagnitude = (a_pos - array[i].transform.position).sqrMagnitude;
				if (sqrMagnitude < num && sqrMagnitude > 0.01f)
				{
					result = array[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public bool IsSpawned()
	{
		return IsSpawned(m_myOnlineId);
	}

	public bool IsSpawned(int a_onlineId)
	{
		return m_players != null && m_players.Length > a_onlineId && a_onlineId > -1 && null != m_players[a_onlineId] && null != m_players[a_onlineId].transform;
	}

	public int GetRank()
	{
		return (-1 < m_myOnlineId) ? m_playerData[m_myOnlineId].rank : 0;
	}

	public int GetGoldCount()
	{
		return m_gold;
	}

	public bool IsInVehicle()
	{
		return m_isInVehicle;
	}

	public void ShowPartyFullPopup()
	{
		m_popupGui.ShowGui(true, LNG.Get("PARTY_POPUP_FULL"));
	}

	public bool IsValidBuildPos(Vector3 a_pos, int a_buildingType)
	{
		foreach (DictionaryEntry building in m_buildings)
		{
			RemoteBuilding remoteBuilding = (RemoteBuilding)building.Value;
			if (null != remoteBuilding && !remoteBuilding.m_isStatic)
			{
				float sqrMagnitude = (remoteBuilding.transform.position - a_pos).sqrMagnitude;
				if ((!remoteBuilding.m_isMine && Buildings.IsDoor(remoteBuilding.m_type) && sqrMagnitude < 25f) || sqrMagnitude < 0.202499986f)
				{
					return false;
				}
			}
		}
		if (!Buildings.IsHarmless(a_buildingType))
		{
			SpecialArea[] specialAreas = m_specialAreas;
			foreach (SpecialArea specialArea in specialAreas)
			{
				if (null != specialArea && (specialArea.m_type == eAreaType.noBuilding || specialArea.m_type == eAreaType.noPvp) && (specialArea.transform.position - a_pos).sqrMagnitude < specialArea.m_radius * specialArea.m_radius)
				{
					return false;
				}
			}
		}
		return Buildings.IsDoor(a_buildingType) || !Raycaster.BuildingSphereCast(a_pos);
	}
}
