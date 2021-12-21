using System;
using System.Collections;
using Carbon.Core;
using Carbon.Core.Events;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000AB3 RID: 2739
	public class Player : MonoBehaviour
	{
		// Token: 0x17000FC8 RID: 4040
		// (get) Token: 0x06003A97 RID: 14999 RVA: 0x0010623C File Offset: 0x0010443C
		public VitalityController VitalityController
		{
			get
			{
				return this.mVitalityController;
			}
		}

		// Token: 0x17000FC9 RID: 4041
		// (get) Token: 0x06003A98 RID: 15000 RVA: 0x00106244 File Offset: 0x00104444
		public Collider PlayerCollider
		{
			get
			{
				if (this.mPlayerCollider == null)
				{
					this.mPlayerCollider = Player.FindPlayerCollider(this);
				}
				return this.mPlayerCollider;
			}
		}

		// Token: 0x17000FCA RID: 4042
		// (get) Token: 0x06003A99 RID: 15001 RVA: 0x00106266 File Offset: 0x00104466
		public Entity Entity
		{
			get
			{
				if (!(this.mEntityBlueprintComponent != null))
				{
					return Entity.Null;
				}
				return this.mEntityBlueprintComponent.Entity;
			}
		}

		// Token: 0x06003A9A RID: 15002 RVA: 0x00106288 File Offset: 0x00104488
		public virtual void InitializeECSData(EntityManager entityManager, Entity entity)
		{
			if (this.mVitalityController != null)
			{
				this.mVitalityController.InitializeECSData(entityManager, entity);
			}
			entityManager.AddComponentData<RespawnLocation>(entity, new RespawnLocation
			{
				Position = base.transform.position,
				Rotation = base.transform.rotation
			});
			entityManager.AddBuffer<PlayerElementalHistory>(entity);
		}

		// Token: 0x06003A9B RID: 15003 RVA: 0x001062FC File Offset: 0x001044FC
		private void Awake()
		{
			this.mPlayerRigidbody = base.GetComponent<Rigidbody>();
			this.mVitalityController = base.GetComponent<VitalityController>();
			Main.EventSystem.Post(new PlayerSpawnedEvent(this));
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if (masterScenePlayer != null)
			{
				base.transform.position = masterScenePlayer.transform.position;
				base.transform.rotation = masterScenePlayer.transform.rotation;
				if (this.m_InitialMotionSettings != null && masterScenePlayer.InitialMotionSettings != null)
				{
					this.m_InitialMotionSettings.CopySettings(masterScenePlayer.InitialMotionSettings);
				}
				this.mDisableThrust = !masterScenePlayer.ThrustEnabled;
				Object.Destroy(masterScenePlayer.gameObject);
			}
			if (SceneLoader.Instance.SpawnLocationOverride != null)
			{
				base.transform.position = SceneLoader.Instance.SpawnLocationOverride.Value;
				SceneLoader.Instance.SpawnLocationOverride = null;
			}
			this.mSpawnedPosition = base.transform.position;
			this.mSpawnedRotation = base.transform.rotation;
			if (EntityBlueprintComponent.TryGetOrAddEntityBlueprint(base.gameObject, out this.mEntityBlueprintComponent))
			{
				this.InitializeECSData(this.mEntityBlueprintComponent.EntityManager, this.mEntityBlueprintComponent.Entity);
				EntityBlueprintComponent.ResetComponentObjectsOnGameObject(this.mEntityBlueprintComponent.Entity, this.mEntityBlueprintComponent.EntityManager, base.gameObject);
				PlayerProfileService.Instance.Profile.RegisterPlayer(this);
				bool enabled = true;
				Player.SetGodMode(this.mEntityBlueprintComponent.Entity, this.mEntityBlueprintComponent.EntityManager, enabled);
				bool enabled2 = true;
				Player.SetTroutMode(this.mEntityBlueprintComponent.Entity, this.mEntityBlueprintComponent.EntityManager, enabled2);
			}
			bool enabled3 = true;
			Player.SetNoClipMode(this.PlayerCollider, Player.FindPlayerMotion(this), enabled3);
			Main.EventSystem.AddHandler<RespawnEvent>(new EventHandler<RespawnEvent>(this.OnRespawned));
			Main.EventSystem.AddHandler<CheckpointEvent>(new EventHandler<CheckpointEvent>(this.OnCheckpoint));
			Main.EventSystem.AddHandler<UnlockAbilityEvent>(new EventHandler<UnlockAbilityEvent>(this.OnOxygenRegenUnlocked));
		}

		// Token: 0x06003A9C RID: 15004 RVA: 0x00106508 File Offset: 0x00104708
		private void Start()
		{
			Inventory inventory = PlayerProfileService.Instance.Profile.Inventory;
			if (inventory != null)
			{
				inventory.ReEquipItems();
			}
			PlayerProfileService.Instance.Profile.ApplyUpgrades();
			if (this.mDisableThrust)
			{
				Main.EventSystem.Post(ThrustChargeChangedEvent.GetEvent(0f, 0f, ThrustController.ChargeState.Disabled, 100f));
			}
			bool enabled = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
			Player.SetGodMode(this.mEntityBlueprintComponent.Entity, this.mEntityBlueprintComponent.EntityManager, enabled);
		}

		// Token: 0x06003A9D RID: 15005 RVA: 0x00106594 File Offset: 0x00104794
		private void OnDestroy()
		{
			PlayerProfileService.Instance.Profile.ClearAppliedUpgrades();
			PlayerProfileService.Instance.Profile.UnregisterPlayer();
			this.ResetAudioValues();
			Main.EventSystem.RemoveHandler<RespawnEvent>(new EventHandler<RespawnEvent>(this.OnRespawned));
			Main.EventSystem.RemoveHandler<CheckpointEvent>(new EventHandler<CheckpointEvent>(this.OnCheckpoint));
			Main.EventSystem.RemoveHandler<UnlockAbilityEvent>(new EventHandler<UnlockAbilityEvent>(this.OnOxygenRegenUnlocked));
		}

		// Token: 0x06003A9E RID: 15006 RVA: 0x00106608 File Offset: 0x00104808
		private void ResetAudioValues()
		{
			AkSoundEngine.SetState(PlayerRoomTrackerSystem.sAudioStatePressure, PlayerRoomTrackerSystem.sAudioStateFalse);
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(PlayerRoomTrackerSystem.sAudioParameterPressure, 0f));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(FogUpdateSystem.sAudioParameterDirtyAir, 0f));
			AkSoundEngine.SetState(PlayerRoomTrackerSystem.sAudioStateRoomType, PlayerRoomTrackerSystem.sAudioStateNone);
		}

		// Token: 0x06003A9F RID: 15007 RVA: 0x0010667C File Offset: 0x0010487C
		private void OnCheckpoint(CheckpointEvent ev)
		{
			this.mCurrentCheckpoint = ev.Checkpoint;
			if (EntityBlueprintComponent.IsValid(this.mEntityBlueprintComponent))
			{
				RespawnLocation respawnLocation = new RespawnLocation
				{
					Position = ((ev.Checkpoint != null) ? ev.Checkpoint.position : this.mSpawnedPosition),
					Rotation = ((ev.Checkpoint != null) ? ev.Checkpoint.rotation : this.mSpawnedRotation)
				};
				if (this.mEntityBlueprintComponent.EntityManager.HasComponent<RespawnPending>(this.mEntityBlueprintComponent.Entity))
				{
					this.mEntityBlueprintComponent.EntityManager.SetComponentData<RespawnLocation>(this.mEntityBlueprintComponent.Entity, respawnLocation);
					return;
				}
				this.mEntityBlueprintComponent.EntityManager.SetComponentData<RespawnLocation>(this.mEntityBlueprintComponent.Entity, respawnLocation);
			}
		}

		// Token: 0x06003AA0 RID: 15008 RVA: 0x00106768 File Offset: 0x00104968
		private void OnOxygenRegenUnlocked(UnlockAbilityEvent ev)
		{
			if (ev.Ability == UnlockAbilityID.OxygenTankRecharge && EntityBlueprintComponent.IsValid(this.mEntityBlueprintComponent) && !this.mEntityBlueprintComponent.EntityManager.HasComponent<UnlockOxygenRegenComponent>(this.Entity))
			{
				this.mEntityBlueprintComponent.EntityManager.AddComponentData<UnlockOxygenRegenComponent>(this.mEntityBlueprintComponent.Entity, default(UnlockOxygenRegenComponent));
			}
		}

		// Token: 0x06003AA1 RID: 15009 RVA: 0x001067D4 File Offset: 0x001049D4
		private void OnRespawned(RespawnEvent ev)
		{
			if (EntityBlueprintComponent.IsValid(this.mEntityBlueprintComponent) && !this.mEntityBlueprintComponent.EntityManager.HasComponent<RespawnPending>(this.mEntityBlueprintComponent.Entity))
			{
				this.mEntityBlueprintComponent.EntityManager.AddComponentData<RespawnPending>(this.mEntityBlueprintComponent.Entity, default(RespawnPending));
				bool enabled = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
				Player.SetGodMode(this.mEntityBlueprintComponent.Entity, this.mEntityBlueprintComponent.EntityManager, enabled);
			}
			if (this.mPlayerRigidbody != null)
			{
				this.mPlayerRigidbody.interpolation = 0;
				this.mPlayerRigidbody.collisionDetectionMode = 0;
				this.mPlayerRigidbody.isKinematic = true;
			}
			base.StartCoroutine(this.WaitToRespawnPlayer());
			this.ResetAudioValues();
		}

		// Token: 0x06003AA2 RID: 15010 RVA: 0x001068AA File Offset: 0x00104AAA
		private IEnumerator WaitToRespawnPlayer()
		{
			yield return new WaitForFixedUpdate();
			if (this.mPlayerRigidbody != null)
			{
				this.mPlayerRigidbody.velocity = Vector3.zero;
				this.mPlayerRigidbody.angularVelocity = Vector3.zero;
				this.mPlayerRigidbody.isKinematic = false;
				this.mPlayerRigidbody.interpolation = 1;
				this.mPlayerRigidbody.collisionDetectionMode = 2;
			}
			yield break;
		}

		// Token: 0x06003AA3 RID: 15011 RVA: 0x001068B9 File Offset: 0x00104AB9
		public static PlayerMotion FindPlayerMotion(Player player)
		{
			if (player == null)
			{
				return null;
			}
			return player.GetComponent<PlayerMotion>();
		}

		// Token: 0x06003AA4 RID: 15012 RVA: 0x001068CC File Offset: 0x00104ACC
		private static Collider FindPlayerCollider(Player player)
		{
			if (player == null)
			{
				return null;
			}
			Collider collider = null;
			SphereCollider[] componentsInChildren = player.GetComponentsInChildren<SphereCollider>();
			if (componentsInChildren != null)
			{
				foreach (SphereCollider sphereCollider in componentsInChildren)
				{
					if (sphereCollider.CompareTag("Player"))
					{
						collider = sphereCollider;
						break;
					}
				}
			}
			if (collider == null)
			{
				CapsuleCollider[] componentsInChildren2 = player.GetComponentsInChildren<CapsuleCollider>();
				if (componentsInChildren2 != null)
				{
					foreach (CapsuleCollider capsuleCollider in componentsInChildren2)
					{
						if (capsuleCollider.CompareTag("Player"))
						{
							collider = capsuleCollider;
							break;
						}
					}
				}
			}
			return collider;
		}

		// Token: 0x06003AA5 RID: 15013 RVA: 0x00106956 File Offset: 0x00104B56
		public static void SetNoClipMode(Collider collider, PlayerMotion playerMotion, bool enabled)
		{
			if (GlobalOptions.Raw.GetBool("General.NoClipMode", false) && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				collider.isTrigger = enabled;
				playerMotion.SetSquishyCollide(!enabled);
			}
		}

		// Token: 0x06003AA6 RID: 15014 RVA: 0x0010697C File Offset: 0x00104B7C
		public static void SetGodMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			entityManager.RemoveComponent<Invulnerable>(entity);
			if (GlobalOptions.Raw.GetBool("General.GodMode", false) && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				entityManager.AddComponentData<Invulnerable>(entity, default(Invulnerable));
				return;
			}
		}

		// Token: 0x06003AA7 RID: 15015 RVA: 0x001069A8 File Offset: 0x00104BA8
		public static void SetTroutMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			if (GlobalOptions.Raw.GetBool("General.TroutMode", false) && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				entityManager.RemoveComponent<ReceiveForceOnDecompression>(entity);
			}
			entityManager.AddComponentData<ReceiveForceOnDecompression>(entity, default(ReceiveForceOnDecompression));
		}

		// Token: 0x04002F1C RID: 12060
		public const string kPlayerTag = "Player";

		// Token: 0x04002F1D RID: 12061
		[Header("Player Settings")]
		[SerializeField]
		private InitialMotionSettings m_InitialMotionSettings;

		// Token: 0x04002F1E RID: 12062
		private bool mDisableThrust;

		// Token: 0x04002F1F RID: 12063
		private Vector3 mSpawnedPosition = Vector3.zero;

		// Token: 0x04002F20 RID: 12064
		private Quaternion mSpawnedRotation = Quaternion.identity;

		// Token: 0x04002F21 RID: 12065
		private Rigidbody mPlayerRigidbody;

		// Token: 0x04002F22 RID: 12066
		private Collider mPlayerCollider;

		// Token: 0x04002F23 RID: 12067
		private Transform mCurrentCheckpoint;

		// Token: 0x04002F24 RID: 12068
		private VitalityController mVitalityController;

		// Token: 0x04002F25 RID: 12069
		private EntityBlueprintComponent mEntityBlueprintComponent;
	}
}
