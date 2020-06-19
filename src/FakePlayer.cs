using UnityEngine;

public class FakePlayer : MonoBehaviour
{
	public int m_id;

	public float m_speed = 5f;

	public float m_rotateSpeed = 270f;

	private int m_inputdir;

	private CharacterController charCon;

	private void Start()
	{
		charCon = (CharacterController)GetComponent(typeof(CharacterController));
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 vector = Vector3.zero;
		if (m_inputdir == 1 || m_inputdir == 2 || m_inputdir == 8)
		{
			vector = base.transform.forward * m_speed;
		}
		else if (m_inputdir == 5 || m_inputdir == 4 || m_inputdir == 6)
		{
			vector = base.transform.forward * (m_speed * -0.5f);
		}
		if (m_inputdir > 1 && m_inputdir < 5)
		{
			base.transform.Rotate(Vector3.up, m_rotateSpeed * fixedDeltaTime);
		}
		else if (m_inputdir > 5 && m_inputdir < 9)
		{
			base.transform.Rotate(Vector3.up, m_rotateSpeed * (0f - fixedDeltaTime));
		}
		if (Vector3.zero != vector)
		{
			charCon.Move(vector * fixedDeltaTime);
		}
	}

	public void SetInput(int a_inputdir)
	{
		m_inputdir = a_inputdir;
	}
}
