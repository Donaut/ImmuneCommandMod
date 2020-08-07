using System;
using UnityEngine;

public class Init : MonoBehaviour
{
	public Texture2D m_cursor;

	public Vector2 m_cursorOffset;

	private void Awake()
	{
		Application.runInBackground = true;
		Application.targetFrameRate = 100;

		Debug.Log(Environment.CommandLine);
		if (!Application.isEditor)
		{
			if (Environment.CommandLine.Contains("-batchmode"))
			{
				Screen.showCursor = true;
				LidServer lidServer = (LidServer)UnityEngine.Object.FindObjectOfType(typeof(LidServer));
				lidServer.enabled = true;
				lidServer.m_shutdownIfEmpty = Environment.CommandLine.Contains("-killonempty");
			}
			else
			{
				LidClient lidClient = (LidClient)UnityEngine.Object.FindObjectOfType(typeof(LidClient));
				lidClient.enabled = true;
			}
		}
		Screen.showCursor = true;

		Cursor.SetCursor(m_cursor, m_cursorOffset, CursorMode.Auto);
	}

	private void Start()
	{
	}
}