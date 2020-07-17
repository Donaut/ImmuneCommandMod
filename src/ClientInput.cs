using Steamworks;
using System;
using UnityEngine;

public class ClientInput : MonoBehaviour
{
	private const int c_uiLayer = 5;

	private const int c_blockInputLayer = 14;

	private const int c_groundLayer = 12;

	private const int c_playerLayer = 13;

	private const int c_npcLayer = 9;

	private const int c_itemLayer = 10;

	private const int c_containerItemLayer = 17;

	private const int c_buildingLayer = 19;

	private const int c_resourceLayer = 15;

	private const int c_mouseOverLayer = 20;

	private const int c_itemStorageLayer = 21;

	private const int c_missionLayer = 22;

	private const int c_vehicleLayer = 11;

	private const int c_moSpecialLayers = 7007776;

	private const int c_lmbSpecialLayers = 6995488;

	private const int c_rmbSpecialLayers = 665600;

	public float m_minSendIntervall = 0.1f;

	public Transform m_bullsEye;

	public Transform m_buildingHealthIndicator;

	public Transform m_walkIndicator;

	public Transform m_tooltip;

	private TextMesh m_tooltipText;

	public Transform m_tooltipHudR;

	private TextMesh m_tooltipHudRText;

	public Transform m_tooltipHudL;

	private TextMesh m_tooltipHudLText;

	public AudioClip m_soundBeverage;

	public AudioClip m_soundFood;

	public AudioClip m_soundBurp;

	private float m_hideCursorTime = 2f;

	private float m_doubleClickTime;

	private bool m_burpFlag;

	private int m_input;

	private LidClient m_client;

	private InventoryGUI m_inventory;

	private QmunicatorGUI m_communicator;

	private ItemGUI m_itemGui;

	private float m_nextInputTime;

	private PopupGUI m_popupGui;

	private int m_buySellPopupSessionId = -1;

	private int m_repairPopupSessionId = -1;

	private int m_missionPopupSessionId = -1;

	private float m_stopAttackingTime;

	private float m_rotTowardsMousePos;

	private float m_attackNoticeTime;

	private RemoteCharacter m_currentTarget;

	private Transform m_dragItem;

	private Vector3 m_startDragPos = Vector3.zero;

	private Vector3 m_lastMousePos = Vector3.zero;

	private float m_mouseOverDur;

	private Vector3 m_sendDragPos = Vector3.zero;

	private Vector3 m_sendDropPos = Vector3.zero;

	private Vector3 m_buySellPos = Vector3.zero;

	private Vector3 m_invalidPos = Vector3.one * -1f;

	private Transform m_mouseOverTransform;

	private Vector3 m_initialMouseOverScale = Vector3.one;

	private Renderer[] m_mouseOverRenderers;

	private int m_mouseOverLayer;

	private RepairingNpc[] m_repairNpcs;

	private float m_buildRot;

	private NavMeshPath m_path;

	private Vector3[] m_pathCorners;

	private int m_nextPathPoint;

	private float m_vertAxis;

	private float m_horiAxis;

	private bool m_isMoving;

	private bool m_interactionAtPathEnd;

	private float m_forceInteractionTime;

	private void Start()
	{
		m_path = new NavMeshPath();
		m_client = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
		m_inventory = (InventoryGUI)UnityEngine.Object.FindObjectOfType(typeof(InventoryGUI));
		m_communicator = (QmunicatorGUI)UnityEngine.Object.FindObjectOfType(typeof(QmunicatorGUI));
		m_itemGui = (ItemGUI)UnityEngine.Object.FindObjectOfType(typeof(ItemGUI));
		m_popupGui = (PopupGUI)UnityEngine.Object.FindObjectOfType(typeof(PopupGUI));
		m_repairNpcs = (RepairingNpc[])UnityEngine.Object.FindObjectsOfType(typeof(RepairingNpc));
		m_tooltipText = m_tooltip.GetComponentInChildren<TextMesh>();
		m_tooltipHudRText = m_tooltipHudR.GetComponentInChildren<TextMesh>();
		m_tooltipHudLText = m_tooltipHudL.GetComponentInChildren<TextMesh>();
		ResetTarget();
		m_buySellPos = m_invalidPos;
	}

	private void Update()
	{
		GetMouseInput(Time.deltaTime);
		if (null != m_client && m_client.enabled)
		{
			CalculateAxis();
			HandlePopup();
			HandleHotKeys();
			if (m_client.GetHealth() == 0f)
			{
				ResetPath();
			}
			if (Input.GetButtonDown("FindTarget") || (IsAttacking() && null == m_currentTarget))
			{
				m_currentTarget = FindTarget(m_currentTarget);
			}
			bool flag = null != m_currentTarget && m_currentTarget.IsVisible();
			bool flag2 = flag && (m_currentTarget.m_type == eCharType.ePlayer || eCharType.ePlayerFemale == m_currentTarget.m_type);
			int num = Mathf.Clamp((int)(m_rotTowardsMousePos / 360f * 255f), 0, 255);
			int a_targetIdOrAtkRot = (!flag) ? num : m_currentTarget.m_id;
			int num2 = 0;
			num2 |= (IsInteracting() ? 1 : 0);
			num2 |= (IsAttacking() ? 2 : 0);
			num2 |= (flag ? 4 : 0);
			num2 |= (flag2 ? 8 : 0);
			num2 |= (IsAttackingWithMouse() ? 16 : 0);
			num2 |= (byte)((m_vertAxis + 1f) * 100f) << 8;
			num2 |= (byte)((m_horiAxis + 1f) * 100f) << 16;
			if ((num2 != m_input || m_sendDragPos != m_sendDropPos || m_nextInputTime + 1f < Time.time) && m_nextInputTime < Time.time)
			{
				m_input = num2;
				m_client.SendInput(m_input, a_targetIdOrAtkRot, m_buildRot, m_sendDragPos, m_sendDropPos);
				m_sendDragPos = Vector3.zero;
				m_sendDropPos = Vector3.zero;
				m_nextInputTime = Time.time + m_minSendIntervall;
			}
		}
		if (null != m_currentTarget)
		{
			if (null != m_bullsEye)
			{
				float x = 0.25f * (float)(3 - (int)(m_currentTarget.m_health * 0.033f));
				m_bullsEye.renderer.material.mainTextureOffset = new Vector2(x, 0f);
				m_bullsEye.position = m_currentTarget.transform.position + Vector3.up * 0.1f;
			}
			if (!m_currentTarget.IsVisible())
			{
				ResetTarget();
			}
		}
		else if (!IsAttacking())
		{
			ResetTarget();
		}
		ScreenshotInput();
		if (m_burpFlag && !base.audio.isPlaying)
		{
			base.audio.clip = m_soundBurp;
			base.audio.Play();
			m_burpFlag = false;
		}
		if (null != m_currentTarget && m_currentTarget.m_isSaint && IsAttacking() && Time.time > m_attackNoticeTime)
		{
			m_client.GetPlayer().OnSpecialEvent(eSpecialEvent.cantHurtSaints);
			m_attackNoticeTime = Time.time + 1f;
		}
	}

	private void HandleHotKeys()
	{
		int num = -1;
		if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
		{
			num = 1;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
		{
			num = 2;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
		{
			num = 3;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
		{
			num = 4;
		}
		if (0 >= num)
		{
			return;
		}
		RemoteItem itemFromPos = m_inventory.GetItemFromPos(num, 0f);
		if (null != itemFromPos)
		{
			if (Items.IsEatable(itemFromPos.m_type) || Items.IsMedicine(itemFromPos.m_type))
			{
				ConsumeItem(itemFromPos);
				return;
			}
			m_sendDragPos = new Vector3(num, 0f);
			m_sendDropPos = Vector3.zero;
		}
	}

	private void HandlePopup()
	{
		if (m_buySellPopupSessionId == m_popupGui.GetSessionId())
		{
			if (m_invalidPos != m_buySellPos && m_popupGui.m_saidYesFlag)
			{
				m_sendDragPos = m_buySellPos;
				m_sendDropPos = Vector3.one * 252f;
				m_buySellPos = m_invalidPos;
				m_popupGui.m_saidYesFlag = false;
			}
			if (m_popupGui.IsActive() && (!m_inventory.IsVisible() || !m_inventory.IsShopActive()))
			{
				m_popupGui.ShowGui(false, string.Empty);
			}
		}
		else if (m_repairPopupSessionId == m_popupGui.GetSessionId())
		{
			if (m_popupGui.m_saidYesFlag)
			{
				m_client.SendSpecialRequest(eSpecialRequest.repairItem);
				m_popupGui.m_saidYesFlag = false;
			}
		}
		else if (m_missionPopupSessionId == m_popupGui.GetSessionId() && m_popupGui.m_saidYesFlag)
		{
			m_client.SendSpecialRequest(eSpecialRequest.acceptMission);
			m_popupGui.m_saidYesFlag = false;
		}
		if (m_popupGui.IsActive() && m_isMoving)
		{
			m_popupGui.ShowGui(false, string.Empty);
		}
	}

	private void CalculateAxis()
	{
		m_vertAxis = Input.GetAxis("Vertical");
		m_horiAxis = Input.GetAxis("Horizontal");
		if (m_pathCorners != null)
		{
			if (m_vertAxis == 0f && m_horiAxis == 0f && m_nextPathPoint < m_pathCorners.Length)
			{
				Vector3 vector = m_pathCorners[m_nextPathPoint] - m_client.GetPos();
				if (2f > vector.sqrMagnitude)
				{
					m_nextPathPoint++;
					if (m_nextPathPoint == m_pathCorners.Length && m_interactionAtPathEnd)
					{
						m_interactionAtPathEnd = false;
						m_forceInteractionTime = Time.time + 0.3f;
					}
				}
				vector = vector.normalized;
				m_vertAxis = vector.z;
				m_horiAxis = vector.x;
			}
			else if (0 < m_pathCorners.Length)
			{
				ResetPath();
			}
		}
		m_isMoving = (m_vertAxis != 0f || 0f != m_horiAxis);
	}

	private void ResetPath()
	{
		if (m_pathCorners != null && 0 < m_pathCorners.Length)
		{
			m_path.ClearCorners();
			m_pathCorners = null;
			m_nextPathPoint = 0;
			m_interactionAtPathEnd = false;
		}
	}

	private RemoteCharacter FindTarget(RemoteCharacter a_curTarget)
	{
		RemoteCharacter result = null;
		if (null != m_client)
		{
			RemoteCharacter player = m_client.GetPlayer();
			if (null != player)
			{
				ItemDef itemDef = Items.GetItemDef(m_client.GetHandItem());
				float num = Mathf.Max(itemDef.range, 5f);
				float d = num * 0.25f;
				float num2 = num * 0.75f;
				Vector3 vector = player.transform.position + player.transform.forward * d;
				RemoteCharacter remoteCharacter = m_client.GetNearestNpc(vector);
				if (null != remoteCharacter && (remoteCharacter.transform.position - vector).magnitude > num2)
				{
					remoteCharacter = null;
				}
				RemoteCharacter remoteCharacter2 = null;
				if (null != remoteCharacter2 && (remoteCharacter2.transform.position - vector).magnitude > num2)
				{
					remoteCharacter2 = null;
				}
				if ((null == remoteCharacter || remoteCharacter == a_curTarget) && player != remoteCharacter2 && null != remoteCharacter2)
				{
					result = remoteCharacter2;
				}
				else if (null != remoteCharacter)
				{
					result = remoteCharacter;
				}
			}
		}
		return result;
	}

	private void ScreenshotInput()
	{
		if (Input.GetKey(KeyCode.F11))
		{
			Application.CaptureScreenshot("Immune_screenshot_" + (int)Time.time + ".png");
		}
		else if (Input.GetKey(KeyCode.F12))
		{
			Application.CaptureScreenshot("Immune_screenshot_2x_" + (int)Time.time + ".png", 2);
		}
	}

	public float GetBuildRot()
	{
		return m_buildRot;
	}

	public bool IsAttacking()
	{
		return !m_inventory.IsVisible() && !m_communicator.IsActive() && (m_stopAttackingTime > Time.time || Input.GetButton("Attack"));
	}

	private bool IsInteracting()
	{
		bool flag = false;
		if (Input.GetButtonDown("Interact") && !m_isMoving)
		{
			for (int i = 0; i < m_repairNpcs.Length; i++)
			{
				Vector3 position = m_repairNpcs[i].transform.position;
				Vector3 pos = m_client.GetPos();
				if (!(Mathf.Abs(position.x - pos.x) < 1.4f) || !(Mathf.Abs(position.z - pos.z) < 1.4f))
				{
					continue;
				}
				RemoteItem handItem = m_inventory.GetHandItem();
				if (null != handItem)
				{
					if (Items.HasCondition(handItem.m_type) && handItem.m_amountOrCond < 100)
					{
						ItemDef itemDef = Items.GetItemDef(handItem.m_type);
						int num = (int)(1f + Items.GetValue(handItem.m_type, 100) * 0.01f * (float)(100 - handItem.m_amountOrCond));
						num = (int)((float)num * m_repairNpcs[i].m_priceMultip + 0.5f);
						if (m_client.GetGoldCount() >= num)
						{
							string a_caption = LNG.Get("REPAIR") + "\n" + LNG.Get(itemDef.ident) + "\n" + handItem.m_amountOrCond + "%\nfor " + num + " " + LNG.Get("CURRENCY") + "?";
							m_repairPopupSessionId = m_popupGui.ShowGui(true, a_caption);
						}
						else
						{
							string a_caption2 = LNG.Get("ITEMSHOP_TOO_LESS_GOLD") + "\n" + num + " " + LNG.Get("CURRENCY");
							m_popupGui.ShowGui(true, a_caption2);
						}
					}
					else
					{
						m_popupGui.ShowGui(true, LNG.Get("REPAIR_NPC_NO_NEED"));
					}
				}
				else
				{
					m_popupGui.ShowGui(true, LNG.Get("REPAIR_NPC_HOWTO"));
				}
				flag = true;
				break;
			}
			if (flag)
			{
			}
		}
		return (Input.GetButton("Interact") && !flag) || m_forceInteractionTime > Time.time;
	}

	public string GetMissionText(Mission a_mission)
	{
		string text = (a_mission.m_type != eMissiontype.eDestroy) ? LNG.Get("MISSION_PERSON_" + a_mission.m_objPerson.ToString("d")) : LNG.Get("MISSION_OBJECT_" + a_mission.m_objObject.ToString("d"));
		return LNG.Get("MISSION") + ":\n" + LNG.Get("MISSION_TYPE_" + a_mission.m_type.ToString("d")) + " " + text + "\n\n" + LNG.Get("LOCATION") + ": " + LNG.Get("MISSION_LOCATION_" + a_mission.m_location.ToString("d")) + "\n" + LNG.Get("REWARD") + ": " + a_mission.m_xpReward.ToString("d") + " XP\n";
	}

	public void ShowMissionPopup(Mission a_mission)
	{
		m_missionPopupSessionId = m_popupGui.ShowGui(true, GetMissionText(a_mission));
	}

	public bool IsAttackingWithMouse()
	{
		return !m_inventory.IsVisible() && !m_communicator.IsActive() && m_stopAttackingTime > Time.time;
	}

	public RemoteCharacter GetTarget()
	{
		return m_currentTarget;
	}

	public void SplitItem(RemoteItem a_item)
	{
		if (null != a_item && 1 < a_item.m_amountOrCond)
		{
			m_sendDragPos = m_inventory.ToWorldPos(a_item.transform.localPosition);
			m_sendDropPos = Vector3.one * 254f;
		}
	}

	public void ConsumeItem(RemoteItem a_item)
	{
		if (!(null != a_item) || (!Items.IsEatable(a_item.m_type) && !Items.IsMedicine(a_item.m_type)))
		{
			return;
		}
		m_sendDragPos = m_inventory.ToWorldPos(a_item.transform.localPosition);
		m_sendDropPos = Vector3.one * 253f;
		if (!Items.IsMedicine(a_item.m_type))
		{
			base.audio.clip = ((!Items.IsBeverage(a_item.m_type)) ? m_soundFood : m_soundBeverage);
			base.audio.Play();
			if (UnityEngine.Random.Range(0, 20) == 0)
			{
				m_burpFlag = true;
			}
		}
	}

	private void GetMouseInput(float a_deltaTime)
	{
		if (Time.timeSinceLevelLoad < 0.5f)
		{
			return;
		}
		bool flag = !m_inventory.IsVisible() && !m_communicator.IsActive() && !m_popupGui.IsActive();
		Vector3 mousePosition = Input.mousePosition;
		bool flag2 = (m_lastMousePos - mousePosition).sqrMagnitude > 4f;
		m_lastMousePos = mousePosition;
		Ray ray = Camera.main.ScreenPointToRay(mousePosition);
		if (flag2)
		{
			m_hideCursorTime = Time.time + 1f;
		}
		bool flag3 = Time.time < m_hideCursorTime;
		if (flag3 != Screen.showCursor)
		{
			Screen.showCursor = flag3;
		}
		if (Input.GetMouseButton(1) && flag)
		{
			m_buildRot += a_deltaTime * 90f;
			while (m_buildRot > 360f)
			{
				m_buildRot -= 360f;
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, 100f, 665600))
			{
				switch (hitInfo.transform.gameObject.layer)
				{
				case 17:
					if (!m_inventory.IsShopActive())
					{
						RemoteItem component2 = hitInfo.transform.GetComponent<RemoteItem>();
						if (null != component2 && m_inventory.IsVisible())
						{
							m_itemGui.Show(component2, ray.GetPoint(4.5f));
						}
					}
					break;
				case 13:
				{
					if (!flag)
					{
						break;
					}
					RemoteCharacter component = hitInfo.transform.GetComponent<RemoteCharacter>();
					if (null != component && null != m_client)
					{
						ulong steamId = m_client.GetSteamId(component.m_id);
						if (Global.isSteamActive && 0 < steamId)
						{
							SteamFriends.ActivateGameOverlayToUser("steamid", new CSteamID(steamId));
						}
					}
					break;
				}
				}
			}
		}
		else if (Input.GetMouseButtonDown(0))
		{
			if (flag)
			{
				ResetTarget();
			}
			bool flag4 = false;
			RaycastHit hitInfo2;
			if (Physics.Raycast(ray, out hitInfo2, 100f, 6995488))
			{
				switch (hitInfo2.transform.gameObject.layer)
				{
				case 9:
				case 13:
				{
					if (!flag)
					{
						break;
					}
					RemoteCharacter component3 = hitInfo2.transform.GetComponent<RemoteCharacter>();
					if (null != component3)
					{
						if (!component3.m_isOwnPlayer)
						{
							m_currentTarget = component3;
							flag4 = true;
						}
					}
					else if (flag && Input.GetAxis("Vertical") == 0f && Input.GetAxis("Horizontal") == 0f)
					{
						CalculatePath(hitInfo2.point, 12 != hitInfo2.transform.gameObject.layer);
					}
					break;
				}
				case 17:
					if (m_inventory.IsShopActive())
					{
						RemoteItem component4 = hitInfo2.transform.GetComponent<RemoteItem>();
						if (null != component4 && component4.m_type != 254 && m_inventory.IsVisible())
						{
							m_buySellPos = m_inventory.ToWorldPos(component4.transform.localPosition);
							bool flag5 = m_buySellPos.x < 6f;
							float num = (!flag5) ? m_inventory.GetShopBuyMultiplier() : m_inventory.GetShopSellMultiplier();
							string text = (!flag5) ? LNG.Get("BUY") : LNG.Get("SELL");
							int num2 = (int)(Items.GetValue(component4.m_type, component4.m_amountOrCond) * num + 0.5f);
							string text2 = (!Items.HasCondition(component4.m_type)) ? ("x " + component4.m_amountOrCond) : (component4.m_amountOrCond + "%");
							ItemDef itemDef = Items.GetItemDef(component4.m_type);
							if (flag5 || m_client.GetGoldCount() >= num2)
							{
								string a_caption = text + "\n" + LNG.Get(itemDef.ident) + "\n" + text2 + "\nfor " + num2 + " " + LNG.Get("CURRENCY") + "?";
								m_buySellPopupSessionId = m_popupGui.ShowGui(true, a_caption);
							}
							else
							{
								string a_caption2 = LNG.Get("ITEMSHOP_TOO_LESS_GOLD") + "\n" + num2 + " " + LNG.Get("CURRENCY");
								m_popupGui.ShowGui(true, a_caption2);
							}
						}
					}
					else if (Time.time < m_doubleClickTime)
					{
						m_doubleClickTime = 0f;
						if (null != hitInfo2.transform)
						{
							Vector3 a_startPos = hitInfo2.transform.localPosition;
							Vector3 a_endPos = Vector3.zero;
							if (m_inventory.DragDrop(ref a_startPos, ref a_endPos))
							{
								m_sendDragPos = a_startPos;
								m_sendDropPos = Vector3.zero;
							}
						}
					}
					else
					{
						m_dragItem = hitInfo2.transform;
						m_startDragPos = m_dragItem.localPosition;
					}
					break;
				case 10:
				case 12:
				case 21:
					if (flag && Input.GetAxis("Vertical") == 0f && Input.GetAxis("Horizontal") == 0f && null != m_client && !m_client.IsInVehicle())
					{
						Vector3 point = hitInfo2.point;
						Transform walkIndicator = m_walkIndicator;
						Vector3 point2 = hitInfo2.point;
						float x = point2.x;
						Vector3 point3 = hitInfo2.point;
						walkIndicator.position = new Vector3(x, 0.1f, point3.z);
						CalculatePath(point, 12 != hitInfo2.transform.gameObject.layer);
					}
					break;
				case 11:
				case 15:
				case 19:
				case 22:
					flag4 = flag;
					break;
				}
			}
			if (flag4)
			{
				CalculateRotTowardsMouse(mousePosition);
				m_stopAttackingTime = Time.time + 0.3f;
			}
		}
		else if (Input.GetMouseButtonUp(0) && null != m_dragItem)
		{
			Vector3 a_startPos2 = m_startDragPos;
			Vector3 a_endPos2 = m_dragItem.localPosition;
			if (m_inventory.DragDrop(ref a_startPos2, ref a_endPos2))
			{
				if (a_startPos2 != a_endPos2)
				{
					m_sendDragPos = a_startPos2;
					m_sendDropPos = a_endPos2;
				}
				else
				{
					m_dragItem.localPosition = m_startDragPos;
					m_doubleClickTime = Time.time + 0.5f;
				}
			}
			m_dragItem = null;
		}
		if (null != m_dragItem)
		{
			m_dragItem.position = ray.GetPoint(5f);
			Vector3 localPosition = m_dragItem.localPosition;
			localPosition.z = 0f;
			m_dragItem.localPosition = localPosition;
		}
		else if (!flag2)
		{
			if (m_mouseOverDur == -1f)
			{
				return;
			}
			m_mouseOverDur += a_deltaTime;
			if (!(m_mouseOverDur > 0.1f))
			{
				return;
			}
			m_buildingHealthIndicator.position = Vector3.up * 1000f;
			m_tooltip.position = Vector3.up * 1000f;
			m_tooltipHudR.position = Vector3.up * 1000f;
			m_tooltipHudR.parent = null;
			m_tooltipHudL.position = Vector3.up * 1000f;
			m_tooltipHudL.parent = null;
			if (m_mouseOverRenderers != null && m_mouseOverRenderers.Length != 0)
			{
				Renderer[] mouseOverRenderers = m_mouseOverRenderers;
				foreach (Renderer renderer in mouseOverRenderers)
				{
					if (null != renderer)
					{
						renderer.gameObject.layer = m_mouseOverLayer;
					}
				}
				m_mouseOverRenderers = null;
			}
			if (null != m_mouseOverTransform)
			{
				m_mouseOverTransform.localScale = m_initialMouseOverScale;
				m_mouseOverTransform = null;
			}
			RaycastHit hitInfo3;
			if (Physics.Raycast(ray, out hitInfo3, 100f, 7007776))
			{
				if (hitInfo3.transform.gameObject.layer == 5)
				{
					string[] array = hitInfo3.transform.gameObject.name.Split('-');
					if (array != null && 1 < array.Length)
					{
						if ("tooltip" == array[0])
						{
							m_tooltipHudRText.text = LNG.Get(array[1]);
							m_tooltipHudR.position = hitInfo3.transform.position - hitInfo3.transform.right * 0.3f;
							m_tooltipHudR.rotation = hitInfo3.transform.rotation;
							m_tooltipHudR.parent = hitInfo3.transform;
						}
						else if ("mission" == array[0])
						{
							int a_index = 0;
							try
							{
								a_index = int.Parse(array[1]);
							}
							catch (Exception ex)
							{
								Debug.LogWarning("ClientInput.cs: " + ex.ToString());
							}
							Mission mission = m_client.GetMission(a_index);
							if (mission != null)
							{
								m_tooltipHudLText.text = GetMissionText(mission) + LNG.Get("TIME_LEFT") + ": " + (int)(mission.m_dieTime / 60f) + " min";
								m_tooltipHudL.position = hitInfo3.transform.position + hitInfo3.transform.right * 0.3f;
								m_tooltipHudL.rotation = hitInfo3.transform.rotation;
								m_tooltipHudL.parent = hitInfo3.transform;
							}
						}
					}
				}
				else
				{
					m_mouseOverTransform = hitInfo3.transform;
					m_initialMouseOverScale = m_mouseOverTransform.localScale;
					if (hitInfo3.transform.gameObject.layer == 17)
					{
						if (m_inventory.IsVisible())
						{
							m_inventory.ShowInfo(hitInfo3.transform.position);
						}
					}
					else if (hitInfo3.transform.gameObject.layer == 10)
					{
						m_mouseOverTransform.localScale *= 1.33f;
					}
					else if (hitInfo3.transform.gameObject.layer == 19)
					{
						RemoteBuilding component5 = hitInfo3.transform.parent.GetComponent<RemoteBuilding>();
						if (null != m_buildingHealthIndicator && null != component5)
						{
							Vector3 b = Vector3.up * 4f;
							float x2 = 0.25f * (float)(3 - (int)(component5.m_health * 0.033f));
							m_buildingHealthIndicator.renderer.material.mainTextureOffset = new Vector2(x2, 0f);
							m_buildingHealthIndicator.position = hitInfo3.transform.position + b;
						}
					}
					else if (hitInfo3.transform.gameObject.layer == 15)
					{
						bool flag6 = "building_10" == hitInfo3.transform.gameObject.name || "building_11" == hitInfo3.transform.gameObject.name;
						Vector3 b2 = Vector3.up * ((!flag6) ? 3f : 6.5f);
						m_tooltipText.text = LNG.Get("TOOLTIP_RESOURCE");
						m_tooltip.position = hitInfo3.transform.position + b2;
					}
					else if (hitInfo3.transform.gameObject.layer == 21)
					{
						Vector3 b3 = Vector3.up * 2f;
						m_tooltipText.text = LNG.Get("TOOLTIP_ITEMSTORAGE");
						m_tooltip.position = hitInfo3.transform.position + b3;
					}
					else if (hitInfo3.transform.gameObject.layer == 9)
					{
						RemoteCharacter component6 = hitInfo3.transform.GetComponent<RemoteCharacter>();
						if (null == component6)
						{
							Vector3 b4 = Vector3.up * 3f;
							m_tooltipText.text = LNG.Get("TOOLTIP_INTERACT");
							m_tooltip.position = hitInfo3.transform.position + b4;
						}
					}
					else if (hitInfo3.transform.gameObject.layer == 22)
					{
						MissionObjective component7 = hitInfo3.transform.GetComponent<MissionObjective>();
						if (null != component7)
						{
							Vector3 b5 = Vector3.up * 3f;
							m_tooltipText.text = LNG.Get("MISSION_TYPE_" + component7.m_type.ToString("d"));
							m_tooltip.position = hitInfo3.transform.position + b5;
						}
					}
					m_mouseOverRenderers = hitInfo3.transform.GetComponentsInChildren<Renderer>();
					if (m_mouseOverRenderers.Length == 0 && null != hitInfo3.transform.parent)
					{
						m_mouseOverRenderers = hitInfo3.transform.parent.GetComponentsInChildren<Renderer>();
					}
					if (m_mouseOverRenderers.Length != 0)
					{
						m_mouseOverLayer = m_mouseOverRenderers[0].gameObject.layer;
						Renderer[] mouseOverRenderers2 = m_mouseOverRenderers;
						foreach (Renderer renderer2 in mouseOverRenderers2)
						{
							renderer2.gameObject.layer = 20;
						}
					}
				}
			}
			m_mouseOverDur = -1f;
		}
		else
		{
			m_mouseOverDur = 0f;
		}
	}

	private bool CalculatePath(Vector3 a_target, bool a_interact)
	{
		if (null != m_client && !m_client.IsInVehicle())
		{
			a_target.y = 0f;
			if (NavMesh.CalculatePath(m_client.GetPos(), a_target, -1, m_path))
			{
				if (m_path.corners.Length > 0 && (m_path.corners[m_path.corners.Length - 1] - a_target).sqrMagnitude > 1f)
				{
					m_pathCorners = new Vector3[2];
					m_pathCorners[0] = m_client.GetPos();
					m_pathCorners[1] = a_target;
				}
				else
				{
					m_pathCorners = m_path.corners;
				}
				m_nextPathPoint = 1;
				m_interactionAtPathEnd = a_interact;
				return true;
			}
		}
		return false;
	}

	private void CalculateRotTowardsMouse(Vector3 a_mousePos)
	{
		m_rotTowardsMousePos = -1f;
		if (!m_inventory.IsVisible() && !m_communicator.IsActive() && null != m_client && null != Camera.main)
		{
			Vector3 b = Camera.main.WorldToScreenPoint(m_client.GetPos() + Vector3.up);
			m_rotTowardsMousePos = Vector3.Angle((a_mousePos - b).normalized, Vector3.up);
			if (a_mousePos.x < b.x)
			{
				m_rotTowardsMousePos = 360f - m_rotTowardsMousePos;
			}
		}
	}

	private void ResetTarget()
	{
		m_rotTowardsMousePos = -1f;
		m_currentTarget = null;
		if (null != m_bullsEye)
		{
			m_bullsEye.position = Vector3.one * 1000f;
		}
	}
}
