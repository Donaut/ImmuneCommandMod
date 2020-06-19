using Mono.Data.SqliteClient;
using System.Data;
using UnityEngine;

public class AnalyseDatabase : MonoBehaviour
{
	public GameObject m_prefab;

	public bool m_createFakePlayers;

	private IDbConnection m_sqlConnection;

	private IDbCommand m_sqlCommand;

	private IDataReader m_sqlReader;

	private void Start()
	{
		m_sqlConnection = new SqliteConnection("URI=file:immune.db");
		m_sqlConnection.Close();
		m_sqlConnection.Open();
		m_sqlCommand = m_sqlConnection.CreateCommand();
		LoadData();
	}

	private void LoadData()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		float num6 = 0f;
		float num7 = 0f;
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = "SELECT aid, pid, x, y, health, energy, karma, xp, condition, gold FROM player;";
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				while (dataReader.Read())
				{
					DatabasePlayer a_player = default(DatabasePlayer);
					a_player.aid = (ulong)dataReader.GetInt64(0);
					a_player.pid = dataReader.GetInt32(1);
					a_player.x = dataReader.GetFloat(2);
					a_player.y = dataReader.GetFloat(3);
					a_player.health = dataReader.GetInt32(4);
					a_player.energy = dataReader.GetInt32(5);
					a_player.karma = dataReader.GetInt32(6);
					a_player.xp = dataReader.GetInt32(7);
					a_player.condition = dataReader.GetInt32(8);
					a_player.gold = dataReader.GetInt32(9);
					num++;
					if (a_player.xp == 0)
					{
						num2++;
					}
					else if (1000 < a_player.xp)
					{
						num5++;
					}
					if (Mathf.Abs(a_player.x - -1114f) < 70f && Mathf.Abs(a_player.y - 720f) < 70f)
					{
						num3++;
					}
					else if (Mathf.Abs(a_player.x - -1114f) > 1000f || Mathf.Abs(a_player.y - 720f) > 1000f)
					{
						num4++;
					}
					num6 += (float)a_player.karma;
					num7 += (float)a_player.gold;
					if (m_createFakePlayers)
					{
						CreateFakePlayer(a_player);
					}
				}
				dataReader.Close();
			}
			dbTransaction.Commit();
			num2 = (int)((float)num2 / (float)num * 100f);
			num3 = (int)((float)num3 / (float)num * 100f);
			num4 = (int)((float)num4 / (float)num * 100f);
			num5 = (int)((float)num5 / (float)num * 100f);
			Debug.Log("players: " + num + "\nplayersWithNoXP: " + num2 + "%\nplayersOnTutorialIsland: " + num3 + "%\nplayersThatMovedSomewhat: " + num4 + "%\nplayersWithOver1000XP: " + num5 + "%\naverageKarma: " + num6 / (float)num + "\naverageGold: " + num7 / (float)num);
		}
	}

	private void CreateFakePlayer(DatabasePlayer a_player)
	{
		string text = "HP:  " + a_player.health + "\nEP:  " + a_player.energy + "\nKA:  " + a_player.karma + "\nXP:  " + a_player.xp + "\nCO:  " + a_player.condition + "\nGO:  " + a_player.gold;
		GameObject gameObject = (GameObject)Object.Instantiate(m_prefab);
		gameObject.transform.position = new Vector3(a_player.x, 0.3f, a_player.y);
		TextMesh componentInChildren = gameObject.GetComponentInChildren<TextMesh>();
		if (null != componentInChildren)
		{
			componentInChildren.text = text;
		}
	}
}
