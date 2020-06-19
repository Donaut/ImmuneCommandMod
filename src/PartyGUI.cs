using UnityEngine;

public class PartyGUI : MonoBehaviour
{
	public TextMesh m_txtNames;

	public TextMesh m_txtRanks;

	public TextMesh m_txtDescription;

	public GameObject[] m_btnRank;

	public GameObject[] m_btnKick;

	public PlayersOnlineGui m_playerListGui;

	private ulong[] m_aid;

	private LidClient m_client;

	private GUI3dMaster m_guimaster;

	private void Start()
	{
		m_guimaster = Object.FindObjectOfType<GUI3dMaster>();
		SetParty(null);
	}

	private void OnEnable()
	{
		if (null == m_client)
		{
			m_client = Object.FindObjectOfType<LidClient>();
		}
	}

	private void LateUpdate()
	{
		if (!(null != m_guimaster))
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		string rightClickedButtonName = m_guimaster.GetRightClickedButtonName();
		string text = (!(string.Empty != clickedButtonName)) ? rightClickedButtonName : clickedButtonName;
		if (!(string.Empty != text))
		{
			return;
		}
		if ("btn_invite" == text)
		{
			if (m_aid != null && m_aid.Length == 5)
			{
				m_client.ShowPartyFullPopup();
				return;
			}
			base.gameObject.SetActive(false);
			m_playerListGui.OpenAsPlayerInvitation();
			return;
		}
		for (int i = 0; i < m_btnRank.Length; i++)
		{
			if (text == m_btnRank[i].name)
			{
				m_client.SendPartyRequest(ePartyControl.prodemote, m_aid[i]);
			}
			else if (text == m_btnKick[i].name)
			{
				m_client.SendPartyRequest(ePartyControl.kick, m_aid[i]);
			}
		}
	}

	public void SetParty(DatabasePlayer[] a_party)
	{
		m_txtDescription.text = ((a_party != null) ? string.Empty : LNG.Get("PARTY_DESCRIPTION"));
		m_txtNames.text = string.Empty;
		m_txtRanks.text = string.Empty;
		bool flag = false;
		int num = (a_party != null) ? Mathf.Min(a_party.Length, 5) : 0;
		string empty = string.Empty;
		m_aid = ((num != 0) ? new ulong[num] : null);
		for (int i = 0; i < num; i++)
		{
			empty = a_party[i].name;
			if (8 < empty.Length)
			{
				empty = empty.Substring(0, 7) + "...";
			}
			TextMesh txtNames = m_txtNames;
			txtNames.text = txtNames.text + empty + "\n";
			TextMesh txtRanks = m_txtRanks;
			txtRanks.text = txtRanks.text + ((a_party[i].partyRank != 1) ? "Member" : "Admin") + "\n";
			m_aid[i] = a_party[i].aid;
			if (m_client.GetSteamId() == m_aid[i] && a_party[i].partyRank == 1)
			{
				flag = true;
			}
		}
		for (int j = 0; j < m_btnRank.Length; j++)
		{
			m_btnRank[j].SetActive(j < num && flag);
			m_btnKick[j].SetActive(j < num && (flag || m_client.GetSteamId() == m_aid[j]));
		}
	}
}
