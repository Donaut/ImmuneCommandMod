using System.Collections.Generic;
using UnityEngine;

public class BodyAISimple : BodyBase
{
	private const float c_circleDist = 8f;

	public float attachDurMultip = 1.75f;

	public eCondition m_causeCondition = eCondition.none;

	public int m_handItemType = 60;

	public int m_bodyItemType;

	public int m_lookItemType;

	public float m_dropMultiplier = 1f;

	public bool m_activeOffscreen;

	private List<DropItem> m_itemDrops = new List<DropItem>();

	private int m_containerType;

	private NavMeshAgent m_agent;

	private LidServer m_server;

	private ServerNpc m_npc;

	private JobAI m_jobAi;

	private bool m_isHappy;

	private bool m_isDead;

	private ItemDef m_bodyItemDef;

	private ItemDef m_handItemDef;

	private Transform m_enemy;

	private float m_nextAttackTime;

	private float m_relaxTime;

	private float m_nextRepositionTime;

	private float m_stuckDuration;

	private bool m_approach;

	private float m_dontAttackTime;

	private void Start()
	{
		Init();
		m_handItemDef = Items.GetItemDef(m_handItemType);
		m_bodyItemDef = Items.GetItemDef(m_bodyItemType);
		try
		{
			m_jobAi = (JobAI)m_job;
		}
		catch
		{
		}
		m_agent = GetComponent<NavMeshAgent>();
		m_npc = GetComponent<ServerNpc>();
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
		if (!(null != m_npc))
		{
			return;
		}
		int num = 0;
		int a_chance = (int)(100f * m_dropMultiplier);
		if (m_npc.m_npcType == eCharType.eGasmaskGuy) // Type of enemy/mob/npc
		{
			m_containerType = 120; // container used for items dropped
			if (m_handItemType != 0)
			{
				m_itemDrops.Add(new DropItem(m_handItemType, m_handItemType, (int)(15f * m_dropMultiplier), 1, 50));
			}
			if (0 < m_handItemDef.ammoItemType)
			{
				m_itemDrops.Add(new DropItem(m_handItemDef.ammoItemType, m_handItemDef.ammoItemType, (int)(80f * m_dropMultiplier), 1, 10));
			}
			if (0 < m_bodyItemType)
			{
				m_itemDrops.Add(new DropItem(m_bodyItemType, m_bodyItemType, (int)(5f * m_dropMultiplier), 1, 60));
			}
			m_itemDrops.Add(new DropItem(1, 12, 67)); //Items <--- Really stupid setup imo, Should be using strings globally not numbered id's
			m_itemDrops.Add(new DropItem(15, 19, 67));
			m_itemDrops.Add(new DropItem(254, 254, 20, 1, 5));
			num = 12;
		}
		else if (m_npc.m_npcType == eCharType.eChicken || m_npc.m_npcType == eCharType.eRaven || m_npc.m_npcType == eCharType.eEagle) // else if for other types
		{
			m_itemDrops.Add(new DropItem(4, 4, a_chance));
			num = 1;
		}
		else if (m_npc.m_npcType == eCharType.eBull || m_npc.m_npcType == eCharType.eCow)
		{
			m_itemDrops.Add(new DropItem(4, 4, a_chance, 3, 4));
			m_itemDrops.Add(new DropItem(133, 133, a_chance, 3, 6));
			num = 2;
		}
		else if (m_npc.m_npcType == eCharType.eMutant || m_npc.m_npcType == eCharType.eSurvivorMutant)
		{
			m_itemDrops.Add(new DropItem(104, 104, 15, 10, 90)); // first id, second id, chance, min_quality, max_quality <--- Really stupid setup imo, Should be using strings globally not numbered id's
			m_itemDrops.Add(new DropItem(254, 254, 15, 1, 3));
			num = ((m_npc.m_npcType != eCharType.eMutant) ? 30 : 15);
		}
		else if (m_npc.m_npcType == eCharType.eSpider || m_npc.m_npcType == eCharType.eSpiderPoison)
		{
			m_itemDrops.Add(new DropItem(254, 254, 10, 1, 3));
			num = ((m_npc.m_npcType != eCharType.eSpider) ? 15 : 3);
		}
		else
		{
			m_itemDrops.Add(new DropItem(4, 4, a_chance, 1, 3));
			m_itemDrops.Add(new DropItem(133, 133, a_chance, 1, 3));
			num = 1;
		}
		if (0 < num)
		{
			int randomType = Items.GetRandomType(150f);
			int a_max = (!Items.HasCondition(randomType)) ? 1 : Random.Range(1, 30);
			m_itemDrops.Add(new DropItem(randomType, randomType, num, 1, a_max));
		}
	}

	private void Update()
	{
		ServerPlayer serverPlayer = (!(null != m_jobAi)) ? null : m_jobAi.GetNearestPlayer();
		bool flag = 0 < m_server.GetPlayerCount() && (m_activeOffscreen || (serverPlayer != null && (serverPlayer.GetPosition() - base.transform.position).sqrMagnitude < 2500f));
		if (flag)
		{
			float deltaTime = Time.deltaTime;
			bool flag2 = !m_isDead && m_brain.IsDead();
			m_isDead = m_brain.IsDead();
			m_state = eBodyBaseState.none;
			if (null != m_enemy && null != m_enemy.collider && !m_enemy.collider.enabled)
			{
				m_enemy = null;
			}
			if (!m_isDead)
			{
				bool flag3 = m_brain.IsHappy();
				if (flag3 != m_isHappy)
				{
					if (!flag3)
					{
						m_nextRepositionTime = 0f;
					}
					m_isHappy = flag3;
				}
				HandleEnemy();
				HandleStuckOnPath(deltaTime);
				if (m_state != eBodyBaseState.attacking && m_agent.enabled && m_agent.hasPath)
				{
					m_agent.Resume();
					m_state = (IsMoving() ? eBodyBaseState.running : eBodyBaseState.none);
				}
			}
			else if (flag2)
			{
				ResetTargets();
				DropLoot();
			}
		}
		if (flag != m_agent.enabled)
		{
			m_agent.enabled = flag;
		}
	}

	private void HandleEnemy()
	{
		if (!(null != m_enemy))
		{
			return;
		}
		if (m_isHappy && m_handItemDef.damage > 5f)
		{
			float a_sqrDist = 10000f;
			Vector3 vector = (!(m_dontAttackTime < Time.time)) ? Vector3.zero : GetAttackDir(ref a_sqrDist);
			if (Vector3.zero != vector)
			{
				base.transform.rotation = Quaternion.LookRotation(vector);
				Idle();
				if (Time.time > m_nextAttackTime)
				{
					Transform a_target = m_enemy;
					Raycaster.Attack(base.transform, m_handItemDef, m_enemy.transform.position, ref a_target);
					if (m_handItemDef.ammoItemType > 0 && null != a_target && a_target != m_enemy)
					{
						m_dontAttackTime = Time.time + Random.Range(3f, 6f);
					}
					if (a_target == m_enemy && m_causeCondition != eCondition.none)
					{
						ControlledChar component = m_enemy.GetComponent<ControlledChar>();
						ServerPlayer serverPlayer = (!(null != component)) ? null : component.GetServerPlayer();
						if (serverPlayer != null)
						{
							serverPlayer.SetCondition(m_causeCondition, true);
						}
					}
					m_nextAttackTime = Time.time + m_handItemDef.attackdur * attachDurMultip;
				}
				m_state = eBodyBaseState.attacking;
			}
			else if (m_approach)
			{
				if (Time.time > m_nextRepositionTime)
				{
					GoTo(m_enemy.position);
					m_nextRepositionTime = Time.time + 0.05f + Mathf.Clamp01(a_sqrDist / 200f);
				}
			}
			else
			{
				m_enemy = null;
				GoTo(m_homePos);
			}
		}
		else if (Time.time > m_nextRepositionTime)
		{
			Vector3 normalized = (base.transform.position - m_enemy.position).normalized;
			Vector3 vector2 = base.transform.position + normalized * 10f;
			if (0.8f > Util.GetTerrainHeight(vector2) || Random.Range(0, 5) == 0)
			{
				float y = (Random.Range(0, 2) != 0) ? 90f : (-90f);
				vector2 = base.transform.position + Quaternion.Euler(0f, y, 0f) * normalized * 10f;
			}
			GoTo(vector2);
			m_nextRepositionTime = Time.time + 1f;
		}
	}

	private void HandleStuckOnPath(float a_deltaTime)
	{
		if (m_agent.enabled && m_agent.hasPath && !IsMoving())
		{
			m_stuckDuration += a_deltaTime;
			if (m_stuckDuration > 2f)
			{
				float d = (Random.Range(0, 2) != 0) ? (-1f) : 1f;
				Vector3 b = base.transform.right * Random.Range(4f, 12f) * d + base.transform.forward * Random.Range(-2f, 0f);
				GoTo(base.transform.position + b);
				m_stuckDuration = 0f;
				m_nextRepositionTime = Time.time + b.magnitude / m_agent.speed;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(base.transform.position + Vector3.up * 0.5f, 0.5f);
	}

	public float GetVestMultiplier()
	{
		return (!Items.IsBody(m_bodyItemType)) ? 1f : m_bodyItemDef.healing;
	}

	public override bool GoTo(Vector3 a_target)
	{
		bool result = false;
		if (m_agent.enabled && (base.transform.position - a_target).sqrMagnitude > 1f)
		{
			result = m_agent.SetDestination(a_target);
		}
		return result;
	}

	public override bool IsMoving()
	{
		return m_agent.velocity.sqrMagnitude > 0.01f;
	}

	public override void Idle()
	{
		if (m_agent.enabled)
		{
			m_agent.Stop();
		}
	}

	public override bool Respawn()
	{
		ResetTargets();
		m_enemy = null;
		if (m_agent.enabled)
		{
			m_agent.Warp(m_homePos);
		}
		return true;
	}

	public override bool Attack(Transform a_victim, bool a_approach)
	{
		m_relaxTime = Time.time + 20f;
		m_approach = a_approach;
		if (a_victim != m_enemy && m_handItemDef.damage > 0f)
		{
			m_nextAttackTime = Time.time + 1f;
		}
		m_enemy = a_victim;
		return m_nextAttackTime > Time.time;
	}

	public override bool FindFood(float status, float deltaTime)
	{
		return IsRelaxed();
	}

	public override bool FindDrink(float status, float deltaTime)
	{
		return IsRelaxed();
	}

	public override bool FindSleep(float status, float deltaTime)
	{
		return IsRelaxed();
	}

	public override bool FindHealing(float status, float deltaTime)
	{
		return false;
	}

	public override bool FindCatharsis(float status, float deltaTime)
	{
		return IsRelaxed();
	}

	public override bool FindMates(float status, float deltaTime)
	{
		return IsRelaxed();
	}

	private bool IsRelaxed()
	{
		return Time.time > m_relaxTime;
	}

	private void DropLoot()
	{
		if (!(null != m_server) || m_itemDrops == null || m_itemDrops.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < m_itemDrops.Count; i++)
		{
			if (m_itemDrops[i] != null && Random.Range(0, 100) < m_itemDrops[i].chance)
			{
				int num = (m_itemDrops[i].typeFrom != m_itemDrops[i].typeTo) ? Random.Range(m_itemDrops[i].typeFrom, m_itemDrops[i].typeTo + 1) : m_itemDrops[i].typeFrom;
				int num2 = (!Items.HasAmountOrCondition(num)) ? 1 : Random.Range(m_itemDrops[i].min, m_itemDrops[i].max + 1);
				if (num2 < 1)
				{
					num2 = 1;
				}
				if (m_containerType != 0)
				{
					m_server.CreateTempContainerItem(num, num2, base.transform.position, m_containerType);
					continue;
				}
				int num3 = 4;
				Vector3 b = new Vector3((float)Random.Range(-num3, num3) * 0.1f, 0f, (float)Random.Range(-num3, num3) * 0.1f);
				m_server.CreateFreeWorldItem(num, num2, base.transform.position + b);
			}
		}
	}

	private void ResetTargets()
	{
		if (m_agent.enabled && m_agent.hasPath)
		{
			m_agent.ResetPath();
		}
	}

	private Vector3 GetAttackDir(ref float a_sqrDist)
	{
		Vector3 result = Vector3.zero;
		if (null != m_enemy)
		{
			result = m_enemy.position - base.transform.position;
			a_sqrDist = result.sqrMagnitude;
			float num = Mathf.Clamp(m_handItemDef.range * m_handItemDef.range * 0.6f, 2f, 10000f);
			if (a_sqrDist > num)
			{
				result = Vector3.zero;
			}
		}
		return result;
	}
}
