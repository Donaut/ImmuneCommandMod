using UnityEngine;

public class RemoteBuilding : MonoBehaviour
{
	private const int c_tntBuildingType = 102;

	[HideInInspector]
	public int m_type;

	[HideInInspector]
	public bool m_isMine;

	[HideInInspector]
	public bool m_isStatic;

	[HideInInspector]
	public float m_health = 100f;

	public AudioClip m_squeelSound;

	public AudioClip m_buildSound;

	public GameObject m_explosion;

	private float m_explosionTimer;

	private Quaternion m_targetRot = Quaternion.identity;

	private bool m_visible;

	private float m_lastUpdate;

	private float m_disappearTime = 0.5f;

	private float m_dieTime = 10f;

	private Transform m_animation;

	private Collider m_collider;

	public void Refresh(float a_yRot, float a_health)
	{
		if (m_explosionTimer == -1f)
		{
			return;
		}
		if (!m_visible)
		{
			SwitchVisibility();
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			if (eulerAngles.y != a_yRot)
			{
				base.transform.rotation = Quaternion.Euler(0f, a_yRot, 0f);
			}
		}
		Quaternion targetRot = m_targetRot;
		m_targetRot = Quaternion.Euler(0f, a_yRot, 0f);
		if (base.transform.rotation != m_targetRot && base.transform.rotation == targetRot && null != base.audio)
		{
			base.audio.Stop();
			base.audio.clip = m_squeelSound;
			base.audio.Play();
		}
		m_health = a_health;
		if (null != m_animation)
		{
			bool flag = m_health < 50f;
			m_animation.gameObject.SetActive(flag);
			if (null != m_collider)
			{
				m_collider.enabled = !flag;
			}
		}
		m_lastUpdate = Time.time;
	}

	public void Init(Vector3 a_pos, int a_type, bool a_isMine, bool a_isStatic)
	{
		base.transform.position = a_pos;
		m_type = a_type;
		m_isMine = a_isMine;
		m_isStatic = a_isStatic;
		if (!a_isStatic)
		{
			GameObject gameObject = (GameObject)Resources.Load("buildings/building_" + a_type);
			GameObject gameObject2 = null;
			if (null != gameObject)
			{
				gameObject2 = (GameObject)Object.Instantiate(gameObject, base.transform.position, Quaternion.identity);
				gameObject2.transform.parent = base.transform;
				m_animation = gameObject2.transform.Find("Animation");
				ServerBuilding componentInChildren = GetComponentInChildren<ServerBuilding>();
				if (null != componentInChildren)
				{
					Object.Destroy(componentInChildren);
				}
			}
			else
			{
				gameObject2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				gameObject2.transform.position = base.transform.position;
				gameObject2.transform.localScale = Vector3.one * 0.5f;
				gameObject2.transform.parent = base.transform;
			}
			m_lastUpdate = Time.time;
		}
		else
		{
			m_animation = base.transform.Find("Animation");
		}
		m_collider = base.collider;
		if (null == m_collider)
		{
			m_collider = GetComponentInChildren<Collider>();
		}
		if (null != base.audio && Time.timeSinceLevelLoad > 5f)
		{
			base.audio.clip = m_buildSound;
			base.audio.Play();
		}
		if (m_type == 102)
		{
			float time = Time.time;
			BuildingDef buildingDef = Buildings.GetBuildingDef(102);
			m_explosionTimer = time + ((float)buildingDef.decayTime - 0.3f);
		}
	}

	private void FixedUpdate()
	{
		if (m_visible)
		{
			float t = Time.fixedDeltaTime * 5f;
			if (m_targetRot != base.transform.rotation)
			{
				base.transform.rotation = Quaternion.Lerp(base.transform.rotation, m_targetRot, t);
			}
			if (!m_isStatic && m_lastUpdate + m_disappearTime < Time.time)
			{
				SwitchVisibility();
			}
		}
		else if (!m_isStatic && m_lastUpdate + m_dieTime < Time.time)
		{
			Object.Destroy(base.gameObject);
		}
		if (Time.time > m_explosionTimer && m_explosionTimer > 0f)
		{
			Vector3 position = base.transform.position + Vector3.up * 0.5f + (Camera.main.transform.position - base.transform.position) * 0.25f;
			GameObject gameObject = (GameObject)Object.Instantiate(m_explosion, position, Quaternion.identity);
			gameObject.transform.parent = Camera.main.transform;
			SwitchVisibility();
			m_explosionTimer = -1f;
		}
	}

	public bool IsVisible()
	{
		return m_visible;
	}

	public bool IsExploding()
	{
		return Mathf.Abs(Time.time - m_explosionTimer) < 0.5f;
	}

	public void SwitchVisibility()
	{
		m_visible = !m_visible;
		if (null != m_collider)
		{
			m_collider.enabled = m_visible;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = m_visible;
		}
	}
}
