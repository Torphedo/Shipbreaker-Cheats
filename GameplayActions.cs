using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

namespace BBI.Unity.Game
{
	public class GameplayActions : PlayerActionSet
	{
		[Serializable]
		public struct GameplayBindings
		{
			public GameplayActionSet Action;

			public InputControlType PCControl;

			public InputControlType PS4Control;

			public InputControlType XboxControl;

			public Key[] Keyboard;

			public Mouse Mouse;

			public GameplayBindings(GameplayActionSet action, InputControlType pcControl = InputControlType.None, Key[] keyboard = null, Mouse mouse = Mouse.None, InputControlType ps4Control = InputControlType.None, InputControlType xboxControl = InputControlType.None)
			{
				Action = action;
				PCControl = pcControl;
				PS4Control = ((ps4Control == InputControlType.None) ? pcControl : ps4Control);
				XboxControl = ((xboxControl == InputControlType.None) ? pcControl : xboxControl);
				Keyboard = ((keyboard == null || keyboard.Length == 0) ? new Key[1] : keyboard);
				Mouse = mouse;
			}
		}

		[Serializable]
		public enum GameplayActionSet
		{
			None = 0,
			GrappleFire = 10,
			RetractionModifier = 11,
			GrappleCancel = 13,
			PlaceTether = 14,
			RetractTethers = 0xF,
			RecallTethers = 0x10,
			CancelTether = 17,
			GrappleAltFire = 18,
			UnequipCurrentEquipment = 19,
			LeftHandGrab = 20,
			RightHandGrab = 30,
			ReleaseGrab = 35,
			Interact = 40,
			Pull = 50,
			Push = 60,
			Throw = 70,
			Reorient = 80,
			Start = 90,
			ReturnToMenu = 100,
			Rewind = 110,
			Pause = 120,
			ChangeEquipment = 130,
			RollBodyLeft = 140,
			ModifiedRollBodyLeft = 141,
			RollBodyRight = 150,
			ModifiedRollBodyRight = 151,
			ModifiedRoll = 152,
			ThrustMoveForward = 160,
			ThrustMoveBackward = 170,
			ThrustBrakeLeft = 171,
			ThrustBrakeRight = 172,
			InvertAxes = 180,
			GlassMode = 190,
			ActivateScanner = 200,
			ScanObject = 201,
			SelectCutter = 210,
			ClearTethers = 220,
			ToggleFlashlight = 230,
			SelectGrapple = 240,
			EquipmentSelectionExtra01 = 241,
			RotateBodyUp = 250,
			RotateBodyDown = 260,
			RotateBodyLeft = 270,
			RotateBodyRight = 280,
			ThrustMoveUp = 290,
			ThrustMoveDown = 300,
			ThrustMoveLeft = 310,
			ThrustMoveRight = 320,
			LateralMoveUp = 330,
			LateralMoveDown = 340,
			LateralMoveLeft = 350,
			LateralMoveRight = 360,
			ToggleFramerate = 370,
			CutterFire = 380,
			CancelCut = 400,
			CutterAltFire = 510,
			RotateGrappledObjectLeft = 520,
			RotateGrappledObjectRight = 530,
			SwingLeft = 540,
			SwingRight = 550,
			SwingUp = 560,
			SwingDown = 570,
			ScanCycleLeft = 580,
			ScanCycleRight = 590,
			WorkOrder = 600,
			PlayAudioLog = 610,
			ToolMenu = 700,
			ToolMode = 710,
			ToolNavUp = 720,
			ToolNavDown = 730,
			ToolNavRight = 740,
			ToolNavLeft = 750,
			SelectDemoCharge = 760,
			DemoChargeFire = 770,
			DemoChargeAltFire = 780,
			Decline = 790,
			ToggleStickyGrab = 10000,
			Flip = 10010,
			Zoom = 10020,
			ToggleObjectives = 10030,
			DataLog = 10040,
			CycleEquipmentMode = 10050,
			EquipmentSpecial = 10060,
			RotateHeadUp = 10070,
			RotateHeadDown = 10080,
			RotateHeadLeft = 10090,
			RotateHeadRight = 10100,
			ToggleObjectDebugInfo = 20130,
			DebugIncrementTimeScale = 20140,
			DebugDecrementTimeScale = 20150,
			DebugResetTimeScale = 20160,
			DebugPauseTimeScale = 20170,
			DebugRefillOxygen = 20180,
			DebugRefillThrusters = 20190,
			ShowDebugControls = 20210,
			DebugSaveGame = 20220,
			DebugLoadGame = 20230,
			DebugMegaCutPlayer = 20240,
			DebugMegaCutAll = 20250,
			ToggleDebugMenu = 20260,
			ToggleBuildInfo = 20270
		}

		public PlayerAction LeftHandGrab;

		public PlayerAction RightHandGrab;

		public PlayerAction ReleaseGrab;

		public PlayerAction Interact;

		public PlayerAction Lunge;

		public PlayerAction Push;

		public PlayerAction Throw;

		public PlayerAction ActivateScanner;

		public PlayerAction ScanObject;

		public PlayerAction ScanCycleLeft;

		public PlayerAction ScanCycleRight;

		public PlayerAction Start;

		public PlayerAction ReturnToMenu;

		public PlayerAction Pause;

		public PlayerAction InvertAxes;

		public PlayerAction GlassMode;

		public PlayerAction ToggleFramerate;

		public PlayerAction WorkOrder;

		public PlayerAction PlayAudioLog;

		public PlayerAction Decline;

		public PlayerAction SelectCutter;

		public PlayerAction ClearTethers;

		public PlayerAction ToggleFlashlight;

		public PlayerAction SelectGrapple;

		public PlayerAction EquipmentSelectionExtra01;

		public PlayerAction UnequipCurrentEquipment;

		public PlayerAction SelectDemoCharge;

		public PlayerAction DemoChargeFire;

		public PlayerAction DemoChargeAltFire;

		public PlayerAction GrappleFire;

		public PlayerAction ChangeEquipment;

		public PlayerAction RetractionModifier;

		public PlayerAction ExtensionModifier;

		public PlayerAction GrappleCancel;

		public PlayerAction SwitchGrappleMode;

		public PlayerAction RotateGrappledObjectLeft;

		public PlayerAction RotateGrappledObjectRight;

		public PlayerAction SwingLeft;

		public PlayerAction SwingRight;

		public PlayerAction SwingUp;

		public PlayerAction SwingDown;

		public PlayerOneAxisAction SwingHorizontal;

		public PlayerOneAxisAction SwingVertical;

		public PlayerAction PlaceTether;

		public PlayerAction RetractTethers;

		public PlayerAction RecallTethers;

		public PlayerAction CancelTether;

		public PlayerAction Reorient;

		public PlayerAction RotateBodyUp;

		public PlayerAction RotateBodyDown;

		public PlayerAction RotateBodyLeft;

		public PlayerAction RotateBodyRight;

		public PlayerTwoAxisAction RotateBody;

		public PlayerAction RollBodyLeft;

		public PlayerAction RollBodyRight;

		private PlayerAction mModifiedRollBodyLeft;

		private PlayerAction mModifiedRollBodyRight;

		public PlayerAction ModifedRoll;

		public PlayerOneAxisAction ModifiedRollBody;

		public PlayerAction ThrustBrakeLeft;

		public PlayerAction ThrustBrakeRight;

		public PlayerAction ThrustMoveForward;

		public PlayerAction ThrustMoveBackward;

		public PlayerOneAxisAction ThrustDepth;

		public PlayerAction ThrustMoveUp;

		public PlayerAction ThrustMoveDown;

		public PlayerOneAxisAction ThrustVertical;

		public PlayerAction ThrustMoveLeft;

		public PlayerAction ThrustMoveRight;

		public PlayerOneAxisAction ThrustHorizontal;

		public PlayerAction LateralMoveUp;

		public PlayerAction LateralMoveDown;

		public PlayerAction LateralMoveLeft;

		public PlayerAction LateralMoveRight;

		public PlayerTwoAxisAction LateralMove;

		public PlayerAction CutterFire;

		public PlayerAction CancelCut;

		public PlayerAction CutterAltFire;

		public PlayerAction ToolMenu;

		public PlayerAction ToolMode;

		public PlayerAction ToolNavUp;

		public PlayerAction ToolNavDown;

		public PlayerAction ToolNavRight;

		public PlayerAction ToolNavLeft;

		public PlayerAction ToggleStickyGrab;

		public PlayerAction Flip;

		public PlayerAction Zoom;

		public PlayerAction ToggleObjectives;

		public PlayerAction DataLog;

		public PlayerAction CycleEquipmentMode;

		public PlayerAction EquipmentSpecial;

		private PlayerAction mRotateHeadUp;

		private PlayerAction mRotateHeadDown;

		private PlayerAction mRotateHeadLeft;

		private PlayerAction mRotateHeadRight;

		public PlayerTwoAxisAction RotateHead;

		public PlayerAction ToggleObjectDebugInfo;

		public PlayerAction DebugIncrementTimeScale;

		public PlayerAction DebugDecrementTimeScale;

		public PlayerAction DebugResetTimeScale;

		public PlayerAction DebugPauseTimeScale;

		public PlayerAction DebugRefillOxygen;

		public PlayerAction DebugRefillThrusters;

		public PlayerAction ShowDebugControls;

		public PlayerAction ToggleDebugMenu;

		public PlayerAction ToggleBuildInfo;

		public PlayerAction DebugSaveGame;

		public PlayerAction DebugLoadGame;

		public PlayerAction DebugMegaCutPlayer;

		public PlayerAction DebugMegaCutAll;

		private List<GameplayBindings> mDefaultBindings = new List<GameplayBindings>();

		private List<GameplayBindings> mLoadedBindings = new List<GameplayBindings>();

		private const float kButtonHoldThresholdInSeconds = 0.4f;

		public float ButtonHoldThreshold => 0.4f;

		public GameplayActions(bool enableAZERTY)
		{
			LoadBindings(GameplayActionSet.ThrustMoveForward);
			LoadBindings(GameplayActionSet.ThrustMoveBackward);
			LoadBindings(GameplayActionSet.ThrustMoveLeft);
			LoadBindings(GameplayActionSet.ThrustMoveRight);
			LoadBindings(GameplayActionSet.ThrustMoveUp);
			LoadBindings(GameplayActionSet.ThrustMoveDown);
			LoadBindings(GameplayActionSet.ThrustBrakeLeft);
			LoadBindings(GameplayActionSet.ThrustBrakeRight);
			LoadBindings(GameplayActionSet.RollBodyLeft);
			LoadBindings(GameplayActionSet.RollBodyRight);
			LoadBindings(GameplayActionSet.LateralMoveUp);
			LoadBindings(GameplayActionSet.LateralMoveDown);
			LoadBindings(GameplayActionSet.LateralMoveRight);
			LoadBindings(GameplayActionSet.LateralMoveLeft);
			LoadBindings(GameplayActionSet.Pull);
			LoadBindings(GameplayActionSet.Push);
			LoadBindings(GameplayActionSet.ActivateScanner);
			LoadBindings(GameplayActionSet.ScanCycleLeft);
			LoadBindings(GameplayActionSet.ScanCycleRight);
			LoadBindings(GameplayActionSet.SelectGrapple);
			LoadBindings(GameplayActionSet.GrappleFire);
			LoadBindings(GameplayActionSet.RetractionModifier);
			LoadBindings(GameplayActionSet.SwingUp);
			LoadBindings(GameplayActionSet.SwingDown);
			LoadBindings(GameplayActionSet.SwingRight);
			LoadBindings(GameplayActionSet.SwingLeft);
			LoadBindings(GameplayActionSet.Throw);
			LoadBindings(GameplayActionSet.PlaceTether);
			LoadBindings(GameplayActionSet.RecallTethers);
			LoadBindings(GameplayActionSet.SelectCutter);
			LoadBindings(GameplayActionSet.CutterFire);
			LoadBindings(GameplayActionSet.CutterAltFire);
			LoadBindings(GameplayActionSet.SelectDemoCharge);
			LoadBindings(GameplayActionSet.DemoChargeFire);
			LoadBindings(GameplayActionSet.DemoChargeAltFire);
			LoadBindings(GameplayActionSet.Interact);
			LoadBindings(GameplayActionSet.ToggleFlashlight);
			LoadBindings(GameplayActionSet.Pause);
			LoadBindings(GameplayActionSet.WorkOrder);
			LoadBindings(GameplayActionSet.RightHandGrab);
			LoadBindings(GameplayActionSet.LeftHandGrab);
			LoadBindings(GameplayActionSet.GlassMode);
			LoadBindings(GameplayActionSet.PlayAudioLog);
			LoadBindings(GameplayActionSet.Decline);
			LoadBindings(GameplayActionSet.Start);
			LoadBindings(GameplayActionSet.ToolMenu);
			LoadBindings(GameplayActionSet.ToolMode);
			LoadBindings(GameplayActionSet.ToolNavUp);
			LoadBindings(GameplayActionSet.ToolNavDown);
			LoadBindings(GameplayActionSet.ToolNavRight);
			LoadBindings(GameplayActionSet.ToolNavLeft);
			LoadBindings(GameplayActionSet.RotateBodyUp);
			LoadBindings(GameplayActionSet.RotateBodyDown);
			LoadBindings(GameplayActionSet.RotateBodyRight);
			LoadBindings(GameplayActionSet.RotateBodyLeft);
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleFramerate, InputControlType.None, new Key[1] { Key.F4 }));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.EquipmentSelectionExtra01, InputControlType.Action4, new Key[1] { Key.Tab }));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Flip));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Zoom));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleStickyGrab));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleObjectives));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DataLog));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadUp));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadDown));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadLeft));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadRight));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CycleEquipmentMode));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.EquipmentSpecial));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ReleaseGrab));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.GrappleCancel));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RetractTethers));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CancelTether));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ChangeEquipment));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRollBodyLeft));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRollBodyRight));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRoll));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Reorient));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CancelCut));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ClearTethers, InputControlType.DPadDown, new Key[1] { Key.Key3 }));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.UnequipCurrentEquipment));
			if (enableAZERTY)
			{
				for (int i = 0; i < mDefaultBindings.Count; i++)
				{
					GameplayBindings value = mDefaultBindings[i];
					Key[] keyboard = value.Keyboard;
					LynxControls.ToggleAZERTY(keyboard);
					value.Keyboard = keyboard;
					mDefaultBindings[i] = value;
				}
			}
			RightHandGrab = CreatePlayerAction(GameplayActionSet.RightHandGrab.ToString());
			LeftHandGrab = CreatePlayerAction(GameplayActionSet.LeftHandGrab.ToString());
			ReleaseGrab = CreatePlayerAction(GameplayActionSet.ReleaseGrab.ToString());
			Interact = CreatePlayerAction(GameplayActionSet.Interact.ToString());
			Lunge = CreatePlayerAction(GameplayActionSet.Pull.ToString());
			Push = CreatePlayerAction(GameplayActionSet.Push.ToString());
			Throw = CreatePlayerAction(GameplayActionSet.Throw.ToString());
			SwingLeft = CreatePlayerAction(GameplayActionSet.SwingLeft.ToString());
			SwingRight = CreatePlayerAction(GameplayActionSet.SwingRight.ToString());
			SwingUp = CreatePlayerAction(GameplayActionSet.SwingUp.ToString());
			SwingDown = CreatePlayerAction(GameplayActionSet.SwingDown.ToString());
			SwingHorizontal = CreateOneAxisPlayerAction(SwingLeft, SwingRight);
			SwingVertical = CreateOneAxisPlayerAction(SwingDown, SwingUp);
			ActivateScanner = CreatePlayerAction(GameplayActionSet.ActivateScanner.ToString());
			ScanObject = CreatePlayerAction(GameplayActionSet.ScanObject.ToString());
			ScanCycleLeft = CreatePlayerAction(GameplayActionSet.ScanCycleLeft.ToString());
			ScanCycleRight = CreatePlayerAction(GameplayActionSet.ScanCycleRight.ToString());
			GrappleFire = CreatePlayerAction(GameplayActionSet.GrappleFire.ToString());
			ChangeEquipment = CreatePlayerAction(GameplayActionSet.ChangeEquipment.ToString());
			RetractionModifier = CreatePlayerAction(GameplayActionSet.RetractionModifier.ToString());
			GrappleCancel = CreatePlayerAction(GameplayActionSet.GrappleCancel.ToString());
			UnequipCurrentEquipment = CreatePlayerAction(GameplayActionSet.UnequipCurrentEquipment.ToString());
			SelectCutter = CreatePlayerAction(GameplayActionSet.SelectCutter.ToString());
			ClearTethers = CreatePlayerAction(GameplayActionSet.ClearTethers.ToString());
			ToggleFlashlight = CreatePlayerAction(GameplayActionSet.ToggleFlashlight.ToString());
			SelectGrapple = CreatePlayerAction(GameplayActionSet.SelectGrapple.ToString());
			EquipmentSelectionExtra01 = CreatePlayerAction(GameplayActionSet.EquipmentSelectionExtra01.ToString());
			SelectDemoCharge = CreatePlayerAction(GameplayActionSet.SelectDemoCharge.ToString());
			DemoChargeFire = CreatePlayerAction(GameplayActionSet.DemoChargeFire.ToString());
			DemoChargeAltFire = CreatePlayerAction(GameplayActionSet.DemoChargeAltFire.ToString());
			PlaceTether = CreatePlayerAction(GameplayActionSet.PlaceTether.ToString());
			RetractTethers = CreatePlayerAction(GameplayActionSet.RetractTethers.ToString());
			RecallTethers = CreatePlayerAction(GameplayActionSet.RecallTethers.ToString());
			CancelTether = CreatePlayerAction(GameplayActionSet.CancelTether.ToString());
			Start = CreatePlayerAction(GameplayActionSet.Start.ToString());
			ReturnToMenu = CreatePlayerAction(GameplayActionSet.ReturnToMenu.ToString());
			Pause = CreatePlayerAction(GameplayActionSet.Pause.ToString());
			InvertAxes = CreatePlayerAction(GameplayActionSet.InvertAxes.ToString());
			GlassMode = CreatePlayerAction(GameplayActionSet.GlassMode.ToString());
			ToggleFramerate = CreatePlayerAction(GameplayActionSet.ToggleFramerate.ToString());
			WorkOrder = CreatePlayerAction(GameplayActionSet.WorkOrder.ToString());
			PlayAudioLog = CreatePlayerAction(GameplayActionSet.PlayAudioLog.ToString());
			Decline = CreatePlayerAction(GameplayActionSet.Decline.ToString());
			RollBodyLeft = CreatePlayerAction(GameplayActionSet.RollBodyLeft.ToString());
			RollBodyRight = CreatePlayerAction(GameplayActionSet.RollBodyRight.ToString());
			mModifiedRollBodyLeft = CreatePlayerAction(GameplayActionSet.ModifiedRollBodyLeft.ToString());
			mModifiedRollBodyRight = CreatePlayerAction(GameplayActionSet.ModifiedRollBodyRight.ToString());
			ModifiedRollBody = CreateOneAxisPlayerAction(mModifiedRollBodyLeft, mModifiedRollBodyRight);
			ModifedRoll = CreatePlayerAction(GameplayActionSet.ModifiedRoll.ToString());
			RotateBodyUp = CreatePlayerAction(GameplayActionSet.RotateBodyUp.ToString());
			RotateBodyDown = CreatePlayerAction(GameplayActionSet.RotateBodyDown.ToString());
			RotateBodyLeft = CreatePlayerAction(GameplayActionSet.RotateBodyLeft.ToString());
			RotateBodyRight = CreatePlayerAction(GameplayActionSet.RotateBodyRight.ToString());
			RotateBody = CreateTwoAxisPlayerAction(RotateBodyLeft, RotateBodyRight, RotateBodyDown, RotateBodyUp);
			Reorient = CreatePlayerAction(GameplayActionSet.Reorient.ToString());
			LateralMoveUp = CreatePlayerAction(GameplayActionSet.LateralMoveUp.ToString());
			LateralMoveDown = CreatePlayerAction(GameplayActionSet.LateralMoveDown.ToString());
			LateralMoveLeft = CreatePlayerAction(GameplayActionSet.LateralMoveLeft.ToString());
			LateralMoveRight = CreatePlayerAction(GameplayActionSet.LateralMoveRight.ToString());
			LateralMove = CreateTwoAxisPlayerAction(LateralMoveLeft, LateralMoveRight, LateralMoveDown, LateralMoveUp);
			ThrustMoveForward = CreatePlayerAction(GameplayActionSet.ThrustMoveForward.ToString());
			ThrustMoveBackward = CreatePlayerAction(GameplayActionSet.ThrustMoveBackward.ToString());
			ThrustMoveUp = CreatePlayerAction(GameplayActionSet.ThrustMoveUp.ToString());
			ThrustMoveDown = CreatePlayerAction(GameplayActionSet.ThrustMoveDown.ToString());
			ThrustMoveLeft = CreatePlayerAction(GameplayActionSet.ThrustMoveLeft.ToString());
			ThrustMoveRight = CreatePlayerAction(GameplayActionSet.ThrustMoveRight.ToString());
			ThrustDepth = CreateOneAxisPlayerAction(ThrustMoveBackward, ThrustMoveForward);
			ThrustVertical = CreateOneAxisPlayerAction(ThrustMoveDown, ThrustMoveUp);
			ThrustHorizontal = CreateOneAxisPlayerAction(ThrustMoveLeft, ThrustMoveRight);
			ThrustBrakeLeft = CreatePlayerAction(GameplayActionSet.ThrustBrakeLeft.ToString());
			ThrustBrakeRight = CreatePlayerAction(GameplayActionSet.ThrustBrakeRight.ToString());
			CutterFire = CreatePlayerAction(GameplayActionSet.CutterFire.ToString());
			CutterAltFire = CreatePlayerAction(GameplayActionSet.CutterAltFire.ToString());
			CancelCut = CreatePlayerAction(GameplayActionSet.CancelCut.ToString());
			ToolMenu = CreatePlayerAction(GameplayActionSet.ToolMenu.ToString());
			ToolMode = CreatePlayerAction(GameplayActionSet.ToolMode.ToString());
			ToolNavUp = CreatePlayerAction(GameplayActionSet.ToolNavUp.ToString());
			ToolNavDown = CreatePlayerAction(GameplayActionSet.ToolNavDown.ToString());
			ToolNavRight = CreatePlayerAction(GameplayActionSet.ToolNavRight.ToString());
			ToolNavLeft = CreatePlayerAction(GameplayActionSet.ToolNavLeft.ToString());
			Flip = CreatePlayerAction(GameplayActionSet.Flip.ToString());
			Zoom = CreatePlayerAction(GameplayActionSet.Zoom.ToString());
			ToggleObjectives = CreatePlayerAction(GameplayActionSet.ToggleObjectives.ToString());
			DataLog = CreatePlayerAction(GameplayActionSet.DataLog.ToString());
			CycleEquipmentMode = CreatePlayerAction(GameplayActionSet.CycleEquipmentMode.ToString());
			EquipmentSpecial = CreatePlayerAction(GameplayActionSet.EquipmentSpecial.ToString());
			ToggleStickyGrab = CreatePlayerAction(GameplayActionSet.ToggleStickyGrab.ToString());
			mRotateHeadUp = CreatePlayerAction(GameplayActionSet.RotateHeadUp.ToString());
			mRotateHeadDown = CreatePlayerAction(GameplayActionSet.RotateHeadDown.ToString());
			mRotateHeadLeft = CreatePlayerAction(GameplayActionSet.RotateHeadLeft.ToString());
			mRotateHeadRight = CreatePlayerAction(GameplayActionSet.RotateHeadRight.ToString());
			RotateHead = CreateTwoAxisPlayerAction(mRotateHeadLeft, mRotateHeadRight, mRotateHeadDown, mRotateHeadUp);
			ToggleObjectDebugInfo = CreatePlayerAction(GameplayActionSet.ToggleObjectDebugInfo.ToString());
			DebugIncrementTimeScale = CreatePlayerAction(GameplayActionSet.DebugIncrementTimeScale.ToString());
			DebugDecrementTimeScale = CreatePlayerAction(GameplayActionSet.DebugDecrementTimeScale.ToString());
			DebugResetTimeScale = CreatePlayerAction(GameplayActionSet.DebugResetTimeScale.ToString());
			DebugPauseTimeScale = CreatePlayerAction(GameplayActionSet.DebugPauseTimeScale.ToString());
			DebugRefillOxygen = CreatePlayerAction(GameplayActionSet.DebugRefillOxygen.ToString());
			DebugRefillThrusters = CreatePlayerAction(GameplayActionSet.DebugRefillThrusters.ToString());
			ShowDebugControls = CreatePlayerAction(GameplayActionSet.ShowDebugControls.ToString());
			DebugSaveGame = CreatePlayerAction(GameplayActionSet.DebugSaveGame.ToString());
			DebugLoadGame = CreatePlayerAction(GameplayActionSet.DebugLoadGame.ToString());
			DebugMegaCutPlayer = CreatePlayerAction(GameplayActionSet.DebugMegaCutPlayer.ToString());
			DebugMegaCutAll = CreatePlayerAction(GameplayActionSet.DebugMegaCutAll.ToString());
			ToggleDebugMenu = CreatePlayerAction(GameplayActionSet.ToggleDebugMenu.ToString());
			ToggleBuildInfo = CreatePlayerAction(GameplayActionSet.ToggleBuildInfo.ToString());
		}

		public static GameplayActions CreateWithDefaultBindings(bool enableAZERTY)
		{
			GameplayActions gameplayActions = new GameplayActions(enableAZERTY);
			gameplayActions.AddDefaultBindings();
			gameplayActions.AddLoadedBindings();
			gameplayActions.ListenOptions.IncludeUnknownControllers = true;
			gameplayActions.ListenOptions.OnBindingFound = delegate(PlayerAction action, BindingSource binding)
			{
				if (binding == new KeyBindingSource(Key.Escape))
				{
					action.StopListeningForBinding();
					return false;
				}
				return true;
			};
			BindingListenOptions bindingListenOptions = gameplayActions.ListenOptions;
			bindingListenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(bindingListenOptions.OnBindingAdded, (Action<PlayerAction, BindingSource>)delegate(PlayerAction action, BindingSource binding)
			{
				Debug.Log("Binding added... " + binding.DeviceName + ": " + binding.Name);
			});
			BindingListenOptions bindingListenOptions2 = gameplayActions.ListenOptions;
			bindingListenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(bindingListenOptions2.OnBindingRejected, (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)delegate(PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
			{
				Debug.Log("Binding rejected... " + reason);
			});
			return gameplayActions;
		}

		public void AddDefaultBindings()
		{
			for (int i = 0; i < mDefaultBindings.Count; i++)
			{
				string text = mDefaultBindings[i].Action.ToString();
				if (IsValidAction(text))
				{
					AddBinding(base[text], mDefaultBindings[i].PCControl, mDefaultBindings[i].PS4Control, mDefaultBindings[i].XboxControl, mDefaultBindings[i].Mouse, mDefaultBindings[i].Keyboard, isDefaultBinding: true);
				}
			}
		}

		public void AddLoadedBindings()
		{
			for (int i = 0; i < mLoadedBindings.Count; i++)
			{
				string text = mLoadedBindings[i].Action.ToString();
				if (!IsValidAction(text))
				{
					continue;
				}
				PlayerAction playerAction = base[text];
				GameplayBindings gameplayBindings = mLoadedBindings[i];
				if (gameplayBindings.PCControl != 0)
				{
					if (gameplayBindings.PCControl == InputControlType.Count)
					{
						playerAction.AddDefaultBinding(InputControlType.None);
					}
					else
					{
						playerAction.AddDefaultBinding(gameplayBindings.PCControl);
					}
				}
				else if (gameplayBindings.PS4Control != 0)
				{
					playerAction.AddDefaultBinding(gameplayBindings.PS4Control);
				}
				else if (gameplayBindings.XboxControl != 0)
				{
					playerAction.AddDefaultBinding(gameplayBindings.XboxControl);
				}
				else if (gameplayBindings.Mouse != 0)
				{
					playerAction.AddDefaultBinding(gameplayBindings.Mouse);
				}
				else
				{
					playerAction.AddDefaultBinding(gameplayBindings.Keyboard);
				}
			}
		}

		public void AddBinding(PlayerAction action, InputControlType pc, InputControlType ps4, InputControlType xbox, Mouse mouse, Key[] keyboard, bool isDefaultBinding)
		{
			if (isDefaultBinding)
			{
				if (pc != 0)
				{
					action.AddDefaultBinding(pc);
				}
				if (ps4 != 0)
				{
					action.AddDefaultBinding(ps4);
				}
				if (xbox != 0)
				{
					action.AddDefaultBinding(xbox);
				}
				if (mouse != 0)
				{
					action.AddDefaultBinding(mouse);
				}
				if (keyboard != null)
				{
					action.AddDefaultBinding(keyboard);
				}
			}
			else
			{
				if (pc != 0)
				{
					action.AddBinding(new DeviceBindingSource(pc));
				}
				if (ps4 != 0)
				{
					action.AddBinding(new DeviceBindingSource(ps4));
				}
				if (xbox != 0)
				{
					action.AddBinding(new DeviceBindingSource(xbox));
				}
				if (mouse != 0)
				{
					action.AddBinding(new MouseBindingSource(mouse));
				}
				if (keyboard != null)
				{
					action.AddBinding(new KeyBindingSource(keyboard));
				}
			}
		}

		public void RestoreDefaultBindings()
		{
			for (int i = 0; i < mDefaultBindings.Count; i++)
			{
				string text = mDefaultBindings[i].Action.ToString();
				if (IsValidAction(text))
				{
					base[text].ResetBindings();
				}
			}
		}

		public void UpdateBindings(List<GameplayBindings> newBindings)
		{
			if (newBindings == null)
			{
				return;
			}
			for (int i = 0; i < newBindings.Count; i++)
			{
				string text = newBindings[i].Action.ToString();
				GameplayBindings gameplayBindings = newBindings[i];
				if (IsValidAction(text))
				{
					base[text].ClearBindings();
					AddBinding(base[text], gameplayBindings.PCControl, gameplayBindings.PS4Control, gameplayBindings.XboxControl, gameplayBindings.Mouse, gameplayBindings.Keyboard, isDefaultBinding: false);
				}
				else
				{
					Debug.LogError("Unbindable Action: " + text + " is not a valid action.");
				}
			}
		}

		private bool IsValidAction(string action)
		{
			for (int i = 0; i < mDefaultBindings.Count; i++)
			{
				if (action.CompareTo(mDefaultBindings[i].Action.ToString()) == 0)
				{
					return true;
				}
			}
			for (int j = 0; j < mLoadedBindings.Count; j++)
			{
				if (action.CompareTo(mLoadedBindings[j].Action.ToString()) == 0)
				{
					return true;
				}
			}
			return false;
		}

		public void SetRotateBodySensitivity(float xSensitivity, float ySensitivity)
		{
			RotateBodyUp.Sensitivity = ySensitivity;
			RotateBodyDown.Sensitivity = ySensitivity;
			RotateBodyLeft.Sensitivity = xSensitivity;
			RotateBodyRight.Sensitivity = xSensitivity;
		}

		public void SetRotateHeadSensitivity(float xSensitivity, float ySensitivity)
		{
			mRotateHeadUp.Sensitivity = ySensitivity;
			mRotateHeadDown.Sensitivity = ySensitivity;
			mRotateHeadLeft.Sensitivity = xSensitivity;
			mRotateHeadRight.Sensitivity = xSensitivity;
		}

		public void LoadBindings(GameplayActionSet set)
		{
			foreach (GameplayBindings item in LynxConfigurableControlOptions.LoadGameplayBinding(set))
			{
				mLoadedBindings.Add(item);
			}
		}

		public bool ButtonHolding(PlayerAction action)
		{
			if (action.LastState && action.IsPressed)
			{
				return true;
			}
			return false;
		}
	}
}
