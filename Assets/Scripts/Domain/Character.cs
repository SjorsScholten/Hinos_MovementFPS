using UnityEngine;

namespace Domain
{
	public class Character
	{
		//Transform Attributes
		private Vector3 m_Position;
		private Quaternion m_Rotation;
		
		//Physical Attributes
		private Vector3 m_Velocity;

		public Vector3 Position
		{
			get => m_Position;
			set => m_Position = value;
		}

		public Quaternion Rotation
		{
			get => m_Rotation;
			set => m_Rotation = value;
		}

		public Vector3 Velocity
		{
			get => m_Velocity;
			set => m_Velocity = value;
		}

		public Character() { }
	}
}