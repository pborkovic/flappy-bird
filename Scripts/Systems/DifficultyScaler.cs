using Godot;

namespace Flappy_Bird.Scripts.Systems;

public class DifficultyScaler
{
	private const float BaseScrollSpeed = 150.0f;
	private const float MaxScrollSpeed = 400.0f;
	private const float SpeedIncreasePerPoint = 3.0f;
	private const float BaseSpawnInterval = 3.0f;
	private const float MinSpawnInterval = 1.5f;
	private const float IntervalDecreasePerPoint = 0.02f;
	private float _currentScrollSpeed = BaseScrollSpeed;
	private float _currentSpawnInterval = BaseSpawnInterval;

	public void UpdateDifficulty(int score)
	{
		_currentScrollSpeed = Mathf.Min(
			BaseScrollSpeed + (score * SpeedIncreasePerPoint),
			MaxScrollSpeed
		);

		_currentSpawnInterval = Mathf.Max(
			BaseSpawnInterval - (score * IntervalDecreasePerPoint),
			MinSpawnInterval
		);
	}

	public void Reset()
	{
		_currentScrollSpeed = BaseScrollSpeed;
		_currentSpawnInterval = BaseSpawnInterval;
	}

	public float GetScrollSpeed() => _currentScrollSpeed;

	public float GetSpawnInterval() => _currentSpawnInterval;

	public int GetDifficultyLevel()
	{
		float speedProgress = (_currentScrollSpeed - BaseScrollSpeed) / (MaxScrollSpeed - BaseScrollSpeed);
		return Mathf.Clamp((int)(speedProgress * 10) + 1, 1, 10);
	}
}
