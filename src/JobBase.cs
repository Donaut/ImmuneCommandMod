using UnityEngine;

public abstract class JobBase : MonoBehaviour
{
	protected BrainBase m_brain;

	protected BodyBase m_body;

	protected void Init()
	{
		m_brain = GetComponent<BrainBase>();
		m_body = GetComponent<BodyBase>();
	}

	public abstract void Execute(float deltaTime);
}
