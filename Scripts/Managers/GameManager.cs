using System;
using Godot;
using Flappy_Bird.Scripts.Entities;
using Flappy_Bird.Scripts.Services;
using Flappy_Bird.Scripts.Systems;
using Flappy_Bird.Scripts.Utils.Enums;

namespace Flappy_Bird.Scripts.Managers;

public partial class GameManager : Node
{
	private const float BirdStartX = 200.0f;
	private const float BirdStartY = 540.0f;
	[Export] public NodePath BirdPath;
	[Export] public NodePath SpawnManagerPath;
	[Export] public NodePath CoinSpawnerPath;
	private Bird _bird;
	private SpawnManager _spawnManager;
	private CoinSpawner _coinSpawner;
	private DatabaseService _databaseService;
	private DifficultyScaler _difficultyScaler;
	private GameState _currentState = GameState.Menu;
	private int _score = 0;
	private int _coinsCollected = 0;
	private DateTime _sessionStartTime;

	[Signal]
	public delegate void GameStartedEventHandler();

	[Signal]
	public delegate void GameOverEventHandler(int finalScore);

	[Signal]
	public delegate void ScoreChangedEventHandler(int newScore);

	[Signal]
	public delegate void CoinsChangedEventHandler(int totalCoins);

	public override void _Ready()
	{
		_bird = GetNode<Bird>(BirdPath);
		_spawnManager = GetNode<SpawnManager>(SpawnManagerPath);
		if (CoinSpawnerPath != null)
			_coinSpawner = GetNodeOrNull<CoinSpawner>(CoinSpawnerPath);
		_databaseService = new DatabaseService();
		_difficultyScaler = new DifficultyScaler();

		if (_bird != null)
		{
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
			_bird.Died += OnBirdDied;
		}

		if (_coinSpawner != null)
		{
			_coinSpawner.CoinCollected += OnCoinCollected;
		}

		ConnectPipeSignals();
	}

	public override void _Process(double delta)
	{
		if (_currentState == GameState.Playing)
		{
			if (_bird != null && !_bird.IsAlive())
			{
				EndGame();
				return;
			}

			CheckBirdOutOfBounds();
			CheckPipeCollision();
		}
	}

	private void CheckPipeCollision()
	{
		if (_bird == null || !_bird.IsAlive())
			return;

		Vector2 birdPos = _bird.Position;
		float birdRadius = 20.0f;

		Godot.Collections.Array<Node> pipes = GetTree().GetNodesInGroup("pipes");
		foreach (Node node in pipes)
		{
			if (node is Pipe pipe)
			{
				float pipeX = pipe.Position.X;
				float pipeY = pipe.Position.Y;
				float pipeHalfWidth = 104.0f;

				if (birdPos.X + birdRadius > pipeX - pipeHalfWidth &&
				    birdPos.X - birdRadius < pipeX + pipeHalfWidth)
				{
					float upperPipeBottom = pipeY - 310.0f;
					float lowerPipeTop = pipeY + 110.0f;

					if (birdPos.Y - birdRadius < upperPipeBottom ||
					    birdPos.Y + birdRadius > lowerPipeTop)
					{
						_bird.Kill();
						EndGame();
						return;
					}
				}
			}
		}
	}

	private void OnBirdDied()
	{
		if (_currentState == GameState.Playing)
		{
			EndGame();
		}
	}

	public void StartGame()
	{
		if (_currentState == GameState.Playing)
			return;

		_currentState = GameState.Playing;
		_score = 0;
		_coinsCollected = 0;
		_sessionStartTime = DateTime.Now;

		_difficultyScaler.Reset();
		Pipe.ScrollSpeed = _difficultyScaler.GetScrollSpeed();
		Coin.ScrollSpeed = _difficultyScaler.GetScrollSpeed();

		if (_bird != null)
		{
			_bird.Reset();
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
		}

		if (_spawnManager != null)
		{
			_spawnManager.ClearAllPipes();
			_spawnManager.SetSpawnInterval(_difficultyScaler.GetSpawnInterval());
			_spawnManager.StartSpawning();
		}

		if (_coinSpawner != null)
		{
			_coinSpawner.ClearAllCoins();
			_coinSpawner.StartSpawning();
		}

		EmitSignal(SignalName.GameStarted);
		EmitSignal(SignalName.ScoreChanged, _score);
		EmitSignal(SignalName.CoinsChanged, _coinsCollected);
	}

	public void EndGame()
	{
		GD.Print("EndGame called");
		if (_currentState == GameState.GameOver)
			return;

		_currentState = GameState.GameOver;

		_spawnManager?.StopSpawning();
		_coinSpawner?.StopSpawning();

		try
		{
			double sessionDuration = (DateTime.Now - _sessionStartTime).TotalSeconds;
			_databaseService?.SaveGameSession(_score, _score, _coinsCollected, sessionDuration);
		}
		catch (Exception) { }

		EmitSignal(SignalName.GameOver, _score);
	}

	public void RestartGame()
	{
		if (_currentState == GameState.Playing)
		{
			EndGame();
		}

		CallDeferred(nameof(StartGame));
	}

	public void ReturnToMenu()
	{
		_currentState = GameState.Menu;

		if (_spawnManager != null)
		{
			_spawnManager.StopSpawning();
			_spawnManager.ClearAllPipes();
		}

		if (_coinSpawner != null)
		{
			_coinSpawner.StopSpawning();
			_coinSpawner.ClearAllCoins();
		}

		if (_bird != null)
		{
			_bird.Reset();
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
		}

		_score = 0;
		_coinsCollected = 0;
	}

	private void CheckBirdOutOfBounds()
	{
		if (_bird == null)
			return;

		if (_bird.Position.Y < -50 || _bird.Position.Y > 1130)
		{
			if (_bird.IsAlive())
			{
				_bird.Kill();
			}
		}
	}

	private void ConnectPipeSignals()
	{
		GetTree().Connect("node_added", new Callable(this, nameof(OnNodeAdded)));
	}

	private void OnNodeAdded(Node node)
	{
		if (node is Pipe pipe)
		{
			pipe.PipePassed += OnPipePassed;
		}
	}

	private void OnPipePassed()
	{
		if (_currentState != GameState.Playing)
			return;

		_score++;

		_difficultyScaler.UpdateDifficulty(_score);
		Pipe.ScrollSpeed = _difficultyScaler.GetScrollSpeed();
		Coin.ScrollSpeed = _difficultyScaler.GetScrollSpeed();

		if (_spawnManager != null)
		{
			_spawnManager.SetSpawnInterval(_difficultyScaler.GetSpawnInterval());
		}

		EmitSignal(SignalName.ScoreChanged, _score);
	}

	private void OnCoinCollected(int totalCoins)
	{
		if (_currentState != GameState.Playing)
			return;

		_coinsCollected = totalCoins;
		EmitSignal(SignalName.CoinsChanged, _coinsCollected);
	}

	public GameState GetCurrentState() => _currentState;

	public int GetScore() => _score;

	public int GetCoinsCollected() => _coinsCollected;

	public DatabaseService GetDatabaseService() => _databaseService;

	public int GetDifficultyLevel() => _difficultyScaler.GetDifficultyLevel();
}
