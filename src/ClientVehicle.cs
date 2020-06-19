using UnityEngine;

public class ClientVehicle : MonoBehaviour
{
	public Transform m_leftWheel;

	public Transform m_rightWheel;

	public Transform[] m_rearWheels;

	public float m_maxCarPartRot = 0.05f;

	public Light m_light;

	public Transform[] m_carParts;

	public float m_maxNoiseSpeed = 15f;

	public AudioSource m_soundCarStart;

	public AudioSource m_soundCrash;

	public AudioSource m_soundSqueel;

	public GameObject m_damageFire;

	private Quaternion[] m_carPartsStartRot;

	private Quaternion m_startRotLeft;

	private Quaternion m_startRotRight;

	private Vector3 m_lastPos = Vector3.zero;

	private float m_lastRot;

	private DayNightCycle m_dayNight;

	private float m_health = 100f;

	private float m_nextEngineStartSound;

	private void Start()
	{
		m_startRotLeft = m_leftWheel.localRotation;
		m_startRotRight = m_rightWheel.localRotation;
		m_lastPos = base.transform.position;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		m_lastRot = eulerAngles.y;
		m_dayNight = (DayNightCycle)Object.FindObjectOfType(typeof(DayNightCycle));
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		Vector3 rhs = base.transform.position - m_lastPos;
		float magnitude = rhs.magnitude;
		float num = magnitude / deltaTime;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		float num2 = eulerAngles.y - m_lastRot;
		float num3 = (!(0f > Vector3.Dot(rhs.normalized, base.transform.forward))) ? 1f : (-1f);
		UpdateSound(deltaTime, num);
		if (num2 != 0f)
		{
			float num4 = 0f;
			if (Mathf.Abs(num2) > 10f * deltaTime)
			{
				num4 = num3 * ((!(num2 > 0f)) ? (-1f) : 1f);
			}
			m_leftWheel.localRotation = Quaternion.Lerp(m_leftWheel.localRotation, m_startRotLeft * Quaternion.Euler(0f, num4 * 35f, 0f), deltaTime * 4f);
			m_rightWheel.localRotation = Quaternion.Lerp(m_rightWheel.localRotation, m_startRotRight * Quaternion.Euler(0f, num4 * 35f, 0f), deltaTime * 4f);
			float num5 = (!(Mathf.Abs(num4) > 0.5f)) ? 0f : (Mathf.Abs(num4) * 0.05f);
			float num6 = Mathf.Max(num - 12f, 0f) * 0.25f;
			m_soundSqueel.volume = Mathf.Clamp(Mathf.Lerp(m_soundSqueel.volume, num6 * num5, deltaTime), 0f, 0.12f);
		}
		else
		{
			m_soundSqueel.volume = 0f;
		}
		if (Vector3.zero != rhs)
		{
			float x = num * num3;
			m_leftWheel.localRotation *= Quaternion.Euler(x, 0f, 0f);
			m_rightWheel.localRotation *= Quaternion.Euler(x, 0f, 0f);
			m_rearWheels[0].localRotation *= Quaternion.Euler(x, 0f, 0f);
			m_rearWheels[1].localRotation *= Quaternion.Euler(x, 0f, 0f);
			m_nextEngineStartSound = Time.time + 3f;
		}
		m_lastPos = base.transform.position;
		Vector3 eulerAngles2 = base.transform.rotation.eulerAngles;
		m_lastRot = eulerAngles2.y;
		m_light.enabled = (m_dayNight.GetDayLight() == 0f && m_health > 0f);
	}

	private void UpdateSound(float a_dt, float a_speedMs)
	{
		float num = (!(a_speedMs > 1f)) ? 0f : Mathf.Clamp01(a_speedMs / m_maxNoiseSpeed);
		float to = 1f + Mathf.Clamp01(a_speedMs / m_maxNoiseSpeed) * 0.3f;
		if (Time.time > m_nextEngineStartSound && base.audio.volume == 0f && 0f < num)
		{
			m_soundCarStart.Play();
		}
		base.audio.volume = ((num != 0f) ? Mathf.Lerp(base.audio.volume, num, a_dt * 2f) : 0f);
		base.audio.pitch = Mathf.Lerp(base.audio.pitch, to, a_dt * 2f);
	}

	private void SetHealth(float a_health)
	{
		if (a_health < m_health && base.audio.volume > 0f)
		{
			m_soundCrash.Play();
		}
		m_health = a_health;
		m_damageFire.SetActive(m_health < 20f && 0f < m_health);
		if (m_carPartsStartRot == null)
		{
			m_carPartsStartRot = new Quaternion[m_carParts.Length];
			for (int i = 0; i < m_carParts.Length; i++)
			{
				m_carPartsStartRot[i] = m_carParts[i].localRotation;
			}
		}
		for (int j = 0; j < m_carParts.Length; j++)
		{
			m_carParts[j].localRotation = Quaternion.Lerp(m_carPartsStartRot[j], Random.rotation, (100f - a_health) * 0.01f * m_maxCarPartRot);
		}
	}
}
