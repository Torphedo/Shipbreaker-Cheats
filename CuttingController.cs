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
	public class CuttingController : DebuggableMonoBehaviour
	{
		public struct CuttableInfo
		{
			public StructurePart Component;

			public RaycastHit HitPoint;

			public int RayIndexCenterOffset;

			public CuttableInfo(StructurePart component, RaycastHit hitPoint, int rayIndexCenterOffset)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				Component = component;
				HitPoint = hitPoint;
				RayIndexCenterOffset = rayIndexCenterOffset;
			}
		}

		private struct QueuedCuttable
		{
			public StructurePart Component;

			public ForceInfo SplitForce;

			public float SplitGap;

			public ForceInfo ImpactForce;

			public int PowerRating;

			private float mStartTime;

			private float mQueueDelay;

			private Vector3 mLocalHitPoint;

			private Vector3 mLocalPlaneNormal;

			private Vector3 mLocalPlaneTangent;

			private Vector3 mLocalImpactDirection;

			public Vector3 WorldHitPoint => ((Component)Component).get_transform().TransformPoint(mLocalHitPoint);

			public Vector3 WorldPlaneNormal => ((Component)Component).get_transform().TransformDirection(mLocalPlaneNormal);

			public Vector3 WorldPlaneTangent => ((Component)Component).get_transform().TransformDirection(mLocalPlaneTangent);

			public Vector3 ImpactDirection => ((Component)Component).get_transform().TransformDirection(mLocalImpactDirection);

			public bool DelayReached => Time.get_time() - mStartTime >= mQueueDelay;

			public QueuedCuttable(StructurePart component, Vector3 worldHitPoint, Vector3 worldNormal, Vector3 worldTangent, ForceInfo splitForce, float splitGap, ForceInfo impactForce, Vector3 impactDirection, int powerRating, float queueDelay)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				//IL_002a: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_0079: Unknown result type (might be due to invalid IL or missing references)
				//IL_007b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0080: Unknown result type (might be due to invalid IL or missing references)
				Component = component;
				mLocalHitPoint = ((Component)Component).get_transform().InverseTransformPoint(worldHitPoint);
				mLocalPlaneNormal = ((Component)Component).get_transform().InverseTransformDirection(worldNormal);
				mLocalPlaneTangent = ((Component)Component).get_transform().InverseTransformDirection(worldTangent);
				SplitForce = splitForce;
				SplitGap = splitGap;
				ImpactForce = impactForce;
				PowerRating = powerRating;
				mLocalImpactDirection = ((Component)Component).get_transform().InverseTransformDirection(impactDirection);
				mQueueDelay = queueDelay;
				mStartTime = Time.get_time();
			}
		}

		public struct CutReloadInfo
		{
			public float CoolDownTimer;

			public bool CoolingDown;

			public float CutSpeedTimer;

			public bool IsCutting;

			public void Cut()
			{
				CutSpeedTimer = 0f;
				IsCutting = true;
				CoolingDown = true;
				CoolDownTimer = 0f;
			}
		}

		public struct EdgeDetectionPoint
		{
			public enum PointState
			{
				None,
				Valid,
				Invalid
			}

			public Vector3 ScreenPosition;

			public PointState State;
		}

		public struct EdgeDetectionPointList
		{
			public EdgeDetectionPoint[] Points;

			public int FirstHitIndex;

			public int LastHitIndex;

			public Vector3 FirstHitScreenPoint => Points[FirstHitIndex].ScreenPosition;

			public Vector3 LastHitScreenPoint => Points[LastHitIndex].ScreenPosition;

			public bool FirstIndexInRange
			{
				get
				{
					if (FirstHitIndex >= 0)
					{
						return FirstHitIndex < Points.Length;
					}
					return false;
				}
			}

			public bool LastIndexInRange
			{
				get
				{
					if (LastHitIndex >= 0)
					{
						return LastHitIndex < Points.Length;
					}
					return false;
				}
			}
		}

		private const float kEdgeDetectionRangeMod = 3f;

		private bool mAnyBelowPowerRating;

		private bool mAnythingInRange;

		[Header("Cutter Asset")]
		[SerializeField]
		private CutterAsset m_CutterAsset;

		[Header("Player Hookups")]
		[SerializeField]
		private Rigidbody m_PlayerRigidbody;

		private ICutterData mData;

		private Vector2 mCutRotation = Vector2.get_right();

		private List<CuttableInfo> mTargetBuffer = new List<CuttableInfo>();

		private List<QueuedCuttable> mQueuedCuttables = new List<QueuedCuttable>();

		private EdgeDetectionPointList mEdgeDetectionPoints;

		private CutReloadInfo mCutReloadInfo;

		private bool mHorizontalCutNext = true;

		private Plane mCutSplitPlaneBuffer;

		private Plane[] mSplitPlanesArrayBuffer = (Plane[])(object)new Plane[1];

		private Vector3[] mSplitTangentArrayBuffer = (Vector3[])(object)new Vector3[1];

		private float mCurrentCutTime;

		private float mMaxCutDelay;

		private bool mAnySuccessfulCuts;

		private CuttingToolController mCuttingToolController;

		public ICutExecutionData ActiveCutData => mData.BuffableCutExecution;

		public bool AnyValidTargetables => mTargetBuffer.Count > 0;

		public bool AnyBelowPowerRating => mAnyBelowPowerRating;

		public bool AnythingInRange => mAnythingInRange;

		public Vector2 CutRotation => mCutRotation;

		public float LastMinCutLength { get; private set; }

		public float CutTravelDuration => mData.CutTravelDuration;

		public bool IsCutting => mCuttingToolController.State == CuttingState.Cutting;

		public bool IsDisabled => mCuttingToolController.State == CuttingState.Disabled;

		public bool CanBeUnequipped { get; set; }

		public LayerMask RaycastLayerMask => mData.RaycastLayerMask;

		public bool IsCurrentModeReady => !mCutReloadInfo.CoolingDown;

		public bool IsCoolingDown => mCutReloadInfo.CoolingDown;

		public EdgeDetectionPointList EdgeDetectionPoints => mEdgeDetectionPoints;

		public ICutterData BuffableCutterData => mData;

		public float CurrentRemainingCooldownNormalized
		{
			get
			{
				float result = 0f;
				if (mCutReloadInfo.CoolingDown)
				{
					result = mCutReloadInfo.CoolDownTimer / mData.BuffableCutExecution.RateOfFire;
				}
				return result;
			}
		}

		public bool IsCurrentEquipment
		{
			get
			{
				if (mCuttingToolController.EquipementController.CurrentEquipment == EquipmentController.Equipment.CuttingTool)
				{
					return mCuttingToolController.CurrentMode == CuttingToolController.CutterMode.Cutter;
				}
				return false;
			}
		}

		private void Awake()
		{
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			mCuttingToolController = ((Component)this).GetComponentInParent<CuttingToolController>();
			if ((Object)(object)mCuttingToolController == (Object)null)
			{
				Debug.LogError((object)"[CuttingTool] Unable to find CuttingToolController. Disabling.");
				((Behaviour)this).set_enabled(false);
				return;
			}
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if ((Object)(object)masterScenePlayer != (Object)null && (Object)(object)masterScenePlayer.CutterAsset != (Object)null)
			{
				m_CutterAsset = masterScenePlayer.CutterAsset;
			}
			if ((Object)(object)m_CutterAsset == (Object)null)
			{
				Debug.LogError((object)"Cutter Asset is missing.");
			}
			mData = new CutterBuffableData(m_CutterAsset.Data);
			if (mData.BuffableCutExecution == null)
			{
				Debug.LogError((object)"No buffable cut executions found.");
			}
			mCutReloadInfo = default(CutReloadInfo);
			CanBeUnequipped = true;
			mCutRotation = Vector2.op_Implicit(Vector3.get_right());
			mEdgeDetectionPoints.Points = new EdgeDetectionPoint[(int)(mData.EdgeDetectionPointDensity * 100f)];
		}

		private void Start()
		{
			Main.EventSystem.Post(RegisterEvent<CuttingController>.Register(this));
		}

		private void OnDestroy()
		{
			Main.EventSystem.Post(RegisterEvent<CuttingController>.Deregister(this));
		}

		public void UpdateCutter()
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			TryGetTargetables(ref mTargetBuffer);
			switch (mCuttingToolController.State)
			{
			case CuttingState.Disabled:
				HandleCutterDisabled();
				break;
			case CuttingState.Ready:
				HandleCuttingReady();
				break;
			}
			if (IsCurrentEquipment && ((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterAltFire).get_WasPressed())
			{
				if ((Object)(object)m_CutterAsset.Data.SwapCutterAngle != (Object)null)
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(m_CutterAsset.Data.SwapCutterAngle));
				}
				mHorizontalCutNext = !mHorizontalCutNext;
				mCutRotation = (mHorizontalCutNext ? Vector2.get_right() : Vector2.get_up());
			}
		}

		public void UpdateCutterPassive()
		{
			HandleCutSpeed();
			HandleCoolingDown();
		}

		public void FixedUpdateCutter()
		{
			if (mCuttingToolController.State == CuttingState.Cutting)
			{
				HandleCutting();
			}
		}

		public void FixedUpdateCutterPassive()
		{
			HandleQueuedCuttables();
		}

		public void OnStateChanged(CuttingState newState)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			switch (newState)
			{
			case CuttingState.Ready:
				mTargetBuffer.Clear();
				CanBeUnequipped = true;
				InputManager.get_ActiveDevice().StopVibration();
				mCutRotation = (mHorizontalCutNext ? Vector2.get_right() : Vector2.get_up());
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Ready));
				break;
			case CuttingState.Cutting:
				mCuttingToolController.EquipCutter();
				CanBeUnequipped = false;
				mCutRotation = (mHorizontalCutNext ? Vector2.get_right() : Vector2.get_up());
				StartCutting(mData.BuffableCutExecution);
				break;
			case CuttingState.Disabled:
				InputManager.get_ActiveDevice().StopVibration();
				CanBeUnequipped = true;
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Disabled));
				break;
			}
		}

		private void HandleQueuedCuttables()
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			for (int num = mQueuedCuttables.Count - 1; num >= 0; num--)
			{
				QueuedCuttable queuedCuttable = mQueuedCuttables[num];
				if ((Object)(object)queuedCuttable.Component != (Object)null)
				{
					if (queuedCuttable.DelayReached)
					{
						mQueuedCuttables.RemoveAt(num);
						CutStructurePart(queuedCuttable.Component, queuedCuttable.WorldHitPoint, queuedCuttable.WorldPlaneNormal, queuedCuttable.WorldPlaneTangent, queuedCuttable.SplitForce, queuedCuttable.SplitGap, queuedCuttable.ImpactForce, queuedCuttable.ImpactDirection, queuedCuttable.PowerRating);
					}
				}
				else
				{
					mQueuedCuttables.RemoveAt(num);
				}
			}
		}

		private void HandleCuttingReady()
		{
			if (IsCurrentEquipment && IsCurrentModeReady && ((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterFire).get_WasPressed())
			{
				mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		private void HandleCutterDisabled()
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if (IsCurrentEquipment && ((OneAxisInputControl)LynxControls.Instance.GameplayActions.CutterFire).get_WasPressed())
			{
				mCuttingToolController.EquipCutter();
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(mCuttingToolController.Data.CutDeniedAudioEvent));
			}
		}

		private void HandleCutSpeed()
		{
			if (!mCutReloadInfo.IsCutting)
			{
				return;
			}
			float amount = Mathf.Lerp(mData.BuffableCutExecution.ControllerVibrationIntensity.Min, mData.BuffableCutExecution.ControllerVibrationIntensity.Max, mCuttingToolController.CurrentHeatPercent);
			InputManager.get_ActiveDevice().Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
			mCutReloadInfo.CutSpeedTimer += Time.get_deltaTime();
			if (mCutReloadInfo.CutSpeedTimer >= mData.CutTravelDuration)
			{
				mCutReloadInfo.CutSpeedTimer = 0f;
				mCutReloadInfo.IsCutting = false;
				InputManager.get_ActiveDevice().StopVibration();
				if (mCuttingToolController.CurrentMode != CuttingToolController.CutterMode.Scalpel)
				{
					mCuttingToolController.SetState(CuttingState.Ready);
				}
			}
		}

		private void HandleCoolingDown()
		{
			if (mCutReloadInfo.CoolingDown)
			{
				mCutReloadInfo.CoolDownTimer += Time.get_deltaTime();
				if (mCutReloadInfo.CoolDownTimer >= mData.BuffableCutExecution.RateOfFire)
				{
					mCutReloadInfo.CoolingDown = false;
					mCutReloadInfo.CoolDownTimer = 0f;
				}
			}
		}

		private void StartCutting(ICutExecutionData data)
		{
			if (!GlobalOptions.Raw.GetBool("General.InfHeat") || SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
			{
				mCuttingToolController.AddHeat();
			}
			if (!mCuttingToolController.IsOverheated)
			{
				mCurrentCutTime = 0f;
				mMaxCutDelay = 0f;
				for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
				{
					mMaxCutDelay = Mathf.Max(mMaxCutDelay, mData.BuffableCutExecution.BuffableCutLines[i].Delay);
				}
				mCuttingToolController.DelayCooldown();
			}
		}

		private void ApplyRecoilForce(ForceInfo recoilForce)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)m_PlayerRigidbody != (Object)null)
			{
				Vector3 val = -((Component)m_PlayerRigidbody).get_transform().get_forward() * recoilForce.Force;
				m_PlayerRigidbody.AddForce(val, recoilForce.ForceMode);
			}
		}

		private void HandleCutting()
		{
			float num = mCurrentCutTime + Time.get_deltaTime();
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = mData.BuffableCutExecution.BuffableCutLines[i];
				if (cutLineBuffableData.Delay >= mCurrentCutTime && cutLineBuffableData.Delay < num)
				{
					mAnySuccessfulCuts |= TryPerformCut(mData.BuffableCutExecution, cutLineBuffableData);
				}
			}
			mCurrentCutTime = num;
			if (mCurrentCutTime > mMaxCutDelay)
			{
				CompleteCutting();
			}
		}

		private void CompleteCutting()
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if (mData.AutoAlternate)
			{
				mHorizontalCutNext = !mHorizontalCutNext;
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.OrientationChangeAudioEvent));
			}
			mAnySuccessfulCuts = false;
			CanBeUnequipped = true;
		}

		private bool TryPerformCut(ICutExecutionData data, ICutLineData cutData)
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)LynxCameraController.MainCamera == (Object)null)
			{
				Debug.LogError((object)"CuttingController failed to get MainCamera");
				return false;
			}
			mTargetBuffer.Clear();
			if (TryGetCutLineTargetables(data, cutData, ref mTargetBuffer, out var cutRotation, out mAnyBelowPowerRating))
			{
				Vector3 val = Vector3.Cross(LynxCameraController.MainCameraTransform.get_forward(), LynxCameraController.MainCameraTransform.TransformDirection(Vector2.op_Implicit(cutRotation)));
				Vector3 normalized = ((Vector3)(ref val)).get_normalized();
				float num = 0f;
				ForceInfo splitForce = cutData.SplitForce;
				float splitGap = cutData.SplitGap;
				ForceInfo impactForce = cutData.ImpactForce;
				Vector3 forward = ((Component)m_PlayerRigidbody).get_transform().get_forward();
				int num2 = 0;
				if (mTargetBuffer.Count > 0)
				{
					for (int num3 = mTargetBuffer.Count - 1; num3 >= 0; num3--)
					{
						CuttableInfo cuttableInfo = mTargetBuffer[num3];
						float3 val2 = math.normalize(math.cross(float3.op_Implicit(((RaycastHit)(ref cuttableInfo.HitPoint)).get_normal()), float3.op_Implicit(normalized)));
						QueuedCuttable item = new QueuedCuttable(cuttableInfo.Component, ((RaycastHit)(ref cuttableInfo.HitPoint)).get_point(), normalized, float3.op_Implicit(val2), splitForce, splitGap, impactForce, forward, mData.BuffableCutExecution.PowerRating, data.SplitDelay);
						mQueuedCuttables.Add(item);
						num = Mathf.Max(Vector3.SqrMagnitude(((RaycastHit)(ref cuttableInfo.HitPoint)).get_point() - LynxCameraController.MainCameraTransform.get_position()), num);
						mTargetBuffer.RemoveAt(num3);
						if (cuttableInfo.Component.CuttingTargetable.PowerRating > num2)
						{
							num2 = cuttableInfo.Component.CuttingTargetable.PowerRating;
						}
					}
				}
				mCuttingToolController.UpdateCutGradeRTPC(num2);
				LastMinCutLength = Mathf.Sqrt(num);
				mCutReloadInfo.Cut();
				mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.CutterExecutedCut);
				mCuttingToolController.TriggerCutterAnimation(mData.BuffableCutExecution.FireAnimationTrigger);
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Cutting, cutData));
				ApplyRecoilForce(cutData.RecoilForce);
				return true;
			}
			LastMinCutLength = data.Range;
			mCutReloadInfo.Cut();
			mCuttingToolController.DurabilityHandler.HandleDurabilityDamageOfType(DurabilityDamageDef.DurabilityDamageType.CutterExecutedCut);
			mCuttingToolController.TriggerCutterAnimation(mData.BuffableCutExecution.MissAnimationTrigger);
			Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Missed, cutData));
			mCuttingToolController.UpdateCutGradeRTPC(0);
			return false;
		}

		private void CutStructurePart(StructurePart cuttable, Vector3 cutPoint, Vector3 planeNormal, Vector3 planeTangent, ForceInfo splitForce, float splitGap, ForceInfo impactForce, Vector3 impactDirection, int powerRating)
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)cuttable == (Object)null || cuttable.CuttingTargetable == null)
			{
				return;
			}
			if ((Object)(object)cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction != (Object)null)
			{
				EntityManager entityManager = World.get_DefaultGameObjectInjectionWorld().get_EntityManager();
				if (((EntityManager)(ref entityManager)).HasComponent<FirstGeneration>(cuttable.Entity))
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction));
				}
			}
			Main.EventSystem.Post(ObjectCutEvent.GetEvent(cuttable));
			Main.EventSystem.Post(ForceReleaseEvent.GetEvent(ForceReleaseEvent.ReleaseState.HandsAndGrapple, ((Component)cuttable).get_gameObject()));
			((Plane)(ref mCutSplitPlaneBuffer)).SetNormalAndPosition(planeNormal, cutPoint);
			mSplitPlanesArrayBuffer[0] = mCutSplitPlaneBuffer;
			mSplitTangentArrayBuffer[0] = planeTangent;
			VaporizationInfo vaporizationInfo = (((Object)(object)mData.VaporizationAsset != (Object)null) ? mData.VaporizationAsset.Data : VaporizationInfo.None);
			cuttable.CuttingTargetable.Split(mSplitPlanesArrayBuffer, splitForce, splitGap, cutPoint, planeNormal, planeTangent, impactForce, impactDirection, powerRating, vaporizationInfo, mSplitTangentArrayBuffer);
		}

		private bool TryGetTargetables(ref List<CuttableInfo> targetablesOnLine)
		{
			targetablesOnLine.Clear();
			bool flag = false;
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				flag |= TryGetCutLineTargetables(mData.BuffableCutExecution, mData.BuffableCutExecution.BuffableCutLines[i], ref targetablesOnLine, out var _, out var _);
			}
			return flag;
		}

		private bool TryGetCutLineTargetables(ICutExecutionData data, ICutLineData cutData, ref List<CuttableInfo> targetablesOnLine, out Vector2 cutRotation, out bool anyBelowPowerRating)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0235: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			anyBelowPowerRating = false;
			if ((Object)(object)LynxCameraController.MainCamera == (Object)null)
			{
				Debug.LogError((object)"CuttingController failed to get MainCamera");
				cutRotation = Vector2.get_zero();
				return false;
			}
			GetRotatedCutLine(cutData, mCutRotation, out var newPosition, out cutRotation);
			Vector2 val = LynxCameraController.ScreenCenter + newPosition * (float)LynxCameraController.ScreenWidth;
			mAnythingInRange = false;
			if (cutData.CutAllObjectsOnLine)
			{
				int num = (int)(cutData.CutLineWidth * mData.RaycastScreenWidthDensity * 100f);
				float num2 = cutData.CutLineWidth * cutRotation.x;
				float num3 = cutData.CutLineWidth * cutRotation.y;
				int num4 = num / 2;
				for (int i = 0; i < num; i++)
				{
					int rayIndexCenterOffset = Mathf.Abs(num4 - i);
					float num5 = (float)i / (float)(num - 1);
					float num6 = val.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num5);
					float num7 = val.y + (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num5);
					Ray ray = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num6, num7));
					AddHitValidTargetable(ray, rayIndexCenterOffset, data, ref targetablesOnLine, out var anyBelowPowerRating2);
					anyBelowPowerRating |= anyBelowPowerRating2;
				}
			}
			else
			{
				Ray ray2 = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(val.x, val.y));
				AddHitValidTargetable(ray2, 0, data, ref targetablesOnLine, out var anyBelowPowerRating3);
				anyBelowPowerRating |= anyBelowPowerRating3;
			}
			bool flag = targetablesOnLine.Count > 0;
			if (flag)
			{
				int num8 = mEdgeDetectionPoints.Points.Length;
				float x = cutRotation.x;
				float y = cutRotation.y;
				_ = num8 / 2;
				mEdgeDetectionPoints.FirstHitIndex = -1;
				mEdgeDetectionPoints.LastHitIndex = -1;
				Vector3 val2 = default(Vector3);
				RaycastHit val4 = default(RaycastHit);
				for (int j = 0; j < num8; j++)
				{
					float num9 = (float)j / (float)(num8 - 1);
					float num10 = val.x + (float)LynxCameraController.ScreenWidth * ((0f - x) * 0.5f + x * num9);
					float num11 = val.y + (float)LynxCameraController.ScreenWidth * ((0f - y) * 0.5f + y * num9);
					((Vector3)(ref val2))._002Ector(num10, num11);
					Ray val3 = LynxCameraController.MainCamera.ScreenPointToRay(val2);
					mEdgeDetectionPoints.Points[j].ScreenPosition = val2;
					if (Physics.Raycast(val3, ref val4, data.Range * 3f, LayerMask.op_Implicit(mData.RaycastLayerMask)))
					{
						bool flag2 = false;
						for (int k = 0; k < targetablesOnLine.Count; k++)
						{
							StructurePart componentInParent = ((Component)((Component)((RaycastHit)(ref val4)).get_collider()).get_transform()).GetComponentInParent<StructurePart>();
							if ((Object)(object)componentInParent != (Object)null && (Object)(object)componentInParent == (Object)(object)targetablesOnLine[k].Component)
							{
								flag2 = true;
								mEdgeDetectionPoints.Points[j].State = ((mCuttingToolController.State != CuttingState.Disabled) ? EdgeDetectionPoint.PointState.Valid : EdgeDetectionPoint.PointState.Invalid);
								if (mEdgeDetectionPoints.FirstHitIndex == -1)
								{
									mEdgeDetectionPoints.FirstHitIndex = j;
								}
								mEdgeDetectionPoints.LastHitIndex = j;
								break;
							}
						}
						if (!flag2)
						{
							mEdgeDetectionPoints.Points[j].State = EdgeDetectionPoint.PointState.Invalid;
						}
					}
					else
					{
						mEdgeDetectionPoints.Points[j].State = EdgeDetectionPoint.PointState.None;
					}
				}
			}
			return flag;
		}

		public static void GetRotatedCutLine(ICutLineData cutData, Vector2 rotation, out Vector2 newPosition, out Vector2 newRotation)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			float x = rotation.x;
			float y = rotation.y;
			float x2 = cutData.PositionOffset.x;
			float y2 = cutData.PositionOffset.y;
			float num = x2 * x - y2 * y;
			float num2 = x2 * y + y2 * x;
			newPosition = new Vector2(num, num2);
			if (cutData.RotationOffset != 0f)
			{
				float num3 = cutData.RotationOffset * ((float)Math.PI / 180f);
				x = Mathf.Cos(num3);
				y = Mathf.Sin(num3);
				num = rotation.x * x - rotation.y * y;
				num2 = rotation.x * y + rotation.y * x;
				newRotation = new Vector2(num, num2);
			}
			else
			{
				newRotation = rotation;
			}
		}

		private void AddHitValidTargetable(Ray ray, int rayIndexCenterOffset, ICutExecutionData data, ref List<CuttableInfo> targetablesOnLine, out bool anyBelowPowerRating)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			anyBelowPowerRating = false;
			RaycastHit val = default(RaycastHit);
			if (!Physics.Raycast(ray, ref val, data.Range, LayerMask.op_Implicit(mData.RaycastLayerMask)))
			{
				return;
			}
			mAnythingInRange = true;
			int num = 1 << ((Component)((RaycastHit)(ref val)).get_collider()).get_gameObject().get_layer();
			LayerMask validSurfaceLayerMask = mData.ValidSurfaceLayerMask;
			if ((num & ((LayerMask)(ref validSurfaceLayerMask)).get_value()) <= 0)
			{
				return;
			}
			for (int i = 0; i < data.ExcludeTags.Length; i++)
			{
				if (((Component)((RaycastHit)(ref val)).get_collider()).CompareTag(data.ExcludeTags[i]))
				{
					return;
				}
			}
			if (!TryGetValidTargetable(val, mData.BuffableCutExecution.PowerRating, out var part, out anyBelowPowerRating))
			{
				return;
			}
			bool flag = true;
			for (int j = 0; j < targetablesOnLine.Count; j++)
			{
				CuttableInfo value = targetablesOnLine[j];
				if ((Object)(object)value.Component == (Object)(object)part)
				{
					flag = false;
					if (rayIndexCenterOffset < value.RayIndexCenterOffset)
					{
						value.RayIndexCenterOffset = rayIndexCenterOffset;
						value.HitPoint = val;
						targetablesOnLine[j] = value;
					}
					break;
				}
			}
			if (flag)
			{
				targetablesOnLine.Add(new CuttableInfo(part, val, rayIndexCenterOffset));
			}
		}

		public static bool TryGetValidTargetable(RaycastHit hit, int powerRating, out StructurePart part, out bool anyBelowPowerRating, bool uiInfo = false)
		{
			part = ((Component)((Component)((RaycastHit)(ref hit)).get_collider()).get_transform()).GetComponentInParent<StructurePart>();
			anyBelowPowerRating = false;
			if ((Object)(object)part != (Object)null && part.CuttingTargetable != null)
			{
				bool flag = part.CuttingTargetable.IsAimCuttable();
				bool flag2 = part.CuttingTargetable.IsQuickCuttable();
				bool flag3 = part.CuttingTargetable.IsBelowPowerRating(powerRating);
				anyBelowPowerRating |= flag3;
				if (uiInfo || ((flag || flag2) && !flag3))
				{
					return true;
				}
			}
			return false;
		}

		public bool TryGetTargetableInRange(out StructurePart targetable, out bool anyBelowPowerRating, bool uiInfo = false)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			anyBelowPowerRating = false;
			RaycastHit hit = default(RaycastHit);
			if ((Object)(object)LynxCameraController.MainCameraTransform != (Object)null && Physics.Raycast(LynxCameraController.MainCameraTransform.get_position(), LynxCameraController.MainCameraTransform.get_forward(), ref hit, ActiveCutData.Range, LayerMask.op_Implicit(mData.RaycastLayerMask)) && TryGetValidTargetable(hit, mData.BuffableCutExecution.PowerRating, out targetable, out anyBelowPowerRating, uiInfo))
			{
				return true;
			}
			targetable = null;
			return false;
		}

		public override void DrawDebugGizmos()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			if (!Application.get_isPlaying() || (Object)(object)LynxCameraController.MainCamera == (Object)null)
			{
				return;
			}
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = mData.BuffableCutExecution.BuffableCutLines[i];
				GetRotatedCutLine(cutLineBuffableData, mCutRotation, out var newPosition, out var newRotation);
				Vector2 val = LynxCameraController.ScreenCenter + newPosition * (float)LynxCameraController.ScreenWidth;
				Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(val.x, val.y)));
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * newRotation.x;
					float num3 = cutLineBuffableData.CutLineWidth * newRotation.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float num5 = val.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num4);
						float num6 = val.y + (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num4);
						Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num5, num6)));
					}
				}
			}
		}

		public override void DrawDebugInfo()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			if (!Application.get_isPlaying() || (Object)(object)LynxCameraController.MainCamera == (Object)null)
			{
				return;
			}
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = mData.BuffableCutExecution.BuffableCutLines[i];
				GetRotatedCutLine(cutLineBuffableData, mCutRotation, out var newPosition, out var newRotation);
				Vector2 val = LynxCameraController.ScreenCenter + newPosition * (float)LynxCameraController.ScreenWidth;
				GUI.Box(new Rect(val.x - 1f, val.y - 1f, 1f, 1f), (Texture)(object)Texture2D.get_whiteTexture());
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * newRotation.x;
					float num3 = cutLineBuffableData.CutLineWidth * newRotation.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float num5 = val.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num4);
						float num6 = val.y - (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num4);
						GUI.Box(new Rect(num5 - 1f, num6 - 1f, 1f, 1f), (Texture)(object)Texture2D.get_whiteTexture());
					}
				}
			}
		}
	}
}
