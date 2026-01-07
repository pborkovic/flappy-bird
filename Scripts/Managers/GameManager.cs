using System;
using Godot;
using Flappy_Bird.Scripts.Entities;
using Flappy_Bird.Scripts.Services;
using Flappy_Bird.Scripts.Utils.Enums;

namespace Flappy_Bird.Scripts.Managers;

public partial class GameManager : Node
{
	private const float BirdStartX = 200.0f;
	private const float BirdStartY = 540.0f;
	[Export] public NodePath BirdPath;
	[Export] public NodePath SpawnManagerPath;
	private Bird _bird;
	private SpawnManager _spawnManager;
	private DatabaseService _databaseService;
	private GameState _currentState = GameState.Menu;
	private int _score = 0;
	private DateTime _sessionStartTime;

	[Signal]
	public delegate void GameStartedEventHandler();

	[Signal]
	public delegate void GameOverEventHandler(int finalScore);

	[Signal]
	public delegate void ScoreChangedEventHandler(int newScore);

	public override void _Ready()
	{
		_bird = GetNode<Bird>(BirdPath);
		_spawnManager = GetNode<SpawnManager>(SpawnManagerPath);
		_databaseService = new DatabaseService();

		if (_bird != null)
		{
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
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
			}

			CheckBirdOutOfBounds();
		}
	}

	public void StartGame()
	{
		if (_currentState == GameState.Playing)
			return;

		_currentState = GameState.Playing;
		_score = 0;
		_sessionStartTime = DateTime.Now;

		if (_bird != null)
		{
			_bird.Reset();
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
		}

		if (_spawnManager != null)
		{
			_spawnManager.ClearAllPipes();
			_spawnManager.StartSpawning();
		}

		EmitSignal(SignalName.GameStarted);
		EmitSignal(SignalName.ScoreChanged, _score);
	}

	public void EndGame()
	{
		if (_currentState != GameState.Playing)
			return;

		_currentState = GameState.GameOver;

		if (_spawnManager != null)
		{
			_spawnManager.StopSpawning();
		}

		var sessionDuration = (DateTime.Now - _sessionStartTime).TotalSeconds;
		_databaseService.SaveGameSession(_score, _score, sessionDuration);

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

		if (_bird != null)
		{
			_bird.Reset();
			_bird.Position = new Vector2(BirdStartX, BirdStartY);
		}

		_score = 0;
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
		EmitSignal(SignalName.ScoreChanged, _score);
	}

	public GameState GetCurrentState() => _currentState;

	public int GetScore() => _score;

	public DatabaseService GetDatabaseService() => _databaseService;
}
