using System;

namespace Flappy_Bird.Scripts.Models;

public class GameSession
{
	public int Id { get; set; }
	public int Score { get; set; }
	public int PipesPassed { get; set; }
	public DateTime PlayedDate { get; set; }
	public double SessionDuration { get; set; }
}
