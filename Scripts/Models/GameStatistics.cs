using System;

namespace Flappy_Bird.Scripts.Models;

public class GameStatistics
{
	public int Id { get; set; }
	public int HighScore { get; set; }
	public int TotalGamesPlayed { get; set; }
	public int TotalDeaths { get; set; }
	public int TotalPipesPassed { get; set; }
	public int TotalCoinsCollected { get; set; }
	public DateTime LastPlayedDate { get; set; }
	public int AverageScore { get; set; }
}
