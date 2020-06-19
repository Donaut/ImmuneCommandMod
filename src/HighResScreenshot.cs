using System;
using UnityEngine;

public class HighResScreenshot : MonoBehaviour
{
	public Camera cam;

	private int takeHiResShot;

	public static string GetScreenShotName(int width, int height)
	{
		return string.Format("{0}/../screenshots/screen_{1}x{2}_{3}.png", Application.dataPath, width, height, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}

	public void TakeHiResShot()
	{
		takeHiResShot = 1;
	}

	private void LateUpdate()
	{
		if (Input.GetKeyDown("k"))
		{
			takeHiResShot = 1;
		}
		if (takeHiResShot == 1)
		{
			takeHiResShot++;
			Application.CaptureScreenshot(GetScreenShotName(Screen.width * 2, Screen.height * 2), 2);
			Debug.Break();
		}
		else if (takeHiResShot == 2)
		{
			takeHiResShot++;
			Application.CaptureScreenshot(GetScreenShotName(Screen.width * 2, Screen.height * 2), 2);
			Debug.Break();
		}
		else if (takeHiResShot == 3)
		{
			takeHiResShot++;
			Application.CaptureScreenshot(GetScreenShotName(Screen.width * 2, Screen.height * 2), 2);
			Debug.Break();
		}
	}
}
