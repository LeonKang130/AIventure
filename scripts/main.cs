using Godot;
using System;

public partial class main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<AudioStreamPlayer2D>("BGM").Play();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void OnVolumeSettingChanged(float value)
	{
		GetNode<AudioStreamPlayer2D>("BGM").VolumeDb = value == 0.001f ? -100.0f : Mathf.Log(value) * 5.0f;
	}

	public void OnBGMFinished()
	{
		GetNode<AudioStreamPlayer2D>("BGM").Play();
	}
}
