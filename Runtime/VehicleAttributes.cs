using UnityEngine;

namespace MooshieGames.SimpleVehicles
{
	[CreateAssetMenu(fileName = "New Vehicle Attributes", menuName = "Vehicle/Attributes")]
	public class VehicleAttributes : ScriptableObject
	{
		[Header("Miscellaneous")]
		public LayerMask DrivableMask;
		public float AirborneCorrectionForce = 70f;
		
		[Header("Power Attributes")]
		public float MotorPower = 220f;
		public float MaxSpeed = 16f;
		public AnimationCurve PowerCurve;
		
		[Header("Suspension Attributes")]
		public float SpringStiffness = 300f;
		public float DamperStiffness = 160f;
		public float VisualWheelMoveSpeed = 70f;
		
		[Header("Steering Attributes")]
		public float MaxSteerAngle = 20f;
		public float SteerSpeed = 10f;
		public float SteerForce = 40f;
		public AnimationCurve SteeringStrengthCurve;
		
		[Header("Traction Attributes")]
		[Range(3f, 10f)]
		public float WheelSkidThreshold = 5f;
		public AnimationCurve GripCurve;
	}
}