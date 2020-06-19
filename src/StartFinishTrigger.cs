using UnityEngine;

public class StartFinishTrigger : MonoBehaviour
{
	public GUIText m_timesDisplay;

	private float m_startTime;

	private void OnTriggerEnter()
	{
		if (m_startTime != 0f)
		{
			m_timesDisplay.text = m_timesDisplay.text + "\n" + (Time.time - m_startTime);
		}
		m_startTime = Time.time;
	}

	private void Update()
	{
	}
}
