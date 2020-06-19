using UnityEngine;

public class GaugeNeedle : MonoBehaviour
{
	private const float c_minInput = 0.6f;

	private const float c_maxInput = 1.4f;

	[Range(0.6f, 1.4f)]
	public float m_input = 0.6f;

	private void Update()
	{
		base.transform.rotation = Quaternion.Euler(0f, 0f, (m_input - 0.6f) / 0.799999952f * -360f);
	}
}
