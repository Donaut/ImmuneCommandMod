using UnityEngine;

public class BirdCam : MonoBehaviour
{
	public Transform m_target;

	public Vector3 m_startOffset = new Vector3(0f, 16f, 16f);

	public float m_lookDirInfluence = 2f;

	public float m_zoomMin = 0.4f;

	public float m_zoomMax = 1.2f;

	private float m_zoom = 1f;

	private float m_zoomAdd;

	private Vector3 m_targetOffset = Vector3.zero;

	private DepthOfFieldScatter m_dof;

	private float m_speed;

	private Vector3 m_lastTargetPos = Vector3.zero;

	private float m_nextSpeedUpdate;

	private float m_speedUpdateInterval = 0.3f;

	private void Start()
	{
		m_dof = GetComponent<DepthOfFieldScatter>();
	}

	private void LateUpdate()
	{
		if (!(null == m_target))
		{
			if (null != m_dof)
			{
				m_dof.focalTransform = m_target;
			}
			float deltaTime = Time.deltaTime;
			if (Time.time > m_nextSpeedUpdate)
			{
				m_speed = (m_lastTargetPos - m_target.position).magnitude / m_speedUpdateInterval;
				m_lastTargetPos = m_target.position;
				m_nextSpeedUpdate = Time.time + m_speedUpdateInterval;
			}
			float num = 0.25f * Mathf.Clamp01(m_speed / 15f);
			m_zoomAdd += (num - m_zoomAdd) * deltaTime;
			m_zoom = Mathf.Clamp(m_zoom - Input.GetAxis("Mouse ScrollWheel") * deltaTime * 5f, m_zoomMin, m_zoomMax);
			Vector3 b = m_startOffset * (m_zoom + m_zoomAdd);
			Vector3 a = m_target.forward * m_lookDirInfluence;
			m_targetOffset += (a - m_targetOffset) * deltaTime;
			base.transform.position = m_target.position + m_targetOffset + b;
		}
	}
}
