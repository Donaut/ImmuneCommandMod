public struct KillXp
{
	public ServerPlayer player;

	public ServerNpc npc;

	public float xp;

	public float deletetime;

	public KillXp(ServerPlayer a_player, ServerNpc a_npc, float a_xp, float a_time)
	{
		player = a_player;
		npc = a_npc;
		xp = a_xp;
		deletetime = a_time;
	}
}
