using UnityEngine;

namespace MooshieGames.SimpleVehicles
{
	public class VehicleRecoveryController : MonoBehaviour
	{
		private VehicleController _vehicleController;
		private float _timer;
		private const float k_threshold = 0.5f;

		[SerializeField]
		private float _upsideDownRollDelay = 3f;

		[SerializeField]
		private float _timeToLift = 1f;
		[SerializeField]
		private float _timeToRoll = 2f;
		[SerializeField]
		private float _liftHeight = 2f;
		[SerializeField]
		private AnimationCurve _liftCurve;
		[SerializeField]
		private AnimationCurve _rollCurve;
		
		private Vector3 _targetLiftPosition;
		private Vector3 _startLiftPosition;

		private Vector3 _startRollRotation;
		private Vector3 _targetRollRotation;

		private VehicleRecoveryState _recoveryState = VehicleRecoveryState.RightSideUp;
		
		private void Start()
		{
			_vehicleController = GetComponent<VehicleController>();
		}

		private void Update()
		{
			if (_recoveryState is VehicleRecoveryState.Lifting)
			{
				Lift(Time.deltaTime);
				return;
			}

			if (_recoveryState is VehicleRecoveryState.Rolling)
			{
				Roll(Time.deltaTime);
				return;
			}
			
			if (IsUpsideDown())
			{
				TickRollTimer(Time.deltaTime);
			}
			else
			{
				_timer = 0f;
				_recoveryState = VehicleRecoveryState.RightSideUp;
			}
		}

		private void Lift(float deltaTime)
		{
			_timer += deltaTime;
			var percent = _timer / _timeToLift;
			var pos = Vector3.Slerp(_startLiftPosition, _targetLiftPosition, _liftCurve.Evaluate(percent));

			if (percent >= 1f)
			{
				pos = _targetLiftPosition;
				_timer = 0f;
				_recoveryState = VehicleRecoveryState.Rolling;
				_startRollRotation = transform.eulerAngles;
				_targetRollRotation = Vector3.zero;
				_targetRollRotation.y = _startRollRotation.y;
			}
			
			transform.position = pos;
		}

		private void Roll(float deltaTime)
		{
			_timer += deltaTime;
			var percent = _timer / _timeToRoll;
			var rot = Vector3.Slerp(_startRollRotation, _targetRollRotation, _rollCurve.Evaluate(percent));
			
			if (percent >= 1f)
			{
				rot = _targetRollRotation;
				_timer = 0f;
				_recoveryState = VehicleRecoveryState.RightSideUp;
				_vehicleController.enabled = true;
			}
			
			transform.eulerAngles = rot;
		}
		
		private void TickRollTimer(float deltaTime)
		{
			_timer += deltaTime;
			_recoveryState = VehicleRecoveryState.UpsideDownWaitingForDelay;
			if (_timer < _upsideDownRollDelay) return;

			_timer = 0f;
			_recoveryState = VehicleRecoveryState.Lifting;
			_startLiftPosition = transform.position;
			_targetLiftPosition = _startLiftPosition + Vector3.up * _liftHeight;
			_vehicleController.enabled = false;
		}
		
		private bool IsUpsideDown()
		{
			return Vector3.Dot(transform.up, Vector3.down) > k_threshold;
		}
	}

	internal enum VehicleRecoveryState : byte
	{
		UpsideDownWaitingForDelay,
		Lifting,
		Rolling,
		RightSideUp
	}
}