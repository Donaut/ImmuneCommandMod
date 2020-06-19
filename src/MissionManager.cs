using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
	private const float m_missionXp = 200.001f;

	private MissionNpc[] m_missionNpcs;

	private MissionObjective[] m_missionObjs;

	private LidServer m_server;

	private float[] m_missionXpLocMultip;

	private Hashtable m_missions = new Hashtable();

	private Hashtable m_missionProposals = new Hashtable();

	private float m_nextMissionDeleteUpdate;

	private void Start()
	{
		m_missionNpcs = (MissionNpc[])Object.FindObjectsOfType(typeof(MissionNpc));
		m_missionObjs = (MissionObjective[])Object.FindObjectsOfType(typeof(MissionObjective));
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
		m_missionXpLocMultip = new float[15];
		for (int i = 0; i < 15; i++)
		{
			m_missionXpLocMultip[i] = 1f;
		}
		m_missionXpLocMultip[1] = 0.8f;
		m_missionXpLocMultip[2] = 1.1f;
		m_missionXpLocMultip[3] = 1.5f;
		m_missionXpLocMultip[4] = 0.8f;
		m_missionXpLocMultip[5] = 1.2f;
		m_missionXpLocMultip[6] = 0.5f;
		m_missionXpLocMultip[7] = 0.7f;
		m_missionXpLocMultip[8] = 1.2f;
		m_missionXpLocMultip[9] = 1f;
		m_missionXpLocMultip[10] = 2f;
		m_missionXpLocMultip[11] = 1.4f;
		m_missionXpLocMultip[12] = 1.3f;
		m_missionXpLocMultip[13] = 1.5f;
		m_missionXpLocMultip[14] = 1.3f;
	}

	private void Update()
	{
		if (Time.time > m_nextMissionDeleteUpdate)
		{
			foreach (DictionaryEntry mission in m_missions)
			{
				List<Mission> list = (List<Mission>)mission.Value;
				for (int i = 0; i < list.Count; i++)
				{
					if (Time.time > list[i].m_dieTime)
					{
						list.RemoveAt(i);
						UpdatePlayer(m_server.GetPlayerByPid((int)mission.Key));
						break;
					}
				}
				if (list.Count == 0)
				{
					m_missions.Remove(mission.Key);
					break;
				}
			}
			m_nextMissionDeleteUpdate = Time.time + 1f;
		}
	}

	private MissionNpc GetNearbyNpc(Vector3 a_pos)
	{
		for (int i = 0; i < m_missionNpcs.Length; i++)
		{
			Vector3 position = m_missionNpcs[i].transform.position;
			Vector3 vector = a_pos;
			if (Mathf.Abs(position.x - vector.x) < 1.4f && Mathf.Abs(position.z - vector.z) < 1.4f)
			{
				return m_missionNpcs[i];
			}
		}
		return null;
	}

	public void DeleteMissions(ServerPlayer a_player)
	{
		if (a_player != null && m_missions.ContainsKey(a_player.m_pid))
		{
			m_missions.Remove(a_player.m_pid);
			UpdatePlayer(a_player);
		}
	}

	public void UpdatePlayer(ServerPlayer a_player)
	{
		if (a_player != null)
		{
			m_server.SendMissionUpdate(a_player, (!m_missions.ContainsKey(a_player.m_pid)) ? null : ((List<Mission>)m_missions[a_player.m_pid]));
		}
	}

	public bool RequestMission(ServerPlayer a_player)
	{
		bool flag = m_missions.ContainsKey(a_player.m_pid) && 3 <= ((List<Mission>)m_missions[a_player.m_pid]).Count;
		MissionNpc nearbyNpc = GetNearbyNpc(a_player.GetPosition());
		if (null != nearbyNpc)
		{
			if (flag)
			{
				m_server.SendSpecialEvent(a_player, eSpecialEvent.tooManyMissions);
			}
			else
			{
				bool flag2 = false;
				if (m_missions.ContainsKey(a_player.m_pid))
				{
					List<Mission> list = (List<Mission>)m_missions[a_player.m_pid];
					for (int i = 0; i < list.Count; i++)
					{
						if (list[i].IsEqual(nearbyNpc.m_mission))
						{
							flag2 = true;
							m_server.SendSpecialEvent(a_player, eSpecialEvent.alreadyGotMission);
							break;
						}
					}
				}
				if (!flag2)
				{
					m_missionProposals[a_player.m_pid] = nearbyNpc.m_mission;
					m_server.SendMissionPropose(a_player, nearbyNpc.m_mission);
				}
			}
		}
		return !flag && null != nearbyNpc;
	}

	public void AcceptMission(ServerPlayer a_player)
	{
		if (m_missionProposals.ContainsKey(a_player.m_pid))
		{
			if (!m_missions.ContainsKey(a_player.m_pid))
			{
				m_missions.Add(a_player.m_pid, new List<Mission>());
			}
			Mission mission = (Mission)m_missionProposals[a_player.m_pid];
			MissionNpc nearbyNpc = GetNearbyNpc(a_player.GetPosition());
			if (null != nearbyNpc && nearbyNpc.m_mission.IsEqual(mission))
			{
				nearbyNpc.AcceptMission();
			}
			mission.m_dieTime = Time.time + 3600f;
			((List<Mission>)m_missions[a_player.m_pid]).Add(mission);
			m_missionProposals.Remove(a_player.m_pid);
			UpdatePlayer(a_player);
		}
	}

	public bool SolveMission(ServerPlayer a_player, bool a_interactionOnly = false)
	{
		if (m_missions.ContainsKey(a_player.m_pid))
		{
			for (int i = 0; i < m_missionObjs.Length; i++)
			{
				Vector3 b = a_player.GetPosition() + a_player.GetForward() * 2f;
				float num = (!a_interactionOnly) ? 144f : 9f;
				if (!(null != m_missionObjs[i]) || !((m_missionObjs[i].gameObject.transform.position - b).sqrMagnitude < num))
				{
					continue;
				}
				List<Mission> list = (List<Mission>)m_missions[a_player.m_pid];
				for (int j = 0; j < list.Count; j++)
				{
					if ((!a_interactionOnly || list[j].m_type == eMissiontype.eRescue) && m_missionObjs[i].IsMission(list[j]))
					{
						a_player.AddXp(list[j].m_xpReward);
						list.RemoveAt(j);
						UpdatePlayer(a_player);
						m_server.SendSpecialEvent(a_player, eSpecialEvent.missionComplete);
						return true;
					}
				}
			}
		}
		return false;
	}

	public Mission GetRandomMission(int a_randomSeed, bool a_easyMode = false)
	{
		Random.seed = a_randomSeed;
		Mission mission = new Mission();
		mission.m_type = (eMissiontype)Random.Range(1, 4);
		mission.m_objPerson = (eObjectivesPerson)Random.Range(1, 9);
		mission.m_objObject = (eObjectivesObject)Random.Range(1, 5);
		mission.m_location = (eLocation)Random.Range(1, 15);
		if (a_easyMode)
		{
			for (int i = 0; i < 1; i++)
			{
				int num = Random.Range(1, 15);
				if (m_missionXpLocMultip[num] < m_missionXpLocMultip[(int)mission.m_location])
				{
					mission.m_location = (eLocation)num;
				}
			}
		}
		mission.m_xpReward = (int)(200.001f * m_missionXpLocMultip[(int)mission.m_location]);
		return mission;
	}
}
