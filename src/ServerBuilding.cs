using UnityEngine;

public class ServerBuilding : MonoBehaviour
{
	public int m_type = 1;

	public int m_damageInSeconds = 100;

	protected bool m_isStatic = true;

	protected int m_ownerPid;

	protected float m_gotDamage;

	protected Transform m_gotAttacker;

	protected BuildingDef m_def = default(BuildingDef);

	protected LidServer m_server;

	protected double m_decayTime = 86400.0;

	protected SQLThreadManager m_sql;

	protected bool m_buildingIsDead;

	private DatabaseBuilding m_dbBuilding;

	private float m_nextSqlUpdateTime;

	public int GetOwnerId()
	{
		return m_ownerPid;
	}

	public virtual bool Use(ServerPlayer a_player)
	{
		return true;
	}

	public virtual bool Repair(ServerPlayer a_player)
	{
		m_decayTime = m_def.decayTime;
		if (null != m_sql)
		{
			m_dbBuilding.health = 100;
			m_dbBuilding.flag = eDbAction.update;
			m_sql.SaveBuilding(m_dbBuilding);
			m_server.SendSpecialEvent(a_player, eSpecialEvent.buildingRepaired);
			return true;
		}
		return false;
	}

	public virtual float GetState()
	{
		return (!(m_def.decayTime > 0.0)) ? 1f : Mathf.Clamp01((float)(m_decayTime / m_def.decayTime));
	}

	protected virtual void Awake()
	{
		Vector3 position = base.transform.position;
		short num = (short)(position.x * 10f);
		Vector3 position2 = base.transform.position;
		short num2 = (short)(position2.z * 10f);
		Vector3 position3 = new Vector3((float)num * 0.1f, 0f, (float)num2 * 0.1f);
		base.transform.position = position3;
		m_def = Buildings.GetBuildingDef(m_type);
		if (!Global.isServer && !m_isStatic)
		{
			Object.Destroy(this);
		}
	}

	public virtual void Init(LidServer a_server, int a_type, int a_ownerPid = 0, int a_health = 100, bool a_isNew = true)
	{
		m_isStatic = false;
		m_server = a_server;
		m_type = a_type;
		m_ownerPid = a_ownerPid;
		m_def = Buildings.GetBuildingDef(m_type);
		m_decayTime = m_def.decayTime * (double)((float)a_health / 100f);
		if (!m_def.persistent)
		{
			return;
		}
		Vector3 position = base.transform.position;
		float a_x = position.x * 1.00001f;
		Vector3 position2 = base.transform.position;
		float a_y = position2.z * 1.00001f;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		m_dbBuilding = new DatabaseBuilding(a_type, a_x, a_y, eulerAngles.y * 1.00001f, a_ownerPid, a_health);
		m_sql = m_server.GetSql();
		if (null != m_sql)
		{
			if (a_isNew)
			{
				m_dbBuilding.flag = eDbAction.insert;
				m_sql.SaveBuilding(m_dbBuilding);
			}
			m_nextSqlUpdateTime = Random.Range(100f, 300f);
		}
	}

	protected virtual void Update()
	{
		if (!m_isStatic && m_def.decayTime > 0.0)
		{
			if (m_gotDamage > 0f && null != m_gotAttacker)
			{
				m_decayTime -= m_gotDamage * (float)m_damageInSeconds;
				m_gotAttacker = null;
				m_gotDamage = 0f;
			}
			m_decayTime -= Time.deltaTime;
			if (m_decayTime <= 0.0)
			{
				m_buildingIsDead = true;
				RemoveFromSQL();
				Object.Destroy(base.gameObject);
			}
			else if (null != m_sql && Time.time > m_nextSqlUpdateTime)
			{
				m_dbBuilding.health = (int)(m_decayTime / m_def.decayTime * 100.0);
				m_dbBuilding.flag = eDbAction.update;
				m_sql.SaveBuilding(m_dbBuilding);
				m_nextSqlUpdateTime = Time.time + Random.Range(300f, 500f);
			}
		}
	}

	private void RemoveFromSQL()
	{
		if (null != m_sql)
		{
			m_dbBuilding.flag = eDbAction.delete;
			m_sql.SaveBuilding(m_dbBuilding);
		}
	}

	public void ChangeHealthBy(float a_dif)
	{
		m_gotDamage = 0f - a_dif;
	}

	public void SetAggressor(Transform a_aggressor)
	{
		m_gotAttacker = a_aggressor;
	}
}
