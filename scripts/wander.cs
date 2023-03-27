using Godot;
using System;
using System.Collections.Generic;
using Communication;

public struct CharacterInfo
{
	public CharacterInfo(string name, string portraitPath)
	{
		this.name = name;
		this.portrait = ImageTexture.CreateFromImage(Image.LoadFromFile(portraitPath));
	}

	public string name;
	public Texture2D portrait;
}

public partial class wander : Node
{
	bool[] PortalUsable = { true, true, true, true };
	string InteractableCharacterName;
	Action InteractCallback = () => { };
	Queue<Tuple<string, string>> FixedDialogQueue = new();
	Dictionary<string, CharacterInfo> CharacterDictionary = new();
	private SkipBackend.ChatBotHandler ChatBotHandler = new SkipBackend.ChatBotHandler();
	[Signal]
	public delegate void BackToMainMenuEventHandler();

	[Signal]
	public delegate void TransitionTriggeredEventHandler();
	private CanvasLayer Dialog => GetNode<CanvasLayer>("Dialog");

	private Label ListeningLabel =>
		GetNode<Label>("Dialog/HSplitContainer/TextMargin/ScrollContainer/VBoxContainer/Label");

	private TextEdit SpeakingEdit => GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit");
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CharacterDictionary.Add("slime", new CharacterInfo("Slime", "arts/portrait-slime.png"));
		CharacterDictionary.Add("harold", new CharacterInfo("Harold", "arts/portrait-harold.png"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Damn, I'm gonna be late for the presentation for the boss!"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime",
			"I'm on my first day at this post. I must not get myself fired so soon."));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Shit! Where should I go?!"));
		IsPlayingFixedConversation = FixedDialogQueue.Count != 0;
	}

	private bool IsListening => IsInConversation && ListeningLabel.Visible;

	private bool IsSpeaking =>
		IsInConversation && SpeakingEdit.Visible;

	private bool IsInConversation => Dialog.Visible;
	private bool IsWaitingForResponse = false;
	private bool IsPlayingFixedConversation = false;
	private bool IsPaused => GetNode<CanvasLayer>("Pause").Visible;
	
	void ShowListeningText(string name, string text)
	{
		GetNode<TextureRect>("Dialog/HSplitContainer/PortraitMargin/TextureRect").Texture =
			CharacterDictionary[name.ToLower()].portrait;
		Dialog.Show();
		SpeakingEdit.Hide();
		ListeningLabel.Text =
			$"{CharacterDictionary[name.ToLower()].name}: {text}";
		ListeningLabel.Show();
	}

	private void ShowSpeakingText()
	{
		GetNode<TextureRect>("Dialog/HSplitContainer/PortraitMargin/TextureRect").Texture =
			CharacterDictionary["slime"].portrait;
		SpeakingEdit.Clear();
		Dialog.Show();
		ListeningLabel.Hide();
		SpeakingEdit.Show();
	}

	async void FetchDialogReponse(string query)
	{
		ShowListeningText(InteractableCharacterName, "...");
		IsWaitingForResponse = true;
		var response = await ChatBotHandler.GetAPIResponse(InteractableCharacterName, query);
		IsWaitingForResponse = false;
		ShowListeningText(InteractableCharacterName, response);
		GD.Print(response);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Escape Game: Back to the "Main" Scene
		if (Input.IsActionJustPressed("pause"))
		{
			GetNode<CanvasLayer>("Pause").Visible ^= true;
		}

		if (IsPaused) return;
		// Play introduction
		if (!GetNode<CanvasLayer>("CanvasLayer").Visible) return;
		// Press enter when the player is speaking
		// Exit conversation if the player says nothing
		if (IsSpeaking && Input.IsActionJustPressed("continue_dialog"))
		{
			var query = SpeakingEdit.Text;
			if (string.IsNullOrEmpty(query))
			{
				Dialog.Hide();
			}
			else
			{
				FetchDialogReponse(query);
			}
		}

		// Enter the process of dialog, and start to talk
		// NPC: Listening
		// Player: Talking
		if (IsListening && !IsWaitingForResponse)
		{
			if (Input.IsActionPressed("quit_dialog"))
			{
				if (FixedDialogQueue.Count != 0) FixedDialogQueue.Clear();
				Dialog.Hide();
			}
			else if (Input.IsActionJustPressed("continue_dialog"))
			{
				if (IsPlayingFixedConversation)
				{
					if (FixedDialogQueue.Count == 0)
					{
						IsPlayingFixedConversation = false;
						Dialog.Hide();
					}
					else
					{
						var (name, text) = FixedDialogQueue.Dequeue();
						ShowListeningText(name, text);
					}
				}
				else
				{
					ShowSpeakingText();
				}
			}
		}

		// Interact with NPC
		if (!IsInConversation && Input.IsActionJustPressed("interact"))
		{
			InteractCallback();
		}

		if (!IsInConversation && FixedDialogQueue.Count != 0)
		{
			IsPlayingFixedConversation = true;
			var (name, text) = FixedDialogQueue.Dequeue();
			ShowListeningText(name, text);
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

	public void OnEnterPortalUp(Node2D _)
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
			InteractCallback = () => { FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door.")); };
		}
	}

	public void OnEnterPortalRight(Node2D _)
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
			InteractCallback = () => { FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door.")); };
		}
	}

	public void OnEnterPortalDown(Node2D _)
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
			InteractCallback = () => { FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door.")); };
		}
	}

	public void OnEnterPortalLeft(Node2D _)
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
			InteractCallback = () => { FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door.")); };
		}
	}

	public void OnExitPortal(Node2D _)
	{
		InteractCallback = () => { };
	}

	public void OnNPCPlayerEnter(string name)
	{
		InteractableCharacterName = name;
		InteractCallback = ShowSpeakingText;
	}

	public void OnNPCPlayerExit(string name)
	{
		if (InteractableCharacterName == name)
		{
			InteractableCharacterName = null;
			InteractCallback = () => { };
		}
	}

	public void OnBackToMainMenuButtonPressed()
	{
		GetTree().ReloadCurrentScene();
		EmitSignal(SignalName.BackToMainMenu);
	}

	public void OnResumeButtonPressed()
	{
		GetNode<CanvasLayer>("Pause").Visible ^= true;
	}
}
