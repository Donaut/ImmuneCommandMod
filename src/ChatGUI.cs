using UnityEngine;

public class ChatGUI : MonoBehaviour
{
	private float m_xOffset = 0.035f;

	private float m_yOffset = 0.92f;

	private float m_length = 0.45f;

	private Rect m_chatRect = default(Rect);

	private string m_chatString = string.Empty;

	private bool m_showTextInput;

	private LidClient m_client;

	private void Start()
	{
		int num = (int)((float)Screen.width * m_length);
		int num2 = 25;
		int num3 = (int)((float)Screen.width * m_xOffset);
		int num4 = (int)((float)Screen.height * m_yOffset);
		m_chatRect = new Rect(num3, num4, num, num2);
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
	}

	private void OnGUI()
	{
		if (m_showTextInput || Time.timeSinceLevelLoad < 1f)
		{
			GUI.SetNextControlName("chatInput");
			m_chatString = GUI.TextField(m_chatRect, m_chatString, 110);
		}
		if (Event.current.type != EventType.KeyUp)
		{
			return;
		}
		if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
		{
			if (m_showTextInput)
			{
				m_chatString = m_chatString.Replace("\\", string.Empty);
				if (m_chatString.Length > 0)
				{
					if (null != m_client)
					{
						m_client.SendChatMsg(m_chatString, true);
					}
					m_chatString = string.Empty;
				}
				m_showTextInput = false;
			}
			else
			{
				GUI.FocusControl("chatInput");
				m_showTextInput = true;
			}
			Event.current.Use();
		}
		if (Event.current.keyCode == KeyCode.Escape)
		{
			m_chatString = string.Empty;
			m_showTextInput = false;
			Event.current.Use();
		}
	}
}
