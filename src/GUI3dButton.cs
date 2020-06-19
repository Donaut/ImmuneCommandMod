using UnityEngine;

public class GUI3dButton : MonoBehaviour
{
	public float m_clickAnimSize = 1.5f;

	public bool m_changePosOnRatioChange = true;

	private float m_startX;

	private Vector3 m_StartScale;

	protected GUI3dMaster m_guimaster;

	protected bool m_animate;

	protected bool m_animPlaying;

	protected virtual void Start()
	{
		m_StartScale = base.transform.localScale;
		Vector3 localPosition = base.transform.localPosition;
		m_startX = localPosition.x;
		m_guimaster = (GUI3dMaster)Object.FindObjectOfType(typeof(GUI3dMaster));
		if (null != m_guimaster && m_changePosOnRatioChange)
		{
			Vector3 localPosition2 = base.transform.localPosition;
			localPosition2.x = m_startX * m_guimaster.GetRatioMultiplier();
			base.transform.localPosition = localPosition2;
		}
	}

	protected virtual void Update()
	{
		ProgressAnimation();
	}

	private void ProgressAnimation()
	{
		m_animPlaying = (m_animate || (base.transform.localScale - m_StartScale).sqrMagnitude > 1E-05f);
		if (m_animate)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, m_StartScale * m_clickAnimSize, 0.5f);
			if ((m_StartScale * m_clickAnimSize - base.transform.localScale).sqrMagnitude < 1E-05f)
			{
				m_animate = false;
			}
		}
		else if (m_animPlaying)
		{
			base.transform.localScale = Vector3.Lerp(base.transform.localScale, m_StartScale, 0.5f);
		}
	}

	public void Animate()
	{
		m_animate = true;
	}

	public bool IsAnimating()
	{
		return m_animPlaying;
	}
}
