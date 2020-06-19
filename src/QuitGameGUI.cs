using UnityEngine;

public class QuitGameGUI : MonoBehaviour
{
	public GameObject m_guiParent;

	public bool m_openWithEsc = true;

	[HideInInspector]
	public float m_cantLogoutTime = -99999f;

	private GUI3dMaster m_guimaster;

	private MessageBarGUI m_msgBar;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_msgBar = Object.FindObjectOfType<MessageBarGUI>();
	}

	private void Update()
	{
		if (m_openWithEsc && Input.GetKeyDown(KeyCode.Escape))
		{
			ShowGui(true);
		}
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster) || !m_guiParent.activeSelf)
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		if (!(string.Empty != clickedButtonName))
		{
			return;
		}
		if ("btn_quit_yes" == clickedButtonName)
		{
			if (!IsBattleLogging())
			{
				TheOneAndOnly theOneAndOnly = (TheOneAndOnly)Object.FindObjectOfType(typeof(TheOneAndOnly));
				Object.DestroyImmediate(theOneAndOnly.gameObject);
				Application.LoadLevel(0);
			}
		}
		else if ("btn_quit_no" == clickedButtonName)
		{
			ShowGui(false);
		}
	}

	private bool IsBattleLogging()
	{
		bool flag = m_cantLogoutTime > Time.time;
		if (flag && null != m_msgBar)
		{
			int num = (int)(m_cantLogoutTime - Time.time + 0.5f);
			m_msgBar.DisplayMessage(LNG.Get("CANT_LOGOUT_DURING_BATTLE").Replace("%1", num.ToString()), 1000);
		}
		return flag;
	}

	public void ShowGui(bool a_show)
	{
		m_guiParent.SetActive(a_show);
	}
}
