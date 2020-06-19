using Mono.Data.SqliteClient;
using System.Data;
using UnityEngine;

public static class SQLWorker
{
	private static IDbConnection m_sqlConnection;

	private static IDbCommand m_sqlCommand;

	private static IDataReader m_sqlReader;

	private static bool m_inited;

	private static void Init()
	{
		if (!m_inited)
		{
			m_inited = true;
			m_sqlConnection = new SqliteConnection("URI=file:deathcarriers.db");
			m_sqlConnection.Close();
			m_sqlConnection.Open();
			m_sqlCommand = m_sqlConnection.CreateCommand();
			m_sqlCommand.CommandText = "CREATE TABLE IF NOT EXISTS player ( pid INTEGER NOT NULL PRIMARY KEY, name TEXT NOT NULL, x REAL NOT NULL DEFAULT '0.0', y REAL NOT NULL DEFAULT '0.0' );";
			m_sqlCommand.ExecuteNonQuery();
		}
	}

	public static void GetOrCreatePlayer(ref ServerPlayer a_player, string a_name)
	{
		if (!m_inited)
		{
			Init();
		}
		a_player = null;
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = "SELECT pid, x, y FROM player WHERE name='" + a_name + "' LIMIT 1;";
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				if (dataReader.Read())
				{
				}
				dataReader.Close();
			}
			if (a_player == null)
			{
				m_sqlCommand.CommandText = "INSERT INTO player (name) VALUES('" + a_name + "');SELECT last_insert_rowid();";
				using (IDataReader dataReader2 = m_sqlCommand.ExecuteReader())
				{
					if (!dataReader2.Read())
					{
						Debug.Log("SQLWorker.cs: ERROR: Couldn't create new player in database?!");
					}
					dataReader2.Close();
				}
			}
			dbTransaction.Commit();
		}
	}
}
