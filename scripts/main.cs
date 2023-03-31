using System.Threading.Tasks;
using Godot;

public partial class main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<AudioStreamPlayer2D>("BGM").Play();
		GetNode<start>("Start").Show();
		GetNode<wander>("Wander").Hide();
		GetNode<wander>("Wander").SetProcess(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	public void OnVolumeSettingChanged(float value)
	{
		GetNode<AudioStreamPlayer2D>("BGM").VolumeDb = value == 0.001f ? -100.0f : Mathf.Log(value) * 5.0f;
	}

	public void OnStartGame()
	{
		GetNode<start>("Start").Hide();
		var wander = GetNode<wander>("Wander");
		wander.Show();
		wander.SetProcess(true);
	}

	public void OnBackToMainMenu()
	{
		GetNode<start>("Start").Show();
		var wander = GetNode<wander>("Wander");
		wander.Hide();
		GetNode<wander>("Wander").SetProcess(false);
	}
	public void OnBGMFinished()
	{
		GetNode<AudioStreamPlayer2D>("BGM").Play();
	}

	public void OnTransitionTriggered()
	{
		Task.Run(async () =>
		{
			GetNode<CanvasLayer>("Transition").Show();
			(GetNode<ColorRect>("Transition/ColorRect").Material as ShaderMaterial)?.SetShaderParameter("opacity", 1.0f);
			await Task.Delay(200);
			for (int i = 0; i < 10; i++)
			{
				(GetNode<ColorRect>("Transition/ColorRect").Material as ShaderMaterial)?.SetShaderParameter("opacity", 1.0 - i * 0.1f);
				await Task.Delay(20);
			}
			GetNode<CanvasLayer>("Transition").Hide();
		});
	}
}
