using Godot;

public partial class player : RigidBody2D
{
	[Export] public CanvasLayer Dialog;
	private bool isInDialog() => Dialog != null && Dialog.Visible;
	public enum FacingDirection
	{
		Left,
		Right,
		Up,
		Down
	}
	[Export] public float Speed;
	[Export] public FacingDirection Direction;
	public bool ResetState = false;
	public Transform2D NextTransform;
	private AnimatedSprite2D _animatedSprite2D;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_animatedSprite2D = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		Direction = FacingDirection.Down;
	}

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		if (!ResetState) return;
		state.Transform = NextTransform;
		state.LinearVelocity = Vector2.Zero;
		ResetState = false;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var velocity = Vector2.Zero;
		var idle = true;
		if (!isInDialog())
		{
			if (Input.IsActionPressed("move_left"))
			{
				velocity.X -= 1.0f;
			}

			if (Input.IsActionPressed("move_right"))
			{
				velocity.X += 1.0f;
			}

			if (Input.IsActionPressed("move_up"))
			{
				velocity.Y -= 1.0f;
			}

			if (Input.IsActionPressed("move_down"))
			{
				velocity.Y += 1.0f;
			}
			idle = velocity.Length() == 0;
			if (!idle)
			{
				velocity = velocity.Normalized() * Speed;
				if (velocity.X > 0)
				{
					Direction = FacingDirection.Right;
				}
				else if (velocity.X < 0)
				{
					Direction = FacingDirection.Left;
				}
				else if (velocity.Y < 0)
				{
					Direction = FacingDirection.Up;
				}
				else
				{
					Direction = FacingDirection.Down;
				}
			}

			ApplyCentralForce(velocity);
			_animatedSprite2D.Play();
		}
		
		switch (Direction)
		{
			case FacingDirection.Up:
				_animatedSprite2D.Animation = idle ? "idle-up" : "up";
				break;
			case FacingDirection.Down:
				_animatedSprite2D.Animation = idle ? "idle-down" : "down";
				break;
			case FacingDirection.Left:
				_animatedSprite2D.Animation = idle ? "idle-left" : "left";
				break;
			case FacingDirection.Right:
				_animatedSprite2D.Animation = idle ? "idle-right" : "right";
				break;
		}
	}
}
