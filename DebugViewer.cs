using System;
using BBI.Unity.Game;
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

		private Vector3 mDebugPlayerVelocity = Vector3.zero;

		private Quaternion mOriginalPlayerRotation = Quaternion.identity;

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

		private bool HasAudioDebugAsset
		{
			get
			{
				if (AudioDebugInfoAsset != null)
				{
					return AudioDebugInfoAsset.Data != null;
				}
				return false;
			}
		}

		public void Refresh()
		{
			DebugInfos = UnityEngine.Object.FindObjectsOfType<DebuggableMonoBehaviour>();
		}

		private void Awake()
		{
			mMegaCutters = UnityEngine.Object.FindObjectsOfType<MegaCuttingController>();
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
			if (LynxControls.Instance != null)
			{
				if (LynxControls.Instance.GameplayActions.ShowDebugControls.WasPressed)
				{
					mShowDebugControls = !mShowDebugControls;
				}
				if (LynxControls.Instance.GameplayActions.ToggleObjectDebugInfo.WasPressed)
				{
					EnableObjectInfoDebugging = !EnableObjectInfoDebugging;
				}
				if (LynxControls.Instance.GameplayActions.DebugRefillOxygen.WasPressed && HealAllDamageAsset != null)
				{
					VitalitySystem.TryModifyVitality(LynxPlayerController.Instance.gameObject, HealAllDamageAsset.Data);
				}
				if (LynxControls.Instance.GameplayActions.DebugRefillThrusters.WasPressed)
				{
					Main.EventSystem.Post(UpdateThrusterChargeEvent.GetEvent(100f, null));
				}
				if (LynxControls.Instance.GameplayActions.DebugMegaCutPlayer.WasPressed && DebugMegaCutter != null)
				{
					DebugMegaCutter.TryCut();
				}
				if (LynxControls.Instance.GameplayActions.DebugMegaCutAll.WasPressed && mMegaCutters != null)
				{
					for (int i = 0; i < mMegaCutters.Length; i++)
					{
						MegaCuttingController megaCuttingController = mMegaCutters[i];
						if (megaCuttingController != null && megaCuttingController != DebugMegaCutter)
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
			if (LynxControls.Instance.GameplayActions.DebugIncrementTimeScale.WasPressed || LynxControls.Instance.GameplayActions.DebugIncrementTimeScale.WasRepeated)
			{
				Time.timeScale = Mathf.Clamp(Time.timeScale += 0.1f, 0f, 20f);
				mDisplayTimeScale = true;
			}
			if (LynxControls.Instance.GameplayActions.DebugDecrementTimeScale.WasPressed || LynxControls.Instance.GameplayActions.DebugDecrementTimeScale.WasRepeated)
			{
				Time.timeScale = Mathf.Clamp(Time.timeScale -= 0.1f, 0f, 20f);
				mDisplayTimeScale = true;
			}
			if (LynxControls.Instance.GameplayActions.DebugResetTimeScale.WasPressed)
			{
				Time.timeScale = 1f;
				mDisplayTimeScale = false;
			}
			if (LynxControls.Instance.GameplayActions.DebugPauseTimeScale.WasPressed)
			{
				Time.timeScale = 0f;
				mDisplayTimeScale = true;
			}
		}

		private void OnGUI()
		{
			if ((HasAudioDebugAsset && AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || mShowDebugControls || (mDisplayTimeScale && Time.timeScale != 1f))
			{
				GUILayout.Label("");
			}
			if ((HasAudioDebugAsset && AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || (DebugAudioService.Instance != null && DebugAudioService.Instance.ShowWwiseTimestamp))
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds(AkSoundEngine.GetTimeStamp());
				GUILayout.BeginArea(new Rect(6f, 20f, 100f, 100f));
				GUILayout.Label($"<color=#ff0088>{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}:{timeSpan.Milliseconds:D3}</color>");
				GUILayout.EndArea();
			}
			if (mShowDebugControls)
			{
				GUILayout.Label("F1 - Refill Oxygen");
				GUILayout.Label("F2 - Refill Thrusters");
				GUILayout.Label("F4 - Frame Rate Counter");
				GUILayout.Label("F5 - Mega Cut Player");
				GUILayout.Label("F6 - Mega Cut All");
				GUILayout.Label("F9 - Save Game");
				GUILayout.Label("F10 - Load Game");
				GUILayout.Label("I - Invert Axes");
				GUILayout.Label("Alt + Z - Glass Mode");
				GUILayout.Label("Shift + Esc - Return to Front End");
			}
		}
	}
}
