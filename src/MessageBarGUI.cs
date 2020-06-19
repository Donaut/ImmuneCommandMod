using UnityEngine;

public class MessageBarGUI : MonoBehaviour
{
	public GameObject m_bar;

	public TextMesh m_text;

	public float m_displayDuration = 8f;

	private float m_disappearTime;

	private int m_curPrio;

	private void Start()
	{
		DisplayMessage(LNG.Get("PRESS_H_FOR_HELP"));
	}

	private void Update()
	{
		if (Time.time > m_disappearTime && m_disappearTime > 0f)
		{
			SetVisibility(false);
			m_disappearTime = 0f;
		}
	}

	private void SetVisibility(bool a_visible)
	{
		if (null != m_bar)
		{
			m_bar.SetActive(a_visible);
		}
		if (null != m_text)
		{
			m_text.gameObject.SetActive(a_visible);
		}
	}

	public bool DisplayMessage(string a_msg, int a_prio = 100)
	{
		bool result = false;
		if (Time.time > m_disappearTime || m_curPrio <= a_prio)
		{
			SetVisibility(true);
			if (null != m_text)
			{
				m_text.text = a_msg;
			}
			m_curPrio = a_prio;
			m_disappearTime = Time.time + m_displayDuration;
			result = true;
		}
		return result;
	}
}
