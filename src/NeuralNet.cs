using UnityEngine;

public class NeuralNet
{
	[HideInInspector]
	public float[,,] m_neuralNet;

	private int m_layerCount;

	private int m_inputOutputCount;

	public void Init(int a_layers = 3, int a_ioCount = 3)
	{
		m_layerCount = a_layers;
		m_inputOutputCount = a_ioCount;
		m_neuralNet = new float[m_layerCount, m_inputOutputCount, m_inputOutputCount];
		InitNetRandomly();
	}

	private void InitNetRandomly()
	{
		if (m_neuralNet == null)
		{
			return;
		}
		for (int i = 0; i < m_layerCount; i++)
		{
			for (int j = 0; j < m_inputOutputCount; j++)
			{
				for (int k = 0; k < m_inputOutputCount; k++)
				{
					m_neuralNet[i, j, k] = Random.Range(-1f, 1f);
				}
			}
		}
	}

	public float[] DoNetIO(float[] a_inputs)
	{
		if (m_neuralNet == null)
		{
			Init();
		}
		float[] array = new float[m_inputOutputCount];
		for (int i = 0; i < m_layerCount; i++)
		{
			if (i > 0)
			{
				a_inputs = array;
			}
			for (int j = 0; j < m_inputOutputCount; j++)
			{
				float num = 0f;
				for (int k = 0; k < m_inputOutputCount; k++)
				{
					num += m_neuralNet[i, j, k] * a_inputs[k];
				}
				array[j] = num;
			}
		}
		return array;
	}

	public void BecomeChild(NeuralNet a_mother, NeuralNet a_father)
	{
		if (m_neuralNet == null)
		{
			Init();
		}
		for (int i = 0; i < m_layerCount; i++)
		{
			for (int j = 0; j < m_inputOutputCount; j++)
			{
				for (int k = 0; k < m_inputOutputCount; k++)
				{
					NeuralNet neuralNet = (Random.Range(0, 2) != 0) ? a_father : a_mother;
					m_neuralNet[i, j, k] = neuralNet.m_neuralNet[i, j, k];
				}
			}
		}
		m_neuralNet[Random.Range(0, m_layerCount), Random.Range(0, m_inputOutputCount), Random.Range(0, m_inputOutputCount)] = Random.Range(-1f, 1f);
	}
}