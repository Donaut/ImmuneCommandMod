public static class SQLManager
{
	public static ServerPlayer[] m_players = new ServerPlayer[50];

	public static bool SpawnPlayer(ref ServerPlayer a_player, string a_name)
	{
		SQLWorker.GetOrCreatePlayer(ref a_player, a_name);
		if (a_player != null)
		{
			for (int i = 0; i < m_players.Length; i++)
			{
				if (m_players[i] == null)
				{
					m_players[i] = a_player;
					m_players[i].m_onlineId = i;
					break;
				}
			}
		}
		return true;
	}

	public static ServerPlayer GetPlayer(int a_onlineId)
	{
		if (a_onlineId < 0 && a_onlineId > m_players.Length)
		{
			return null;
		}
		return m_players[a_onlineId];
	}

	public static void DeletePlayer(int a_onlineId)
	{
		if (a_onlineId >= 0 && a_onlineId < m_players.Length)
		{
			m_players[a_onlineId] = null;
		}
	}
}
