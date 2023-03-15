using Godot;
using System;

public partial class candles : PointLight2D
{
	private readonly string[] _animationNames = {"campfire", "candles"};
	private string _animationName;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animationName = _animationNames[GD.Randi() % _animationNames.Length];
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = _animationName;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
	}
}
