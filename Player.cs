using System.Collections;
using Unity.Entities;
using UnityEngine;

namespace BBI.Unity.Game
{
	public class Player : MonoBehaviour
	{
		public const string kPlayerTag = "Player";

		[Header("Player Settings")]
		[SerializeField]
		private InitialMotionSettings m_InitialMotionSettings;

		private bool mDisableThrust;

		private Vector3 mSpawnedPosition = Vector3.zero;

		private Quaternion mSpawnedRotation = Quaternion.identity;

		private Rigidbody mPlayerRigidbody;

		private Collider mPlayerCollider;

		private Transform mCurrentCheckpoint;

		private VitalityController mVitalityController;

		private EntityBlueprintComponent mEntityBlueprintComponent;

		public VitalityController VitalityController => mVitalityController;

		public Collider PlayerCollider
		{
			get
			{
				if (mPlayerCollider == null)
				{
					mPlayerCollider = FindPlayerCollider(this);
				}
				return mPlayerCollider;
			}
		}

		public Entity Entity
		{
			get
			{
				if (!(mEntityBlueprintComponent != null))
				{
					return Entity.Null;
				}
				return mEntityBlueprintComponent.Entity;
			}
		}

		public virtual void InitializeECSData(EntityManager entityManager, Entity entity)
		{
			if (mVitalityController != null)
			{
				mVitalityController.InitializeECSData(entityManager, entity);
			}
			entityManager.AddComponentData(entity, new RespawnLocation
			{
				Position = base.transform.position,
				Rotation = base.transform.rotation
			});
			entityManager.AddBuffer<PlayerElementalHistory>(entity);
		}

		private void Awake()
		{
			mPlayerRigidbody = GetComponent<Rigidbody>();
			mVitalityController = GetComponent<VitalityController>();
			Main.EventSystem.Post(new PlayerSpawnedEvent(this));
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if (masterScenePlayer != null)
			{
				base.transform.position = masterScenePlayer.transform.position;
				base.transform.rotation = masterScenePlayer.transform.rotation;
				if (m_InitialMotionSettings != null && masterScenePlayer.InitialMotionSettings != null)
				{
					m_InitialMotionSettings.CopySettings(masterScenePlayer.InitialMotionSettings);
				}
				mDisableThrust = !masterScenePlayer.ThrustEnabled;
				Object.Destroy(masterScenePlayer.gameObject);
			}
			if (SceneLoader.Instance.SpawnLocationOverride.HasValue)
			{
				base.transform.position = SceneLoader.Instance.SpawnLocationOverride.Value;
				SceneLoader.Instance.SpawnLocationOverride = null;
			}
			mSpawnedPosition = base.transform.position;
			mSpawnedRotation = base.transform.rotation;
			if (EntityBlueprintComponent.TryGetOrAddEntityBlueprint(base.gameObject, out mEntityBlueprintComponent))
			{
				InitializeECSData(mEntityBlueprintComponent.EntityManager, mEntityBlueprintComponent.Entity);
				EntityBlueprintComponent.ResetComponentObjectsOnGameObject(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, base.gameObject);
				PlayerProfileService.Instance.Profile.RegisterPlayer(this);
				bool flag = false;
				SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, flag);
				bool flag2 = false;
				SetTroutMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, flag2);
			}
			bool flag3 = false;
			SetNoClipMode(PlayerCollider, FindPlayerMotion(this), flag3);
			Main.EventSystem.AddHandler<RespawnEvent>(OnRespawned);
			Main.EventSystem.AddHandler<CheckpointEvent>(OnCheckpoint);
			Main.EventSystem.AddHandler<UnlockAbilityEvent>(OnOxygenRegenUnlocked);
		}

		private void Start()
		{
			PlayerProfileService.Instance.Profile.Inventory?.ReEquipItems();
			PlayerProfileService.Instance.Profile.ApplyUpgrades();
			if (mDisableThrust)
			{
				Main.EventSystem.Post(ThrustChargeChangedEvent.GetEvent(0f, 0f, ThrustController.ChargeState.Disabled, 100f));
			}
			bool flag = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
			SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, flag);
		}

		private void OnDestroy()
		{
			PlayerProfileService.Instance.Profile.ClearAppliedUpgrades();
			PlayerProfileService.Instance.Profile.UnregisterPlayer();
			ResetAudioValues();
			Main.EventSystem.RemoveHandler<RespawnEvent>(OnRespawned);
			Main.EventSystem.RemoveHandler<CheckpointEvent>(OnCheckpoint);
			Main.EventSystem.RemoveHandler<UnlockAbilityEvent>(OnOxygenRegenUnlocked);
		}

		private void ResetAudioValues()
		{
			AkSoundEngine.SetState(PlayerRoomTrackerSystem.sAudioStatePressure, PlayerRoomTrackerSystem.sAudioStateFalse);
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(PlayerRoomTrackerSystem.sAudioParameterPressure, 0f));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(FogUpdateSystem.sAudioParameterDirtyAir, 0f));
			AkSoundEngine.SetState(PlayerRoomTrackerSystem.sAudioStateRoomType, PlayerRoomTrackerSystem.sAudioStateNone);
		}

		private void OnCheckpoint(CheckpointEvent ev)
		{
			mCurrentCheckpoint = ev.Checkpoint;
			if (EntityBlueprintComponent.IsValid(mEntityBlueprintComponent))
			{
				RespawnLocation respawnLocation = default(RespawnLocation);
				respawnLocation.Position = ((ev.Checkpoint != null) ? ev.Checkpoint.position : mSpawnedPosition);
				respawnLocation.Rotation = ((ev.Checkpoint != null) ? ev.Checkpoint.rotation : mSpawnedRotation);
				RespawnLocation componentData = respawnLocation;
				if (mEntityBlueprintComponent.EntityManager.HasComponent<RespawnPending>(mEntityBlueprintComponent.Entity))
				{
					mEntityBlueprintComponent.EntityManager.SetComponentData(mEntityBlueprintComponent.Entity, componentData);
				}
				else
				{
					mEntityBlueprintComponent.EntityManager.SetComponentData(mEntityBlueprintComponent.Entity, componentData);
				}
			}
		}

		private void OnOxygenRegenUnlocked(UnlockAbilityEvent ev)
		{
			if (ev.Ability == UnlockAbilityID.OxygenTankRecharge && EntityBlueprintComponent.IsValid(mEntityBlueprintComponent) && !mEntityBlueprintComponent.EntityManager.HasComponent<UnlockOxygenRegenComponent>(Entity))
			{
				mEntityBlueprintComponent.EntityManager.AddComponentData(mEntityBlueprintComponent.Entity, default(UnlockOxygenRegenComponent));
			}
		}

		private void OnRespawned(RespawnEvent ev)
		{
			if (EntityBlueprintComponent.IsValid(mEntityBlueprintComponent) && !mEntityBlueprintComponent.EntityManager.HasComponent<RespawnPending>(mEntityBlueprintComponent.Entity))
			{
				mEntityBlueprintComponent.EntityManager.AddComponentData(mEntityBlueprintComponent.Entity, default(RespawnPending));
				bool flag = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
				SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, flag);
			}
			if (mPlayerRigidbody != null)
			{
				mPlayerRigidbody.interpolation = RigidbodyInterpolation.None;
				mPlayerRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
				mPlayerRigidbody.isKinematic = true;
			}
			StartCoroutine(WaitToRespawnPlayer());
			ResetAudioValues();
		}

		private IEnumerator WaitToRespawnPlayer()
		{
			yield return new WaitForFixedUpdate();
			if (mPlayerRigidbody != null)
			{
				mPlayerRigidbody.velocity = Vector3.zero;
				mPlayerRigidbody.angularVelocity = Vector3.zero;
				mPlayerRigidbody.isKinematic = false;
				mPlayerRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
				mPlayerRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			}
		}

		public static PlayerMotion FindPlayerMotion(Player player)
		{
			if (player == null)
			{
				return null;
			}
			return player.GetComponent<PlayerMotion>();
		}

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

		public static void SetNoClipMode(Collider collider, PlayerMotion playerMotion, bool enabled)
		{
			if (collider != null)
			{
				collider.isTrigger = enabled;
			}
			if (playerMotion != null)
			{
				playerMotion.SetSquishyCollide(!enabled);
			}
		}

		public static void SetGodMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			if (enabled)
			{
				entityManager.AddComponentData(entity, default(Invulnerable));
			}
			else
			{
				entityManager.RemoveComponent<Invulnerable>(entity);
			}
		}

		public static void SetTroutMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			if (enabled)
			{
				entityManager.RemoveComponent<ReceiveForceOnDecompression>(entity);
			}
			else
			{
				entityManager.AddComponentData(entity, default(ReceiveForceOnDecompression));
			}
		}
	}
}
