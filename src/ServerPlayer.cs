using Lidgren.Network;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayer
{
	private const float c_xpForgetTime = 20f;

	private const int c_npcLayer = 9;

	private const int c_playerLayer = 13;

	private const int c_buildingLayer = 19;

	private const int c_vehicleLayer = 11;

	public int m_onlineId = -1;

	public string m_name = string.Empty;

	public bool m_isAdmin;

	public int m_partyId;

	public int m_partyRank;

	public int m_partyInviteId;

	public float m_nextPartyActionTime;

	public eCharType m_charType;

	public NetConnection m_connection;

	public float m_nextUpdate;

	public int m_updateCount;

	public int m_pid = -1;

	public ulong m_accountId;

	public int m_lookIndex;

	public int m_skinIndex;

	public ItemContainer m_freeWorldContainer;

	public ItemContainer m_persistentContainer;

	public ShopContainer m_shopContainer;

	public bool m_updateInfoFlag = true;

	public bool m_updateContainersFlag = true;

	public int m_gold;

	public float m_cantLogoutTime = -1f;

	public float m_disconnectTime = -1f;

	public ItemContainer m_inventory;

	private ControlledChar m_char;

	private ServerVehicle m_vehicle;

	private float m_health = 100f;

	private float m_energy = 100f;

	private float m_karma = 100f;

	private float m_lastUpdateKarma = 100f;

	private int m_xp;

	private int m_lastRank = -1;

	private int m_condition;

	private int m_lastCondition;

	private int m_rank;

	private float m_rankProgress;

	private float m_healthGainPerSec = 0.6f;

	private float m_healHealthGainPerSec = 10f;

	private float m_energyLossPerSec = 0.1f;

	private float m_karmaGainPerSec = 0.0075f;

	private float m_healEndTime;

	private SQLThreadManager m_sql;

	private LidServer m_server;

	private BuildingManager m_buildMan;

	private MissionManager m_missionMan;

	private Transform m_victim;

	private List<KillXp> m_killXp = new List<KillXp>();

	private float m_rndSpawnRadius = 3f;

	private bool m_attackBtnPressed;

	private float m_buildRotation;

	private float m_forcedRotation = -1f;

	private bool m_isMoving;

	private float m_respawnTime = -1f;

	private float m_nextAttackTime;

	private float m_nextForbiddenEventTime;

	private float m_nextComplexConditionUpdate;

	private float m_nextPossibleBedSpawnTime;

	private float m_interactVehicleTime = -1f;

	private float m_nextOutOfAmmoMsgTime;

	public ServerPlayer(string a_name, ulong a_accountId, int a_onlineId, eCharType a_type, NetConnection a_con, SQLThreadManager a_sql, LidServer a_server, BuildingManager a_buildMan, MissionManager a_missionMan)
	{
		m_name = a_name;
		m_accountId = a_accountId;
		m_onlineId = a_onlineId;
		m_charType = a_type;
		m_connection = a_con;
		if (m_connection != null)
		{
			m_connection.Tag = m_onlineId;
		}
		m_sql = a_sql;
		m_server = a_server;
		m_buildMan = a_buildMan;
		m_missionMan = a_missionMan;
	}

	public void Spawn(GameObject a_prefab, DatabasePlayer a_dbPlayer)
	{
		Vector3 position = Vector3.zero;
		if (a_dbPlayer.x == 0f && 0f == a_dbPlayer.y)
		{
			a_dbPlayer.karma = 200;
			ServerTutorial tutorial = m_server.GetTutorial();
			if (null != tutorial)
			{
				position = tutorial.StartTutorial();
			}
		}
		else
		{
			position.x = a_dbPlayer.x;
			position.z = a_dbPlayer.y;
		}
		position.y = 0f;
		m_pid = a_dbPlayer.pid;
		GameObject gameObject = (GameObject)Object.Instantiate(a_prefab, position, Quaternion.identity);
		m_char = gameObject.GetComponent<ControlledChar>();
		m_health = a_dbPlayer.health;
		m_energy = a_dbPlayer.energy;
		m_karma = a_dbPlayer.karma;
		m_lastUpdateKarma = a_dbPlayer.karma;
		m_xp = a_dbPlayer.xp;
		m_condition = a_dbPlayer.condition;
		m_gold = a_dbPlayer.gold;
		m_partyId = a_dbPlayer.partyId;
		m_partyRank = a_dbPlayer.partyRank;
		RecalculateRank();
		if (m_health == 0f)
		{
			m_respawnTime = Time.time;
		}
		m_char.Init(this);
		m_inventory = new ItemContainer(5, 4, 0, SQLThreadManager.PidToCid(m_pid), m_sql, this);
		m_buildMan.IgnoreBedCollision(m_pid, m_char.collider);
	}

	public bool IsAttacking()
	{
		return Time.time < m_nextAttackTime;
	}

	public bool IsSpawned()
	{
		return null != m_char;
	}

	public bool HasUseChanged()
	{
		return IsSpawned() && m_char.HasUseChanged();
	}

	public bool HasSpaceChanged()
	{
		return IsSpawned() && m_char.HasSpaceChanged();
	}

	public void SetRotation(float a_rot)
	{
		m_forcedRotation = a_rot;
	}

	public void SetVictim(Transform a_victim)
	{
		m_victim = a_victim;
	}

	public Transform GetVictim()
	{
		return m_victim;
	}

	public void ResetContainers()
	{
		m_freeWorldContainer = null;
		m_persistentContainer = null;
		m_shopContainer = null;
		m_updateContainersFlag = true;
		m_server.SendShopInfo(this, -1f, -1f);
	}

	public Transform GetTransform()
	{
		Transform result = null;
		if (null != m_vehicle)
		{
			result = m_vehicle.transform;
		}
		else if (IsSpawned())
		{
			result = m_char.transform;
		}
		return result;
	}

	public Vector3 GetPosition()
	{
		Transform transform = GetTransform();
		return (!(null == transform)) ? GetTransform().position : Vector3.zero;
	}

	public void SetPosition(Vector3 a_pos)
	{
		if (IsSpawned())
		{
			a_pos.y = 0f;
			m_char.transform.position = a_pos;
		}
	}

	public float GetRotation()
	{
		float result = 0f;
		if (null != m_vehicle)
		{
			Vector3 eulerAngles = m_vehicle.transform.rotation.eulerAngles;
			result = eulerAngles.y;
		}
		else if (IsSpawned())
		{
			result = m_char.GetRotation();
		}
		return result;
	}

	public Vector3 GetForward()
	{
		return Quaternion.Euler(0f, GetRotation(), 0f) * Vector3.forward;
	}

	public void ConsumeItem(int a_itemType)
	{
		if (Items.IsMedicine(a_itemType))
		{
			switch (a_itemType)
			{
			case 140:
				SetCondition(eCondition.bleeding, false);
				break;
			case 141:
				SetCondition(eCondition.infection, false);
				break;
			case 142:
				SetCondition(eCondition.pain, false);
				break;
			case 143:
			{
				float time = Time.time;
				ItemDef itemDef = Items.GetItemDef(a_itemType);
				m_healEndTime = time + itemDef.healing / m_healHealthGainPerSec;
				SetCondition(eCondition.pain, false);
				SetCondition(eCondition.bleeding, false);
				break;
			}
			}
		}
		else if (Items.IsEatable(a_itemType))
		{
			ItemDef itemDef2 = Items.GetItemDef(a_itemType);
			ChangeEnergyBy(itemDef2.healing);
		}
	}

	public void SetCondition(eCondition a_condition, bool a_state)
	{
		m_condition = ((!a_state) ? (m_condition & ~(1 << (int)a_condition)) : (m_condition | (1 << (int)a_condition)));
	}

	public int GetConditions()
	{
		return m_condition;
	}

	public void SetConditions(int a_conditions)
	{
		m_condition = a_conditions;
	}

	public bool HasCondition(eCondition a_condition)
	{
		return 0 < (m_condition & (1 << (int)a_condition));
	}

	public void Progress(float a_deltaTime)
	{
		if (!IsSpawned())
		{
			return;
		}
		DatabaseItem itemFromPos = m_inventory.GetItemFromPos(0f, 0f);
		ItemDef itemDef = Items.GetItemDef(itemFromPos.type);
		bool flag = itemDef.buildingIndex > 0;
		if (!IsDead())
		{
			UpdateConditions(a_deltaTime);
			CalculateEnergyHealthKarma(a_deltaTime);
			if (m_attackBtnPressed)
			{
				if (m_server.IsInSpecialArea(GetPosition(), eAreaType.noPvp) || (flag && !Buildings.IsHarmless(itemDef.buildingIndex) && m_server.IsInSpecialArea(GetPosition(), eAreaType.noBuilding)))
				{
					if (Time.time > m_nextForbiddenEventTime)
					{
						m_server.SendSpecialEvent(this, eSpecialEvent.forbidden);
						m_nextForbiddenEventTime = Time.time + 1f;
					}
				}
				else if (null == m_vehicle)
				{
					TryAttack(itemFromPos.type, itemDef);
				}
			}
			HandleRotation();
			DatabaseItem itemFromPos2 = m_inventory.GetItemFromPos(0f, 3f);
			ItemDef itemDef2 = Items.GetItemDef(itemFromPos2.type);
			float num = (!Items.IsShoes(itemFromPos2.type)) ? 0f : itemDef2.healing;
			num -= ((!HasCondition(eCondition.pain)) ? 0f : 0.2f);
			m_char.AddSpeed(num);
			if (Items.IsShoes(itemFromPos2.type) && m_isMoving && null == m_vehicle)
			{
				DamageItem(0f, 3f);
			}
		}
		m_char.m_isWalking = (IsAttacking() || itemDef.buildingIndex > 0);
		if (m_respawnTime > 0f && Time.time > m_respawnTime)
		{
			Respawn();
			m_respawnTime = -1f;
		}
		int num2 = 0;
		bool flag2;
		while (true)
		{
			if (num2 >= m_killXp.Count)
			{
				return;
			}
			float time = Time.time;
			KillXp killXp = m_killXp[num2];
			flag2 = (time > killXp.deletetime);
			KillXp killXp2 = m_killXp[num2];
			if (null != killXp2.npc)
			{
				KillXp killXp3 = m_killXp[num2];
				if (killXp3.npc.GetHealth() == 0f)
				{
					break;
				}
			}
			KillXp killXp4 = m_killXp[num2];
			if (killXp4.player != null)
			{
				KillXp killXp5 = m_killXp[num2];
				if (killXp5.player.IsDead())
				{
					break;
				}
			}
			if (flag2)
			{
				break;
			}
			num2++;
		}
		if (!flag2)
		{
			KillXp killXp6 = m_killXp[num2];
			AddXp((int)killXp6.xp);
		}
		m_killXp.RemoveAt(num2);
	}

	public void Remove()
	{
		if (m_char != null)
		{
			Object.Destroy(m_char.gameObject);
		}
	}

	public float GetHealth()
	{
		return m_health;
	}

	public float GetEnergy()
	{
		return m_energy;
	}

	public float GetKarma()
	{
		return m_karma;
	}

	public bool IsSaint()
	{
		return 199f <= m_karma;
	}

	public int GetRank()
	{
		return m_rank;
	}

	public float GetRankProgress()
	{
		return m_rankProgress;
	}

	public int GetXp()
	{
		return m_xp;
	}

	public void AddXp(int a_xp)
	{
		m_xp = Mathf.Max(m_xp + a_xp, 0);
		RecalculateRank();
        LidServer.SendRankUpdate(this, a_xp);
	}

	public float ChangeHealthBy(float a_delta)
	{
		a_delta = HandleDamage(a_delta);
		m_health = Mathf.Clamp(m_health + a_delta, 0f, 100.001f);
		if (IsDead() && m_respawnTime < 0f)
		{
			ExitVehicle();
			if (IsSpawned())
			{
				m_char.AssignInput(0f, 0f, false, false);
				m_char.collider.enabled = false;
			}
			DropLootAsContainer();
			ResetContainers();
			if (null != m_missionMan)
			{
				m_missionMan.DeleteMissions(this);
			}
			m_condition = 0;
			AddXp((int)((float)m_xp * 0.05f) * -1);
			m_gold = (int)((float)m_gold * 0.95f);
            LidServer.SendMoneyUpdate(this);
			m_cantLogoutTime = -1f;
			m_respawnTime = Time.time + 5f;
		}
		return m_health;
	}

	public float ChangeEnergyBy(float a_delta)
	{
		m_energy = Mathf.Clamp(m_energy + a_delta, 0f, 100.001f);
		return m_energy;
	}

	public float ChangeKarmaBy(float a_delta)
	{
		m_karma = Mathf.Clamp(m_karma + a_delta, 0f, 200.001f);
		if ((int)m_karma != (int)m_lastUpdateKarma)
		{
			m_updateInfoFlag = true;
			m_lastUpdateKarma = m_karma;
		}
		return m_karma;
	}

	public bool IsDead()
	{
		return m_health < 1f;
	}

	public bool IsVisible()
	{
		return IsSpawned() && m_char.collider.enabled;
	}

	public void AssignInput(float a_v, float a_h, bool a_use, bool a_space, float a_buildRotation)
	{
		if (null == m_vehicle)
		{
			if (IsSpawned())
			{
				m_char.AssignInput(a_v, a_h, a_use, a_space);
				m_attackBtnPressed = a_space;
			}
		}
		else if (m_onlineId == m_vehicle.m_data.passengerIds[0])
		{
			m_vehicle.AssignInput(a_v, a_h, a_space);
			m_attackBtnPressed = false;
		}
		m_isMoving = (a_v != 0f || 0f != a_h);
		m_buildRotation = a_buildRotation;
	}

	public int GetVehicleId()
	{
		return (!(null == m_vehicle)) ? m_vehicle.m_id : (-1);
	}

	public bool ExitVehicle(bool a_force = false)
	{
		if (null != m_vehicle && Time.time > m_interactVehicleTime)
		{
			Vector3 passengerExitPos = m_vehicle.GetPassengerExitPos(m_onlineId);
			if (Vector3.zero == passengerExitPos && !IsDead() && !a_force)
			{
				m_server.SendSpecialEvent(this, eSpecialEvent.carExitsBlocked);
			}
			else if (Vector3.zero != passengerExitPos || IsDead() || a_force)
			{
				SetPosition((!(Vector3.zero != passengerExitPos)) ? m_vehicle.transform.position : passengerExitPos);
				m_vehicle.RemovePassenger(m_onlineId);
				SetVehicle(null);
				m_interactVehicleTime = Time.time + 1f;
				return true;
			}
		}
		return false;
	}

	public ServerVehicle GetVehicle()
	{
		return m_vehicle;
	}

	public bool CanEnterExitVehicle()
	{
		return Time.time > m_interactVehicleTime;
	}

	public bool SetVehicle(ServerVehicle a_vehicle)
	{
		if (Time.time > m_interactVehicleTime)
		{
			m_vehicle = a_vehicle;
			m_interactVehicleTime = Time.time + 1f;
			if (m_char != null)
			{
				m_char.collider.enabled = (null == m_vehicle);
			}
			return true;
		}
		return false;
	}

	private void RecalculateRank()
	{
		int num = Mathf.Max(m_xp, 1);
		float num2 = (float)num * 0.001f;
		m_rank = 0;
		if (num2 >= 0.25f && num2 < 1f)
		{
			m_rank = ((num2 < 0.5f) ? 1 : 2);
		}
		else if (num2 >= 1f)
		{
			m_rank = Mathf.Max(3 + (int)Mathf.Log(num2, 2f), 0);
		}
		int num3 = (0 < m_rank) ? ((int)(Mathf.Pow(2f, m_rank - 3) * 1000f)) : 0;
		int num4 = (int)(Mathf.Pow(2f, m_rank - 2) * 1000f);
		m_rankProgress = (float)(num - num3) / (float)(num4 - num3);
		if (m_lastRank != m_rank)
		{
			if (m_lastRank != -1)
			{
				m_updateInfoFlag = true;
			}
			m_lastRank = m_rank;
		}
	}

	private float HandleDamage(float a_healthDif)
	{
		float num = a_healthDif;
		if (num < -2f)
		{
			m_cantLogoutTime = Time.time + 20f;
			DatabaseItem itemFromPos = m_inventory.GetItemFromPos(0f, 2f);
			if (Items.IsBody(itemFromPos.type))
			{
				ItemDef itemDef = Items.GetItemDef(itemFromPos.type);
				num *= itemDef.healing;
				DamageItem(0f, 2f);
			}
			if (Mathf.Abs(num) > Random.Range(0f, 140f))
			{
				switch (Random.Range(0, 3))
				{
				case 0:
					SetCondition(eCondition.bleeding, true);
					break;
				case 1:
					SetCondition(eCondition.infection, true);
					break;
				case 2:
					SetCondition(eCondition.pain, true);
					break;
				}
			}
		}
		return num;
	}

	private void HandleRotation()
	{
		float num = -1f;
		if (IsAttacking())
		{
			if (null != m_victim)
			{
				Vector3 eulerAngles = Quaternion.LookRotation(m_victim.position - m_char.transform.position).eulerAngles;
				num = eulerAngles.y;
			}
			else if (m_forcedRotation > -1f)
			{
				num = m_forcedRotation;
				m_forcedRotation = -1f;
			}
		}
		if (num != -1f)
		{
			m_char.SetForceRotation(num);
		}
	}

	private void DropLootAsContainer()
	{
		if (m_inventory != null)
		{
			for (int i = 0; i < m_inventory.m_items.Count; i++)
			{
				LidServer server = m_server;
				DatabaseItem databaseItem = m_inventory.m_items[i];
				int type = databaseItem.type;
				DatabaseItem databaseItem2 = m_inventory.m_items[i];
				server.CreateTempContainerItem(type, databaseItem2.amount, GetPosition());
			}
			m_inventory.m_items.Clear();
			m_sql.ClearInventory(m_pid);
			m_updateContainersFlag = true;
			m_updateInfoFlag = true;
		}
	}

	private Vector3 Respawn()
	{
		Vector3 vector = Vector3.zero;
		if (Time.time > m_nextPossibleBedSpawnTime)
		{
			vector = m_buildMan.GetRespawnPos(GetPosition(), m_pid);
			m_nextPossibleBedSpawnTime = Time.time + 60f;
		}
		if (Vector3.zero == vector)
		{
			SpawnPos[] spawnPoints = m_server.GetSpawnPoints();
			if (spawnPoints.Length > 0)
			{
				vector = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
				vector.x += Random.Range(0f - m_rndSpawnRadius, m_rndSpawnRadius);
				vector.y = 0f;
				vector.z += Random.Range(0f - m_rndSpawnRadius, m_rndSpawnRadius);
			}
		}
		if (IsSpawned())
		{
			SetPosition(vector);
			ChangeHealthBy(100f);
			ChangeEnergyBy(100f);
		}
		m_char.collider.enabled = true;
		m_buildMan.IgnoreBedCollision(m_pid, m_char.collider);
		return vector;
	}

	private void TryAttack(int a_itemType, ItemDef a_handItemDef)
	{
		if (a_handItemDef.buildingIndex > 0)
		{
			if (m_buildMan.CreateBuilding(a_handItemDef.buildingIndex, m_char.transform.position + GetForward() * 2f, m_pid, m_buildRotation))
			{
				m_inventory.DeclineHandItem();
				AddXp(20);
				if (a_handItemDef.buildingIndex == 102 && IsSaint())
				{
					ChangeKarmaBy(-2.5f);
				}
				m_updateContainersFlag = true;
				m_updateInfoFlag = true;
			}
			return;
		}
		ItemDef itemDef = a_handItemDef;
		if (itemDef.damage == 0f)
		{
			itemDef = Items.GetItemDef(0);
		}
		if (!(Time.time > m_nextAttackTime))
		{
			return;
		}
		if (FireWeapon(itemDef))
		{
			if (!HandleSpecialItemAbility(a_itemType))
			{
				Vector3 a_targetPos = m_char.transform.position + GetForward() * 1.2f;
				Transform a_target = m_victim;
				float a_weaponDamage = Raycaster.Attack(m_char.transform, itemDef, a_targetPos, ref a_target);
				CalculateXpAndKarma(a_target, a_weaponDamage);
			}
			m_nextAttackTime = Time.time + itemDef.attackdur;
		}
		else if (Time.time > m_nextOutOfAmmoMsgTime)
		{
			m_server.SendSpecialEvent(this, eSpecialEvent.noAmmo);
			m_nextOutOfAmmoMsgTime = Time.time + 1f;
		}
	}

	private bool FireWeapon(ItemDef a_weaponItem)
	{
		bool flag = true;
		if (a_weaponItem.ammoItemType > 0)
		{
			flag = false;
			if (m_inventory.m_items.Count > 0)
			{
				for (int i = 0; i < m_inventory.m_items.Count; i++)
				{
					DatabaseItem databaseItem = m_inventory.m_items[i];
					if (databaseItem.type == a_weaponItem.ammoItemType)
					{
						DatabaseItem value = m_inventory.m_items[i];
						value.amount--;
						if (value.amount < 1)
						{
							value.flag = eDbAction.delete;
						}
						else
						{
							value.flag = eDbAction.update;
						}
						m_inventory.m_items[i] = value;
						flag = true;
						break;
					}
				}
			}
		}
		if (flag)
		{
			DamageItem(0f, 0f);
		}
		return flag;
	}

	private bool HandleSpecialItemAbility(int a_itemType)
	{
		bool flag = false;
		switch (a_itemType)
		{
		case 109:
			m_server.Dig(GetPosition());
			break;
		case 92:
		{
			Vector3 a_distCheckPos = GetPosition() + GetForward() * 1.2f;
			flag = m_server.RepairVehicle(this, a_distCheckPos);
			if (!flag)
			{
				flag = m_buildMan.RepairBuilding(this, a_distCheckPos);
			}
			break;
		}
		case 110:
		{
			Vector3 a_pos = GetPosition() + GetForward() * 3.5f;
			if (0.6f > Util.GetTerrainHeight(a_pos))
			{
				if (Random.Range(0, 20) == 0)
				{
					m_server.CreateFreeWorldItem(11, 1, GetPosition());
				}
				else
				{
					m_server.SendSpecialEvent(this, eSpecialEvent.fishingfail);
				}
			}
			break;
		}
		}
		return flag;
	}

	private void DamageItem(float a_x, float a_y)
	{
		DatabaseItem itemFromPos = m_inventory.GetItemFromPos(a_x, a_y);
		ItemDef itemDef = Items.GetItemDef(itemFromPos.type);
		if (!(0f < itemDef.durability) || !(1f > itemDef.durability) || !(Random.Range(0f, 1f) > itemDef.durability) || !m_inventory.DeclineItem(a_x, a_y))
		{
			return;
		}
		m_updateContainersFlag = true;
		if (itemFromPos.amount != 1)
		{
			return;
		}
		m_updateInfoFlag = true;
		m_server.SendSpecialEvent(this, eSpecialEvent.itemBroke);
		if (0 < itemDef.wood)
		{
			int num = Random.Range(0, itemDef.wood);
			if (0 < num)
			{
				m_server.CreateFreeWorldItem(130, num, GetPosition());
			}
		}
		if (0 < itemDef.metal)
		{
			int num2 = Random.Range(0, itemDef.metal);
			if (0 < num2)
			{
				m_server.CreateFreeWorldItem(131, num2, GetPosition());
			}
		}
		if (0 < itemDef.stone)
		{
			int num3 = Random.Range(0, itemDef.stone);
			if (0 < num3)
			{
				m_server.CreateFreeWorldItem(132, num3, GetPosition());
			}
		}
		if (0 < itemDef.cloth)
		{
			int num4 = Random.Range(0, itemDef.cloth);
			if (0 < num4)
			{
				m_server.CreateFreeWorldItem(133, num4, GetPosition());
			}
		}
	}

	private void UpdateConditions(float a_deltaTime)
	{
		bool flag = a_deltaTime > Random.Range(0f, 150f);
		if (HasCondition(eCondition.bleeding) && flag)
		{
			SetCondition(eCondition.bleeding, false);
			flag = false;
		}
		if (HasCondition(eCondition.infection) && flag)
		{
			SetCondition(eCondition.infection, false);
			flag = false;
		}
		if (HasCondition(eCondition.pain) && flag)
		{
			SetCondition(eCondition.pain, false);
			flag = false;
		}
		SetCondition(eCondition.starvation, m_energy < 0.01f);
		if (Time.time > m_nextComplexConditionUpdate && m_inventory != null)
		{
			Vector3 position = GetPosition();
			DatabaseItem itemFromPos = m_inventory.GetItemFromPos(0f, 2f);
			bool a_state = 0.1f > m_server.GetDayLight() && !Items.IsBody(itemFromPos.type) && !m_server.IsInSpecialArea(position, eAreaType.warm) && !m_buildMan.IsNearCampfire(position);
			SetCondition(eCondition.freezing, a_state);
			SetCondition(eCondition.radiation, m_server.IsInSpecialArea(position, eAreaType.radiation));
			m_nextComplexConditionUpdate = Time.time + 3f;
		}
		if (m_lastCondition != m_condition)
		{
			m_lastCondition = m_condition;
            LidServer.SendConditionUpdate(this);
		}
	}

	private void CalculateEnergyHealthKarma(float a_deltaTime)
	{
		float num = 0f;
		float num2 = m_energyLossPerSec;
		if (HasCondition(eCondition.starvation))
		{
			num -= 0.2f;
		}
		if (HasCondition(eCondition.bleeding))
		{
			num -= 0.3f;
		}
		if (HasCondition(eCondition.infection))
		{
			num -= 0.15f;
		}
		if (HasCondition(eCondition.radiation))
		{
			num -= 0.1f;
		}
		if (m_healEndTime > Time.time)
		{
			num = m_healHealthGainPerSec;
		}
		else if (num == 0f)
		{
			num = m_healthGainPerSec * (m_energy * 0.01f);
		}
		if (HasCondition(eCondition.freezing) || HasCondition(eCondition.radiation))
		{
			num2 *= 2f;
		}
		ChangeHealthBy(a_deltaTime * num);
		ChangeEnergyBy(a_deltaTime * (0f - num2));
		ChangeKarmaBy(a_deltaTime * m_karmaGainPerSec);
	}

	private void CalculateXpAndKarma(Transform a_victim, float a_weaponDamage)
	{
		if (!(null != a_victim) || !(0f < a_weaponDamage))
		{
			return;
		}
		ServerNpc serverNpc = (a_victim.gameObject.layer != 9) ? null : a_victim.GetComponent<ServerNpc>();
		if (null == serverNpc)
		{
			if (a_victim.gameObject.layer == 13)
			{
				ServerPlayer serverPlayer = null;
				ControlledChar component = a_victim.GetComponent<ControlledChar>();
				serverPlayer = ((!(null != component)) ? null : component.GetServerPlayer());
				if (serverPlayer == null || serverPlayer.IsSaint())
				{
					return;
				}
				if (8f < serverPlayer.m_karma)
				{
					float num = a_weaponDamage * 0.5f * (serverPlayer.m_karma / 200f);
					if (IsSaint())
					{
						num = Mathf.Max(2.5f, num);
					}
					ChangeKarmaBy(0f - num);
				}
				else if (IsSaint())
				{
					ChangeKarmaBy(-2.5f);
				}
			}
			else if (a_victim.gameObject.layer == 19)
			{
				ServerBuilding component2 = a_victim.GetComponent<ServerBuilding>();
				if (null != component2 && m_pid != component2.GetOwnerId() && IsSaint())
				{
					ChangeKarmaBy(-2.5f);
				}
			}
			else
			{
				if (a_victim.gameObject.layer != 11)
				{
					return;
				}
				ServerVehicle component3 = a_victim.GetComponent<ServerVehicle>();
				if (null != component3 && 0 < component3.GetPassengerCount())
				{
					float num2 = a_weaponDamage * 0.25f;
					if (IsSaint())
					{
						num2 = Mathf.Max(2.5f, num2);
					}
					ChangeKarmaBy(0f - num2);
				}
			}
			return;
		}
		int handItem = serverNpc.GetHandItem();
		float num3 = Mathf.Min(serverNpc.GetLastHealth(), a_weaponDamage);
		float num4 = num3 * 0.2f + Items.GetWeaponXpMultiplier(handItem) * (num3 * 0.01f);
		if (!(0f < num4))
		{
			return;
		}
		bool flag = true;
		for (int i = 0; i < m_killXp.Count; i++)
		{
			KillXp killXp = m_killXp[i];
			if (killXp.player == null)
			{
				KillXp killXp2 = m_killXp[i];
				if (serverNpc == killXp2.npc)
				{
					KillXp value = m_killXp[i];
					value.xp += num4;
					value.deletetime = Time.time + 20f;
					m_killXp[i] = value;
					flag = false;
					break;
				}
			}
		}
		if (flag)
		{
			KillXp item = new KillXp(null, serverNpc, num4, Time.time + 20f);
			m_killXp.Add(item);
		}
	}
}
