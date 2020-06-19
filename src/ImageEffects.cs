using System;
using UnityEngine;

public class ImageEffects : MonoBehaviour
{
	public float m_minVignette = 1f;

	public float m_maxVignette = 5f;

	public float m_maxOverlay = 0.5f;

	public float m_pulseSpeed = 2f;

	private Vignetting m_vignetteEffect;

	private ScreenOverlay m_overlayEffect;

	private float m_sinProgress;

	private LidClient m_client;

	private void Start()
	{
		m_client = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
		m_vignetteEffect = GetComponent<Vignetting>();
		m_overlayEffect = GetComponent<ScreenOverlay>();
		if (2 > QualitySettings.GetQualityLevel())
		{
			SSAOEffect component = GetComponent<SSAOEffect>();
			if (null != component)
			{
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	private void Update()
	{
		if (!Global.isServer && null != m_client && m_client.enabled)
		{
			float num = 1f - Mathf.Clamp01(m_client.GetHealth() * 0.01f);
			float num2 = 1f - Mathf.Clamp01(m_client.GetEnergy() * 0.01f);
			float num3 = 0.5f + (FastSin.Get(m_sinProgress) + 1f) * 0.25f;
			m_sinProgress += Time.deltaTime * m_pulseSpeed;
			if (m_sinProgress > (float)Math.PI * 2f)
			{
				m_sinProgress -= (float)Math.PI * 2f;
			}
			m_vignetteEffect.intensity = m_minVignette + num2 * (m_maxVignette - m_minVignette);
			m_overlayEffect.intensity = num * m_maxOverlay * num3;
		}
	}
}
