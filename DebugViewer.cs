using System;
using BBI.Unity.Game;
using InControl;
using UnityEngine;

namespace BBI.Core.Utility
{
	public class DebugViewer : MonoBehaviour
	{
		public DebuggableMonoBehaviour[] DebugInfos = new DebuggableMonoBehaviour[0];

		public bool EnableQuickRestart = true;

		public bool EnableObjectInfoDebugging;

		public float DebugPlayerSpeed = 2f;

		public float DebugPlayerDrag = 0.95f;

		public float DebugMouselookSensitivityX = 10f;

		public float DebugMouselookSensitivityY = 10f;

		public AudioDebugInfoAsset AudioDebugInfoAsset;

		public DamageAsset HealAllDamageAsset;

		public MegaCuttingController DebugMegaCutter;

		private Vector3 mDebugPlayerVelocity = Vector3.get_zero();

		private Quaternion mOriginalPlayerRotation = Quaternion.get_identity();

		private const float kMinMouselookX = -360f;

		private const float kMaxMouselookX = 360f;

		private const float kMinMouselookY = -360f;

		private const float kMaxMouselookY = 360f;

		private const float kTimeScaleIncrement = 0.1f;

		private const float kMaxTimeScale = 20f;

		private bool mDisplayTimeScale;

		private bool mShowDebugControls;

		private MegaCuttingController[] mMegaCutters;

		public ObjectInfoDebugger ObjectInfoDebugger = new ObjectInfoDebugger();

		private bool mShowRaceControls;

		private bool HasAudioDebugAsset
		{
			get
			{
				if ((Object)(object)AudioDebugInfoAsset != (Object)null)
				{
					return AudioDebugInfoAsset.Data != null;
				}
				return false;
			}
		}

		public void Refresh()
		{
			DebugInfos = Object.FindObjectsOfType<DebuggableMonoBehaviour>();
		}

		private void Awake()
		{
			mMegaCutters = Object.FindObjectsOfType<MegaCuttingController>();
			Main.EventSystem.AddHandler<ToggleObjectDebuggerEvent>(OnToggleObjectDebugger);
		}

		private void OnDestroy()
		{
			Main.EventSystem.RemoveHandler<ToggleObjectDebuggerEvent>(OnToggleObjectDebugger);
		}

		private void OnToggleObjectDebugger(ToggleObjectDebuggerEvent eventReceived)
		{
			EnableObjectInfoDebugging = !EnableObjectInfoDebugging;
		}

		private void Update()
		{
			if ((Object)(object)LynxControls.Instance != (Object)null)
			{
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.ShowDebugControls).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
				{
					mShowDebugControls = !mShowDebugControls;
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.ShowDebugControls).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.WeeklyShip)
				{
					mShowRaceControls = !mShowRaceControls;
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.ToggleObjectDebugInfo).get_WasPressed())
				{
					EnableObjectInfoDebugging = !EnableObjectInfoDebugging;
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugRefillOxygen).get_WasPressed() && (Object)(object)HealAllDamageAsset != (Object)null)
				{
					VitalitySystem.TryModifyVitality(((Component)LynxPlayerController.Instance).get_gameObject(), HealAllDamageAsset.Data);
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugRefillThrusters).get_WasPressed())
				{
					Main.EventSystem.Post(UpdateThrusterChargeEvent.GetEvent(100f, null));
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugMegaCutPlayer).get_WasPressed() && (Object)(object)DebugMegaCutter != (Object)null)
				{
					DebugMegaCutter.TryCut();
				}
				if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugMegaCutAll).get_WasPressed() && mMegaCutters != null)
				{
					for (int i = 0; i < mMegaCutters.Length; i++)
					{
						MegaCuttingController megaCuttingController = mMegaCutters[i];
						if ((Object)(object)megaCuttingController != (Object)null && (Object)(object)megaCuttingController != (Object)(object)DebugMegaCutter)
						{
							megaCuttingController.TryCut();
						}
					}
				}
			}
			if (EnableObjectInfoDebugging)
			{
				ObjectInfoDebugger.CalledUpdate();
				ObjectInfoDebugger.DrawGizmos();
			}
			HandleTimeScaleDebugInput();
		}

		private void HandleTimeScaleDebugInput()
		{
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugIncrementTimeScale).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				float num = Time.get_timeScale() + 0.1f;
				Time.set_timeScale(num);
				Time.set_timeScale(Mathf.Clamp(num, 0f, 20f));
				mDisplayTimeScale = true;
			}
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugDecrementTimeScale).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				float num2 = Time.get_timeScale() - 0.1f;
				Time.set_timeScale(num2);
				Time.set_timeScale(Mathf.Clamp(num2, 0f, 20f));
				mDisplayTimeScale = true;
			}
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugResetTimeScale).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				Time.set_timeScale(1f);
				mDisplayTimeScale = false;
			}
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.DebugPauseTimeScale).get_WasPressed())
			{
				Time.set_timeScale(0f);
				mDisplayTimeScale = true;
			}
			if (((OneAxisInputControl)LynxControls.Instance.GameplayActions.ToggleDebugMenu).get_WasPressed() && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				CertificationService.Instance.TryIncreaseCertification();
			}
		}

		private void OnGUI()
		{
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			if ((HasAudioDebugAsset && AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || mShowDebugControls || (mDisplayTimeScale && Time.get_timeScale() != 1f))
			{
				GUILayout.Label("", Array.Empty<GUILayoutOption>());
			}
			if ((HasAudioDebugAsset && AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || (DebugAudioService.Instance != null && DebugAudioService.Instance.ShowWwiseTimestamp))
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(AkSoundEngine.GetTimeStamp());
				GUILayout.BeginArea(new Rect(6f, 20f, 100f, 100f));
				GUILayout.Label($"<color=#ff0088>{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}:{timeSpan.Milliseconds:D3}</color>", Array.Empty<GUILayoutOption>());
				GUILayout.EndArea();
			}
			if (mShowDebugControls)
			{
				GUILayout.Label("F1 - Refill Oxygen", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F2 - Refill Thrusters", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F3 - Glass Mode", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F4 - Frame Rate Counter", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F5 - Mega Cutter", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F6 - Complete Current Certification", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F7 - Toggle Wireframe", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F10 - Show Modded Controls", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Left Arrow - Pause Time", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Right Arrow - Reset Game Speed", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Up Arrow - Increase Game Speed by 0.1x", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Down Arrow - Decrease Game Speed by 0.1x", Array.Empty<GUILayoutOption>());
			}
			if (mShowRaceControls)
			{
				GUILayout.Label(" ", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F3 - Glass Mode", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F4 - Frame Rate Counter", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F7 - Toggle Wireframe", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F10 - Show Modded Controls", Array.Empty<GUILayoutOption>());
			}
		}

		public DebugViewer()
			: this()
		{
		}//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)

	}
}
