using UnityEngine;

public class ChangeGeneration : MonoBehaviour
{
	public GUIText m_debugTxt;

	private NeuralAnt[] m_ants;

	private float m_generationDuration;

	private int m_generationCounter;

	private void Start()
	{
		m_ants = Object.FindObjectsOfType<NeuralAnt>();
	}

	private void Update()
	{
		if (m_ants == null)
		{
			return;
		}
		m_generationDuration += Time.deltaTime;
		string text = string.Empty;
		int num = -1;
		for (int i = 0; i < m_ants.Length; i++)
		{
			if (9 < m_ants[i].m_score)
			{
				num = i;
			}
			string text2 = text;
			text = text2 + i.ToString() + ": " + m_ants[i].m_score + "\n";
		}
		m_debugTxt.text = text;
		if (num == -1)
		{
			return;
		}
		m_generationCounter++;
		int num2 = -1;
		int num3 = -1;
		for (int j = 0; j < m_ants.Length; j++)
		{
			if (num3 < m_ants[j].m_score && j != num)
			{
				num2 = j;
				num3 = m_ants[j].m_score;
			}
		}
		for (int k = 0; k < m_ants.Length; k++)
		{
			if (k != num && k != num2)
			{
				m_ants[k].BecomeChild(m_ants[num], m_ants[num2]);
			}
			m_ants[k].m_score = 0;
		}
		Debug.Log("Generation " + m_generationCounter + " over! It took: " + m_generationDuration + " and index " + num + " won. Second: " + num2 + " Avg Gen Dur: " + Time.time / (float)m_generationCounter);
		m_generationDuration = 0f;
	}
}