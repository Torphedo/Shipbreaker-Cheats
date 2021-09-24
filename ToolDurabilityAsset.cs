using System;
using System.Collections.Generic;
using Carbon.Audio;
using Carbon.Audio.Unity;
using Carbon.Core;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	public class ToolDurabilityAsset : EntityComponentDataAssetBase
	{
		[Serializable]
		public class ToolDurabilityData : IToolDurabilityData, IBaseData
		{
			[SerializeField]
			private ToolType m_ToolType;

			[SerializeField]
			[WWiseObjectIDSelector(/*Could not decode attribute arguments.*/)]
			private UnityWwiseObjectIDWrapper m_DurabilityAudioRTPC;

			[SerializeField]
			private float m_DefaultDrainMultiplier = 1f;

			[SerializeField]
			private List<CutterDurabilityDamageDef> m_CutterDamageDefs;

			[SerializeField]
			private List<GrappleDurabilityDamageDef> m_GrappleDamageDefs;

			[SerializeField]
			private List<ScannerDurabilityDamageDef> m_ScannerDamageDefs;

			[SerializeField]
			private List<ThrusterDurabilityDamageDef> m_ThrusterDamageDefs;

			[SerializeField]
			private List<DemoChargeDurabilityDamageDef> m_DemoChargeDamageDefs;

			[SerializeField]
			private List<CutterDurabilityRange> m_CutterDurabilityRanges;

			[SerializeField]
			private List<GrappleDurabilityRange> m_GrappleDurabilityRanges;

			[SerializeField]
			private List<ScannerDurabilityRange> m_ScannerDurabilityRanges;

			[SerializeField]
			private List<ThrusterDurabilityRange> m_ThrusterDurabilityRanges;

			[SerializeField]
			private List<DemoChargeDurabilityRange> m_DemoChargeDurabilityRanges;

			private bool mIsCutterType;

			private bool mIsGrappleType;

			private bool mIsScannerType;

			private bool mIsSuitType;

			private bool mIsThrusterType;

			private bool mIsDemoChargeType;

			public ToolType ToolType => m_ToolType;

			public WwiseShortID DurabilityAudioRTPC
			{
				get
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					WwiseObjectIDWrapper wwiseObjectIDWrapper = ((UnityWwiseObjectIDWrapper)(ref m_DurabilityAudioRTPC)).get_WwiseObjectIDWrapper();
					return ((WwiseObjectIDWrapper)(ref wwiseObjectIDWrapper)).get_ShortID();
				}
			}

			public float DrainMultiplier
			{
				get
				{
					if (GlobalOptions.Raw.GetBool("General.InfDurability") && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
					{
						return m_DefaultDrainMultiplier = 0f;
					}
					return m_DefaultDrainMultiplier = 1f;
				}
			}

			public List<CutterDurabilityDamageDef> CutterDamageDefs => m_CutterDamageDefs;

			public List<GrappleDurabilityDamageDef> GrappleDamageDefs => m_GrappleDamageDefs;

			public List<ScannerDurabilityDamageDef> ScannerDamageDefs => m_ScannerDamageDefs;

			public List<ThrusterDurabilityDamageDef> ThrusterDamageDefs => m_ThrusterDamageDefs;

			public List<DemoChargeDurabilityDamageDef> DemoChargeDamageDefs => m_DemoChargeDamageDefs;

			public List<CutterDurabilityRange> CutterDurabilityRanges => m_CutterDurabilityRanges;

			public List<GrappleDurabilityRange> GrappleDurabilityRanges => m_GrappleDurabilityRanges;

			public List<ScannerDurabilityRange> ScannerDurabilityRanges => m_ScannerDurabilityRanges;

			public List<ThrusterDurabilityRange> ThrusterDurabilityRanges => m_ThrusterDurabilityRanges;

			public List<DemoChargeDurabilityRange> DemoChargeDurabilityRanges => m_DemoChargeDurabilityRanges;

			private bool ValidateToolType(ToolType toolType)
			{
				mIsCutterType = false;
				mIsGrappleType = false;
				mIsScannerType = false;
				mIsSuitType = false;
				mIsThrusterType = false;
				mIsDemoChargeType = false;
				switch (toolType)
				{
				case ToolType.Cutter:
					mIsCutterType = true;
					break;
				case ToolType.Grapple:
					mIsGrappleType = true;
					break;
				case ToolType.Scanner:
					mIsScannerType = true;
					break;
				case ToolType.Suit:
					mIsSuitType = true;
					break;
				case ToolType.Thrusters:
					mIsThrusterType = true;
					break;
				case ToolType.DemoCharge:
					mIsDemoChargeType = true;
					break;
				}
				return true;
			}
		}

		public const float kDefaultDurabilityValue = 100f;

		public const float kHardMaxDurabilityValue = 150f;

		public ToolDurabilityData Data = new ToolDurabilityData();

		public override IEnumerable<Type> GetTypes()
		{
			yield return typeof(DurabilityComponent);
		}

		public override void SetComponentData(EntityManager entityManager, Entity entity)
		{
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			DurabilityComponent durabilityComponent;
			WwiseShortID durabilityAudioRTPC;
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career && PlayerProfileService.Instance.Profile.StoredDurabilityMap.TryGetValue(Data.ToolType, out var value))
			{
				durabilityComponent = default(DurabilityComponent);
				durabilityComponent.ToolType = value.ToolType;
				durabilityAudioRTPC = Data.DurabilityAudioRTPC;
				durabilityComponent.WwiseShortRawObjectID = ((WwiseShortID)(ref durabilityAudioRTPC)).get_ObjectID();
				durabilityComponent.PreviousDurability = value.PreviousDurability;
				durabilityComponent.CurrentDurability = value.CurrentDurability;
				durabilityComponent.MaxDurability = value.MaxDurability;
				DurabilityComponent durabilityComponent2 = durabilityComponent;
				PlayerProfileService.Instance.Profile.UpdateStoredDurability(durabilityComponent2);
				((EntityManager)(ref entityManager)).SetComponentData<DurabilityComponent>(entity, durabilityComponent2);
			}
			else
			{
				durabilityComponent = default(DurabilityComponent);
				durabilityComponent.ToolType = Data.ToolType;
				durabilityAudioRTPC = Data.DurabilityAudioRTPC;
				durabilityComponent.WwiseShortRawObjectID = ((WwiseShortID)(ref durabilityAudioRTPC)).get_ObjectID();
				durabilityComponent.PreviousDurability = 100f;
				durabilityComponent.CurrentDurability = 100f;
				durabilityComponent.MaxDurability = 100f;
				DurabilityComponent durabilityComponent3 = durabilityComponent;
				PlayerProfileService.Instance.Profile.UpdateStoredDurability(durabilityComponent3);
				((EntityManager)(ref entityManager)).SetComponentData<DurabilityComponent>(entity, durabilityComponent3);
			}
		}

		public override void Initialize(EntityManager entityManager, Entity entity)
		{
		}
	}
}
