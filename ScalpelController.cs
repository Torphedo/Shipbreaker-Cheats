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

		private float mInstantMelt;

		private float mIgnorePowerRating;

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
			mCuttingToolController = GetComponentInParent<CuttingToolController>();
			if (mCuttingToolController == null)
			{
				Debug.LogError("[CuttingTool] Unable to find CuttingToolController. Disabling.");
				base.enabled = false;
			}
			else if (m_ScalpelAsset == null)
			{
				Debug.LogError("[CuttingTool] Scalpel Asset is missing.");
				base.enabled = false;
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
			StructurePart part = Target.Part;
			StructureGroup structureGroup = ((Target.Part != null) ? Target.Part.Group : null);
			StructureGroup structureGroup2 = null;
			int targetTypeRTPCValue = GetTargetTypeRTPCValue(Target);
			if (Physics.Raycast(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward, out var hitInfo, mData.Range, mData.LayerMask))
			{
				Target.Position = hitInfo.point;
				Target.Normal = hitInfo.normal;
				Target.Part = hitInfo.collider.transform.GetComponentInParent<StructurePart>();
				Target.GameObject = hitInfo.collider.gameObject;
				Target.IsValid = IsValidTargetable(Target.Part);
				structureGroup2 = ((Target.Part != null) ? Target.Part.Group : null);
			}
			else
			{
				Target.Reset(mData.Range);
			}
			if (mCuttingToolController.State == CuttingState.Cutting && part != Target.Part && (structureGroup2 == null || structureGroup != structureGroup2))
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
			if (LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		private void HandleCuttingState()
		{
			if (!LynxControls.Instance.GameplayActions.CutterFire.IsPressed)
			{
				mCuttingToolController.DelayCooldown();
				mCuttingToolController.SetState(CuttingState.Ready);
				return;
			}
			float amount = Mathf.Lerp(mData.ControllerVibrationIntensity.Min, mData.ControllerVibrationIntensity.Max, mCuttingToolController.CurrentHeatPercent);
			InputManager.ActiveDevice.Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
			if (Time.time - mCameraShakeStartTime >= mData.CuttingCameraShakeSettings.ShakeDuration)
			{
				mCameraShakeStartTime = Time.time;
				Main.EventSystem.Post(CameraShakeEvent.GetGlobalEvent(mData.CuttingCameraShakeSettings));
			}
			StructurePart part = Target.Part;
			if (part == null || part.CuttingTargetable == null)
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
				Entity entity = (isPartOfGroup ? Target.Part.Group.EntityBlueprintComponent.Entity : Target.Part.EntityBlueprintComponent.Entity);
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
				mInstantMelt = 0f;
				if (!GlobalOptions.Raw.GetBool("General.InstantMelt") || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
				{
					float num = (mInstantMelt = Target.Part.CuttingTargetable.GetMeltingTime(mass));
				}
				if (entityManager.TryGetComponent<VaporizationComponent>(entity, out var component))
				{
					component.ModifiedThisFrame = true;
					component.VaporizationAmount += Time.deltaTime;
					component.MaxVaporization = mInstantMelt;
					component.Type = VaporizationType.Scalpel;
					entityManager.SetComponentData(entity, component);
				}
				else
				{
					entityManager.AddComponentData(entity, new VaporizationComponent
					{
						ModifiedThisFrame = true,
						VaporizationAmount = Time.deltaTime,
						MaxVaporization = mInstantMelt,
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
			if (IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
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
				mCameraShakeStartTime = Time.time;
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
			InputManager.ActiveDevice.StopVibration();
			Main.EventSystem.Post(ScalpelStateChangedEvent.GetEvent(newState));
		}

		private bool IsValidTargetable(StructurePart part)
		{
			mIgnorePowerRating = mData.PowerRating;
			if (GlobalOptions.Raw.GetBool("General.IgnoreCutGrade") || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				mIgnorePowerRating = 5f;
			}
			if (part != null && part.CuttingTargetable != null && part.CuttingTargetable.IsScalpelCuttable() && mIgnorePowerRating >= (float)part.CuttingTargetable.PowerRating && EntityBlueprintComponent.IsValid(part.EntityBlueprintComponent))
			{
				if (!(part.Group == null))
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
			if (target.Part != null)
			{
				return 2;
			}
			return 0;
		}

		private void UpdateTargetTypeRTPC(int value)
		{
			Main.EventSystem.Post(SetRTPCEvent.GetMasterEvent(mData.ScalpelTargetTypeAudioParameter, value));
		}
	}
}
