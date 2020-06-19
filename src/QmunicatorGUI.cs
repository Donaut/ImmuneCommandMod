using System;
using UnityEngine;
using UnityEngine.UI;

public class QmunicatorGUI : MonoBehaviour
{
	public Transform m_guiRoot;

	public GameObject m_guiComBtn;

	public GameObject m_guiCloseBtn;

	public GameObject m_guiQuitBtn;

	public GameObject[] m_guis;

	public Renderer[] m_btnActiveRenderer;

	public TextMesh m_txtClock;

	public TextMesh m_txtPlayerCount;

	public TextMesh m_txtHealth;

	public TextMesh m_txtEnergy;

	public TextMesh m_txtRank;

	public TextMesh m_txtKarma;

	public Transform m_barHealth;

	public Transform m_barEnergy;

	public Transform m_barRank;

	public Transform m_barKarma;

	public TextMesh m_helpText;

	public Toggle m_hintsToggle;

	public Slider m_volumeSlider;

	public float m_animSpeed = 2f;

	private eActiveApp m_activeApp = eActiveApp.home;

	private GUI3dMaster m_guimaster;

	private LidClient m_client;

	private DayNightCycle m_dayNightCycle;

	private float m_sinOffset;

	private float m_curSin = -1f;

	private float m_zOffset;

	private void Start()
	{
		m_guimaster = (GUI3dMaster)UnityEngine.Object.FindObjectOfType(typeof(GUI3dMaster));
		m_client = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
		m_dayNightCycle = (DayNightCycle)UnityEngine.Object.FindObjectOfType(typeof(DayNightCycle));
		Vector3 localPosition = m_guiRoot.localPosition;
		m_zOffset = localPosition.z;
		if (null != m_volumeSlider)
		{
			m_volumeSlider.value = PlayerPrefs.GetFloat("prefVolume", 1f);
		}
		if (null != m_hintsToggle)
		{
			m_hintsToggle.isOn = ((PlayerPrefs.GetInt("prefHints", 1) == 1) ? true : false);
		}
		ActivateGui(m_activeApp);
	}

	private void SetVisible(bool a_visible)
	{
		if ((a_visible ^ IsActive()) && m_curSin == -1f)
		{
			m_sinOffset = ((!a_visible) ? 0.5f : 1.5f);
			m_curSin = m_sinOffset;
		}
	}

	private void ActivateGui(eActiveApp a_app)
	{
		m_activeApp = a_app;
		int num = (int)(a_app - 1);
		for (int i = 0; i < m_guis.Length; i++)
		{
			m_guis[i].SetActive(num == i);
			if (i < m_btnActiveRenderer.Length && null != m_btnActiveRenderer[i])
			{
				m_btnActiveRenderer[i].enabled = (num == i);
			}
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		if (m_curSin > -1f)
		{
			Vector3 b = Vector3.forward * m_zOffset;
			m_guiRoot.localPosition = Vector3.up * ((FastSin.Get(m_curSin * (float)Math.PI) - 1f) * 0.5f) + b;
			m_curSin += deltaTime * m_animSpeed;
			if (m_curSin > m_sinOffset + 1f)
			{
				m_curSin = -1f;
				Transform guiRoot = m_guiRoot;
				Vector3 up = Vector3.up;
				Vector3 localPosition = m_guiRoot.localPosition;
				guiRoot.localPosition = up * ((!(localPosition.y < -0.5f)) ? 0f : (-1f)) + b;
			}
		}
		eActiveApp eActiveApp = eActiveApp.none;
		if (Input.GetButtonDown("Communicator"))
		{
			SetVisible(!IsActive());
		}
		else if (Input.GetButtonDown("Inventory") || Input.GetButtonDown("Exit"))
		{
			SetVisible(false);
		}
		else if (Input.GetButtonDown("Help"))
		{
			eActiveApp = eActiveApp.help;
		}
		else if (Input.GetButtonDown("Crafting"))
		{
			eActiveApp = eActiveApp.crafting;
		}
		else if (Input.GetButtonDown("Global Chat"))
		{
			eActiveApp = eActiveApp.chat;
		}
		else if (Input.GetButtonDown("Map"))
		{
			eActiveApp = eActiveApp.maps;
		}
		if (eActiveApp != 0)
		{
			if (m_activeApp == eActiveApp || !IsActive())
			{
				SetVisible(!IsActive());
			}
			ActivateGui(eActiveApp);
		}
		if (IsActive())
		{
			eActiveApp activeApp = m_activeApp;
			if (activeApp == eActiveApp.home)
			{
				UpdateHomeApp();
			}
			m_txtClock.text = m_dayNightCycle.GetTime();
			if (null != m_client)
			{
				m_txtPlayerCount.text = LNG.Get("PLAYERCOUNT") + ": " + m_client.GetPlayerCount();
			}
		}
	}

	private void UpdateHomeApp()
	{
		if (null != m_client)
		{
			m_txtHealth.text = ((int)m_client.GetHealth()).ToString();
			m_barHealth.localScale = new Vector3(m_client.GetHealth() * 0.01f, 1f, 1f);
			m_txtEnergy.text = ((int)m_client.GetEnergy()).ToString();
			m_barEnergy.localScale = new Vector3(m_client.GetEnergy() * 0.01f, 1f, 1f);
			m_txtRank.text = ((int)(m_client.GetRankProgress() * 100f)).ToString();
			m_barRank.localScale = new Vector3(m_client.GetRankProgress(), 1f, 1f);
			m_txtKarma.text = ((int)(m_client.GetKarma() * 0.50001f)).ToString();
			m_barKarma.localScale = new Vector3(m_client.GetKarma() / 200f, 1f, 1f);
		}
	}

	private void LateUpdate()
	{
		if (Time.timeSinceLevelLoad < 1f || !(null != m_guimaster))
		{
			return;
		}
		string clickedButtonName = m_guimaster.GetClickedButtonName();
		if (!(string.Empty != clickedButtonName))
		{
			return;
		}
		if (IsActive())
		{
			if (clickedButtonName.Length == 1)
			{
				try
				{
					ActivateGui((eActiveApp)int.Parse(clickedButtonName));
				}
				catch (Exception message)
				{
					Debug.Log(message);
				}
			}
			else if (clickedButtonName.StartsWith("HELP_"))
			{
				m_helpText.text = LNG.Get(clickedButtonName + "_TEXT");
			}
			else if (null != m_guiCloseBtn && m_guiCloseBtn.name == clickedButtonName)
			{
				SetVisible(false);
			}
			else if (null != m_guiQuitBtn && m_guiQuitBtn.name == clickedButtonName)
			{
				QuitGameGUI quitGameGUI = (QuitGameGUI)UnityEngine.Object.FindObjectOfType(typeof(QuitGameGUI));
				if (null != quitGameGUI)
				{
					quitGameGUI.ShowGui(true);
				}
			}
		}
		else if (null != m_guiComBtn && m_guiComBtn.name == clickedButtonName)
		{
			SetVisible(true);
		}
	}

	public bool IsActive(bool a_ignoreAnimation = true)
	{
		int result;
		if (a_ignoreAnimation)
		{
			Vector3 localPosition = m_guiRoot.localPosition;
			if (localPosition.y != -1f)
			{
				result = 1;
				goto IL_0049;
			}
		}
		if (!a_ignoreAnimation)
		{
			Vector3 localPosition2 = m_guiRoot.localPosition;
			result = ((0f == localPosition2.y) ? 1 : 0);
		}
		else
		{
			result = 0;
		}
		goto IL_0049;
		IL_0049:
		return (byte)result != 0;
	}

	public void OpenCrafting()
	{
		SetVisible(true);
		ActivateGui(eActiveApp.crafting);
	}

	public void ToggleHints()
	{
		PlayerPrefs.SetInt("prefHints", m_hintsToggle.isOn ? 1 : 0);
	}

	public void SetVolume()
	{
		AudioListener.volume = m_volumeSlider.value;
		PlayerPrefs.SetFloat("prefVolume", m_volumeSlider.value);
	}

	public void SetAppearance(int a_id)
	{
		PlayerPrefs.SetInt("prefAppearance", a_id);
		m_client.SendChatMsg("/char " + a_id, true);
	}
}
