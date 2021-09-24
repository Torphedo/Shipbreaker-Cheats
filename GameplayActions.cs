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

			public GameplayBindings(GameplayActionSet action, InputControlType pcControl = 0, Key[] keyboard = null, Mouse mouse = 0, InputControlType ps4Control = 0, InputControlType xboxControl = 0)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				//IL_000f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_001e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0022: Unknown result type (might be due to invalid IL or missing references)
				//IL_0026: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				Action = action;
				PCControl = pcControl;
				PS4Control = (((int)ps4Control == 0) ? pcControl : ps4Control);
				XboxControl = (((int)xboxControl == 0) ? pcControl : xboxControl);
				Keyboard = (Key[])((keyboard == null || keyboard.Length == 0) ? ((Array)new Key[1]) : ((Array)keyboard));
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
			: this()
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
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleFramerate, (InputControlType)0, (Key[])(object)new Key[1] { (Key)17 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugRefillOxygen, (InputControlType)0, (Key[])(object)new Key[1] { (Key)14 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugRefillThrusters, (InputControlType)0, (Key[])(object)new Key[1] { (Key)15 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.GlassMode, (InputControlType)0, (Key[])(object)new Key[1] { (Key)16 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugMegaCutPlayer, (InputControlType)0, (Key[])(object)new Key[1] { (Key)18 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleDebugMenu, (InputControlType)0, (Key[])(object)new Key[1] { (Key)19 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleObjectDebugInfo, (InputControlType)0, (Key[])(object)new Key[1] { (Key)20 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ShowDebugControls, (InputControlType)0, (Key[])(object)new Key[1] { (Key)23 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugResetTimeScale, (InputControlType)0, (Key[])(object)new Key[1] { (Key)84 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugPauseTimeScale, (InputControlType)0, (Key[])(object)new Key[1] { (Key)83 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugIncrementTimeScale, (InputControlType)0, (Key[])(object)new Key[1] { (Key)85 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DebugDecrementTimeScale, (InputControlType)0, (Key[])(object)new Key[1] { (Key)86 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.EquipmentSelectionExtra01, (InputControlType)22, (Key[])(object)new Key[1] { (Key)66 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Flip, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Zoom, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleStickyGrab, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ToggleObjectives, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.DataLog, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadUp, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadDown, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadLeft, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RotateHeadRight, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CycleEquipmentMode, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.EquipmentSpecial, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ReleaseGrab, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.GrappleCancel, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.RetractTethers, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CancelTether, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ChangeEquipment, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRollBodyLeft, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRollBodyRight, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ModifiedRoll, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.Reorient, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.CancelCut, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.ClearTethers, (InputControlType)12, (Key[])(object)new Key[1] { (Key)29 }, (Mouse)0, (InputControlType)0, (InputControlType)0));
			mDefaultBindings.Add(new GameplayBindings(GameplayActionSet.UnequipCurrentEquipment, (InputControlType)0, null, (Mouse)0, (InputControlType)0, (InputControlType)0));
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
			RightHandGrab = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RightHandGrab.ToString());
			LeftHandGrab = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.LeftHandGrab.ToString());
			ReleaseGrab = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ReleaseGrab.ToString());
			Interact = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Interact.ToString());
			Lunge = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Pull.ToString());
			Push = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Push.ToString());
			Throw = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Throw.ToString());
			SwingLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SwingLeft.ToString());
			SwingRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SwingRight.ToString());
			SwingUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SwingUp.ToString());
			SwingDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SwingDown.ToString());
			SwingHorizontal = ((PlayerActionSet)this).CreateOneAxisPlayerAction(SwingLeft, SwingRight);
			SwingVertical = ((PlayerActionSet)this).CreateOneAxisPlayerAction(SwingDown, SwingUp);
			ActivateScanner = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ActivateScanner.ToString());
			ScanObject = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ScanObject.ToString());
			ScanCycleLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ScanCycleLeft.ToString());
			ScanCycleRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ScanCycleRight.ToString());
			GrappleFire = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.GrappleFire.ToString());
			ChangeEquipment = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ChangeEquipment.ToString());
			RetractionModifier = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RetractionModifier.ToString());
			GrappleCancel = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.GrappleCancel.ToString());
			UnequipCurrentEquipment = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.UnequipCurrentEquipment.ToString());
			SelectCutter = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SelectCutter.ToString());
			ClearTethers = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ClearTethers.ToString());
			ToggleFlashlight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleFlashlight.ToString());
			SelectGrapple = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SelectGrapple.ToString());
			EquipmentSelectionExtra01 = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.EquipmentSelectionExtra01.ToString());
			SelectDemoCharge = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.SelectDemoCharge.ToString());
			DemoChargeFire = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DemoChargeFire.ToString());
			DemoChargeAltFire = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DemoChargeAltFire.ToString());
			PlaceTether = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.PlaceTether.ToString());
			RetractTethers = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RetractTethers.ToString());
			RecallTethers = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RecallTethers.ToString());
			CancelTether = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.CancelTether.ToString());
			Start = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Start.ToString());
			ReturnToMenu = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ReturnToMenu.ToString());
			Pause = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Pause.ToString());
			InvertAxes = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.InvertAxes.ToString());
			GlassMode = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.GlassMode.ToString());
			ToggleFramerate = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleFramerate.ToString());
			WorkOrder = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.WorkOrder.ToString());
			PlayAudioLog = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.PlayAudioLog.ToString());
			Decline = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Decline.ToString());
			RollBodyLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RollBodyLeft.ToString());
			RollBodyRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RollBodyRight.ToString());
			mModifiedRollBodyLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ModifiedRollBodyLeft.ToString());
			mModifiedRollBodyRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ModifiedRollBodyRight.ToString());
			ModifiedRollBody = ((PlayerActionSet)this).CreateOneAxisPlayerAction(mModifiedRollBodyLeft, mModifiedRollBodyRight);
			ModifedRoll = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ModifiedRoll.ToString());
			RotateBodyUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateBodyUp.ToString());
			RotateBodyDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateBodyDown.ToString());
			RotateBodyLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateBodyLeft.ToString());
			RotateBodyRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateBodyRight.ToString());
			RotateBody = ((PlayerActionSet)this).CreateTwoAxisPlayerAction(RotateBodyLeft, RotateBodyRight, RotateBodyDown, RotateBodyUp);
			Reorient = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Reorient.ToString());
			LateralMoveUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.LateralMoveUp.ToString());
			LateralMoveDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.LateralMoveDown.ToString());
			LateralMoveLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.LateralMoveLeft.ToString());
			LateralMoveRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.LateralMoveRight.ToString());
			LateralMove = ((PlayerActionSet)this).CreateTwoAxisPlayerAction(LateralMoveLeft, LateralMoveRight, LateralMoveDown, LateralMoveUp);
			ThrustMoveForward = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveForward.ToString());
			ThrustMoveBackward = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveBackward.ToString());
			ThrustMoveUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveUp.ToString());
			ThrustMoveDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveDown.ToString());
			ThrustMoveLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveLeft.ToString());
			ThrustMoveRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustMoveRight.ToString());
			ThrustDepth = ((PlayerActionSet)this).CreateOneAxisPlayerAction(ThrustMoveBackward, ThrustMoveForward);
			ThrustVertical = ((PlayerActionSet)this).CreateOneAxisPlayerAction(ThrustMoveDown, ThrustMoveUp);
			ThrustHorizontal = ((PlayerActionSet)this).CreateOneAxisPlayerAction(ThrustMoveLeft, ThrustMoveRight);
			ThrustBrakeLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustBrakeLeft.ToString());
			ThrustBrakeRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ThrustBrakeRight.ToString());
			CutterFire = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.CutterFire.ToString());
			CutterAltFire = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.CutterAltFire.ToString());
			CancelCut = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.CancelCut.ToString());
			ToolMenu = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolMenu.ToString());
			ToolMode = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolMode.ToString());
			ToolNavUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolNavUp.ToString());
			ToolNavDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolNavDown.ToString());
			ToolNavRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolNavRight.ToString());
			ToolNavLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToolNavLeft.ToString());
			Flip = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Flip.ToString());
			Zoom = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.Zoom.ToString());
			ToggleObjectives = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleObjectives.ToString());
			DataLog = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DataLog.ToString());
			CycleEquipmentMode = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.CycleEquipmentMode.ToString());
			EquipmentSpecial = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.EquipmentSpecial.ToString());
			ToggleStickyGrab = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleStickyGrab.ToString());
			mRotateHeadUp = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateHeadUp.ToString());
			mRotateHeadDown = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateHeadDown.ToString());
			mRotateHeadLeft = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateHeadLeft.ToString());
			mRotateHeadRight = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.RotateHeadRight.ToString());
			RotateHead = ((PlayerActionSet)this).CreateTwoAxisPlayerAction(mRotateHeadLeft, mRotateHeadRight, mRotateHeadDown, mRotateHeadUp);
			ToggleObjectDebugInfo = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleObjectDebugInfo.ToString());
			DebugIncrementTimeScale = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugIncrementTimeScale.ToString());
			DebugDecrementTimeScale = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugDecrementTimeScale.ToString());
			DebugResetTimeScale = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugResetTimeScale.ToString());
			DebugPauseTimeScale = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugPauseTimeScale.ToString());
			DebugRefillOxygen = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugRefillOxygen.ToString());
			DebugRefillThrusters = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugRefillThrusters.ToString());
			ShowDebugControls = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ShowDebugControls.ToString());
			DebugSaveGame = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugSaveGame.ToString());
			DebugLoadGame = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugLoadGame.ToString());
			DebugMegaCutPlayer = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugMegaCutPlayer.ToString());
			DebugMegaCutAll = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.DebugMegaCutAll.ToString());
			ToggleDebugMenu = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleDebugMenu.ToString());
			ToggleBuildInfo = ((PlayerActionSet)this).CreatePlayerAction(GameplayActionSet.ToggleBuildInfo.ToString());
		}

		public static GameplayActions CreateWithDefaultBindings(bool enableAZERTY)
		{
			GameplayActions gameplayActions = new GameplayActions(enableAZERTY);
			gameplayActions.AddDefaultBindings();
			gameplayActions.AddLoadedBindings();
			((PlayerActionSet)gameplayActions).get_ListenOptions().IncludeUnknownControllers = true;
			((PlayerActionSet)gameplayActions).get_ListenOptions().OnBindingFound = delegate(PlayerAction action, BindingSource binding)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0016: Expected O, but got Unknown
				if (binding == (BindingSource)new KeyBindingSource((Key[])(object)new Key[1] { (Key)13 }))
				{
					action.StopListeningForBinding();
					return false;
				}
				return true;
			};
			BindingListenOptions listenOptions = ((PlayerActionSet)gameplayActions).get_ListenOptions();
			listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, (Action<PlayerAction, BindingSource>)delegate(PlayerAction action, BindingSource binding)
			{
				Debug.Log((object)("Binding added... " + binding.get_DeviceName() + ": " + binding.get_Name()));
			});
			BindingListenOptions listenOptions2 = ((PlayerActionSet)gameplayActions).get_ListenOptions();
			listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)delegate(PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
			{
				//IL_0005: Unknown result type (might be due to invalid IL or missing references)
				Debug.Log((object)("Binding rejected... " + reason));
			});
			return gameplayActions;
		}

		public void AddDefaultBindings()
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < mDefaultBindings.Count; i++)
			{
				string text = mDefaultBindings[i].Action.ToString();
				if (IsValidAction(text))
				{
					AddBinding(((PlayerActionSet)this).get_Item(text), mDefaultBindings[i].PCControl, mDefaultBindings[i].PS4Control, mDefaultBindings[i].XboxControl, mDefaultBindings[i].Mouse, mDefaultBindings[i].Keyboard, isDefaultBinding: true);
				}
			}
		}

		public void AddLoadedBindings()
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Invalid comparison between Unknown and I4
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < mLoadedBindings.Count; i++)
			{
				string text = mLoadedBindings[i].Action.ToString();
				if (!IsValidAction(text))
				{
					continue;
				}
				PlayerAction val = ((PlayerActionSet)this).get_Item(text);
				GameplayBindings gameplayBindings = mLoadedBindings[i];
				if ((int)gameplayBindings.PCControl != 0)
				{
					if ((int)gameplayBindings.PCControl == 520)
					{
						val.AddDefaultBinding((InputControlType)0);
					}
					else
					{
						val.AddDefaultBinding(gameplayBindings.PCControl);
					}
				}
				else if ((int)gameplayBindings.PS4Control != 0)
				{
					val.AddDefaultBinding(gameplayBindings.PS4Control);
				}
				else if ((int)gameplayBindings.XboxControl != 0)
				{
					val.AddDefaultBinding(gameplayBindings.XboxControl);
				}
				else if ((int)gameplayBindings.Mouse != 0)
				{
					val.AddDefaultBinding(gameplayBindings.Mouse);
				}
				else
				{
					val.AddDefaultBinding(gameplayBindings.Keyboard);
				}
			}
		}

		public void AddBinding(PlayerAction action, InputControlType pc, InputControlType ps4, InputControlType xbox, Mouse mouse, Key[] keyboard, bool isDefaultBinding)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Expected O, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			if (isDefaultBinding)
			{
				if ((int)pc != 0)
				{
					action.AddDefaultBinding(pc);
				}
				if ((int)ps4 != 0)
				{
					action.AddDefaultBinding(ps4);
				}
				if ((int)xbox != 0)
				{
					action.AddDefaultBinding(xbox);
				}
				if ((int)mouse != 0)
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
				if ((int)pc != 0)
				{
					action.AddBinding((BindingSource)new DeviceBindingSource(pc));
				}
				if ((int)ps4 != 0)
				{
					action.AddBinding((BindingSource)new DeviceBindingSource(ps4));
				}
				if ((int)xbox != 0)
				{
					action.AddBinding((BindingSource)new DeviceBindingSource(xbox));
				}
				if ((int)mouse != 0)
				{
					action.AddBinding((BindingSource)new MouseBindingSource(mouse));
				}
				if (keyboard != null)
				{
					action.AddBinding((BindingSource)new KeyBindingSource(keyboard));
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
					((PlayerActionSet)this).get_Item(text).ResetBindings();
				}
			}
		}

		public void UpdateBindings(List<GameplayBindings> newBindings)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
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
					((PlayerActionSet)this).get_Item(text).ClearBindings();
					AddBinding(((PlayerActionSet)this).get_Item(text), gameplayBindings.PCControl, gameplayBindings.PS4Control, gameplayBindings.XboxControl, gameplayBindings.Mouse, gameplayBindings.Keyboard, isDefaultBinding: false);
				}
				else
				{
					Debug.LogError((object)("Unbindable Action: " + text + " is not a valid action."));
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
			((OneAxisInputControl)RotateBodyUp).set_Sensitivity(ySensitivity);
			((OneAxisInputControl)RotateBodyDown).set_Sensitivity(ySensitivity);
			((OneAxisInputControl)RotateBodyLeft).set_Sensitivity(xSensitivity);
			((OneAxisInputControl)RotateBodyRight).set_Sensitivity(xSensitivity);
		}

		public void SetRotateHeadSensitivity(float xSensitivity, float ySensitivity)
		{
			((OneAxisInputControl)mRotateHeadUp).set_Sensitivity(ySensitivity);
			((OneAxisInputControl)mRotateHeadDown).set_Sensitivity(ySensitivity);
			((OneAxisInputControl)mRotateHeadLeft).set_Sensitivity(xSensitivity);
			((OneAxisInputControl)mRotateHeadRight).set_Sensitivity(xSensitivity);
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
			if (((OneAxisInputControl)action).get_LastState())
			{
				return ((OneAxisInputControl)action).get_IsPressed();
			}
			return false;
		}
	}
}
