using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
	public bool m_carveNavMesh = true;

	[HideInInspector]
	public List<ServerBuilding> m_buildings = new List<ServerBuilding>();

	private LidServer m_server;

	private float m_nextUpdate;

	private ResourceSpawner[] m_resSpawns;

	private void Start()
	{
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
		m_resSpawns = (ResourceSpawner[])Object.FindObjectsOfType(typeof(ResourceSpawner));
		m_nextUpdate = Random.Range(3f, 12f);
	}

	private void Update()
	{
		if (Time.time > m_nextUpdate && m_resSpawns != null && m_resSpawns.Length > 0)
		{
			SpawnResources();
			m_nextUpdate = Time.time + 30.8f;
		}
	}

	private void SpawnResources()
	{
		int num = 0;
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i])
			{
				if (Buildings.IsResource(m_buildings[i].m_type))
				{
					num++;
				}
			}
			else
			{
				m_buildings.RemoveAt(i);
				i--;
			}
		}
		if (null != m_server && Mathf.Clamp(6 + m_server.GetPlayerCount() * 2, 0, 40) > num)
		{
			int num2 = Random.Range(0, m_resSpawns.Length);
			float radius = m_resSpawns[num2].m_radius;
			Vector3 a_pos = m_resSpawns[num2].transform.position + new Vector3(Random.Range(0f - radius, radius), 0f, Random.Range(0f - radius, radius));
			CreateBuilding(m_resSpawns[num2].m_resourceBuildingType, a_pos);
		}
	}

	public bool CreateBuilding(int a_type, Vector3 a_pos, int a_ownerPid = 0, float a_yRot = 0f, int a_health = 100, bool a_isNew = true)
	{
		bool flag = !a_isNew;
		if (!flag)
		{
			ServerBuilding nearestPlayerBuilding = GetNearestPlayerBuilding(a_pos);
			if (null == nearestPlayerBuilding || (nearestPlayerBuilding.transform.position - a_pos).sqrMagnitude > 0.202499986f)
			{
				nearestPlayerBuilding = ((Buildings.IsCollider(a_type) && a_ownerPid != 0) ? GetNearestPlayerBuilding(a_pos, a_ownerPid) : null);
				if ((null == nearestPlayerBuilding || !Buildings.IsDoor(nearestPlayerBuilding.m_type) || (nearestPlayerBuilding.transform.position - a_pos).sqrMagnitude > 25f) && (Buildings.IsDoor(a_type) || !Raycaster.BuildingSphereCast(a_pos)) && 0.8f < Util.GetTerrainHeight(a_pos))
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			GameObject gameObject = (GameObject)Resources.Load("buildings/building_" + a_type);
			if (null != gameObject)
			{
				GameObject gameObject2 = (GameObject)Object.Instantiate(gameObject, a_pos, Quaternion.Euler(0f, a_yRot, 0f));
				ServerBuilding component = gameObject2.GetComponent<ServerBuilding>();
				NavMeshObstacle componentInChildren = gameObject2.GetComponentInChildren<NavMeshObstacle>();
				if (null != componentInChildren)
				{
					componentInChildren.carving = m_carveNavMesh;
				}
				if (null != component)
				{
					component.Init(m_server, a_type, a_ownerPid, a_health, a_isNew);
					m_buildings.Add(component);
				}
				else
				{
					Debug.Log("BuildingManager.cs: ERROR: Building without ServerBuilding.cs script spawned!");
				}
			}
		}
		return flag;
	}

	public bool RepairBuilding(ServerPlayer a_player, Vector3 a_distCheckPos)
	{
		return UseBuilding(a_player, a_distCheckPos, true);
	}

	public bool UseBuilding(ServerPlayer a_player, Vector3 a_distCheckPos, bool a_repair = false)
	{
		bool result = false;
		float num = 9999999f;
		int num2 = -1;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i])
			{
				zero = m_buildings[i].transform.position;
				if (Buildings.IsDoor(m_buildings[i].m_type))
				{
					zero += m_buildings[i].transform.right;
				}
				float sqrMagnitude = (zero - a_distCheckPos).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num2 = i;
					num = sqrMagnitude;
				}
			}
		}
		float num3 = (num2 == -1 || m_buildings[num2].m_type != 103) ? 2.4f : 1.2f;
		if (num < num3)
		{
			result = ((!a_repair) ? m_buildings[num2].Use(a_player) : m_buildings[num2].Repair(a_player));
		}
		return result;
	}

	public ServerBuilding GetNearestPlayerBuilding(Vector3 a_pos, int a_ignoreOwnerPid = -1)
	{
		float num = 9999999f;
		ServerBuilding result = null;
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i] && m_buildings[i].GetState() > 0f && m_buildings[i].GetOwnerId() != a_ignoreOwnerPid && m_buildings[i].GetOwnerId() != 0)
			{
				float sqrMagnitude = (a_pos - m_buildings[i].transform.position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					result = m_buildings[i];
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public Vector3 GetRespawnPos(Vector3 a_pos, int a_pid)
	{
		Vector3 result = Vector3.zero;
		float num = 9999999f;
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i] && m_buildings[i].m_type == 101 && m_buildings[i].GetState() > 0f && m_buildings[i].GetOwnerId() == a_pid && m_buildings[i].GetOwnerId() != 0)
			{
				float sqrMagnitude = (a_pos - m_buildings[i].transform.position).sqrMagnitude;
				if (sqrMagnitude < num && sqrMagnitude > 400f)
				{
					result = m_buildings[i].transform.position;
					num = sqrMagnitude;
				}
			}
		}
		return result;
	}

	public void IgnoreBedCollision(int a_pid, Collider a_playerCollider)
	{
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i] && m_buildings[i].m_type == 101 && m_buildings[i].GetState() > 0f && m_buildings[i].GetOwnerId() == a_pid && m_buildings[i].GetOwnerId() != 0)
			{
				Physics.IgnoreCollision(m_buildings[i].collider, a_playerCollider);
			}
		}
	}

	public bool IsNearCampfire(Vector3 a_pos)
	{
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i] && m_buildings[i].m_type == 100 && m_buildings[i].GetState() < 0.5f && (m_buildings[i].transform.position - a_pos).sqrMagnitude < 16f)
			{
				return true;
			}
		}
		return false;
	}

	public Lootbox AddItemToLootContainer(DatabaseItem a_item)
	{
		for (int i = 0; i < m_buildings.Count; i++)
		{
			if (null != m_buildings[i] && m_buildings[i].m_type == 103 && 0f < m_buildings[i].GetState())
			{
				Lootbox lootbox = (Lootbox)m_buildings[i];
				if (null != lootbox && a_item.cid == lootbox.m_cid && lootbox.m_container != null)
				{
					lootbox.m_container.UpdateOrCreateItem(a_item);
					return lootbox;
				}
			}
		}
		return null;
	}
}
