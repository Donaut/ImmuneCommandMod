using UnityEngine;

public struct DatabaseItem
{
	public int iid;

	public int cid;

	public bool hidden;

	public int type;

	public int amount;

	public float x;

	public float y;

	public eDbAction flag;

	public float dropTime;

	public int dropPlayerId;

	public DatabaseItem(int a_type, float a_x = 0f, float a_y = 0f, int a_amount = 1, bool a_hidden = false, int a_cid = 0, int a_iid = 0)
	{
		type = a_type;
		amount = a_amount;
		hidden = a_hidden;
		iid = a_iid;
		x = a_x;
		y = a_y;
		cid = a_cid;
		dropTime = Time.time;
		dropPlayerId = 0;
		flag = eDbAction.none;
	}

	public Vector3 GetPos()
	{
		return new Vector3(x, 0f, y);
	}
}
