using System;
using BBI.Unity.Game;
using Carbon.Core.Events;
using UnityEngine;

namespace BBI.Core.Utility
{
	// Token: 0x0200007F RID: 127
	public class DebugViewer : MonoBehaviour
	{
		// Token: 0x17000084 RID: 132
		// (get) Token: 0x0600038B RID: 907 RVA: 0x00015B5F File Offset: 0x00013D5F
		private bool HasAudioDebugAsset
		{
			get
			{
				return this.AudioDebugInfoAsset != null && this.AudioDebugInfoAsset.Data != null;
			}
		}

		// Token: 0x0600038C RID: 908 RVA: 0x00015B7F File Offset: 0x00013D7F
		public void Refresh()
		{
			this.DebugInfos = Object.FindObjectsOfType<DebuggableMonoBehaviour>();
		}

		// Token: 0x0600038D RID: 909 RVA: 0x00015B8C File Offset: 0x00013D8C
		private void Awake()
		{
			this.mMegaCutters = Object.FindObjectsOfType<MegaCuttingController>();
			Main.EventSystem.AddHandler<ToggleObjectDebuggerEvent>(new EventHandler<ToggleObjectDebuggerEvent>(this.OnToggleObjectDebugger));
		}

		// Token: 0x0600038E RID: 910 RVA: 0x00015BAF File Offset: 0x00013DAF
		private void OnDestroy()
		{
			Main.EventSystem.RemoveHandler<ToggleObjectDebuggerEvent>(new EventHandler<ToggleObjectDebuggerEvent>(this.OnToggleObjectDebugger));
		}

		// Token: 0x0600038F RID: 911 RVA: 0x00015BC7 File Offset: 0x00013DC7
		private void OnToggleObjectDebugger(ToggleObjectDebuggerEvent eventReceived)
		{
			this.EnableObjectInfoDebugging = !this.EnableObjectInfoDebugging;
		}

		// Token: 0x06000390 RID: 912 RVA: 0x00015BD8 File Offset: 0x00013DD8
		private void Update()
		{
			if (LynxControls.Instance != null)
			{
				if (LynxControls.Instance.GameplayActions.ShowDebugControls.WasPressed)
				{
					this.mShowDebugControls = !this.mShowDebugControls;
				}
				if (LynxControls.Instance.GameplayActions.ToggleObjectDebugInfo.WasPressed)
				{
					this.EnableObjectInfoDebugging = !this.EnableObjectInfoDebugging;
				}
				if (LynxControls.Instance.GameplayActions.DebugRefillOxygen.WasPressed && this.HealAllDamageAsset != null)
				{
					VitalitySystem.TryModifyVitality(LynxPlayerController.Instance.gameObject, this.HealAllDamageAsset.Data);
				}
				if (LynxControls.Instance.GameplayActions.DebugRefillThrusters.WasPressed)
				{
					Main.EventSystem.Post(UpdateThrusterChargeEvent.GetEvent(100f, null));
				}
				if (LynxControls.Instance.GameplayActions.DebugMegaCutPlayer.WasPressed && this.DebugMegaCutter != null)
				{
					this.DebugMegaCutter.TryCut();
				}
				if (LynxControls.Instance.GameplayActions.DebugMegaCutAll.WasPressed && this.mMegaCutters != null)
				{
					for (int i = 0; i < this.mMegaCutters.Length; i++)
					{
						MegaCuttingController megaCuttingController = this.mMegaCutters[i];
						if (megaCuttingController != null && megaCuttingController != this.DebugMegaCutter)
						{
							megaCuttingController.TryCut();
						}
					}
				}
			}
			if (this.EnableObjectInfoDebugging)
			{
				this.ObjectInfoDebugger.CalledUpdate();
				this.ObjectInfoDebugger.DrawGizmos();
			}
			this.HandleTimeScaleDebugInput();
		}

		// Token: 0x06000391 RID: 913 RVA: 0x00015D54 File Offset: 0x00013F54
		private void HandleTimeScaleDebugInput()
		{
			if (LynxControls.Instance.GameplayActions.DebugIncrementTimeScale.WasPressed || LynxControls.Instance.GameplayActions.DebugIncrementTimeScale.WasRepeated)
			{
				Time.timeScale = Mathf.Clamp(Time.timeScale += 0.1f, 0f, 20f);
				this.mDisplayTimeScale = true;
			}
			if (LynxControls.Instance.GameplayActions.DebugDecrementTimeScale.WasPressed || LynxControls.Instance.GameplayActions.DebugDecrementTimeScale.WasRepeated)
			{
				Time.timeScale = Mathf.Clamp(Time.timeScale -= 0.1f, 0f, 20f);
				this.mDisplayTimeScale = true;
			}
			if (LynxControls.Instance.GameplayActions.DebugResetTimeScale.WasPressed)
			{
				Time.timeScale = 1f;
				this.mDisplayTimeScale = false;
			}
			if (LynxControls.Instance.GameplayActions.DebugPauseTimeScale.WasPressed)
			{
				Time.timeScale = 0f;
				this.mDisplayTimeScale = true;
			}
		}

		// Token: 0x06000392 RID: 914 RVA: 0x00015E60 File Offset: 0x00014060
		private void OnGUI()
		{
			if ((this.HasAudioDebugAsset && this.AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || this.mShowDebugControls || (this.mDisplayTimeScale && Time.timeScale != 1f))
			{
				GUILayout.Label("", Array.Empty<GUILayoutOption>());
			}
			if ((this.HasAudioDebugAsset && this.AudioDebugInfoAsset.Data.ShowWwiseTimestamp) || (DebugAudioService.Instance != null && DebugAudioService.Instance.ShowWwiseTimestamp))
			{
				TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)AkSoundEngine.GetTimeStamp());
				GUILayout.BeginArea(new Rect(6f, 20f, 100f, 100f));
				GUILayout.Label(string.Format("<color=#ff0088>{0:D2}:{1:D2}:{2:D2}:{3:D3}</color>", new object[] { timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds }), Array.Empty<GUILayoutOption>());
				GUILayout.EndArea();
			}
			if (this.mShowDebugControls)
			{
				GUILayout.Label("F1 - Refill Oxygen", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F2 - Refill Thrusters", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F4 - Frame Rate Counter", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F5 - Mega Cut Player", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F6 - Mega Cut All", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F9 - Save Game", Array.Empty<GUILayoutOption>());
				GUILayout.Label("F10 - Load Game", Array.Empty<GUILayoutOption>());
				GUILayout.Label("I - Invert Axes", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Alt + Z - Glass Mode", Array.Empty<GUILayoutOption>());
				GUILayout.Label("Shift + Esc - Return to Front End", Array.Empty<GUILayoutOption>());
			}
		}

		// Token: 0x040003CC RID: 972
		public DebuggableMonoBehaviour[] DebugInfos = new DebuggableMonoBehaviour[0];

		// Token: 0x040003CD RID: 973
		public bool EnableQuickRestart = true;

		// Token: 0x040003CE RID: 974
		public bool EnableObjectInfoDebugging;

		// Token: 0x040003CF RID: 975
		public float DebugPlayerSpeed = 2f;

		// Token: 0x040003D0 RID: 976
		public float DebugPlayerDrag = 0.95f;

		// Token: 0x040003D1 RID: 977
		public float DebugMouselookSensitivityX = 10f;

		// Token: 0x040003D2 RID: 978
		public float DebugMouselookSensitivityY = 10f;

		// Token: 0x040003D3 RID: 979
		public AudioDebugInfoAsset AudioDebugInfoAsset;

		// Token: 0x040003D4 RID: 980
		public DamageAsset HealAllDamageAsset;

		// Token: 0x040003D5 RID: 981
		public MegaCuttingController DebugMegaCutter;

		// Token: 0x040003D6 RID: 982
		private Vector3 mDebugPlayerVelocity = Vector3.zero;

		// Token: 0x040003D7 RID: 983
		private Quaternion mOriginalPlayerRotation = Quaternion.identity;

		// Token: 0x040003D8 RID: 984
		private const float kMinMouselookX = -360f;

		// Token: 0x040003D9 RID: 985
		private const float kMaxMouselookX = 360f;

		// Token: 0x040003DA RID: 986
		private const float kMinMouselookY = -360f;

		// Token: 0x040003DB RID: 987
		private const float kMaxMouselookY = 360f;

		// Token: 0x040003DC RID: 988
		private const float kTimeScaleIncrement = 0.1f;

		// Token: 0x040003DD RID: 989
		private const float kMaxTimeScale = 20f;

		// Token: 0x040003DE RID: 990
		private bool mDisplayTimeScale;

		// Token: 0x040003DF RID: 991
		private bool mShowDebugControls;

		// Token: 0x040003E0 RID: 992
		private MegaCuttingController[] mMegaCutters;

		// Token: 0x040003E1 RID: 993
		public ObjectInfoDebugger ObjectInfoDebugger = new ObjectInfoDebugger();
	}
}
