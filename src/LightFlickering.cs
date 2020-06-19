using UnityEngine;

public class LightFlickering : MonoBehaviour
{
	public float m_minIntensity = 1f;

	public float m_maxIntensity = 1.6f;

	private void FixedUpdate()
	{
		if (null != base.light)
		{
			base.light.intensity = Random.Range(m_minIntensity, m_maxIntensity);
		}
	}
}
