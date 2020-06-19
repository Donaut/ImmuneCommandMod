using SimpleJSON;
using System;
using UnityEngine;

public class CaseOpeningGUI : MonoBehaviour
{
	public float m_speed = 3f;

	public float m_showResultDur = 5f;

	public GameObject[] m_displayItems;

	public TextMesh m_newItemTxt;

	public GameObject m_btnClose;

	public GameObject m_gui;

	public GameObject m_audioEffect;

	public AudioClip m_successSound;

	private TextMesh[] m_displayTexts;

	private MeshCollider[] m_displayRenderers;

	private int[] m_displayDefIds;

	private int[] m_generatorDefIds;

	private float m_slowDownRate = 0.5f;

	private float m_timeToSlowdown;

	private float m_curSpeed;

	private int m_itemDefToWin;

	private bool m_setWinningItemFlag;

	private GUI3dMaster m_guimaster;

	private LidClient m_client;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)UnityEngine.Object.FindObjectOfType(typeof(GUI3dMaster));
		m_client = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
		m_displayTexts = new TextMesh[m_displayItems.Length];
		m_displayRenderers = new MeshCollider[m_displayItems.Length];
		m_displayDefIds = new int[m_displayItems.Length];
		m_gui.SetActive(true);
		for (int i = 0; i < m_displayItems.Length; i++)
		{
			m_displayTexts[i] = m_displayItems[i].GetComponentInChildren<TextMesh>();
			m_displayRenderers[i] = m_displayItems[i].GetComponentInChildren<MeshCollider>();
		}
		m_gui.SetActive(false);
	}

	public void Showtime(int a_itemDefToWin, int a_generatorDef)
	{
		m_gui.SetActive(true);
		m_btnClose.SetActive(false);
		m_itemDefToWin = a_itemDefToWin;
		m_timeToSlowdown = Time.time + UnityEngine.Random.Range(0f, 1f);
		m_curSpeed = m_speed;
		m_newItemTxt.text = LNG.Get("STEAM_INV_NEW_ITEM") + "\n ";
		m_setWinningItemFlag = false;
		GetItemDefsFromGenerator(a_generatorDef);
		for (int i = 0; i < m_displayItems.Length; i++)
		{
			ChangeItem(i);
		}
	}

	public bool InProgress()
	{
		return m_gui.activeSelf && !m_btnClose.activeSelf;
	}

	private void GetItemDefsFromGenerator(int a_generatorDef)
	{
		JSONNode item = JsonItems.GetItem(a_generatorDef);
		if (null != item)
		{
			string text = item["bundle"];
			string[] array = text.Split(';');
			m_generatorDefIds = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('x');
				try
				{
					m_generatorDefIds[i] = int.Parse(array2[0]);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void Update()
	{
		if (m_gui.activeSelf)
		{
			float deltaTime = Time.deltaTime;
			if (m_curSpeed > 0.01f)
			{
				for (int i = 0; i < m_displayItems.Length; i++)
				{
					Vector3 localPosition = m_displayItems[i].transform.localPosition;
					m_displayItems[i].transform.localPosition += Vector3.right * deltaTime * m_curSpeed;
					Vector3 localPosition2 = m_displayItems[i].transform.localPosition;
					if (localPosition2.x > 0.085f)
					{
						m_displayItems[i].transform.localPosition -= Vector3.right * 0.6f;
						int a_newDefId = 0;
						if (m_curSpeed < 0.2f && !m_setWinningItemFlag)
						{
							a_newDefId = m_itemDefToWin;
							m_setWinningItemFlag = true;
						}
						ChangeItem(i, a_newDefId);
					}
					if (localPosition.x < 0.015f)
					{
						Vector3 localPosition3 = m_displayItems[i].transform.localPosition;
						if (0.015f < localPosition3.x && null != base.audio)
						{
							base.audio.Play();
						}
					}
				}
				if (Time.time > m_timeToSlowdown)
				{
					m_curSpeed *= 1f - deltaTime * m_slowDownRate;
				}
			}
			else if (!m_btnClose.activeSelf)
			{
				JSONNode item = JsonItems.GetItem(m_itemDefToWin);
				if (null != item)
				{
					m_newItemTxt.text = LNG.Get("STEAM_INV_NEW_ITEM") + "\n " + item["market_name"];
					if (null != m_client)
					{
						m_client.SendChatMsg(":#~" + item["market_name"], false);
					}
					else
					{
						ComChatGUI comChatGUI = UnityEngine.Object.FindObjectOfType<ComChatGUI>();
						comChatGUI.AddString(string.Concat("Ethan The just opened a case and received: \n<color=\"red\">", item["market_name"], "</color>"));
					}
				}
				GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(m_audioEffect);
				gameObject.audio.clip = m_successSound;
				gameObject.audio.volume = 0.4f;
				gameObject.audio.Play();
				m_btnClose.SetActive(true);
			}
		}
		if (Application.isEditor && Input.GetKeyDown(KeyCode.L))
		{
			Showtime(20009, 1004);
		}
	}

	private void ChangeItem(int a_index, int a_newDefId = 0)
	{
		int num = a_newDefId;
		int num2 = 0;
		UnityEngine.Random.seed = (int)(Time.time * 1000f);
		while (num == 0 && m_generatorDefIds != null && 0 < m_generatorDefIds.Length)
		{
			num = m_generatorDefIds[UnityEngine.Random.Range(0, m_generatorDefIds.Length)];
			for (int i = 0; i < m_displayDefIds.Length; i++)
			{
				if (num == m_displayDefIds[i] && 100 > num2)
				{
					num = 0;
					num2++;
					break;
				}
			}
		}
		JSONNode item = JsonItems.GetItem(num);
		if (null != item && a_index < m_displayItems.Length && num != m_displayDefIds[a_index])
		{
			m_displayDefIds[a_index] = num;
			m_displayTexts[a_index].text = string.Concat("<color=#", item["name_color"], ">", item["market_name"], "</color>");
			m_displayRenderers[a_index].renderer.material.mainTexture = Resources.Load<Texture>("inventory_steam/inventory_s_" + num);
		}
	}

	private void LateUpdate()
	{
		if (null != m_guimaster)
		{
			string clickedButtonName = m_guimaster.GetClickedButtonName();
			if (string.Empty != clickedButtonName && m_btnClose.name == clickedButtonName)
			{
				m_gui.SetActive(false);
			}
		}
	}
}
