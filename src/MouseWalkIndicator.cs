using UnityEngine;

public class MouseWalkIndicator : MonoBehaviour
{
	public float m_startAlpha = 0.3f;

	public float m_speed = 0.5f;

	private Material m_mat;

	private Vector3 m_lastPos = Vector3.zero;

	private void Start()
	{
		m_mat = base.transform.renderer.material;
	}

	private void Update()
	{
		Color color = m_mat.GetColor("_TintColor");
		if (base.transform.position != m_lastPos)
		{
			m_lastPos = base.transform.position;
			color.a = m_startAlpha;
		}
		if (0f < color.a)
		{
			color.a -= Time.deltaTime * m_speed;
			if (0f >= color.a)
			{
				base.transform.position = Vector3.up * 1000f;
				m_lastPos = base.transform.position;
				color.a = 0f;
			}
		}
		if (color != m_mat.GetColor("_TintColor"))
		{
			m_mat.SetColor("_TintColor", color);
		}
	}
}
