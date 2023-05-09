using Godot;

public partial class npc : Area2D
{
	[Export] public bool Triggered = false;
	private AnimatedSprite2D _emotion;
	[Export]
	public string CharacterName;

	[Export] public bool Hibernating = false;

	[Signal]
	public delegate void PlayerEnterEventHandler(string name);

	[Signal]
	public delegate void PlayerExitEventHandler(string name);
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_emotion = GetNode<AnimatedSprite2D>("Emotion");
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").SpriteFrames =
			ResourceLoader.Load<SpriteFrames>($"res://animations/{CharacterName.ToLower()}.tres");
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
	
	void OnPlayerEnterArea(Node2D _)
	{
		if (Hibernating) return;
		Triggered = true;
		_emotion.Show();
		_emotion.Frame = 0;
		EmitSignal(SignalName.PlayerEnter, CharacterName);
	}
	void OnPlayerExitArea(Node2D _)
	{
		if (Hibernating) return;
		Triggered = false;
		_emotion.Hide();
		EmitSignal(SignalName.PlayerExit, CharacterName);
	}

	public void Hide()
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Hide();
		_emotion.Hide();
		GetNode<CollisionShape2D>("CollisionShape2D").SetPhysicsProcess(false);
		GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetPhysicsProcess(false);
		GetNode<StaticBody2D>("StaticBody2D").SetCollisionLayerValue(1, false);
		Hibernating = true;
	}

	public void Show()
	{
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").SpriteFrames =
			ResourceLoader.Load<SpriteFrames>($"res://animations/{CharacterName.ToLower()}.tres");
		GetNode<AnimatedSprite2D>("AnimatedSprite2D").Show();
		GetNode<CollisionShape2D>("CollisionShape2D").SetPhysicsProcess(true);
		GetNode<CollisionShape2D>("StaticBody2D/CollisionShape2D").SetPhysicsProcess(true);
		GetNode<StaticBody2D>("StaticBody2D").SetCollisionLayerValue(1, true);
		Hibernating = false;
	}
}
