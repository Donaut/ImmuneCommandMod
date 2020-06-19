using UnityEngine;

public class GameInit : MonoBehaviour
{
	public float m_timeScale = 1f;

	private void Start()
	{
		Time.timeScale = m_timeScale;
	}
}
