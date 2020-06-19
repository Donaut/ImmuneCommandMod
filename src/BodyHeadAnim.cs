using UnityEngine;

public class BodyHeadAnim : MonoBehaviour
{
	public float m_maxSpeed = 6f;

	public GameObject m_bodyPrefab;

	public Texture m_bodyTexture;

	public GameObject m_headPrefab;

	public GameObject m_rucksackPrefab;

	public GameObject m_shootEffectPrefab;

	public GameObject m_trailEffectPrefab;

	public GameObject m_digEffectPrefab;

	public GameObject m_textEffectPrefab;

	public GameObject m_waterEffectPrefab;

	public RuntimeAnimatorController m_animController;

	public Material m_hologramValidMat;

	public Material m_hologramInvalidMat;

	public Bone m_boneHand;

	public Bone m_boneArm;

	public Bone m_boneBody;

	public Bone m_boneHead;

	public int m_defaultHandItemType = -1;

	public int m_defaultHeadItemType = -1;

	public int m_defaultBodyItemType = -1;

	public int m_defaultSkinItemType = -1;

	private Animator m_animator;

	private Vector3 m_lastPos;

	private ItemDef m_handItemDef = Items.GetItemDef(0);

	private int m_handItemType;

	private Transform m_handItemExit;

	private float m_nextAttackTime;

	private float m_playAttackAnimTime;

	private Renderer[] m_hologramRenderers;

	private GameObject m_holoBuilding;

	private SkinnedMeshRenderer m_bodyRenderer;

	private Renderer m_headRenderer;

	private Texture m_headTexture;

	private bool m_isMale;

	private ClientInput m_input;

	private LidClient m_client;

	private CharSounds m_sound;

	public bool m_isSitting;

	[HideInInspector]
	public bool m_isTakingAction;

	private void Awake()
	{
		if (Global.isServer)
		{
			Object.DestroyImmediate(this);
			return;
		}
		m_isMale = ((!(null != m_bodyPrefab) || !m_bodyPrefab.name.Contains("female")) ? true : false);
		GameObject gameObject = SetupPart(m_bodyPrefab, base.transform, false);
		if (null != gameObject)
		{
			m_bodyRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			if (null != m_bodyTexture)
			{
				m_bodyRenderer.material.mainTexture = m_bodyTexture;
			}
			else
			{
				m_bodyTexture = m_bodyRenderer.material.mainTexture;
			}
			m_boneHand.bone = gameObject.transform.Find(m_boneHand.name);
			m_boneArm.bone = gameObject.transform.Find(m_boneArm.name);
			m_boneBody.bone = gameObject.transform.Find(m_boneBody.name);
			m_boneHead.bone = gameObject.transform.Find(m_boneHead.name);
			if (null != m_boneBody.bone && null != m_rucksackPrefab)
			{
				GameObject gameObject2 = SetupPart(m_rucksackPrefab, m_boneBody.bone, false);
				gameObject2.transform.localPosition = new Vector3(0.11f, -0.14f, 0f);
				gameObject2.transform.localRotation = Quaternion.Euler(270f, 90f, 0f);
			}
			if (null != m_boneHead.bone)
			{
				m_boneHead.lookPart = SetupPart(m_headPrefab, m_boneHead.bone, false);
				m_boneHead.lookPart.transform.localPosition = new Vector3(-0.04f, 0.02f, 0f);
				m_boneHead.lookPart.transform.localRotation = Quaternion.Euler(0f, 270f, 180f);
				m_headRenderer = m_boneHead.lookPart.GetComponentInChildren<Renderer>();
				m_headTexture = m_headRenderer.material.mainTexture;
			}
			m_animator = gameObject.GetComponentInChildren<Animator>();
			m_animator.runtimeAnimatorController = m_animController;
			m_animator.Rebind();
		}
		m_client = (LidClient)Object.FindObjectOfType(typeof(LidClient));
		m_lastPos = base.transform.position;
		m_sound = GetComponent<CharSounds>();
		ChangeHandItem(m_defaultHandItemType);
		ChangeHeadItem(m_defaultHeadItemType);
		ChangeBodyItem(m_defaultBodyItemType);
		ChangeSkin(m_defaultSkinItemType);
	}

	public void Init(bool a_isOwnPlayer)
	{
		if (a_isOwnPlayer)
		{
			m_input = (ClientInput)Object.FindObjectOfType(typeof(ClientInput));
		}
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector3 vector = base.transform.position - m_lastPos;
		m_lastPos = base.transform.position;
		float num = vector.magnitude / deltaTime;
		float to = (!(num > 1f)) ? 0f : Mathf.Clamp01(num / m_maxSpeed);
		m_animator.SetFloat("speed", Mathf.Lerp(m_animator.GetFloat("speed"), to, deltaTime * 6f));
	}

	private void LateUpdate()
	{
		if (m_isTakingAction && (null == m_input || m_input.IsAttacking()))
		{
			AnimateAttacking();
			if ((m_handItemDef.ammoItemType > 0 || m_handItemDef.buildingIndex > 0) && null != m_boneArm.bone)
			{
				m_boneArm.bone.localRotation = Quaternion.Euler(17f, 288f, 336f);
			}
		}
		m_animator.SetBool("attack", m_playAttackAnimTime > Time.time);
		m_animator.SetBool("sit", m_isSitting);
		if (null != m_animator)
		{
			m_animator.transform.localPosition = Vector3.zero;
		}
		HandleHologram();
	}

	private GameObject SetupPart(GameObject a_part, Transform a_parent, bool a_optional)
	{
		GameObject gameObject = null;
		if (null != a_part && null != a_parent)
		{
			gameObject = (GameObject)Object.Instantiate(a_part);
			Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
			NavMeshObstacle[] componentsInChildren2 = gameObject.GetComponentsInChildren<NavMeshObstacle>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i]);
			}
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Object.Destroy(componentsInChildren2[j]);
			}
			if (a_optional)
			{
				gameObject.transform.parent = a_parent;
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.identity;
				Transform transform = gameObject.transform.FindChild("Handle");
				if (null != transform)
				{
					gameObject.transform.localPosition = transform.localPosition * -0.61f;
				}
				Transform transform2 = gameObject.transform.FindChild("Particles");
				if (null != transform2)
				{
					transform2.gameObject.SetActive(true);
				}
			}
			else
			{
				gameObject.transform.position = base.transform.position;
				gameObject.transform.rotation = base.transform.rotation;
				gameObject.transform.parent = a_parent;
			}
		}
		return gameObject;
	}

	private void AnimateAttacking()
	{
		ItemDef a_weapon = m_handItemDef;
		if (a_weapon.damage < 1f)
		{
			a_weapon = Items.GetItemDef(0);
		}
		if (a_weapon.buildingIndex != 0 || !(Time.time > m_nextAttackTime))
		{
			return;
		}
		if (a_weapon.ammoItemType > 0)
		{
			Vector3 position;
			if (null != m_handItemExit)
			{
				position = m_handItemExit.position;
				Quaternion rotation = Quaternion.LookRotation(-m_handItemExit.forward);
				GameObject gameObject = (GameObject)Object.Instantiate(m_shootEffectPrefab, position, rotation);
				gameObject.transform.parent = m_handItemExit;
			}
			Vector3 vector = (!(null != m_input) || !(null != m_input.GetTarget())) ? base.transform.forward : (m_input.GetTarget().transform.position - base.transform.position).normalized;
			position = base.transform.position + Quaternion.LookRotation(vector) * new Vector3(0.5f, 1.2f, 1.5f);
			GameObject gameObject2 = (GameObject)Object.Instantiate(m_trailEffectPrefab, position, base.transform.rotation);
			RiseAndDie component = gameObject2.GetComponent<RiseAndDie>();
			component.m_riseVector = vector * a_weapon.range * 0.9f;
			component.SetEndByCollision(base.transform.position + Vector3.up * 1.5f);
		}
		else
		{
			m_playAttackAnimTime = Time.time + 0.6f;
		}
		HandleSpecialWeaponAttack();
		m_sound.Attack(a_weapon);
		DoRaycast();
		m_nextAttackTime = Time.time + a_weapon.attackdur;
	}

	private void HandleSpecialWeaponAttack()
	{
		if (m_handItemType == 109)
		{
			Object.Instantiate(m_digEffectPrefab, base.transform.position + base.transform.forward * 0.3f, Quaternion.identity);
		}
		else if (m_handItemType == 110)
		{
			Vector3 vector = base.transform.position + base.transform.forward * 3.5f;
			if (0.8f > Util.GetTerrainHeight(vector))
			{
				Object.Instantiate(m_waterEffectPrefab, vector, Quaternion.identity);
				return;
			}
			GameObject gameObject = (GameObject)Object.Instantiate(m_textEffectPrefab, base.transform.position + Vector3.up * 3f, Quaternion.identity);
			TextLNG component = gameObject.GetComponent<TextLNG>();
			component.m_lngKey = "EVENT_NOWATER";
		}
	}

	private void DoRaycast()
	{
		Transform a_target = null;
		Raycaster.Attack(base.transform, m_handItemDef, base.transform.position + base.transform.forward * 1.2f, ref a_target);
	}

	private void HandleHologram()
	{
		if (m_hologramRenderers == null || !(null != m_client) || !(null != m_holoBuilding))
		{
			return;
		}
		if (m_isSitting)
		{
			m_holoBuilding.SetActive(false);
			return;
		}
		if (!m_holoBuilding.activeSelf)
		{
			m_holoBuilding.SetActive(true);
		}
		Renderer[] hologramRenderers = m_hologramRenderers;
		foreach (Renderer renderer in hologramRenderers)
		{
			if (null == renderer)
			{
				m_hologramRenderers = null;
				break;
			}
			bool flag = m_client.IsValidBuildPos(m_holoBuilding.transform.position, m_handItemDef.buildingIndex);
			renderer.sharedMaterial = ((!flag) ? m_hologramInvalidMat : m_hologramValidMat);
		}
		m_holoBuilding.transform.rotation = Quaternion.Euler(0f, m_input.GetBuildRot(), 0f);
	}

	private Renderer[] GetRenderersAndDisableShadows(GameObject a_go)
	{
		Renderer[] componentsInChildren = a_go.GetComponentsInChildren<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (null != renderer)
			{
				renderer.castShadows = false;
				renderer.receiveShadows = false;
			}
		}
		return componentsInChildren;
	}

	private void ChangePart(ref Bone a_bone, bool a_optional, int a_newPartType)
	{
		GameObject gameObject = (!a_optional) ? a_bone.lookPart : a_bone.addPart;
		if (null != gameObject)
		{
			Object.Destroy(gameObject);
		}
		if (a_bone == m_boneHand)
		{
			m_handItemExit = null;
			m_handItemType = 0;
			m_handItemDef = Items.GetItemDef(m_handItemType);
			if (null != m_holoBuilding)
			{
				Object.Destroy(m_holoBuilding);
			}
			m_holoBuilding = null;
			m_hologramRenderers = null;
		}
		string empty = string.Empty;
		empty = ((a_bone != m_boneHead) ? ("items/item_" + a_newPartType) : ("inventory_steam/go_" + (a_newPartType - 1 + 10000)));
		GameObject gameObject2 = (GameObject)Resources.Load(empty);
		if (null == gameObject2 && !a_optional)
		{
			gameObject2 = ((a_bone != m_boneBody) ? m_headPrefab : m_rucksackPrefab);
		}
		else if (a_optional && a_bone == m_boneBody && !Items.IsBody(a_newPartType))
		{
			gameObject2 = null;
		}
		if (!(null != gameObject2))
		{
			return;
		}
		if (a_optional)
		{
			a_bone.addPart = SetupPart(gameObject2, a_bone.bone, a_optional);
			if (!(null != a_bone.addPart))
			{
				return;
			}
			GetRenderersAndDisableShadows(a_bone.addPart);
			if (a_bone == m_boneHand)
			{
				m_handItemType = a_newPartType;
				m_handItemDef = Items.GetItemDef(m_handItemType);
				if (m_handItemDef.ammoItemType > 0)
				{
					m_handItemExit = a_bone.addPart.transform.FindChild("Exit");
				}
				else
				{
					if (m_handItemDef.buildingIndex <= 0 || !(null != m_input))
					{
						return;
					}
					GameObject gameObject3 = (GameObject)Resources.Load("buildings/building_" + m_handItemDef.buildingIndex);
					if (!(null != gameObject3))
					{
						return;
					}
					m_holoBuilding = SetupPart(gameObject3, base.transform, false);
					if (null != m_holoBuilding)
					{
						ServerBuilding component = m_holoBuilding.GetComponent<ServerBuilding>();
						if (null != component)
						{
							Object.Destroy(component);
						}
						m_holoBuilding.transform.localPosition = new Vector3(0f, 0f, 2f);
						m_holoBuilding.transform.localRotation = Quaternion.Euler(0f, m_input.GetBuildRot(), 0f);
						m_hologramRenderers = GetRenderersAndDisableShadows(m_holoBuilding);
					}
				}
			}
			else if (a_bone == m_boneHead)
			{
				a_bone.addPart.transform.localPosition = new Vector3(-0.15f, 0.01f, 0f);
				a_bone.addPart.transform.localRotation = Quaternion.Euler(90f, 270f, 0f);
			}
			else if (a_bone == m_boneBody)
			{
				a_bone.addPart.transform.localPosition = new Vector3(0.104f, 0.018f, 0f);
				a_bone.addPart.transform.localRotation = Quaternion.Euler(90f, 270f, 0f);
			}
		}
		else
		{
			a_bone.lookPart = SetupPart(gameObject2, a_bone.bone, a_optional);
		}
	}

	public void ResetAnim()
	{
		ChangeHandItem(-1);
		ChangeBodyItem(-1);
		m_isTakingAction = false;
		m_isSitting = false;
	}

	public void ChangeSkin(int a_skinIndex)
	{
		if (1 > a_skinIndex)
		{
			m_bodyRenderer.material.mainTexture = m_bodyTexture;
			m_headRenderer.material.mainTexture = m_headTexture;
			return;
		}
		a_skinIndex = a_skinIndex - 1 + 20000;
		Texture texture = (Texture)Resources.Load("skins/skin_" + ((!m_isMale) ? "0_" : "1_") + a_skinIndex);
		if (null != texture)
		{
			m_bodyRenderer.material.mainTexture = texture;
			m_headRenderer.material.mainTexture = texture;
		}
	}

	public void ChangeHeadItem(int a_itemIndex)
	{
		ChangePart(ref m_boneHead, true, a_itemIndex);
	}

	public void ChangeHandItem(int a_itemIndex)
	{
		ChangePart(ref m_boneHand, true, a_itemIndex);
	}

	public void ChangeBodyItem(int a_itemIndex)
	{
		ChangePart(ref m_boneBody, true, a_itemIndex);
	}
}
