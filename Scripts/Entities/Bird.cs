using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Bird : CharacterBody2D
{
	private const float Gravity = 980.0f;
	private const float JumpVelocity = -350.0f;
	private const float MaxRotationDown = Mathf.Pi / 2;
	private const float MaxRotationUp = -Mathf.Pi / 6;
	private const float RotationSpeed = 0.003f;
	private const float GroundY = 1000.0f;
	private bool _isAlive = true;

	[Signal]
	public delegate void DiedEventHandler();

	public override void _PhysicsProcess(double delta)
	{
		if (!_isAlive)
			return;

		Vector2 velocity = Velocity;

		if (Input.IsActionJustPressed("ui_accept") ||
		    Input.IsKeyPressed(Key.Space) ||
		    Input.IsMouseButtonPressed(MouseButton.Left))
		{
			velocity.Y = JumpVelocity;
		}

		if (!IsOnFloor())
		{
			velocity.Y += Gravity * (float)delta;
		}

		float targetRotation = velocity.Y * RotationSpeed;
		targetRotation = Mathf.Clamp(targetRotation, MaxRotationUp, MaxRotationDown);
		Rotation = targetRotation;

		Velocity = velocity;
		MoveAndSlide();

		// Die if on floor or past ground level
		if (IsOnFloor() || Position.Y >= GroundY)
		{
			Kill();
		}
	}

	public void Kill()
	{
		if (!_isAlive)
			return;
		_isAlive = false;
		EmitSignal(SignalName.Died);
	}

	public void Reset()
	{
		_isAlive = true;
		Velocity = Vector2.Zero;
		Rotation = 0;
	}

	public bool IsAlive() => _isAlive;
}
