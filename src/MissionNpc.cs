using System;
using UnityEngine;

public class MissionNpc : MonoBehaviour
{
	public bool m_easyMode;

	public Mission m_mission = new Mission();

	private float m_nextMissionChangeTime;

	private MissionManager m_manager;

	private void Start()
	{
		if (Global.isServer)
		{
			m_manager = (MissionManager)UnityEngine.Object.FindObjectOfType(typeof(MissionManager));
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (Time.time > m_nextMissionChangeTime && m_nextMissionChangeTime != -1f)
		{
			int num = DateTime.Now.Second + DateTime.Now.Minute * 60;
			MissionManager manager = m_manager;
			Vector3 position = base.transform.position;
			float num2 = 10f * position.x;
			Vector3 position2 = base.transform.position;
			m_mission = manager.GetRandomMission((int)(num2 + 1000f * position2.z) + num, m_easyMode);
			m_nextMissionChangeTime = -1f;
		}
	}

	public void AcceptMission()
	{
		m_nextMissionChangeTime = Time.time + 30f;
	}
}
