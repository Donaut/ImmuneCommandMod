using System;
using UnityEngine;

public class RiseAndDie : MonoBehaviour
{
	public Vector3 m_riseVector = new Vector3(0f, 9f, 0f);

	public float m_riseTime = 2f;

	public float m_alphaFadeOutStart = 0.7f;

	private Vector3 m_startPos = Vector3.zero;

	private Vector3 m_endPos = Vector3.zero;

	private float m_myTime;

	private float m_progress;

	private Renderer[] m_renderers;

	private void Start()
	{
		m_startPos = base.transform.localPosition;
		if (Vector3.zero == m_endPos)
		{
			m_endPos = base.transform.localPosition + m_riseVector;
		}
		m_renderers = GetComponentsInChildren<Renderer>();
	}

	public void SetEndByCollision(Vector3 a_collisionStartPos)
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(a_collisionStartPos, m_riseVector, out hitInfo))
		{
			m_riseVector = m_riseVector.normalized * (hitInfo.point - base.transform.localPosition).magnitude;
			m_endPos = base.transform.localPosition + m_riseVector;
		}
	}

	private void Update()
	{
		m_myTime += Time.deltaTime * Time.timeScale;
		m_progress = m_myTime / m_riseTime;
		float t = Mathf.Sin((float)Math.PI / 2f * m_progress);
		base.transform.localPosition = Vector3.Lerp(m_startPos, m_endPos, t);
		Renderer[] renderers = m_renderers;
		foreach (Renderer renderer in renderers)
		{
			if (renderer.material.HasProperty("_Color"))
			{
				Color color = renderer.material.color;
				color.a = 1f - (m_progress - m_alphaFadeOutStart) / (1f - m_alphaFadeOutStart);
				renderer.material.color = color;
			}
		}
		if (m_progress >= 1f)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
