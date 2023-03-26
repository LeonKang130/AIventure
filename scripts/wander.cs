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
	public string? InteractableCharacterName;
	Queue<Tuple<string, string>> FixedDialogQueue = new();
	Dictionary<string, CharacterInfo> CharacterDictionary = new();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CharacterDictionary.Add("slime", new CharacterInfo("Slime", "arts/portrait-slime.png"));
		CharacterDictionary.Add("nazarec", new CharacterInfo("Nazarec", "arts/portrait-nazarec.png"));
		CharacterDictionary.Add("harold", new CharacterInfo("Harold", "arts/portrait-harold.png"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Damn, I'm gonna be late for the presentation for the boss!"));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "I'm on my first day at this post. I must not get myself fired so soon."));
		FixedDialogQueue.Enqueue(Tuple.Create("Slime", "Shit! Where should I go?!"));
	}

	private bool IsListening => GetNode<Label>("Dialog/HSplitContainer/TextMargin/Label").Visible;
	private bool IsSpeaking => GetNode<TextEdit>("Dialog/HSplitContainer/TextMargin/TextEdit").Visible;
	private bool IsInConversation => GetNode<CanvasLayer>("Dialog").Visible;
	private bool IsWaitingForResponse = false;
	private void ShowListeningText(string name, string text)
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
				if (FixedDialogQueue.Count != 0)
				{
					FixedDialogQueue.Dequeue();
					if (FixedDialogQueue.Count == 0)
					{
						GetNode<CanvasLayer>("Dialog").Hide();
					}
					else
					{
						var (name, text) = FixedDialogQueue.First();
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
			if (InteractableCharacterName != null)
			{
				ShowSpeakingText();
			}
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
		if (FixedDialogQueue.Count != 0)
		{
			var (name, text) = FixedDialogQueue.First();
			ShowListeningText(name, text);
		}
	}

	public void OnEnterPortal(Node2D _)
	{
		InteractableCharacterName = "Nazarec";
		ShowListeningText("Nazarec","Where do you think you are going?");
	}
	public void OnNPCPlayerEnter(string name)
	{
		InteractableCharacterName = name;
	}
	public void OnNPCPlayerExit(string name)
	{
		if (InteractableCharacterName != null && InteractableCharacterName.CasecmpTo(name) == 0)
		{
			InteractableCharacterName = null;
		}
	}
}
