using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
	public bool m_isGoalA = true;

	public SoccerManager m_manager;

	public Transform m_ball;

	private void OnTriggerEnter(Collider a_col)
	{
		if (a_col.transform == m_ball)
		{
			if (m_isGoalA)
			{
				m_manager.m_goalsB++;
			}
			else
			{
				m_manager.m_goalsA++;
			}
		}
	}
}