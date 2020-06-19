using UnityEngine;

public class DestroyIf : MonoBehaviour
{
	public bool ifServer;

	public bool ifClient;

	private void Awake()
	{
		if ((ifServer && Global.isServer) || (ifClient && !Global.isServer))
		{
			Object.DestroyImmediate(base.gameObject);
		}
	}
}
