using UnityEngine;

public struct DatabasePlayer
{
	public ulong aid;

	public int pid;

	public string name;

	public float x;

	public float y;

	public int health;

	public int energy;

	public int karma;

	public int xp;

	public int condition;

	public int gold;

	public int partyId;

	public int partyRank;

	public DatabasePlayer(ulong a_aid, string a_name = "", int a_pid = 0, float a_x = 0f, float a_y = 0f, int a_h = 100, int a_e = 100, int a_k = 100, int a_xp = 0, int a_condition = 0, int a_gold = 0, int a_partyId = 0, int a_partyRank = 0)
	{
		aid = a_aid;
		pid = a_pid;
		name = a_name;
		x = a_x;
		y = a_y;
		health = a_h;
		energy = a_e;
		karma = a_k;
		xp = a_xp;
		condition = a_condition;
		gold = a_gold;
		partyId = a_partyId;
		partyRank = a_partyRank;
	}

	public Vector3 GetPos()
	{
		return new Vector3(x, 0f, y);
	}
}
