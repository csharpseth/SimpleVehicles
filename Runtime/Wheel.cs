using System;
using UnityEngine;

namespace MooshieGames.SimpleVehicles
{
	[Serializable]
	public class Wheel
	{
		public Transform rayPoint;
		[HideInInspector]
		public Transform wheelVisual;
		public float radius;
		public bool powered;
		public bool steering;
		public TrailRenderer skidRenderer;

		public Vector3 RayPosition => rayPoint.position;
		public Vector3 Up => rayPoint.up;
		public Vector3 Down => -rayPoint.up;

		public Vector3 Forward => rayPoint.forward;
		public Vector3 Right => rayPoint.right;
		public Vector3 WheelCenter => hitPoint + (rayPoint.up * radius);
		
		public float restLength = 0.5f;
		public float springTravel = 0.25f;
		
		[HideInInspector]
		public bool grounded;
		[HideInInspector]
		public Vector3 suspensionForce;
		[HideInInspector]
		public Vector3 steerForce;
		[HideInInspector]
		public Vector3 hitPoint;

		private Vector3 _defaultLocalPosition;
		private Quaternion _defaultLocalRotation;
		
		public void Setup()
		{
			if(!wheelVisual) wheelVisual = rayPoint.GetChild(0);
			
			_defaultLocalPosition = wheelVisual.localPosition;
			_defaultLocalRotation = wheelVisual.localRotation;
		}

		public void ResetVisual()
		{
			wheelVisual.localPosition = _defaultLocalPosition;
			wheelVisual.localRotation = _defaultLocalRotation;
		}
	}
}