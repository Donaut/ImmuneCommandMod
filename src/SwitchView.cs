using UnityEngine;

public class SwitchView : MonoBehaviour
{
	public Camera mainCam;

	public Camera subCam01;

	public Camera subCam02;

	public Vector2 pos;

	public Vector2 size;

	private void OnGUI()
	{
		if (GUI.Button(new Rect(pos.x * (float)Screen.width, pos.y * (float)Screen.height, size.x * (float)Screen.width, size.y * (float)Screen.height), "Camera01"))
		{
			DisableCam();
			mainCam.enabled = true;
		}
		if (GUI.Button(new Rect(pos.x * (float)Screen.width + 200f, pos.y * (float)Screen.height, size.x * (float)Screen.width, size.y * (float)Screen.height), "Camera02"))
		{
			DisableCam();
			subCam01.enabled = true;
		}
		if (GUI.Button(new Rect(pos.x * (float)Screen.width + 400f, pos.y * (float)Screen.height, size.x * (float)Screen.width, size.y * (float)Screen.height), "Camera03"))
		{
			DisableCam();
			subCam02.enabled = true;
		}
	}

	private void DisableCam()
	{
		mainCam.enabled = false;
		subCam01.enabled = false;
		subCam02.enabled = false;
	}
}
