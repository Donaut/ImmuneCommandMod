using UnityEngine;

public class JobPet : JobBase
{
	private float m_nextPickupTime;

	private LidServer m_server;

	private float m_resetAttackTime;

	private float m_nextFollowUpdate;

	private Vector3 m_targetItemPos;

	private ServerPlayer m_masterPlayer;

	private void Start()
	{
		Init();
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
	}

	public void SetAggressor(Transform a_aggressor)
	{
		if (m_masterPlayer == null || m_masterPlayer.GetTransform() != a_aggressor || Random.Range(0, 10) < 2)
		{
			m_body.Attack(a_aggressor, true);
			m_resetAttackTime = Time.time + Random.Range(3f, 10f);
		}
	}

	public override void Execute(float deltaTime)
	{
		if (m_body.GetState() == eBodyBaseState.none && null != m_server)
		{
			if (m_masterPlayer == null)
			{
				PickupNearestFood();
			}
			else
			{
				Vector3 position = m_masterPlayer.GetPosition();
				float sqrMagnitude = (base.transform.position - position).sqrMagnitude;
				if (Time.time > m_resetAttackTime)
				{
					Transform victim = m_masterPlayer.GetVictim();
					if (null != victim && (victim.position - position).sqrMagnitude < 200f && !m_server.IsInSpecialArea(victim.position, eAreaType.noPvp))
					{
						ControlledChar component = victim.GetComponent<ControlledChar>();
						if (null == component || (component.GetServerPlayer() != null && !component.GetServerPlayer().IsSaint()))
						{
							SetAggressor(m_masterPlayer.GetVictim());
						}
					}
					else if (sqrMagnitude > 900f)
					{
						m_masterPlayer = null;
					}
					else if (sqrMagnitude > 16f && Time.time > m_nextFollowUpdate)
					{
						m_body.GoTo(position);
						m_nextFollowUpdate = Time.time + 0.5f;
					}
				}
				else if (sqrMagnitude > 300f)
				{
					ResetAttack();
				}
			}
		}
		if (m_resetAttackTime > 0f && Time.time > m_resetAttackTime)
		{
			ResetAttack();
		}
	}

	private void ResetAttack()
	{
		m_resetAttackTime = 0f;
		m_body.Attack(null, false);
	}

	private void PickupNearestFood()
	{
		if (!(Time.time > m_nextPickupTime) && !((m_targetItemPos - base.transform.position).sqrMagnitude < 1.4f))
		{
			return;
		}
		m_nextPickupTime = Time.time + Random.Range(5f, 15f);
		DatabaseItem databaseItem = m_server.PickupItem(null, m_brain);
		Vector3 vector = Vector3.zero;
		if (databaseItem.dropPlayerId > 0)
		{
			ServerPlayer playerByPid = m_server.GetPlayerByPid(databaseItem.dropPlayerId);
			if (playerByPid != null && (playerByPid.GetPosition() - base.transform.position).sqrMagnitude < 64f)
			{
				m_masterPlayer = playerByPid;
				vector = m_masterPlayer.GetPosition();
			}
		}
		if (Vector3.zero == vector)
		{
			DatabaseItem nearestItem = m_server.GetNearestItem(base.transform.position, true);
			m_targetItemPos = new Vector3(nearestItem.x, 0f, nearestItem.y);
			vector = ((nearestItem.type != 0 && !((m_targetItemPos - base.transform.position).sqrMagnitude > 40000f)) ? m_targetItemPos : (base.transform.position + new Vector3(Random.Range(-100f, 100f), 0f, Random.Range(-100f, 100f))));
		}
		m_body.GoTo(vector);
	}
}
