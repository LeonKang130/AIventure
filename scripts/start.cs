using Godot;
using System;

public partial class start : Node
{
	[Signal]
	public delegate void VolumeChangedEventHandler(float value);

	[Signal]
	public delegate void StartGameEventHandler();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<CanvasLayer>("CanvasLayer/StartMenu").Show();
		GetNode<CanvasLayer>("CanvasLayer/SettingsMenu").Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnQuitButtonPressed() => GetTree().Quit();
	public void OnStartButtonPressed() => EmitSignal(SignalName.StartGame);
	public void OnSettingsButtonPressed()
	{
		GetNode<CanvasLayer>("CanvasLayer/StartMenu").Hide();
		GetNode<CanvasLayer>("CanvasLayer/SettingsMenu").Show();
	}

	public void OnSettingsBackButtonPressed()
	{
		GetNode<CanvasLayer>("CanvasLayer/StartMenu").Show();
		GetNode<CanvasLayer>("CanvasLayer/SettingsMenu").Hide();
	}

	public void OnVolumeSettingChanged(float value)
	{
		EmitSignal(SignalName.VolumeChanged, value);
	}
	public void Hide()
	{
		GetNode<CanvasLayer>("CanvasLayer").Hide();
		GetNode<CanvasLayer>("CanvasLayer/StartMenu").Hide();
		GetNode<CanvasLayer>("CanvasLayer/SettingsMenu").Hide();
	}

	public void Show()
	{
		GetNode<CanvasLayer>("CanvasLayer").Show();
		GetNode<CanvasLayer>("CanvasLayer/StartMenu").Show();
	}
}
