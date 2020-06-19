using UnityEngine;

public class BallPhysics : MonoBehaviour
{
	public float m_maxMagnitude = 10f;

	public float m_gravity = -20f;

	private Vector3 m_startPos;

	private void Start()
	{
		m_startPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 position = base.transform.position;
		if (position.y < 1.1f)
		{
			base.transform.position = m_startPos;
			base.rigidbody.velocity = Vector3.zero;
		}
		Vector3 velocity = base.rigidbody.velocity;
		if (velocity.magnitude > m_maxMagnitude)
		{
			velocity = velocity.normalized * m_maxMagnitude;
		}
		velocity.y += m_gravity * fixedDeltaTime;
		base.rigidbody.velocity = velocity;
	}
}
