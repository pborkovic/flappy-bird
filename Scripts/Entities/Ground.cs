using Godot;

namespace Flappy_Bird.Scripts.Entities;

public partial class Ground : StaticBody2D
{
	public override void _Ready()
	{
		// Bird will collide with it via CharacterBody2D physics
	}
}
