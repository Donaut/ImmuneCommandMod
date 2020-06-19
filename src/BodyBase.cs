using UnityEngine;

public abstract class BodyBase : MonoBehaviour
{
	protected eBodyBaseState m_state;

	protected BrainBase m_brain;

	protected JobBase m_job;

	public Vector3 m_homePos;

	protected void Init()
	{
		m_brain = GetComponent<BrainBase>();
		m_job = GetComponent<JobBase>();
		m_homePos = base.transform.position;
	}

	public abstract bool GoTo(Vector3 a_target);

	public abstract bool IsMoving();

	public abstract void Idle();

	public abstract bool Respawn();

	public abstract bool Attack(Transform a_victim, bool a_approach);

	public abstract bool FindFood(float status, float deltaTime);

	public abstract bool FindDrink(float status, float deltaTime);

	public abstract bool FindSleep(float status, float deltaTime);

	public abstract bool FindHealing(float status, float deltaTime);

	public abstract bool FindCatharsis(float status, float deltaTime);

	public abstract bool FindMates(float status, float deltaTime);

	public eBodyBaseState GetState()
	{
		return m_state;
	}
}
