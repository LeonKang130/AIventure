using Godot;
using System;
using Godot.Collections;

public partial class wander : Node
{
    string npc = "Dialog/HSplitContainer/TextMargin/Label";
    string player = "Dialog/HSplitContainer/TextMargin/TextEdit";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}
	
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        // Skip dialog while player is listening
        if (GetNode<Label>(npc).Visible && Input.IsActionPressed("skip_dialog"))
        {
            GetNode<CanvasLayer>("Dialog").Hide();
            GetNode<Label>(npc).Text = "???: Back again?";  // To Be Implemented
        }

        // Enter the process of dialog, and start to talk
        // NPC: Listening
        // Player: Talking
        if (GetNode<Label>(npc).Visible && Input.IsActionPressed("start_continue_dialog"))
        {
            GetNode<Label>(npc).Hide();
            GetNode<TextEdit>(player).Show();
        }

        // Player finishes talking
        // NPC: Responding
        // Player: Listening
        if (GetNode<TextEdit>(player).Visible && Input.IsActionPressed("end_text_edit"))
        {
            string player_words = GetNode<TextEdit>(player).Text;
            GetNode<TextEdit>(player).Text = "";
            GetNode<TextEdit>(player).Hide();
            GetNode<Label>(npc).Text = "???: It sounds like nonsense...";  // To Be Implemented
            GetNode<Label>(npc).Show();
        }

        // Forced escape dialog  BUT CANNOT FUCKING TYPE Q
        // if (GetNode<TextEdit>(player).Visible && Input.IsActionPressed("skip_dialog"))
        // {
        //     GetNode<TextEdit>(player).Text = "";
        //     GetNode<TextEdit>(player).Hide();
        //     GetNode<Label>(npc).Text = "???: Back again?";  // To Be Implemented
        //     GetNode<Label>(npc).Show();
        // 	GetNode<CanvasLayer>("Dialog").Hide();
        // }

        // Escape Game: Back to the "Main" Scene
        if (Input.IsActionPressed("escape"))
        {
            GetParent<main>().GetNode<wander>("Wander").Hide();
            GetParent<main>().GetNode<start>("Start").Show();
        }

        // Shader
        var material = GetNode<ColorRect>("CanvasLayer/Vignette").Material as ShaderMaterial;
		if (material != null)
		{
			material.SetShaderParameter("player_pos", GetNode<player>("CanvasLayer/Player").Position / 1024);
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
        GetNode<Label>(npc).Text = "???: Where do you think you are going?";
    }
}
