using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using Util;
using Microsoft.FSharp.Core;

public partial class wander : Node
{
	private enum InteractionType
	{
		UsePortalUp = 0,
		UsePortalRight = 1,
		UsePortalDown = 2,
		UsePortalLeft = 3,

		// Don't touch the first four
		NPCConversation,
		OpenTreasureChest,
		None,
	}

	private InteractionType _interaction = InteractionType.None;
	private Conversation.ConversationManager ConversationManager;
	private Levels.LevelHandler LevelHandler;
	private bool[] PortalUsability => LevelHandler.PortalUsability;
	private Vector2I CurrentLocation => LevelHandler.CurrentLocation;
	private Vector2I Destination => LevelHandler.Destination;
	private bool IsPaused => GetNode<CanvasLayer>("Pause").Visible;
	private CanvasLayer Dialog => GetNode<CanvasLayer>("Dialog");
	private CanvasLayer Inventory => GetNode<CanvasLayer>("Inventory");
	private bool ExitRequested = false;
	private List<Levels.Item> _inventory = new();
	private int _inventoryItemIndex = 0;
	private Marker2D[] SpawnMarkers =>
		new List<string> { "SpawnLocationDown", "SpawnLocationLeft", "SpawnLocationUp", "SpawnLocationRight" }
			.Select(markerName => GetNode<Marker2D>("PlayerSpawnLocations/" + markerName))
			.ToArray();

	[Signal]
	public delegate void BackToMainMenuEventHandler();

	[Signal]
	public delegate void TransitionTriggeredEventHandler();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ConversationManager = new Conversation.ConversationManager(Dialog);
		ConversationManager.EnqueuePrioritizedDialog("Slime",
			"Damn, I'm gonna be late for the presentation for the boss!");
		ConversationManager.EnqueuePrioritizedDialog("Slime",
			"I'm on my first day at this post. I must not get myself fired so soon.");
		ConversationManager.EnqueuePrioritizedDialog("Slime", "Shit! Where should I go?!");
		LevelHandler = new Levels.LevelHandler(ConversationManager.CharacterList);
		GD.Print($"Slime Spawned At: {CurrentLocation}");
		GD.Print($"Destination At: {Destination}");
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Escape Game: Back to the "Main" Scene
		if (Input.IsActionJustPressed("pause"))
		{
			GetNode<CanvasLayer>("Pause").Visible ^= true;
		}
		else if (IsPaused || !GetNode<CanvasLayer>("CanvasLayer").Visible) return;
		else if (!Dialog.Visible && Input.IsActionJustPressed("toggle_inventory"))
		{
			if (!Inventory.Visible && _inventory.Count == 0)
			{
				ConversationManager.EnqueuePrioritizedDialog("Slime", "I have nothing on me yet.");
			}
			else
			{
				if (!Inventory.Visible)
				{
					_inventoryItemIndex = 0;
					GetNode<TextureRect>("Inventory/Item/Icon").Texture = _inventory[_inventoryItemIndex].imageTexture;
					GetNode<Label>("Inventory/Item/Name").Text = _inventory[_inventoryItemIndex].name;
					GetNode<Label>("Inventory/Item/Description").Text = _inventory[_inventoryItemIndex].description;
				}
				Inventory.Visible ^= true;
			}
		}
		else if (Inventory.Visible)
		{
			if (Input.IsActionJustPressed("move_left"))
			{
				_inventoryItemIndex = (_inventoryItemIndex + _inventory.Count - 1) % _inventory.Count;
				GetNode<TextureRect>("Inventory/Item/Icon").Texture = _inventory[_inventoryItemIndex].imageTexture;
				GetNode<Label>("Inventory/Item/Name").Text = _inventory[_inventoryItemIndex].name;
				GetNode<Label>("Inventory/Item/Description").Text = _inventory[_inventoryItemIndex].description;
			}
			else if (Input.IsActionJustPressed("move_right"))
			{
				_inventoryItemIndex = (_inventoryItemIndex + 1) % _inventory.Count;
				GetNode<TextureRect>("Inventory/Item/Icon").Texture = _inventory[_inventoryItemIndex].imageTexture;
				GetNode<Label>("Inventory/Item/Name").Text = _inventory[_inventoryItemIndex].name;
				GetNode<Label>("Inventory/Item/Description").Text = _inventory[_inventoryItemIndex].description;
			}
		}
		else if (Input.IsActionJustPressed("continue_dialog"))
		{
			var groundTruth = LevelHandler.GroundTruthInfo;
			ConversationManager.OnContinueDialog(groundTruth);
		}
		else if (Input.IsActionPressed("quit_dialog"))
		{
			ConversationManager.OnQuitDialog();
		}
		else if (Input.IsActionJustPressed("interact") && !ConversationManager.InConversation())
		{
			switch (_interaction)
			{
				case InteractionType.NPCConversation:
					{
					ConversationManager.ShowTextEdit();
					break;
				}
				case InteractionType.UsePortalUp or InteractionType.UsePortalRight or InteractionType.UsePortalDown
					or InteractionType.UsePortalLeft:
				{
					var portalIndex = (int)_interaction;
					if (PortalUsability[portalIndex])
					{
						MoveToLevel(portalIndex);
					}
					else
					{
						ConversationManager.EnqueuePrioritizedDialog("Slime", "I can't open this door!");
					}
					break;
				}
				case InteractionType.OpenTreasureChest:
				{
					if (!GetNode<treasure>("CanvasLayer/Treasure").active) break;
					var item = LevelHandler.CurrentLevelTreasureContent;
					if (item.IsEmpty)
					{
						ConversationManager.EnqueuePrioritizedDialog("Slime", "This chest has nothing in it.");
					}
					else
					{
						_inventory.Add(item);
						ConversationManager.EnqueuePrioritizedDialog("Slime", $"There's a {item.name} inside the chest. I'll take it with me.");
						LevelHandler.RemoveCurrentLevelTreasureContent();
						GetNode<treasure>("CanvasLayer/Treasure").OnChestOpened();
					}
					break;
				}
			}
		}
		else if (ConversationManager.RequestExit())
		{
			OnBackToMainMenuButtonPressed();
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

	private void SetUpLevel()
	{
		// Reload lamps and Set NPC
		var npc = GetNode<npc>("CanvasLayer/NPC");
		var traps = GetNode<TileMap>("CanvasLayer/Traps");
		var treasure = GetNode<treasure>("CanvasLayer/Treasure");
		if (LevelHandler.CurrentLevel.IsNPC)
		{
			treasure.Hide();
			traps.Hide();
			npc.CharacterName = LevelHandler.CurrentLevelNPCName;
			npc.Show();
			if (npc.CharacterName == "Devil")
			{
				ConversationManager.OnDestinationReached();
			}
		}
		else if (LevelHandler.CurrentLevel.IsTrap)
		{
			npc.Hide();
			treasure.Hide();
			if (_inventory.Count == 0)
			{
				traps.Show();
				ConversationManager.OnTrapEntered();
			}
			else
			{
				var rng = new Random();
				var index = rng.Next(0, _inventory.Count - 1);
				var name = _inventory[index].name;
				_inventory.Remove(_inventory[index]);
				ConversationManager.OnTrapAvoided(name);
				LevelHandler.RemoveCurrentLevelTrap();
			}
		}
		else if (LevelHandler.CurrentLevel.IsTreasure)
		{
			npc.Hide();
			treasure.opened = LevelHandler.CurrentLevelTreasureContent.IsEmpty;
			treasure.Show();
			traps.Hide();
		}
		else
		{
			treasure.Hide();
			npc.Hide();
			traps.Hide();
		}
	}

	private void MoveToLevel(int portalIndex)
	{
		EmitSignal(SignalName.TransitionTriggered);
		var spawnLocation = SpawnMarkers[portalIndex].Position;
		var player = GetNode<player>("CanvasLayer/Player");
		player.NextTransform = player.Transform;
		player.NextTransform = player.NextTransform.Translated(spawnLocation - player.Position);
		player.Direction = (player.FacingDirection)portalIndex;
		player.ResetState = true;
		var offset =
			(portalIndex & 2) == 0
				? (portalIndex & 1) == 0 ? Vector2I.Up : Vector2I.Right
				: (portalIndex & 1) == 0
					? Vector2I.Down
					: Vector2I.Left;
		var NextLocation = CurrentLocation + offset;
		GD.Print($"Moving: {CurrentLocation} => {NextLocation}");
		LevelHandler.SetLocation(NextLocation);
		SetUpLevel();
	}

	void OnEnterPortalUp(Node2D _)
	{
		_interaction = InteractionType.UsePortalUp;
	}

	void OnEnterPortalRight(Node2D _)
	{
		_interaction = InteractionType.UsePortalRight;
	}

	void OnEnterPortalDown(Node2D _)
	{
		_interaction = InteractionType.UsePortalDown;
	}

	void OnEnterPortalLeft(Node2D _)
	{
		_interaction = InteractionType.UsePortalLeft;
	}

	void OnExitPortal(Node2D _)
	{
		if ((int)_interaction > 3) return;
		_interaction = InteractionType.None;
	}

	void OnNPCPlayerEnter(string name)
	{
		if (!LevelHandler.CurrentLevel.IsNPC) return;
		ConversationManager.SetInteractableCharacterName(name);
		_interaction = InteractionType.NPCConversation;
	}

	void OnNPCPlayerExit(string name)
	{
		if (name != ConversationManager.GetInteractableCharacterName() || !LevelHandler.CurrentLevel.IsNPC) return;
		ConversationManager.SetInteractableCharacterName(null);
		if (_interaction != InteractionType.NPCConversation) return;
		_interaction = InteractionType.None;
	}

	void OnTreasurePlayerEnter(Node2D _)
	{
		if (LevelHandler.CurrentLevel.IsTreasure)
			_interaction = InteractionType.OpenTreasureChest;
	}

	void OnTreasurePlayerExit(Node2D _)
	{
		if (LevelHandler.CurrentLevel.IsTreasure)
			_interaction = InteractionType.None;
	}

	void OnBackToMainMenuButtonPressed()
	{
		GetTree().ReloadCurrentScene();
		EmitSignal(SignalName.BackToMainMenu);
	}

	void OnResumeButtonPressed() => GetNode<CanvasLayer>("Pause").Visible ^= true;
}
