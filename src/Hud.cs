using System.Collections.Generic;
using UnityEngine;

public class Hud : MonoBehaviour
{
	public Transform m_barHealth;

	public Transform m_barEnergy;

	public GameObject m_inventory;

	public QmunicatorGUI m_comGui;

	public int m_debugCondition;

	public GameObject[] m_conditions;

	public Transform[] m_missions;

	private Vector3 m_startScale = Vector3.zero;

	private LidClient m_client;

	private bool m_active = true;

	private int m_condition = -1;

	private void Start()
	{
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_startScale = m_barHealth.localScale;
		UpdateMissions(null);
	}

	private void Update()
	{
		if (null != m_client)
		{
			m_barHealth.localScale = new Vector3(m_client.GetHealth() * 0.01f * m_startScale.x, m_startScale.y, m_startScale.z);
			m_barEnergy.localScale = new Vector3(m_client.GetEnergy() * 0.01f * m_startScale.x, m_startScale.y, m_startScale.z);
		}
		bool flag = !m_inventory.activeSelf && !m_comGui.IsActive();
		if (flag != m_active)
		{
			m_active = flag;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (null != child)
				{
					child.gameObject.SetActive(m_active);
				}
			}
		}
		int num = (!(null != m_client)) ? m_debugCondition : m_client.GetCondition();
		if (m_condition != num)
		{
			m_condition = num;
			UpdateConditions();
		}
	}

	private void UpdateConditions()
	{
		float num = -0.4f;
		float num2 = 0.15f;
		for (int i = 0; i < m_conditions.Length; i++)
		{
			bool flag = 0 < (m_condition & (1 << i));
			m_conditions[i].SetActive(flag);
			if (flag)
			{
				Vector3 localPosition = m_conditions[i].transform.localPosition;
				localPosition.y = num;
				m_conditions[i].transform.localPosition = localPosition;
				num -= num2;
			}
		}
	}

	public void UpdateMissions(List<Mission> a_missions)
	{
		int num = (a_missions != null) ? a_missions.Count : 0;
		for (int i = 0; i < m_missions.Length; i++)
		{
			bool flag = i < num;
			m_missions[i].parent.gameObject.SetActive(flag);
			if (flag)
			{
				m_missions[i].renderer.material.mainTextureOffset = new Vector2(0.125f * ((float)a_missions[i].m_type - 1f), 0f);
			}
		}
	}
}
