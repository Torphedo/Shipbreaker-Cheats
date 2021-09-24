using System;
using BBI.Core.Utility;
using Carbon.Core;
using InControl;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	public class ScalpelController : DebuggableMonoBehaviour
	{
		[SerializeField]
		private ScalpelAsset m_ScalpelAsset;

		private IScalpelData mData;

		private bool mCanBeUnequipped = true;

		private float mCameraShakeStartTime;

		private CuttingToolController mCuttingToolController;

		private const int kTargetTypeNothing = 0;

		private const int kTargetTypeValid = 1;

		private const int kTargetTypeInvalid = 2;

		private float m_MeltTime = GlobalOptions.Raw.GetFloat("General.StingerMeltTime", -1f);

		private float mSetPowerRating;

		public IScalpelData Data => mData;

		public ScalpelTarget Target { get; } = new ScalpelTarget();


		public bool CanBeUnequipped => mCanBeUnequipped;

		public bool IsCurrentEquipment
		{
			get
			{
				if (mCuttingToolController.EquipementController.CurrentEquipment == EquipmentController.Equipment.CuttingTool)
				{
					return mCuttingToolController.CurrentMode == CuttingToolController.CutterMode.Scalpel;
				}
				return false;
			}
		}

		private void Awake()
		{
			mCuttingToolController = ((Component)this).GetComponentInParent<CuttingToolController>();
			if ((Object)(object)mCuttingToolController == (Object)null)
			{
				Debug.LogError((object)"[CuttingTool] Unable to find CuttingToolController. Disabling.");
				((Behaviour)this).set_enabled(false);
			}
			else if ((Object)(object)m_ScalpelAsset == (Object)null)
			{
				Debug.LogError((object)"[CuttingTool] Scalpel Asset is missing.");
				((Behaviour)this).set_enabled(false);
			}
			else
			{
				mData = new ScalpelBuffableData(m_ScalpelAsset.Data);
			}
		}

		public void UpdateScalpel()
		{
			UpdateTargeting();
			switch (mCuttingToolController.State)
			{
			case CuttingState.Ready:
				HandleReadyState();
				break;
			case CuttingState.Cutting:
				HandleCuttingState();
				break;
			case CuttingState.Disabled:
				HandleDisabledState();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private void UpdateTargeting()
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			StructurePart part = Target.Part;
			StructureGroup structureGroup = (((Object)(object)Target.Part != (Object)null) ? Target.Part.Group : null);
			StructureGroup structureGroup2 = null;
			int targetTypeRTPCValue = GetTargetTypeRTPCValue(Target);
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(LynxCameraController.MainCameraTransform.get_position(), LynxCameraController.MainCameraTransform.get_forward(), ref val, mData.Range, LayerMask.op_Implicit(mData.LayerMask)))
			{
				Target.Position = ((RaycastHit)(ref val)).get_point();
				Target.Normal = ((RaycastHit)(ref val)).get_normal();
				Target.Part = ((Component)((Component)((RaycastHit)(ref val)).get_collider()).get_transform()).GetComponentInParent<StructurePart>();
				Target.GameObject = ((Component)((RaycastHit)(ref val)).get_collider()).get_gameObject();
				Target.IsValid = IsValidTargetable(Target.Part);
				structureGroup2 = (((Object)(object)Target.Part != (Object)null) ? Target.Part.Group : null);
			}
			else
			{
				Target.Reset(mData.Range);
			}
			if (mCuttingToolController.State == CuttingState.Cutting && (Object)(object)part != (Object)(object)Target.Part && ((Object)(object)structureGroup2 == (Object)null || (Object)(object)structureGroup != (Object)(object)structureGroup2))
			{
				int targetTypeRTPCValue2 = GetTargetTypeRTPCValue(Target);
				if (targetTypeRTPCValue2 != 2 || targetTypeRTPCValue != 2)
				{
					Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.ScalpelTargetChangedAudioEvent));
				}
				UpdateTargetTypeRTPC(targetTypeRTPCValue2);
			}
		}

		private void HandleReadyState()
		{
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterFire).get_WasPressed())
			{
				mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		private void HandleCuttingState()
		{
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0243: Unknown result type (might be due to invalid IL or missing references)
			if (!((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterFire).get_IsPressed())
			{
				mCuttingToolController.DelayCooldown();
				mCuttingToolController.SetState(CuttingState.Ready);
				return;
			}
			float amount = Mathf.Lerp(mData.ControllerVibrationIntensity.Min, mData.ControllerVibrationIntensity.Max, mCuttingToolController.CurrentHeatPercent);
			InputManager.get_ActiveDevice().Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
			if (Time.get_time() - mCameraShakeStartTime >= mData.CuttingCameraShakeSettings.ShakeDuration)
			{
				mCameraShakeStartTime = Time.get_time();
				Main.EventSystem.Post(CameraShakeEvent.GetGlobalEvent(mData.CuttingCameraShakeSettings));
			}
			StructurePart part = Target.Part;
			if ((Object)(object)part == (Object)null || part.CuttingTargetable == null)
			{
				mCuttingToolController.UpdateCutGradeRTPC(0);
			}
			else
			{
				mCuttingToolController.UpdateCutGradeRTPC(Target.Part.CuttingTargetable.PowerRating);
			}
			if (Target.IsValid)
			{
				bool isPartOfGroup = Target.Part.IsPartOfGroup;
				Entity val = (isPartOfGroup ? Target.Part.Group.EntityBlueprintComponent.Entity : Target.Part.EntityBlueprintComponent.Entity);
				EntityManager entityManager = (isPartOfGroup ? Target.Part.Group.EntityBlueprintComponent.EntityManager : Target.Part.EntityBlueprintComponent.EntityManager);
				float mass = 0f;
				if (isPartOfGroup)
				{
					mass = Target.Part.Group.GetTotalGroupMass();
				}
				else
				{
					Target.Part.TryGetPartMass(out mass);
				}
				if (m_MeltTime < 0f || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
				{
					m_MeltTime = Target.Part.CuttingTargetable.GetMeltingTime(mass);
				}
				if (entityManager.TryGetComponent<VaporizationComponent>(val, out VaporizationComponent component))
				{
					component.ModifiedThisFrame = true;
					component.VaporizationAmount += Time.get_deltaTime();
					component.MaxVaporization = m_MeltTime;
					component.Type = VaporizationType.Scalpel;
					((EntityManager)(ref entityManager)).SetComponentData<VaporizationComponent>(val, component);
				}
				else
				{
					((EntityManager)(ref entityManager)).AddComponentData<VaporizationComponent>(val, new VaporizationComponent
					{
						ModifiedThisFrame = true,
						VaporizationAmount = Time.get_deltaTime(),
						MaxVaporization = m_MeltTime,
						Type = VaporizationType.Scalpel
					});
				}
			}
			if (!GlobalOptions.Raw.GetBool("General.InfHeat") || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				mCuttingToolController.AddHeat();
			}
			mCuttingToolController.DelayCooldown();
		}

		private void HandleDisabledState()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if (IsCurrentEquipment && ((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterFire).get_WasPressed())
			{
				mCuttingToolController.EquipCutter();
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(mCuttingToolController.Data.CutDeniedAudioEvent));
			}
		}

		public void OnStateChanged(CuttingState newState)
		{
			switch (newState)
			{
			case CuttingState.Ready:
				mCameraShakeStartTime = 0f;
				mCanBeUnequipped = true;
				break;
			case CuttingState.Cutting:
				mCuttingToolController.EquipCutter();
				mCameraShakeStartTime = Time.get_time();
				Main.EventSystem.Post(CameraShakeEvent.GetGlobalEvent(mData.CuttingCameraShakeSettings));
				mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.ScalpelIsActive, () => IsCurrentEquipment && mCuttingToolController.State == CuttingState.Cutting);
				UpdateTargetTypeRTPC(GetTargetTypeRTPCValue(Target));
				mCanBeUnequipped = false;
				break;
			case CuttingState.Disabled:
				mCameraShakeStartTime = 0f;
				mCanBeUnequipped = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			InputManager.get_ActiveDevice().StopVibration();
			Main.EventSystem.Post(ScalpelStateChangedEvent.GetEvent(newState));
		}

		private bool IsValidTargetable(StructurePart part)
		{
			mSetPowerRating = GlobalOptions.Raw.GetFloat("General.StingerCutGrade", 1f);
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				mSetPowerRating = mData.PowerRating;
			}
			if ((Object)(object)part != (Object)null && part.CuttingTargetable != null && part.CuttingTargetable.IsScalpelCuttable() && mSetPowerRating >= (float)part.CuttingTargetable.PowerRating && EntityBlueprintComponent.IsValid(part.EntityBlueprintComponent))
			{
				if (!((Object)(object)part.Group == (Object)null))
				{
					return EntityBlueprintComponent.IsValid(part.Group.EntityBlueprintComponent);
				}
				return true;
			}
			return false;
		}

		private int GetTargetTypeRTPCValue(ScalpelTarget target)
		{
			if (target.IsValid)
			{
				return 1;
			}
			if ((Object)(object)target.Part != (Object)null)
			{
				return 2;
			}
			return 0;
		}

		private void UpdateTargetTypeRTPC(int value)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Main.EventSystem.Post(SetRTPCEvent.GetMasterEvent(mData.ScalpelTargetTypeAudioParameter, value));
		}
	}
}
