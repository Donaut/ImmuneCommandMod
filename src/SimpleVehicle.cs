using UnityEngine;

public class SimpleVehicle : MonoBehaviour
{
	public bool m_testMode;

	public float m_power = 1000f;

	public float m_rotPower = 1000f;

	public float m_sideWaysDrag = 2f;

	public float m_maxSpeedMs = 14f;

	public float m_brakeDrag = 2f;

	public float m_maxSteerSpeedMs = 15f;

	public float m_steerSpeed = 4f;

	public float m_snapAngleDeg = 6f;

	public GUIText m_txt;

	private float m_startDrag;

	private float m_rotationTarget = -1f;

	private float m_axis_v;

	private float m_axis_h;

	private bool m_space;

	public void AssignInput(float a_v, float a_h, bool a_space)
	{
		m_axis_v = a_v;
		m_axis_h = a_h;
		m_space = a_space;
	}

	private void Start()
	{
		m_startDrag = base.rigidbody.drag;
	}

	private void Update()
	{
		if (m_testMode && Application.isEditor)
		{
			AssignInput(Input.GetAxis("Vertical"), Input.GetAxis("Horizontal"), Input.GetKey(KeyCode.Space));
		}
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		float magnitude = base.rigidbody.velocity.magnitude;
		float num = Mathf.Clamp(magnitude, 0f, m_maxSteerSpeedMs) / m_maxSteerSpeedMs;
		float axis_v = m_axis_v;
		float num2 = Vector3.Dot(base.transform.forward, base.rigidbody.velocity.normalized);
		float num3 = 1f - Mathf.Abs(num2);
		float num4 = m_sideWaysDrag + (1f - num) * 2f * m_sideWaysDrag;
		float d = m_power * fixedDeltaTime * (num3 * 0.5f + 0.5f) * (num * 0.5f + 0.5f);
		if (axis_v != 0f && magnitude < m_maxSpeedMs)
		{
			base.rigidbody.AddForce(base.transform.forward * axis_v * d * (1f - Mathf.Abs(num3) * 0.5f));
		}
		base.rigidbody.drag = m_startDrag;
		if (m_space)
		{
			base.rigidbody.drag += m_brakeDrag;
		}
		else if (1f < magnitude)
		{
			base.rigidbody.drag += num3 * num4;
		}
		if (magnitude > 1f)
		{
			float num5 = num * (1f - num3);
			float num6 = num2 * num5 * fixedDeltaTime * m_axis_h;
			if (num6 != 0f)
			{
				base.rigidbody.AddTorque(Vector3.up * num6 * m_rotPower);
				m_rotationTarget = -1f;
			}
			else
			{
				SnapRotationToStraight();
			}
			if (null != m_txt)
			{
				m_txt.text = "v: " + Input.GetAxis("Vertical") * 100f + " h: " + Input.GetAxis("Horizontal") * 100f + " speed: " + magnitude * 3.6f + " kmh steer: " + num5 + "\ninteract: " + Input.GetButton("Interact") + " attack: " + Input.GetButton("Attack") + " inventory: " + Input.GetButton("Inventory") + " com.: " + Input.GetButton("Communicator");
			}
		}
	}

	private void SnapRotationToStraight()
	{
		float snapAngleDeg = m_snapAngleDeg;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		while (eulerAngles.y > 360f)
		{
			eulerAngles.y -= 360f;
		}
		while (eulerAngles.y < 0f)
		{
			eulerAngles.y += 360f;
		}
		if (eulerAngles.y < snapAngleDeg)
		{
			m_rotationTarget = 0f;
		}
		else if (eulerAngles.y > 360f - snapAngleDeg)
		{
			m_rotationTarget = 360f;
		}
		else if (eulerAngles.y > 90f - snapAngleDeg && eulerAngles.y < 90f + snapAngleDeg)
		{
			m_rotationTarget = 90f;
		}
		else if (eulerAngles.y > 180f - snapAngleDeg && eulerAngles.y < 180f + snapAngleDeg)
		{
			m_rotationTarget = 180f;
		}
		else if (eulerAngles.y > 270f - snapAngleDeg && eulerAngles.y < 270f + snapAngleDeg)
		{
			m_rotationTarget = 270f;
		}
		else
		{
			m_rotationTarget = -1f;
		}
		base.transform.rotation = Quaternion.Euler(eulerAngles);
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.y = 0f;
		base.transform.position = position;
		if (m_rotationTarget != -1f)
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, m_rotationTarget, 0f), Time.deltaTime * 3f);
		}
	}
}
