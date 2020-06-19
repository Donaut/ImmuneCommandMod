using Steamworks;
using UnityEngine;

public class RemoteCharacter : MonoBehaviour
{
	public GameObject[] m_prefabs;

	public GameObject[] m_deadPrefabs;

	public GameObject m_xpEffect;

	public GameObject m_lvlUpEffect;

	public GameObject m_missionCompleteEffect;

	public GameObject m_bloodEffectPrefab;

	public GameObject m_sparksEffectPrefab;

	public GameObject m_damageHealIndicatorPrefab;

	public GameObject m_weaponBreakEffect;

	public GameObject m_textEffect;

	public bool m_doOwnPlayerPrediciton;

	public float m_interpPercent = 0.9f;

	public GameObject m_labelPrefab;

	public GameObject m_labelChatPrefab;

	public Vector3 m_labelOffset = new Vector3(0f, 0.1f, -0.8f);

	public Vector3 m_labelChatOffset = new Vector3(0f, 0.1f, -1f);

	public float m_corpseDisappearTime = 60f;

	[HideInInspector]
	public bool m_isOwnPlayer;

	[HideInInspector]
	public eCharType m_type;

	[HideInInspector]
	public int m_id = -1;

	[HideInInspector]
	public float m_health = 100f;

	[HideInInspector]
	public float m_energy = 100f;

	[HideInInspector]
	public bool m_isSaint;

	private int m_handItem = -1;

	private int m_look = -1;

	private int m_skin = -1;

	private int m_body = -1;

	private Vector3 m_targetPos = Vector3.zero;

	private Quaternion m_targetRot = Quaternion.identity;

	private float m_interpSpeed = 4f;

	private GameObject m_avatar;

	private TextMesh m_label;

	private ChatLabel m_labelChat;

	private BodyHeadAnim m_animControl;

	private CharAnim2 m_animControl2;

	private CharSounds m_sound;

	private PopupItemGUI m_itemPopupGui;

	private QuitGameGUI m_quitGameGui;

	private bool m_visible;

	private float m_lastUpdate;

	private float m_disappearTime = 1f;

	private float m_dieTime = 10f;

	private int m_lastRank = -1;

	private SteamInventoryResult_t m_itemDropHandle;

	private bool m_waitForResult;

	public void Refresh(Vector3 a_pos, float a_rotation, CharAnim2.ePose a_anim = CharAnim2.ePose.eStand, float a_health = 100f, float a_energy = 100f)
	{
		if (RefreshStatus(a_health, a_energy))
		{
			if (!m_visible)
			{
				SwitchVisibility();
				base.transform.position = a_pos;
				base.transform.rotation = Quaternion.Euler(0f, a_rotation, 0f);
			}
			if (null != m_animControl)
			{
				m_animControl.m_isTakingAction = (CharAnim2.ePose.eAttack == a_anim);
				m_animControl.m_isSitting = (CharAnim2.ePose.eSit == a_anim);
				m_sound.enabled = !m_animControl.m_isSitting;
			}
			else if (null != m_animControl2)
			{
				m_animControl2.PlayAnimation(a_anim);
			}
			m_targetPos = a_pos;
			m_targetRot = Quaternion.Euler(0f, a_rotation, 0f);
			float num = Time.time - m_lastUpdate;
			if (num > 0f && num < 1f)
			{
				m_interpSpeed = 1f / num * m_interpPercent;
			}
			m_lastUpdate = Time.time;
		}
	}

	public bool RefreshStatus(float a_health, float a_energy)
	{
		if (m_health == 0f && a_health == 0f)
		{
			return false;
		}
		if (9999999f > a_health)
		{
			if (m_visible && m_health != a_health)
			{
				float num = a_health - m_health;
				bool flag = num < 0f;
				if (flag || 1f < num)
				{
					GameObject gameObject = (GameObject)Object.Instantiate(m_damageHealIndicatorPrefab, base.transform.position + Vector3.up * 3f, Quaternion.identity);
					TextMesh component = gameObject.GetComponent<TextMesh>();
					if (null != component)
					{
						string text = num.ToString();
						if (!flag)
						{
							text = "+" + text;
						}
						component.text = "<color=\"" + ((!flag) ? "green" : "red") + "\">" + text + "</color>";
					}
				}
				if (flag)
				{
					if (null != m_sound)
					{
						m_sound.Suffer(0f == a_health);
					}
					if (m_isOwnPlayer && 0f < a_health && null != m_quitGameGui)
					{
						m_quitGameGui.m_cantLogoutTime = Time.time + 20f;
					}
					GameObject original = (m_type != eCharType.eCar) ? m_bloodEffectPrefab : m_sparksEffectPrefab;
					Object.Instantiate(original, base.transform.position + Vector3.up * 2f, Quaternion.LookRotation(Vector3.up));
				}
			}
			if (null != m_avatar && m_health != a_health)
			{
				m_avatar.SendMessage("SetHealth", a_health, SendMessageOptions.DontRequireReceiver);
			}
			m_health = a_health;
			if (m_health == 0f)
			{
				CreateCorpse();
				if (null != m_animControl)
				{
					m_animControl.ResetAnim();
				}
				if (m_visible)
				{
					SwitchVisibility();
				}
			}
		}
		if (9999999f > a_energy)
		{
			m_energy = a_energy;
		}
		return 0f != m_health;
	}

	public bool IsVisible()
	{
		return m_visible;
	}

	public void Remove()
	{
		if (null != m_label)
		{
			Object.Destroy(m_label.gameObject);
		}
		Object.Destroy(base.gameObject);
	}

	public void Spawn(int a_onlineId, eCharType a_type, bool a_isOwnPlayer)
	{
		m_id = a_onlineId;
		m_isOwnPlayer = a_isOwnPlayer;
		m_type = a_type;
		GameObject prefab = GetPrefab();
		if (null != prefab)
		{
			if (null != m_avatar)
			{
				Object.Destroy(m_avatar);
			}
			m_avatar = (GameObject)Object.Instantiate(prefab, base.transform.position, base.transform.rotation);
			m_avatar.transform.parent = base.transform;
			m_animControl = m_avatar.GetComponent<BodyHeadAnim>();
			m_animControl2 = m_avatar.GetComponent<CharAnim2>();
			m_sound = m_avatar.GetComponent<CharSounds>();
			if (m_isOwnPlayer)
			{
				m_itemPopupGui = (PopupItemGUI)Object.FindObjectOfType(typeof(PopupItemGUI));
				m_quitGameGui = (QuitGameGUI)Object.FindObjectOfType(typeof(QuitGameGUI));
			}
			else
			{
				base.gameObject.layer = m_avatar.layer;
			}
			if (null != m_animControl)
			{
				m_animControl.Init(m_isOwnPlayer);
			}
		}
	}

	private void SetName(int a_rank, int a_karma, string a_name)
	{
		m_isSaint = (a_karma >= 199);
		if (a_name == null || a_name.Length <= 0)
		{
			return;
		}
		if (null == m_label && null != m_labelPrefab)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_labelPrefab, base.transform.position + m_labelOffset, Quaternion.identity);
			m_label = gameObject.GetComponent<TextMesh>();
			m_label.transform.parent = base.transform;
		}
		if (!(null != m_label))
		{
			return;
		}
		string text = string.Empty;
		string empty = string.Empty;
		string text2 = string.Empty;
		m_label.text = string.Empty;
		if (0 < a_rank)
		{
			switch (a_rank)
			{
			case 1:
				empty = "ffff00";
				text2 = "I";
				break;
			case 2:
				empty = "ffee00";
				text2 = "II";
				break;
			case 3:
				empty = "ffdd00";
				text2 = "III";
				break;
			case 4:
				empty = "ffcc00";
				text2 = "IV";
				break;
			case 5:
				empty = "ffbb00";
				text2 = "V";
				break;
			case 6:
				empty = "ffaa00";
				text2 = "VI";
				break;
			case 7:
				empty = "ff9900";
				text2 = "VII";
				break;
			case 8:
				empty = "ff8800";
				text2 = "VIII";
				break;
			case 9:
				empty = "ff7700";
				text2 = "IX";
				break;
			default:
				empty = "ff6600";
				text2 = "X";
				break;
			}
			text2 = "<color=\"#" + empty + "\">" + text2 + "</color> ";
		}
		if (m_isSaint)
		{
			empty = "FFD800";
			text = "<color=\"#" + empty + "\">★</color> ";
		}
		else
		{
			empty = ((int)(75f + Mathf.Clamp01(1f - (float)(a_karma - 100) * 0.01f) * 180f)).ToString("X2") + ((int)(75f + Mathf.Clamp01((float)a_karma * 0.01f) * 180f)).ToString("X2") + ((int)(75f + Mathf.Clamp01(1f - Mathf.Abs((float)(a_karma - 100) * 0.01f)) * 180f)).ToString("X2");
			if (8f >= (float)a_karma)
			{
				text = "<color=\"#ff0000\">☠</color> ";
			}
		}
		m_label.text = text + text2 + "<color=\"#" + empty + "\">" + a_name + "</color>";
	}

	public void SetChatText(string a_text)
	{
		if (a_text != null && a_text.Length > 0)
		{
			if (null == m_labelChat && null != m_labelChatPrefab)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(m_labelChatPrefab, base.transform.position + m_labelChatOffset, Quaternion.identity);
				gameObject.transform.parent = base.transform;
				m_labelChat = gameObject.GetComponent<ChatLabel>();
			}
			if (null != m_labelChat)
			{
				m_labelChat.SetText(a_text);
			}
		}
	}

	public void SetInfo(CharData a_data)
	{
		if (a_data.type != m_type)
		{
			m_type = a_data.type;
			Spawn(m_id, m_type, m_isOwnPlayer);
			m_handItem = -1;
			m_look = -1;
			m_skin = -1;
			m_body = -1;
		}
		if (null != m_animControl)
		{
			if (m_handItem != a_data.handItem)
			{
				m_animControl.ChangeHandItem(a_data.handItem);
				m_handItem = a_data.handItem;
			}
			if (m_look != a_data.look)
			{
				m_animControl.ChangeHeadItem(a_data.look);
				m_look = a_data.look;
			}
			if (m_skin != a_data.skin)
			{
				m_animControl.ChangeSkin(a_data.skin);
				m_skin = a_data.skin;
			}
			if (m_body != a_data.body)
			{
				m_animControl.ChangeBodyItem(a_data.body);
				m_body = a_data.body;
			}
		}
		if (m_lastRank != a_data.rank)
		{
			if (m_lastRank == -1)
			{
				if (m_isOwnPlayer && Global.isSteamActive)
				{
					bool flag = false;
					bool pbAchieved = true;
					int num = Mathf.Min(10, a_data.rank);
					for (int i = 1; i <= num; i++)
					{
						if (SteamUserStats.GetAchievement("ACH_IMM_RANK_" + i, out pbAchieved) && !pbAchieved)
						{
							SteamUserStats.SetAchievement("ACH_IMM_RANK_" + i);
							flag = true;
						}
					}
					if (flag)
					{
						SteamUserStats.StoreStats();
					}
				}
			}
			else if (a_data.rank > m_lastRank && a_data.rank < 11)
			{
				Object.Instantiate(m_lvlUpEffect, base.transform.position + Vector3.up * 3.5f, Quaternion.identity);
				if (m_isOwnPlayer && Global.isSteamActive)
				{
					SteamUserStats.SetAchievement("ACH_IMM_RANK_" + Mathf.Clamp(a_data.rank, 1, 10));
					SteamUserStats.StoreStats();
					PlayerPrefs.SetInt("prefHints", 0);
				}
				Debug.Log(base.name + " m_lastRank " + m_lastRank + " a_data.rank " + a_data.rank + " level up");
			}
			m_lastRank = a_data.rank;
		}
		SetName(a_data.rank, a_data.karma, a_data.name);
	}

	public void AddXp(int a_xp)
	{
		if (a_xp > 0)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_xpEffect, base.transform.position + Vector3.up * 3f, Quaternion.identity);
			TextMesh component = gameObject.GetComponent<TextMesh>();
			if (null != component)
			{
				component.text = a_xp + " XP";
			}
		}
	}

	public void OnSpecialEvent(eSpecialEvent a_event)
	{
		string text = string.Empty;
		Vector3 position = Vector3.zero;
		switch (a_event)
		{
		case eSpecialEvent.itemBroke:
			Object.Instantiate(m_weaponBreakEffect, base.transform.position + Vector3.up * 1.5f + base.transform.forward, Quaternion.identity);
			break;
		case eSpecialEvent.missionComplete:
			Object.Instantiate(m_missionCompleteEffect, base.transform.position + Vector3.up * 4f, Quaternion.identity);
			if (m_isOwnPlayer && Global.isSteamActive)
			{
				m_waitForResult = SteamInventory.TriggerItemDrop(out m_itemDropHandle, (SteamItemDef_t)100);
			}
			break;
		case eSpecialEvent.empty:
		case eSpecialEvent.forbidden:
		case eSpecialEvent.fishingfail:
		case eSpecialEvent.tooManyMissions:
		case eSpecialEvent.alreadyGotMission:
		case eSpecialEvent.noAmmo:
		case eSpecialEvent.carExitsBlocked:
		case eSpecialEvent.cantHurtSaints:
		case eSpecialEvent.buildingRepaired:
			position = base.transform.position + Vector3.up * 3f;
			text = "EVENT_" + (int)a_event;
			break;
		}
		if (string.Empty != text)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(m_textEffect, position, Quaternion.identity);
			TextLNG component = gameObject.GetComponent<TextLNG>();
			component.m_lngKey = text;
		}
	}

	public bool IsNpc()
	{
		return m_type != 0 && m_type != eCharType.ePlayerFemale && eCharType.eCar != m_type;
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		float num = Time.time - m_lastUpdate;
		if (m_visible)
		{
			float num2 = deltaTime * m_interpSpeed;
			float num3 = num2;
			if (m_isOwnPlayer && m_doOwnPlayerPrediciton)
			{
				Vector3 vector = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
				if (Vector3.zero != vector)
				{
					m_targetRot = Quaternion.LookRotation(vector);
					num3 *= 0.25f;
				}
			}
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, m_targetRot, num3);
			Vector3 vector2 = Vector3.Lerp(base.transform.position, m_targetPos, num2);
			if ((null != m_animControl && !m_animControl.m_isSitting) || null != m_animControl2)
			{
				vector2.y = Util.GetTerrainHeight(vector2) - 1.5f;
				if (vector2.y < -1f)
				{
					vector2.y = -4f;
				}
			}
			base.transform.position = vector2;
			if (num > m_disappearTime)
			{
				SwitchVisibility();
			}
		}
		else if (num > m_dieTime)
		{
			Remove();
		}
		if (m_isOwnPlayer && Global.isSteamActive)
		{
			HandleDropResult();
			SteamInventory.SendItemDropHeartbeat();
		}
	}

	private void HandleDropResult()
	{
		if (!m_waitForResult || !Global.isSteamActive)
		{
			return;
		}
		bool flag = false;
		EResult resultStatus = SteamInventory.GetResultStatus(m_itemDropHandle);
		switch (resultStatus)
		{
		case EResult.k_EResultPending:
			return;
		case EResult.k_EResultOK:
		{
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(m_itemDropHandle, null, ref punOutItemsArraySize))
			{
				break;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			if (punOutItemsArraySize != 0)
			{
				SteamInventory.GetResultItems(m_itemDropHandle, array, ref punOutItemsArraySize);
				for (int i = 0; i < array.Length; i++)
				{
					if ((array[i].m_unFlags & 0x100) == 0)
					{
						m_itemPopupGui.ShowGui(true, array[i].m_iDefinition.m_SteamItemDef);
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			break;
		}
		default:
			Debug.Log("RemoteCharacter.cs: Couldn't get item drop: " + resultStatus);
			break;
		}
		SteamInventory.DestroyResult(m_itemDropHandle);
		m_waitForResult = false;
		if (flag)
		{
			GetPromoItem();
		}
	}

	private void GetPromoItem()
	{
		if (Global.isSteamActive && PlayerPrefs.GetInt("prefGotPromoItemVetHat", 0) == 0)
		{
			m_waitForResult = SteamInventory.GrantPromoItems(out m_itemDropHandle);
			PlayerPrefs.SetInt("prefGotPromoItemVetHat", 1);
		}
	}

	private void LateUpdate()
	{
		if (m_visible && null != m_label)
		{
			Vector3 b = m_labelOffset;
			if (null != m_animControl && m_animControl.m_isSitting)
			{
				b = Vector3.up * 1000f;
			}
			m_label.transform.position = base.transform.position + b;
			m_label.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
		}
		if (null != m_label && m_label.renderer.enabled != m_visible)
		{
			m_label.renderer.enabled = m_visible;
		}
	}

	private void CreateCorpse()
	{
		GameObject prefab = GetPrefab(true);
		if (null != prefab)
		{
			Vector3 b = Vector3.up * ((!(null == m_animControl) && !m_animControl.m_isSitting) ? 1f : 0.1f);
			Object.Instantiate(prefab, base.transform.position + b, base.transform.rotation);
		}
	}

	private void SwitchVisibility()
	{
		m_visible = !m_visible;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			renderer.enabled = m_visible;
		}
		if (null != base.collider)
		{
			base.collider.enabled = m_visible;
		}
		if (null != m_labelChat)
		{
			m_labelChat.SetText(string.Empty);
		}
	}

	private GameObject GetPrefab(bool a_isDead = false)
	{
		GameObject[] array = (!a_isDead) ? m_prefabs : m_deadPrefabs;
		int type = (int)m_type;
		if (type < 0 || type >= array.Length)
		{
			return null;
		}
		return array[type];
	}
}
