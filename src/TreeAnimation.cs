using System;
using UnityEngine;

public class TreeAnimation : MonoBehaviour
{
	public Transform m_tree;

	private float m_animProgress;

	private void OnEnable()
	{
		m_animProgress = 0f;
	}

	private void OnDisable()
	{
		if (null != m_tree)
		{
			m_tree.localRotation = Quaternion.identity;
			m_tree.renderer.enabled = true;
		}
	}

	private void Update()
	{
		if (m_animProgress != -1f)
		{
			if (m_animProgress < 2f)
			{
				m_animProgress += Time.deltaTime;
				float num = 1f + FastSin.Get(4.712389f + Mathf.Clamp01(m_animProgress) * 0.5f * (float)Math.PI);
				m_tree.localRotation = Quaternion.Euler(num * -85f, 0f, 0f);
			}
			else
			{
				m_tree.renderer.enabled = false;
				m_animProgress = -1f;
			}
		}
	}
}
