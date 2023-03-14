using Godot;
using System;

public partial class wander : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (GetNode<CanvasLayer>("Dialog").Visible && Input.IsActionPressed("skip_dialog"))
		{
			GetNode<CanvasLayer>("Dialog").Hide();
		}
	}

	public void Hide()
	{
		GetNode<CanvasLayer>("CanvasLayer").Hide();
		GetNode<CanvasLayer>("Dialog").Hide();
	}
	public void Show() => GetNode<CanvasLayer>("CanvasLayer").Show();

	public void OnEnterPortal(Node2D _)
	{
		GetNode<CanvasLayer>("Dialog").Show();
	}
}
