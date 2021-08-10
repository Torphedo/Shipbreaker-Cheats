using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	public class PlayableArea : MonoBehaviour
	{
		public enum PlayableAreaState
		{
			None,
			Safe,
			Warning,
			Danger
		}

		[SerializeField]
		private float m_WarningRadius = 100f;

		[SerializeField]
		private float m_DangerRadius = 125f;

		[SerializeField]
		private float m_ObjectDangerRadius = 130f;

		[SerializeField]
		private List<Transform> m_PlayableNodes = new List<Transform>();

		public float WarningRadius => m_WarningRadius;

		public float DangerRadius => m_DangerRadius;

		public float ObjectDangerRadius => m_ObjectDangerRadius;

		public List<Transform> PlayableNodes => m_PlayableNodes;

		private void Awake()
		{
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Entity entity = entityManager.CreateEntity(typeof(SingletonPlayableArea));
			DynamicBuffer<PlayableAreaNodeElement> orCreateBuffer = entityManager.GetOrCreateBuffer<PlayableAreaNodeElement>(entity);
			for (int i = 0; i < m_PlayableNodes.Count; i++)
			{
				Transform transform = m_PlayableNodes[i];
				if (transform != null)
				{
					orCreateBuffer.Add(new PlayableAreaNodeElement
					{
						Position = transform.position
					});
				}
				else
				{
					Debug.LogError("[PlayableArea] null node found.", this);
				}
			}
		}

		public PlayableAreaState GetPlayableAreaState(Vector3 position)
		{
			float num = float.MaxValue;
			for (int i = 0; i < m_PlayableNodes.Count; i++)
			{
				float num2 = Vector3.Distance(position, m_PlayableNodes[i].position);
				if (num2 < m_WarningRadius)
				{
					return PlayableAreaState.Safe;
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			_ = m_DangerRadius;
			return PlayableAreaState.Safe;
		}
	}
}
