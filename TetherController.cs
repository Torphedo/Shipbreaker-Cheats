using System;
using System.Collections.Generic;
using BBI;
using BBI.Unity.Game;
using Carbon.Core;
using Carbon.Core.Unity;
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
				if (!(HookedBody != null))
				{
					return Vector3.zero;
				}
				return HookedBody.transform.TransformDirection(LocalNormal);
			}
		}

		public Vector3 WorldAttachPoint
		{
			get
			{
				if (!(HookedBody != null))
				{
					return Vector3.zero;
				}
				return HookedBody.transform.TransformPoint(LocalAttachPoint);
			}
		}

		public CandidateHookPoint(Rigidbody hookedBody, StructurePart hookedStructurePart, Vector3 localAttachPoint, Vector3 localNormal)
		{
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
			if (!(mData.TetherPrefab != null))
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
			if (!(mData.TetherPrefab != null))
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
			if (!(mData.TetherPrefab != null))
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
		}
	}

	private void Awake()
	{
		EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
		if (masterScenePlayer != null && masterScenePlayer.TethersAsset != null)
		{
			m_TethersAsset = masterScenePlayer.TethersAsset;
		}
		if (m_TethersAsset == null)
		{
			Log.Error(Log.Channel.Gameplay, "Tether Asset is missing.");
		}
		mData = new TethersBuffableData(m_TethersAsset.Data);
		mState = TetherState.Ready;
		NumAvailableTethers = mData.StartingTethers;
		if (mData.TetherPrefab == null)
		{
			Log.Error(Log.Channel.Gameplay, "Tether prefab is missing.");
		}
		mPreviewPoints = new Vector3[BezierPoints];
		mReversePreviewPoints = new Vector3[BezierPoints];
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

	private void Update()
	{
		if (mTetherUnlocked)
		{
			if (SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode || (!PlayerProfileService.Instance.Profile.TutorialCompleted && SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.Career))
			{
				mMaxTethers = 1000;
				NumAvailableTethers = 1000;
			}
			else if (mMaxTethers != mData.MaxTethers)
			{
				mMaxTethers = mData.MaxTethers;
				NumAvailableTethers = mData.MaxTethers;
			}
			if (mLifetime != mData.Lifetime)
			{
				mLifetime = mData.Lifetime;
			}
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				mActiveTethers[i].Process(Time.deltaTime);
			}
			switch (mState)
			{
			case TetherState.Ready:
				HandleReady();
				break;
			case TetherState.Placing:
				HandlePlacing();
				break;
			default:
				Debug.LogError("Unhandled tether state " + mState);
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
		case TetherState.Ready:
		{
			CandidateHookPoint validCandidateEndAnchor = ValidCandidateEndAnchor;
			if (validCandidateEndAnchor.HookedStructurePart != null || validCandidateEndAnchor.HookedBody != null)
			{
				GameObject gameObject = ((validCandidateEndAnchor.HookedBody != null) ? validCandidateEndAnchor.HookedBody.gameObject : null);
				GameObject virtualObject = ((validCandidateEndAnchor.HookedStructurePart != null) ? validCandidateEndAnchor.HookedStructurePart.gameObject : null);
				if (!GrapplingHook.IsObjectStatic(gameObject, virtualObject))
				{
					Main.EventSystem.Post(ObjectHighlightEvent.Tether(ObjectHighlightEvent.HighlightState.Stop, gameObject, virtualObject));
				}
			}
			mCandidateTetherState = CandidateState.None;
			m_GrapplingHook.CanBeUnequipped = true;
			m_TetherPreview.positionCount = 0;
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.ReleaseAnimationName);
			mStartAnchorPlaced = false;
			DetachHookFX();
			break;
		}
		case TetherState.Placing:
			mCandidateTetherState = CandidateState.None;
			m_GrapplingHook.CanBeUnequipped = false;
			m_TetherPreview.positionCount = 0;
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.LaunchAnimationName);
			m_GrapplingHook.TriggerGrappleAnimation(m_GrapplingHook.ImpactAnimationName);
			break;
		default:
			Debug.LogError("Trying to set tether state to unhanded state " + state);
			return;
		}
		mState = state;
	}

	private void HandleReady()
	{
		TryDespawnTether();
		if (m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook || m_GrapplingHook.GrappledRigidbody != null || m_GrabController.RightHand.CurrentHandState == HandGrab.HandState.Success || !LynxControls.Instance.GameplayActions.PlaceTether.WasPressed)
		{
			return;
		}
		m_GunController.EquipGun();
		if (NumAvailableTethers > 0)
		{
			if (TryGetTetherPoint(out var hitInfo) && hitInfo.rigidbody != null)
			{
				SpawnFireFX();
				mStartHook = SpawnHookFX(hitInfo.normal);
				mCandidateStartAnchor = new CandidateHookPoint(hitInfo.rigidbody, hitInfo.collider.GetComponentInParent<StructurePart>(), hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point), hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal));
				SetTetherState(TetherState.Placing);
				Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Placing));
			}
		}
		else
		{
			Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Unavailable));
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherUnavailableAudioEvent));
			Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, mData.NoTethersSpeech.Data.TriggeredSpeech));
		}
	}

	private void HandlePlacing()
	{
		if (LynxControls.Instance.GameplayActions.GrappleFire.WasPressed || LynxControls.Instance.GameplayActions.RecallTethers.WasPressed || LynxControls.Instance.GameplayActions.CancelTether.WasPressed)
		{
			ClearTetherState();
			return;
		}
		if (m_EquipmentController.CurrentEquipment != EquipmentController.Equipment.GrappleHook)
		{
			ClearTetherState();
			return;
		}
		if (m_GrapplingHook.GrappledRigidbody != null)
		{
			ClearTetherState();
			return;
		}
		if (mStartAnchorPlaced && mCandidateStartAnchor.HookedBody == null)
		{
			ClearTetherState();
			return;
		}
		HandlePreviewLine();
		if (!LynxControls.Instance.GameplayActions.PlaceTether.WasReleased)
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
		m_TetherPreview.positionCount = 0;
		if (mStartAnchorPlaced && mCandidateTetherState != 0)
		{
			TryCreateTether();
			DespawnHookFX();
			SetTetherState(TetherState.Ready);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherPlacedGoodAudioEvent));
			if (NumAvailableTethers == mData.LowTethersAmount)
			{
				Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, mData.LowTethersSpeech.Data.TriggeredSpeech));
			}
			else if (NumAvailableTethers == 0 && m_TethersAsset.Data.NoTethersAction != null)
			{
				Main.EventSystem.Post(PlayerActionTrackerEvent.GetEvent(m_TethersAsset.Data.NoTethersAction));
			}
			if (m_TethersAsset.Data.PlaceTetherAction != null)
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
		if (!mStartAnchorPlaced)
		{
			return;
		}
		Rigidbody hookedBody = mCandidateEndAnchor.HookedBody;
		StructurePart hookedStructurePart = mCandidateEndAnchor.HookedStructurePart;
		mCandidateTetherState = CandidateState.None;
		Vector3 startPoint = mCandidateStartAnchor.WorldAttachPoint;
		Vector3 endPoint = LynxCameraController.MainCameraTransform.position + LynxCameraController.MainCameraTransform.forward * mData.LaunchRange;
		if (TryGetTetherPoint(out var hitInfo) && hitInfo.rigidbody != null)
		{
			endPoint = hitInfo.point;
			ResetControlPoints(mControlPoints, startPoint, endPoint, mCandidateStartAnchor.WorldNormal * StartAnchorBezierFactor, hitInfo.normal * EndAnchorBezierFactor);
			bool flag = FindIntersections(mPreviewPoints, ref startPoint, ref endPoint, BezierPoints, mControlPoints, mData.RaycastLayerMask, out mCandidateEndAnchor);
			mCandidateTetherState = (flag ? CandidateState.Forward : CandidateState.None);
			StructurePart componentInParent = hitInfo.collider.GetComponentInParent<StructurePart>();
			if (!flag)
			{
				flag = true;
				mCandidateTetherState = CandidateState.Forward;
				mCandidateEndAnchor = new CandidateHookPoint(hitInfo.rigidbody, componentInParent, hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point), hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal));
			}
			else if (mCandidateEndAnchor.HookedBody == mCandidateStartAnchor.HookedBody)
			{
				bool num = hitInfo.rigidbody != mCandidateEndAnchor.HookedBody;
				bool flag2 = !num && componentInParent != mCandidateEndAnchor.HookedStructurePart;
				if (num || flag2)
				{
					Vector3 startPoint2 = hitInfo.point;
					Vector3 endPoint2 = startPoint;
					ResetControlPoints(mControlPoints, startPoint2, endPoint2, hitInfo.normal * StartAnchorBezierFactor, mCandidateStartAnchor.WorldNormal * EndAnchorBezierFactor);
					bool num2 = FindIntersections(mReversePreviewPoints, ref startPoint2, ref endPoint2, BezierPoints, mControlPoints, mData.RaycastLayerMask, out mReverseCandidateStartAnchor);
					bool flag3 = false;
					bool flag4 = false;
					bool flag5 = false;
					if (num2)
					{
						flag3 = mReverseCandidateStartAnchor.HookedBody == mCandidateStartAnchor.HookedBody;
						if (flag3)
						{
							flag4 = mReverseCandidateStartAnchor.HookedStructurePart != null && mReverseCandidateStartAnchor.HookedStructurePart == mCandidateStartAnchor.HookedStructurePart;
							if (!flag4 && mReverseCandidateStartAnchor.HookedStructurePart != null)
							{
								StructureGroup group = mReverseCandidateStartAnchor.HookedStructurePart.Group;
								flag5 = group != null && group == mCandidateStartAnchor.HookedStructurePart.Group;
							}
						}
					}
					if (num2 && flag3 && (!flag2 || flag4 || flag5))
					{
						mReverseCandidateEndAnchor = new CandidateHookPoint(hitInfo.rigidbody, componentInParent, hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point), hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal));
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
		bool flag6 = mCandidateTetherState != CandidateState.None;
		if (!flag6)
		{
			mCandidateEndAnchor = default(CandidateHookPoint);
			ResetPreviewPoints(mPreviewPoints, startPoint, endPoint, BezierPoints);
		}
		m_TetherPreview.material = ((!flag6) ? mData.TetherUnacceptableMaterial : mData.TetherAcceptableMaterial);
		m_TetherPreview.positionCount = BezierPoints;
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

	private static bool FindIntersections(Vector3[] previewPoints, ref Vector3 startPoint, ref Vector3 endPoint, int bezierPoints, List<Vector3> controlPoints, LayerMask raycastLayerMask, out CandidateHookPoint candidateHookPoint)
	{
		bool flag = false;
		candidateHookPoint = default(CandidateHookPoint);
		previewPoints[0] = startPoint;
		for (int i = 1; i < bezierPoints - 1; i++)
		{
			if (!flag)
			{
				if (Physics.Linecast(end: previewPoints[i] = BezierCurve.GetCubicPoint(controlPoints, (float)i / (float)bezierPoints), start: previewPoints[i - 1], hitInfo: out var hitInfo, layerMask: raycastLayerMask) && hitInfo.rigidbody != null)
				{
					endPoint = hitInfo.point;
					flag = true;
					candidateHookPoint = new CandidateHookPoint(hitInfo.rigidbody, hitInfo.collider.GetComponent<StructurePart>(), hitInfo.rigidbody.transform.InverseTransformPoint(hitInfo.point), hitInfo.rigidbody.transform.InverseTransformDirection(hitInfo.normal));
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
		previewPoints[0] = startPoint;
		for (int i = 1; i < bezierPoints; i++)
		{
			previewPoints[i] = endPoint;
		}
	}

	private void HandleHookMovement()
	{
		if (!(mStartHook != null))
		{
			return;
		}
		if (mStartAnchorPlaced)
		{
			CandidateHookPoint validCandidateStartAnchor = ValidCandidateStartAnchor;
			mStartHook.transform.position = validCandidateStartAnchor.WorldAttachPoint;
			mStartHook.transform.rotation = Quaternion.LookRotation(validCandidateStartAnchor.WorldNormal);
			return;
		}
		CandidateHookPoint candidateHookPoint = mCandidateStartAnchor;
		if (Vector3.Distance(mStartHook.transform.position, candidateHookPoint.WorldAttachPoint) <= mData.HookSpeed * Time.deltaTime)
		{
			mStartAnchorPlaced = true;
			SpawnImpactFX(candidateHookPoint.WorldAttachPoint, candidateHookPoint.WorldNormal);
			Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.StartHookPlacedAudioEvent));
		}
		else
		{
			Vector3 normalized = (candidateHookPoint.WorldAttachPoint - mStartHook.transform.position).normalized;
			mStartHook.transform.position += normalized * mData.HookSpeed * Time.deltaTime;
		}
	}

	private void HandleDistanceToTethers()
	{
		if (m_TethersAsset.Data.TetherAudioProximityAsset == null)
		{
			return;
		}
		AudioProximityData data = m_TethersAsset.Data.TetherAudioProximityAsset.Data;
		if (data == null || !data.ParameterID.IsValid)
		{
			return;
		}
		float newValue = 0f;
		float glitchAmount = 0f;
		if (mActiveTethers.Count > 0)
		{
			Vector3 position = LynxCameraController.MainCameraTransform.position;
			float num = float.MaxValue;
			for (int i = 0; i < mActiveTethers.Count; i++)
			{
				Tether tether = mActiveTethers[i];
				num = Mathf.Min(num, MathUtility.DistanceFromPointToSegment(tether.WorldStartAttachPoint, tether.WorldEndAttachPoint, position));
			}
			float time = 1f - Mathf.Clamp01(num / data.Range);
			float num2 = data.ProximityRamp.Evaluate(time);
			newValue = num2 * data.MaxValue;
			glitchAmount = num2 * data.MaxVisualGlitchAmount;
		}
		if (AudioParameterController.HasParameterChanged(mCurrentProximityParameter, newValue))
		{
			mCurrentProximityParameter = newValue;
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(data.ParameterID, mCurrentProximityParameter));
			Main.EventSystem.Post(ProximityDistortionParamaterChangedEvent.GetEvent(base.gameObject, glitchAmount));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(mData.TetherSoundVolRTPC, mCurrentProximityParameter));
		}
	}

	private void TryCreateTether()
	{
		CandidateHookPoint validCandidateStartAnchor = ValidCandidateStartAnchor;
		CandidateHookPoint validCandidateEndAnchor = ValidCandidateEndAnchor;
		if (mData.TetherPrefab != null && validCandidateStartAnchor.HookedBody != null && validCandidateEndAnchor.HookedBody != null)
		{
			Tether component = GameSession.SpawnPoolManager.SpawnObject(mData.TetherPrefab.gameObject, Vector3.zero, Quaternion.identity).GetComponent<Tether>();
			if (component != null)
			{
				component.SpawnRope(validCandidateStartAnchor.HookedBody, validCandidateEndAnchor.HookedBody, validCandidateStartAnchor.HookedStructurePart, validCandidateEndAnchor.HookedStructurePart, validCandidateStartAnchor.WorldAttachPoint, validCandidateEndAnchor.WorldAttachPoint, startManipulator: false, endManipulator: false, mData.RawTetheredMassRange, validCandidateStartAnchor.WorldNormal, validCandidateEndAnchor.WorldNormal, mData.MaxRopeLength, mData.ForceCurve, mData.RetractSpeedCurve, mLifetime);
				if (validCandidateStartAnchor.HookedBody != validCandidateEndAnchor.HookedBody)
				{
					validCandidateStartAnchor.HookedBody.velocity *= mData.VelocityDampening;
					validCandidateStartAnchor.HookedBody.angularVelocity *= mData.TorqueDampening;
					validCandidateEndAnchor.HookedBody.velocity *= mData.VelocityDampening;
					validCandidateEndAnchor.HookedBody.angularVelocity *= mData.TorqueDampening;
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
		if (mActiveTethers.Count <= 0 || !LynxControls.Instance.GameplayActions.RecallTethers.WasPressed)
		{
			return;
		}
		for (int num = mActiveTethers.Count - 1; num >= 0; num--)
		{
			Tether tether = mActiveTethers[num];
			if (tether != null && tether.gameObject.activeInHierarchy)
			{
				Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Recalled));
				tether.DespawnRope();
			}
		}
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherRecalledAudioEvent));
	}

	private void ClearTetherState()
	{
		DetachHookFX();
		Main.EventSystem.Post(TetherChangedEvent.GetEvent(TetherChangedEvent.TetherState.Failed));
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.TetherLoopStopperAudioEvent));
		mTargetAudioEventTracker = false;
		SetTetherState(TetherState.Ready);
	}

	private void SpawnFireFX()
	{
		if (mData.FireFXPrefab != null)
		{
			GameSession.SpawnPoolManager.SpawnObject(mData.FireFXPrefab, m_GunBarrel.position, m_GunBarrel.rotation).transform.SetParent(m_GunBarrel);
		}
	}

	private void SpawnImpactFX(Vector3 position, Vector3 normal)
	{
		if (mData.ImpactFXPrefab != null)
		{
			GameSession.SpawnPoolManager.SpawnObject(mData.ImpactFXPrefab, position, Quaternion.LookRotation(normal));
		}
	}

	private FXElement SpawnHookFX(Vector3 normal)
	{
		Main.EventSystem.Post(MasterSFXEvent.GetEvent(mData.HookFiredAudioEvent));
		if (mData.HookFX != null)
		{
			return GameSession.SpawnPoolManager.SpawnObject(mData.HookFX, m_GunBarrel.transform.position, Quaternion.LookRotation(normal));
		}
		return null;
	}

	private void DespawnHookFX()
	{
		if (mStartHook != null)
		{
			GameSession.SpawnPoolManager.DespawnObject(mStartHook);
			mStartHook = null;
		}
	}

	private void DetachHookFX()
	{
		if (mStartHook != null)
		{
			SpawnImpactFX(mStartHook.transform.position, ValidCandidateStartAnchor.WorldNormal);
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
	}

	private void OnTetherDespawned(Tether tether)
	{
		if (mActiveTethers.Remove(tether))
		{
			tether.OnTetherDespawned = (Action<Tether>)Delegate.Remove(tether.OnTetherDespawned, new Action<Tether>(OnTetherDespawned));
		}
		else
		{
			Debug.LogError("Trying to remove tether that was never added.", tether.gameObject);
		}
	}

	private void OnTetherChanged(TetherChangedEvent ev)
	{
		if (ev.State == TetherChangedEvent.TetherState.AddTethers)
		{
			NumAvailableTethers += ev.NumTethers;
			NumAvailableTethers = math.min(NumAvailableTethers, mData.MaxTethers);
			Main.EventSystem.Post(TriggerableSpeechEvent.GetEvent(base.gameObject, mData.AddTethersSpeech.Data.TriggeredSpeech));
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
		if (Physics.Raycast(new Ray(LynxCameraController.MainCameraTransform.position, LynxCameraController.MainCameraTransform.forward), out hitInfo, mData.LaunchRange, mData.RaycastLayerMask) && IsValidHookable(hitInfo))
		{
			return true;
		}
		bool flag = false;
		Vector2 viewportPoint = Vector2.zero;
		Vector2 zero = Vector2.zero;
		RaycastHit raycastHit = default(RaycastHit);
		for (int i = 0; i < mData.TetherScreenRaysPerRow; i++)
		{
			for (int j = 0; j < mData.TetherScreenRaysPerColumn; j++)
			{
				float x = (float)LynxCameraController.ScreenWidth * (0.5f - mData.TetherScreenWidth / 2f + mData.TetherScreenWidth * ((float)i / (float)(mData.TetherScreenRaysPerRow - 1)));
				float y = (float)LynxCameraController.ScreenHeight * (0.5f - mData.TetherScreenHeight / 2f + mData.TetherScreenHeight * ((float)j / (float)(mData.TetherScreenRaysPerColumn - 1)));
				if (Physics.Raycast(LynxCameraController.MainCamera.ScreenPointToRay(new Vector3(x, y)), out var hitInfo2, mData.LaunchRange, mData.RaycastLayerMask) && IsValidHookable(hitInfo2))
				{
					zero = LynxCameraController.MainCamera.WorldToViewportPoint(hitInfo2.point);
					float num = GameObjectHelper.DistanceSquaredFromCentreScreen(zero);
					float num2 = GameObjectHelper.DistanceSquaredFromCentreScreen(viewportPoint);
					if (!flag || num < num2)
					{
						viewportPoint = zero;
						raycastHit = hitInfo2;
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

	private bool IsValidHookable(RaycastHit hit)
	{
		return ((1 << hit.collider.gameObject.layer) & mData.RaycastLayerMask.value) > 0;
	}
}
