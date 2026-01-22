using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Pipe : Area2D
{
	public static float ScrollSpeed = 150.0f;
	private const float DestroyPositionX = -200.0f;
	private bool _hasBeenPassed = false;
	private bool _hasHitBird = false;

	[Signal]
	public delegate void PipePassedEventHandler();

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		Monitoring = true;
	}

	public override void _PhysicsProcess(double delta)
	{
		Position += new Vector2(-ScrollSpeed * (float)delta, 0);

		if (!_hasHitBird && HasOverlappingBodies())
		{
			foreach (Node2D body in GetOverlappingBodies())
			{
				if (body is Bird bird && bird.IsAlive())
				{
					_hasHitBird = true;
					bird.Kill();
					
					break;
				}
			}
		}

		if (!_hasBeenPassed && Position.X < 100)
		{
			_hasBeenPassed = true;
			EmitSignal(SignalName.PipePassed);
		}

		if (Position.X < DestroyPositionX)
		{
			QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (!_hasHitBird && body is Bird bird && bird.IsAlive())
		{
			_hasHitBird = true;
			bird.Kill();
		}
	}

	public void SetGapPosition(float gapY)
	{
		Position = new Vector2(Position.X, gapY);
	}

	public bool HasBeenPassed() => _hasBeenPassed;
}
