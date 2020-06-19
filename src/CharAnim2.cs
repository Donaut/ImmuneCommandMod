using UnityEngine;

public class CharAnim2 : MonoBehaviour
{
	public enum ePose
	{
		eStand,
		eAttack,
		eDead,
		eSit
	}

	public float m_fadeDur = 0.3f;

	public string m_attackAni = "attack";

	public float m_attackAniSpeed = 1f;

	public string m_dieAni = "die";

	public float m_dieAniSpeed = 1f;

	public string m_runAni = "run";

	public float m_runAniSpeed = 1f;

	public string m_sitAni = "sit";

	public float m_sitAniSpeed = 1f;

	public string m_idleAni = "idle";

	public float m_idleAniSpeed = 1f;

	private Vector3 m_lastPos = Vector3.zero;

	private bool m_isMoving;

	private void Start()
	{
		m_lastPos = base.transform.position;
		base.animation[m_attackAni].speed = m_attackAniSpeed;
		base.animation[m_dieAni].speed = m_dieAniSpeed;
		base.animation[m_runAni].speed = m_runAniSpeed;
		base.animation[m_sitAni].speed = m_sitAniSpeed;
		base.animation[m_idleAni].speed = m_idleAniSpeed;
	}

	private void FixedUpdate()
	{
		m_isMoving = ((base.transform.position - m_lastPos).sqrMagnitude > 0.0002f);
		m_lastPos = base.transform.position;
	}

	public void PlayAnimation(ePose a_anim)
	{
		if (null != base.animation)
		{
			switch (a_anim)
			{
			case ePose.eAttack:
				base.animation.CrossFade(m_attackAni, m_fadeDur);
				break;
			case ePose.eDead:
				base.animation.CrossFade(m_dieAni, m_fadeDur);
				break;
			case ePose.eSit:
				base.animation.CrossFade(m_sitAni, m_fadeDur);
				break;
			default:
				base.animation.CrossFade((!m_isMoving) ? m_idleAni : m_runAni, m_fadeDur);
				break;
			}
		}
	}
}
