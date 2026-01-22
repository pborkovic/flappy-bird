using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Coin : Area2D
{
	public static float ScrollSpeed = 150.0f;
	private const float DestroyPositionX = -100.0f;
	private const float BounceAmount = 3.0f;
	private const float BounceSpeed = 5.0f;
	private bool _isCollected = false;
	private float _time = 0.0f;
	private float _baseY;

	[Signal]
	public delegate void CoinCollectedEventHandler();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		_baseY = Position.Y;
	}

	public override void _PhysicsProcess(double delta)
	{
		_time += (float)delta;
		float bounceOffset = Mathf.Sin(_time * BounceSpeed) * BounceAmount;

		Position = new Vector2(
			Position.X - ScrollSpeed * (float)delta,
			_baseY + bounceOffset
		);

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
