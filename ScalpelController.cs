using System;
using BBI.Core.Utility;
using Carbon.Core;
using InControl;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000618 RID: 1560
	public class ScalpelController : DebuggableMonoBehaviour
	{
		// Token: 0x170008C4 RID: 2244
		// (get) Token: 0x06001F0D RID: 7949 RVA: 0x00084B10 File Offset: 0x00082D10
		public IScalpelData Data
		{
			get
			{
				return this.mData;
			}
		}

		// Token: 0x170008C5 RID: 2245
		// (get) Token: 0x06001F0E RID: 7950 RVA: 0x00084B18 File Offset: 0x00082D18
		public ScalpelTarget Target { get; } = new ScalpelTarget();

		// Token: 0x170008C6 RID: 2246
		// (get) Token: 0x06001F0F RID: 7951 RVA: 0x00084B20 File Offset: 0x00082D20
		public bool CanBeUnequipped
		{
			get
			{
				return this.mCanBeUnequipped;
			}
		}

		// Token: 0x170008C7 RID: 2247
		// (get) Token: 0x06001F10 RID: 7952 RVA: 0x00084B28 File Offset: 0x00082D28
		public bool IsCurrentEquipment
		{
			get
			{
				return this.mCuttingToolController.EquipementController.CurrentEquipment == EquipmentController.Equipment.CuttingTool && this.mCuttingToolController.CurrentMode == CuttingToolController.CutterMode.Scalpel;
			}
		}

		// Token: 0x06001F11 RID: 7953 RVA: 0x00084B50 File Offset: 0x00082D50
		private void Awake()
		{
			this.mCuttingToolController = base.GetComponentInParent<CuttingToolController>();
			if (this.mCuttingToolController == null)
			{
				Debug.LogError("[CuttingTool] Unable to find CuttingToolController. Disabling.");
				base.enabled = false;
				return;
			}
			if (this.m_ScalpelAsset == null)
			{
				Debug.LogError("[CuttingTool] Scalpel Asset is missing.");
				base.enabled = false;
				return;
			}
			this.mData = new ScalpelBuffableData(this.m_ScalpelAsset.Data);
		}

		// Token: 0x06001F12 RID: 7954 RVA: 0x00084BC0 File Offset: 0x00082DC0
		public void UpdateScalpel()
		{
			this.UpdateTargeting();
			switch (this.mCuttingToolController.State)
			{
			case CuttingState.Ready:
				this.HandleReadyState();
				return;
			case CuttingState.Cutting:
				this.HandleCuttingState();
				return;
			case CuttingState.Disabled:
				this.HandleDisabledState();
				return;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x00084C10 File Offset: 0x00082E10
		private void UpdateTargeting()
		{
			StructurePart part = this.Target.Part;
			StructureGroup structureGroup = ((this.Target.Part != null) ? this.Target.Part.Group : null);
			StructureGroup structureGroup2 = null;
			int targetTypeRTPCValue = this.GetTargetTypeRTPCValue(this.Target);
			RaycastHit raycastHit;
			if (Physics.Raycast(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward, ref raycastHit, this.mData.Range, this.mData.LayerMask))
			{
				this.Target.Position = raycastHit.point;
				this.Target.Normal = raycastHit.normal;
				this.Target.Part = raycastHit.collider.transform.GetComponentInParent<StructurePart>();
				this.Target.GameObject = raycastHit.collider.gameObject;
				this.Target.IsValid = this.IsValidTargetable(this.Target.Part);
				structureGroup2 = ((this.Target.Part != null) ? this.Target.Part.Group : null);
			}
			else
			{
				this.Target.Reset(this.mData.Range);
			}
			if (this.mCuttingToolController.State == CuttingState.Cutting && part != this.Target.Part && (structureGroup2 == null || structureGroup != structureGroup2))
			{
				int targetTypeRTPCValue2 = this.GetTargetTypeRTPCValue(this.Target);
				if (targetTypeRTPCValue2 != 2 || targetTypeRTPCValue != 2)
				{
					Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.ScalpelTargetChangedAudioEvent));
				}
				this.UpdateTargetTypeRTPC(targetTypeRTPCValue2);
			}
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x00084DB8 File Offset: 0x00082FB8
		private void HandleReadyState()
		{
			if (LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				this.mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x00084DDC File Offset: 0x00082FDC
		private void HandleCuttingState()
		{
			if (!LynxControls.Instance.GameplayActions.CutterFire.IsPressed)
			{
				this.mCuttingToolController.DelayCooldown();
				this.mCuttingToolController.SetState(CuttingState.Ready);
				return;
			}
			float amount = Mathf.Lerp(this.mData.ControllerVibrationIntensity.Min, this.mData.ControllerVibrationIntensity.Max, this.mCuttingToolController.CurrentHeatPercent);
			InputManager.ActiveDevice.Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
			if (Time.time - this.mCameraShakeStartTime >= this.mData.CuttingCameraShakeSettings.ShakeDuration)
			{
				this.mCameraShakeStartTime = Time.time;
				Main.EventSystem.Post(CameraShakeEvent.GetGlobalEvent(this.mData.CuttingCameraShakeSettings));
			}
			StructurePart part = this.Target.Part;
			if (part == null || part.CuttingTargetable == null)
			{
				this.mCuttingToolController.UpdateCutGradeRTPC(0);
			}
			else
			{
				this.mCuttingToolController.UpdateCutGradeRTPC(this.Target.Part.CuttingTargetable.PowerRating);
			}
			if (this.Target.IsValid)
			{
				bool isPartOfGroup = this.Target.Part.IsPartOfGroup;
				Entity entity = (isPartOfGroup ? this.Target.Part.Group.EntityBlueprintComponent.Entity : this.Target.Part.EntityBlueprintComponent.Entity);
				EntityManager entityManager = (isPartOfGroup ? this.Target.Part.Group.EntityBlueprintComponent.EntityManager : this.Target.Part.EntityBlueprintComponent.EntityManager);
				float mass = 0f;
				if (isPartOfGroup)
				{
					mass = this.Target.Part.Group.GetTotalGroupMass();
				}
				else
				{
					this.Target.Part.TryGetPartMass(out mass);
				}
				if (this.m_MeltTime < 0f || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
				{
					this.m_MeltTime = this.Target.Part.CuttingTargetable.GetMeltingTime(mass);
				}
				VaporizationComponent vaporizationComponent;
				if (entityManager.TryGetComponent(entity, out vaporizationComponent))
				{
					vaporizationComponent.ModifiedThisFrame = true;
					vaporizationComponent.VaporizationAmount += Time.deltaTime;
					vaporizationComponent.MaxVaporization = this.m_MeltTime;
					vaporizationComponent.Type = VaporizationType.Scalpel;
					entityManager.SetComponentData<VaporizationComponent>(entity, vaporizationComponent);
				}
				else
				{
					entityManager.AddComponentData<VaporizationComponent>(entity, new VaporizationComponent
					{
						ModifiedThisFrame = true,
						VaporizationAmount = Time.deltaTime,
						MaxVaporization = this.m_MeltTime,
						Type = VaporizationType.Scalpel
					});
				}
			}
			if (!GlobalOptions.Raw.GetBool("General.InfHeat", false) || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				this.mCuttingToolController.AddHeat();
			}
			this.mCuttingToolController.DelayCooldown();
		}

		// Token: 0x06001F16 RID: 7958 RVA: 0x00085054 File Offset: 0x00083254
		private void HandleDisabledState()
		{
			if (this.IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				this.mCuttingToolController.EquipCutter();
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mCuttingToolController.Data.CutDeniedAudioEvent));
			}
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x000850AC File Offset: 0x000832AC
		public void OnStateChanged(CuttingState newState)
		{
			switch (newState)
			{
			case CuttingState.Ready:
				this.mCameraShakeStartTime = 0f;
				this.mCanBeUnequipped = true;
				break;
			case CuttingState.Cutting:
				this.mCuttingToolController.EquipCutter();
				this.mCameraShakeStartTime = Time.time;
				Main.EventSystem.Post(CameraShakeEvent.GetGlobalEvent(this.mData.CuttingCameraShakeSettings));
				this.mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.ScalpelIsActive, () => this.IsCurrentEquipment && this.mCuttingToolController.State == CuttingState.Cutting);
				this.UpdateTargetTypeRTPC(this.GetTargetTypeRTPCValue(this.Target));
				this.mCanBeUnequipped = false;
				break;
			case CuttingState.Disabled:
				this.mCameraShakeStartTime = 0f;
				this.mCanBeUnequipped = true;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			InputManager.ActiveDevice.StopVibration();
			Main.EventSystem.Post(ScalpelStateChangedEvent.GetEvent(newState));
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x00085188 File Offset: 0x00083388
		private bool IsValidTargetable(StructurePart part)
		{
			this.mSetPowerRating = GlobalOptions.Raw.GetFloat("General.StingerCutGrade", 1f);
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				this.mSetPowerRating = (float)this.mData.PowerRating;
			}
			return part != null && part.CuttingTargetable != null && part.CuttingTargetable.IsScalpelCuttable() && this.mSetPowerRating >= (float)part.CuttingTargetable.PowerRating && EntityBlueprintComponent.IsValid(part.EntityBlueprintComponent) && (part.Group == null || EntityBlueprintComponent.IsValid(part.Group.EntityBlueprintComponent));
		}

		// Token: 0x06001F19 RID: 7961 RVA: 0x000851FC File Offset: 0x000833FC
		private int GetTargetTypeRTPCValue(ScalpelTarget target)
		{
			int result;
			if (target.IsValid)
			{
				result = 1;
			}
			else if (target.Part != null)
			{
				result = 2;
			}
			else
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x06001F1A RID: 7962 RVA: 0x0008522A File Offset: 0x0008342A
		private void UpdateTargetTypeRTPC(int value)
		{
			Main.EventSystem.Post(SetRTPCEvent.GetMasterEvent(this.mData.ScalpelTargetTypeAudioParameter, (float)value));
		}

		// Token: 0x04001665 RID: 5733
		[SerializeField]
		private ScalpelAsset m_ScalpelAsset;

		// Token: 0x04001666 RID: 5734
		private IScalpelData mData;

		// Token: 0x04001667 RID: 5735
		private bool mCanBeUnequipped = true;

		// Token: 0x04001668 RID: 5736
		private float mCameraShakeStartTime;

		// Token: 0x04001669 RID: 5737
		private CuttingToolController mCuttingToolController;

		// Token: 0x0400166A RID: 5738
		private const int kTargetTypeNothing = 0;

		// Token: 0x0400166B RID: 5739
		private const int kTargetTypeValid = 1;

		// Token: 0x0400166C RID: 5740
		private const int kTargetTypeInvalid = 2;

		// Token: 0x0400234F RID: 9039
		private float m_MeltTime = GlobalOptions.Raw.GetFloat("General.StingerMeltTime", -1f);

		// Token: 0x04002350 RID: 9040
		private float mSetPowerRating;
	}
}
