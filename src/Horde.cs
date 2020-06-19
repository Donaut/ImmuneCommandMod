using UnityEngine;

public class Horde : MonoBehaviour
{
	public JobAI[] m_actors;

	public Transform[] m_waypoints;

	private int m_curPoint;

	private float m_nextUpdate;

	private float rndRadius = 1f;

	private void Start()
	{
		rndRadius = m_actors.Length;
		for (int i = 0; i < m_actors.Length; i++)
		{
			BodyAISimple component = m_actors[i].gameObject.GetComponent<BodyAISimple>();
			if (null != component)
			{
				component.m_activeOffscreen = true;
			}
		}
	}

	private void Update()
	{
		if (!(Time.time > m_nextUpdate))
		{
			return;
		}
		if (DidActorsArrive())
		{
			m_curPoint++;
			if (m_curPoint == m_waypoints.Length)
			{
				m_curPoint = 0;
			}
			RelocateActors(m_waypoints[m_curPoint].position);
		}
		AttackAsTeam();
		m_nextUpdate = Time.time + Random.Range(1f, 2f);
	}

	private bool DidActorsArrive()
	{
		for (int i = 0; i < m_actors.Length; i++)
		{
			if (null != m_actors[i] && m_actors[i].IsRelocating())
			{
				return false;
			}
		}
		return true;
	}

	private void RelocateActors(Vector3 a_pos)
	{
		for (int i = 0; i < m_actors.Length; i++)
		{
			if (null != m_actors[i])
			{
				Vector3 a_pos2 = a_pos + new Vector3(Random.Range(0f - rndRadius, rndRadius), 0f, Random.Range(0f - rndRadius, rndRadius));
				m_actors[i].RelocateHome(a_pos2);
			}
		}
	}

	private void AttackAsTeam()
	{
		Transform transform = null;
		for (int i = 0; i < m_actors.Length; i++)
		{
			if (null != m_actors[i] && null != m_actors[i].GetEnemy())
			{
				transform = m_actors[i].GetEnemy();
				break;
			}
		}
		if (!(null != transform))
		{
			return;
		}
		for (int j = 0; j < m_actors.Length; j++)
		{
			if (null != m_actors[j] && null == m_actors[j].GetEnemy())
			{
				m_actors[j].SetAggressor(transform);
			}
		}
	}
}
