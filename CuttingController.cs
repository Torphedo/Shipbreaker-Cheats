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

			public Vector3 WorldHitPoint => Component.transform.TransformPoint(mLocalHitPoint);

			public Vector3 WorldPlaneNormal => Component.transform.TransformDirection(mLocalPlaneNormal);

			public Vector3 WorldPlaneTangent => Component.transform.TransformDirection(mLocalPlaneTangent);

			public Vector3 ImpactDirection => Component.transform.TransformDirection(mLocalImpactDirection);

			public bool DelayReached => Time.time - mStartTime >= mQueueDelay;

			public QueuedCuttable(StructurePart component, Vector3 worldHitPoint, Vector3 worldNormal, Vector3 worldTangent, ForceInfo splitForce, float splitGap, ForceInfo impactForce, Vector3 impactDirection, int powerRating, float queueDelay)
			{
				Component = component;
				mLocalHitPoint = Component.transform.InverseTransformPoint(worldHitPoint);
				mLocalPlaneNormal = Component.transform.InverseTransformDirection(worldNormal);
				mLocalPlaneTangent = Component.transform.InverseTransformDirection(worldTangent);
				SplitForce = splitForce;
				SplitGap = splitGap;
				ImpactForce = impactForce;
				PowerRating = powerRating;
				mLocalImpactDirection = Component.transform.InverseTransformDirection(impactDirection);
				mQueueDelay = queueDelay;
				mStartTime = Time.time;
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

		private bool mAnythingInRange;

		[Header("Cutter Asset")]
		[SerializeField]
		private CutterAsset m_CutterAsset;

		[Header("Player Hookups")]
		[SerializeField]
		private Rigidbody m_PlayerRigidbody;

		private ICutterData mData;

		private Vector2 mCutRotation = Vector2.right;

		private List<CuttableInfo> mTargetBuffer = new List<CuttableInfo>();

		private List<QueuedCuttable> mQueuedCuttables = new List<QueuedCuttable>();

		private EdgeDetectionPointList mEdgeDetectionPoints;

		private CutReloadInfo mCutReloadInfo;

		private bool mHorizontalCutNext = true;

		private Plane mCutSplitPlaneBuffer;

		private Plane[] mSplitPlanesArrayBuffer = new Plane[1];

		private Vector3[] mSplitTangentArrayBuffer = new Vector3[1];

		private float mCurrentCutTime;

		private float mMaxCutDelay;

		private bool mAnySuccessfulCuts;

		private CuttingToolController mCuttingToolController;

		public ICutExecutionData ActiveCutData => mData.BuffableCutExecution;

		public bool AnyValidTargetables => mTargetBuffer.Count > 0;

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
			mCuttingToolController = GetComponentInParent<CuttingToolController>();
			if (mCuttingToolController == null)
			{
				Debug.LogError("[CuttingTool] Unable to find CuttingToolController. Disabling.");
				base.enabled = false;
				return;
			}
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if (masterScenePlayer != null && masterScenePlayer.CutterAsset != null)
			{
				m_CutterAsset = masterScenePlayer.CutterAsset;
			}
			if (m_CutterAsset == null)
			{
				Debug.LogError("Cutter Asset is missing.");
			}
			mData = new CutterBuffableData(m_CutterAsset.Data);
			if (mData.BuffableCutExecution == null)
			{
				Debug.LogError("No buffable cut executions found.");
			}
			mCutReloadInfo = default(CutReloadInfo);
			CanBeUnequipped = true;
			mCutRotation = Vector3.right;
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
			TryGetTargetables(ref mTargetBuffer);
			switch (mCuttingToolController.State)
			{
			case CuttingState.Ready:
				HandleCuttingReady();
				break;
			case CuttingState.Disabled:
				HandleCutterDisabled();
				break;
			}
			if (IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterAltFire.WasPressed)
			{
				if (m_CutterAsset.Data.SwapCutterAngle != null)
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(m_CutterAsset.Data.SwapCutterAngle));
				}
				mHorizontalCutNext = !mHorizontalCutNext;
				mCutRotation = (mHorizontalCutNext ? Vector2.right : Vector2.up);
			}
		}

		public void UpdateCutterPassive()
		{
			HandleCutSpeed();
			HandleCoolingDown();
		}

		public void FixedUpdateCutter()
		{
			CuttingState state = mCuttingToolController.State;
			if (state == CuttingState.Cutting)
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
			switch (newState)
			{
			case CuttingState.Ready:
				mTargetBuffer.Clear();
				CanBeUnequipped = true;
				InputManager.ActiveDevice.StopVibration();
				mCutRotation = (mHorizontalCutNext ? Vector2.right : Vector2.up);
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Ready));
				break;
			case CuttingState.Disabled:
				InputManager.ActiveDevice.StopVibration();
				CanBeUnequipped = true;
				Main.EventSystem.Post(CuttingChangedEvent.GetEvent(CuttingChangedEvent.CutState.Disabled));
				break;
			case CuttingState.Cutting:
				mCuttingToolController.EquipCutter();
				CanBeUnequipped = false;
				mCutRotation = (mHorizontalCutNext ? Vector2.right : Vector2.up);
				StartCutting(mData.BuffableCutExecution);
				break;
			}
		}

		private void HandleQueuedCuttables()
		{
			for (int num = mQueuedCuttables.Count - 1; num >= 0; num--)
			{
				QueuedCuttable queuedCuttable = mQueuedCuttables[num];
				if (queuedCuttable.Component != null)
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
			if (IsCurrentEquipment && IsCurrentModeReady && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
			{
				mCuttingToolController.SetState(CuttingState.Cutting);
			}
		}

		private void HandleCutterDisabled()
		{
			if (IsCurrentEquipment && LynxControls.Instance.GameplayActions.CutterFire.WasPressed)
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
			InputManager.ActiveDevice.Vibrate(LynxControls.Instance.GetVibrationIntensity(amount));
			mCutReloadInfo.CutSpeedTimer += Time.deltaTime;
			if (mCutReloadInfo.CutSpeedTimer >= mData.CutTravelDuration)
			{
				mCutReloadInfo.CutSpeedTimer = 0f;
				mCutReloadInfo.IsCutting = false;
				InputManager.ActiveDevice.StopVibration();
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
				mCutReloadInfo.CoolDownTimer += Time.deltaTime;
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
			if (m_PlayerRigidbody != null)
			{
				Vector3 force = -m_PlayerRigidbody.transform.forward * recoilForce.Force;
				m_PlayerRigidbody.AddForce(force, recoilForce.ForceMode);
			}
		}

		private void HandleCutting()
		{
			float num = mCurrentCutTime + Time.deltaTime;
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
			if (LynxCameraController.MainCamera == null)
			{
				Debug.LogError("CuttingController failed to get MainCamera");
				return false;
			}
			mTargetBuffer.Clear();
			if (TryGetCutLineTargetables(data, cutData, ref mTargetBuffer, out var cutRotation))
			{
				Vector3 normalized = Vector3.Cross(LynxCameraController.MainCameraTransform.forward, LynxCameraController.MainCameraTransform.TransformDirection(cutRotation)).normalized;
				float num = 0f;
				ForceInfo splitForce = cutData.SplitForce;
				float splitGap = cutData.SplitGap;
				ForceInfo impactForce = cutData.ImpactForce;
				Vector3 forward = m_PlayerRigidbody.transform.forward;
				int num2 = 0;
				if (mTargetBuffer.Count > 0)
				{
					for (int num3 = mTargetBuffer.Count - 1; num3 >= 0; num3--)
					{
						CuttableInfo cuttableInfo = mTargetBuffer[num3];
						float3 @float = math.normalize(math.cross(cuttableInfo.HitPoint.normal, normalized));
						QueuedCuttable item = new QueuedCuttable(cuttableInfo.Component, cuttableInfo.HitPoint.point, normalized, @float, splitForce, splitGap, impactForce, forward, mData.BuffableCutExecution.PowerRating, data.SplitDelay);
						mQueuedCuttables.Add(item);
						num = Mathf.Max(Vector3.SqrMagnitude(cuttableInfo.HitPoint.point - LynxCameraController.MainCameraTransform.position), num);
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
			if (!(cuttable == null) && cuttable.CuttingTargetable != null)
			{
				if (cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction != null && World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<FirstGeneration>(cuttable.Entity))
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(cuttable.StructurePartAsset.Data.ObjectCutBySplitsawAction));
				}
				Main.EventSystem.Post(ObjectCutEvent.GetEvent(cuttable));
				Main.EventSystem.Post(ForceReleaseEvent.GetEvent(ForceReleaseEvent.ReleaseState.HandsAndGrapple, cuttable.gameObject));
				mCutSplitPlaneBuffer.SetNormalAndPosition(planeNormal, cutPoint);
				mSplitPlanesArrayBuffer[0] = mCutSplitPlaneBuffer;
				mSplitTangentArrayBuffer[0] = planeTangent;
				VaporizationInfo vaporizationInfo = ((mData.VaporizationAsset != null) ? mData.VaporizationAsset.Data : VaporizationInfo.None);
				cuttable.CuttingTargetable.Split(mSplitPlanesArrayBuffer, splitForce, splitGap, cutPoint, planeNormal, planeTangent, impactForce, impactDirection, powerRating, vaporizationInfo, mSplitTangentArrayBuffer);
			}
		}

		private bool TryGetTargetables(ref List<CuttableInfo> targetablesOnLine)
		{
			targetablesOnLine.Clear();
			bool flag = false;
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				flag |= TryGetCutLineTargetables(mData.BuffableCutExecution, mData.BuffableCutExecution.BuffableCutLines[i], ref targetablesOnLine, out var _);
			}
			return flag;
		}

		private bool TryGetCutLineTargetables(ICutExecutionData data, ICutLineData cutData, ref List<CuttableInfo> targetablesOnLine, out Vector2 cutRotation)
		{
			if (LynxCameraController.MainCamera == null)
			{
				Debug.LogError("CuttingController failed to get MainCamera");
				cutRotation = Vector2.zero;
				return false;
			}
			GetRotatedCutLine(cutData, mCutRotation, out var newPosition, out cutRotation);
			Vector2 vector = LynxCameraController.ScreenCenter + newPosition * LynxCameraController.ScreenWidth;
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
					float x = vector.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num5);
					float y = vector.y + (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num5);
					Ray ray = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(x, y));
					AddHitValidTargetable(ray, rayIndexCenterOffset, data, ref targetablesOnLine);
				}
			}
			else
			{
				Ray ray2 = LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(vector.x, vector.y));
				AddHitValidTargetable(ray2, 0, data, ref targetablesOnLine);
			}
			bool flag = targetablesOnLine.Count > 0;
			if (flag)
			{
				int num6 = mEdgeDetectionPoints.Points.Length;
				float x2 = cutRotation.x;
				float y2 = cutRotation.y;
				_ = num6 / 2;
				mEdgeDetectionPoints.FirstHitIndex = -1;
				mEdgeDetectionPoints.LastHitIndex = -1;
				for (int j = 0; j < num6; j++)
				{
					float num7 = (float)j / (float)(num6 - 1);
					float x3 = vector.x + (float)LynxCameraController.ScreenWidth * ((0f - x2) * 0.5f + x2 * num7);
					float y3 = vector.y + (float)LynxCameraController.ScreenWidth * ((0f - y2) * 0.5f + y2 * num7);
					Vector3 vector2 = new Vector3(x3, y3);
					Ray ray3 = LynxCameraController.MainCamera.ScreenPointToRay(vector2);
					mEdgeDetectionPoints.Points[j].ScreenPosition = vector2;
					if (Physics.Raycast(ray3, out var hitInfo, data.Range * 3f, mData.RaycastLayerMask))
					{
						bool flag2 = false;
						for (int k = 0; k < targetablesOnLine.Count; k++)
						{
							StructurePart componentInParent = hitInfo.collider.transform.GetComponentInParent<StructurePart>();
							if (componentInParent != null && componentInParent == targetablesOnLine[k].Component)
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
			float x = rotation.x;
			float y = rotation.y;
			float x2 = cutData.PositionOffset.x;
			float y2 = cutData.PositionOffset.y;
			float x3 = x2 * x - y2 * y;
			float y3 = x2 * y + y2 * x;
			newPosition = new Vector2(x3, y3);
			if (cutData.RotationOffset != 0f)
			{
				float f = cutData.RotationOffset * ((float)Math.PI / 180f);
				x = Mathf.Cos(f);
				y = Mathf.Sin(f);
				x3 = rotation.x * x - rotation.y * y;
				y3 = rotation.x * y + rotation.y * x;
				newRotation = new Vector2(x3, y3);
			}
			else
			{
				newRotation = rotation;
			}
		}

		private void AddHitValidTargetable(Ray ray, int rayIndexCenterOffset, ICutExecutionData data, ref List<CuttableInfo> targetablesOnLine)
		{
			if (!Physics.Raycast(ray, out var hitInfo, data.Range, mData.RaycastLayerMask))
			{
				return;
			}
			mAnythingInRange = true;
			if (((1 << hitInfo.collider.gameObject.layer) & mData.ValidSurfaceLayerMask.value) <= 0)
			{
				return;
			}
			for (int i = 0; i < data.ExcludeTags.Length; i++)
			{
				if (hitInfo.collider.CompareTag(data.ExcludeTags[i]))
				{
					return;
				}
			}
			if (!TryGetValidTargetable(hitInfo, mData.BuffableCutExecution.PowerRating, out var part))
			{
				return;
			}
			bool flag = true;
			for (int j = 0; j < targetablesOnLine.Count; j++)
			{
				CuttableInfo value = targetablesOnLine[j];
				if (value.Component == part)
				{
					flag = false;
					if (rayIndexCenterOffset < value.RayIndexCenterOffset)
					{
						value.RayIndexCenterOffset = rayIndexCenterOffset;
						value.HitPoint = hitInfo;
						targetablesOnLine[j] = value;
					}
					break;
				}
			}
			if (flag)
			{
				targetablesOnLine.Add(new CuttableInfo(part, hitInfo, rayIndexCenterOffset));
			}
		}

		public static bool TryGetValidTargetable(RaycastHit hit, int powerRating, out StructurePart part, bool uiInfo = false)
		{
			part = hit.collider.transform.GetComponentInParent<StructurePart>();
			if (part != null && part.CuttingTargetable != null)
			{
				bool flag = part.CuttingTargetable.IsAimCuttable();
				bool flag2 = part.CuttingTargetable.IsQuickCuttable();
				if (uiInfo || ((flag || flag2) && !part.CuttingTargetable.IsBelowPowerRating(powerRating)))
				{
					return true;
				}
			}
			return false;
		}

		public bool TryGetTargetableInRange(out StructurePart targetable, bool uiInfo = false)
		{
			if (LynxCameraController.MainCameraTransform != null && Physics.Raycast(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward, out var hitInfo, ActiveCutData.Range, mData.RaycastLayerMask) && TryGetValidTargetable(hitInfo, mData.BuffableCutExecution.PowerRating, out targetable, uiInfo))
			{
				return true;
			}
			targetable = null;
			return false;
		}

		public override void DrawDebugGizmos()
		{
			if (!Application.isPlaying || LynxCameraController.MainCamera == null)
			{
				return;
			}
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = mData.BuffableCutExecution.BuffableCutLines[i];
				GetRotatedCutLine(cutLineBuffableData, mCutRotation, out var newPosition, out var newRotation);
				Vector2 vector = LynxCameraController.ScreenCenter + newPosition * LynxCameraController.ScreenWidth;
				Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(vector.x, vector.y)));
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * newRotation.x;
					float num3 = cutLineBuffableData.CutLineWidth * newRotation.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float x = vector.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num4);
						float y = vector.y + (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num4);
						Gizmos.DrawRay(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(x, y)));
					}
				}
			}
		}

		public override void DrawDebugInfo()
		{
			if (!Application.isPlaying || LynxCameraController.MainCamera == null)
			{
				return;
			}
			for (int i = 0; i < mData.BuffableCutExecution.BuffableCutLines.Count; i++)
			{
				CutLineBuffableData cutLineBuffableData = mData.BuffableCutExecution.BuffableCutLines[i];
				GetRotatedCutLine(cutLineBuffableData, mCutRotation, out var newPosition, out var newRotation);
				Vector2 vector = LynxCameraController.ScreenCenter + newPosition * LynxCameraController.ScreenWidth;
				GUI.Box(new Rect(vector.x - 1f, vector.y - 1f, 1f, 1f), Texture2D.whiteTexture);
				if (cutLineBuffableData.CutAllObjectsOnLine)
				{
					int num = (int)(cutLineBuffableData.CutLineWidth * mData.RaycastScreenWidthDensity * 100f);
					float num2 = cutLineBuffableData.CutLineWidth * newRotation.x;
					float num3 = cutLineBuffableData.CutLineWidth * newRotation.y;
					for (int j = 0; j < num; j++)
					{
						float num4 = (float)j / (float)(num - 1);
						float num5 = vector.x + (float)LynxCameraController.ScreenWidth * ((0f - num2) * 0.5f + num2 * num4);
						float num6 = vector.y - (float)LynxCameraController.ScreenWidth * ((0f - num3) * 0.5f + num3 * num4);
						GUI.Box(new Rect(num5 - 1f, num6 - 1f, 1f, 1f), Texture2D.whiteTexture);
					}
				}
			}
		}
	}
}
