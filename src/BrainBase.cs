using UnityEngine;

public class BrainBase : MonoBehaviour
{
	private eBrainBaseState m_state = eBrainBaseState.happy;

	private float m_hunger;

	private float m_thirst;

	private float m_fatigue;

	private float m_injury;

	private float m_stress;

	private float m_loneliness;

	private float m_stateTolerance = 0.5f;

	private float m_hungerDurability = 7200f;

	private float m_thirstDurability = 7200f;

	private float m_fatigueDurability = 3600f;

	private float m_injuryDurability = 3600f;

	private float m_stressDurability = 3600f;

	private float m_lonelinessDurability = 3600f;

	protected BodyBase m_body;

	protected JobBase m_job;

	protected void Init()
	{
		m_body = GetComponent<BodyBase>();
		m_job = GetComponent<JobBase>();
	}

	protected eBrainBaseState UpdateState(float deltaTime)
	{
		if (!IsDead())
		{
			if (m_hungerDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.hungry, 0.45f / m_hungerDurability * deltaTime);
			}
			if (m_thirstDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.thirsty, 0.45f / m_thirstDurability * deltaTime);
			}
			if (m_fatigueDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.fatigued, 1f / m_fatigueDurability * deltaTime);
			}
			if (m_lonelinessDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.lonely, 1f / m_lonelinessDurability * deltaTime);
			}
			if (m_stressDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.stressed, -1f / m_stressDurability * deltaTime);
			}
			if (m_injuryDurability != 0f)
			{
				ChangeStateBy(eBrainBaseState.injured, -1f / m_injuryDurability * deltaTime);
			}
			eBrainBaseState state = m_state;
			state = ((m_fatigue > m_stateTolerance) ? eBrainBaseState.fatigued : ((m_thirst > m_stateTolerance) ? eBrainBaseState.thirsty : ((m_hunger > m_stateTolerance) ? eBrainBaseState.hungry : ((m_injury > m_stateTolerance) ? eBrainBaseState.injured : ((m_stress > m_stateTolerance) ? eBrainBaseState.stressed : ((!(m_loneliness > m_stateTolerance)) ? eBrainBaseState.happy : eBrainBaseState.lonely))))));
			if (state != m_state)
			{
				if (Application.isEditor)
				{
					Debug.Log(string.Concat(" BrainBase.cs (", base.gameObject.name, "): state has changed from ", m_state, " to ", state, " isDead: ", IsDead()));
				}
				m_state = state;
			}
			HandleState(deltaTime);
		}
		return m_state;
	}

	public void Reset()
	{
		m_hunger = 0f;
		m_thirst = 0f;
		m_fatigue = 0f;
		m_injury = 0f;
		m_stress = 0f;
		m_loneliness = 0f;
	}

	public bool IsHappy()
	{
		return eBrainBaseState.happy == m_state;
	}

	public eBrainBaseState GetState()
	{
		return m_state;
	}

	public bool IsDead()
	{
		return 0.99f < m_hunger || 0.99f < m_thirst || 0.99f < m_fatigue || 0.99f < m_injury || 0.99f < m_stress || 0.99f < m_loneliness;
	}

	public void SetStateTolerance(float v)
	{
		m_stateTolerance = Mathf.Clamp(v, 0f, 1f);
	}

	public float GetState(eBrainBaseState state)
	{
		float result = 0f;
		switch (state)
		{
		case eBrainBaseState.hungry:
			result = m_hunger;
			break;
		case eBrainBaseState.thirsty:
			result = m_thirst;
			break;
		case eBrainBaseState.fatigued:
			result = m_fatigue;
			break;
		case eBrainBaseState.injured:
			result = m_injury;
			break;
		case eBrainBaseState.stressed:
			result = m_stress;
			break;
		case eBrainBaseState.lonely:
			result = m_loneliness;
			break;
		}
		return result;
	}

	public float ChangeStateBy(eBrainBaseState state, float delta)
	{
		float result = -1f;
		switch (state)
		{
		case eBrainBaseState.hungry:
			m_hunger = Mathf.Clamp(m_hunger + delta, 0f, 1f);
			result = m_hunger;
			break;
		case eBrainBaseState.thirsty:
			m_thirst = Mathf.Clamp(m_thirst + delta, 0f, 1f);
			result = m_thirst;
			break;
		case eBrainBaseState.fatigued:
			m_fatigue = Mathf.Clamp(m_fatigue + delta, 0f, 1f);
			result = m_fatigue;
			break;
		case eBrainBaseState.injured:
			m_injury = Mathf.Clamp(m_injury + delta, 0f, 1f);
			result = m_injury;
			break;
		case eBrainBaseState.stressed:
			m_stress = Mathf.Clamp(m_stress + delta, 0f, 1f);
			result = m_stress;
			break;
		case eBrainBaseState.lonely:
			m_loneliness = Mathf.Clamp(m_loneliness + delta, 0f, 1f);
			result = m_loneliness;
			break;
		}
		return result;
	}

	public void SetStateDurability(eBrainBaseState state, float durInSec)
	{
		switch (state)
		{
		case eBrainBaseState.hungry:
			m_hungerDurability = durInSec;
			break;
		case eBrainBaseState.thirsty:
			m_thirstDurability = durInSec;
			break;
		case eBrainBaseState.fatigued:
			m_fatigueDurability = durInSec;
			break;
		case eBrainBaseState.injured:
			m_injuryDurability = durInSec;
			break;
		case eBrainBaseState.stressed:
			m_stressDurability = durInSec;
			break;
		case eBrainBaseState.lonely:
			m_lonelinessDurability = durInSec;
			break;
		}
	}

	private void HandleState(float deltaTime)
	{
		switch (m_state)
		{
		case eBrainBaseState.hungry:
			if (m_body.FindFood(m_hunger, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.hungry, -1f);
			}
			break;
		case eBrainBaseState.thirsty:
			if (m_body.FindDrink(m_thirst, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.thirsty, -1f);
			}
			break;
		case eBrainBaseState.fatigued:
			if (m_body.FindSleep(m_fatigue, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.fatigued, -1f);
			}
			break;
		case eBrainBaseState.injured:
			if (m_body.FindHealing(m_injury, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.injured, -1f);
			}
			break;
		case eBrainBaseState.stressed:
			if (m_body.FindCatharsis(m_stress, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.stressed, -1f);
			}
			break;
		case eBrainBaseState.lonely:
			if (m_body.FindMates(m_loneliness, deltaTime))
			{
				ChangeStateBy(eBrainBaseState.lonely, -1f);
			}
			break;
		case eBrainBaseState.happy:
			m_job.Execute(deltaTime);
			break;
		}
	}
}
