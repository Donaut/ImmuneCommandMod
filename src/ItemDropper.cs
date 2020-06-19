using UnityEngine;

public class ItemDropper : MonoBehaviour
{
	private float m_dropInterval = 900f;

	private float m_nextDropTime;

	private LidServer m_server;

	private void Start()
	{
		m_server = Object.FindObjectOfType<LidServer>();
	}

	private void SetAggressor(Transform a_aggressor)
	{
		if (!Global.isServer || !(null != m_server) || !(null != a_aggressor))
		{
			return;
		}
		bool flag = false;
		if (Time.time > m_nextDropTime)
		{
			int num = -1;
			int a_amount = 1;
			int num2 = Random.Range(0, 8);
			if (num2 == 0)
			{
				num = Items.GetRandomType(90f);
				a_amount = ((!Items.HasCondition(num)) ? 1 : Random.Range(1, 20));
			}
			else if (3 > num2)
			{
				num = Random.Range(130, 134);
			}
			if (num != -1 && Items.IsValid(num))
			{
				m_server.CreateFreeWorldItem(num, a_amount, a_aggressor.position);
				flag = true;
			}
			m_nextDropTime = Time.time + Random.Range(m_dropInterval * 0.6f, m_dropInterval * 1.4f);
		}
		if (!flag)
		{
			ServerPlayer playerByTransform = m_server.GetPlayerByTransform(a_aggressor);
			if (playerByTransform != null)
			{
				m_server.SendSpecialEvent(playerByTransform, eSpecialEvent.empty);
			}
		}
	}
}
