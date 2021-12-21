using System;
using System.Collections.Generic;
using BBI.Core.Utility;
using Carbon.Core;
using InControl;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000A9F RID: 2719
	public class CuttingController : DebuggableMonoBehaviour
	{
		// Token: 0x17000F4C RID: 3916
		// (get) Token: 0x060038F8 RID: 14584 RVA: 0x000F8764 File Offset: 0x000F6964
		public ICutExecutionData ActiveCutData
		{
			get
			{
				return this.mData.BuffableCutExecution;
			}
		}

		// Token: 0x17000F4D RID: 3917
		// (get) Token: 0x060038F9 RID: 14585 RVA: 0x000F8771 File Offset: 0x000F6971
		public bool AnyValidTargetables
		{
			get
			{
				return this.mTargetBuffer.Count > 0;
			}
		}

		// Token: 0x17000F4E RID: 3918
		// (get) Token: 0x060038FA RID: 14586 RVA: 0x000F8781 File Offset: 0x000F6981
		public bool AnyBelowPowerRating
		{
			get
			{
				return this.mAnyBelowPowerRating;
			}
		}

		// Token: 0x17000F4F RID: 3919
		// (get) Token: 0x060038FB RID: 14587 RVA: 0x000F8789 File Offset: 0x000F6989
		public bool AnythingInRange
		{
			get
			{
				return this.mAnythingInRange;
			}
		}

		// Token: 0x17000F50 RID: 3920
		// (get) Token: 0x060038FC RID: 14588 RVA: 0x000F8791 File Offset: 0x000F6991
		public Vector2 CutRotation
		{
			get
			{
				return this.mCutRotation;
			}
		}

		// Token: 0x17000F51 RID: 3921
		// (get) Token: 0x060038FD RID: 14589 RVA: 0x000F8799 File Offset: 0x000F6999
		// (set) Token: 0x060038FE RID: 14590 RVA: 0x000F87A1 File Offset: 0x000F69A1
		public float LastMinCutLength { get; private set; }

		// Token: 0x17000F52 RID: 3922
		// (get) Token: 0x060038FF RID: 14591 RVA: 0x000F87AA File Offset: 0x000F69AA
		public float CutTravelDuration
		{
			get
			{
				return this.mData.CutTravelDuration;
			}
		}

		// Token: 0x17000F53 RID: 3923
		// (get) Token: 0x06003900 RID: 14592 RVA: 0x000F87B7 File Offset: 0x000F69B7
		public bool IsCutting
		{
			get
			{
				return this.mCuttingToolController.State == CuttingState.Cutting;
			}
		}

		// Token: 0x17000F54 RID: 3924
		// (get) Token: 0x06003901 RID: 14593 RVA: 0x000F87C7 File Offset: 0x000F69C7
		public bool IsDisabled
		{
			get
			{
				return this.mCuttingToolController.State == CuttingState.Disabled;
			}
		}

		// Token: 0x17000F55 RID: 3925
		// (get) Token: 0x06003902 RID: 14594 RVA: 0x000F87D7 File Offset: 0x000F69D7
		// (set) Token: 0x06003903 RID: 14595 RVA: 0x000F87DF File Offset: 0x000F69DF
		public bool CanBeUnequipped { get; set; }

		// Token: 0x17000F56 RID: 3926
		// (get) Token: 0x06003904 RID: 14596 RVA: 0x000F87E8 File Offset: 0x000F69E8
		public LayerMask RaycastLayerMask
		{
			get
			{
				return this.mData.RaycastLayerMask;
			}
		}

		// Token: 0x17000F57 RID: 3927
		// (get) Token: 0x06003905 RID: 14597 RVA: 0x000F87F5 File Offset: 0x000F69F5
		public bool IsCurrentModeReady
		{
			get
			{
				return !this.mCutReloadInfo.CoolingDown;
			}
		}

		// Token: 0x17000F58 RID: 3928
		// (get) Token: 0x06003906 RID: 14598 RVA: 0x000F8805 File Offset: 0x000F6A05
		public bool IsCoolingDown
		{
			get
			{
				return this.mCutReloadInfo.CoolingDown;
			}
		}

		// Token: 0x17000F59 RID: 3929
		// (get) Token: 0x06003907 RID: 14599 RVA: 0x000F8812 File Offset: 0x000F6A12
		public CuttingController.EdgeDetectionPointList EdgeDetectionPoints
		{
			get
			{
				return this.mEdgeDetectionPoints;
			}
		}

		// Token: 0x17000F5A RID: 3930
		// (get) Token: 0x06003908 RID: 14600 RVA: 0x000F881A File Offset: 0x000F6A1A
		public ICutterData BuffableCutterData
		{
			get
			{
				return this.mData;
			}
		}

		// Token: 0x17000F5B RID: 3931
		// (get) Token: 0x06003909 RID: 14601 RVA: 0x000F8824 File Offset: 0x000F6A24
		public float CurrentRemainingCooldownNormalized
		{
			get
			{
				float result = 0f;
				if (this.mCutReloadInfo.CoolingDown)
				{
					result = this.mCutReloadInfo.CoolDownTimer / this.mData.BuffableCutExecution.RateOfFire;
				}
				return result;
			}
		}

		// Token: 0x17000F5C RID: 3932
		// (get) Token: 0x0600390A RID: 14602 RVA: 0x000F8862 File Offset: 0x000F6A62
		public bool IsCurrentEquipment
		{
			get
			{
				return this.mCuttingToolController.EquipementController.CurrentEquipment == EquipmentController.Equipment.CuttingTool && this.mCuttingToolController.CurrentMode == CuttingToolController.CutterMode.Cutter;
			}
		}

		// Token: 0x0600390B RID: 14603 RVA: 0x000F8888 File Offset: 0x000F6A88
		private void Awake()
		{
			this.mCuttingToolController = base.GetComponentInParent<CuttingToolController>();
			if (this.mCuttingToolController == null)
			{
				Debug.LogError("[CuttingTool] Unable to find CuttingToolController. Disabling.");
				base.enabled = false;
				return;
			}
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if (masterScenePlayer != null && masterScenePlayer.CutterAsset != null)
			{
				this.m_CutterAsset = masterScenePlayer.CutterAsset;
			}
			if (this.m_CutterAsset == null)
			{
				Debug.LogError("Cutter Asset is missing.");
			}
			this.mData = new CutterBuffableData(this.m_CutterAsset.Data);
			if (this.mData.BuffableCutExecution == null)
			{
				Debug.LogError("No buffable cut executions found.");
			}
			this.mCutReloadInfo = default(CuttingController.CutReloadInfo);
			this.CanBeUnequipped = true;
			this.mCutRotation = Vector3.right;
			this.mEdgeDetectionPoints.Points = new CuttingController.EdgeDetectionPoint[(int)(this.mData.EdgeDetectionPointDensity * 100f)];
		}

		// Token: 0x0600390C RID: 14604 RVA: 0x000F8974 File Offset: 0x000F6B74
		private void Start()
		{
			Main.EventSystem.Post(RegisterEvent<CuttingController>.Register(this));
		}

		// Token: 0x0600390D RID: 14605 RVA: 0x000F8986 File Offset: 0x000F6B86
		private void OnDestroy()
		{
			Main.EventSystem.Post(RegisterEvent<CuttingController>.Deregister(this));
		}

		// Token: 0x0600390E RID: 14606 RVA: 0x000F8998 File Offset: 0x000F6B98
		public void UpdateCutter()
		{
			this.TryGetTargetables(ref this.mTargetBuffer);
			CuttingState state = this.mCuttingToolController.State;
			if (state != CuttingState.Ready)
			{
				if (state == CuttingState.Disabled)
				{
					this.HandleCutterDisabled();
				}
			}
			else
			{
				this.HandleCuttingReady();
			}
			if (this.IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterAltFire.WasPressed)
			{
				if (this.m_CutterAsset.Data.SwapCutterAngle != null)
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(this.m_CutterAsset.Data.SwapCutterAngle, MathUtility.OperationType.Add, 1));
				}
				this.mHorizontalCutNext = !this.mHorizontalCutNext;
				this.mCutRotation = (this.mHorizontalCutNext ? Vector2.right : Vector2.up);
			}
		}

		// Token: 0x0600390F RID: 14607 RVA: 0x000F8A56 File Offset: 0x000F6C56
		public void UpdateCutterPassive()
		{
			this.HandleCutSpeed();
			this.HandleCoolingDown();
		}

		// Token: 0x06003910 RID: 14608 RVA: 0x000F8A64 File Offset: 0x000F6C64
		public void FixedUpdateCutter()
		{
			CuttingState state = this.mCuttingToolController.State;
			if (state == CuttingState.Cutting)
			{
				this.HandleCutting();
			}
		}

		// Token: 0x06003911 RID: 14609 RVA: 0x000F8A87 File Offset: 0x000F6C87
		public void FixedUpdateCutterPassive()
		{
			this.HandleQueuedCuttables();
		}

		// Token: 0x06003912 RID: 14610 RVA: 0x000F8A90 File Offset: 0x000F6C90
		public void OnStateChanged(CuttingState newState)
		{
			switch (newState)
			{
			case CuttingState.Ready:
				this.mTargetBuffer.Clear();
				this.CanBeUnequipped = true;
				InputManager.ActiveDevice.StopVibration();
				this.mCutRotation = (this.mHorizontalCutNext ? Vector2.right : Vector2.up);
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Ready));
				return;
			case CuttingState.Cutting:
				this.mCuttingToolController.EquipCutter();
				this.CanBeUnequipped = false;
				this.mCutRotation = (this.mHorizontalCutNext ? Vector2.right : Vector2.up);
				this.StartCutting(this.mData.BuffableCutExecution);
				return;
			case CuttingState.Disabled:
				InputManager.ActiveDevice.StopVibration();
				this.CanBeUnequipped = true;
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Disabled));
				return;
			default:
				return;
			}
		}

		// Token: 0x06003913 RID: 14611 RVA: 0x000F8B58 File Offset: 0x000F6D58
		private void HandleQueuedCuttables()
		{
			for (int i = this.mQueuedCuttables.Count - 1; i >= 0; i--)
			{
				CuttingController.QueuedCuttable queuedCuttable = this.mQueuedCuttables[i];
				if (queuedCuttable.Component != null)
				{
					if (queuedCuttable.DelayReached)
					{
						this.mQueuedCuttables.RemoveAt(i);
						this.CutStructurePart(queuedCuttable.Component, queuedCuttable.WorldHitPoint, queuedCuttable.WorldPlaneNormal, queuedCuttable.WorldPlaneTangent, queuedCuttable.SplitForce, queuedCuttable.SplitGap, queuedCuttable.ImpactForce, queuedCuttable.ImpactDirection, queuedCuttable.PowerRating);
					}
				}
				else
				{
					this.mQueuedCuttables.RemoveAt(i);
				}
			}
		}

		// Token: 0x06003914 RID: 14612 RVA: 0x000F8C01 File Offset: 0x000F6E01
		private void HandleCuttingReady()
		{
			if (this.IsCurrentEquipment && this.IsCurrentModeReady && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				this.mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		// Token: 0x06003915 RID: 14613 RVA: 0x000F8C38 File Offset: 0x000F6E38
		private void HandleCutterDisabled()
		{
			if (this.IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				this.mCuttingToolController.EquipCutter();
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mCuttingToolController.Data.CutDeniedAudioEvent));
			}
		}

		// Token: 0x06003916 RID: 14614 RVA: 0x000F8C90 File Offset: 0x000F6E90
		private void HandleCutSpeed()
		{
			if (this.mCutReloadInfo.IsCutting)
			{
				float amount = Mathf.Lerp(this.mData.BuffableCutExecution.ControllerVibrationIntensity.Min, this.mData.BuffableCutExecution.ControllerVibrationIntensity.Max, this.mCuttingToolController.CurrentHeatPercent);
				InputManager.ActiveDevice.Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
				this.mCutReloadInfo.CutSpeedTimer = this.mCutReloadInfo.CutSpeedTimer + Time.deltaTime;
				if (this.mCutReloadInfo.CutSpeedTimer >= this.mData.CutTravelDuration)
				{
					this.mCutReloadInfo.CutSpeedTimer = 0f;
					this.mCutReloadInfo.IsCutting = false;
					InputManager.ActiveDevice.StopVibration();
					if (this.mCuttingToolController.CurrentMode != CuttingToolController.CutterMode.Scalpel)
					{
						this.mCuttingToolController.SetState(CuttingState.Ready);
					}
				}
			}
		}

		// Token: 0x06003917 RID: 14615 RVA: 0x000F8D6C File Offset: 0x000F6F6C
		private void HandleCoolingDown()
		{
			if (this.mCutReloadInfo.CoolingDown)
			{
				this.mCutReloadInfo.CoolDownTimer = this.mCutReloadInfo.CoolDownTimer + Time.deltaTime;
				if (this.mCutReloadInfo.CoolDownTimer >= this.mData.BuffableCutExecution.RateOfFire)
				{
					this.mCutReloadInfo.CoolingDown = false;
					this.mCutReloadInfo.CoolDownTimer = 0f;
				}
			}
		}

		// Token: 0x06003918 RID: 14616 RVA: 0x000F8DD4 File Offset: 0x000F6FD4
		private void StartCutting(ICutExecutionData data)
		{
			if (!GlobalOptions.Raw.GetBool("General.InfHeat", false) || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				this.mCuttingToolController.AddHeat();
			}
			if (!this.mCuttingToolController.IsOverheated)
			{
				this.mCurrentCutTime = 0f;
				this.mMaxCutDelay = 0f;
				for (int i = 0; i < this.mData.BuffableCutExecution.BuffableCutLines.Count; i++)
				{
					this.mMaxCutDelay = Mathf.Max(this.mMaxCutDelay, this.mData.BuffableCutExecution.BuffableCutLines[i].Delay);
				}
				this.mCuttingToolController.DelayCooldown();
			}
		}

		// Token: 0x06003919 RID: 14617 RVA: 0x000F8E68 File Offset: 0x000F7068
		private void ApplyRecoilForce(ForceInfo recoilForce)
		{
			if (this.m_PlayerRigidbody != null)
			{
				Vector3 vector = -this.m_PlayerRigidbody.transform.forward * recoilForce.Force;
				this.m_PlayerRigidbody.AddForce(vector, recoilForce.ForceMode);
			}
		}

		// Token: 0x0600391A RID: 14618 RVA: 0x000F8EB8 File Offset: 0x000F70B8
		private void HandleCutting()
		{
			float num = this.mCurrentCutTime + Time.deltaTime;
			for (int i = 0; i < this.mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = this.mData.BuffableCutExecution.BuffableCutLines[i];
				if (cutLineBuffableData.Delay >= this.mCurrentCutTime && cutLineBuffableData.Delay < num)
				{
					this.mAnySuccessfulCuts |= this.TryPerformCut(this.mData.BuffableCutExecution, cutLineBuffableData);
				}
			}
			this.mCurrentCutTime = num;
			if (this.mCurrentCutTime > this.mMaxCutDelay)
			{
				this.CompleteCutting();
			}
		}

		// Token: 0x0600391B RID: 14619 RVA: 0x000F8F5C File Offset: 0x000F715C
		private void CompleteCutting()
		{
			if (this.mData.AutoAlternate)
			{
				this.mHorizontalCutNext = !this.mHorizontalCutNext;
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.OrientationChangeAudioEvent));
			}
			this.mAnySuccessfulCuts = false;
			this.CanBeUnequipped = true;
		}

		// Token: 0x0600391C RID: 14620 RVA: 0x000F8FB0 File Offset: 0x000F71B0
		private bool TryPerformCut(ICutExecutionData data, ICutLineData cutData)
		{
			if (LynxCameraController.MainCamera == null)
			{
				Debug.LogError("CuttingController failed to get MainCamera");
				return false;
			}
			this.mTargetBuffer.Clear();
			Vector2 vector;
			if (this.TryGetCutLineTargetables(data, cutData, ref this.mTargetBuffer, out vector, out this.mAnyBelowPowerRating))
			{
				Vector3 normalized = Vector3.Cross(LynxCameraController.MainCameraTransform.forward, LynxCameraController.MainCameraTransform.TransformDirection(vector)).normalized;
				float num = 0f;
				ForceInfo splitForce = cutData.SplitForce;
				float splitGap = cutData.SplitGap;
				ForceInfo impactForce = cutData.ImpactForce;
				Vector3 forward = this.m_PlayerRigidbody.transform.forward;
				int num2 = 0;
				if (this.mTargetBuffer.Count > 0)
				{
					for (int i = this.mTargetBuffer.Count - 1; i >= 0; i--)
					{
						CuttingController.CuttableInfo cuttableInfo = this.mTargetBuffer[i];
						float3 @float = math.normalize(math.cross(cuttableInfo.HitPoint.normal, normalized));
						CuttingController.QueuedCuttable item = new CuttingController.QueuedCuttable(cuttableInfo.Component, cuttableInfo.HitPoint.point, normalized, @float, splitForce, splitGap, impactForce, forward, this.mData.BuffableCutExecution.PowerRating, data.SplitDelay);
						this.mQueuedCuttables.Add(item);
						num = Mathf.Max(Vector3.SqrMagnitude(cuttableInfo.HitPoint.point - LynxCameraController.MainCameraTransform.position), num);
						this.mTargetBuffer.RemoveAt(i);
						if (cuttableInfo.Component.CuttingTargetable.PowerRating > num2)
						{
							num2 = cuttableInfo.Component.CuttingTargetable.PowerRating;
						}
					}
				}
				this.mCuttingToolController.UpdateCutGradeRTPC(num2);
				this.LastMinCutLength = Mathf.Sqrt(num);
				this.mCutReloadInfo.Cut();
				this.mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.CutterExecutedCut, null);
				this.mCuttingToolController.TriggerCutterAnimation(this.mData.BuffableCutExecution.FireAnimationTrigger);
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Cutting, cutData));
				this.ApplyRecoilForce(cutData.RecoilForce);
				return true;
			}
			this.LastMinCutLength = data.Range;
			this.mCutReloadInfo.Cut();
			this.mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.CutterExecutedCut, null);
			this.mCuttingToolController.TriggerCutterAnimation(this.mData.BuffableCutExecution.MissAnimationTrigger);
			Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Missed, cutData));
			this.mCuttingToolController.UpdateCutGradeRTPC(0);
			return false;
		}

		// Token: 0x0600391D RID: 14621 RVA: 0x000F9238 File Offset: 0x000F7438
		private void CutStructurePart(StructurePart cuttable, Vector3 cutPoint, Vector3 planeNormal, Vector3 planeTangent, ForceInfo splitForce, float splitGap, ForceInfo impactForce, Vector3 impactDirection, int powerRating)
		{
			if (cuttable == null || cuttable.CuttingTargetable == null)
			{
				return;
			}
			if (cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<FirstGeneration>(cuttable.Entity))
			{
				Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction, MathUtility.OperationType.Add, 1));
			}
			Main.EventSystem.Post(ObjectCutEvent.GetEvent(cuttable));
			Main.EventSystem.Post(ForceReleaseEvent.GetEvent(ForceReleaseEvent.ReleaseState.HandsAndGrapple, cuttable.gameObject));
			this.mCutSplitPlaneBuffer.SetNormalAndPosition(planeNormal, cutPoint);
			this.mSplitPlanesArrayBuffer[0] = this.mCutSplitPlaneBuffer;
			this.mSplitTangentArrayBuffer[0] = planeTangent;
			VaporizationInfo vaporizationInfo = ((this.mData.VaporizationAsset != null) ? this.mData.VaporizationAsset.Data : VaporizationInfo.None);
			cuttable.CuttingTargetable.Split(this.mSplitPlanesArrayBuffer, splitForce, splitGap, cutPoint, planeNormal, planeTangent, impactForce, impactDirection, powerRating, vaporizationInfo, this.mSplitTangentArrayBuffer);
		}

		// Token: 0x0600391E RID: 14622 RVA: 0x000F9350 File Offset: 0x000F7550
		private bool TryGetTargetables(ref List<CuttingController.CuttableInfo> targetablesOnLine)
		{
			targetablesOnLine.Clear();
			bool flag = false;
			for (int i = 0; i < this.mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				Vector2 vector;
				bool flag2;
				flag |= this.TryGetCutLineTargetables(this.mData.BuffableCutExecution, this.mData.BuffableCutExecution.BuffableCutLines[i], ref targetablesOnLine, out vector, out flag2);
			}
			return flag;
		}

		// Token: 0x0600391F RID: 14623 RVA: 0x000F93B8 File Offset: 0x000F75B8
		private bool TryGetCutLineTargetables(ICutExecutionData data, ICutLineData cutData, ref List<CuttingController.CuttableInfo> targetablesOnLine, out Vector2 cutRotation, out bool anyBelowPowerRating)
		{
			anyBelowPowerRating = false;
			if (LynxCameraController.MainCamera == null)
			{
				Debug.LogError("CuttingController failed to get MainCamera");
				cutRotation = Vector2.zero;
				return false;
			}
			Vector2 vector;
			CuttingController.GetRotatedCutLine(cutData, this.mCutRotation, out vector, out cutRotation);
			Vector2 vector2 = LynxCameraController.ScreenCenter + vector * (float)LynxCameraController.ScreenWidth;
			this.mAnythingInRange = false;
			if (cutData.CutAllObjectsOnLine)
			{
				int num = (int)(cutData.CutLineWidth * this.mData.RaycastScreenWidthDensity * 100f);
				float num2 = cutData.CutLineWidth * cutRotation.x;
				float num3 = cutData.CutLineWidth * cutRotation.y;
				int num4 = num / 2;
				for (int i = 0; i < num; i++)
				{
					int rayIndexCenterOffset = Mathf.Abs(num4 - i);
					float num5 = (float)i / (float)(num - 1);
					float num6 = vector2.x + (float)LynxCameraController.ScreenWidth * (-num2 * 0.5f + num2 * num5);
					float num7 = vector2.y + (float)LynxCameraController.ScreenWidth * (-num3 * 0.5f + num3 * num5);
					Ray ray = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num6, num7));
					bool flag;
					this.AddHitValidTargetable(ray, rayIndexCenterOffset, data, ref targetablesOnLine, out flag);
					anyBelowPowerRating = anyBelowPowerRating || flag;
				}
			}
			else
			{
				Ray ray2 = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(vector2.x, vector2.y));
				bool flag2;
				this.AddHitValidTargetable(ray2, 0, data, ref targetablesOnLine, out flag2);
				anyBelowPowerRating = anyBelowPowerRating || flag2;
			}
			bool flag3 = targetablesOnLine.Count > 0;
			if (flag3)
			{
				int num8 = this.mEdgeDetectionPoints.Points.Length;
				float x = cutRotation.x;
				float y = cutRotation.y;
				int num9 = num8 / 2;
				this.mEdgeDetectionPoints.FirstHitIndex = -1;
				this.mEdgeDetectionPoints.LastHitIndex = -1;
				for (int j = 0; j < num8; j++)
				{
					float num10 = (float)j / (float)(num8 - 1);
					float num11 = vector2.x + (float)LynxCameraController.ScreenWidth * (-x * 0.5f + x * num10);
					float num12 = vector2.y + (float)LynxCameraController.ScreenWidth * (-y * 0.5f + y * num10);
					Vector3 vector3;
					vector3..ctor(num11, num12);
					Ray ray3 = LynxCameraController.MainCamera.ScreenPointToRay(vector3);
					this.mEdgeDetectionPoints.Points[j].ScreenPosition = vector3;
					RaycastHit raycastHit;
					if (Physics.Raycast(ray3, ref raycastHit, data.Range * 3f, this.mData.RaycastLayerMask))
					{
						bool flag4 = false;
						for (int k = 0; k < targetablesOnLine.Count; k++)
						{
							StructurePart componentInParent = raycastHit.collider.transform.GetComponentInParent<StructurePart>();
							if (componentInParent != null && componentInParent == targetablesOnLine[k].Component)
							{
								flag4 = true;
								this.mEdgeDetectionPoints.Points[j].State = ((this.mCuttingToolController.State == CuttingState.Disabled) ? CuttingController.EdgeDetectionPoint.PointState.Invalid : CuttingController.EdgeDetectionPoint.PointState.Valid);
								if (this.mEdgeDetectionPoints.FirstHitIndex == -1)
								{
									this.mEdgeDetectionPoints.FirstHitIndex = j;
								}
								this.mEdgeDetectionPoints.LastHitIndex = j;
								break;
							}
						}
						if (!flag4)
						{
							this.mEdgeDetectionPoints.Points[j].State = CuttingController.EdgeDetectionPoint.PointState.Invalid;
						}
					}
					else
					{
						this.mEdgeDetectionPoints.Points[j].State = CuttingController.EdgeDetectionPoint.PointState.None;
					}
				}
			}
			return flag3;
		}

		// Token: 0x06003920 RID: 14624 RVA: 0x000F9718 File Offset: 0x000F7918
		public static void GetRotatedCutLine(ICutLineData cutData, Vector2 rotation, out Vector2 newPosition, out Vector2 newRotation)
		{
			float num = rotation.x;
			float num2 = rotation.y;
			float x = cutData.PositionOffset.x;
			float y = cutData.PositionOffset.y;
			float num3 = x * num - y * num2;
			float num4 = x * num2 + y * num;
			newPosition = new Vector2(num3, num4);
			if (cutData.RotationOffset != 0f)
			{
				float num5 = cutData.RotationOffset * 0.017453292f;
				num = Mathf.Cos(num5);
				num2 = Mathf.Sin(num5);
				num3 = rotation.x * num - rotation.y * num2;
				num4 = rotation.x * num2 + rotation.y * num;
				newRotation = new Vector2(num3, num4);
				return;
			}
			newRotation = rotation;
		}

		// Token: 0x06003921 RID: 14625 RVA: 0x000F97CC File Offset: 0x000F79CC
		private void AddHitValidTargetable(Ray ray, int rayIndexCenterOffset, ICutExecutionData data, ref List<CuttingController.CuttableInfo> targetablesOnLine, out bool anyBelowPowerRating)
		{
			anyBelowPowerRating = false;
			RaycastHit raycastHit;
			if (Physics.Raycast(ray, ref raycastHit, data.Range, this.mData.RaycastLayerMask))
			{
				this.mAnythingInRange = true;
				if (((1 << raycastHit.collider.gameObject.layer) & this.mData.ValidSurfaceLayerMask.value) <= 0)
				{
					return;
				}
				for (int i = 0; i < data.ExcludeTags.Length; i++)
				{
					if (raycastHit.collider.CompareTag(data.ExcludeTags[i]))
					{
						return;
					}
				}
				StructurePart structurePart;
				if (CuttingController.TryGetValidTargetable(raycastHit, this.mData.BuffableCutExecution.PowerRating, out structurePart, out anyBelowPowerRating, false))
				{
					bool flag = true;
					int j = 0;
					while (j < targetablesOnLine.Count)
					{
						CuttingController.CuttableInfo cuttableInfo = targetablesOnLine[j];
						if (cuttableInfo.Component == structurePart)
						{
							flag = false;
							if (rayIndexCenterOffset < cuttableInfo.RayIndexCenterOffset)
							{
								cuttableInfo.RayIndexCenterOffset = rayIndexCenterOffset;
								cuttableInfo.HitPoint = raycastHit;
								targetablesOnLine[j] = cuttableInfo;
								break;
							}
							break;
						}
						else
						{
							j++;
						}
					}
					if (flag)
					{
						targetablesOnLine.Add(new CuttingController.CuttableInfo(structurePart, raycastHit, rayIndexCenterOffset));
					}
				}
			}
		}

		// Token: 0x06003922 RID: 14626 RVA: 0x000F98F0 File Offset: 0x000F7AF0
		public static bool TryGetValidTargetable(RaycastHit hit, int powerRating, out StructurePart part, out bool anyBelowPowerRating, bool uiInfo = false)
		{
			part = hit.collider.transform.GetComponentInParent<StructurePart>();
			anyBelowPowerRating = false;
			if (part != null && part.CuttingTargetable != null)
			{
				bool flag = part.CuttingTargetable.IsAimCuttable();
				bool flag2 = part.CuttingTargetable.IsQuickCuttable();
				bool flag3 = part.CuttingTargetable.IsBelowPowerRating(powerRating);
				anyBelowPowerRating = anyBelowPowerRating || flag3;
				if (uiInfo || ((flag || flag2) && !flag3))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003923 RID: 14627 RVA: 0x000F9964 File Offset: 0x000F7B64
		public bool TryGetTargetableInRange(out StructurePart targetable, out bool anyBelowPowerRating, bool uiInfo = false)
		{
			anyBelowPowerRating = false;
			RaycastHit hit;
			if (LynxCameraController.MainCameraTransform != null && Physics.Raycast(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward, ref hit, this.ActiveCutData.Range, this.mData.RaycastLayerMask) && CuttingController.TryGetValidTargetable(hit, this.mData.BuffableCutExecution.PowerRating, out targetable, out anyBelowPowerRating, uiInfo))
			{
				return true;
			}
			targetable = null;
			return false;
		}

		// Token: 0x06003924 RID: 14628 RVA: 0x000F99DC File Offset: 0x000F7BDC
		public override void DrawDebugGizmos()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (LynxCameraController.MainCamera == null)
			{
				return;
			}
			for (int i = 0; i < this.mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = this.mData.BuffableCutExecution.BuffableCutLines[i];
				Vector2 vector;
				Vector2 vector2;
				CuttingController.GetRotatedCutLine(cutLineBuffableData, this.mCutRotation, out vector, out vector2);
				Vector2 vector3 = LynxCameraController.ScreenCenter + vector * (float)LynxCameraController.ScreenWidth;
				Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(vector3.x, vector3.y)));
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * this.mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * vector2.x;
					float num3 = cutLineBuffableData.CutLineWidth * vector2.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float num5 = vector3.x + (float)LynxCameraController.ScreenWidth * (-num2 * 0.5f + num2 * num4);
						float num6 = vector3.y + (float)LynxCameraController.ScreenWidth * (-num3 * 0.5f + num3 * num4);
						Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num5, num6)));
					}
				}
			}
		}

		// Token: 0x06003925 RID: 14629 RVA: 0x000F9B40 File Offset: 0x000F7D40
		public override void DrawDebugInfo()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (LynxCameraController.MainCamera == null)
			{
				return;
			}
			for (int i = 0; i < this.mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = this.mData.BuffableCutExecution.BuffableCutLines[i];
				Vector2 vector;
				Vector2 vector2;
				CuttingController.GetRotatedCutLine(cutLineBuffableData, this.mCutRotation, out vector, out vector2);
				Vector2 vector3 = LynxCameraController.ScreenCenter + vector * (float)LynxCameraController.ScreenWidth;
				GUI.Box(new Rect(vector3.x - 1f, vector3.y - 1f, 1f, 1f), Texture2D.whiteTexture);
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * this.mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * vector2.x;
					float num3 = cutLineBuffableData.CutLineWidth * vector2.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float num5 = vector3.x + (float)LynxCameraController.ScreenWidth * (-num2 * 0.5f + num2 * num4);
						float num6 = vector3.y - (float)LynxCameraController.ScreenWidth * (-num3 * 0.5f + num3 * num4);
						GUI.Box(new Rect(num5 - 1f, num6 - 1f, 1f, 1f), Texture2D.whiteTexture);
					}
				}
			}
		}

		// Token: 0x04002D20 RID: 11552
		private const float kEdgeDetectionRangeMod = 3f;

		// Token: 0x04002D21 RID: 11553
		private bool mAnyBelowPowerRating;

		// Token: 0x04002D22 RID: 11554
		private bool mAnythingInRange;

		// Token: 0x04002D25 RID: 11557
		[Header("Cutter Asset")]
		[SerializeField]
		private CutterAsset m_CutterAsset;

		// Token: 0x04002D26 RID: 11558
		[Header("Player Hookups")]
		[SerializeField]
		private Rigidbody m_PlayerRigidbody;

		// Token: 0x04002D27 RID: 11559
		private ICutterData mData;

		// Token: 0x04002D28 RID: 11560
		private Vector2 mCutRotation = Vector2.right;

		// Token: 0x04002D29 RID: 11561
		private List<CuttingController.CuttableInfo> mTargetBuffer = new List<CuttingController.CuttableInfo>();

		// Token: 0x04002D2A RID: 11562
		private List<CuttingController.QueuedCuttable> mQueuedCuttables = new List<CuttingController.QueuedCuttable>();

		// Token: 0x04002D2B RID: 11563
		private CuttingController.EdgeDetectionPointList mEdgeDetectionPoints;

		// Token: 0x04002D2C RID: 11564
		private CuttingController.CutReloadInfo mCutReloadInfo;

		// Token: 0x04002D2D RID: 11565
		private bool mHorizontalCutNext = true;

		// Token: 0x04002D2E RID: 11566
		private Plane mCutSplitPlaneBuffer;

		// Token: 0x04002D2F RID: 11567
		private Plane[] mSplitPlanesArrayBuffer = new Plane[1];

		// Token: 0x04002D30 RID: 11568
		private Vector3[] mSplitTangentArrayBuffer = new Vector3[1];

		// Token: 0x04002D31 RID: 11569
		private float mCurrentCutTime;

		// Token: 0x04002D32 RID: 11570
		private float mMaxCutDelay;

		// Token: 0x04002D33 RID: 11571
		private bool mAnySuccessfulCuts;

		// Token: 0x04002D34 RID: 11572
		private CuttingToolController mCuttingToolController;

		// Token: 0x02000ECB RID: 3787
		public struct CuttableInfo
		{
			// Token: 0x06004A14 RID: 18964 RVA: 0x0014655A File Offset: 0x0014475A
			public CuttableInfo(StructurePart component, RaycastHit hitPoint, int rayIndexCenterOffset)
			{
				this.Component = component;
				this.HitPoint = hitPoint;
				this.RayIndexCenterOffset = rayIndexCenterOffset;
			}

			// Token: 0x040046A1 RID: 18081
			public StructurePart Component;

			// Token: 0x040046A2 RID: 18082
			public RaycastHit HitPoint;

			// Token: 0x040046A3 RID: 18083
			public int RayIndexCenterOffset;
		}

		// Token: 0x02000ECC RID: 3788
		private struct QueuedCuttable
		{
			// Token: 0x06004A15 RID: 18965 RVA: 0x00146574 File Offset: 0x00144774
			public QueuedCuttable(StructurePart component, Vector3 worldHitPoint, Vector3 worldNormal, Vector3 worldTangent, ForceInfo splitForce, float splitGap, ForceInfo impactForce, Vector3 impactDirection, int powerRating, float queueDelay)
			{
				this.Component = component;
				this.mLocalHitPoint = this.Component.transform.InverseTransformPoint(worldHitPoint);
				this.mLocalPlaneNormal = this.Component.transform.InverseTransformDirection(worldNormal);
				this.mLocalPlaneTangent = this.Component.transform.InverseTransformDirection(worldTangent);
				this.SplitForce = splitForce;
				this.SplitGap = splitGap;
				this.ImpactForce = impactForce;
				this.PowerRating = powerRating;
				this.mLocalImpactDirection = this.Component.transform.InverseTransformDirection(impactDirection);
				this.mQueueDelay = queueDelay;
				this.mStartTime = Time.time;
			}

			// Token: 0x17001494 RID: 5268
			// (get) Token: 0x06004A16 RID: 18966 RVA: 0x00146619 File Offset: 0x00144819
			public Vector3 WorldHitPoint
			{
				get
				{
					return this.Component.transform.TransformPoint(this.mLocalHitPoint);
				}
			}

			// Token: 0x17001495 RID: 5269
			// (get) Token: 0x06004A17 RID: 18967 RVA: 0x00146631 File Offset: 0x00144831
			public Vector3 WorldPlaneNormal
			{
				get
				{
					return this.Component.transform.TransformDirection(this.mLocalPlaneNormal);
				}
			}

			// Token: 0x17001496 RID: 5270
			// (get) Token: 0x06004A18 RID: 18968 RVA: 0x00146649 File Offset: 0x00144849
			public Vector3 WorldPlaneTangent
			{
				get
				{
					return this.Component.transform.TransformDirection(this.mLocalPlaneTangent);
				}
			}

			// Token: 0x17001497 RID: 5271
			// (get) Token: 0x06004A19 RID: 18969 RVA: 0x00146661 File Offset: 0x00144861
			public Vector3 ImpactDirection
			{
				get
				{
					return this.Component.transform.TransformDirection(this.mLocalImpactDirection);
				}
			}

			// Token: 0x17001498 RID: 5272
			// (get) Token: 0x06004A1A RID: 18970 RVA: 0x00146679 File Offset: 0x00144879
			public bool DelayReached
			{
				get
				{
					return Time.time - this.mStartTime >= this.mQueueDelay;
				}
			}

			// Token: 0x040046A4 RID: 18084
			public StructurePart Component;

			// Token: 0x040046A5 RID: 18085
			public ForceInfo SplitForce;

			// Token: 0x040046A6 RID: 18086
			public float SplitGap;

			// Token: 0x040046A7 RID: 18087
			public ForceInfo ImpactForce;

			// Token: 0x040046A8 RID: 18088
			public int PowerRating;

			// Token: 0x040046A9 RID: 18089
			private float mStartTime;

			// Token: 0x040046AA RID: 18090
			private float mQueueDelay;

			// Token: 0x040046AB RID: 18091
			private Vector3 mLocalHitPoint;

			// Token: 0x040046AC RID: 18092
			private Vector3 mLocalPlaneNormal;

			// Token: 0x040046AD RID: 18093
			private Vector3 mLocalPlaneTangent;

			// Token: 0x040046AE RID: 18094
			private Vector3 mLocalImpactDirection;
		}

		// Token: 0x02000ECD RID: 3789
		public struct CutReloadInfo
		{
			// Token: 0x06004A1B RID: 18971 RVA: 0x00146692 File Offset: 0x00144892
			public void Cut()
			{
				this.CutSpeedTimer = 0f;
				this.IsCutting = true;
				this.CoolingDown = true;
				this.CoolDownTimer = 0f;
			}

			// Token: 0x040046AF RID: 18095
			public float CoolDownTimer;

			// Token: 0x040046B0 RID: 18096
			public bool CoolingDown;

			// Token: 0x040046B1 RID: 18097
			public float CutSpeedTimer;

			// Token: 0x040046B2 RID: 18098
			public bool IsCutting;
		}

		// Token: 0x02000ECE RID: 3790
		public struct EdgeDetectionPoint
		{
			// Token: 0x040046B3 RID: 18099
			public Vector3 ScreenPosition;

			// Token: 0x040046B4 RID: 18100
			public CuttingController.EdgeDetectionPoint.PointState State;

			// Token: 0x02000F60 RID: 3936
			public enum PointState
			{
				// Token: 0x04004A24 RID: 18980
				None,
				// Token: 0x04004A25 RID: 18981
				Valid,
				// Token: 0x04004A26 RID: 18982
				Invalid
			}
		}

		// Token: 0x02000ECF RID: 3791
		public struct EdgeDetectionPointList
		{
			// Token: 0x17001499 RID: 5273
			// (get) Token: 0x06004A1C RID: 18972 RVA: 0x001466B8 File Offset: 0x001448B8
			public Vector3 FirstHitScreenPoint
			{
				get
				{
					return this.Points[this.FirstHitIndex].ScreenPosition;
				}
			}

			// Token: 0x1700149A RID: 5274
			// (get) Token: 0x06004A1D RID: 18973 RVA: 0x001466D0 File Offset: 0x001448D0
			public Vector3 LastHitScreenPoint
			{
				get
				{
					return this.Points[this.LastHitIndex].ScreenPosition;
				}
			}

			// Token: 0x1700149B RID: 5275
			// (get) Token: 0x06004A1E RID: 18974 RVA: 0x001466E8 File Offset: 0x001448E8
			public bool FirstIndexInRange
			{
				get
				{
					return this.FirstHitIndex >= 0 && this.FirstHitIndex < this.Points.Length;
				}
			}

			// Token: 0x1700149C RID: 5276
			// (get) Token: 0x06004A1F RID: 18975 RVA: 0x00146705 File Offset: 0x00144905
			public bool LastIndexInRange
			{
				get
				{
					return this.LastHitIndex >= 0 && this.LastHitIndex < this.Points.Length;
				}
			}

			// Token: 0x040046B5 RID: 18101
			public CuttingController.EdgeDetectionPoint[] Points;

			// Token: 0x040046B6 RID: 18102
			public int FirstHitIndex;

			// Token: 0x040046B7 RID: 18103
			public int LastHitIndex;
		}
	}
}
