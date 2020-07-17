using UnityEngine;

public class SoccerManager : MonoBehaviour
{
	public Transform m_ball;

	public GUIText m_debugTxt;

	public float m_timeScale = 1f;

	[HideInInspector]
	public int m_goalsA;

	[HideInInspector]
	public int m_goalsB;

	private SoccerBot[] m_bots;

	private float m_generationDuration;

	private int m_generationCounter;

	private Vector3 m_ballStartPos = Vector3.zero;

	private Vector3[] m_botStartPos;

	private void Start()
	{
		m_ballStartPos = m_ball.position;
		m_bots = Object.FindObjectsOfType<SoccerBot>();
		m_botStartPos = new Vector3[m_bots.Length];
		for (int i = 0; i < m_bots.Length; i++)
		{
			m_botStartPos[i] = m_bots[i].transform.position;
		}
	}

	private void Update()
	{
		Time.timeScale = m_timeScale;
		if (m_bots == null)
		{
			return;
		}
		m_generationDuration += Time.deltaTime;
		m_debugTxt.text = m_goalsA + " : " + m_goalsB;
		bool flag = 0 < m_goalsA;
		bool flag2 = 0 < m_goalsB;
		if (!flag && !flag2)
		{
			return;
		}
		m_generationCounter++;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = -1;
		for (int i = 0; i < m_bots.Length; i++)
		{
			if (flag)
			{
				if (m_bots[i].m_inTeamA)
				{
					if (num == -1)
					{
						num = i;
					}
					else
					{
						num2 = i;
					}
				}
				else if (num3 == -1)
				{
					num3 = i;
				}
				else
				{
					num4 = i;
				}
			}
			else if (m_bots[i].m_inTeamA)
			{
				if (num3 == -1)
				{
					num3 = i;
				}
				else
				{
					num4 = i;
				}
			}
			else if (num == -1)
			{
				num = i;
			}
			else
			{
				num2 = i;
			}
		}
		m_bots[num3].m_neuralNet.BecomeChild(m_bots[num].m_neuralNet, m_bots[num2].m_neuralNet);
		m_bots[num4].m_neuralNet.BecomeChild(m_bots[num].m_neuralNet, m_bots[num2].m_neuralNet);
		Debug.Log("Generation " + m_generationCounter + " over! Team A Won: " + flag + " It took: " + m_generationDuration + " Avg Gen Dur: " + Time.time / (float)m_generationCounter);
		ResetRound();
		m_generationDuration = 0f;
	}

	private void ResetRound()
	{
		m_ball.transform.position = m_ballStartPos;
		for (int i = 0; i < m_bots.Length; i++)
		{
			m_bots[i].transform.position = m_botStartPos[i];
		}
		m_goalsA = (m_goalsB = 0);
	}
}