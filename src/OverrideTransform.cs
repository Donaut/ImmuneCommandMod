using UnityEngine;

public class OverrideTransform : MonoBehaviour
{
	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
	}
}
