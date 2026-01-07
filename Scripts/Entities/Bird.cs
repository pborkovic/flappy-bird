using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Bird : CharacterBody2D
{
	private const float Gravity = 980.0f;
	private const float JumpVelocity = -350.0f;
	private const float MaxRotationDown = Mathf.Pi / 2; // 90 degrees
	private const float MaxRotationUp = -Mathf.Pi / 6; // -30 degrees
	private const float RotationSpeed = 0.003f;
	private bool _isAlive = true;

	public override void _Ready()
	{
		// Initialization
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_isAlive)
			return;

		var velocity = Velocity;

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

		var targetRotation = velocity.Y * RotationSpeed;
		targetRotation = Mathf.Clamp(targetRotation, MaxRotationUp, MaxRotationDown);
		Rotation = targetRotation;

		Velocity = velocity;
		
		MoveAndSlide();
	}

	public void Kill()
	{
		_isAlive = false;
	}

	public void Reset()
	{
		_isAlive = true;
		Velocity = Vector2.Zero;
		Rotation = 0;
	}

	public bool IsAlive() => _isAlive;
}
