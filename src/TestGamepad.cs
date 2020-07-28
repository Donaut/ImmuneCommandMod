using UnityEngine;

public class TestGamepad : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
		for (KeyCode keyCode = KeyCode.JoystickButton0; keyCode < KeyCode.JoystickButton19; keyCode++)
		{
			if (Input.GetKeyDown(keyCode))
			{
				Debug.Log("joykey0 pressed: " + (int)(keyCode - 330));
			}
		}
		for (KeyCode keyCode2 = KeyCode.Joystick1Button0; keyCode2 < KeyCode.Joystick1Button19; keyCode2++)
		{
			if (Input.GetKeyDown(keyCode2))
			{
				Debug.Log("joykey1 pressed: " + (int)(keyCode2 - 350));
			}
		}
		for (KeyCode keyCode3 = KeyCode.Joystick2Button0; keyCode3 < KeyCode.Joystick2Button19; keyCode3++)
		{
			if (Input.GetKeyDown(keyCode3))
			{
				Debug.Log("joykey2 pressed: " + (int)(keyCode3 - 370));
			}
		}
		for (KeyCode keyCode4 = KeyCode.Joystick3Button0; keyCode4 < KeyCode.Joystick3Button19; keyCode4++)
		{
			if (Input.GetKeyDown(keyCode4))
			{
				Debug.Log("joykey3 pressed: " + (int)(keyCode4 - 390));
			}
		}
	}
}