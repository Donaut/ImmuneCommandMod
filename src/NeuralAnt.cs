using UnityEngine;

public class NeuralAnt : MonoBehaviour
{
	private const int c_netLayers = 3;

	private const int c_layerNodes = 3;

	private const int c_nodeWeights = 3;

	private const int c_ioCount = 3;

	public float m_speed = 3f;

	public GUIText m_debugTxt;

	[HideInInspector]
	public float[,,] m_neuralNet = new float[3, 3, 3];

	private float[] m_inputs = new float[3];

	private float[] m_outputs = new float[3];

	private AntFood[] m_foods;

	private NeuralAnt[] m_ants;

	[HideInInspector]
	public int m_score;

	private void Start()
	{
		m_foods = Object.FindObjectsOfType<AntFood>();
		m_ants = Object.FindObjectsOfType<NeuralAnt>();
		InitNetRandomly();
	}

	private void Update()
	{
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		float y = eulerAngles.y;
		float num = FindFood();
		float num2 = 0f;
		m_inputs[0] = y;
		m_inputs[1] = num;
		m_inputs[2] = num2;
		m_outputs = DoNetIO(m_inputs);
		float num3 = (m_outputs[0] + m_outputs[1] + m_outputs[2]) / 3f;
		base.transform.rotation = Quaternion.Lerp(base.transform.rotation, Quaternion.Euler(0f, num3 * 180f, 0f), 0.1f);
		base.transform.position += base.transform.forward * Time.deltaTime * m_speed;
		Vector3 position = base.transform.position;
		if (position.x > 11f)
		{
			position.x -= 22f;
		}
		else if (position.x < -11f)
		{
			position.x += 22f;
		}
		if (position.z > 11f)
		{
			position.z -= 22f;
		}
		else if (position.z < -11f)
		{
			position.z += 22f;
		}
		base.transform.position = position;
		if (null != m_debugTxt)
		{
			m_debugTxt.text = m_inputs[0] + "\n" + m_inputs[1] + "\n" + m_inputs[2] + "\n" + m_outputs[0] + "\n" + m_outputs[1] + "\n" + m_outputs[2] + "\nscore: " + m_score;
		}
	}

	private float FindFood()
	{
		float num = 99999f;
		float result = 0f;
		for (int i = 0; i < m_foods.Length; i++)
		{
			if (!(null != m_foods[i]))
			{
				continue;
			}
			Vector3 forward = m_foods[i].transform.position - base.transform.position;
			if (forward.sqrMagnitude < num)
			{
				num = forward.sqrMagnitude;
				Vector3 eulerAngles = Quaternion.LookRotation(forward).eulerAngles;
				result = eulerAngles.y;
				if (num < 1f)
				{
					m_foods[i].Consume();
					m_score++;
				}
			}
		}
		return result;
	}

	private float FindCompetition()
	{
		float num = 99999f;
		float result = 0f;
		for (int i = 0; i < m_ants.Length; i++)
		{
			if (null != m_ants[i] && this != m_ants[i])
			{
				Vector3 forward = m_ants[i].transform.position - base.transform.position;
				if (forward.sqrMagnitude < num)
				{
					num = forward.sqrMagnitude;
					Vector3 eulerAngles = Quaternion.LookRotation(forward).eulerAngles;
					result = eulerAngles.y;
				}
			}
		}
		return result;
	}

	private void InitNetRandomly()
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					m_neuralNet[i, j, k] = Random.Range(-1f, 1f);
				}
			}
		}
	}

	private float[] DoNetIO(float[] a_inputs)
	{
		float[] array = new float[3];
		for (int i = 0; i < 3; i++)
		{
			if (i > 0)
			{
				a_inputs = array;
			}
			for (int j = 0; j < 3; j++)
			{
				float num = 0f;
				for (int k = 0; k < 3; k++)
				{
					num += m_neuralNet[i, j, k] * a_inputs[k];
				}
				array[j] = num;
			}
		}
		return array;
	}

	public void BecomeChild(NeuralAnt a_mother, NeuralAnt a_father)
	{
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				for (int k = 0; k < 3; k++)
				{
					NeuralAnt neuralAnt = (Random.Range(0, 2) != 0) ? a_father : a_mother;
					m_neuralNet[i, j, k] = neuralAnt.m_neuralNet[i, j, k];
				}
			}
		}
		m_neuralNet[Random.Range(0, 3), Random.Range(0, 3), Random.Range(0, 3)] = Random.Range(-1f, 1f);
	}
}