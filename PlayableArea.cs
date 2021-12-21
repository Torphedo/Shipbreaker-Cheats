using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x020009D0 RID: 2512
	public class PlayableArea : MonoBehaviour
	{
		// Token: 0x17000D3A RID: 3386
		// (get) Token: 0x060031F0 RID: 12784 RVA: 0x000D6DE8 File Offset: 0x000D4FE8
		public float WarningRadius
		{
			get
			{
				return this.m_WarningRadius;
			}
		}

		// Token: 0x17000D3B RID: 3387
		// (get) Token: 0x060031F1 RID: 12785 RVA: 0x000D6DF0 File Offset: 0x000D4FF0
		public float DangerRadius
		{
			get
			{
				return this.m_DangerRadius;
			}
		}

		// Token: 0x17000D3C RID: 3388
		// (get) Token: 0x060031F2 RID: 12786 RVA: 0x000D6DF8 File Offset: 0x000D4FF8
		public float ObjectDangerRadius
		{
			get
			{
				return this.m_ObjectDangerRadius;
			}
		}

		// Token: 0x17000D3D RID: 3389
		// (get) Token: 0x060031F3 RID: 12787 RVA: 0x000D6E00 File Offset: 0x000D5000
		public List<Transform> PlayableNodes
		{
			get
			{
				return this.m_PlayableNodes;
			}
		}

		// Token: 0x060031F4 RID: 12788 RVA: 0x000D6E08 File Offset: 0x000D5008
		private void Awake()
		{
			EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
			Entity entity = entityManager.CreateEntity(new ComponentType[] { typeof(SingletonPlayableArea) });
			DynamicBuffer<PlayableAreaNodeElement> orCreateBuffer = entityManager.GetOrCreateBuffer(entity);
			for (int i = 0; i < this.m_PlayableNodes.Count; i++)
			{
				Transform transform = this.m_PlayableNodes[i];
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

		// Token: 0x060031F5 RID: 12789 RVA: 0x000D6EAC File Offset: 0x000D50AC
		public PlayableArea.PlayableAreaState GetPlayableAreaState(Vector3 position)
		{
			float num = float.MaxValue;
			for (int i = 0; i < this.m_PlayableNodes.Count; i++)
			{
				float num2 = Vector3.Distance(position, this.m_PlayableNodes[i].position);
				if (num2 < this.m_WarningRadius)
				{
					return PlayableArea.PlayableAreaState.Safe;
				}
				if (num2 < num)
				{
					num = num2;
				}
			}
			if (num < this.m_DangerRadius)
			{
				return PlayableArea.PlayableAreaState.Warning;
			}
			return PlayableArea.PlayableAreaState.Danger;
		}

		// Token: 0x040025E5 RID: 9701
		[SerializeField]
		private float m_WarningRadius = 100f;

		// Token: 0x040025E6 RID: 9702
		[SerializeField]
		private float m_DangerRadius = 125f;

		// Token: 0x040025E7 RID: 9703
		[SerializeField]
		private float m_ObjectDangerRadius = 130f;

		// Token: 0x040025E8 RID: 9704
		[SerializeField]
		private List<Transform> m_PlayableNodes = new List<Transform>();

		// Token: 0x02000E9B RID: 3739
		public enum PlayableAreaState
		{
			// Token: 0x04004490 RID: 17552
			None,
			// Token: 0x04004491 RID: 17553
			Safe,
			// Token: 0x04004492 RID: 17554
			Warning,
			// Token: 0x04004493 RID: 17555
			Danger
		}
	}
}
