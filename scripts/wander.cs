using Godot;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

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
	public bool[] PortalUsable = { true, true, true, true };
	public string? InteractableCharacterName;
	public Action InteractCallback = () => { };
	public Queue<Tuple<string, string>> FixedDialogQueue = new();
	Dictionary<string, CharacterInfo> CharacterDictionary = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CharacterDictionary.Add("slime", new CharacterInfo("Slime", "arts/portrait-slime.png"));
		CharacterDictionary.Add("nezarec", new CharacterInfo("Nezarec", "arts/portrait-nezarec.png"));
		CharacterDictionary.Add("harold", new CharacterInfo("Harold", "arts/portrait-harold.png"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Damn, I'm gonna be late for the presentation for the boss!"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I'm on my first day at this post. I must not get myself fired so soon."));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Shit! Where should I go?!"));
		IsPlayingFixedConversation = FixedDialogQueue.Count != 0;
	}

	private bool IsListening => IsInConversation && GetNode<Label>("Dialog/HSplitContainer/TextMargin/Label").Visible;
	private bool IsSpeaking => IsInConversation && GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit").Visible;
	private bool IsInConversation => GetNode<CanvasLayer>("Dialog").Visible;
	private bool IsWaitingForResponse = false;
	private bool IsPlayingFixedConversation = false;
	void ShowListeningText(string name, string text)
	{
		GetNode<TextureRect>("Dialog/HSplitContainer/PortraitMargin/TextureRect").Texture = CharacterDictionary[name.ToLower()].portrait;
		GetNode<CanvasLayer>("Dialog").Show();
		GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit").Hide();
		GetNode<Label>("Dialog/HSplitContainer/TextMargin/Label").Text = $"{CharacterDictionary[name.ToLower()].name}: {text}";
		GetNode<Label>("Dialog/HSplitContainer/TextMargin/Label").Show();
	}

	private void ShowSpeakingText()
	{
		GetNode<TextureRect>("Dialog/HSplitContainer/PortraitMargin/TextureRect").Texture = CharacterDictionary["slime"].portrait;
		GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit").Clear();
		GetNode<CanvasLayer>("Dialog").Show();
		GetNode<Label>("Dialog/HSplitContainer/TextMargin/Label").Hide();
		GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit").Show();
	}

	async void FetchDialogReponse()
	{
		ShowListeningText(InteractableCharacterName, "...");
		IsWaitingForResponse = true;
		await Task.Delay(1000);
		IsWaitingForResponse = false;
		ShowListeningText(InteractableCharacterName, "(non-distinguishable mumbling)");
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Play introduction
		if (!GetNode<CanvasLayer>("CanvasLayer").Visible) return;
		// Press enter when the player is speaking
		// Exit conversation if the player says nothing
		if (IsSpeaking && Input.IsActionJustPressed("continue_dialog"))
		{
			var textEdit = GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit");
			var content = textEdit.Text;
			if (string.IsNullOrEmpty(content))
			{
				GetNode<CanvasLayer>("Dialog").Hide();
			}
			else
			{
				//Todo Send content to server
				//show "..." while waiting
				//asynchronously display answer when the answer arrived
				FetchDialogReponse();
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
				GetNode<CanvasLayer>("Dialog").Hide();
			}
			else if (Input.IsActionJustPressed("continue_dialog"))
			{
				if (IsPlayingFixedConversation)
				{
					if (FixedDialogQueue.Count == 0)
					{
						IsPlayingFixedConversation = false;
						GetNode<CanvasLayer>("Dialog").Hide();
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
		// Escape Game: Back to the "Main" Scene
		if (Input.IsActionPressed("pause"))
		{
			GetParent<main>().GetNode<wander>("Wander").Hide();
			GetParent<main>().GetNode<start>("Start").Show();
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
				FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door."));
			};
		}
		
	}
	public void OnEnterPortalRight(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[1])
		{
			InteractCallback = () =>
			{
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
				FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door."));
			};
		}
		
	}
	public void OnEnterPortalDown(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[2])
		{
			InteractCallback = () =>
			{
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
				FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door."));
			};
		}
		
	}
	public void OnEnterPortalLeft(Node2D _)
	{
		var player = GetNode<player>("CanvasLayer/Player");
		if (PortalUsable[3])
		{
			InteractCallback = () =>
			{
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
				FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I can't open this door."));
			};
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
}
