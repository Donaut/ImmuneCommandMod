using UnityEngine;

public class NetStatsGUI : MonoBehaviour
{
	public GUIText m_text;

	private LidClient m_client;

	private void Start()
	{
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
	}

	private void Update()
	{
		if (!Global.isServer && null != m_text && null != m_client && m_client.enabled && m_client.GetStats() != null)
		{
			m_text.text = "ReceivedBytes: " + m_client.GetStats().ReceivedBytes + " - kbs: " + (float)(m_client.GetStats().ReceivedBytes / 1024) / Time.timeSinceLevelLoad + "\nReceivedPackets: " + m_client.GetStats().ReceivedPackets + "\nSentBytes: " + m_client.GetStats().SentBytes + " - kbs: " + (float)(m_client.GetStats().SentBytes / 1024) / Time.timeSinceLevelLoad + "\nResentMessages: " + m_client.GetStats().ResentMessages;
		}
	}
}
