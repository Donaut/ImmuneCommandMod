using UnityEngine;

public class ControlledChar : MonoBehaviour
{
	public bool m_testMode;

	[HideInInspector]
	public bool m_isWalking;

	public float m_speed = 8f;

	public float m_rotationSpeed2 = 6f;

	private CharacterController m_controller;

	private ServerPlayer m_serverPlayer;

	private float m_ignoreMoveRotTime = -1f;

	private float m_rot = 180f;

	private bool m_isMoving;

	private float m_curSpeed;

	private float m_changeHealth;

	private Transform m_aggressor;

	private float m_axis_v;

	private float m_axis_h;

	private bool m_use;

	private bool m_space;

	private bool m_useChanged;

	private bool m_spaceChanged;

	public bool IsMoving()
	{
		return m_isMoving;
	}

	public bool HasUseChanged()
	{
		return m_useChanged;
	}

	public bool HasSpaceChanged()
	{
		return m_spaceChanged;
	}

	public void AssignInput(float a_v, float a_h, bool a_use, bool a_space)
	{
		m_useChanged = (a_use != m_use);
		m_spaceChanged = (a_space != m_space);
		m_axis_v = a_v;
		m_axis_h = a_h;
		m_use = a_use;
		m_space = a_space;
	}

	public void Init(ServerPlayer a_player)
	{
		m_serverPlayer = a_player;
	}

	public ServerPlayer GetServerPlayer()
	{
		return m_serverPlayer;
	}

	public void AddSpeed(float a_percent)
	{
		m_curSpeed = m_speed * (1f + a_percent);
	}

	public void SetForceRotation(float a_rot)
	{
		if (a_rot != -1f)
		{
			m_rot = a_rot;
			m_ignoreMoveRotTime = Time.time + 0.5f;
		}
	}

	public float GetRotation()
	{
		return m_rot;
	}

	private void Start()
	{
		m_controller = GetComponent<CharacterController>();
		m_curSpeed = m_speed;
		m_testMode &= Application.isEditor;
	}

	private void Update()
	{
		if (null != m_aggressor && m_changeHealth != 0f && m_serverPlayer != null)
		{
			if (m_aggressor.gameObject.layer != 13 || !m_serverPlayer.IsSaint())
			{
				m_serverPlayer.ChangeHealthBy(m_changeHealth);
			}
			m_aggressor = null;
			m_changeHealth = 0f;
		}
		if (m_testMode)
		{
			float axis = Input.GetAxis("Horizontal");
			float axis2 = Input.GetAxis("Vertical");
			AssignInput(axis2, axis, false, false);
		}
		Move(Time.deltaTime);
	}

	private void Move(float a_dt)
	{
		m_isMoving = false;
		if (!m_controller.enabled || (m_serverPlayer != null && m_serverPlayer.IsDead()))
		{
			return;
		}
		float d = (!m_isWalking) ? m_curSpeed : (m_curSpeed * 0.6f);
		Vector3 vector = Vector3.forward * m_axis_v + Vector3.right * m_axis_h;
		if (Vector3.zero != vector)
		{
			if (Time.time > m_ignoreMoveRotTime)
			{
				float num = (!m_isWalking) ? m_rotationSpeed2 : (m_rotationSpeed2 * 0.5f);
				Vector3 eulerAngles = Quaternion.Lerp(Quaternion.Euler(0f, m_rot, 0f), Quaternion.LookRotation(vector), a_dt * num).eulerAngles;
				m_rot = eulerAngles.y;
			}
			Vector3 vector2 = vector.normalized * d * a_dt;
			if (m_testMode || 0.8f < Util.GetTerrainHeight(base.transform.position + vector2))
			{
				m_controller.Move(vector2);
			}
			m_isMoving = true;
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		base.transform.position = position;
	}

	private void OnCollisionEnter(Collision a_col)
	{
		float num = 0.5f * Mathf.Clamp(a_col.relativeVelocity.sqrMagnitude - 10f, 0f, 10000f);
		if (num > 1f)
		{
			ChangeHealthBy(0f - num);
			m_aggressor = a_col.transform;
			a_col.gameObject.SendMessage("CausedDamage", num, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void ChangeHealthBy(float a_delta)
	{
		m_changeHealth = a_delta;
	}

	private void SetAggressor(Transform a_aggressor)
	{
		m_aggressor = a_aggressor;
	}
}
