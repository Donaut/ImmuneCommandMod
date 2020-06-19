using UnityEngine;

public class ServerTutorial : MonoBehaviour
{
	public int m_startItemType = 108;

	public int m_foodItemType = 5;

	public Transform m_itemSpawnPos;

	public Transform m_foodSpawnPos;

	private LidServer m_server;

	private void Start()
	{
		m_server = (LidServer)Object.FindObjectOfType(typeof(LidServer));
	}

	public Vector3 StartTutorial()
	{
		float num = 3f;
		SpawnItems(2, m_startItemType, 86, m_itemSpawnPos.position, num);
		SpawnItems(1, m_foodItemType, 3, m_foodSpawnPos.position, num);
		Vector3 position = base.transform.position;
		position.x += Random.Range(0f - num, num);
		position.z += Random.Range(0f - num, num);
		position.y = 0f;
		return position;
	}

	private void SpawnItems(int a_amount, int a_itemId, int a_itemAmountOrState, Vector3 a_pos, float a_rndDist)
	{
		for (int i = 0; i < a_amount; i++)
		{
			Vector3 a_pos2 = a_pos;
			a_pos2.x += Random.Range(0f - a_rndDist, a_rndDist);
			a_pos2.z += Random.Range(0f - a_rndDist, a_rndDist);
			a_pos2.y = 0f;
			m_server.CreateFreeWorldItem(a_itemId, a_itemAmountOrState, a_pos2);
		}
	}
}
