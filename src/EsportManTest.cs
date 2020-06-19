using System.Collections.Generic;
using UnityEngine;

public class EsportManTest : MonoBehaviour
{
	public TextMesh m_teamAtxt;

	public TextMesh m_teamBtxt;

	public TextMesh m_resultsTxt;

	public TextMesh m_strDevTxt;

	public int m_matchCount = 1;

	private List<EsportPlayer> m_teamA = new List<EsportPlayer>();

	private List<EsportPlayer> m_teamB = new List<EsportPlayer>();

	private float m_startSkill = 50f;

	private int m_gamesPlayed;

	private void FixedUpdate()
	{
		m_startSkill += Random.Range(-1f, 1f);
		m_gamesPlayed++;
		m_strDevTxt.text = "Games played: " + m_gamesPlayed + "\nSkill: " + m_startSkill;
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.A))
		{
			return;
		}
		m_teamA.Clear();
		m_teamB.Clear();
		m_teamAtxt.text = string.Empty;
		m_teamBtxt.text = string.Empty;
		m_resultsTxt.text = string.Empty;
		for (int i = 0; i < 5; i++)
		{
			m_teamA.Add(new EsportPlayer(Random.Range(0f, 99f), Random.Range(0f, 99f), Random.Range(0f, 99f)));
			m_teamB.Add(new EsportPlayer(Random.Range(0f, 99f), Random.Range(0f, 99f), Random.Range(0f, 99f)));
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		string text;
		for (int j = 0; j < m_matchCount; j++)
		{
			int num4 = 0;
			int num5 = 0;
			for (int k = 0; k < 30; k++)
			{
				if (num4 >= 16)
				{
					break;
				}
				if (num5 >= 16)
				{
					break;
				}
				while (0 < GetAlivePlayerCount(m_teamA) && 0 < GetAlivePlayerCount(m_teamB))
				{
					int randomAlivePlayerIndex = GetRandomAlivePlayerIndex(m_teamA);
					int randomAlivePlayerIndex2 = GetRandomAlivePlayerIndex(m_teamB);
					float strength = m_teamA[randomAlivePlayerIndex].strength;
					float strength2 = m_teamB[randomAlivePlayerIndex2].strength;
					float num6 = strength / (strength + strength2);
					if (Random.Range(0f, 1f) < num6)
					{
						m_teamA[randomAlivePlayerIndex].kills++;
						m_teamB[randomAlivePlayerIndex2].deaths++;
						m_teamB[randomAlivePlayerIndex2].alive = false;
					}
					else
					{
						m_teamA[randomAlivePlayerIndex].deaths++;
						m_teamB[randomAlivePlayerIndex2].kills++;
						m_teamA[randomAlivePlayerIndex].alive = false;
					}
				}
				if (GetAlivePlayerCount(m_teamA) == 0)
				{
					num5++;
				}
				else
				{
					num4++;
				}
				for (int l = 0; l < 5; l++)
				{
					m_teamA[l].alive = true;
					m_teamB[l].alive = true;
				}
			}
			if (num4 > num5)
			{
				num++;
			}
			else if (num4 == num5)
			{
				num3++;
			}
			else
			{
				num2++;
			}
			TextMesh resultsTxt = m_resultsTxt;
			text = resultsTxt.text;
			resultsTxt.text = text + num4.ToString() + ":" + num5 + ", ";
		}
		TextMesh resultsTxt2 = m_resultsTxt;
		text = resultsTxt2.text;
		resultsTxt2.text = text + "\n" + num + " : " + num3 + " : " + num2;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		for (int m = 0; m < 5; m++)
		{
			num7 += m_teamA[m].strength;
			num8 += m_teamB[m].strength;
			num9 += m_teamA[m].skill;
			num10 += m_teamB[m].skill;
		}
		num7 /= 5f;
		num8 /= 5f;
		num9 /= 5f;
		num10 /= 5f;
		for (int n = 0; n < 5; n++)
		{
			TextMesh teamAtxt = m_teamAtxt;
			text = teamAtxt.text;
			teamAtxt.text = text + n.ToString() + " S " + (int)m_teamA[n].skill + " E " + (int)m_teamA[n].experience + " M " + (int)m_teamA[n].motivation + " Str " + (int)m_teamA[n].strength + " KD " + m_teamA[n].kills / m_matchCount + ":" + m_teamA[n].deaths / m_matchCount + "\n";
			TextMesh teamBtxt = m_teamBtxt;
			text = teamBtxt.text;
			teamBtxt.text = text + n.ToString() + " S " + (int)m_teamB[n].skill + " E " + (int)m_teamB[n].experience + " M " + (int)m_teamB[n].motivation + " Str " + (int)m_teamB[n].strength + " KD " + m_teamB[n].kills / m_matchCount + ":" + m_teamB[n].deaths / m_matchCount + "\n";
		}
		TextMesh teamAtxt2 = m_teamAtxt;
		text = teamAtxt2.text;
		teamAtxt2.text = text + " Str: " + num7 + " T: " + num9;
		TextMesh teamBtxt2 = m_teamBtxt;
		text = teamBtxt2.text;
		teamBtxt2.text = text + " Str: " + num8 + " T: " + num10;
	}

	private int GetAlivePlayerCount(List<EsportPlayer> a_players)
	{
		int num = 0;
		if (a_players != null)
		{
			for (int i = 0; i < a_players.Count; i++)
			{
				if (a_players[i].alive)
				{
					num++;
				}
			}
		}
		return num;
	}

	private int GetRandomAlivePlayerIndex(List<EsportPlayer> a_players)
	{
		if (a_players != null)
		{
			int num = Random.Range(0, a_players.Count);
			int num2 = 0;
			for (int i = 0; i < a_players.Count; i++)
			{
				num2 = (i + num) % a_players.Count;
				if (a_players[num2].alive)
				{
					return num2;
				}
			}
		}
		return -1;
	}
}
