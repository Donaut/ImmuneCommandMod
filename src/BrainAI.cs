using UnityEngine;

public class BrainAI : BrainBase
{
	public float m_respawnDuration = 180f;

	public bool m_spawnWhenVisible;

	public float m_hungerMultip;

	public float m_thirstMultip;

	public float m_fatigueMultip;

	public float m_lonelyMultip;

	public float m_injuryMultip;

	public float m_stressMultip;

	public float m_stateTolMin = 0.5f;

	public float m_stateTolMax = 0.9f;

	private float m_respawnTime = -1f;

	private LidServer m_server;

	private void Start()
	{
		Init();
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
		SetStateTolerance(Random.Range(m_stateTolMin, m_stateTolMax));
		SetStateDurability(eBrainBaseState.hungry, m_hungerMultip);
		SetStateDurability(eBrainBaseState.thirsty, m_thirstMultip);
		SetStateDurability(eBrainBaseState.fatigued, m_fatigueMultip);
		SetStateDurability(eBrainBaseState.lonely, m_lonelyMultip);
		SetStateDurability(eBrainBaseState.injured, m_injuryMultip);
		SetStateDurability(eBrainBaseState.stressed, m_stressMultip);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		UpdateState(deltaTime);
		if (IsDead() && m_respawnTime < 0f)
		{
			base.collider.enabled = false;
			m_respawnTime = Time.time + m_respawnDuration;
		}
		if (!(m_respawnTime > 0f) || !(Time.time > m_respawnTime) || !(null != m_server))
		{
			return;
		}
		if (m_spawnWhenVisible)
		{
			Respawn();
			return;
		}
		ServerPlayer nearestPlayer = m_server.GetNearestPlayer(base.transform.position);
		if (nearestPlayer == null || (base.transform.position - nearestPlayer.GetPosition()).sqrMagnitude > 1600f)
		{
			Respawn();
		}
		else
		{
			m_respawnTime = Time.time + 10f;
		}
	}

	private void Respawn()
	{
		base.collider.enabled = true;
		Reset();
		m_body.Respawn();
		m_respawnTime = -1f;
	}
}
