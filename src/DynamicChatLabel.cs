using UnityEngine;

public class DynamicChatLabel : MonoBehaviour
{
	public GameObject m_chatLabel;

	public Vector3 m_addVector = Vector3.zero;

	public string m_lngKey = string.Empty;

	public float m_displayLabelDist = 12f;

	private float m_nextUpdateTime;

	private ChatLabel m_label;

	private LidClient m_client;

	private void Start()
	{
		if (Global.isServer)
		{
			Object.DestroyImmediate(this);
			return;
		}
		GameObject gameObject = (GameObject)Object.Instantiate(m_chatLabel, base.transform.position + m_addVector, Quaternion.Euler(55f, 0f, 0f));
		gameObject.transform.parent = base.transform;
		m_label = gameObject.GetComponent<ChatLabel>();
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
	}

	private void Update()
	{
		if (null != m_label && null != m_client && Time.time > m_nextUpdateTime)
		{
			bool flag = (base.transform.position - m_client.GetPos()).sqrMagnitude < m_displayLabelDist * m_displayLabelDist;
			m_label.SetText((!flag) ? string.Empty : LNG.Get(m_lngKey), true);
			m_nextUpdateTime = Time.time + Random.Range(0.5f, 1.5f);
		}
	}
}
