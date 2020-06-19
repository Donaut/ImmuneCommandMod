using System;
using UnityEngine;

[Serializable]
public class Bone
{
	public string name;

	[HideInInspector]
	public Transform bone;

	[HideInInspector]
	public GameObject lookPart;

	[HideInInspector]
	public GameObject addPart;
}
