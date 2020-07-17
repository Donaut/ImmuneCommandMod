using UnityEngine;

public class SoccerBot : MonoBehaviour
{
	private const int c_netLayers = 3;

	private const int c_ioCount = 3;

	public bool m_inTeamA = true;

	public Transform m_ball;

	public float m_speed = 5f;

	public float m_rotSpeed = 0.5f;

	[HideInInspector]
	public NeuralNet m_neuralNet;

	private float[] m_inputs = new float[3];

	private float[] m_outputs = new float[3];

	private void Start()
	{
		m_neuralNet = new NeuralNet();
		m_neuralNet.Init();
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		float y = eulerAngles.y;
		Vector3 eulerAngles2 = Quaternion.LookRotation((m_ball.position - base.transform.position).normalized).eulerAngles;
		float y2 = eulerAngles2.y;
		m_inputs[0] = y;
		m_inputs[1] = y2;
		float[] inputs = m_inputs;
		Vector3 position = m_ball.position;
		inputs[2] = position.y;
		m_outputs = m_neuralNet.DoNetIO(m_inputs);
		Move(fixedDeltaTime);
	}

	private void Move(float a_dt)
	{
		float d = (m_outputs[0] + m_outputs[1]) / 2f;
		float d2 = Mathf.Clamp(m_outputs[2], 0f - m_speed, m_speed);
		base.rigidbody.MovePosition(base.transform.position + base.transform.forward * a_dt * d2);
		base.rigidbody.AddTorque(Vector3.up * d * a_dt * m_rotSpeed);
	}
}