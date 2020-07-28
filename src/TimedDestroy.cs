using UnityEngine;

public class TimedDestroy : MonoBehaviour
{
	public float m_destroyAfter = 10f;

	private float m_dieTime;

	private void Start()
	{
		m_dieTime = Time.time + m_destroyAfter;
	}

	private void Update()
	{
		if (Time.time > m_dieTime)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
