using UnityEngine;

public class ItemGUI : MonoBehaviour
{
	public string m_btnSplitName = "todo";

	public string m_btnEatName = "todo";

	public GameObject m_guiBlock;

	public GameObject m_guiEatSplit;

	public GameObject m_guiEat;

	public GameObject m_guiSplit;

	private GUI3dMaster m_guimaster;

	private RemoteItem m_item;

	private ClientInput m_input;

	private float m_lastShowTime;

	public void Show(RemoteItem a_item, Vector3 a_pos)
	{
		m_item = a_item;
		base.transform.position = a_pos;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.z = 4.5f;
		base.transform.localPosition = localPosition;
		ShowGui(Items.IsEatable(m_item.m_type) || Items.IsMedicine(m_item.m_type), Items.IsStackable(m_item.m_type) && 1 < m_item.m_amountOrCond);
		m_lastShowTime = Time.time;
	}

	public void Hide()
	{
		ShowGui(false, false);
	}

	private void ShowGui(bool a_consumable, bool a_splitable)
	{
		m_guiBlock.SetActive(a_consumable || a_splitable);
		m_guiEatSplit.SetActive(a_consumable && a_splitable);
		m_guiEat.SetActive(a_consumable && !a_splitable);
		m_guiSplit.SetActive(!a_consumable && a_splitable);
	}

	private void Start()
	{
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		m_input = (ClientInput)Object.FindObjectOfType(typeof(ClientInput));
	}

	private void LateUpdate()
	{
		if (null != m_guimaster)
		{
			string clickedButtonName = m_guimaster.GetClickedButtonName();
			if (string.Empty != clickedButtonName)
			{
				if (m_btnSplitName == clickedButtonName)
				{
					m_input.SplitItem(m_item);
					Hide();
				}
				else if (m_btnEatName == clickedButtonName)
				{
					m_input.ConsumeItem(m_item);
					Hide();
				}
			}
		}
		if (Input.anyKeyDown && Time.time > m_lastShowTime + 0.5f)
		{
			Hide();
		}
	}
}
