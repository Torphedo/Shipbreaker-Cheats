using System;
using System.Collections.Generic;
using BBI;
using BBI.Unity.Game;
using Carbon.Audio;
using Carbon.Core;
using Carbon.Core.Unity;
using InControl;
using Unity.Mathematics;
using UnityEngine;

public class TetherController : MonoBehaviour
{
	public enum TetherState
	{
		None,
		Ready,
		Placing
	}

	public enum CandidateState
	{
		None,
		Forward,
		Reverse
	}

	public struct CandidateHookPoint
	{
		public Rigidbody HookedBody;

		public StructurePart HookedStructurePart;

		public Vector3 LocalNormal;

		public Vector3 LocalAttachPoint;

		public Vector3 WorldNormal
		{
			get
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)HookedBody != (Object)null))
				{
					return Vector3.get_zero();
				}
				return ((Component)HookedBody).get_transform().TransformDirection(LocalNormal);
			}
		}

		public Vector3 WorldAttachPoint
		{
			get
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_0025: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)HookedBody != (Object)null))
				{
					return Vector3.get_zero();
				}
				return ((Component)HookedBody).get_transform().TransformPoint(LocalAttachPoint);
			}
		}

		public CandidateHookPoint(Rigidbody hookedBody, StructurePart hookedStructurePart, Vector3 localAttachPoint, Vector3 localNormal)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			HookedBody = hookedBody;
			HookedStructurePart = hookedStructurePart;
			LocalAttachPoint = localAttachPoint;
			LocalNormal = localNormal;
		}
	}

	[SerializeField]
	private TethersAsset m_TethersAsset;

	[Header("Hook Ups")]
	[SerializeField]
	private EquipmentController m_EquipmentController;

	[SerializeField]
	private LineRenderer m_TetherPreview;

	[SerializeField]
	private GrapplingHook m_GrapplingHook;

	[SerializeField]
	private GunController m_GunController;

	[SerializeField]
	private GrabController m_GrabController;

	[SerializeField]
	private Transform m_GunBarrel;

	private ITethersData mData;

	private TetherState mState;

	private int mNumAvailableTethers;

	private int mMaxTethers;

	private float mLifetime;

	private bool mUnlimitedTethers;

	private CandidateHookPoint mCandidateStartAnchor;

	private CandidateHookPoint mCandidateEndAnchor;

	private CandidateHookPoint mReverseCandidateStartAnchor;

	private CandidateHookPoint mReverseCandidateEndAnchor;

	private bool mIsInitialized;

	private CandidateState mCandidateTetherState;

	private List<Tether> mActiveTethers = new List<Tether>();

	private List<Vector3> mControlPoints = new List<Vector3>(4);

	private Vector3[] mPreviewPoints;

	private Vector3[] mReversePreviewPoints;

	private bool mTetherUnlocked;

	private FXElement mStartHook;

	private bool mStartAnchorPlaced;

	private float mCurrentProximityParameter;

	private bool mTargetAudioEventTracker;

	private CandidateHookPoint ValidCandidateStartAnchor
	{
		get
		{
			if (mCandidateTetherState == CandidateState.Reverse)
			{
				return mReverseCandidateStartAnchor;
			}
			return mCandidateStartAnchor;
		}
	}

	private CandidateHookPoint ValidCandidateEndAnchor
	{
		get
		{
			if (mCandidateTetherState == CandidateState.Reverse)
			{
				return mReverseCandidateEndAnchor;
			}
			return mCandidateEndAnchor;
		}
	}

	private int BezierPoints
	{
		get
		{
			if (!((Object)(object)mData.TetherPrefab != (Object)null))
			{
				return 0;
			}
			return mData.TetherPrefab.BezierPoints;
		}
	}

	private float StartAnchorBezierFactor
	{
		get
		{
			if (!((Object)(object)mData.TetherPrefab != (Object)null))
			{
				return 1f;
			}
			return mData.TetherPrefab.StartAnchorBezierFactor;
		}
	}

	private float EndAnchorBezierFactor
	{
		get
		{
			if (!((Object)(object)mData.TetherPrefab != (Object)null))
			{
				return 1f;
			}
			return mData.TetherPrefab.EndAnchorBezierFactor;
		}
	}

	public Action<int> OnNumTethersChanged { get; set; }

	public TetherState State => mState;

	public bool IsEquipped => m_EquipmentController.CurrentEquipment == EquipmentController.Equipment.GrappleHook;

	public bool TetherUnlocked => mTetherUnlocked;

	public bool UnlimitedTethers
	{
		get
		{
			if (GlobalOptions.Raw.GetBool("General.InfTethers") && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				return mUnlimitedTethers = true;
			}
			return mUnlimitedTethers = false;
		}
	}

	public int NumAvailableTethers
	{
		get
		{
			return mNumAvailableTethers;
		}
		private set
		{
			mNumAvailableTethers = value;
			if (OnNumTethersChanged != null)
			{
				OnNumTethersChanged(mNumAvailableTethers);
			}
			if (IsCareerMode && IsProfileValid)
			{
				PlayerProfileService.Instance.Profile.Tethers = mNumAvailableTethers;
			}
		}
	}

	private bool IsCareerMode
	{
		get
		{
			if ((Object)(object)SceneLoader.Instance != (Object)null)
			{
				return SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career;
			}
			return false;
		}
	}

	private bool IsProfileValid
	{
		get
		{
			if (PlayerProfileService.Instance != null)
			{
				return PlayerProfileService.Instance.Profile != null;
			}
			return false;
		}
	}

	private void Awake()
	{
		EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
		if ((Object)(object)masterScenePlayer != (Object)null && (Object)(object)masterScenePlayer.TethersAsset != (Object)null)
		{
			m_TethersAsset = masterScenePlayer.TethersAsset;
		}
		if ((Object)(object)m_TethersAsset == (Object)null)
		{
			Log.Error(Log.Channel.Gameplay, "Tether Asset is missing.");
		}
		mData = new TethersBuffableData(m_TethersAsset.Data);
		mState = TetherState.Ready;
		mMaxTethers = mData.MaxTethers;
		mIsInitialized = false;
		if ((Object)(object)mData.TetherPrefab == (Object)null)
		{
			Log.Error(Log.Channel.Gameplay, "Tether prefab is missing.");
		}
		mPreviewPoints = (Vector3[])(object)new Vector3[BezierPoints];
		mReversePreviewPoints = (Vector3[])(object)new Vector3[BezierPoints];
		Main.EventSystem.AddHandler<GameStateChangedEvent>(OnGameStateChanged);
		Main.EventSystem.AddHandler<TetherChangedEvent>(OnTetherChanged);
		Main.EventSystem.AddHandler<RigidbodyCutEvent>(OnRigidbodyCut);
		Main.EventSystem.AddHandler<HierarchySplitCompleteEvent>(OnHierarchySplit);
		Main.EventSystem.AddHandler<ForceReleaseEvent>(OnForceRelease);
		Main.EventSystem.AddHandler<HierarchyJointReparentedEvent>(OnHierarchyJointReparented);
		Main.EventSystem.AddHandler<UnlockAbilityEvent>(OnTetherUnlocked);
		Main.EventSystem.AddHandler<PurchaseTetherEvent>(OnTetherPurchased);
		Main.EventSystem.AddHandler<EnableUnlimitedTethersEvent>(OnUnlimitedTethersEnabled);
	}

	private void OnDestroy()
	{
		Main.EventSystem.RemoveHandler<GameStateChangedEvent>(OnGameStateChanged);
		Main.EventSystem.RemoveHandler<TetherChangedEvent>(OnTetherChanged);
		Main.EventSystem.RemoveHandler<RigidbodyCutEvent>(OnRigidbodyCut);
		Main.EventSystem.RemoveHandler<HierarchySplitCompleteEvent>(OnHierarchySplit);
		Main.EventSystem.RemoveHandler<ForceReleaseEvent>(OnForceRelease);
		Main.EventSystem.RemoveHandler<HierarchyJointReparentedEvent>(OnHierarchyJointReparented);
		Main.EventSystem.RemoveHandler<PurchaseTetherEvent>(OnTetherPurchased);
		Main.EventSystem.RemoveHandler<EnableUnlimitedTethersEvent>(OnUnlimitedTethersEnabled);
	}

	private void Initialize()
	{
		mMaxTethers = mData.MaxTethers;
		if (IsCareerMode && IsProfileValid && !PlayerProfileService.Instance.Profile.PendingTetherRefill && PlayerProfileService.Instance.Profile.Tethers != -1)
		{
			NumAvailableTethers = PlayerProfileService.Instance.Profile.Tethers;
		}
		else
		{
			NumAvailableTethers = mMaxTethers;
			if (IsCareerMode && IsProfileValid && PlayerProfileService.Instance.Profile.PendingTetherRefill)
			{
				PlayerProfileService.Instance.Profile.PendingTetherRefill = false;
			}
		}
		mIsInitialized = true;
	}

	private void Update()
	{
		if (mTetherUnlocked)
		{
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode || (!PlayerProfileService.Instance.Profile.TutorialCompleted && SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career))
			{
				mMaxTethers = 1000;
				NumAvailableTethers = 1000;
				mIsInitialized = true;
			}
			else if (!mIsInitialized)
			{
				Initialize();
			}
			if (mLifetime != mData.Lifetime)
			{
				mLifetime = mData.Lifetime;
			}
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				mActiveTethers[i].Process(Time.get_deltaTime());
			}
			switch (mState)
			{
			default:
				Debug.LogError((object)("Unhandled tether state " + mState));
				break;
			case TetherState.Placing:
				HandlePlacing();
				break;
			case TetherState.Ready:
				HandleReady();
				break;
			}
			HandleHookMovement();
			HandleDistanceToTethers();
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < mActiveTethers.Count; i++)
		{
			mActiveTethers[i].LateProcess();
		}
	}

	private void SetTetherState(TetherState state)
	{
		if (state == mState)
		{
			return;
		}
		switch (state)
		{
		default:
			Debug.LogError((object)("Trying to set tether state to unhanded state " + state));
			return;
		case TetherState.Placing:
			mCandidateTetherState = CandidateState.None;
			m_GrapplingHook.CanBeUnequipped = false;
			m_TetherPreview.set_positionCount(0);
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.LaunchAnimationName);
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.ImpactAnimationName);
			break;
		case TetherState.Ready:
		{
			CandidateHookPoint validCandidateEndAnchor = ValidCandidateEndAnchor;
			if ((Object)(object)validCandidateEndAnchor.HookedStructurePart != (Object)null || (Object)(object)validCandidateEndAnchor.HookedBody != (Object)null)
			{
				GameObject val = (((Object)(object)validCandidateEndAnchor.HookedBody != (Object)null) ? ((Component)validCandidateEndAnchor.HookedBody).get_gameObject() : null);
				GameObject virtualObject = (((Object)(object)validCandidateEndAnchor.HookedStructurePart != (Object)null) ? ((Component)validCandidateEndAnchor.HookedStructurePart).get_gameObject() : null);
				if (!GrapplingHook.IsObjectStatic(val, virtualObject))
				{
					Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Stop, val, virtualObject));
				}
			}
			mCandidateTetherState = CandidateState.None;
			m_GrapplingHook.CanBeUnequipped = true;
			m_TetherPreview.set_positionCount(0);
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.ReleaseAnimationName);
			mStartAnchorPlaced = false;
			DetachHookFX();
			break;
		}
		}
		mState = state;
	}

	private void HandleReady()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		TryDespawnTether();
		if (m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook || (Object)(object)m_GrapplingHook.GrappledRigidbody != (Object)null || m_GrabController.RightHand.CurrentHandState == HandGrab.HandState.Success || !((OneAxisInputControl)LynxControls.Instance.GameplayActions.PlaceTether).get_WasPressed())
		{
			return;
		}
		m_GunController.EquipGun();
		if (NumAvailableTethers > 0)
		{
			if (TryGetTetherPoint(out var hitInfo) && (Object)(object)((RaycastHit)(ref hitInfo)).get_rigidbody() != (Object)null)
			{
				SpawnFireFX();
				mStartHook = SpawnHookFX(((RaycastHit)(ref hitInfo)).get_normal());
				mCandidateStartAnchor = new CandidateHookPoint(((RaycastHit)(ref hitInfo)).get_rigidbody(), ((Component)((RaycastHit)(ref hitInfo)).get_collider()).GetComponentInParent<StructurePart>(), ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformPoint(((RaycastHit)(ref hitInfo)).get_point()), ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformDirection(((RaycastHit)(ref hitInfo)).get_normal()));
				SetTetherState(TetherState.Placing);
				Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Placing));
			}
		}
		else
		{
			Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Unavailable));
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherUnavailableAudioEvent));
			Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(((Component)this).get_gameObject(), mData.NoTethersSpeech.Data.TriggeredSpeech));
		}
	}

	private void HandlePlacing()
	{
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.GrappleFire).get_WasPressed() || ((OneAxisInputControl)LynxControls.Instance.GameplayActions.RecallTethers).get_WasPressed() || ((OneAxisInputControl)LynxControls.Instance.GameplayActions.CancelTether).get_WasPressed())
		{
			ClearTetherState();
			return;
		}
		if (m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook)
		{
			ClearTetherState();
			return;
		}
		if ((Object)(object)m_GrapplingHook.GrappledRigidbody != (Object)null)
		{
			ClearTetherState();
			return;
		}
		if (mStartAnchorPlaced && (Object)(object)mCandidateStartAnchor.HookedBody == (Object)null)
		{
			ClearTetherState();
			return;
		}
		HandlePreviewLine();
		if (!((OneAxisInputControl)LynxControls.Instance.GameplayActions.PlaceTether).get_WasReleased())
		{
			return;
		}
		if (mCandidateTetherState != 0)
		{
			SpawnFireFX();
		}
		else
		{
			ClearTetherState();
		}
		m_TetherPreview.set_positionCount(0);
		if (mStartAnchorPlaced && mCandidateTetherState != 0)
		{
			TryCreateTether();
			DespawnHookFX();
			SetTetherState(TetherState.Ready);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherPlacedGoodAudioEvent));
			if (NumAvailableTethers == mData.LowTethersAmount)
			{
				Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(((Component)this).get_gameObject(), mData.LowTethersSpeech.Data.TriggeredSpeech));
			}
			else if (NumAvailableTethers == 0 && (Object)(object)m_TethersAsset.Data.NoTethersAction != (Object)null)
			{
				Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(m_TethersAsset.Data.NoTethersAction));
			}
			if ((Object)(object)m_TethersAsset.Data.PlaceTetherAction != (Object)null)
			{
				Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(m_TethersAsset.Data.PlaceTetherAction));
			}
		}
		else
		{
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherPlacedBadAudioEvent));
		}
	}

	private void HandlePreviewLine()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		if (!mStartAnchorPlaced)
		{
			return;
		}
		Rigidbody hookedBody = mCandidateEndAnchor.HookedBody;
		StructurePart hookedStructurePart = mCandidateEndAnchor.HookedStructurePart;
		mCandidateTetherState = CandidateState.None;
		Vector3 startPoint = mCandidateStartAnchor.WorldAttachPoint;
		Vector3 endPoint = LynxCameraController.MainCameraTransform.get_position() + LynxCameraController.MainCameraTransform.get_forward() * mData.LaunchRange;
		if (TryGetTetherPoint(out var hitInfo) && (Object)(object)((RaycastHit)(ref hitInfo)).get_rigidbody() != (Object)null)
		{
			endPoint = ((RaycastHit)(ref hitInfo)).get_point();
			ResetControlPoints(mControlPoints, startPoint, endPoint, mCandidateStartAnchor.WorldNormal * StartAnchorBezierFactor, ((RaycastHit)(ref hitInfo)).get_normal() * EndAnchorBezierFactor);
			bool flag = FindIntersections(mPreviewPoints, ref startPoint, ref endPoint, BezierPoints, mControlPoints, mData.RaycastLayerMask, out mCandidateEndAnchor);
			mCandidateTetherState = (flag ? CandidateState.Forward : CandidateState.None);
			StructurePart componentInParent = ((Component)((RaycastHit)(ref hitInfo)).get_collider()).GetComponentInParent<StructurePart>();
			if (!flag)
			{
				mCandidateTetherState = CandidateState.Forward;
				mCandidateEndAnchor = new CandidateHookPoint(((RaycastHit)(ref hitInfo)).get_rigidbody(), componentInParent, ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformPoint(((RaycastHit)(ref hitInfo)).get_point()), ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformDirection(((RaycastHit)(ref hitInfo)).get_normal()));
			}
			else if ((Object)(object)mCandidateEndAnchor.HookedBody == (Object)(object)mCandidateStartAnchor.HookedBody)
			{
				bool num = (Object)(object)((RaycastHit)(ref hitInfo)).get_rigidbody() != (Object)(object)mCandidateEndAnchor.HookedBody;
				bool flag2 = !num && (Object)(object)componentInParent != (Object)(object)mCandidateEndAnchor.HookedStructurePart;
				if (num || flag2)
				{
					Vector3 startPoint2 = ((RaycastHit)(ref hitInfo)).get_point();
					Vector3 endPoint2 = startPoint;
					ResetControlPoints(mControlPoints, startPoint2, endPoint2, ((RaycastHit)(ref hitInfo)).get_normal() * StartAnchorBezierFactor, mCandidateStartAnchor.WorldNormal * EndAnchorBezierFactor);
					bool num2 = FindIntersections(mReversePreviewPoints, ref startPoint2, ref endPoint2, BezierPoints, mControlPoints, mData.RaycastLayerMask, out mReverseCandidateStartAnchor);
					bool flag3 = false;
					bool flag4 = false;
					bool flag5 = false;
					if (num2)
					{
						flag3 = (Object)(object)mReverseCandidateStartAnchor.HookedBody == (Object)(object)mCandidateStartAnchor.HookedBody;
						if (flag3)
						{
							flag4 = (Object)(object)mReverseCandidateStartAnchor.HookedStructurePart != (Object)null && (Object)(object)mReverseCandidateStartAnchor.HookedStructurePart == (Object)(object)mCandidateStartAnchor.HookedStructurePart;
							if (!flag4 && (Object)(object)mReverseCandidateStartAnchor.HookedStructurePart != (Object)null)
							{
								StructureGroup group = mReverseCandidateStartAnchor.HookedStructurePart.Group;
								flag5 = (Object)(object)group != (Object)null && (Object)(object)group == (Object)(object)mCandidateStartAnchor.HookedStructurePart.Group;
							}
						}
					}
					if (num2 && flag3 && (!flag2 || flag4 || flag5))
					{
						mReverseCandidateEndAnchor = new CandidateHookPoint(((RaycastHit)(ref hitInfo)).get_rigidbody(), componentInParent, ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformPoint(((RaycastHit)(ref hitInfo)).get_point()), ((Component)((RaycastHit)(ref hitInfo)).get_rigidbody()).get_transform().InverseTransformDirection(((RaycastHit)(ref hitInfo)).get_normal()));
						startPoint = startPoint2;
						endPoint = endPoint2;
						mCandidateTetherState = CandidateState.Reverse;
					}
					else
					{
						mReverseCandidateStartAnchor = default(CandidateHookPoint);
						ResetPreviewPoints(mReversePreviewPoints, startPoint2, endPoint2, BezierPoints);
					}
				}
			}
		}
		bool flag6 = mCandidateTetherState > CandidateState.None;
		if (!flag6)
		{
			mCandidateEndAnchor = default(CandidateHookPoint);
			ResetPreviewPoints(mPreviewPoints, startPoint, endPoint, BezierPoints);
		}
		((Renderer)m_TetherPreview).set_material((!flag6) ? mData.TetherUnacceptableMaterial : mData.TetherAcceptableMaterial);
		m_TetherPreview.set_positionCount(BezierPoints);
		Vector3[] positions = ((mCandidateTetherState == CandidateState.Reverse) ? mReversePreviewPoints : mPreviewPoints);
		m_TetherPreview.SetPositions(positions);
		if (flag6 && !mTargetAudioEventTracker)
		{
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherTargetGoodAudioEvent));
			mTargetAudioEventTracker = true;
		}
		else if (!flag6 && mTargetAudioEventTracker)
		{
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherTargetBadAudioEvent));
			mTargetAudioEventTracker = false;
		}
		CandidateHookPoint validCandidateEndAnchor = ValidCandidateEndAnchor;
		if (((Object)(object)hookedStructurePart != (Object)null && (Object)(object)hookedStructurePart != (Object)(object)validCandidateEndAnchor.HookedStructurePart) || ((Object)(object)hookedBody != (Object)null && (Object)(object)hookedBody != (Object)(object)validCandidateEndAnchor.HookedBody))
		{
			GameObject val = (((Object)(object)hookedBody != (Object)null) ? ((Component)hookedBody).get_gameObject() : null);
			GameObject virtualObject = (((Object)(object)hookedStructurePart != (Object)null) ? ((Component)hookedStructurePart).get_gameObject() : null);
			if (!GrapplingHook.IsObjectStatic(val, virtualObject))
			{
				Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Stop, val, virtualObject));
			}
		}
		if (((Object)(object)validCandidateEndAnchor.HookedStructurePart != (Object)null && (Object)(object)validCandidateEndAnchor.HookedStructurePart != (Object)(object)hookedStructurePart) || ((Object)(object)validCandidateEndAnchor.HookedBody != (Object)null && (Object)(object)validCandidateEndAnchor.HookedBody != (Object)(object)hookedBody))
		{
			GameObject val2 = (((Object)(object)validCandidateEndAnchor.HookedBody != (Object)null) ? ((Component)validCandidateEndAnchor.HookedBody).get_gameObject() : null);
			GameObject virtualObject2 = (((Object)(object)validCandidateEndAnchor.HookedStructurePart != (Object)null) ? ((Component)validCandidateEndAnchor.HookedStructurePart).get_gameObject() : null);
			if (!GrapplingHook.IsObjectStatic(val2, virtualObject2))
			{
				Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Start, val2, virtualObject2));
			}
		}
	}

	private static bool FindIntersections(Vector3[] previewPoints, ref Vector3 startPoint, ref Vector3 endPoint, int bezierPoints, List<Vector3> controlPoints, LayerMask raycastLayerMask, out CandidateHookPoint candidateHookPoint)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		candidateHookPoint = default(CandidateHookPoint);
		previewPoints[0] = startPoint;
		RaycastHit val2 = default(RaycastHit);
		for (int i = 1; i < bezierPoints - 1; i++)
		{
			if (!flag)
			{
				Vector3 val = (previewPoints[i] = BezierCurve.GetCubicPoint(controlPoints, (float)i / (float)bezierPoints));
				if (Physics.Linecast(previewPoints[i - 1], val, ref val2, LayerMask.op_Implicit(raycastLayerMask)) && (Object)(object)((RaycastHit)(ref val2)).get_rigidbody() != (Object)null)
				{
					endPoint = ((RaycastHit)(ref val2)).get_point();
					flag = true;
					candidateHookPoint = new CandidateHookPoint(((RaycastHit)(ref val2)).get_rigidbody(), ((Component)((RaycastHit)(ref val2)).get_collider()).GetComponent<StructurePart>(), ((Component)((RaycastHit)(ref val2)).get_rigidbody()).get_transform().InverseTransformPoint(((RaycastHit)(ref val2)).get_point()), ((Component)((RaycastHit)(ref val2)).get_rigidbody()).get_transform().InverseTransformDirection(((RaycastHit)(ref val2)).get_normal()));
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

	private static void ResetControlPoints(List<Vector3> controlPoints, Vector3 startPoint, Vector3 endPoint, Vector3 perpendicularStartVector, Vector3 perpendicularEndVector)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		controlPoints.Clear();
		Vector3 item = startPoint + perpendicularStartVector;
		Vector3 item2 = endPoint + perpendicularEndVector;
		controlPoints.Add(startPoint);
		controlPoints.Add(item);
		controlPoints.Add(item2);
		controlPoints.Add(endPoint);
	}

	private static void ResetPreviewPoints(Vector3[] previewPoints, Vector3 startPoint, Vector3 endPoint, int bezierPoints)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		previewPoints[0] = startPoint;
		for (int i = 1; i < bezierPoints; i++)
		{
			previewPoints[i] = endPoint;
		}
	}

	private void HandleHookMovement()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)mStartHook != (Object)null))
		{
			return;
		}
		if (mStartAnchorPlaced)
		{
			CandidateHookPoint validCandidateStartAnchor = ValidCandidateStartAnchor;
			((Component)mStartHook).get_transform().set_position(validCandidateStartAnchor.WorldAttachPoint);
			((Component)mStartHook).get_transform().set_rotation(Quaternion.LookRotation(validCandidateStartAnchor.WorldNormal));
			return;
		}
		CandidateHookPoint candidateHookPoint = mCandidateStartAnchor;
		if (Vector3.Distance(((Component)mStartHook).get_transform().get_position(), candidateHookPoint.WorldAttachPoint) <= mData.HookSpeed * Time.get_deltaTime())
		{
			mStartAnchorPlaced = true;
			SpawnImpactFX(candidateHookPoint.WorldAttachPoint, candidateHookPoint.WorldNormal);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.StartHookPlacedAudioEvent));
		}
		else
		{
			Vector3 val = candidateHookPoint.WorldAttachPoint - ((Component)mStartHook).get_transform().get_position();
			Vector3 normalized = ((Vector3)(ref val)).get_normalized();
			Transform transform = ((Component)mStartHook).get_transform();
			transform.set_position(transform.get_position() + normalized * mData.HookSpeed * Time.get_deltaTime());
		}
	}

	private void HandleDistanceToTethers()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_TethersAsset.Data.TetherAudioProximityAsset == (Object)null)
		{
			return;
		}
		AudioProximityData data = m_TethersAsset.Data.TetherAudioProximityAsset.Data;
		if (data == null)
		{
			return;
		}
		WwiseShortID parameterID = data.ParameterID;
		if (!((WwiseShortID)(ref parameterID)).get_IsValid())
		{
			return;
		}
		float newValue = 0f;
		float glitchAmount = 0f;
		if (mActiveTethers.Count > 0)
		{
			Vector3 position = LynxCameraController.MainCameraTransform.get_position();
			float num = float.MaxValue;
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				Tether tether = mActiveTethers[i];
				num = Mathf.Min(num, MathUtility.DistanceFromPointToSegment(float3.op_Implicit(tether.WorldStartAttachPoint), float3.op_Implicit(tether.WorldEndAttachPoint), float3.op_Implicit(position)));
			}
			float num2 = 1f - Mathf.Clamp01(num / data.Range);
			float num3 = data.ProximityRamp.Evaluate(num2);
			newValue = num3 * data.MaxValue;
			glitchAmount = num3 * data.MaxVisualGlitchAmount;
		}
		if (AudioParameterController.HasParameterChanged(mCurrentProximityParameter, newValue))
		{
			mCurrentProximityParameter = newValue;
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(data.ParameterID, mCurrentProximityParameter));
			Main.EventSystem.Post(ProximityDistortionParamaterChangedEvent.GetEvent(((Component)this).get_gameObject(), glitchAmount));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(mData.TetherSoundVolRTPC, mCurrentProximityParameter));
		}
	}

	private void TryCreateTether()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		CandidateHookPoint validCandidateStartAnchor = ValidCandidateStartAnchor;
		CandidateHookPoint validCandidateEndAnchor = ValidCandidateEndAnchor;
		if ((Object)(object)mData.TetherPrefab != (Object)null && (Object)(object)validCandidateStartAnchor.HookedBody != (Object)null && (Object)(object)validCandidateEndAnchor.HookedBody != (Object)null)
		{
			Tether component = GameSession.SpawnPoolManager.SpawnObject(((Component)mData.TetherPrefab).get_gameObject(), Vector3.get_zero(), Quaternion.get_identity()).GetComponent<Tether>();
			if ((Object)(object)component != (Object)null)
			{
				component.SpawnRope(validCandidateStartAnchor.HookedBody, validCandidateEndAnchor.HookedBody, validCandidateStartAnchor.HookedStructurePart, validCandidateEndAnchor.HookedStructurePart, validCandidateStartAnchor.WorldAttachPoint, validCandidateEndAnchor.WorldAttachPoint, startManipulator: false, endManipulator: false, mData.RawTetheredMassRange, validCandidateStartAnchor.WorldNormal, validCandidateEndAnchor.WorldNormal, mData.MaxRopeLength, mData.ForceCurve, mData.RetractSpeedCurve, mLifetime);
				if ((Object)(object)validCandidateStartAnchor.HookedBody != (Object)(object)validCandidateEndAnchor.HookedBody)
				{
					Rigidbody hookedBody = validCandidateStartAnchor.HookedBody;
					hookedBody.set_velocity(hookedBody.get_velocity() * mData.VelocityDampening);
					Rigidbody hookedBody2 = validCandidateStartAnchor.HookedBody;
					hookedBody2.set_angularVelocity(hookedBody2.get_angularVelocity() * mData.TorqueDampening);
					Rigidbody hookedBody3 = validCandidateEndAnchor.HookedBody;
					hookedBody3.set_velocity(hookedBody3.get_velocity() * mData.VelocityDampening);
					Rigidbody hookedBody4 = validCandidateEndAnchor.HookedBody;
					hookedBody4.set_angularVelocity(hookedBody4.get_angularVelocity() * mData.TorqueDampening);
				}
				component.OnTetherDespawned = (Action<Tether>)Delegate.Combine(component.OnTetherDespawned, new Action<Tether>(OnTetherDespawned));
				mActiveTethers.Add(component);
				if (!mUnlimitedTethers && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.FreeMode)
				{
					NumAvailableTethers--;
				}
			}
		}
		else
		{
			ClearTetherState();
		}
	}

	private void TryDespawnTether()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (mActiveTethers.Count <= 0 || !((OneAxisInputControl)LynxControls.Instance.GameplayActions.RecallTethers).get_WasPressed())
		{
			return;
		}
		for (int num = mActiveTethers.Count - 1; num >= 0; num--)
		{
			Tether tether = mActiveTethers[num];
			if ((Object)(object)tether != (Object)null && ((Component)tether).get_gameObject().get_activeInHierarchy())
			{
				Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Recalled));
				tether.DespawnRope();
			}
		}
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherRecalledAudioEvent));
	}

	private void ClearTetherState()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		DetachHookFX();
		Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Failed));
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherLoopStopperAudioEvent));
		mTargetAudioEventTracker = false;
		SetTetherState(TetherState.Ready);
	}

	private void SpawnFireFX()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mData.FireFXPrefab != (Object)null)
		{
			((Component)GameSession.SpawnPoolManager.SpawnObject<FXElement>(mData.FireFXPrefab, m_GunBarrel.get_position(), m_GunBarrel.get_rotation(), (Transform)null, (object)null)).get_transform().SetParent(m_GunBarrel);
		}
	}

	private void SpawnImpactFX(Vector3 position, Vector3 normal)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mData.ImpactFXPrefab != (Object)null)
		{
			GameSession.SpawnPoolManager.SpawnObject<FXElement>(mData.ImpactFXPrefab, position, Quaternion.LookRotation(normal), (Transform)null, (object)null);
		}
	}

	private FXElement SpawnHookFX(Vector3 normal)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.HookFiredAudioEvent));
		if ((Object)(object)mData.HookFX != (Object)null)
		{
			return GameSession.SpawnPoolManager.SpawnObject<FXElement>(mData.HookFX, ((Component)m_GunBarrel).get_transform().get_position(), Quaternion.LookRotation(normal), (Transform)null, (object)null);
		}
		return null;
	}

	private void DespawnHookFX()
	{
		if ((Object)(object)mStartHook != (Object)null)
		{
			GameSession.SpawnPoolManager.DespawnObject<FXElement>(mStartHook, (object)null);
			mStartHook = null;
		}
	}

	private void DetachHookFX()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mStartHook != (Object)null)
		{
			SpawnImpactFX(((Component)mStartHook).get_transform().get_position(), ValidCandidateStartAnchor.WorldNormal);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherHookDetachedAudioEvent));
			mStartHook.DetachElement();
			mStartHook = null;
		}
	}

	private void OnGameStateChanged(GameStateChangedEvent ev)
	{
		if (State == TetherState.Placing)
		{
			ClearTetherState();
		}
		if (ev.GameState == GameSession.GameState.Gameplay && ev.PrevGameState == GameSession.GameState.Hab)
		{
			mIsInitialized = false;
		}
	}

	private void OnTetherDespawned(Tether tether)
	{
		if (mActiveTethers.Remove(tether))
		{
			tether.OnTetherDespawned = (Action<Tether>)Delegate.Remove(tether.OnTetherDespawned, new Action<Tether>(OnTetherDespawned));
		}
		else
		{
			Debug.LogError((object)"Trying to remove tether that was never added.", (Object)(object)((Component)tether).get_gameObject());
		}
	}

	private void OnTetherChanged(TetherChangedEvent ev)
	{
		if (ev.State == TetherChangedEvent.TetherState.AddTethers)
		{
			NumAvailableTethers += ev.NumTethers;
			NumAvailableTethers = math.min(NumAvailableTethers, mData.MaxTethers);
			Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(((Component)this).get_gameObject(), mData.AddTethersSpeech.Data.TriggeredSpeech));
		}
	}

	public void EquipmentChanged()
	{
		ClearTetherState();
	}

	private void OnRigidbodyCut(RigidbodyCutEvent ev)
	{
		if (ev.Children.Length != 0)
		{
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				mActiveTethers[i].OnRigidbodyCut(ev);
			}
		}
	}

	private void OnHierarchySplit(HierarchySplitCompleteEvent ev)
	{
		if (ev.NewParts.Length != 0)
		{
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				mActiveTethers[i].OnHierarchySplit(ev);
			}
		}
	}

	private void OnHierarchyJointReparented(HierarchyJointReparentedEvent ev)
	{
		for (int i = 0; i < mActiveTethers.Count; i++)
		{
			mActiveTethers[i].OnHierarchyJointReparented(ev);
		}
	}

	private void OnForceRelease(ForceReleaseEvent ev)
	{
		if (ev.State == ForceReleaseEvent.ReleaseState.All || ev.State == ForceReleaseEvent.ReleaseState.Tether)
		{
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				mActiveTethers[i].OnForceRelease(ev);
			}
		}
	}

	private void OnTetherUnlocked(UnlockAbilityEvent ev)
	{
		if (ev.Ability == UnlockAbilityID.GrappleTethers)
		{
			mTetherUnlocked = true;
		}
	}

	private void OnTetherPurchased(PurchaseTetherEvent eventReceived)
	{
		bool flag = NumAvailableTethers < mMaxTethers;
		if (flag)
		{
			Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.AddTethers, eventReceived.NumTethers));
		}
		eventReceived.Callback?.Invoke(flag);
	}

	private void OnUnlimitedTethersEnabled(EnableUnlimitedTethersEvent ev)
	{
		mUnlimitedTethers = ev.EnableUnlimitedTethers;
	}

	private bool TryGetTetherPoint(out RaycastHit hitInfo)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		if (Physics.Raycast(new Ray(LynxCameraController.MainCameraTransform.get_position(), LynxCameraController.MainCameraTransform.get_forward()), ref hitInfo, mData.LaunchRange, LayerMask.op_Implicit(mData.RaycastLayerMask)) && IsValidHookable(hitInfo))
		{
			return true;
		}
		bool flag = false;
		Vector2 val = Vector2.get_zero();
		Vector2 zero = Vector2.get_zero();
		RaycastHit val2 = default(RaycastHit);
		RaycastHit val3 = default(RaycastHit);
		for (int i = 0; i < mData.TetherScreenRaysPerRow; i++)
		{
			for (int j = 0; j < mData.TetherScreenRaysPerColumn; j++)
			{
				float num = (float)LynxCameraController.ScreenWidth * (0.5f - mData.TetherScreenWidth / 2f + mData.TetherScreenWidth * ((float)i / (float)(mData.TetherScreenRaysPerRow - 1)));
				float num2 = (float)LynxCameraController.ScreenHeight * (0.5f - mData.TetherScreenHeight / 2f + mData.TetherScreenHeight * ((float)j / (float)(mData.TetherScreenRaysPerColumn - 1)));
				if (Physics.Raycast(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(num, num2)), ref val3, mData.LaunchRange, LayerMask.op_Implicit(mData.RaycastLayerMask)) && IsValidHookable(val3))
				{
					zero = Vector2.op_Implicit(LynxCameraController.MainCamera.WorldToViewportPoint(((RaycastHit)(ref val3)).get_point()));
					float num3 = GameObjectHelper.DistanceSquaredFromCentreScreen(zero);
					float num4 = GameObjectHelper.DistanceSquaredFromCentreScreen(val);
					if (!flag || num3 < num4)
					{
						val = zero;
						val2 = val3;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			hitInfo = val2;
			return true;
		}
		return false;
	}

	private bool IsValidHookable(RaycastHit hit)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		int num = 1 << ((Component)((RaycastHit)(ref hit)).get_collider()).get_gameObject().get_layer();
		LayerMask raycastLayerMask = mData.RaycastLayerMask;
		return (num & ((LayerMask)(ref raycastLayerMask)).get_value()) > 0;
	}

	public TetherController()
		: this()
	{
	}
}
