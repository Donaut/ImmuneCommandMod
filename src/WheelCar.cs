using UnityEngine;

public class WheelCar : MonoBehaviour
{
	public bool m_testMode;

	public GUIText m_testText;

	public float m_torque = 600f;

	public float m_brakeTorque = 1800f;

	public bool m_allWheelDrive;

	public float m_steerAngle = 20f;

	public float m_steerSpeed = 4f;

	public float m_maxSpeed = 15f;

	public float m_maxControlSpeed = 15f;

	public Vector3 m_centerOfMass = new Vector3(0f, 0.3f, 0.5f);

	public float m_frontFriction = 1f;

	public float m_rearFriction = 1f;

	private WheelCollider[] m_wheels;

	private float m_speed;

	private float m_frontSlipTorque;

	private float m_rearSlipTorque;

	private bool m_isControlledByPlayer = true;

	private int m_groundedWheelCount;

	private float m_distanceDriven;

	private bool m_usesOrdinaryBrake;

	private float m_maxRearSlip;

	private float m_axis_v;

	private float m_axis_h;

	private float m_axis_h_target;

	private bool m_space;

	public void AssignInput(bool a_w, bool a_a, bool a_s, bool a_d, bool a_space)
	{
		if (a_w)
		{
			m_axis_v = 1f;
		}
		else if (a_s)
		{
			m_axis_v = -1f;
		}
		else
		{
			m_axis_v = 0f;
		}
		if (a_d)
		{
			m_axis_h_target = 1f;
		}
		else if (a_a)
		{
			m_axis_h_target = -1f;
		}
		else
		{
			m_axis_h_target = 0f;
		}
		m_space = a_space;
	}

	private void Start()
	{
		m_wheels = base.gameObject.GetComponentsInChildren<WheelCollider>();
		base.rigidbody.centerOfMass = m_centerOfMass;
	}

	private void FixedUpdate()
	{
		m_speed = base.rigidbody.velocity.magnitude;
		if (!m_isControlledByPlayer)
		{
			return;
		}
		float num = (m_maxControlSpeed - Mathf.Min(m_speed, m_maxControlSpeed)) / m_maxControlSpeed;
		float num2 = 0.1f * (m_torque * m_frontFriction) + 0.8f * num * (m_torque * m_frontFriction);
		m_maxRearSlip = 0.5f * (m_torque * m_rearFriction) + num * (m_torque * m_rearFriction);
		bool flag = Vector3.Dot(base.rigidbody.velocity.normalized, base.transform.forward) > 0f;
		bool handbrake = GetHandbrake();
		if (handbrake)
		{
			float num3 = m_maxRearSlip * 0.4f;
			float num4 = m_maxRearSlip * 0.6f;
			m_rearSlipTorque = ((!flag) ? num4 : num3);
			m_frontSlipTorque = ((!flag) ? num3 : num4);
		}
		else
		{
			if (m_rearSlipTorque < m_maxRearSlip * 0.9f)
			{
				m_rearSlipTorque += m_maxRearSlip * 0.01f;
			}
			else
			{
				m_rearSlipTorque = m_maxRearSlip;
			}
			if (m_frontSlipTorque < num2 * 0.9f)
			{
				m_frontSlipTorque += num2 * 0.01f;
			}
			else
			{
				m_frontSlipTorque = num2;
			}
		}
		HandleWheels(handbrake, num2, m_maxRearSlip, flag);
		if ((bool)m_testText)
		{
			m_testText.text = (int)(m_speed * 3.6f) + " kmh" + (int)m_distanceDriven + " m";
		}
	}

	private void Update()
	{
		if (Mathf.Abs(m_axis_h_target - m_axis_h) > 0.05f)
		{
			m_axis_h += Mathf.Clamp(Time.deltaTime * m_steerSpeed * ((!(m_axis_h_target < m_axis_h)) ? 1f : (-1f)), -1f, 1f);
		}
		else
		{
			m_axis_h = m_axis_h_target;
		}
		if (m_testMode && Application.isEditor)
		{
			AssignInput(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.A), Input.GetKey(KeyCode.S), Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.Space));
		}
	}

	private void OnCollisionEnter(Collision col)
	{
		if (col.relativeVelocity.sqrMagnitude > 10f)
		{
			float num = Mathf.Clamp01(1.5f - Mathf.Clamp01((col.relativeVelocity.sqrMagnitude - 10f) * 0.001f));
			m_rearSlipTorque *= num;
			m_frontSlipTorque *= num;
		}
	}

	private void HandleWheels(bool isBraking, float maxFrontSlip, float maxRearSlip, bool isDrivingForward)
	{
		bool flag = false;
		float steering = GetSteering();
		m_groundedWheelCount = 0;
		WheelCollider[] wheels = m_wheels;
		foreach (WheelCollider wheelCollider in wheels)
		{
			m_groundedWheelCount += (wheelCollider.isGrounded ? 1 : 0);
			flag = ("tire_fl" == wheelCollider.gameObject.name || "tire_fr" == wheelCollider.gameObject.name);
			if (isBraking)
			{
				wheelCollider.brakeTorque = (flag ? 0f : ((!isDrivingForward) ? (m_brakeTorque * 0.4f) : (m_brakeTorque * 0.6f)));
				wheelCollider.motorTorque = 0f;
			}
			else
			{
				float num = GetAcceleration() * m_torque;
				m_usesOrdinaryBrake = ((num > 0f && !isDrivingForward) || (num < 0f && isDrivingForward));
				if (!m_usesOrdinaryBrake)
				{
					wheelCollider.brakeTorque = 0f;
					if (isDrivingForward)
					{
						float num2 = num * 0.5f + Mathf.Clamp((steering + 0.5f) * num, 0f, num);
						float num3 = num * 0.5f + Mathf.Clamp((steering * -1f + 0.5f) * num, 0f, num);
						if (m_allWheelDrive)
						{
							if ("tire_rr" == wheelCollider.gameObject.name || "tire_fr" == wheelCollider.gameObject.name)
							{
								wheelCollider.motorTorque = num3 * 0.5f;
							}
							else
							{
								wheelCollider.motorTorque = num2 * 0.5f;
							}
						}
						else if ("tire_rr" == wheelCollider.gameObject.name)
						{
							wheelCollider.motorTorque = num3;
						}
						else if ("tire_rl" == wheelCollider.gameObject.name)
						{
							wheelCollider.motorTorque = num2;
						}
						else
						{
							wheelCollider.motorTorque = 0f;
						}
					}
					else
					{
						wheelCollider.motorTorque = ((!flag) ? num : 0f);
					}
				}
				else
				{
					num = Mathf.Abs((GetAcceleration() + ((!isDrivingForward) ? 0.1f : (-0.1f))) * m_brakeTorque);
					wheelCollider.brakeTorque = ((!flag) ? (num * 0.6f) : (num * 0.4f));
					wheelCollider.motorTorque = 0f;
					m_rearSlipTorque = maxRearSlip * 0.66f;
				}
			}
			WheelFrictionCurve sidewaysFriction = wheelCollider.sidewaysFriction;
			if (flag)
			{
				wheelCollider.steerAngle = steering * m_steerAngle;
				sidewaysFriction.extremumValue = m_frontSlipTorque;
				sidewaysFriction.asymptoteValue = m_frontSlipTorque * 0.5f;
			}
			else
			{
				sidewaysFriction.extremumValue = m_rearSlipTorque;
				sidewaysFriction.asymptoteValue = m_rearSlipTorque * 0.5f;
			}
			wheelCollider.sidewaysFriction = sidewaysFriction;
		}
	}

	private float GetSteering()
	{
		float value = Mathf.Clamp(m_axis_h * 1.5f, -1f, 1f);
		return Mathf.Clamp(value, -1f, 1f);
	}

	private float GetAcceleration()
	{
		float value = (!(Mathf.Abs(m_speed) < m_maxSpeed)) ? 0f : (m_axis_v * 1.5f);
		return Mathf.Clamp(value, -1f, 1f);
	}

	private bool GetHandbrake()
	{
		return m_space;
	}

	public float GetDistanceDriven()
	{
		return m_distanceDriven;
	}
}
