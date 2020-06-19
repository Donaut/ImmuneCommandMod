using UnityEngine;

public class ActionOnClickGUI : MonoBehaviour
{
	public GameObject m_button;

	private GUI3dMaster m_guimaster;

	private LidClient m_client;

	private bool m_wasDeadFlag;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
	}

	private void Update()
	{
		if (null != m_client && m_client.GetHealth() == 0f)
		{
			m_wasDeadFlag = true;
		}
		else if (m_wasDeadFlag)
		{
			m_button.SetActive(true);
			m_wasDeadFlag = false;
		}
	}

	private void LateUpdate()
	{
		if (!(Time.timeSinceLevelLoad < 1f) && null != m_guimaster)
		{
			string clickedButtonName = m_guimaster.GetClickedButtonName();
			if (string.Empty != clickedButtonName && null != m_button && m_button.name == clickedButtonName)
			{
				m_button.SetActive(false);
			}
		}
	}
}
