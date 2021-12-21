using System;
using System.Collections.Generic;
using BBI;
using BBI.Unity.Game;
using Carbon.Core;
using Carbon.Core.Events;
using Carbon.Core.Unity;
using Unity.Mathematics;
using UnityEngine;

// Token: 0x0200005B RID: 91
public class TetherController : MonoBehaviour
{
	// Token: 0x17000062 RID: 98
	// (get) Token: 0x0600025D RID: 605 RVA: 0x0000CBC5 File Offset: 0x0000ADC5
	private TetherController.CandidateHookPoint ValidCandidateStartAnchor
	{
		get
		{
			if (this.mCandidateTetherState == TetherController.CandidateState.Reverse)
			{
				return this.mReverseCandidateStartAnchor;
			}
			return this.mCandidateStartAnchor;
		}
	}

	// Token: 0x17000063 RID: 99
	// (get) Token: 0x0600025E RID: 606 RVA: 0x0000CBDD File Offset: 0x0000ADDD
	private TetherController.CandidateHookPoint ValidCandidateEndAnchor
	{
		get
		{
			if (this.mCandidateTetherState == TetherController.CandidateState.Reverse)
			{
				return this.mReverseCandidateEndAnchor;
			}
			return this.mCandidateEndAnchor;
		}
	}

	// Token: 0x17000064 RID: 100
	// (get) Token: 0x0600025F RID: 607 RVA: 0x0000CBF5 File Offset: 0x0000ADF5
	private int BezierPoints
	{
		get
		{
			if (!(this.mData.TetherPrefab != null))
			{
				return 0;
			}
			return this.mData.TetherPrefab.BezierPoints;
		}
	}

	// Token: 0x17000065 RID: 101
	// (get) Token: 0x06000260 RID: 608 RVA: 0x0000CC1C File Offset: 0x0000AE1C
	private float StartAnchorBezierFactor
	{
		get
		{
			if (!(this.mData.TetherPrefab != null))
			{
				return 1f;
			}
			return this.mData.TetherPrefab.StartAnchorBezierFactor;
		}
	}

	// Token: 0x17000066 RID: 102
	// (get) Token: 0x06000261 RID: 609 RVA: 0x0000CC47 File Offset: 0x0000AE47
	private float EndAnchorBezierFactor
	{
		get
		{
			if (!(this.mData.TetherPrefab != null))
			{
				return 1f;
			}
			return this.mData.TetherPrefab.EndAnchorBezierFactor;
		}
	}

	// Token: 0x17000067 RID: 103
	// (get) Token: 0x06000262 RID: 610 RVA: 0x0000CC72 File Offset: 0x0000AE72
	// (set) Token: 0x06000263 RID: 611 RVA: 0x0000CC7A File Offset: 0x0000AE7A
	public Action<int> OnNumTethersChanged { get; set; }

	// Token: 0x17000068 RID: 104
	// (get) Token: 0x06000264 RID: 612 RVA: 0x0000CC83 File Offset: 0x0000AE83
	public TetherController.TetherState State
	{
		get
		{
			return this.mState;
		}
	}

	// Token: 0x17000069 RID: 105
	// (get) Token: 0x06000265 RID: 613 RVA: 0x0000CC8B File Offset: 0x0000AE8B
	public bool IsEquipped
	{
		get
		{
			return this.m_EquipmentController.CurrentEquipment == EquipmentController.Equipment.GrappleHook;
		}
	}

	// Token: 0x1700006A RID: 106
	// (get) Token: 0x06000266 RID: 614 RVA: 0x0000CC9B File Offset: 0x0000AE9B
	public bool TetherUnlocked
	{
		get
		{
			return this.mTetherUnlocked;
		}
	}

	// Token: 0x1700006B RID: 107
	// (get) Token: 0x06000267 RID: 615 RVA: 0x0000CCA3 File Offset: 0x0000AEA3
	public bool UnlimitedTethers
	{
		get
		{
			if (GlobalOptions.Raw.GetBool("General.InfTethers", false) && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				return this.mUnlimitedTethers = true;
			}
			return this.mUnlimitedTethers = false;
		}
	}

	// Token: 0x1700006C RID: 108
	// (get) Token: 0x06000268 RID: 616 RVA: 0x0000CCAB File Offset: 0x0000AEAB
	// (set) Token: 0x06000269 RID: 617 RVA: 0x0000CCB4 File Offset: 0x0000AEB4
	public int NumAvailableTethers
	{
		get
		{
			return this.mNumAvailableTethers;
		}
		private set
		{
			this.mNumAvailableTethers = value;
			if (this.OnNumTethersChanged != null)
			{
				this.OnNumTethersChanged(this.mNumAvailableTethers);
			}
			if (this.IsCareerMode && this.IsProfileValid)
			{
				PlayerProfileService.Instance.Profile.Tethers = this.mNumAvailableTethers;
			}
		}
	}

	// Token: 0x1700006D RID: 109
	// (get) Token: 0x0600026A RID: 618 RVA: 0x0000CD06 File Offset: 0x0000AF06
	private bool IsCareerMode
	{
		get
		{
			return SceneLoader.Instance != null && SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career;
		}
	}

	// Token: 0x1700006E RID: 110
	// (get) Token: 0x0600026B RID: 619 RVA: 0x0000CD29 File Offset: 0x0000AF29
	private bool IsProfileValid
	{
		get
		{
			return PlayerProfileService.Instance != null && PlayerProfileService.Instance.Profile != null;
		}
	}

	// Token: 0x0600026C RID: 620 RVA: 0x0000CD44 File Offset: 0x0000AF44
	private void Awake()
	{
		EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
		if (masterScenePlayer != null && masterScenePlayer.TethersAsset != null)
		{
			this.m_TethersAsset = masterScenePlayer.TethersAsset;
		}
		if (this.m_TethersAsset == null)
		{
			Log.Error(128, "Tether Asset is missing.", Array.Empty<object>());
		}
		this.mData = new TethersBuffableData(this.m_TethersAsset.Data);
		this.mState = TetherController.TetherState.Ready;
		this.mMaxTethers = this.mData.MaxTethers;
		this.mIsInitialized = false;
		if (this.mData.TetherPrefab == null)
		{
			Log.Error(128, "Tether prefab is missing.", Array.Empty<object>());
		}
		this.mPreviewPoints = new Vector3[this.BezierPoints];
		this.mReversePreviewPoints = new Vector3[this.BezierPoints];
		Main.EventSystem.AddHandler<GameStateChangedEvent>(new EventHandler<GameStateChangedEvent>(this.OnGameStateChanged));
		Main.EventSystem.AddHandler<TetherChangedEvent>(new EventHandler<TetherChangedEvent>(this.OnTetherChanged));
		Main.EventSystem.AddHandler<RigidbodyCutEvent>(new EventHandler<RigidbodyCutEvent>(this.OnRigidbodyCut));
		Main.EventSystem.AddHandler<HierarchySplitCompleteEvent>(new EventHandler<HierarchySplitCompleteEvent>(this.OnHierarchySplit));
		Main.EventSystem.AddHandler<ForceReleaseEvent>(new EventHandler<ForceReleaseEvent>(this.OnForceRelease));
		Main.EventSystem.AddHandler<HierarchyJointReparentedEvent>(new EventHandler<HierarchyJointReparentedEvent>(this.OnHierarchyJointReparented));
		Main.EventSystem.AddHandler<UnlockAbilityEvent>(new EventHandler<UnlockAbilityEvent>(this.OnTetherUnlocked));
		Main.EventSystem.AddHandler<PurchaseTetherEvent>(new EventHandler<PurchaseTetherEvent>(this.OnTetherPurchased));
		Main.EventSystem.AddHandler<EnableUnlimitedTethersEvent>(new EventHandler<EnableUnlimitedTethersEvent>(this.OnUnlimitedTethersEnabled));
	}

	// Token: 0x0600026D RID: 621 RVA: 0x0000CEE0 File Offset: 0x0000B0E0
	private void OnDestroy()
	{
		Main.EventSystem.RemoveHandler<GameStateChangedEvent>(new EventHandler<GameStateChangedEvent>(this.OnGameStateChanged));
		Main.EventSystem.RemoveHandler<TetherChangedEvent>(new EventHandler<TetherChangedEvent>(this.OnTetherChanged));
		Main.EventSystem.RemoveHandler<RigidbodyCutEvent>(new EventHandler<RigidbodyCutEvent>(this.OnRigidbodyCut));
		Main.EventSystem.RemoveHandler<HierarchySplitCompleteEvent>(new EventHandler<HierarchySplitCompleteEvent>(this.OnHierarchySplit));
		Main.EventSystem.RemoveHandler<ForceReleaseEvent>(new EventHandler<ForceReleaseEvent>(this.OnForceRelease));
		Main.EventSystem.RemoveHandler<HierarchyJointReparentedEvent>(new EventHandler<HierarchyJointReparentedEvent>(this.OnHierarchyJointReparented));
		Main.EventSystem.RemoveHandler<PurchaseTetherEvent>(new EventHandler<PurchaseTetherEvent>(this.OnTetherPurchased));
		Main.EventSystem.RemoveHandler<EnableUnlimitedTethersEvent>(new EventHandler<EnableUnlimitedTethersEvent>(this.OnUnlimitedTethersEnabled));
	}

	// Token: 0x0600026E RID: 622 RVA: 0x0000CFA0 File Offset: 0x0000B1A0
	private void Initialize()
	{
		this.mMaxTethers = this.mData.MaxTethers;
		if (this.IsCareerMode && this.IsProfileValid && !PlayerProfileService.Instance.Profile.PendingTetherRefill && PlayerProfileService.Instance.Profile.Tethers != -1)
		{
			this.NumAvailableTethers = PlayerProfileService.Instance.Profile.Tethers;
		}
		else
		{
			this.NumAvailableTethers = this.mMaxTethers;
			if (this.IsCareerMode && this.IsProfileValid && PlayerProfileService.Instance.Profile.PendingTetherRefill)
			{
				PlayerProfileService.Instance.Profile.PendingTetherRefill = false;
			}
		}
		this.mIsInitialized = true;
	}

	// Token: 0x0600026F RID: 623 RVA: 0x0000D04C File Offset: 0x0000B24C
	private void Update()
	{
		if (this.mTetherUnlocked)
		{
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode || (!PlayerProfileService.Instance.Profile.TutorialCompleted && SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career))
			{
				this.mMaxTethers = 1000;
				this.NumAvailableTethers = 1000;
				this.mIsInitialized = true;
			}
			else if (!this.mIsInitialized)
			{
				this.Initialize();
			}
			if (this.mLifetime != this.mData.Lifetime)
			{
				this.mLifetime = this.mData.Lifetime;
			}
			for (int i = 0; i < this.mActiveTethers.Count; i++)
			{
				this.mActiveTethers[i].Process(Time.deltaTime);
			}
			TetherController.TetherState tetherState = this.mState;
			if (tetherState != TetherController.TetherState.Ready)
			{
				if (tetherState != TetherController.TetherState.Placing)
				{
					Debug.LogError("Unhandled tether state " + this.mState);
				}
				else
				{
					this.HandlePlacing();
				}
			}
			else
			{
				this.HandleReady();
			}
			this.HandleHookMovement();
			this.HandleDistanceToTethers();
		}
	}

	// Token: 0x06000270 RID: 624 RVA: 0x0000D160 File Offset: 0x0000B360
	private void LateUpdate()
	{
		for (int i = 0; i < this.mActiveTethers.Count; i++)
		{
			this.mActiveTethers[i].LateProcess();
		}
	}

	// Token: 0x06000271 RID: 625 RVA: 0x0000D194 File Offset: 0x0000B394
	private void SetTetherState(TetherController.TetherState state)
	{
		if (state != this.mState)
		{
			if (state != TetherController.TetherState.Ready)
			{
				if (state != TetherController.TetherState.Placing)
				{
					Debug.LogError("Trying to set tether state to unhanded state " + state);
					return;
				}
				this.mCandidateTetherState = TetherController.CandidateState.None;
				this.m_GrapplingHook.CanBeUnequipped = false;
				this.m_TetherPreview.positionCount = 0;
				this.m_GrapplingHook.TriggerGrappleAnimation(this.m_GrapplingHook.LaunchAnimationName);
				this.m_GrapplingHook.TriggerGrappleAnimation(this.m_GrapplingHook.ImpactAnimationName);
			}
			else
			{
				TetherController.CandidateHookPoint validCandidateEndAnchor = this.ValidCandidateEndAnchor;
				if (validCandidateEndAnchor.HookedStructurePart != null || validCandidateEndAnchor.HookedBody != null)
				{
					GameObject gameObject = ((validCandidateEndAnchor.HookedBody != null) ? validCandidateEndAnchor.HookedBody.gameObject : null);
					GameObject virtualObject = ((validCandidateEndAnchor.HookedStructurePart != null) ? validCandidateEndAnchor.HookedStructurePart.gameObject : null);
					if (!GrapplingHook.IsObjectStatic(gameObject, virtualObject))
					{
						Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Stop, gameObject, virtualObject));
					}
				}
				this.mCandidateTetherState = TetherController.CandidateState.None;
				this.m_GrapplingHook.CanBeUnequipped = true;
				this.m_TetherPreview.positionCount = 0;
				this.m_GrapplingHook.TriggerGrappleAnimation(this.m_GrapplingHook.ReleaseAnimationName);
				this.mStartAnchorPlaced = false;
				this.DetachHookFX();
			}
			this.mState = state;
		}
	}

	// Token: 0x06000272 RID: 626 RVA: 0x0000D2E4 File Offset: 0x0000B4E4
	private void HandleReady()
	{
		this.TryDespawnTether();
		if (this.m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook)
		{
			return;
		}
		if (this.m_GrapplingHook.GrappledRigidbody != null)
		{
			return;
		}
		if (this.m_GrabController.RightHand.CurrentHandState == HandGrab.HandState.Success)
		{
			return;
		}
		if (LynxControls.Instance.GameplayActions.PlaceTether.WasPressed)
		{
			this.m_GunController.EquipGun();
			if (this.NumAvailableTethers > 0)
			{
				RaycastHit raycastHit;
				if (this.TryGetTetherPoint(out raycastHit) && raycastHit.rigidbody != null)
				{
					this.SpawnFireFX();
					this.mStartHook = this.SpawnHookFX(raycastHit.normal);
					this.mCandidateStartAnchor = new TetherController.CandidateHookPoint(raycastHit.rigidbody, raycastHit.collider.GetComponentInParent<StructurePart>(), raycastHit.rigidbody.transform.InverseTransformPoint(raycastHit.point), raycastHit.rigidbody.transform.InverseTransformDirection(raycastHit.normal));
					this.SetTetherState(TetherController.TetherState.Placing);
					Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Placing));
					return;
				}
			}
			else
			{
				Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Unavailable));
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherUnavailableAudioEvent));
				Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, this.mData.NoTethersSpeech.Data.TriggeredSpeech, null, null));
			}
		}
	}

	// Token: 0x06000273 RID: 627 RVA: 0x0000D454 File Offset: 0x0000B654
	private void HandlePlacing()
	{
		if (LynxControls.Instance.GameplayActions.GrappleFire.WasPressed || LynxControls.Instance.GameplayActions.RecallTethers.WasPressed || LynxControls.Instance.GameplayActions.CancelTether.WasPressed)
		{
			this.ClearTetherState();
			return;
		}
		if (this.m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook)
		{
			this.ClearTetherState();
			return;
		}
		if (this.m_GrapplingHook.GrappledRigidbody != null)
		{
			this.ClearTetherState();
			return;
		}
		if (this.mStartAnchorPlaced && this.mCandidateStartAnchor.HookedBody == null)
		{
			this.ClearTetherState();
			return;
		}
		this.HandlePreviewLine();
		if (LynxControls.Instance.GameplayActions.PlaceTether.WasReleased)
		{
			if (this.mCandidateTetherState != TetherController.CandidateState.None)
			{
				this.SpawnFireFX();
			}
			else
			{
				this.ClearTetherState();
			}
			this.m_TetherPreview.positionCount = 0;
			if (this.mStartAnchorPlaced && this.mCandidateTetherState != TetherController.CandidateState.None)
			{
				this.TryCreateTether();
				this.DespawnHookFX();
				this.SetTetherState(TetherController.TetherState.Ready);
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherPlacedGoodAudioEvent));
				if (this.NumAvailableTethers == this.mData.LowTethersAmount)
				{
					Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, this.mData.LowTethersSpeech.Data.TriggeredSpeech, null, null));
				}
				else if (this.NumAvailableTethers == 0 && this.m_TethersAsset.Data.NoTethersAction != null)
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(this.m_TethersAsset.Data.NoTethersAction, MathUtility.OperationType.Add, 1));
				}
				if (this.m_TethersAsset.Data.PlaceTetherAction != null)
				{
					Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(this.m_TethersAsset.Data.PlaceTetherAction, MathUtility.OperationType.Add, 1));
					return;
				}
			}
			else
			{
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherPlacedBadAudioEvent));
			}
		}
	}

	// Token: 0x06000274 RID: 628 RVA: 0x0000D658 File Offset: 0x0000B858
	private void HandlePreviewLine()
	{
		if (this.mStartAnchorPlaced)
		{
			Rigidbody hookedBody = this.mCandidateEndAnchor.HookedBody;
			StructurePart hookedStructurePart = this.mCandidateEndAnchor.HookedStructurePart;
			this.mCandidateTetherState = TetherController.CandidateState.None;
			Vector3 vector = this.mCandidateStartAnchor.WorldAttachPoint;
			Vector3 endPoint = LynxCameraController.MainCameraTransform.position + LynxCameraController.MainCameraTransform.forward * this.mData.LaunchRange;
			RaycastHit raycastHit;
			if (this.TryGetTetherPoint(out raycastHit) && raycastHit.rigidbody != null)
			{
				endPoint = raycastHit.point;
				TetherController.ResetControlPoints(this.mControlPoints, vector, endPoint, this.mCandidateStartAnchor.WorldNormal * this.StartAnchorBezierFactor, raycastHit.normal * this.EndAnchorBezierFactor);
				bool flag = TetherController.FindIntersections(this.mPreviewPoints, ref vector, ref endPoint, this.BezierPoints, this.mControlPoints, this.mData.RaycastLayerMask, out this.mCandidateEndAnchor);
				this.mCandidateTetherState = (flag ? TetherController.CandidateState.Forward : TetherController.CandidateState.None);
				StructurePart componentInParent = raycastHit.collider.GetComponentInParent<StructurePart>();
				if (!flag)
				{
					this.mCandidateTetherState = TetherController.CandidateState.Forward;
					this.mCandidateEndAnchor = new TetherController.CandidateHookPoint(raycastHit.rigidbody, componentInParent, raycastHit.rigidbody.transform.InverseTransformPoint(raycastHit.point), raycastHit.rigidbody.transform.InverseTransformDirection(raycastHit.normal));
				}
				else if (this.mCandidateEndAnchor.HookedBody == this.mCandidateStartAnchor.HookedBody)
				{
					bool flag2 = raycastHit.rigidbody != this.mCandidateEndAnchor.HookedBody;
					bool flag3 = !flag2 && componentInParent != this.mCandidateEndAnchor.HookedStructurePart;
					if (flag2 || flag3)
					{
						Vector3 point = raycastHit.point;
						Vector3 vector2 = vector;
						TetherController.ResetControlPoints(this.mControlPoints, point, vector2, raycastHit.normal * this.StartAnchorBezierFactor, this.mCandidateStartAnchor.WorldNormal * this.EndAnchorBezierFactor);
						bool flag4 = TetherController.FindIntersections(this.mReversePreviewPoints, ref point, ref vector2, this.BezierPoints, this.mControlPoints, this.mData.RaycastLayerMask, out this.mReverseCandidateStartAnchor);
						bool flag5 = false;
						bool flag6 = false;
						bool flag7 = false;
						if (flag4)
						{
							flag5 = this.mReverseCandidateStartAnchor.HookedBody == this.mCandidateStartAnchor.HookedBody;
							if (flag5)
							{
								flag6 = this.mReverseCandidateStartAnchor.HookedStructurePart != null && this.mReverseCandidateStartAnchor.HookedStructurePart == this.mCandidateStartAnchor.HookedStructurePart;
								if (!flag6 && this.mReverseCandidateStartAnchor.HookedStructurePart != null)
								{
									StructureGroup group = this.mReverseCandidateStartAnchor.HookedStructurePart.Group;
									flag7 = group != null && group == this.mCandidateStartAnchor.HookedStructurePart.Group;
								}
							}
						}
						if (flag4 && flag5 && (!flag3 || (flag6 || flag7)))
						{
							this.mReverseCandidateEndAnchor = new TetherController.CandidateHookPoint(raycastHit.rigidbody, componentInParent, raycastHit.rigidbody.transform.InverseTransformPoint(raycastHit.point), raycastHit.rigidbody.transform.InverseTransformDirection(raycastHit.normal));
							vector = point;
							endPoint = vector2;
							this.mCandidateTetherState = TetherController.CandidateState.Reverse;
						}
						else
						{
							this.mReverseCandidateStartAnchor = default(TetherController.CandidateHookPoint);
							TetherController.ResetPreviewPoints(this.mReversePreviewPoints, point, vector2, this.BezierPoints);
						}
					}
				}
			}
			bool flag8 = this.mCandidateTetherState > TetherController.CandidateState.None;
			if (!flag8)
			{
				this.mCandidateEndAnchor = default(TetherController.CandidateHookPoint);
				TetherController.ResetPreviewPoints(this.mPreviewPoints, vector, endPoint, this.BezierPoints);
			}
			this.m_TetherPreview.material = ((!flag8) ? this.mData.TetherUnacceptableMaterial : this.mData.TetherAcceptableMaterial);
			this.m_TetherPreview.positionCount = this.BezierPoints;
			Vector3[] positions = ((this.mCandidateTetherState == TetherController.CandidateState.Reverse) ? this.mReversePreviewPoints : this.mPreviewPoints);
			this.m_TetherPreview.SetPositions(positions);
			if (flag8 && !this.mTargetAudioEventTracker)
			{
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherTargetGoodAudioEvent));
				this.mTargetAudioEventTracker = true;
			}
			else if (!flag8 && this.mTargetAudioEventTracker)
			{
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherTargetBadAudioEvent));
				this.mTargetAudioEventTracker = false;
			}
			TetherController.CandidateHookPoint validCandidateEndAnchor = this.ValidCandidateEndAnchor;
			if ((hookedStructurePart != null && hookedStructurePart != validCandidateEndAnchor.HookedStructurePart) || (hookedBody != null && hookedBody != validCandidateEndAnchor.HookedBody))
			{
				GameObject gameObject = ((hookedBody != null) ? hookedBody.gameObject : null);
				GameObject virtualObject = ((hookedStructurePart != null) ? hookedStructurePart.gameObject : null);
				if (!GrapplingHook.IsObjectStatic(gameObject, virtualObject))
				{
					Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Stop, gameObject, virtualObject));
				}
			}
			if ((validCandidateEndAnchor.HookedStructurePart != null && validCandidateEndAnchor.HookedStructurePart != hookedStructurePart) || (validCandidateEndAnchor.HookedBody != null && validCandidateEndAnchor.HookedBody != hookedBody))
			{
				GameObject gameObject2 = ((validCandidateEndAnchor.HookedBody != null) ? validCandidateEndAnchor.HookedBody.gameObject : null);
				GameObject virtualObject2 = ((validCandidateEndAnchor.HookedStructurePart != null) ? validCandidateEndAnchor.HookedStructurePart.gameObject : null);
				if (!GrapplingHook.IsObjectStatic(gameObject2, virtualObject2))
				{
					Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Start, gameObject2, virtualObject2));
				}
			}
		}
	}

	// Token: 0x06000275 RID: 629 RVA: 0x0000DBC8 File Offset: 0x0000BDC8
	private static bool FindIntersections(Vector3[] previewPoints, ref Vector3 startPoint, ref Vector3 endPoint, int bezierPoints, List<Vector3> controlPoints, LayerMask raycastLayerMask, out TetherController.CandidateHookPoint candidateHookPoint)
	{
		bool flag = false;
		candidateHookPoint = default(TetherController.CandidateHookPoint);
		previewPoints[0] = startPoint;
		for (int i = 1; i < bezierPoints - 1; i++)
		{
			if (!flag)
			{
				Vector3 cubicPoint = BezierCurve.GetCubicPoint(controlPoints, (float)i / (float)bezierPoints);
				previewPoints[i] = cubicPoint;
				RaycastHit raycastHit;
				if (Physics.Linecast(previewPoints[i - 1], cubicPoint, ref raycastHit, raycastLayerMask) && raycastHit.rigidbody != null)
				{
					endPoint = raycastHit.point;
					flag = true;
					candidateHookPoint = new TetherController.CandidateHookPoint(raycastHit.rigidbody, raycastHit.collider.GetComponent<StructurePart>(), raycastHit.rigidbody.transform.InverseTransformPoint(raycastHit.point), raycastHit.rigidbody.transform.InverseTransformDirection(raycastHit.normal));
				}
			}
			else
			{
				previewPoints[i] = endPoint;
			}
		}
		previewPoints[bezierPoints - 1] = endPoint;
		return flag;
	}

	// Token: 0x06000276 RID: 630 RVA: 0x0000DCC4 File Offset: 0x0000BEC4
	private static void ResetControlPoints(List<Vector3> controlPoints, Vector3 startPoint, Vector3 endPoint, Vector3 perpendicularStartVector, Vector3 perpendicularEndVector)
	{
		controlPoints.Clear();
		Vector3 item = startPoint + perpendicularStartVector;
		Vector3 item2 = endPoint + perpendicularEndVector;
		controlPoints.Add(startPoint);
		controlPoints.Add(item);
		controlPoints.Add(item2);
		controlPoints.Add(endPoint);
	}

	// Token: 0x06000277 RID: 631 RVA: 0x0000DD04 File Offset: 0x0000BF04
	private static void ResetPreviewPoints(Vector3[] previewPoints, Vector3 startPoint, Vector3 endPoint, int bezierPoints)
	{
		previewPoints[0] = startPoint;
		for (int i = 1; i < bezierPoints; i++)
		{
			previewPoints[i] = endPoint;
		}
	}

	// Token: 0x06000278 RID: 632 RVA: 0x0000DD30 File Offset: 0x0000BF30
	private void HandleHookMovement()
	{
		if (this.mStartHook != null)
		{
			if (this.mStartAnchorPlaced)
			{
				TetherController.CandidateHookPoint validCandidateStartAnchor = this.ValidCandidateStartAnchor;
				this.mStartHook.transform.position = validCandidateStartAnchor.WorldAttachPoint;
				this.mStartHook.transform.rotation = Quaternion.LookRotation(validCandidateStartAnchor.WorldNormal);
				return;
			}
			TetherController.CandidateHookPoint candidateHookPoint = this.mCandidateStartAnchor;
			if (Vector3.Distance(this.mStartHook.transform.position, candidateHookPoint.WorldAttachPoint) <= this.mData.HookSpeed * Time.deltaTime)
			{
				this.mStartAnchorPlaced = true;
				this.SpawnImpactFX(candidateHookPoint.WorldAttachPoint, candidateHookPoint.WorldNormal);
				Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.StartHookPlacedAudioEvent));
				return;
			}
			Vector3 normalized = (candidateHookPoint.WorldAttachPoint - this.mStartHook.transform.position).normalized;
			this.mStartHook.transform.position += normalized * this.mData.HookSpeed * Time.deltaTime;
		}
	}

	// Token: 0x06000279 RID: 633 RVA: 0x0000DE58 File Offset: 0x0000C058
	private void HandleDistanceToTethers()
	{
		if (this.m_TethersAsset.Data.TetherAudioProximityAsset == null)
		{
			return;
		}
		AudioProximityData data = this.m_TethersAsset.Data.TetherAudioProximityAsset.Data;
		if (data == null || !data.ParameterID.IsValid)
		{
			return;
		}
		float newValue = 0f;
		float glitchAmount = 0f;
		if (this.mActiveTethers.Count > 0)
		{
			Vector3 position = LynxCameraController.MainCameraTransform.position;
			float num = float.MaxValue;
			for (int i = 0; i < this.mActiveTethers.Count; i++)
			{
				Tether tether = this.mActiveTethers[i];
				num = Mathf.Min(num, MathUtility.DistanceFromPointToSegment(tether.WorldStartAttachPoint, tether.WorldEndAttachPoint, position));
			}
			float num2 = 1f - Mathf.Clamp01(num / data.Range);
			float num3 = data.ProximityRamp.Evaluate(num2);
			newValue = num3 * data.MaxValue;
			glitchAmount = num3 * data.MaxVisualGlitchAmount;
		}
		if (AudioParameterController.HasParameterChanged(this.mCurrentProximityParameter, newValue))
		{
			this.mCurrentProximityParameter = newValue;
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(data.ParameterID, this.mCurrentProximityParameter));
			Main.EventSystem.Post(ProximityDistortionParamaterChangedEvent.GetEvent(base.gameObject, glitchAmount));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(this.mData.TetherSoundVolRTPC, this.mCurrentProximityParameter));
		}
	}

	// Token: 0x0600027A RID: 634 RVA: 0x0000DFC8 File Offset: 0x0000C1C8
	private void TryCreateTether()
	{
		TetherController.CandidateHookPoint validCandidateStartAnchor = this.ValidCandidateStartAnchor;
		TetherController.CandidateHookPoint validCandidateEndAnchor = this.ValidCandidateEndAnchor;
		if (this.mData.TetherPrefab != null && validCandidateStartAnchor.HookedBody != null && validCandidateEndAnchor.HookedBody != null)
		{
			Tether component = GameSession.SpawnPoolManager.SpawnObject(this.mData.TetherPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Tether>();
			if (component != null)
			{
				component.SpawnRope(validCandidateStartAnchor.HookedBody, validCandidateEndAnchor.HookedBody, validCandidateStartAnchor.HookedStructurePart, validCandidateEndAnchor.HookedStructurePart, validCandidateStartAnchor.WorldAttachPoint, validCandidateEndAnchor.WorldAttachPoint, false, false, this.mData.RawTetheredMassRange, validCandidateStartAnchor.WorldNormal, validCandidateEndAnchor.WorldNormal, this.mData.MaxRopeLength, this.mData.ForceCurve, this.mData.RetractSpeedCurve, this.mLifetime);
				if (validCandidateStartAnchor.HookedBody != validCandidateEndAnchor.HookedBody)
				{
					validCandidateStartAnchor.HookedBody.velocity *= this.mData.VelocityDampening;
					validCandidateStartAnchor.HookedBody.angularVelocity *= this.mData.TorqueDampening;
					validCandidateEndAnchor.HookedBody.velocity *= this.mData.VelocityDampening;
					validCandidateEndAnchor.HookedBody.angularVelocity *= this.mData.TorqueDampening;
				}
				Tether tether = component;
				tether.OnTetherDespawned = (Action<Tether>)Delegate.Combine(tether.OnTetherDespawned, new Action<Tether>(this.OnTetherDespawned));
				this.mActiveTethers.Add(component);
				if (!this.mUnlimitedTethers && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.FreeMode)
				{
					int numAvailableTethers = this.NumAvailableTethers;
					this.NumAvailableTethers = numAvailableTethers - 1;
					return;
				}
			}
		}
		else
		{
			this.ClearTetherState();
		}
	}

	// Token: 0x0600027B RID: 635 RVA: 0x0000E1B8 File Offset: 0x0000C3B8
	private void TryDespawnTether()
	{
		if (this.mActiveTethers.Count <= 0)
		{
			return;
		}
		if (LynxControls.Instance.GameplayActions.RecallTethers.WasPressed)
		{
			for (int i = this.mActiveTethers.Count - 1; i >= 0; i--)
			{
				Tether tether = this.mActiveTethers[i];
				if (tether != null && tether.gameObject.activeInHierarchy)
				{
					Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Recalled));
					tether.DespawnRope();
				}
			}
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherRecalledAudioEvent));
		}
	}

	// Token: 0x0600027C RID: 636 RVA: 0x0000E255 File Offset: 0x0000C455
	private void ClearTetherState()
	{
		this.DetachHookFX();
		Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Failed));
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherLoopStopperAudioEvent));
		this.mTargetAudioEventTracker = false;
		this.SetTetherState(TetherController.TetherState.Ready);
	}

	// Token: 0x0600027D RID: 637 RVA: 0x0000E298 File Offset: 0x0000C498
	private void SpawnFireFX()
	{
		if (this.mData.FireFXPrefab != null)
		{
			GameSession.SpawnPoolManager.SpawnObject<FXElement>(this.mData.FireFXPrefab, this.m_GunBarrel.position, this.m_GunBarrel.rotation, null, null).transform.SetParent(this.m_GunBarrel);
		}
	}

	// Token: 0x0600027E RID: 638 RVA: 0x0000E2F5 File Offset: 0x0000C4F5
	private void SpawnImpactFX(Vector3 position, Vector3 normal)
	{
		if (this.mData.ImpactFXPrefab != null)
		{
			GameSession.SpawnPoolManager.SpawnObject<FXElement>(this.mData.ImpactFXPrefab, position, Quaternion.LookRotation(normal), null, null);
		}
	}

	// Token: 0x0600027F RID: 639 RVA: 0x0000E32C File Offset: 0x0000C52C
	private FXElement SpawnHookFX(Vector3 normal)
	{
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.HookFiredAudioEvent));
		if (this.mData.HookFX != null)
		{
			return GameSession.SpawnPoolManager.SpawnObject<FXElement>(this.mData.HookFX, this.m_GunBarrel.transform.position, Quaternion.LookRotation(normal), null, null);
		}
		return null;
	}

	// Token: 0x06000280 RID: 640 RVA: 0x0000E395 File Offset: 0x0000C595
	private void DespawnHookFX()
	{
		if (this.mStartHook != null)
		{
			GameSession.SpawnPoolManager.DespawnObject<FXElement>(this.mStartHook, null);
			this.mStartHook = null;
		}
	}

	// Token: 0x06000281 RID: 641 RVA: 0x0000E3C0 File Offset: 0x0000C5C0
	private void DetachHookFX()
	{
		if (this.mStartHook != null)
		{
			this.SpawnImpactFX(this.mStartHook.transform.position, this.ValidCandidateStartAnchor.WorldNormal);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(this.mData.TetherHookDetachedAudioEvent));
			this.mStartHook.DetachElement();
			this.mStartHook = null;
		}
	}

	// Token: 0x06000282 RID: 642 RVA: 0x0000E42B File Offset: 0x0000C62B
	private void OnGameStateChanged(GameStateChangedEvent ev)
	{
		if (this.State == TetherController.TetherState.Placing)
		{
			this.ClearTetherState();
		}
		if (ev.GameState == GameSession.GameState.Gameplay && ev.PrevGameState == GameSession.GameState.Hab)
		{
			this.mIsInitialized = false;
		}
	}

	// Token: 0x06000283 RID: 643 RVA: 0x0000E458 File Offset: 0x0000C658
	private void OnTetherDespawned(Tether tether)
	{
		if (this.mActiveTethers.Remove(tether))
		{
			tether.OnTetherDespawned = (Action<Tether>)Delegate.Remove(tether.OnTetherDespawned, new Action<Tether>(this.OnTetherDespawned));
			return;
		}
		Debug.LogError("Trying to remove tether that was never added.", tether.gameObject);
	}

	// Token: 0x06000284 RID: 644 RVA: 0x0000E4A8 File Offset: 0x0000C6A8
	private void OnTetherChanged(TetherChangedEvent ev)
	{
		if (ev.State == TetherChangedEvent.TetherState.AddTethers)
		{
			this.NumAvailableTethers += ev.NumTethers;
			this.NumAvailableTethers = math.min(this.NumAvailableTethers, this.mData.MaxTethers);
			Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, this.mData.AddTethersSpeech.Data.TriggeredSpeech, null, null));
		}
	}

	// Token: 0x06000285 RID: 645 RVA: 0x0000E519 File Offset: 0x0000C719
	public void EquipmentChanged()
	{
		this.ClearTetherState();
	}

	// Token: 0x06000286 RID: 646 RVA: 0x0000E524 File Offset: 0x0000C724
	private void OnRigidbodyCut(RigidbodyCutEvent ev)
	{
		if (ev.Children.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this.mActiveTethers.Count; i++)
		{
			this.mActiveTethers[i].OnRigidbodyCut(ev);
		}
	}

	// Token: 0x06000287 RID: 647 RVA: 0x0000E564 File Offset: 0x0000C764
	private void OnHierarchySplit(HierarchySplitCompleteEvent ev)
	{
		if (ev.NewParts.Length == 0)
		{
			return;
		}
		for (int i = 0; i < this.mActiveTethers.Count; i++)
		{
			this.mActiveTethers[i].OnHierarchySplit(ev);
		}
	}

	// Token: 0x06000288 RID: 648 RVA: 0x0000E5A4 File Offset: 0x0000C7A4
	private void OnHierarchyJointReparented(HierarchyJointReparentedEvent ev)
	{
		for (int i = 0; i < this.mActiveTethers.Count; i++)
		{
			this.mActiveTethers[i].OnHierarchyJointReparented(ev);
		}
	}

	// Token: 0x06000289 RID: 649 RVA: 0x0000E5DC File Offset: 0x0000C7DC
	private void OnForceRelease(ForceReleaseEvent ev)
	{
		if (ev.State == ForceReleaseEvent.ReleaseState.All || ev.State == ForceReleaseEvent.ReleaseState.Tether)
		{
			for (int i = 0; i < this.mActiveTethers.Count; i++)
			{
				this.mActiveTethers[i].OnForceRelease(ev);
			}
		}
	}

	// Token: 0x0600028A RID: 650 RVA: 0x0000E623 File Offset: 0x0000C823
	private void OnTetherUnlocked(UnlockAbilityEvent ev)
	{
		if (ev.Ability == UnlockAbilityID.GrappleTethers)
		{
			this.mTetherUnlocked = true;
		}
	}

	// Token: 0x0600028B RID: 651 RVA: 0x0000E638 File Offset: 0x0000C838
	private void OnTetherPurchased(PurchaseTetherEvent eventReceived)
	{
		bool flag = this.NumAvailableTethers < this.mMaxTethers;
		if (flag)
		{
			Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.AddTethers, eventReceived.NumTethers));
		}
		VendingMachinePurchaseBase.PurchaseCallback callback = eventReceived.Callback;
		if (callback == null)
		{
			return;
		}
		callback(flag);
	}

	// Token: 0x0600028C RID: 652 RVA: 0x0000E67E File Offset: 0x0000C87E
	private void OnUnlimitedTethersEnabled(EnableUnlimitedTethersEvent ev)
	{
		this.mUnlimitedTethers = ev.EnableUnlimitedTethers;
	}

	// Token: 0x0600028D RID: 653 RVA: 0x0000E68C File Offset: 0x0000C88C
	private bool TryGetTetherPoint(out RaycastHit hitInfo)
	{
		if (Physics.Raycast(new Ray(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward), ref hitInfo, this.mData.LaunchRange, this.mData.RaycastLayerMask) && this.IsValidHookable(hitInfo))
		{
			return true;
		}
		bool flag = false;
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
		RaycastHit raycastHit = default(RaycastHit);
		for (int i = 0; i < this.mData.TetherScreenRaysPerRow; i++)
		{
			for (int j = 0; j < this.mData.TetherScreenRaysPerColumn; j++)
			{
				float num = (float)LynxCameraController.ScreenWidth * (0.5f - this.mData.TetherScreenWidth / 2f + this.mData.TetherScreenWidth * ((float)i / (float)(this.mData.TetherScreenRaysPerRow - 1)));
				float num2 = (float)LynxCameraController.ScreenHeight * (0.5f - this.mData.TetherScreenHeight / 2f + this.mData.TetherScreenHeight * ((float)j / (float)(this.mData.TetherScreenRaysPerColumn - 1)));
				RaycastHit raycastHit2;
				if (Physics.Raycast(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num, num2)), ref raycastHit2, this.mData.LaunchRange, this.mData.RaycastLayerMask) && this.IsValidHookable(raycastHit2))
				{
					vector2 = LynxCameraController.MainCamera.WorldToViewportPoint(raycastHit2.point);
					float num3 = GameObjectHelper.DistanceSquaredFromCentreScreen(vector2);
					float num4 = GameObjectHelper.DistanceSquaredFromCentreScreen(vector);
					if (!flag || num3 < num4)
					{
						vector = vector2;
						raycastHit = raycastHit2;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			hitInfo = raycastHit;
			return true;
		}
		return false;
	}

	// Token: 0x0600028E RID: 654 RVA: 0x0000E840 File Offset: 0x0000CA40
	private bool IsValidHookable(RaycastHit hit)
	{
		return ((1 << hit.collider.gameObject.layer) & this.mData.RaycastLayerMask.value) > 0;
	}

	// Token: 0x040001F8 RID: 504
	[SerializeField]
	private TethersAsset m_TethersAsset;

	// Token: 0x040001F9 RID: 505
	[Header("Hook Ups")]
	[SerializeField]
	private EquipmentController m_EquipmentController;

	// Token: 0x040001FA RID: 506
	[SerializeField]
	private LineRenderer m_TetherPreview;

	// Token: 0x040001FB RID: 507
	[SerializeField]
	private GrapplingHook m_GrapplingHook;

	// Token: 0x040001FC RID: 508
	[SerializeField]
	private GunController m_GunController;

	// Token: 0x040001FD RID: 509
	[SerializeField]
	private GrabController m_GrabController;

	// Token: 0x040001FE RID: 510
	[SerializeField]
	private Transform m_GunBarrel;

	// Token: 0x040001FF RID: 511
	private ITethersData mData;

	// Token: 0x04000200 RID: 512
	private TetherController.TetherState mState;

	// Token: 0x04000201 RID: 513
	private int mNumAvailableTethers;

	// Token: 0x04000202 RID: 514
	private int mMaxTethers;

	// Token: 0x04000203 RID: 515
	private float mLifetime;

	// Token: 0x04000204 RID: 516
	private bool mUnlimitedTethers;

	// Token: 0x04000205 RID: 517
	private TetherController.CandidateHookPoint mCandidateStartAnchor;

	// Token: 0x04000206 RID: 518
	private TetherController.CandidateHookPoint mCandidateEndAnchor;

	// Token: 0x04000207 RID: 519
	private TetherController.CandidateHookPoint mReverseCandidateStartAnchor;

	// Token: 0x04000208 RID: 520
	private TetherController.CandidateHookPoint mReverseCandidateEndAnchor;

	// Token: 0x04000209 RID: 521
	private bool mIsInitialized;

	// Token: 0x0400020A RID: 522
	private TetherController.CandidateState mCandidateTetherState;

	// Token: 0x0400020B RID: 523
	private List<Tether> mActiveTethers = new List<Tether>();

	// Token: 0x0400020C RID: 524
	private List<Vector3> mControlPoints = new List<Vector3>(4);

	// Token: 0x0400020D RID: 525
	private Vector3[] mPreviewPoints;

	// Token: 0x0400020E RID: 526
	private Vector3[] mReversePreviewPoints;

	// Token: 0x0400020F RID: 527
	private bool mTetherUnlocked;

	// Token: 0x04000210 RID: 528
	private FXElement mStartHook;

	// Token: 0x04000211 RID: 529
	private bool mStartAnchorPlaced;

	// Token: 0x04000212 RID: 530
	private float mCurrentProximityParameter;

	// Token: 0x04000213 RID: 531
	private bool mTargetAudioEventTracker;

	// Token: 0x02000B8A RID: 2954
	public enum TetherState
	{
		// Token: 0x040034A1 RID: 13473
		None,
		// Token: 0x040034A2 RID: 13474
		Ready,
		// Token: 0x040034A3 RID: 13475
		Placing
	}

	// Token: 0x02000B8B RID: 2955
	public enum CandidateState
	{
		// Token: 0x040034A5 RID: 13477
		None,
		// Token: 0x040034A6 RID: 13478
		Forward,
		// Token: 0x040034A7 RID: 13479
		Reverse
	}

	// Token: 0x02000B8C RID: 2956
	public struct CandidateHookPoint
	{
		// Token: 0x170010A6 RID: 4262
		// (get) Token: 0x06004084 RID: 16516 RVA: 0x00127B62 File Offset: 0x00125D62
		public Vector3 WorldNormal
		{
			get
			{
				if (!(this.HookedBody != null))
				{
					return Vector3.zero;
				}
				return this.HookedBody.transform.TransformDirection(this.LocalNormal);
			}
		}

		// Token: 0x170010A7 RID: 4263
		// (get) Token: 0x06004085 RID: 16517 RVA: 0x00127B8E File Offset: 0x00125D8E
		public Vector3 WorldAttachPoint
		{
			get
			{
				if (!(this.HookedBody != null))
				{
					return Vector3.zero;
				}
				return this.HookedBody.transform.TransformPoint(this.LocalAttachPoint);
			}
		}

		// Token: 0x06004086 RID: 16518 RVA: 0x00127BBA File Offset: 0x00125DBA
		public CandidateHookPoint(Rigidbody hookedBody, StructurePart hookedStructurePart, Vector3 localAttachPoint, Vector3 localNormal)
		{
			this.HookedBody = hookedBody;
			this.HookedStructurePart = hookedStructurePart;
			this.LocalAttachPoint = localAttachPoint;
			this.LocalNormal = localNormal;
		}

		// Token: 0x040034A8 RID: 13480
		public Rigidbody HookedBody;

		// Token: 0x040034A9 RID: 13481
		public StructurePart HookedStructurePart;

		// Token: 0x040034AA RID: 13482
		public Vector3 LocalNormal;

		// Token: 0x040034AB RID: 13483
		public Vector3 LocalAttachPoint;
	}
}
