using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	public float m_jumpForce = 100f;

	public float m_moveForce = 100f;

	public float m_gravity = -20f;

	private bool m_isGrounded = true;

	private float m_lastGroundedTime;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 position = base.transform.position;
		m_isGrounded = (position.y < 1.6f);
		if (m_isGrounded)
		{
			m_lastGroundedTime = Time.time;
		}
		Vector3 velocity = base.rigidbody.velocity;
		velocity.y += m_gravity * fixedDeltaTime;
		if (Input.GetKey(KeyCode.W))
		{
			if (m_isGrounded)
			{
				velocity.y = m_jumpForce;
			}
			else if (Time.time - m_lastGroundedTime < 0.3f)
			{
				velocity.y += m_jumpForce * fixedDeltaTime;
			}
		}
		velocity.x = Input.GetAxis("Horizontal") * m_moveForce;
		base.rigidbody.velocity = velocity;
	}

	private void Update()
	{
	}
}
