using System.Collections;
using Carbon.Audio;
using Carbon.Core;
using Unity.Entities;
using Unity.Mathematics;
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

		private Vector3 mSpawnedPosition = Vector3.get_zero();

		private Quaternion mSpawnedRotation = Quaternion.get_identity();

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
				if ((Object)(object)mPlayerCollider == (Object)null)
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
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				if (!((Object)(object)mEntityBlueprintComponent != (Object)null))
				{
					return Entity.get_Null();
				}
				return mEntityBlueprintComponent.Entity;
			}
		}

		public virtual void InitializeECSData(EntityManager entityManager, Entity entity)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)mVitalityController != (Object)null)
			{
				mVitalityController.InitializeECSData(entityManager, entity);
			}
			((EntityManager)(ref entityManager)).AddComponentData<RespawnLocation>(entity, new RespawnLocation
			{
				Position = float3.op_Implicit(((Component)this).get_transform().get_position()),
				Rotation = quaternion.op_Implicit(((Component)this).get_transform().get_rotation())
			});
			((EntityManager)(ref entityManager)).AddBuffer<PlayerElementalHistory>(entity);
		}

		private void Awake()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			mPlayerRigidbody = ((Component)this).GetComponent<Rigidbody>();
			mVitalityController = ((Component)this).GetComponent<VitalityController>();
			Main.EventSystem.Post(new PlayerSpawnedEvent(this));
			EditorScenePlayer masterScenePlayer = EditorScenePlayer.MasterScenePlayer;
			if ((Object)(object)masterScenePlayer != (Object)null)
			{
				((Component)this).get_transform().set_position(((Component)masterScenePlayer).get_transform().get_position());
				((Component)this).get_transform().set_rotation(((Component)masterScenePlayer).get_transform().get_rotation());
				if ((Object)(object)m_InitialMotionSettings != (Object)null && (Object)(object)masterScenePlayer.InitialMotionSettings != (Object)null)
				{
					m_InitialMotionSettings.CopySettings(masterScenePlayer.InitialMotionSettings);
				}
				mDisableThrust = !masterScenePlayer.ThrustEnabled;
				Object.Destroy((Object)(object)((Component)masterScenePlayer).get_gameObject());
			}
			if (SceneLoader.Instance.SpawnLocationOverride.HasValue)
			{
				((Component)this).get_transform().set_position(SceneLoader.Instance.SpawnLocationOverride.Value);
				SceneLoader.Instance.SpawnLocationOverride = null;
			}
			mSpawnedPosition = ((Component)this).get_transform().get_position();
			mSpawnedRotation = ((Component)this).get_transform().get_rotation();
			if (EntityBlueprintComponent.TryGetOrAddEntityBlueprint(((Component)this).get_gameObject(), out mEntityBlueprintComponent))
			{
				InitializeECSData(mEntityBlueprintComponent.EntityManager, mEntityBlueprintComponent.Entity);
				EntityBlueprintComponent.ResetComponentObjectsOnGameObject(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, ((Component)this).get_gameObject());
				PlayerProfileService.Instance.Profile.RegisterPlayer(this);
				bool enabled = true;
				SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, enabled);
				bool enabled2 = true;
				SetTroutMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, enabled2);
			}
			bool enabled3 = true;
			SetNoClipMode(PlayerCollider, FindPlayerMotion(this), enabled3);
			Main.EventSystem.AddHandler<RespawnEvent>(OnRespawned);
			Main.EventSystem.AddHandler<CheckpointEvent>(OnCheckpoint);
			Main.EventSystem.AddHandler<UnlockAbilityEvent>(OnOxygenRegenUnlocked);
		}

		private void Start()
		{
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			PlayerProfileService.Instance.Profile.Inventory?.ReEquipItems();
			PlayerProfileService.Instance.Profile.ApplyUpgrades();
			if (mDisableThrust)
			{
				Main.EventSystem.Post(ThrustChargeChangedEvent.GetEvent(0f, 0f, ThrustController.ChargeState.Disabled, 100f));
			}
			bool enabled = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
			SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, enabled);
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
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			AkSoundEngine.SetState(WwiseShortID.op_Implicit(PlayerRoomTrackerSystem.sAudioStatePressure), WwiseShortID.op_Implicit(PlayerRoomTrackerSystem.sAudioStateFalse));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(PlayerRoomTrackerSystem.sAudioParameterPressure, 0f));
			Main.EventSystem.Post(SetRTPCEvent.GetGlobalAndMasterEvent(FogUpdateSystem.sAudioParameterDirtyAir, 0f));
			AkSoundEngine.SetState(WwiseShortID.op_Implicit(PlayerRoomTrackerSystem.sAudioStateRoomType), WwiseShortID.op_Implicit(PlayerRoomTrackerSystem.sAudioStateNone));
		}

		private void OnCheckpoint(CheckpointEvent ev)
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			mCurrentCheckpoint = ev.Checkpoint;
			if (EntityBlueprintComponent.IsValid(mEntityBlueprintComponent))
			{
				RespawnLocation respawnLocation = default(RespawnLocation);
				respawnLocation.Position = float3.op_Implicit(((Object)(object)ev.Checkpoint != (Object)null) ? ev.Checkpoint.get_position() : mSpawnedPosition);
				respawnLocation.Rotation = quaternion.op_Implicit(((Object)(object)ev.Checkpoint != (Object)null) ? ev.Checkpoint.get_rotation() : mSpawnedRotation);
				RespawnLocation respawnLocation2 = respawnLocation;
				EntityManager entityManager = mEntityBlueprintComponent.EntityManager;
				if (((EntityManager)(ref entityManager)).HasComponent<RespawnPending>(mEntityBlueprintComponent.Entity))
				{
					entityManager = mEntityBlueprintComponent.EntityManager;
					((EntityManager)(ref entityManager)).SetComponentData<RespawnLocation>(mEntityBlueprintComponent.Entity, respawnLocation2);
				}
				else
				{
					entityManager = mEntityBlueprintComponent.EntityManager;
					((EntityManager)(ref entityManager)).SetComponentData<RespawnLocation>(mEntityBlueprintComponent.Entity, respawnLocation2);
				}
			}
		}

		private void OnOxygenRegenUnlocked(UnlockAbilityEvent ev)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			if (ev.Ability == UnlockAbilityID.OxygenTankRecharge && EntityBlueprintComponent.IsValid(mEntityBlueprintComponent))
			{
				EntityManager entityManager = mEntityBlueprintComponent.EntityManager;
				if (!((EntityManager)(ref entityManager)).HasComponent<UnlockOxygenRegenComponent>(Entity))
				{
					entityManager = mEntityBlueprintComponent.EntityManager;
					((EntityManager)(ref entityManager)).AddComponentData<UnlockOxygenRegenComponent>(mEntityBlueprintComponent.Entity, default(UnlockOxygenRegenComponent));
				}
			}
		}

		private void OnRespawned(RespawnEvent ev)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (EntityBlueprintComponent.IsValid(mEntityBlueprintComponent))
			{
				EntityManager entityManager = mEntityBlueprintComponent.EntityManager;
				if (!((EntityManager)(ref entityManager)).HasComponent<RespawnPending>(mEntityBlueprintComponent.Entity))
				{
					entityManager = mEntityBlueprintComponent.EntityManager;
					((EntityManager)(ref entityManager)).AddComponentData<RespawnPending>(mEntityBlueprintComponent.Entity, default(RespawnPending));
					bool enabled = SceneLoader.Instance.LastLoadedLevelData.SessionType == GameSession.SessionType.FreeMode;
					SetGodMode(mEntityBlueprintComponent.Entity, mEntityBlueprintComponent.EntityManager, enabled);
				}
			}
			if ((Object)(object)mPlayerRigidbody != (Object)null)
			{
				mPlayerRigidbody.set_interpolation((RigidbodyInterpolation)0);
				mPlayerRigidbody.set_collisionDetectionMode((CollisionDetectionMode)0);
				mPlayerRigidbody.set_isKinematic(true);
			}
			((MonoBehaviour)this).StartCoroutine(WaitToRespawnPlayer());
			ResetAudioValues();
		}

		private IEnumerator WaitToRespawnPlayer()
		{
			yield return (object)new WaitForFixedUpdate();
			if ((Object)(object)mPlayerRigidbody != (Object)null)
			{
				mPlayerRigidbody.set_velocity(Vector3.get_zero());
				mPlayerRigidbody.set_angularVelocity(Vector3.get_zero());
				mPlayerRigidbody.set_isKinematic(false);
				mPlayerRigidbody.set_interpolation((RigidbodyInterpolation)1);
				mPlayerRigidbody.set_collisionDetectionMode((CollisionDetectionMode)2);
			}
		}

		public static PlayerMotion FindPlayerMotion(Player player)
		{
			if ((Object)(object)player == (Object)null)
			{
				return null;
			}
			return ((Component)player).GetComponent<PlayerMotion>();
		}

		private static Collider FindPlayerCollider(Player player)
		{
			if ((Object)(object)player == (Object)null)
			{
				return null;
			}
			Collider val = null;
			SphereCollider[] componentsInChildren = ((Component)player).GetComponentsInChildren<SphereCollider>();
			if (componentsInChildren != null)
			{
				SphereCollider[] array = componentsInChildren;
				foreach (SphereCollider val2 in array)
				{
					if (((Component)val2).CompareTag("Player"))
					{
						val = (Collider)(object)val2;
						break;
					}
				}
			}
			if ((Object)(object)val == (Object)null)
			{
				CapsuleCollider[] componentsInChildren2 = ((Component)player).GetComponentsInChildren<CapsuleCollider>();
				if (componentsInChildren2 != null)
				{
					CapsuleCollider[] array2 = componentsInChildren2;
					foreach (CapsuleCollider val3 in array2)
					{
						if (((Component)val3).CompareTag("Player"))
						{
							val = (Collider)(object)val3;
							break;
						}
					}
				}
			}
			return val;
		}

		public static void SetNoClipMode(Collider collider, PlayerMotion playerMotion, bool enabled)
		{
			if (GlobalOptions.Raw.GetBool("General.NoClipMode") && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				collider.set_isTrigger(enabled);
				playerMotion.SetSquishyCollide(!enabled);
			}
		}

		public static void SetGodMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (GlobalOptions.Raw.GetBool("General.GodMode") && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				((EntityManager)(ref entityManager)).AddComponentData<Invulnerable>(entity, default(Invulnerable));
			}
			((EntityManager)(ref entityManager)).RemoveComponent<Invulnerable>(entity);
		}

		public static void SetTroutMode(Entity entity, EntityManager entityManager, bool enabled)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			if (GlobalOptions.Raw.GetBool("General.TroutMode") && SceneLoader.Instance.LastLoadedLevelData.SessionType != GameSession.SessionType.WeeklyShip)
			{
				((EntityManager)(ref entityManager)).RemoveComponent<ReceiveForceOnDecompression>(entity);
			}
			((EntityManager)(ref entityManager)).AddComponentData<ReceiveForceOnDecompression>(entity, default(ReceiveForceOnDecompression));
		}

		public Player()
			: this()
		{
		}//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)

	}
}
