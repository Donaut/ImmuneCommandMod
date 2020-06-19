using UnityEngine;

public class ServerNpc : MonoBehaviour
{
	public eCharType m_npcType = eCharType.eGasmaskGuy;

	public float m_damageMultiplier = 1f;

	private BrainBase m_brain;

	private BodyAISimple m_body;

	private float m_lastHealth = 100f;

	private NavMeshAgent m_agent;

	private void Start()
	{
		m_brain = GetComponent<BrainBase>();
		m_body = GetComponent<BodyAISimple>();
		m_agent = GetComponent<NavMeshAgent>();
	}

	public int GetHandItem()
	{
		return (null != m_body) ? m_body.m_handItemType : 0;
	}

	public int GetBodyItem()
	{
		return (null != m_body) ? m_body.m_bodyItemType : 0;
	}

	public int GetLookItem()
	{
		return (null != m_body) ? m_body.m_lookItemType : 0;
	}

	public eBodyBaseState GetBodyState()
	{
		return m_body.GetState();
	}

	public float GetHealth()
	{
		return (!m_brain.IsDead()) ? ((1f - m_brain.GetState(eBrainBaseState.injured)) * 100f) : 0f;
	}

	public float GetLastHealth()
	{
		return m_lastHealth;
	}

	public float ChangeHealthBy(float a_delta)
	{
		m_lastHealth = GetHealth();
		if (0f > a_delta)
		{
			a_delta *= m_damageMultiplier * m_body.GetVestMultiplier();
		}
		m_brain.ChangeStateBy(eBrainBaseState.injured, a_delta * -0.01f);
		return GetHealth();
	}

	private void OnCollisionEnter(Collision a_col)
	{
		float num = 0.5f * Mathf.Clamp(a_col.relativeVelocity.sqrMagnitude - 10f, 0f, 10000f) * m_damageMultiplier;
		if (num > 1f)
		{
			ChangeHealthBy(0f - num);
		}
		if (0f < GetHealth() && null != m_agent)
		{
			Vector3 offset = Vector3.zero;
			offset = base.transform.position - a_col.collider.transform.position;
			offset.y = 0f;
			offset = offset.normalized * (1f + num * 0.02f);
			m_agent.Move(offset);
		}
	}
}
