using Godot;
using System;
using Util;


public partial class wander : Node
{
	public bool[] PortalUsable = { false, true, true, true };
	Action InteractCallback = () => { };
	private Conversation.ConversationManager ConversationManager;
	[Signal]
	public delegate void BackToMainMenuEventHandler();
	[Signal]
	public delegate void TransitionTriggeredEventHandler();
	private CanvasLayer Dialog => GetNode<CanvasLayer>("Dialog");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConversationManager = new Conversation.ConversationManager(Dialog);
		ConversationManager.EnqueuePrioritizedDialog("Slime", "Damn, I'm gonna be late for the presentation for the boss!");
		ConversationManager.EnqueuePrioritizedDialog("Slime",
			"I'm on my first day at this post. I must not get myself fired so soon.");
		ConversationManager.EnqueuePrioritizedDialog("Slime", "Shit! Where should I go?!");
	}
	private bool IsPaused => GetNode<CanvasLayer>("Pause").Visible;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Escape Game: Back to the "Main" Scene
		if (Input.IsActionJustPressed("pause"))
		{
			GetNode<CanvasLayer>("Pause").Visible ^= true;
		}
		else if (IsPaused || !GetNode<CanvasLayer>("CanvasLayer").Visible) return;
		else if (Input.IsActionJustPressed("continue_dialog"))
		{
			ConversationManager.OnContinueDialog();
		}
		else if (Input.IsActionPressed("quit_dialog"))
		{
			ConversationManager.OnQuitDialog();
		}
		else if (Input.IsActionJustPressed("interact") && !ConversationManager.InConversation())
		{
			InteractCallback();
		}
		else
		{
			ConversationManager.PlayPrioritizedDialog();
		}
		// Shader
		var material = GetNode<ColorRect>("CanvasLayer/Vignette").Material as ShaderMaterial;
		material?.SetShaderParameter("player_pos", GetNode<player>("CanvasLayer/Player").Position / 1024);
	}

	public void Hide()
	{
		GetNode<CanvasLayer>("CanvasLayer").Hide();
		GetNode<CanvasLayer>("Dialog").Hide();
		GetNode<CanvasLayer>("Pause").Hide();
	}

	public void Show()
	{
		GetNode<CanvasLayer>("CanvasLayer").Show();
	}

	void OnEnterPortalUp(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[0])
		{
			InteractCallback = () =>
			{
				EmitSignal(SignalName.TransitionTriggered);
				player.NextTransform = player.Transform;
				player.NextTransform = player.NextTransform.Translated(
					GetNode<Marker2D>("PlayerSpawnLocations/SpawnLocationDown").Position -
					player.Position);
				player.Direction = player.FacingDirection.Up;
				player.ResetState = true;
				InteractCallback = () => { };
			};
		}
		else
		{
			InteractCallback = () =>
			{
				ConversationManager.EnqueuePrioritizedDialog("Slime", "I can't open this door.");
			};
		}
	}

	void OnEnterPortalRight(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[1])
		{
			InteractCallback = () =>
			{
				EmitSignal(SignalName.TransitionTriggered);
				player.NextTransform = player.Transform;
				player.NextTransform = player.NextTransform.Translated(
					GetNode<Marker2D>("PlayerSpawnLocations/SpawnLocationLeft").Position -
					player.Position);
				player.Direction = player.FacingDirection.Right;
				player.ResetState = true;
				InteractCallback = () => { };
			};
		}
		else
		{
			InteractCallback = () =>
			{
				ConversationManager.EnqueuePrioritizedDialog("Slime", "I can't open this door.");
			};
		}
	}

	void OnEnterPortalDown(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[2])
		{
			InteractCallback = () =>
			{
				EmitSignal(SignalName.TransitionTriggered);
				player.NextTransform = player.Transform;
				player.NextTransform = player.NextTransform.Translated(
					GetNode<Marker2D>("PlayerSpawnLocations/SpawnLocationUp").Position -
					player.Position);
				player.Direction = player.FacingDirection.Down;
				player.ResetState = true;
				InteractCallback = () => { };
			};
		}
		else
		{
			InteractCallback = () =>
			{
				ConversationManager.EnqueuePrioritizedDialog("Slime", "I can't open this door.");
			};
		}
	}

	void OnEnterPortalLeft(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[3])
		{
			InteractCallback = () =>
			{
				EmitSignal(SignalName.TransitionTriggered);
				player.NextTransform = player.Transform;
				player.NextTransform = player.NextTransform.Translated(
					GetNode<Marker2D>("PlayerSpawnLocations/SpawnLocationRight").Position -
					player.Position);
				player.Direction = player.FacingDirection.Left;
				player.ResetState = true;
				InteractCallback = () => { };
			};
		}
		else
		{
			InteractCallback = () =>
			{
				ConversationManager.EnqueuePrioritizedDialog("Slime", "I can't open this door.");
			};
		}
	}

	void OnExitPortal(Node2D _) => InteractCallback = () => { };

	void OnNPCPlayerEnter(string name)
	{
		ConversationManager.SetInteractableCharacterName(name);
		InteractCallback = ConversationManager.ShowTextEdit;
	}

	void OnNPCPlayerExit(string name)
	{
		if (name != ConversationManager.GetInteractableCharacterName()) return;
		ConversationManager.SetInteractableCharacterName(null);
		InteractCallback = () => { };
	}

	void OnBackToMainMenuButtonPressed()
	{
		GetTree().ReloadCurrentScene();
		EmitSignal(SignalName.BackToMainMenu);
	}

	void OnResumeButtonPressed() => GetNode<CanvasLayer>("Pause").Visible ^= true;
}
