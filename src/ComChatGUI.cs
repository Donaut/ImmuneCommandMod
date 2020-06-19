using System.Collections;
using UnityEngine;

public class ComChatGUI : MonoBehaviour
{
	public TextMesh m_chatText;

	public int m_maxChatLines = 8;

	public PlayersOnlineGui m_playersOnlineGui;

	public float m_xOffset = 0.36f;

	public float m_yOffset = 0.75f;

	public float m_length = 0.47f;

	public GameObject m_unreadMsgIndicator;

	public TextMesh m_unreadMsgText;

	public GameObject m_unreadMsgIndicator2;

	public TextMesh m_unreadMsgText2;

	private Rect m_chatRect = default(Rect);

	private static Hashtable m_chatEntries = new Hashtable();

	private float m_nextChatTime;

	private int m_spamCounter;

	private string m_chatString = string.Empty;

	private string m_chatAsStr = string.Empty;

	private int m_unreadMessages;

	private LidClient m_client;

	private QmunicatorGUI m_qmunicator;

	private bool m_isActive;

	private void Start()
	{
		float num = 1.77777779f / ((float)Screen.width / (float)Screen.height);
		float num2 = ((float)Screen.width / (float)Screen.height / 1.77777779f + 1f) * 0.5f;
		int num3 = (int)((float)Screen.width * m_length * num);
		int num4 = 25;
		int num5 = (int)((float)Screen.width * m_xOffset * num2);
		int num6 = (int)((float)Screen.height * m_yOffset);
		m_chatRect = new Rect(num5, num6, num3, num4);
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_qmunicator = GetComponent<QmunicatorGUI>();
		m_unreadMsgIndicator.SetActive(false);
		m_unreadMsgIndicator2.SetActive(false);
	}

	private void OnGUI()
	{
		m_isActive = m_chatText.gameObject.activeInHierarchy;
		if (!m_qmunicator.IsActive(false) || !m_isActive)
		{
			return;
		}
		if (m_unreadMsgIndicator.activeSelf)
		{
			m_unreadMessages = 0;
			m_unreadMsgIndicator.SetActive(false);
			m_unreadMsgIndicator2.SetActive(false);
		}
		GUI.SetNextControlName("chatInputCom");
		m_chatString = GUI.TextField(m_chatRect, m_chatString, 100);
		if (Event.current.type != EventType.KeyUp)
		{
			return;
		}
		if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
		{
			m_chatString = m_chatString.Replace("\\", string.Empty);
			if (m_chatString.Length > 0)
			{
				if (!IsSpam(m_chatString))
				{
					if (null != m_client)
					{
						m_client.SendChatMsg(m_chatString, false);
					}
					else
					{
						AddString(m_chatString);
					}
				}
				else
				{
					AddString("System§ " + LNG.Get("ANTI_SPAM_CHAT"));
				}
				m_chatString = string.Empty;
			}
			else
			{
				GUI.FocusControl((!("chatInputCom" == GUI.GetNameOfFocusedControl())) ? "chatInputCom" : string.Empty);
			}
			Event.current.Use();
		}
		if (Event.current.keyCode == KeyCode.Escape)
		{
			GUI.FocusControl(string.Empty);
			m_chatString = string.Empty;
			Event.current.Use();
		}
	}

	private bool IsSpam(string a_str)
	{
		bool flag = Time.time < m_nextChatTime;
		if (!flag)
		{
			Hashtable hashtable = new Hashtable();
			foreach (DictionaryEntry chatEntry in m_chatEntries)
			{
				float num = (float)chatEntry.Key;
				if (Time.time - num < 10f)
				{
					hashtable.Add(chatEntry.Key, chatEntry.Value);
				}
			}
			m_chatEntries = hashtable;
			if (4 < m_chatEntries.Count)
			{
				m_spamCounter++;
				flag = true;
				m_nextChatTime = Time.time + 10f * (float)m_spamCounter;
			}
			else
			{
				m_chatEntries.Add(Time.time, a_str);
			}
		}
		return flag;
	}

	private void DeleteOldestChatEntry()
	{
		if (m_chatAsStr.Length > 0)
		{
			int num = m_chatAsStr.IndexOf('\n');
			if (num == -1)
			{
				m_chatAsStr = string.Empty;
			}
			else
			{
				m_chatAsStr = m_chatAsStr.Substring(m_chatAsStr.IndexOf('\n') + 1);
			}
			if (null != m_chatText)
			{
				m_chatText.text = m_chatAsStr;
			}
		}
	}

	public void AddString(string a_str)
	{
		a_str = CropStr(a_str);
		string text = string.Empty;
		string[] array = a_str.Split('§');
		if (0 >= array.Length || m_playersOnlineGui.IsMuted(array[0]))
		{
			return;
		}
		if (1 < array.Length)
		{
			for (int i = 0; i < array.Length; i++)
			{
				text = ((i != 0) ? (text + array[i]) : (text + "<color=\"#aaaaaa\">" + array[i] + ":</color>"));
			}
		}
		else
		{
			text = a_str;
		}
		m_chatAsStr = m_chatAsStr + "\n" + text;
		while (m_chatAsStr.Split('\n').Length - 1 > m_maxChatLines)
		{
			DeleteOldestChatEntry();
		}
		if (null != m_chatText)
		{
			m_chatText.text = m_chatAsStr;
		}
		if (!m_qmunicator.IsActive(false) || !m_isActive)
		{
			m_unreadMsgIndicator.SetActive(true);
			m_unreadMsgIndicator2.SetActive(true);
			m_unreadMessages = Mathf.Min(m_unreadMessages + 1, 99);
			m_unreadMsgText.text = m_unreadMessages.ToString();
			m_unreadMsgText2.text = m_unreadMessages.ToString();
		}
		if (null != base.audio && m_unreadMessages == 1)
		{
			base.audio.Play();
		}
	}

	private string CropStr(string a_text, int a_newLineAfter = 55)
	{
		int num = a_newLineAfter / 2;
		string text = a_text;
		if (text.Length > a_newLineAfter)
		{
			string[] array = text.Split('\n');
			text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = string.Empty;
				string[] array2 = array[i].Split(' ');
				for (int j = 0; j < array2.Length; j++)
				{
					string text3 = array2[j];
					if (num < text3.Length)
					{
						text3 = text3.Substring(0, num);
					}
					text2 = text2 + text3 + " ";
					int num2 = (j + 1 < array2.Length) ? (array2[j + 1].Length / 2) : 0;
					if (text2.Length + num2 > a_newLineAfter)
					{
						text = text + text2 + ((j + 1 >= array2.Length) ? string.Empty : "\n");
						text2 = string.Empty;
					}
				}
				text = text + text2 + "\n";
			}
		}
		if (text.EndsWith("\n"))
		{
			text = text.Substring(0, text.Length - 1);
		}
		return text;
	}
}
