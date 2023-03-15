using Godot;

public partial class npc : Area2D
{
	[Export] public bool Triggered = false;
	private AnimatedSprite2D _emotion;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_emotion = GetNode<AnimatedSprite2D>("Emotion");
		Triggered = false;
		_emotion.Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Triggered && _emotion?.Visible == true)
		{
			_emotion.Play();
		}
	}

	public void OnPlayerEnterArea(Node2D _)
	{
		Triggered = true;
		_emotion.Show();
		_emotion.Frame = 0;
	}

	public void OnPlayerExitArea(Node2D _)
	{
		Triggered = false;
		_emotion.Hide();
	}
}
