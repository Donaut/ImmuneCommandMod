using Mono.Data.SqliteClient;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using UnityEngine;

public class SQLThreadManager : MonoBehaviour
{
	private const int c_pidToCidOffset = 1000000000;

	private int m_maxPartyId = 1;

	private List<DatabasePlayer> m_playersToWrite = new List<DatabasePlayer>();

	private List<ulong> m_playerRequests = new List<ulong>();

	private List<DatabasePlayer> m_playerAnswers = new List<DatabasePlayer>();

	private List<int> m_partyRequests = new List<int>();

	private List<DatabasePlayer> m_partyAnswers = new List<DatabasePlayer>();

	private List<DatabasePlayer> m_partyPlayersToWrite = new List<DatabasePlayer>();

	private bool m_requestBuildings;

	private List<DatabaseBuilding> m_buildingAnswers = new List<DatabaseBuilding>();

	private List<int> m_inventoryClear = new List<int>();

	private List<int> m_containerRequests = new List<int>();

	private List<Vector3> m_hiddenItemsRequests = new List<Vector3>();

	private List<DatabaseItem> m_itemsToWrite = new List<DatabaseItem>();

	private List<DatabaseItem> m_itemAnswers = new List<DatabaseItem>();

	private List<DatabaseBuilding> m_buildingsToWrite = new List<DatabaseBuilding>();

	private object m_threadLock = new object();

	private Thread m_thread;

	private IDbConnection m_sqlConnection;

	private IDbCommand m_sqlCommand;

	private IDataReader m_sqlReader;

	private void Start()
	{
		if (m_sqlConnection == null)
		{
			m_sqlConnection = new SqliteConnection("URI=file:immune.db");
			m_sqlConnection.Close();
			m_sqlConnection.Open();
			m_sqlCommand = m_sqlConnection.CreateCommand();
			string str = "CREATE TABLE IF NOT EXISTS player ( pid INTEGER NOT NULL PRIMARY KEY, aid INTEGER NOT NULL, name TEXT NOT NULL DEFAULT '', x REAL NOT NULL DEFAULT '0.0', y REAL NOT NULL DEFAULT '0.0', health INTEGER NOT NULL DEFAULT '100', energy INTEGER NOT NULL DEFAULT '100', karma INTEGER NOT NULL DEFAULT '100', xp INTEGER NOT NULL DEFAULT '0', condition INTEGER NOT NULL DEFAULT '0', gold INTEGER NOT NULL DEFAULT '0', partyId INTEGER NOT NULL DEFAULT '0', partyRank INTEGER NOT NULL DEFAULT '0' );";
			str += "CREATE TABLE IF NOT EXISTS item ( iid INTEGER NOT NULL PRIMARY KEY, cid INTEGER NOT NULL DEFAULT '0', hidden INTEGER NOT NULL DEFAULT '0', type INTEGER NOT NULL DEFAULT '0', amount INTEGER NOT NULL DEFAULT '0', x REAL NOT NULL DEFAULT '0.0', y REAL NOT NULL DEFAULT '0.0' );";
			str += "CREATE TABLE IF NOT EXISTS building ( bid INTEGER NOT NULL PRIMARY KEY, pid INTEGER NOT NULL DEFAULT '0', type INTEGER NOT NULL DEFAULT '0', health INTEGER NOT NULL DEFAULT '100', x REAL NOT NULL DEFAULT '0.0', y REAL NOT NULL DEFAULT '0.0', rot REAL NOT NULL DEFAULT '0.0' );";
			SQLExecute(str);
			m_maxPartyId = SQLGetInt("SELECT MAX(partyId) FROM player;");
			m_thread = new Thread(ThreadFunc);
			m_thread.IsBackground = true;
			m_thread.Start();
		}
	}

	private void AddColumnIfNotExists()
	{
		string text = "ALTER TABLE player ADD COLUMN partyId INTEGER NOT NULL DEFAULT '0';ALTER TABLE player ADD COLUMN partyRank INTEGER NOT NULL DEFAULT '0';";
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = string.Format("PRAGMA table_info({0})", "player");
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				while (dataReader.Read())
				{
					if ("partyId" == dataReader.GetString(dataReader.GetOrdinal("Name")))
					{
						text = string.Empty;
						break;
					}
				}
				dataReader.Close();
			}
			dbTransaction.Commit();
		}
		if (!string.IsNullOrEmpty(text))
		{
			SQLExecute(text);
		}
	}

	private void OnApplicationQuit()
	{
		if (m_thread != null)
		{
			m_thread.Abort();
		}
	}

	public int GetMaxPartyId()
	{
		return m_maxPartyId;
	}

	public static int PidToCid(int a_pid)
	{
		if (a_pid == 0)
		{
			return 0;
		}
		return (1000000000 >= a_pid) ? (a_pid + 1000000000) : a_pid;
	}

	public static int CidToPid(int a_cid)
	{
		if (a_cid == 0)
		{
			return 0;
		}
		return (1000000000 <= a_cid) ? (a_cid - 1000000000) : a_cid;
	}

	public static bool IsInventoryContainer(int a_cid)
	{
		return a_cid >= 1000000000;
	}

	public void RequestPlayer(ulong a_id)
	{
		lock (m_threadLock)
		{
			if (!m_playerRequests.Contains(a_id))
			{
				m_playerRequests.Add(a_id);
			}
		}
	}

	public void RequestParty(int a_id)
	{
		if (0 < a_id)
		{
			lock (m_threadLock)
			{
				if (!m_partyRequests.Contains(a_id))
				{
					m_partyRequests.Add(a_id);
				}
			}
		}
	}

	public void SavePlayers(ServerPlayer[] a_players, int a_count)
	{
		if (a_players != null && a_players.Length >= 1 && a_count >= 1)
		{
			lock (m_threadLock)
			{
				m_playersToWrite = new List<DatabasePlayer>(a_count);
				for (int i = 0; i < a_players.Length; i++)
				{
					if (a_players[i] != null && a_players[i].IsSpawned())
					{
						List<DatabasePlayer> playersToWrite = m_playersToWrite;
						ulong accountId = a_players[i].m_accountId;
						string a_name = saveStr(a_players[i].m_name);
						int pid = a_players[i].m_pid;
						Vector3 position = a_players[i].GetPosition();
						float x = position.x;
						Vector3 position2 = a_players[i].GetPosition();
						playersToWrite.Add(new DatabasePlayer(accountId, a_name, pid, x, position2.z, (int)(a_players[i].GetHealth() + 0.5f), (int)(a_players[i].GetEnergy() + 0.5f), (int)a_players[i].GetKarma(), a_players[i].GetXp(), a_players[i].GetConditions(), a_players[i].m_gold, a_players[i].m_partyId, a_players[i].m_partyRank));
					}
				}
			}
		}
	}

	public void SavePlayer(ServerPlayer a_player, bool a_justUpdateParty = false)
	{
		if (a_player != null)
		{
			lock (m_threadLock)
			{
				List<DatabasePlayer> list = (!a_justUpdateParty) ? m_playersToWrite : m_partyPlayersToWrite;
				ulong accountId = a_player.m_accountId;
				string a_name = saveStr(a_player.m_name);
				int pid = a_player.m_pid;
				Vector3 position = a_player.GetPosition();
				float x = position.x;
				Vector3 position2 = a_player.GetPosition();
				list.Add(new DatabasePlayer(accountId, a_name, pid, x, position2.z, (int)(a_player.GetHealth() + 0.5f), (int)(a_player.GetEnergy() + 0.5f), (int)a_player.GetKarma(), a_player.GetXp(), a_player.GetConditions(), a_player.m_gold, a_player.m_partyId, a_player.m_partyRank));
			}
		}
	}

	public void SavePartyPlayer(DatabasePlayer a_player)
	{
		if (a_player.aid != 0L)
		{
			lock (m_threadLock)
			{
				m_partyPlayersToWrite.Add(a_player);
			}
		}
	}

	public DatabasePlayer[] PopRequestedPlayers()
	{
		DatabasePlayer[] result = null;
		lock (m_threadLock)
		{
			if (0 >= m_playerAnswers.Count)
			{
				return result;
			}
			result = m_playerAnswers.ToArray();
			m_playerAnswers.Clear();
			return result;
		}
	}

	public DatabasePlayer[] PopRequestedParty()
	{
		DatabasePlayer[] result = null;
		lock (m_threadLock)
		{
			if (0 >= m_partyAnswers.Count)
			{
				return result;
			}
			result = m_partyAnswers.ToArray();
			m_partyAnswers.Clear();
			return result;
		}
	}

	public void RequestContainer(int a_cid)
	{
		lock (m_threadLock)
		{
			m_containerRequests.Add(a_cid);
		}
	}

	public void ClearInventory(int a_pid)
	{
		lock (m_threadLock)
		{
			m_inventoryClear.Add(a_pid);
		}
	}

	public void RequestHiddenItems(Vector3 a_pos)
	{
		lock (m_threadLock)
		{
			m_hiddenItemsRequests.Add(a_pos);
		}
	}

	public void SaveItem(DatabaseItem a_item)
	{
		lock (m_threadLock)
		{
			m_itemsToWrite.Add(a_item);
		}
	}

	public void RequestBuildings()
	{
		lock (m_threadLock)
		{
			m_requestBuildings = true;
		}
	}

	public void SaveBuilding(DatabaseBuilding a_building)
	{
		lock (m_threadLock)
		{
			m_buildingsToWrite.Add(a_building);
		}
	}

	public DatabaseBuilding[] PopRequestedBuildings()
	{
		DatabaseBuilding[] result = null;
		lock (m_threadLock)
		{
			if (0 >= m_buildingAnswers.Count)
			{
				return result;
			}
			result = m_buildingAnswers.ToArray();
			m_buildingAnswers.Clear();
			return result;
		}
	}

	public DatabaseItem[] PopRequestedItems()
	{
		DatabaseItem[] result = null;
		lock (m_threadLock)
		{
			if (0 >= m_itemAnswers.Count)
			{
				return result;
			}
			result = m_itemAnswers.ToArray();
			m_itemAnswers.Clear();
			return result;
		}
	}

	private void ThreadFunc()
	{
		int num = 0;
		while (true)
		{
			Thread.Sleep(25);
			num++;
			if (num % 2 == 0)
			{
				ulong[] a_ids = null;
				int num2 = 0;
				DatabasePlayer[] a_players = null;
				DatabasePlayer[] a_players2 = null;
				bool flag = false;
				lock (m_threadLock)
				{
					a_ids = m_playerRequests.ToArray();
					m_playerRequests.Clear();
					if (0 < m_partyRequests.Count)
					{
						num2 = m_partyRequests[0];
						m_partyRequests.RemoveAt(0);
					}
					a_players = m_playersToWrite.ToArray();
					m_playersToWrite.Clear();
					a_players2 = m_partyPlayersToWrite.ToArray();
					m_partyPlayersToWrite.Clear();
					flag = m_requestBuildings;
					m_requestBuildings = false;
				}
				SQLGetOrCreatePlayer(a_ids);
				SQLUpdatePlayers(a_players);
				SQLUpdatePartyPlayers(a_players2);
				if (num2 != 0)
				{
					SQLGetParty(num2);
				}
				if (flag)
				{
					SQLGetBuildings();
				}
			}
			else
			{
				Vector3[] a_hiPos = null;
				int[] a_cids = null;
				int[] a_inClearRequestees = null;
				DatabaseItem[] a_changedItems = null;
				DatabaseBuilding[] a_changedBuildings = null;
				lock (m_threadLock)
				{
					a_hiPos = m_hiddenItemsRequests.ToArray();
					m_hiddenItemsRequests.Clear();
					a_cids = m_containerRequests.ToArray();
					m_containerRequests.Clear();
					a_inClearRequestees = m_inventoryClear.ToArray();
					m_inventoryClear.Clear();
					a_changedItems = m_itemsToWrite.ToArray();
					m_itemsToWrite.Clear();
					a_changedBuildings = m_buildingsToWrite.ToArray();
					m_buildingsToWrite.Clear();
				}
				SQLClearInventory(a_inClearRequestees);
				SQLRevealItems(a_hiPos);
				SQLGetContainerItems(a_cids);
				SQLChangeItems(a_changedItems);
				SQLChangeBuildings(a_changedBuildings);
			}
		}
	}

	private void SQLGetOrCreatePlayer(ulong[] a_ids)
	{
		foreach (ulong num in a_ids)
		{
			DatabasePlayer item = new DatabasePlayer(num, string.Empty);
			using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
			{
				m_sqlCommand.CommandText = "SELECT aid, pid, x, y, health, energy, karma, xp, condition, gold, name, partyId, partyRank FROM player WHERE aid='" + num + "' LIMIT 1;";
				using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
				{
					if (dataReader.Read())
					{
						item.aid = (ulong)dataReader.GetInt64(0);
						item.pid = dataReader.GetInt32(1);
						item.x = dataReader.GetFloat(2);
						item.y = dataReader.GetFloat(3);
						item.health = dataReader.GetInt32(4);
						item.energy = dataReader.GetInt32(5);
						item.karma = dataReader.GetInt32(6);
						item.xp = dataReader.GetInt32(7);
						item.condition = dataReader.GetInt32(8);
						item.gold = dataReader.GetInt32(9);
						item.name = dataReader.GetString(10);
						item.partyId = dataReader.GetInt32(11);
						item.partyRank = dataReader.GetInt32(12);
					}
					dataReader.Close();
				}
				dbTransaction.Commit();
			}
			if (item.pid == 0)
			{
				item.pid = SQLExecuteAndGetId("INSERT INTO player (aid) VALUES('" + num + "');");
			}
			lock (m_threadLock)
			{
				m_playerAnswers.Add(item);
			}
		}
	}

	private void SQLGetParty(int a_id)
	{
		if (1 <= a_id)
		{
			DatabasePlayer item = new DatabasePlayer(0uL, string.Empty);
			using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
			{
				m_sqlCommand.CommandText = "SELECT aid, pid, name, partyId, partyRank FROM player WHERE partyId=" + a_id + ";";
				using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
				{
					while (dataReader.Read())
					{
						item.aid = (ulong)dataReader.GetInt64(0);
						item.pid = dataReader.GetInt32(1);
						item.name = dataReader.GetString(2);
						item.partyId = dataReader.GetInt32(3);
						item.partyRank = dataReader.GetInt32(4);
						lock (m_threadLock)
						{
							m_partyAnswers.Add(item);
						}
					}
					dataReader.Close();
				}
				dbTransaction.Commit();
			}
		}
	}

	private void SQLUpdatePlayers(DatabasePlayer[] a_players)
	{
		if (a_players != null && a_players.Length > 0)
		{
			string text = string.Empty;
			for (int i = 0; i < a_players.Length; i++)
			{
				DatabasePlayer databasePlayer = a_players[i];
				string text2 = text;
				text = text2 + "UPDATE player SET name='" + saveStr(databasePlayer.name) + "', x=" + databasePlayer.x + ", y=" + databasePlayer.y + ", health=" + databasePlayer.health + ", energy=" + databasePlayer.energy + ", karma=" + databasePlayer.karma + ", xp=" + databasePlayer.xp + ", condition=" + databasePlayer.condition + ", gold=" + databasePlayer.gold + ", partyId=" + databasePlayer.partyId + ", partyRank=" + databasePlayer.partyRank + " WHERE pid=" + databasePlayer.pid + ";";
			}
			SQLExecute(text);
		}
	}

	private void SQLUpdatePartyPlayers(DatabasePlayer[] a_players)
	{
		if (a_players != null && a_players.Length > 0)
		{
			string text = string.Empty;
			for (int i = 0; i < a_players.Length; i++)
			{
				DatabasePlayer databasePlayer = a_players[i];
				string text2 = text;
				text = text2 + "UPDATE player SET partyId=" + databasePlayer.partyId + ", partyRank=" + databasePlayer.partyRank + " WHERE pid=" + databasePlayer.pid + ";";
			}
			SQLExecute(text);
		}
	}

	private void SQLClearInventory(int[] a_inClearRequestees)
	{
		int num = 0;
		string text = string.Empty;
		foreach (int a_pid in a_inClearRequestees)
		{
			num = PidToCid(a_pid);
			string text2 = text;
			text = text2 + "DELETE FROM item WHERE cid=" + num + ";";
		}
		if (text.Length > 1)
		{
			SQLExecute(text);
		}
	}

	private void SQLRevealItems(Vector3[] a_hiPos)
	{
		float num = 0.5f;
		for (int i = 0; i < a_hiPos.Length; i++)
		{
			Vector3 vector = a_hiPos[i];
			List<DatabaseItem> list = new List<DatabaseItem>();
			using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
			{
				m_sqlCommand.CommandText = "SELECT type, x, y, amount, iid FROM item WHERE hidden=1 AND cid=0 AND x>" + (vector.x - num) + " AND x<" + (vector.x + num) + " AND y>" + (vector.z - num) + " AND y<" + (vector.z + num) + ";";
				using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
				{
					while (dataReader.Read())
					{
						DatabaseItem item = new DatabaseItem(dataReader.GetInt32(0), dataReader.GetFloat(1), dataReader.GetFloat(2), dataReader.GetInt32(3));
						lock (m_threadLock)
						{
							m_itemAnswers.Add(item);
						}
						item.flag = eDbAction.delete;
						item.iid = dataReader.GetInt32(4);
						list.Add(item);
					}
					dataReader.Close();
				}
				dbTransaction.Commit();
			}
			if (list.Count > 0)
			{
				SQLChangeItems(list.ToArray());
			}
		}
	}

	private void SQLGetContainerItems(int[] a_cids)
	{
		foreach (int num in a_cids)
		{
			using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
			{
				m_sqlCommand.CommandText = "SELECT type, x, y, amount, iid FROM item WHERE cid=" + num + ";";
				using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
				{
					while (dataReader.Read())
					{
						lock (m_threadLock)
						{
							m_itemAnswers.Add(new DatabaseItem(dataReader.GetInt32(0), dataReader.GetFloat(1), dataReader.GetFloat(2), dataReader.GetInt32(3), false, num, dataReader.GetInt32(4)));
						}
					}
					dataReader.Close();
				}
				dbTransaction.Commit();
			}
		}
	}

	private void SQLChangeItems(DatabaseItem[] a_changedItems)
	{
		if (a_changedItems == null || a_changedItems.Length <= 0)
		{
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < a_changedItems.Length; i++)
		{
			DatabaseItem databaseItem = a_changedItems[i];
			if (databaseItem.flag == eDbAction.delete)
			{
				string text2 = text;
				text = text2 + "DELETE FROM item WHERE iid=" + databaseItem.iid + ";";
			}
			else if (databaseItem.flag == eDbAction.update)
			{
				string text2 = text;
				text = text2 + "UPDATE item SET cid=" + databaseItem.cid + ", hidden=" + (databaseItem.hidden ? 1 : 0) + ", type=" + databaseItem.type + ", amount=" + databaseItem.amount + ", x=" + databaseItem.x + ", y=" + databaseItem.y + " WHERE iid=" + databaseItem.iid + ";";
			}
			else if (databaseItem.flag == eDbAction.insert)
			{
				string text3 = "INSERT INTO item (cid, hidden, type, amount, x, y) VALUES(" + databaseItem.cid + ", " + (databaseItem.hidden ? 1 : 0) + ", " + databaseItem.type + ", " + databaseItem.amount + ", " + databaseItem.x + ", " + databaseItem.y + ");";
				if (databaseItem.cid > 0)
				{
					DatabaseItem item = databaseItem;
					item.iid = SQLExecuteAndGetId(text3);
					lock (m_threadLock)
					{
						m_itemAnswers.Add(item);
					}
				}
				else
				{
					text += text3;
				}
			}
		}
		if (text.Length > 1)
		{
			SQLExecute(text);
		}
	}

	private void SQLGetBuildings()
	{
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = "SELECT type, x, y, rot, pid, health FROM building;";
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				while (dataReader.Read())
				{
					lock (m_threadLock)
					{
						m_buildingAnswers.Add(new DatabaseBuilding(dataReader.GetInt32(0), dataReader.GetFloat(1), dataReader.GetFloat(2), dataReader.GetFloat(3), dataReader.GetInt32(4), dataReader.GetInt32(5)));
					}
				}
				dataReader.Close();
			}
			dbTransaction.Commit();
		}
	}

	private void SQLChangeBuildings(DatabaseBuilding[] a_changedBuildings)
	{
		if (a_changedBuildings == null || a_changedBuildings.Length <= 0)
		{
			return;
		}
		string text = string.Empty;
		for (int i = 0; i < a_changedBuildings.Length; i++)
		{
			DatabaseBuilding databaseBuilding = a_changedBuildings[i];
			string text2 = " WHERE x > (" + databaseBuilding.x + "-0.2) AND x < (" + databaseBuilding.x + "+0.2) AND y > (" + databaseBuilding.y + "-0.2) AND y < (" + databaseBuilding.y + "+0.2);";
			if (databaseBuilding.flag == eDbAction.delete)
			{
				text = text + "DELETE FROM building" + text2;
			}
			else if (databaseBuilding.flag == eDbAction.update)
			{
				string text3 = text;
				text = text3 + "UPDATE building SET pid=" + databaseBuilding.pid + ", type=" + databaseBuilding.type + ", health=" + databaseBuilding.health + ", x=" + databaseBuilding.x + ", y=" + databaseBuilding.y + ", rot=" + databaseBuilding.rot + text2;
			}
			else if (databaseBuilding.flag == eDbAction.insert)
			{
				string text3 = text;
				text = text3 + "INSERT INTO building (pid, type, health, x, y, rot) VALUES(" + databaseBuilding.pid + ", " + databaseBuilding.type + ", " + databaseBuilding.health + ", " + databaseBuilding.x + ", " + databaseBuilding.y + ", " + databaseBuilding.rot + ");";
			}
		}
		if (text.Length > 1)
		{
			SQLExecute(text);
		}
	}

	private void SQLExecute(string a_sql)
	{
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = a_sql;
			m_sqlCommand.ExecuteNonQuery();
			dbTransaction.Commit();
		}
	}

	private int SQLExecuteAndGetId(string a_sql)
	{
		int result = -1;
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = a_sql + "SELECT last_insert_rowid();";
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				if (dataReader.Read())
				{
					result = dataReader.GetInt32(0);
				}
				dataReader.Close();
			}
			dbTransaction.Commit();
			return result;
		}
	}

	private int SQLGetInt(string a_sql)
	{
		int result = -1;
		using (IDbTransaction dbTransaction = m_sqlConnection.BeginTransaction())
		{
			m_sqlCommand.CommandText = a_sql;
			using (IDataReader dataReader = m_sqlCommand.ExecuteReader())
			{
				if (dataReader.Read())
				{
					result = dataReader.GetInt32(0);
				}
				dataReader.Close();
			}
			dbTransaction.Commit();
			return result;
		}
	}

	private string saveStr(string a_str)
	{
		return a_str.Replace("'", string.Empty);
	}
}
