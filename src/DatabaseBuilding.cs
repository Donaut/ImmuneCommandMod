using UnityEngine;

public struct DatabaseBuilding
{
	public int pid;

	public int type;

	public int health;

	public float x;

	public float y;

	public float rot;

	public eDbAction flag;

	public DatabaseBuilding(int a_type, float a_x = 0f, float a_y = 0f, float a_rot = 0f, int a_pid = 0, int a_health = 100)
	{
		type = a_type;
		x = a_x;
		y = a_y;
		rot = a_rot;
		pid = a_pid;
		health = a_health;
		flag = eDbAction.none;
	}

	public Vector3 GetPos()
	{
		return new Vector3(x, 0f, y);
	}
}
