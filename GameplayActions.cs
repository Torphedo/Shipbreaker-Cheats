using System;
using System.Collections.Generic;
using InControl;
using UnityEngine;

namespace BBI.Unity.Game
{
	// Token: 0x02000AB2 RID: 2738
	public class GameplayActions : PlayerActionSet
	{
		// Token: 0x06003A8A RID: 14986 RVA: 0x00104B04 File Offset: 0x00102D04
		public GameplayActions(bool enableAZERTY)
		{
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveForward);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveBackward);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveUp);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustMoveDown);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustBrakeLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.ThrustBrakeRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.RollBodyLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.RollBodyRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.LateralMoveUp);
			this.LoadBindings(GameplayActions.GameplayActionSet.LateralMoveDown);
			this.LoadBindings(GameplayActions.GameplayActionSet.LateralMoveRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.LateralMoveLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.Pull);
			this.LoadBindings(GameplayActions.GameplayActionSet.Push);
			this.LoadBindings(GameplayActions.GameplayActionSet.ActivateScanner);
			this.LoadBindings(GameplayActions.GameplayActionSet.ScanCycleLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.ScanCycleRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.SelectGrapple);
			this.LoadBindings(GameplayActions.GameplayActionSet.GrappleFire);
			this.LoadBindings(GameplayActions.GameplayActionSet.RetractionModifier);
			this.LoadBindings(GameplayActions.GameplayActionSet.SwingUp);
			this.LoadBindings(GameplayActions.GameplayActionSet.SwingDown);
			this.LoadBindings(GameplayActions.GameplayActionSet.SwingRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.SwingLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.Throw);
			this.LoadBindings(GameplayActions.GameplayActionSet.PlaceTether);
			this.LoadBindings(GameplayActions.GameplayActionSet.RecallTethers);
			this.LoadBindings(GameplayActions.GameplayActionSet.SelectCutter);
			this.LoadBindings(GameplayActions.GameplayActionSet.CutterFire);
			this.LoadBindings(GameplayActions.GameplayActionSet.CutterAltFire);
			this.LoadBindings(GameplayActions.GameplayActionSet.SelectDemoCharge);
			this.LoadBindings(GameplayActions.GameplayActionSet.DemoChargeFire);
			this.LoadBindings(GameplayActions.GameplayActionSet.DemoChargeAltFire);
			this.LoadBindings(GameplayActions.GameplayActionSet.Interact);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToggleFlashlight);
			this.LoadBindings(GameplayActions.GameplayActionSet.Pause);
			this.LoadBindings(GameplayActions.GameplayActionSet.WorkOrder);
			this.LoadBindings(GameplayActions.GameplayActionSet.RightHandGrab);
			this.LoadBindings(GameplayActions.GameplayActionSet.LeftHandGrab);
			this.LoadBindings(GameplayActions.GameplayActionSet.GlassMode);
			this.LoadBindings(GameplayActions.GameplayActionSet.PlayAudioLog);
			this.LoadBindings(GameplayActions.GameplayActionSet.Decline);
			this.LoadBindings(GameplayActions.GameplayActionSet.Start);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolMenu);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolMode);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolNavUp);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolNavDown);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolNavRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.ToolNavLeft);
			this.LoadBindings(GameplayActions.GameplayActionSet.RotateBodyUp);
			this.LoadBindings(GameplayActions.GameplayActionSet.RotateBodyDown);
			this.LoadBindings(GameplayActions.GameplayActionSet.RotateBodyRight);
			this.LoadBindings(GameplayActions.GameplayActionSet.RotateBodyLeft);
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ToggleFramerate, 0, new Key[] { 17 }, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugRefillOxygen, InputControlType.None, new Key[] { Key.F1 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugRefillThrusters, InputControlType.None, new Key[] { Key.F2 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.GlassMode, InputControlType.None, new Key[] { Key.F3 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugMegaCutPlayer, InputControlType.None, new Key[] { Key.F5 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ToggleDebugMenu, InputControlType.None, new Key[] { Key.F6 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ToggleObjectDebugInfo, InputControlType.None, new Key[] { Key.F7 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ShowDebugControls, InputControlType.None, new Key[] { Key.F10 }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugResetTimeScale, InputControlType.None, new Key[] { Key.RightArrow }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugPauseTimeScale, InputControlType.None, new Key[] { Key.LeftArrow }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugIncrementTimeScale, InputControlType.None, new Key[] { Key.UpArrow }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DebugDecrementTimeScale, InputControlType.None, new Key[] { Key.DownArrow }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.EquipmentSelectionExtra01, InputControlType.Action4, new Key[] { Key.Tab }, Mouse.None, InputControlType.None, InputControlType.None));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.EquipmentSelectionExtra01, 22, new Key[] { 66 }, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.Flip, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.Zoom, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ToggleStickyGrab, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ToggleObjectives, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.DataLog, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.RotateHeadUp, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.RotateHeadDown, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.RotateHeadLeft, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.RotateHeadRight, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.CycleEquipmentMode, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.EquipmentSpecial, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ReleaseGrab, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.GrappleCancel, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.RetractTethers, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.CancelTether, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ChangeEquipment, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ModifiedRollBodyLeft, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ModifiedRollBodyRight, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ModifiedRoll, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.Reorient, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.CancelCut, 0, null, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.ClearTethers, 12, new Key[] { 29 }, 0, 0, 0));
			this.mDefaultBindings.Add(new GameplayActions.GameplayBindings(GameplayActions.GameplayActionSet.UnequipCurrentEquipment, 0, null, 0, 0, 0));
			if (enableAZERTY)
			{
				for (int i = 0; i < this.mDefaultBindings.Count; i++)
				{
					GameplayActions.GameplayBindings gameplayBindings = this.mDefaultBindings[i];
					Key[] keyboard = gameplayBindings.Keyboard;
					LynxControls.ToggleAZERTY(keyboard);
					gameplayBindings.Keyboard = keyboard;
					this.mDefaultBindings[i] = gameplayBindings;
				}
			}
			this.RightHandGrab = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RightHandGrab.ToString());
			this.LeftHandGrab = base.CreatePlayerAction(GameplayActions.GameplayActionSet.LeftHandGrab.ToString());
			this.ReleaseGrab = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ReleaseGrab.ToString());
			this.Interact = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Interact.ToString());
			this.Lunge = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Pull.ToString());
			this.Push = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Push.ToString());
			this.Throw = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Throw.ToString());
			this.SwingLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SwingLeft.ToString());
			this.SwingRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SwingRight.ToString());
			this.SwingUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SwingUp.ToString());
			this.SwingDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SwingDown.ToString());
			this.SwingHorizontal = base.CreateOneAxisPlayerAction(this.SwingLeft, this.SwingRight);
			this.SwingVertical = base.CreateOneAxisPlayerAction(this.SwingDown, this.SwingUp);
			this.ActivateScanner = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ActivateScanner.ToString());
			this.ScanObject = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ScanObject.ToString());
			this.ScanCycleLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ScanCycleLeft.ToString());
			this.ScanCycleRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ScanCycleRight.ToString());
			this.GrappleFire = base.CreatePlayerAction(GameplayActions.GameplayActionSet.GrappleFire.ToString());
			this.ChangeEquipment = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ChangeEquipment.ToString());
			this.RetractionModifier = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RetractionModifier.ToString());
			this.GrappleCancel = base.CreatePlayerAction(GameplayActions.GameplayActionSet.GrappleCancel.ToString());
			this.UnequipCurrentEquipment = base.CreatePlayerAction(GameplayActions.GameplayActionSet.UnequipCurrentEquipment.ToString());
			this.SelectCutter = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SelectCutter.ToString());
			this.ClearTethers = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ClearTethers.ToString());
			this.ToggleFlashlight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleFlashlight.ToString());
			this.SelectGrapple = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SelectGrapple.ToString());
			this.EquipmentSelectionExtra01 = base.CreatePlayerAction(GameplayActions.GameplayActionSet.EquipmentSelectionExtra01.ToString());
			this.SelectDemoCharge = base.CreatePlayerAction(GameplayActions.GameplayActionSet.SelectDemoCharge.ToString());
			this.DemoChargeFire = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DemoChargeFire.ToString());
			this.DemoChargeAltFire = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DemoChargeAltFire.ToString());
			this.PlaceTether = base.CreatePlayerAction(GameplayActions.GameplayActionSet.PlaceTether.ToString());
			this.RetractTethers = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RetractTethers.ToString());
			this.RecallTethers = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RecallTethers.ToString());
			this.CancelTether = base.CreatePlayerAction(GameplayActions.GameplayActionSet.CancelTether.ToString());
			this.Start = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Start.ToString());
			this.ReturnToMenu = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ReturnToMenu.ToString());
			this.Pause = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Pause.ToString());
			this.InvertAxes = base.CreatePlayerAction(GameplayActions.GameplayActionSet.InvertAxes.ToString());
			this.GlassMode = base.CreatePlayerAction(GameplayActions.GameplayActionSet.GlassMode.ToString());
			this.ToggleFramerate = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleFramerate.ToString());
			this.WorkOrder = base.CreatePlayerAction(GameplayActions.GameplayActionSet.WorkOrder.ToString());
			this.PlayAudioLog = base.CreatePlayerAction(GameplayActions.GameplayActionSet.PlayAudioLog.ToString());
			this.Decline = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Decline.ToString());
			this.RollBodyLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RollBodyLeft.ToString());
			this.RollBodyRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RollBodyRight.ToString());
			this.mModifiedRollBodyLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ModifiedRollBodyLeft.ToString());
			this.mModifiedRollBodyRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ModifiedRollBodyRight.ToString());
			this.ModifiedRollBody = base.CreateOneAxisPlayerAction(this.mModifiedRollBodyLeft, this.mModifiedRollBodyRight);
			this.ModifedRoll = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ModifiedRoll.ToString());
			this.RotateBodyUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateBodyUp.ToString());
			this.RotateBodyDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateBodyDown.ToString());
			this.RotateBodyLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateBodyLeft.ToString());
			this.RotateBodyRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateBodyRight.ToString());
			this.RotateBody = base.CreateTwoAxisPlayerAction(this.RotateBodyLeft, this.RotateBodyRight, this.RotateBodyDown, this.RotateBodyUp);
			this.Reorient = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Reorient.ToString());
			this.LateralMoveUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.LateralMoveUp.ToString());
			this.LateralMoveDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.LateralMoveDown.ToString());
			this.LateralMoveLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.LateralMoveLeft.ToString());
			this.LateralMoveRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.LateralMoveRight.ToString());
			this.LateralMove = base.CreateTwoAxisPlayerAction(this.LateralMoveLeft, this.LateralMoveRight, this.LateralMoveDown, this.LateralMoveUp);
			this.ThrustMoveForward = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveForward.ToString());
			this.ThrustMoveBackward = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveBackward.ToString());
			this.ThrustMoveUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveUp.ToString());
			this.ThrustMoveDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveDown.ToString());
			this.ThrustMoveLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveLeft.ToString());
			this.ThrustMoveRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustMoveRight.ToString());
			this.ThrustDepth = base.CreateOneAxisPlayerAction(this.ThrustMoveBackward, this.ThrustMoveForward);
			this.ThrustVertical = base.CreateOneAxisPlayerAction(this.ThrustMoveDown, this.ThrustMoveUp);
			this.ThrustHorizontal = base.CreateOneAxisPlayerAction(this.ThrustMoveLeft, this.ThrustMoveRight);
			this.ThrustBrakeLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustBrakeLeft.ToString());
			this.ThrustBrakeRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ThrustBrakeRight.ToString());
			this.CutterFire = base.CreatePlayerAction(GameplayActions.GameplayActionSet.CutterFire.ToString());
			this.CutterAltFire = base.CreatePlayerAction(GameplayActions.GameplayActionSet.CutterAltFire.ToString());
			this.CancelCut = base.CreatePlayerAction(GameplayActions.GameplayActionSet.CancelCut.ToString());
			this.ToolMenu = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolMenu.ToString());
			this.ToolMode = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolMode.ToString());
			this.ToolNavUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolNavUp.ToString());
			this.ToolNavDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolNavDown.ToString());
			this.ToolNavRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolNavRight.ToString());
			this.ToolNavLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToolNavLeft.ToString());
			this.Flip = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Flip.ToString());
			this.Zoom = base.CreatePlayerAction(GameplayActions.GameplayActionSet.Zoom.ToString());
			this.ToggleObjectives = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleObjectives.ToString());
			this.DataLog = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DataLog.ToString());
			this.CycleEquipmentMode = base.CreatePlayerAction(GameplayActions.GameplayActionSet.CycleEquipmentMode.ToString());
			this.EquipmentSpecial = base.CreatePlayerAction(GameplayActions.GameplayActionSet.EquipmentSpecial.ToString());
			this.ToggleStickyGrab = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleStickyGrab.ToString());
			this.mRotateHeadUp = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateHeadUp.ToString());
			this.mRotateHeadDown = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateHeadDown.ToString());
			this.mRotateHeadLeft = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateHeadLeft.ToString());
			this.mRotateHeadRight = base.CreatePlayerAction(GameplayActions.GameplayActionSet.RotateHeadRight.ToString());
			this.RotateHead = base.CreateTwoAxisPlayerAction(this.mRotateHeadLeft, this.mRotateHeadRight, this.mRotateHeadDown, this.mRotateHeadUp);
			this.ToggleObjectDebugInfo = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleObjectDebugInfo.ToString());
			this.DebugIncrementTimeScale = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugIncrementTimeScale.ToString());
			this.DebugDecrementTimeScale = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugDecrementTimeScale.ToString());
			this.DebugResetTimeScale = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugResetTimeScale.ToString());
			this.DebugPauseTimeScale = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugPauseTimeScale.ToString());
			this.DebugRefillOxygen = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugRefillOxygen.ToString());
			this.DebugRefillThrusters = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugRefillThrusters.ToString());
			this.ShowDebugControls = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ShowDebugControls.ToString());
			this.DebugSaveGame = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugSaveGame.ToString());
			this.DebugLoadGame = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugLoadGame.ToString());
			this.DebugMegaCutPlayer = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugMegaCutPlayer.ToString());
			this.DebugMegaCutAll = base.CreatePlayerAction(GameplayActions.GameplayActionSet.DebugMegaCutAll.ToString());
			this.ToggleDebugMenu = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleDebugMenu.ToString());
			this.ToggleBuildInfo = base.CreatePlayerAction(GameplayActions.GameplayActionSet.ToggleBuildInfo.ToString());
		}

		// Token: 0x06003A8B RID: 14987 RVA: 0x00105CC8 File Offset: 0x00103EC8
		public static GameplayActions CreateWithDefaultBindings(bool enableAZERTY)
		{
			GameplayActions gameplayActions = new GameplayActions(enableAZERTY);
			gameplayActions.AddDefaultBindings();
			gameplayActions.AddLoadedBindings();
			gameplayActions.ListenOptions.IncludeUnknownControllers = true;
			gameplayActions.ListenOptions.OnBindingFound = delegate(PlayerAction action, BindingSource binding)
			{
				if (binding == new KeyBindingSource(new Key[] { 13 }))
				{
					action.StopListeningForBinding();
					return false;
				}
				return true;
			};
			BindingListenOptions listenOptions = gameplayActions.ListenOptions;
			listenOptions.OnBindingAdded = (Action<PlayerAction, BindingSource>)Delegate.Combine(listenOptions.OnBindingAdded, new Action<PlayerAction, BindingSource>(delegate(PlayerAction action, BindingSource binding)
			{
				Debug.Log("Binding added... " + binding.DeviceName + ": " + binding.Name);
			}));
			BindingListenOptions listenOptions2 = gameplayActions.ListenOptions;
			listenOptions2.OnBindingRejected = (Action<PlayerAction, BindingSource, BindingSourceRejectionType>)Delegate.Combine(listenOptions2.OnBindingRejected, new Action<PlayerAction, BindingSource, BindingSourceRejectionType>(delegate(PlayerAction action, BindingSource binding, BindingSourceRejectionType reason)
			{
				Debug.Log("Binding rejected... " + reason);
			}));
			return gameplayActions;
		}

		// Token: 0x06003A8C RID: 14988 RVA: 0x00105D94 File Offset: 0x00103F94
		public void AddDefaultBindings()
		{
			for (int i = 0; i < this.mDefaultBindings.Count; i++)
			{
				string text = this.mDefaultBindings[i].Action.ToString();
				if (this.IsValidAction(text))
				{
					this.AddBinding(base[text], this.mDefaultBindings[i].PCControl, this.mDefaultBindings[i].PS4Control, this.mDefaultBindings[i].XboxControl, this.mDefaultBindings[i].Mouse, this.mDefaultBindings[i].Keyboard, true);
				}
			}
		}

		// Token: 0x06003A8D RID: 14989 RVA: 0x00105E4C File Offset: 0x0010404C
		public void AddLoadedBindings()
		{
			for (int i = 0; i < this.mLoadedBindings.Count; i++)
			{
				string text = this.mLoadedBindings[i].Action.ToString();
				if (this.IsValidAction(text))
				{
					PlayerAction playerAction = base[text];
					GameplayActions.GameplayBindings gameplayBindings = this.mLoadedBindings[i];
					if (gameplayBindings.PCControl != null)
					{
						if (gameplayBindings.PCControl == 520)
						{
							playerAction.AddDefaultBinding(0);
						}
						else
						{
							playerAction.AddDefaultBinding(gameplayBindings.PCControl);
						}
					}
					else if (gameplayBindings.PS4Control != null)
					{
						playerAction.AddDefaultBinding(gameplayBindings.PS4Control);
					}
					else if (gameplayBindings.XboxControl != null)
					{
						playerAction.AddDefaultBinding(gameplayBindings.XboxControl);
					}
					else if (gameplayBindings.Mouse != null)
					{
						playerAction.AddDefaultBinding(gameplayBindings.Mouse);
					}
					else
					{
						playerAction.AddDefaultBinding(gameplayBindings.Keyboard);
					}
				}
			}
		}

		// Token: 0x06003A8E RID: 14990 RVA: 0x00105F3C File Offset: 0x0010413C
		public void AddBinding(PlayerAction action, InputControlType pc, InputControlType ps4, InputControlType xbox, Mouse mouse, Key[] keyboard, bool isDefaultBinding)
		{
			if (isDefaultBinding)
			{
				if (pc != null)
				{
					action.AddDefaultBinding(pc);
				}
				if (ps4 != null)
				{
					action.AddDefaultBinding(ps4);
				}
				if (xbox != null)
				{
					action.AddDefaultBinding(xbox);
				}
				if (mouse != null)
				{
					action.AddDefaultBinding(mouse);
				}
				if (keyboard != null)
				{
					action.AddDefaultBinding(keyboard);
					return;
				}
			}
			else
			{
				if (pc != null)
				{
					action.AddBinding(new DeviceBindingSource(pc));
				}
				if (ps4 != null)
				{
					action.AddBinding(new DeviceBindingSource(ps4));
				}
				if (xbox != null)
				{
					action.AddBinding(new DeviceBindingSource(xbox));
				}
				if (mouse != null)
				{
					action.AddBinding(new MouseBindingSource(mouse));
				}
				if (keyboard != null)
				{
					action.AddBinding(new KeyBindingSource(keyboard));
				}
			}
		}

		// Token: 0x06003A8F RID: 14991 RVA: 0x00105FDC File Offset: 0x001041DC
		public void RestoreDefaultBindings()
		{
			for (int i = 0; i < this.mDefaultBindings.Count; i++)
			{
				string text = this.mDefaultBindings[i].Action.ToString();
				if (this.IsValidAction(text))
				{
					base[text].ResetBindings();
				}
			}
		}

		// Token: 0x06003A90 RID: 14992 RVA: 0x00106034 File Offset: 0x00104234
		public void UpdateBindings(List<GameplayActions.GameplayBindings> newBindings)
		{
			if (newBindings != null)
			{
				for (int i = 0; i < newBindings.Count; i++)
				{
					string text = newBindings[i].Action.ToString();
					GameplayActions.GameplayBindings gameplayBindings = newBindings[i];
					if (this.IsValidAction(text))
					{
						base[text].ClearBindings();
						this.AddBinding(base[text], gameplayBindings.PCControl, gameplayBindings.PS4Control, gameplayBindings.XboxControl, gameplayBindings.Mouse, gameplayBindings.Keyboard, false);
					}
					else
					{
						Debug.LogError("Unbindable Action: " + text + " is not a valid action.");
					}
				}
			}
		}

		// Token: 0x06003A91 RID: 14993 RVA: 0x001060D8 File Offset: 0x001042D8
		private bool IsValidAction(string action)
		{
			for (int i = 0; i < this.mDefaultBindings.Count; i++)
			{
				GameplayActions.GameplayBindings gameplayBindings = this.mDefaultBindings[i];
				if (action.CompareTo(gameplayBindings.Action.ToString()) == 0)
				{
					return true;
				}
			}
			for (int j = 0; j < this.mLoadedBindings.Count; j++)
			{
				GameplayActions.GameplayBindings gameplayBindings = this.mLoadedBindings[j];
				if (action.CompareTo(gameplayBindings.Action.ToString()) == 0)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06003A92 RID: 14994 RVA: 0x00106164 File Offset: 0x00104364
		public void SetRotateBodySensitivity(float xSensitivity, float ySensitivity)
		{
			this.RotateBodyUp.Sensitivity = ySensitivity;
			this.RotateBodyDown.Sensitivity = ySensitivity;
			this.RotateBodyLeft.Sensitivity = xSensitivity;
			this.RotateBodyRight.Sensitivity = xSensitivity;
		}

		// Token: 0x06003A93 RID: 14995 RVA: 0x00106196 File Offset: 0x00104396
		public void SetRotateHeadSensitivity(float xSensitivity, float ySensitivity)
		{
			this.mRotateHeadUp.Sensitivity = ySensitivity;
			this.mRotateHeadDown.Sensitivity = ySensitivity;
			this.mRotateHeadLeft.Sensitivity = xSensitivity;
			this.mRotateHeadRight.Sensitivity = xSensitivity;
		}

		// Token: 0x06003A94 RID: 14996 RVA: 0x001061C8 File Offset: 0x001043C8
		public void LoadBindings(GameplayActions.GameplayActionSet set)
		{
			foreach (GameplayActions.GameplayBindings item in LynxConfigurableControlOptions.LoadGameplayBinding(set))
			{
				this.mLoadedBindings.Add(item);
			}
		}

		// Token: 0x17000FC7 RID: 4039
		// (get) Token: 0x06003A95 RID: 14997 RVA: 0x00106220 File Offset: 0x00104420
		public float ButtonHoldThreshold
		{
			get
			{
				return 0.4f;
			}
		}

		// Token: 0x06003A96 RID: 14998 RVA: 0x00106227 File Offset: 0x00104427
		public bool ButtonHolding(PlayerAction action)
		{
			return action.LastState && action.IsPressed;
		}

		// Token: 0x04002EAB RID: 11947
		public PlayerAction LeftHandGrab;

		// Token: 0x04002EAC RID: 11948
		public PlayerAction RightHandGrab;

		// Token: 0x04002EAD RID: 11949
		public PlayerAction ReleaseGrab;

		// Token: 0x04002EAE RID: 11950
		public PlayerAction Interact;

		// Token: 0x04002EAF RID: 11951
		public PlayerAction Lunge;

		// Token: 0x04002EB0 RID: 11952
		public PlayerAction Push;

		// Token: 0x04002EB1 RID: 11953
		public PlayerAction Throw;

		// Token: 0x04002EB2 RID: 11954
		public PlayerAction ActivateScanner;

		// Token: 0x04002EB3 RID: 11955
		public PlayerAction ScanObject;

		// Token: 0x04002EB4 RID: 11956
		public PlayerAction ScanCycleLeft;

		// Token: 0x04002EB5 RID: 11957
		public PlayerAction ScanCycleRight;

		// Token: 0x04002EB6 RID: 11958
		public PlayerAction Start;

		// Token: 0x04002EB7 RID: 11959
		public PlayerAction ReturnToMenu;

		// Token: 0x04002EB8 RID: 11960
		public PlayerAction Pause;

		// Token: 0x04002EB9 RID: 11961
		public PlayerAction InvertAxes;

		// Token: 0x04002EBA RID: 11962
		public PlayerAction GlassMode;

		// Token: 0x04002EBB RID: 11963
		public PlayerAction ToggleFramerate;

		// Token: 0x04002EBC RID: 11964
		public PlayerAction WorkOrder;

		// Token: 0x04002EBD RID: 11965
		public PlayerAction PlayAudioLog;

		// Token: 0x04002EBE RID: 11966
		public PlayerAction Decline;

		// Token: 0x04002EBF RID: 11967
		public PlayerAction SelectCutter;

		// Token: 0x04002EC0 RID: 11968
		public PlayerAction ClearTethers;

		// Token: 0x04002EC1 RID: 11969
		public PlayerAction ToggleFlashlight;

		// Token: 0x04002EC2 RID: 11970
		public PlayerAction SelectGrapple;

		// Token: 0x04002EC3 RID: 11971
		public PlayerAction EquipmentSelectionExtra01;

		// Token: 0x04002EC4 RID: 11972
		public PlayerAction UnequipCurrentEquipment;

		// Token: 0x04002EC5 RID: 11973
		public PlayerAction SelectDemoCharge;

		// Token: 0x04002EC6 RID: 11974
		public PlayerAction DemoChargeFire;

		// Token: 0x04002EC7 RID: 11975
		public PlayerAction DemoChargeAltFire;

		// Token: 0x04002EC8 RID: 11976
		public PlayerAction GrappleFire;

		// Token: 0x04002EC9 RID: 11977
		public PlayerAction ChangeEquipment;

		// Token: 0x04002ECA RID: 11978
		public PlayerAction RetractionModifier;

		// Token: 0x04002ECB RID: 11979
		public PlayerAction ExtensionModifier;

		// Token: 0x04002ECC RID: 11980
		public PlayerAction GrappleCancel;

		// Token: 0x04002ECD RID: 11981
		public PlayerAction SwitchGrappleMode;

		// Token: 0x04002ECE RID: 11982
		public PlayerAction RotateGrappledObjectLeft;

		// Token: 0x04002ECF RID: 11983
		public PlayerAction RotateGrappledObjectRight;

		// Token: 0x04002ED0 RID: 11984
		public PlayerAction SwingLeft;

		// Token: 0x04002ED1 RID: 11985
		public PlayerAction SwingRight;

		// Token: 0x04002ED2 RID: 11986
		public PlayerAction SwingUp;

		// Token: 0x04002ED3 RID: 11987
		public PlayerAction SwingDown;

		// Token: 0x04002ED4 RID: 11988
		public PlayerOneAxisAction SwingHorizontal;

		// Token: 0x04002ED5 RID: 11989
		public PlayerOneAxisAction SwingVertical;

		// Token: 0x04002ED6 RID: 11990
		public PlayerAction PlaceTether;

		// Token: 0x04002ED7 RID: 11991
		public PlayerAction RetractTethers;

		// Token: 0x04002ED8 RID: 11992
		public PlayerAction RecallTethers;

		// Token: 0x04002ED9 RID: 11993
		public PlayerAction CancelTether;

		// Token: 0x04002EDA RID: 11994
		public PlayerAction Reorient;

		// Token: 0x04002EDB RID: 11995
		public PlayerAction RotateBodyUp;

		// Token: 0x04002EDC RID: 11996
		public PlayerAction RotateBodyDown;

		// Token: 0x04002EDD RID: 11997
		public PlayerAction RotateBodyLeft;

		// Token: 0x04002EDE RID: 11998
		public PlayerAction RotateBodyRight;

		// Token: 0x04002EDF RID: 11999
		public PlayerTwoAxisAction RotateBody;

		// Token: 0x04002EE0 RID: 12000
		public PlayerAction RollBodyLeft;

		// Token: 0x04002EE1 RID: 12001
		public PlayerAction RollBodyRight;

		// Token: 0x04002EE2 RID: 12002
		private PlayerAction mModifiedRollBodyLeft;

		// Token: 0x04002EE3 RID: 12003
		private PlayerAction mModifiedRollBodyRight;

		// Token: 0x04002EE4 RID: 12004
		public PlayerAction ModifedRoll;

		// Token: 0x04002EE5 RID: 12005
		public PlayerOneAxisAction ModifiedRollBody;

		// Token: 0x04002EE6 RID: 12006
		public PlayerAction ThrustBrakeLeft;

		// Token: 0x04002EE7 RID: 12007
		public PlayerAction ThrustBrakeRight;

		// Token: 0x04002EE8 RID: 12008
		public PlayerAction ThrustMoveForward;

		// Token: 0x04002EE9 RID: 12009
		public PlayerAction ThrustMoveBackward;

		// Token: 0x04002EEA RID: 12010
		public PlayerOneAxisAction ThrustDepth;

		// Token: 0x04002EEB RID: 12011
		public PlayerAction ThrustMoveUp;

		// Token: 0x04002EEC RID: 12012
		public PlayerAction ThrustMoveDown;

		// Token: 0x04002EED RID: 12013
		public PlayerOneAxisAction ThrustVertical;

		// Token: 0x04002EEE RID: 12014
		public PlayerAction ThrustMoveLeft;

		// Token: 0x04002EEF RID: 12015
		public PlayerAction ThrustMoveRight;

		// Token: 0x04002EF0 RID: 12016
		public PlayerOneAxisAction ThrustHorizontal;

		// Token: 0x04002EF1 RID: 12017
		public PlayerAction LateralMoveUp;

		// Token: 0x04002EF2 RID: 12018
		public PlayerAction LateralMoveDown;

		// Token: 0x04002EF3 RID: 12019
		public PlayerAction LateralMoveLeft;

		// Token: 0x04002EF4 RID: 12020
		public PlayerAction LateralMoveRight;

		// Token: 0x04002EF5 RID: 12021
		public PlayerTwoAxisAction LateralMove;

		// Token: 0x04002EF6 RID: 12022
		public PlayerAction CutterFire;

		// Token: 0x04002EF7 RID: 12023
		public PlayerAction CancelCut;

		// Token: 0x04002EF8 RID: 12024
		public PlayerAction CutterAltFire;

		// Token: 0x04002EF9 RID: 12025
		public PlayerAction ToolMenu;

		// Token: 0x04002EFA RID: 12026
		public PlayerAction ToolMode;

		// Token: 0x04002EFB RID: 12027
		public PlayerAction ToolNavUp;

		// Token: 0x04002EFC RID: 12028
		public PlayerAction ToolNavDown;

		// Token: 0x04002EFD RID: 12029
		public PlayerAction ToolNavRight;

		// Token: 0x04002EFE RID: 12030
		public PlayerAction ToolNavLeft;

		// Token: 0x04002EFF RID: 12031
		public PlayerAction ToggleStickyGrab;

		// Token: 0x04002F00 RID: 12032
		public PlayerAction Flip;

		// Token: 0x04002F01 RID: 12033
		public PlayerAction Zoom;

		// Token: 0x04002F02 RID: 12034
		public PlayerAction ToggleObjectives;

		// Token: 0x04002F03 RID: 12035
		public PlayerAction DataLog;

		// Token: 0x04002F04 RID: 12036
		public PlayerAction CycleEquipmentMode;

		// Token: 0x04002F05 RID: 12037
		public PlayerAction EquipmentSpecial;

		// Token: 0x04002F06 RID: 12038
		private PlayerAction mRotateHeadUp;

		// Token: 0x04002F07 RID: 12039
		private PlayerAction mRotateHeadDown;

		// Token: 0x04002F08 RID: 12040
		private PlayerAction mRotateHeadLeft;

		// Token: 0x04002F09 RID: 12041
		private PlayerAction mRotateHeadRight;

		// Token: 0x04002F0A RID: 12042
		public PlayerTwoAxisAction RotateHead;

		// Token: 0x04002F0B RID: 12043
		public PlayerAction ToggleObjectDebugInfo;

		// Token: 0x04002F0C RID: 12044
		public PlayerAction DebugIncrementTimeScale;

		// Token: 0x04002F0D RID: 12045
		public PlayerAction DebugDecrementTimeScale;

		// Token: 0x04002F0E RID: 12046
		public PlayerAction DebugResetTimeScale;

		// Token: 0x04002F0F RID: 12047
		public PlayerAction DebugPauseTimeScale;

		// Token: 0x04002F10 RID: 12048
		public PlayerAction DebugRefillOxygen;

		// Token: 0x04002F11 RID: 12049
		public PlayerAction DebugRefillThrusters;

		// Token: 0x04002F12 RID: 12050
		public PlayerAction ShowDebugControls;

		// Token: 0x04002F13 RID: 12051
		public PlayerAction ToggleDebugMenu;

		// Token: 0x04002F14 RID: 12052
		public PlayerAction ToggleBuildInfo;

		// Token: 0x04002F15 RID: 12053
		public PlayerAction DebugSaveGame;

		// Token: 0x04002F16 RID: 12054
		public PlayerAction DebugLoadGame;

		// Token: 0x04002F17 RID: 12055
		public PlayerAction DebugMegaCutPlayer;

		// Token: 0x04002F18 RID: 12056
		public PlayerAction DebugMegaCutAll;

		// Token: 0x04002F19 RID: 12057
		private List<GameplayActions.GameplayBindings> mDefaultBindings = new List<GameplayActions.GameplayBindings>();

		// Token: 0x04002F1A RID: 12058
		private List<GameplayActions.GameplayBindings> mLoadedBindings = new List<GameplayActions.GameplayBindings>();

		// Token: 0x04002F1B RID: 12059
		private const float kButtonHoldThresholdInSeconds = 0.4f;

		// Token: 0x02000EE3 RID: 3811
		[Serializable]
		public struct GameplayBindings
		{
			// Token: 0x06004A3C RID: 19004 RVA: 0x001469A0 File Offset: 0x00144BA0
			public GameplayBindings(GameplayActions.GameplayActionSet action, InputControlType pcControl = 0, Key[] keyboard = null, Mouse mouse = 0, InputControlType ps4Control = 0, InputControlType xboxControl = 0)
			{
				this.Action = action;
				this.PCControl = pcControl;
				this.PS4Control = ((ps4Control == null) ? pcControl : ps4Control);
				this.XboxControl = ((xboxControl == null) ? pcControl : xboxControl);
				this.Keyboard = ((keyboard == null || keyboard.Length == 0) ? new Key[1] : keyboard);
				this.Mouse = mouse;
			}

			// Token: 0x0400472E RID: 18222
			public GameplayActions.GameplayActionSet Action;

			// Token: 0x0400472F RID: 18223
			public InputControlType PCControl;

			// Token: 0x04004730 RID: 18224
			public InputControlType PS4Control;

			// Token: 0x04004731 RID: 18225
			public InputControlType XboxControl;

			// Token: 0x04004732 RID: 18226
			public Key[] Keyboard;

			// Token: 0x04004733 RID: 18227
			public Mouse Mouse;
		}

		// Token: 0x02000EE4 RID: 3812
		[Serializable]
		public enum GameplayActionSet
		{
			// Token: 0x04004735 RID: 18229
			None,
			// Token: 0x04004736 RID: 18230
			GrappleFire = 10,
			// Token: 0x04004737 RID: 18231
			RetractionModifier,
			// Token: 0x04004738 RID: 18232
			GrappleCancel = 13,
			// Token: 0x04004739 RID: 18233
			PlaceTether,
			// Token: 0x0400473A RID: 18234
			RetractTethers,
			// Token: 0x0400473B RID: 18235
			RecallTethers,
			// Token: 0x0400473C RID: 18236
			CancelTether,
			// Token: 0x0400473D RID: 18237
			GrappleAltFire,
			// Token: 0x0400473E RID: 18238
			UnequipCurrentEquipment,
			// Token: 0x0400473F RID: 18239
			LeftHandGrab,
			// Token: 0x04004740 RID: 18240
			RightHandGrab = 30,
			// Token: 0x04004741 RID: 18241
			ReleaseGrab = 35,
			// Token: 0x04004742 RID: 18242
			Interact = 40,
			// Token: 0x04004743 RID: 18243
			Pull = 50,
			// Token: 0x04004744 RID: 18244
			Push = 60,
			// Token: 0x04004745 RID: 18245
			Throw = 70,
			// Token: 0x04004746 RID: 18246
			Reorient = 80,
			// Token: 0x04004747 RID: 18247
			Start = 90,
			// Token: 0x04004748 RID: 18248
			ReturnToMenu = 100,
			// Token: 0x04004749 RID: 18249
			Rewind = 110,
			// Token: 0x0400474A RID: 18250
			Pause = 120,
			// Token: 0x0400474B RID: 18251
			ChangeEquipment = 130,
			// Token: 0x0400474C RID: 18252
			RollBodyLeft = 140,
			// Token: 0x0400474D RID: 18253
			ModifiedRollBodyLeft,
			// Token: 0x0400474E RID: 18254
			RollBodyRight = 150,
			// Token: 0x0400474F RID: 18255
			ModifiedRollBodyRight,
			// Token: 0x04004750 RID: 18256
			ModifiedRoll,
			// Token: 0x04004751 RID: 18257
			ThrustMoveForward = 160,
			// Token: 0x04004752 RID: 18258
			ThrustMoveBackward = 170,
			// Token: 0x04004753 RID: 18259
			ThrustBrakeLeft,
			// Token: 0x04004754 RID: 18260
			ThrustBrakeRight,
			// Token: 0x04004755 RID: 18261
			InvertAxes = 180,
			// Token: 0x04004756 RID: 18262
			GlassMode = 190,
			// Token: 0x04004757 RID: 18263
			ActivateScanner = 200,
			// Token: 0x04004758 RID: 18264
			ScanObject,
			// Token: 0x04004759 RID: 18265
			SelectCutter = 210,
			// Token: 0x0400475A RID: 18266
			ClearTethers = 220,
			// Token: 0x0400475B RID: 18267
			ToggleFlashlight = 230,
			// Token: 0x0400475C RID: 18268
			SelectGrapple = 240,
			// Token: 0x0400475D RID: 18269
			EquipmentSelectionExtra01,
			// Token: 0x0400475E RID: 18270
			RotateBodyUp = 250,
			// Token: 0x0400475F RID: 18271
			RotateBodyDown = 260,
			// Token: 0x04004760 RID: 18272
			RotateBodyLeft = 270,
			// Token: 0x04004761 RID: 18273
			RotateBodyRight = 280,
			// Token: 0x04004762 RID: 18274
			ThrustMoveUp = 290,
			// Token: 0x04004763 RID: 18275
			ThrustMoveDown = 300,
			// Token: 0x04004764 RID: 18276
			ThrustMoveLeft = 310,
			// Token: 0x04004765 RID: 18277
			ThrustMoveRight = 320,
			// Token: 0x04004766 RID: 18278
			LateralMoveUp = 330,
			// Token: 0x04004767 RID: 18279
			LateralMoveDown = 340,
			// Token: 0x04004768 RID: 18280
			LateralMoveLeft = 350,
			// Token: 0x04004769 RID: 18281
			LateralMoveRight = 360,
			// Token: 0x0400476A RID: 18282
			ToggleFramerate = 370,
			// Token: 0x0400476B RID: 18283
			CutterFire = 380,
			// Token: 0x0400476C RID: 18284
			CancelCut = 400,
			// Token: 0x0400476D RID: 18285
			CutterAltFire = 510,
			// Token: 0x0400476E RID: 18286
			RotateGrappledObjectLeft = 520,
			// Token: 0x0400476F RID: 18287
			RotateGrappledObjectRight = 530,
			// Token: 0x04004770 RID: 18288
			SwingLeft = 540,
			// Token: 0x04004771 RID: 18289
			SwingRight = 550,
			// Token: 0x04004772 RID: 18290
			SwingUp = 560,
			// Token: 0x04004773 RID: 18291
			SwingDown = 570,
			// Token: 0x04004774 RID: 18292
			ScanCycleLeft = 580,
			// Token: 0x04004775 RID: 18293
			ScanCycleRight = 590,
			// Token: 0x04004776 RID: 18294
			WorkOrder = 600,
			// Token: 0x04004777 RID: 18295
			PlayAudioLog = 610,
			// Token: 0x04004778 RID: 18296
			ToolMenu = 700,
			// Token: 0x04004779 RID: 18297
			ToolMode = 710,
			// Token: 0x0400477A RID: 18298
			ToolNavUp = 720,
			// Token: 0x0400477B RID: 18299
			ToolNavDown = 730,
			// Token: 0x0400477C RID: 18300
			ToolNavRight = 740,
			// Token: 0x0400477D RID: 18301
			ToolNavLeft = 750,
			// Token: 0x0400477E RID: 18302
			SelectDemoCharge = 760,
			// Token: 0x0400477F RID: 18303
			DemoChargeFire = 770,
			// Token: 0x04004780 RID: 18304
			DemoChargeAltFire = 780,
			// Token: 0x04004781 RID: 18305
			Decline = 790,
			// Token: 0x04004782 RID: 18306
			ToggleStickyGrab = 10000,
			// Token: 0x04004783 RID: 18307
			Flip = 10010,
			// Token: 0x04004784 RID: 18308
			Zoom = 10020,
			// Token: 0x04004785 RID: 18309
			ToggleObjectives = 10030,
			// Token: 0x04004786 RID: 18310
			DataLog = 10040,
			// Token: 0x04004787 RID: 18311
			CycleEquipmentMode = 10050,
			// Token: 0x04004788 RID: 18312
			EquipmentSpecial = 10060,
			// Token: 0x04004789 RID: 18313
			RotateHeadUp = 10070,
			// Token: 0x0400478A RID: 18314
			RotateHeadDown = 10080,
			// Token: 0x0400478B RID: 18315
			RotateHeadLeft = 10090,
			// Token: 0x0400478C RID: 18316
			RotateHeadRight = 10100,
			// Token: 0x0400478D RID: 18317
			ToggleObjectDebugInfo = 20130,
			// Token: 0x0400478E RID: 18318
			DebugIncrementTimeScale = 20140,
			// Token: 0x0400478F RID: 18319
			DebugDecrementTimeScale = 20150,
			// Token: 0x04004790 RID: 18320
			DebugResetTimeScale = 20160,
			// Token: 0x04004791 RID: 18321
			DebugPauseTimeScale = 20170,
			// Token: 0x04004792 RID: 18322
			DebugRefillOxygen = 20180,
			// Token: 0x04004793 RID: 18323
			DebugRefillThrusters = 20190,
			// Token: 0x04004794 RID: 18324
			ShowDebugControls = 20210,
			// Token: 0x04004795 RID: 18325
			DebugSaveGame = 20220,
			// Token: 0x04004796 RID: 18326
			DebugLoadGame = 20230,
			// Token: 0x04004797 RID: 18327
			DebugMegaCutPlayer = 20240,
			// Token: 0x04004798 RID: 18328
			DebugMegaCutAll = 20250,
			// Token: 0x04004799 RID: 18329
			ToggleDebugMenu = 20260,
			// Token: 0x0400479A RID: 18330
			ToggleBuildInfo = 20270
		}
	}
}
