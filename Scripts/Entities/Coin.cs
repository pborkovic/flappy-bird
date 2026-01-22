using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Coin : Area2D
{
	public static float ScrollSpeed = 150.0f;
	private const float DestroyPositionX = -100.0f;
	private bool _isCollected = false;

	[Signal]
	public delegate void CoinCollectedEventHandler();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += new Vector2(-ScrollSpeed * (float)delta, 0);

		if (Position.X < DestroyPositionX)
		{
			QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_isCollected)
			return;

		if (body is Bird)
		{
			_isCollected = true;
			EmitSignal(SignalName.CoinCollected);
			QueueFree();
		}
	}
}
