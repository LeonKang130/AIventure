using Godot;
using System;

public partial class treasure : Area2D
{
	[Export]
	public bool opened = false;

	private AnimatedSprite2D _animatedSprite2D => GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	void OnChestOpened()
	{
		if (opened) return;
		opened = true;
		_animatedSprite2D.Play("open");
	}

	void OnChestOpeningAnimationFinished()
	{
		if (opened) _animatedSprite2D.Animation = new StringName("opened");
	}
	
	public void Hide()
	{
		_animatedSprite2D.Hide();
		GetNode<CollisionShape2D>("CollisionShape2D").SetPhysicsProcess(false);
		GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetPhysicsProcess(false);
		GetNode<StaticBody2D>("StaticBody2D").SetCollisionLayerValue(1, false);
	}
	
	public void Show()
	{
		_animatedSprite2D.Show();
		if (opened) _animatedSprite2D.Animation = new StringName("opened");
		GetNode<CollisionShape2D>("CollisionShape2D").SetPhysicsProcess(true);
		GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetPhysicsProcess(true);
		GetNode<StaticBody2D>("StaticBody2D").SetCollisionLayerValue(1, true);
	}
}
