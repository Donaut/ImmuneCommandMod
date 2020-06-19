using UnityEngine;

public class RunAway : MonoBehaviour
{
	private void Start()
	{
		NavMeshAgent component = GetComponent<NavMeshAgent>();
		if (null != component)
		{
			component.SetDestination(Vector3.zero);
		}
	}
}
