using UnityEngine;

public class PopupGUI : MonoBehaviour
{
	public GameObject m_guiParent;

	public TextMesh m_caption;

	[HideInInspector]
	public bool m_saidYesFlag;

	private int m_sessionId;

	private GUI3dMaster m_guimaster;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			ShowGui(false, string.Empty);
		}
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster) || !m_guiParent.activeSelf)
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		if (string.Empty != clickedButtonName)
		{
			if ("btn_quit_yes" == clickedButtonName)
			{
				m_saidYesFlag = true;
				ShowGui(false, string.Empty);
			}
			else if ("btn_quit_no" == clickedButtonName)
			{
				ShowGui(false, string.Empty);
			}
		}
	}

	public int ShowGui(bool a_show, string a_caption = "")
	{
		if (a_show)
		{
			m_sessionId++;
			m_saidYesFlag = false;
		}
		m_guiParent.SetActive(a_show);
		m_caption.text = a_caption;
		return m_sessionId;
	}

	public bool IsActive()
	{
		return m_guiParent.activeSelf;
	}

	public int GetSessionId()
	{
		return m_sessionId;
	}
}
