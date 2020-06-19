using UnityEngine;

public class SQLTest : MonoBehaviour
{
	private SQLThreadManager m_sql;

	private void Start()
	{
	}

	private void Update()
	{
		DatabaseItem[] array = m_sql.PopRequestedItems();
		if (array != null)
		{
			Debug.Log("got items: " + array.Length + " time: " + Time.time);
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].cid == 0 && array[i].iid != 0)
				{
					Debug.Log(array[i].type + " " + array[i].amount);
				}
			}
		}
		DatabasePlayer[] array2 = m_sql.PopRequestedPlayers();
		if (array2 != null)
		{
			Debug.Log("got players: " + array2[0].name + " x " + array2[0].x + " time: " + Time.time);
		}
	}
}
