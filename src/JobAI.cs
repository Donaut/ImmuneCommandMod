using UnityEngine;

public class JobAI : JobBase
{
	public bool m_attackOnSight = true;

	public float m_walkRadius = 5f;

	public float m_threatRadiusDay = 10f;

	public float m_threatRadiusNight = 10f;

	public float m_maxHuntRadius = 20f;

	private Transform m_enemy;

	private float m_threatRadius = 10f;

	private LidServer m_server;

	private float m_nextPlayerDistCheckTime;

	private ServerPlayer m_nearestPlayer;

	private float m_nextBoredActionTime;

	private Vector3 m_curPos = Vector3.zero;

	private Vector3 m_relocationPos = Vector3.zero;

	private void Start()
	{
		Init();
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
		m_curPos = m_body.m_homePos;
	}

	private void Update()
	{
		if (null != m_server)
		{
			m_threatRadius = ((m_server.GetDayLight() != 0f) ? m_threatRadiusDay : m_threatRadiusNight);
		}
		if (Vector3.zero != m_relocationPos && null == m_enemy)
		{
			if ((m_relocationPos - base.transform.position).sqrMagnitude < 9f)
			{
				m_body.m_homePos = m_relocationPos;
				m_relocationPos = Vector3.zero;
			}
			else
			{
				m_body.m_homePos = base.transform.position;
			}
		}
	}

	private void RunAway(Transform a_target)
	{
		if (m_brain.IsHappy() && null != a_target)
		{
			Vector3 normalized = (base.transform.position - a_target.position).normalized;
			m_body.GoTo(base.transform.position + normalized * m_threatRadius * 1.5f);
		}
	}

	private bool AttackOrRunAway(Transform a_target, Vector3 a_focusPos)
	{
		bool result = false;
		if (m_attackOnSight)
		{
			m_enemy = null;
			if (null != a_target)
			{
				float sqrMagnitude = (a_target.position - a_focusPos).sqrMagnitude;
				if (sqrMagnitude < m_threatRadius * m_threatRadius)
				{
					float sqrMagnitude2 = (a_target.position - m_body.m_homePos).sqrMagnitude;
					bool a_approach = sqrMagnitude2 < m_maxHuntRadius * m_maxHuntRadius;
					result = m_body.Attack(a_target, a_approach);
					m_enemy = a_target;
				}
				else
				{
					m_body.Attack(null, false);
					m_enemy = null;
				}
			}
		}
		else
		{
			float sqrMagnitude3 = (a_target.position - base.transform.position).sqrMagnitude;
			if (sqrMagnitude3 < m_threatRadius * m_threatRadius)
			{
				RunAway(a_target);
			}
		}
		return result;
	}

	public void SetAggressor(Transform a_aggressor)
	{
		if (m_brain.IsHappy())
		{
			AttackOrRunAway(a_aggressor, (!(null == a_aggressor)) ? a_aggressor.position : Vector3.zero);
			m_nextPlayerDistCheckTime = Time.time + 1f;
		}
	}

	public override void Execute(float deltaTime)
	{
		if (Time.time > m_nextBoredActionTime)
		{
			if (m_body.GetState() == eBodyBaseState.none && !m_body.IsMoving() && null == m_enemy)
			{
				if (Vector3.zero != m_relocationPos)
				{
					m_body.GoTo(m_relocationPos);
				}
				else
				{
					m_curPos = m_body.m_homePos + new Vector3(Random.Range(0f - m_walkRadius, m_walkRadius), 0f, Random.Range(0f - m_walkRadius, m_walkRadius));
					m_body.GoTo(m_curPos);
				}
			}
			m_nextBoredActionTime = Time.time + Random.Range(10f, 30f);
		}
		if (Time.time > m_nextPlayerDistCheckTime && null != m_server)
		{
			Vector3 vector = base.transform.position + base.transform.forward * (m_threatRadius * 0.5f);
			m_nearestPlayer = m_server.GetNearestPlayer(vector);
			if (m_nearestPlayer != null)
			{
				AttackOrRunAway(m_nearestPlayer.GetTransform(), vector);
			}
			m_nextPlayerDistCheckTime = Time.time + 0.5f + Random.Range(0f, 0.5f);
		}
	}

	public void RelocateHome(Vector3 a_pos)
	{
		m_relocationPos = a_pos;
		if (null == m_enemy && Vector3.zero != m_relocationPos)
		{
			m_body.GoTo(m_relocationPos);
		}
	}

	public bool IsRelocating()
	{
		return Vector3.zero != m_relocationPos && !m_brain.IsDead();
	}

	public Transform GetEnemy()
	{
		return m_enemy;
	}

	public ServerPlayer GetNearestPlayer()
	{
		return m_nearestPlayer;
	}
}
