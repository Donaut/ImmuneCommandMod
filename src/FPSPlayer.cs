using UnityEngine;

public class FPSPlayer : MonoBehaviour
{
	public float m_mouseSensitivity = 5f;

	public float m_speed = 5f;

	private CharacterController m_char;

	private void Start()
	{
		m_char = GetComponent<CharacterController>();
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		float y = localEulerAngles.y + Input.GetAxis("Mouse X") * m_mouseSensitivity;
		base.transform.localEulerAngles = new Vector3(0f, y, 0f);
		float axis = Input.GetAxis("Horizontal");
		float axis2 = Input.GetAxis("Vertical");
		Vector3 vector = base.transform.forward * axis2 + base.transform.right * axis;
		m_char.Move((vector.normalized + Vector3.up * -5f) * fixedDeltaTime * m_speed);
	}
}
