using UnityEngine;

public class ServerVehicle : MonoBehaviour
{
	private const int c_allLayersExceptVehicle = 268433407;

	private const int c_allLayers = 268435455;

	public int m_id = -1;

	public CarData m_data;

	public float m_crashDamageMultip = 3f;

	public float m_armorPercent = 0.8f;

	private int m_passengerCount;

	private float m_health = 100f;

	private SimpleVehicle m_vehicle;

	private float m_nextWaterDamageTime;

	private float m_nextCollisionTime;

	private float m_respawnTime = -1f;

	private Vector3 m_spawnPoint;

	private Vector3 m_lastPos;

	private float m_lastSpeed;

	private LidServer m_server;

	private void Awake()
	{
		m_data.passengerIds = new int[4];
		m_vehicle = GetComponent<SimpleVehicle>();
		m_server = Object.FindObjectOfType<LidServer>();
		m_spawnPoint = (m_lastPos = base.transform.position);
		KillAndResetPassengers();
	}

	private void Update()
	{
		if (m_respawnTime > 0f && Time.time > m_respawnTime)
		{
			base.transform.position = m_spawnPoint;
			ChangeHealthBy(10000f);
			base.collider.enabled = true;
			m_respawnTime = -1f;
		}
	}

	private void FixedUpdate()
	{
		float sqrMagnitude = (m_lastPos - base.transform.position).sqrMagnitude;
		bool flag = sqrMagnitude > 0f;
		m_lastPos = base.transform.position;
		if (flag && 0.8f > Util.GetTerrainHeight(base.transform.position + base.transform.forward))
		{
			base.rigidbody.AddForce(-base.rigidbody.velocity, ForceMode.VelocityChange);
			if (Time.time > m_nextWaterDamageTime)
			{
				ChangeHealthBy(-50f);
				m_nextWaterDamageTime = Time.time + 1f;
			}
		}
		if (m_health < 20f)
		{
			ChangeHealthBy(0f - Time.fixedDeltaTime);
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		base.transform.position = position;
	}

	private void KillAndResetPassengers()
	{
		for (int i = 0; i < 4; i++)
		{
			if (m_data.passengerIds[i] == -1)
			{
				continue;
			}
			if (null != m_server)
			{
				ServerPlayer playerByOnlineid = m_server.GetPlayerByOnlineid(m_data.passengerIds[i]);
				if (playerByOnlineid != null)
				{
					playerByOnlineid.ChangeHealthBy(-10000f);
				}
			}
			if (i == 0)
			{
				m_vehicle.AssignInput(0f, 0f, false);
			}
			m_data.passengerIds[i] = -1;
		}
	}

	private bool ChangePassenger(int a_getId, int a_setId)
	{
		if (a_getId == -1 && a_setId == -1)
		{
			Debug.Log("ServerVehicle.cs: Error: ChangePassenger( -1, -1 ) is not valid");
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			if (a_getId == m_data.passengerIds[i])
			{
				if (i == 0 && a_setId == -1)
				{
					m_vehicle.AssignInput(0f, 0f, false);
				}
				m_data.passengerIds[i] = a_setId;
				return true;
			}
		}
		return false;
	}

	private void OnCollisionEnter(Collision a_col)
	{
		if (Time.time > m_nextCollisionTime)
		{
			float num = 0.5f * Mathf.Clamp(a_col.relativeVelocity.sqrMagnitude - 10f, 0f, 100000f);
			if (num > 1f)
			{
				ChangeHealthBy(0f - num);
			}
			m_nextCollisionTime = Time.time + 0.3f;
		}
	}

	private void CausedDamage(float a_dmg)
	{
		if (null != m_server && m_data.passengerIds[0] != -1)
		{
			ServerPlayer playerByOnlineid = m_server.GetPlayerByOnlineid(m_data.passengerIds[0]);
			if (playerByOnlineid != null)
			{
				playerByOnlineid.ChangeKarmaBy(Mathf.Clamp(a_dmg, 0f, 100f) * -0.5f);
			}
		}
	}

	public float ChangeHealthBy(float a_delta)
	{
		if (a_delta < 0f)
		{
			a_delta *= 1f - m_armorPercent;
		}
		m_health = Mathf.Clamp(m_health + a_delta, 0f, 100f);
		if (IsDead() && m_respawnTime < 0f)
		{
			KillAndResetPassengers();
			m_vehicle.AssignInput(0f, 0f, false);
			base.collider.enabled = false;
			if (null != m_server)
			{
				m_server.CreateFreeWorldItem(131, 5, base.transform.position);
			}
			m_respawnTime = Time.time + 180f;
		}
		return m_health;
	}

	public void DestroyCarAndForceExitPassengers()
	{
		for (int i = 0; i < 4; i++)
		{
			if (m_data.passengerIds[i] == -1)
			{
				continue;
			}
			if (null != m_server)
			{
				ServerPlayer playerByOnlineid = m_server.GetPlayerByOnlineid(m_data.passengerIds[i]);
				if (playerByOnlineid != null)
				{
					playerByOnlineid.ExitVehicle(true);
				}
			}
			m_data.passengerIds[i] = -1;
		}
		ChangeHealthBy(-100000f);
	}

	public int GetPassengerCount()
	{
		return m_passengerCount;
	}

	public bool IsNpcControlled()
	{
		return false;
	}

	public float GetHealth()
	{
		return m_health;
	}

	public bool IsDead()
	{
		return m_health < 1f;
	}

	public void AssignInput(float a_v, float a_h, bool a_space)
	{
		if (null != m_vehicle)
		{
			m_vehicle.AssignInput(a_v, a_h, a_space);
		}
	}

	public bool AddPassenger(int a_id)
	{
		bool flag = false;
		if (!IsDead())
		{
			flag = ChangePassenger(-1, a_id);
			if (flag)
			{
				m_passengerCount++;
			}
		}
		return flag;
	}

	public bool RemovePassenger(int a_id)
	{
		bool flag = ChangePassenger(a_id, -1);
		if (flag)
		{
			m_passengerCount--;
		}
		return flag;
	}

	public Vector3 GetPassengerExitPos(int a_id)
	{
		Vector3 result = Vector3.zero;
		Vector3 position = m_vehicle.transform.position;
		position.y = 0f;
		Vector3 right = m_vehicle.transform.right;
		float x = right.x;
		Vector3 right2 = m_vehicle.transform.right;
		Vector3 vector = new Vector3(x, 0f, right2.z);
		Vector3 normalized = vector.normalized;
		Vector3 forward = m_vehicle.transform.forward;
		float x2 = forward.x;
		Vector3 forward2 = m_vehicle.transform.forward;
		Vector3 vector2 = new Vector3(x2, 0f, forward2.z);
		Vector3 normalized2 = vector2.normalized;
		for (int i = 0; i < 4; i++)
		{
			if (a_id == m_data.passengerIds[i])
			{
				float num = (i % 2 != 0) ? 2.1f : (-2.1f);
				float d = 0f - num;
				float d2 = ((2 <= i) ? (-1.5f) : 0f) + Random.Range(-0.5f, 0.5f);
				float num2 = 0.6f;
				Vector3 vector3 = position + normalized * num + normalized2 * d2;
				Vector3 vector4 = position + normalized * d + normalized2 * d2;
				vector3.y = (vector4.y = num2 + 0.2f);
				if (!Raycaster.CheckSphere(vector3, num2) && 0.8f < Util.GetTerrainHeight(vector3))
				{
					result = vector3;
				}
				else if (!Raycaster.CheckSphere(vector4, num2) && 0.8f < Util.GetTerrainHeight(vector4))
				{
					result = vector4;
				}
				break;
			}
		}
		return result;
	}
}
