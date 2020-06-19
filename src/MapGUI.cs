using System.Collections.Generic;
using UnityEngine;

public class MapGUI : MonoBehaviour
{
	public Transform m_playerBlip;

	public Transform[] m_missionBlip;

	public float m_mapRadius = 1400f;

	public float m_miniMapRadius = 0.477f;

	private float m_nextBlipTime;

	private LidClient m_client;

	private void Start()
	{
		if (Global.isServer)
		{
			Object.Destroy(this);
		}
		else
		{
			m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		}
	}

	private void Update()
	{
		if (!(Time.time > m_nextBlipTime))
		{
			return;
		}
		if (null != m_client && m_client.enabled)
		{
			m_playerBlip.localPosition = WorldToMapPos(m_client.GetPos());
		}
		else if (Application.isEditor)
		{
			List<Mission> list = new List<Mission>(3);
			for (int i = 0; i < 3; i++)
			{
				Mission mission = new Mission();
				mission.m_location = (eLocation)Random.Range(1, 15);
				list.Add(mission);
			}
			UpdateMissions(list);
		}
		m_nextBlipTime = Time.time + 1f;
	}

	private Vector3 WorldToMapPos(Vector3 a_worldPos)
	{
		a_worldPos.x = a_worldPos.x / m_mapRadius * m_miniMapRadius;
		a_worldPos.y = a_worldPos.z / m_mapRadius * m_miniMapRadius;
		a_worldPos.z = -0.1f;
		return a_worldPos;
	}

	private Vector3 GetMissionPos(eLocation a_location)
	{
		Vector3 result = Vector3.zero;
		switch (a_location)
		{
		case eLocation.eHometown:
			result = new Vector3(-870f, 0f, 525f);
			break;
		case eLocation.eGastown:
			result = new Vector3(-1035f, 0f, 334f);
			break;
		case eLocation.eTerminus:
			result = new Vector3(-945f, 0f, 886f);
			break;
		case eLocation.eVenore:
			result = new Vector3(-435f, 0f, 628f);
			break;
		case eLocation.eFortBenning:
			result = new Vector3(-1035f, 0f, 45f);
			break;
		case eLocation.eGarbageStation:
			result = new Vector3(-635f, 0f, 1091f);
			break;
		case eLocation.eTallahassee:
			result = new Vector3(55f, 0f, 1095f);
			break;
		case eLocation.eRiverside:
			result = new Vector3(425f, 0f, 815f);
			break;
		case eLocation.eGasRanch:
			result = new Vector3(690f, 0f, 1147f);
			break;
		case eLocation.ePowerPlant:
			result = new Vector3(1130f, 0f, 1170f);
			break;
		case eLocation.eAirport:
			result = new Vector3(-335f, 0f, 1105f);
			break;
		case eLocation.eAlexandria:
			result = new Vector3(351f, 0f, 212f);
			break;
		case eLocation.eArea42:
			result = new Vector3(-1091f, 0f, -315f);
			break;
		case eLocation.eValley:
			result = new Vector3(-990f, 0f, -1033f);
			break;
		}
		return result;
	}

	public void UpdateMissions(List<Mission> a_missions)
	{
		int num = (a_missions != null) ? a_missions.Count : 0;
		for (int i = 0; i < m_missionBlip.Length; i++)
		{
			bool flag = i < num;
			m_missionBlip[i].renderer.enabled = flag;
			if (flag)
			{
				m_missionBlip[i].localPosition = WorldToMapPos(GetMissionPos(a_missions[i].m_location));
			}
		}
	}
}
