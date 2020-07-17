using UnityEngine;

public class Bounce : MonoBehaviour
{
	public float m_speed = 1f;

	public float m_startScale = 1f;

	public float m_addToScale = 0.05f;

	private void Update()
	{
		float num = Mathf.Abs(Mathf.Sin(Time.timeSinceLevelLoad * m_speed)) * m_addToScale;
		base.transform.localScale = new Vector3(m_startScale + num, 1f, m_startScale + num);
	}
}