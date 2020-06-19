using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
	public float m_speed = 0.01f;

	public float m_moveDistance = 10f;

	public Transform m_target;

	public Light m_highlight;

	public Light m_highlightMouse;

	public Light m_backgroundDirLight;

	private float m_progress;

	private float m_maxIntensity;

	private float m_maxBgIntensity;

	private void Start()
	{
		m_maxIntensity = base.light.intensity;
		m_maxBgIntensity = m_backgroundDirLight.intensity;
	}

	private void FixedUpdate()
	{
		m_progress += Time.fixedDeltaTime * m_speed;
		if (m_progress > 1f)
		{
			m_progress -= 1f;
		}
		float a_pip = 1f;
		float lightIntensity = Util.GetLightIntensity(m_progress, out a_pip);
		base.light.intensity = lightIntensity * m_maxIntensity;
		m_backgroundDirLight.intensity = lightIntensity * m_maxBgIntensity;
		Vector3 position = base.transform.position;
		position.x = 0f - (m_moveDistance * m_progress * a_pip - m_moveDistance * 0.5f);
		base.transform.position = position;
		base.transform.LookAt(m_target);
		base.audio.volume = Mathf.Clamp01(1f - GetDayLight() - 0.8f);
		HandleHighlight();
	}

	private void HandleHighlight()
	{
		float num = FastSin.Get(Time.time * 3f);
		float num2 = 0.2f;
		m_highlight.intensity = base.light.intensity + num2 + num * (base.light.intensity * 0.1f + num2);
		m_highlightMouse.intensity = base.light.intensity + 0.2f;
	}

	public void Init(float a_progress, float a_speed)
	{
		m_progress = a_progress;
		m_speed = a_speed;
	}

	public float GetDayLight()
	{
		return base.light.intensity;
	}

	public float GetProgress()
	{
		return m_progress;
	}

	public string GetTime()
	{
		float num = m_progress * 24f + 4f;
		if (num >= 24f)
		{
			num -= 24f;
		}
		string text = ((int)((num - (float)(int)num) * 60f)).ToString();
		if (text.Length == 1)
		{
			text = "0" + text;
		}
		string text2 = ((int)num).ToString();
		if (text2.Length == 1)
		{
			text2 = "0" + text2;
		}
		return text2 + ":" + text;
	}
}
