using System;
using UnityEngine;

public class SimpleAnim : MonoBehaviour
{
	public float m_speed = 1f;

	public float m_animMoveDelta = 0.4f;

	private float m_progress;

	private float[] m_startVars;

	private void Start()
	{
		m_progress = UnityEngine.Random.Range(0f, 1f);
		m_startVars = new float[base.transform.childCount];
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (null != child)
			{
				float[] startVars = m_startVars;
				int num = i;
				Vector3 localPosition = child.localPosition;
				startVars[num] = localPosition.y;
			}
		}
	}

	private void Update()
	{
		m_progress += Time.deltaTime * m_speed;
		if (1f < m_progress)
		{
			m_progress -= 1f;
		}
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (null != child)
			{
				float num = m_progress + (float)i * 0.05f;
				if (1f < num)
				{
					num -= 1f;
				}
				Vector3 localPosition = child.localPosition;
				localPosition.y = m_startVars[i] + FastSin.Get(num * (float)Math.PI) * m_animMoveDelta;
				child.localPosition = localPosition;
			}
		}
	}
}
