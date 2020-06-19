using UnityEngine;

public class MissionObjective : MonoBehaviour
{
	public eMissiontype m_type;

	public eLocation m_location;

	public eObjectivesObject m_objObject;

	public eObjectivesPerson m_objPerson;

	public GameObject[] m_charPrefabs;

	public GameObject[] m_ragdollPrefabs;

	public GameObject[] m_runawayPrefabs;

	public GameObject m_explosionPrefab;

	public GameObject m_exlMarkPrefab;

	private GameObject m_char;

	private float m_hp;

	private bool m_on;

	private LidClient m_client;

	private Transform m_objectsOn;

	private Transform m_objectsOff;

	private void Start()
	{
		m_client = Object.FindObjectOfType<LidClient>();
		m_objectsOn = base.transform.Find("active");
		m_objectsOff = base.transform.Find("inactive");
		if (null != m_exlMarkPrefab)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_exlMarkPrefab, base.transform.position + Vector3.up * 4f, Quaternion.Euler(270f, 0f, 0f));
			gameObject.transform.parent = m_objectsOn;
		}
		UpdateGameObjects();
	}

	private void Init()
	{
		if (m_type == eMissiontype.eKill || m_type == eMissiontype.eRescue)
		{
			m_hp = 1f;
		}
		else if (m_type == eMissiontype.eDestroy)
		{
			m_hp = 100f;
		}
		else
		{
			m_hp = 0f;
		}
	}

	private void UpdateGameObjects()
	{
		if (null != m_objectsOn)
		{
			m_objectsOn.gameObject.SetActive(m_on);
		}
		if (null != m_objectsOff)
		{
			m_objectsOff.gameObject.SetActive(!m_on);
		}
		if (null != base.collider)
		{
			base.collider.enabled = (m_type != eMissiontype.eKill || m_on);
		}
		base.gameObject.layer = (m_on ? 22 : 0);
	}

	private void SolveMission()
	{
		if (m_on && null != m_client && !Global.isServer)
		{
			m_client.SendSpecialRequest(eSpecialRequest.solveMission);
			SetOnOff(false);
		}
	}

	public void SetOnOff(bool a_on)
	{
		if (m_on == a_on)
		{
			return;
		}
		m_on = a_on;
		if (m_on)
		{
			Init();
		}
		if (m_type == eMissiontype.eKill || m_type == eMissiontype.eRescue)
		{
			int num = (int)m_objPerson % 2;
			GameObject gameObject = (m_type != eMissiontype.eKill) ? m_runawayPrefabs[num] : m_ragdollPrefabs[num];
			if (null != m_char)
			{
				Object.Destroy(m_char);
			}
			m_char = (GameObject)Object.Instantiate((!m_on) ? gameObject : m_charPrefabs[num], base.transform.position, base.transform.rotation);
			if (m_on)
			{
				Renderer[] componentsInChildren = m_char.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					if (null != componentsInChildren[i])
					{
						componentsInChildren[i].gameObject.layer = 22;
					}
				}
			}
			BodyHeadAnim bodyHeadAnim = (BodyHeadAnim)m_char.GetComponent(typeof(BodyHeadAnim));
			if (null != bodyHeadAnim)
			{
				bodyHeadAnim.ChangeHeadItem((int)m_objPerson);
			}
		}
		else if (m_type == eMissiontype.eDestroy && !m_on)
		{
			Vector3 position = base.transform.position + Vector3.up * 0.5f + (Camera.main.transform.position - base.transform.position) * 0.25f;
			GameObject gameObject2 = (GameObject)Object.Instantiate(m_explosionPrefab, position, Quaternion.identity);
			gameObject2.transform.parent = Camera.main.transform;
		}
		UpdateGameObjects();
	}

	public bool IsMission(Mission a_mission)
	{
		return a_mission.m_type == m_type && a_mission.m_location == m_location && (m_type != eMissiontype.eDestroy || a_mission.m_objObject == m_objObject);
	}

	private void SetAggressor(Transform a_aggressor)
	{
		if (m_on && null != m_client && !Global.isServer)
		{
			ItemDef itemDef = Items.GetItemDef(m_client.GetHandItem());
			m_hp -= itemDef.damage;
			if (m_hp < 0f)
			{
				SolveMission();
			}
		}
	}
}
