using System;
using System.Collections.Generic;
using Carbon.Audio;
using Carbon.Audio.Unity;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000169 RID: 361
	public class ToolDurabilityAsset : EntityComponentDataAssetBase
	{
		// Token: 0x06000819 RID: 2073 RVA: 0x00027C9A File Offset: 0x00025E9A
		public override IEnumerable<Type> GetTypes()
		{
			yield return typeof(DurabilityComponent);
			yield break;
		}

		// Token: 0x0600081A RID: 2074 RVA: 0x00027CA4 File Offset: 0x00025EA4
		public override void SetComponentData(EntityManager entityManager, Entity entity)
		{
			DurabilityComponent durabilityComponent;
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career && PlayerProfileService.Instance.Profile.StoredDurabilityMap.TryGetValue(this.Data.ToolType, out durabilityComponent))
			{
				DurabilityComponent durabilityComponent2 = new DurabilityComponent
				{
					ToolType = durabilityComponent.ToolType,
					WwiseShortRawObjectID = this.Data.DurabilityAudioRTPC.ObjectID,
					PreviousDurability = durabilityComponent.PreviousDurability,
					CurrentDurability = durabilityComponent.CurrentDurability,
					MaxDurability = durabilityComponent.MaxDurability
				};
				PlayerProfileService.Instance.Profile.UpdateStoredDurability(durabilityComponent2);
				entityManager.SetComponentData<DurabilityComponent>(entity, durabilityComponent2);
				return;
			}
			DurabilityComponent durabilityComponent3 = new DurabilityComponent
			{
				ToolType = this.Data.ToolType,
				WwiseShortRawObjectID = this.Data.DurabilityAudioRTPC.ObjectID,
				PreviousDurability = 100f,
				CurrentDurability = 100f,
				MaxDurability = 100f
			};
			PlayerProfileService.Instance.Profile.UpdateStoredDurability(durabilityComponent3);
			entityManager.SetComponentData<DurabilityComponent>(entity, durabilityComponent3);
		}

		// Token: 0x0600081B RID: 2075 RVA: 0x00003469 File Offset: 0x00001669
		public override void Initialize(EntityManager entityManager, Entity entity)
		{
		}

		// Token: 0x04000839 RID: 2105
		public const float kDefaultDurabilityValue = 100f;

		// Token: 0x0400083A RID: 2106
		public const float kHardMaxDurabilityValue = 150f;

		// Token: 0x0400083B RID: 2107
		public ToolDurabilityAsset.ToolDurabilityData Data = new ToolDurabilityAsset.ToolDurabilityData();

		// Token: 0x02000BF2 RID: 3058
		[Serializable]
		public class ToolDurabilityData : IToolDurabilityData, IBaseData
		{
			// Token: 0x17001118 RID: 4376
			// (get) Token: 0x06004164 RID: 16740 RVA: 0x00129721 File Offset: 0x00127921
			public ToolType ToolType
			{
				get
				{
					return this.m_ToolType;
				}
			}

			// Token: 0x17001119 RID: 4377
			// (get) Token: 0x06004165 RID: 16741 RVA: 0x0012972C File Offset: 0x0012792C
			public WwiseShortID DurabilityAudioRTPC
			{
				get
				{
					return this.m_DurabilityAudioRTPC.WwiseObjectIDWrapper.ShortID;
				}
			}

			// Token: 0x1700111A RID: 4378
			// (get) Token: 0x06004166 RID: 16742 RVA: 0x0012974C File Offset: 0x0012794C
			public float DrainMultiplier
			{
				get
				{
					return this.m_DefaultDrainMultiplier;
				}
			}

			// Token: 0x1700111B RID: 4379
			// (get) Token: 0x06004167 RID: 16743 RVA: 0x00129754 File Offset: 0x00127954
			public List<CutterDurabilityDamageDef> CutterDamageDefs
			{
				get
				{
					return this.m_CutterDamageDefs;
				}
			}

			// Token: 0x1700111C RID: 4380
			// (get) Token: 0x06004168 RID: 16744 RVA: 0x0012975C File Offset: 0x0012795C
			public List<GrappleDurabilityDamageDef> GrappleDamageDefs
			{
				get
				{
					return this.m_GrappleDamageDefs;
				}
			}

			// Token: 0x1700111D RID: 4381
			// (get) Token: 0x06004169 RID: 16745 RVA: 0x00129764 File Offset: 0x00127964
			public List<ScannerDurabilityDamageDef> ScannerDamageDefs
			{
				get
				{
					return this.m_ScannerDamageDefs;
				}
			}

			// Token: 0x1700111E RID: 4382
			// (get) Token: 0x0600416A RID: 16746 RVA: 0x0012976C File Offset: 0x0012796C
			public List<ThrusterDurabilityDamageDef> ThrusterDamageDefs
			{
				get
				{
					return this.m_ThrusterDamageDefs;
				}
			}

			// Token: 0x1700111F RID: 4383
			// (get) Token: 0x0600416B RID: 16747 RVA: 0x00129774 File Offset: 0x00127974
			public List<DemoChargeDurabilityDamageDef> DemoChargeDamageDefs
			{
				get
				{
					return this.m_DemoChargeDamageDefs;
				}
			}

			// Token: 0x17001120 RID: 4384
			// (get) Token: 0x0600416C RID: 16748 RVA: 0x0012977C File Offset: 0x0012797C
			public List<CutterDurabilityRange> CutterDurabilityRanges
			{
				get
				{
					return this.m_CutterDurabilityRanges;
				}
			}

			// Token: 0x17001121 RID: 4385
			// (get) Token: 0x0600416D RID: 16749 RVA: 0x00129784 File Offset: 0x00127984
			public List<GrappleDurabilityRange> GrappleDurabilityRanges
			{
				get
				{
					return this.m_GrappleDurabilityRanges;
				}
			}

			// Token: 0x17001122 RID: 4386
			// (get) Token: 0x0600416E RID: 16750 RVA: 0x0012978C File Offset: 0x0012798C
			public List<ScannerDurabilityRange> ScannerDurabilityRanges
			{
				get
				{
					return this.m_ScannerDurabilityRanges;
				}
			}

			// Token: 0x17001123 RID: 4387
			// (get) Token: 0x0600416F RID: 16751 RVA: 0x00129794 File Offset: 0x00127994
			public List<ThrusterDurabilityRange> ThrusterDurabilityRanges
			{
				get
				{
					return this.m_ThrusterDurabilityRanges;
				}
			}

			// Token: 0x17001124 RID: 4388
			// (get) Token: 0x06004170 RID: 16752 RVA: 0x0012979C File Offset: 0x0012799C
			public List<DemoChargeDurabilityRange> DemoChargeDurabilityRanges
			{
				get
				{
					return this.m_DemoChargeDurabilityRanges;
				}
			}

			// Token: 0x06004171 RID: 16753 RVA: 0x001297A4 File Offset: 0x001279A4
			private bool ValidateToolType(ToolType toolType)
			{
				this.mIsCutterType = false;
				this.mIsGrappleType = false;
				this.mIsScannerType = false;
				this.mIsSuitType = false;
				this.mIsThrusterType = false;
				this.mIsDemoChargeType = false;
				switch (toolType)
				{
				case ToolType.Cutter:
					this.mIsCutterType = true;
					break;
				case ToolType.Grapple:
					this.mIsGrappleType = true;
					break;
				case ToolType.Scanner:
					this.mIsScannerType = true;
					break;
				case ToolType.Suit:
					this.mIsSuitType = true;
					break;
				case ToolType.Thrusters:
					this.mIsThrusterType = true;
					break;
				case ToolType.DemoCharge:
					this.mIsDemoChargeType = true;
					break;
				}
				return true;
			}

			// Token: 0x040036C4 RID: 14020
			[SerializeField]
			private ToolType m_ToolType;

			// Token: 0x040036C5 RID: 14021
			[SerializeField]
			[WWiseObjectIDSelector(3, true)]
			private UnityWwiseObjectIDWrapper m_DurabilityAudioRTPC;

			// Token: 0x040036C6 RID: 14022
			[SerializeField]
			private float m_DefaultDrainMultiplier = 1f;

			// Token: 0x040036C7 RID: 14023
			[SerializeField]
			private List<CutterDurabilityDamageDef> m_CutterDamageDefs;

			// Token: 0x040036C8 RID: 14024
			[SerializeField]
			private List<GrappleDurabilityDamageDef> m_GrappleDamageDefs;

			// Token: 0x040036C9 RID: 14025
			[SerializeField]
			private List<ScannerDurabilityDamageDef> m_ScannerDamageDefs;

			// Token: 0x040036CA RID: 14026
			[SerializeField]
			private List<ThrusterDurabilityDamageDef> m_ThrusterDamageDefs;

			// Token: 0x040036CB RID: 14027
			[SerializeField]
			private List<DemoChargeDurabilityDamageDef> m_DemoChargeDamageDefs;

			// Token: 0x040036CC RID: 14028
			[SerializeField]
			private List<CutterDurabilityRange> m_CutterDurabilityRanges;

			// Token: 0x040036CD RID: 14029
			[SerializeField]
			private List<GrappleDurabilityRange> m_GrappleDurabilityRanges;

			// Token: 0x040036CE RID: 14030
			[SerializeField]
			private List<ScannerDurabilityRange> m_ScannerDurabilityRanges;

			// Token: 0x040036CF RID: 14031
			[SerializeField]
			private List<ThrusterDurabilityRange> m_ThrusterDurabilityRanges;

			// Token: 0x040036D0 RID: 14032
			[SerializeField]
			private List<DemoChargeDurabilityRange> m_DemoChargeDurabilityRanges;

			// Token: 0x040036D1 RID: 14033
			private bool mIsCutterType;

			// Token: 0x040036D2 RID: 14034
			private bool mIsGrappleType;

			// Token: 0x040036D3 RID: 14035
			private bool mIsScannerType;

			// Token: 0x040036D4 RID: 14036
			private bool mIsSuitType;

			// Token: 0x040036D5 RID: 14037
			private bool mIsThrusterType;

			// Token: 0x040036D6 RID: 14038
			private bool mIsDemoChargeType;
		}
	}
}
