using UnityEngine;

public class GUI3dMaster : MonoBehaviour
{
	public int m_designResX = 9;

	public int m_designResY = 16;

	private int m_guiLayer = 288;

	private string m_buttonClickedName = string.Empty;

	private string m_buttonRightClickedName = string.Empty;

	private void Update()
	{
		m_buttonClickedName = string.Empty;
		m_buttonRightClickedName = string.Empty;
		Vector3 vector = (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) ? Vector3.zero : Input.mousePosition;
		if (!(Vector3.zero != vector))
		{
			return;
		}
		Ray ray = Camera.main.ScreenPointToRay(vector);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, 100f, m_guiLayer))
		{
			if (Input.GetMouseButtonDown(0))
			{
				m_buttonClickedName = hitInfo.transform.name;
			}
			if (Input.GetMouseButtonDown(1))
			{
				m_buttonRightClickedName = hitInfo.transform.name;
			}
			hitInfo.transform.SendMessage("Animate", SendMessageOptions.DontRequireReceiver);
			if (null != base.audio)
			{
				base.audio.Play();
			}
		}
	}

	public string GetClickedButtonName()
	{
		return m_buttonClickedName;
	}

	public string GetRightClickedButtonName()
	{
		return m_buttonRightClickedName;
	}

	public float GetRatioMultiplier()
	{
		float num = (float)m_designResX / (float)m_designResY;
		float num2 = (float)Screen.width / (float)Screen.height;
		return num2 / num;
	}
}
