using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
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
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			EntityManager entityManager = World.get_DefaultGameObjectInjectionWorld().get_EntityManager();
			Entity entity = ((EntityManager)(ref entityManager)).CreateEntity((ComponentType[])(object)new ComponentType[1] { ComponentType.op_Implicit(typeof(SingletonPlayableArea)) });
			DynamicBuffer<PlayableAreaNodeElement> orCreateBuffer = entityManager.GetOrCreateBuffer<PlayableAreaNodeElement>(entity);
			for (int i = 0; i < m_PlayableNodes.Count; i++)
			{
				Transform val = m_PlayableNodes[i];
				if ((Object)(object)val != (Object)null)
				{
					orCreateBuffer.Add(new PlayableAreaNodeElement
					{
						Position = float3.op_Implicit(val.get_position())
					});
				}
				else
				{
					Debug.LogError((object)"[PlayableArea] null node found.", (Object)(object)this);
				}
			}
		}

		public PlayableAreaState GetPlayableAreaState(Vector3 position)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			float num = float.MaxValue;
			for (int i = 0; i < m_PlayableNodes.Count; i++)
			{
				float num2 = Vector3.Distance(position, m_PlayableNodes[i].get_position());
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

		public PlayableArea()
			: this()
		{
		}
	}
}
