using UnityEngine;

public class RPGThirdPersonCamera : MonoBehaviour
{
	private const bool LockCameraBehindTarget = true;

	private const bool RotateCameraBehindTarget = true;

	private float targetYaw;

	private float targetPitch;

	private float targetDistance;

	private float yClamp = float.MinValue;

	private float currentYaw;

	private float currentPitch;

	private float currentDistance;

	private float currentMinDistance;

	private float currentMaxDistance;

	private float realDistance;

	public Camera Camera;

	public Transform Target;

	public float MinDistance = 1f;

	public float MaxDistance = 32f;

	public float MinPitch = -80f;

	public float MaxPitch = 80f;

	public float ZoomSpeed = 16f;

	public Vector3 TargetOffset = Vector3.zero;

	public string ZoomAxis = "Mouse ScrollWheel";

	public string YawAxis = "Mouse X";

	public string PitchAxis = "Mouse Y";

	public string MouseLookButton = "Fire2";

	public static RPGThirdPersonCamera Instance
	{
		get;
		set;
	}

	public bool HasCamera
	{
		get
		{
			return Camera != null;
		}
	}

	public bool HasTarget
	{
		get
		{
			return Target != null;
		}
	}

	public Vector3 TargetPosition
	{
		get
		{
			return (!HasTarget) ? TargetOffset : (Target.position + TargetOffset);
		}
	}

	private void Start()
	{
		Instance = this;
		if (!HasCamera)
		{
			Camera = GetComponentInChildren<Camera>();
		}
		if (!HasTarget)
		{
			try
			{
				Target = GameObject.FindGameObjectWithTag("CameraTarget").transform;
			}
			catch
			{
			}
		}
		MinPitch = Mathf.Clamp(MinPitch, -85f, 0f);
		MaxPitch = Mathf.Clamp(MaxPitch, 0f, 85f);
		MinDistance = Mathf.Max(0f, MinDistance);
		currentMinDistance = MinDistance;
		currentMaxDistance = MaxDistance;
		currentYaw = (targetYaw = 0f);
		currentPitch = (targetPitch = Mathf.Lerp(MinPitch, MaxPitch, 0.6f));
		currentDistance = (targetDistance = (realDistance = Mathf.Lerp(MinDistance, MaxDistance, 0.5f)));
	}

	private void LateUpdate()
	{
		Instance = this;
		if (HasCamera && HasTarget)
		{
			bool buttonSafe = RPGControllerUtils.GetButtonSafe(MouseLookButton, false);
			realDistance -= RPGControllerUtils.GetAxisRawSafe(ZoomAxis, 0f) * ZoomSpeed;
			realDistance = Mathf.Clamp(realDistance, MinDistance, MaxDistance);
			targetDistance = realDistance;
			targetDistance = Mathf.Clamp(targetDistance, currentMinDistance, currentMaxDistance);
			currentDistance = targetDistance;
			Vector3 point = new Vector3(0f, 0f, 0f - currentDistance);
			if (buttonSafe)
			{
				targetPitch -= RPGControllerUtils.GetAxisRawSafe(PitchAxis, 0f) * 4f;
				targetPitch = Mathf.Clamp(targetPitch, MinPitch, MaxPitch);
			}
			targetYaw = RPGControllerUtils.SignedAngle(point.normalized, -Target.transform.forward, Vector3.up);
			targetYaw = Mathf.Repeat(targetYaw + 180f, 360f) - 180f;
			currentYaw = targetYaw;
			currentPitch = targetPitch;
			point = Quaternion.Euler(currentPitch, currentYaw, 0f) * point;
			base.transform.position = TargetPosition + point;
			Vector3 position = base.transform.position;
			base.transform.position = new Vector3(position.x, Mathf.Clamp(position.y, yClamp, float.MaxValue), position.z);
			Camera.transform.LookAt(TargetPosition);
		}
	}
}
