using UnityEngine;

namespace Domain
{
	public abstract class BaseCamera
	{
		private float m_Pitch, m_Jaw;

		public Vector2 GetDirection { get; set; }
		
		public abstract void Rotate();
	}
}