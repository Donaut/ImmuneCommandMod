using System;
using UnityEngine;

public class ScaleAnimOnHit : MonoBehaviour
{
	public bool m_animateParent = true;

	public AudioClip[] m_audioEffects;

	private Vector3 m_startScale = Vector3.zero;

	private Transform m_animTrans;

	private float m_animProgress = -1f;

	private void Start()
	{
		m_animTrans = ((!m_animateParent || !(null != base.transform.parent)) ? base.transform : base.transform.parent);
		m_startScale = m_animTrans.localScale;
	}

	private void Update()
	{
		if (-1f < m_animProgress)
		{
			m_animProgress = Mathf.Clamp01(m_animProgress + Time.deltaTime * 10f);
			m_animTrans.localScale = m_startScale * (1f - 0.1f * Mathf.Sin((float)Math.PI * m_animProgress));
			if (m_animProgress == 1f)
			{
				m_animProgress = -1f;
			}
		}
	}

	private void SetAggressor(Transform a_aggressor)
	{
		if (!Global.isServer)
		{
			m_animProgress = 0f;
			if (null != base.audio && m_audioEffects != null && 0 < m_audioEffects.Length)
			{
				base.audio.clip = m_audioEffects[UnityEngine.Random.Range(0, m_audioEffects.Length)];
				base.audio.Play();
			}
		}
	}
}
