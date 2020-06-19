using UnityEngine;

public class CharSounds : MonoBehaviour
{
	public float m_stepIntervall = 0.5f;

	public GameObject m_audioPrefab;

	public AudioClip[] m_stepSounds;

	public AudioClip[] m_sufferSounds;

	public AudioClip[] m_rangedSounds;

	public AudioClip[] m_meleeSounds;

	private Vector3 m_lastPos = Vector3.zero;

	private float m_timeToNextStep;

	private void Start()
	{
		m_lastPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		bool flag = (base.transform.position - m_lastPos).sqrMagnitude > 0.001f;
		m_lastPos = base.transform.position;
		if (flag)
		{
			m_timeToNextStep -= fixedDeltaTime;
			if (m_timeToNextStep < 0f && m_stepSounds != null && 0 < m_stepSounds.Length)
			{
				base.audio.clip = m_stepSounds[Random.Range(0, m_stepSounds.Length)];
				base.audio.Play();
				m_timeToNextStep = m_stepIntervall;
			}
		}
	}

	private void InstantiateSound(AudioClip a_clip, float a_volume = 1f)
	{
		GameObject gameObject = (GameObject)Object.Instantiate(m_audioPrefab, base.transform.position, Quaternion.identity);
		TimedDestroy component = gameObject.GetComponent<TimedDestroy>();
		component.m_destroyAfter = a_clip.length + 0.1f;
		gameObject.audio.clip = a_clip;
		gameObject.audio.volume = a_volume;
		gameObject.audio.pitch = Random.Range(0.9f, 1.1f);
		gameObject.audio.Play();
	}

	public void Attack(ItemDef a_weapon)
	{
		if (a_weapon.ammoItemType > 0)
		{
			InstantiateSound(m_rangedSounds[Items.GetAmmoSoundIndex(a_weapon.ammoItemType)], 0.4f);
		}
		else
		{
			InstantiateSound(m_meleeSounds[Random.Range(0, m_meleeSounds.Length)], 0.4f);
		}
	}

	public void Suffer(bool a_isDead)
	{
		InstantiateSound(m_sufferSounds[Random.Range(0, m_sufferSounds.Length)], 0.4f);
	}
}
