using UnityEngine;
using UnityEngine.Serialization;

namespace MooshieGames.SimpleVehicles
{
	public class VehicleController : MonoBehaviour
	{
		[SerializeField] private VehicleAttributes _attributes;
		[SerializeField] private InputReader _input;
		[SerializeField, FormerlySerializedAs("wheels")] private Wheel[] _wheels;
		
		private Rigidbody _rigidbody;
		
		private float _throttleInput;
		private float _steerInput;
		private float _speed;
		private bool _grounded;
		
		private float SpeedRatio => _speed / _attributes.MaxSpeed;
		
		private void Start()
		{
			_rigidbody = GetComponent<Rigidbody>();
			_input.Enable();

			foreach (var wheel in _wheels)
			{
				wheel.Setup();
			}
		}

		private void OnEnable()
		{
			if (!_rigidbody) return;
			
			_rigidbody.isKinematic = false;
		}

		private void OnDisable()
		{
			if (!_rigidbody) return;

			_rigidbody.isKinematic = true;
			ResetWheels();
		}

		private void ResetWheels()
		{
			foreach (var wheel in _wheels)
			{
				wheel.ResetVisual();
			}
		}
		
		private void Update()
		{
			GetInput();

			foreach (var wheel in _wheels)
			{
				var wheelVerticalCenter = wheel.rayPoint.InverseTransformPoint(wheel.WheelCenter);
				wheelVerticalCenter.x = 0f;
				wheelVerticalCenter.z = 0f;
				
				var newWheelVisualPosition = Vector3.Slerp(wheel.wheelVisual.localPosition, wheelVerticalCenter, Time.deltaTime * _attributes.VisualWheelMoveSpeed);
				wheel.wheelVisual.localPosition = newWheelVisualPosition;
			}
		}

		private void FixedUpdate()
		{
			_speed = _rigidbody.linearVelocity.magnitude;
			
			CalculateSuspension();
			CalculateSteering();
			if (_grounded == false)
			{
				Airborne();
				return;
			}
			
			ApplyPowerForces();
			ApplySuspensionForces();
			ApplySteeringForces();
		}

		private void Airborne()
		{
			var rollDot = Vector3.Dot(-transform.right, Vector3.up);
			var pitchDot = Vector3.Dot(transform.forward, Vector3.up);
			
			var rollTorque = transform.forward * (rollDot * (_attributes.AirborneCorrectionForce * 0.5f));
			rollTorque += transform.right * (pitchDot * _attributes.AirborneCorrectionForce);
			_rigidbody.AddTorque(rollTorque);
			_rigidbody.AddForce(Vector3.down * _attributes.AirborneCorrectionForce);
		}
		
		private void GetInput()
		{
			var moveSign = Mathf.Sign(_input.Move.y);
			_throttleInput = Mathf.Abs(_input.Move.y) > 0.2f ? moveSign : 0f;
			
			var steerSign = Mathf.Sign(_input.Move.x);
			_steerInput = Mathf.Abs(_input.Move.x) > 0.2f ? steerSign : 0f;
			
			var steerAngle = _attributes.MaxSteerAngle * (_steerInput * _attributes.SteeringStrengthCurve.Evaluate(SpeedRatio));

			foreach (var wheel in _wheels)
			{
				if(wheel.steering == false) continue;

				var angle = Mathf.LerpAngle(wheel.rayPoint.localEulerAngles.y, steerAngle, _attributes.SteerSpeed * Time.deltaTime);
				wheel.rayPoint.localEulerAngles = Vector3.up * angle;
			}
		}

		private void ApplySteeringForces()
		{
			foreach (var wheel in _wheels)
			{
				if(wheel.grounded == false) continue;
				
				_rigidbody.AddForceAtPosition(wheel.steerForce, wheel.WheelCenter);
			}
		}
		
		private void ApplyPowerForces()
		{
			foreach (var wheel in _wheels)
			{
				if(wheel.powered == false || wheel.grounded == false) continue;
				
				var force = wheel.Forward * (_throttleInput * _attributes.MotorPower * _attributes.PowerCurve.Evaluate(SpeedRatio));
				_rigidbody.AddForceAtPosition(force, wheel.hitPoint);
			}
		}
		
		private void ApplySuspensionForces()
		{
			foreach (var wheel in _wheels)
			{
				if(wheel.grounded == false) continue;
				
				_rigidbody.AddForceAtPosition(wheel.suspensionForce, wheel.RayPosition);
			}
		}
		
		private void CalculateSteering()
		{
			foreach (var wheel in _wheels)
			{
				var velocity = _rigidbody.GetPointVelocity(wheel.RayPosition);
				var slideDot = Vector3.Dot(velocity, wheel.Right);

				if (wheel.skidRenderer)
				{
					wheel.skidRenderer.emitting = Mathf.Abs(slideDot) > _attributes.WheelSkidThreshold && wheel.grounded;
					wheel.skidRenderer.transform.position = wheel.hitPoint;
				}

				wheel.steerForce = wheel.Right * (-slideDot * (_attributes.SteerForce * _attributes.GripCurve.Evaluate(SpeedRatio)));
				wheel.steerForce.y = 0f;
			}
		}
		
		private void CalculateSuspension()
		{
			var numWheelsGrounded = 0;
			
			foreach (var wheel in _wheels)
			{
				var maxSuspensionLength = wheel.restLength + wheel.springTravel;
				
				if (Physics.Raycast(wheel.RayPosition, wheel.Down, out var hit, maxSuspensionLength + wheel.radius, _attributes.DrivableMask))
				{
					var currentSpringLength = hit.distance - wheel.radius;
					var springCompression = (wheel.restLength - currentSpringLength) / wheel.springTravel;

					var springVelocity = Vector3.Dot(_rigidbody.GetPointVelocity(wheel.RayPosition), wheel.Up);
					var dampForce = _attributes.DamperStiffness * springVelocity;
                
					var springForce = springCompression * _attributes.SpringStiffness;
					var netForce = springForce - dampForce;

					wheel.suspensionForce = netForce * Vector3.up;
					wheel.hitPoint = hit.point;
					wheel.grounded = true;
					
					numWheelsGrounded++;
				}
				else
				{
					wheel.grounded = false;
					wheel.hitPoint = wheel.RayPosition + (wheel.Down * (maxSuspensionLength + wheel.radius));
					wheel.suspensionForce = Vector3.zero;
				}
			}
			
			_grounded = numWheelsGrounded > 0;
		}

		private void OnDrawGizmosSelected()
		{
			foreach (var wheel in _wheels)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(wheel.hitPoint, 0.1f);
				
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(wheel.WheelCenter, wheel.radius);
				
				if(!_rigidbody) continue;
				
				Gizmos.DrawSphere(transform.TransformPoint(_rigidbody.centerOfMass), 0.2f);
			}
		}
	}
}