using Godot;
using Flappy_Bird.Scripts.Managers;
using Flappy_Bird.Scripts.Models;
using Flappy_Bird.Scripts.Utils.Enums;

namespace Flappy_Bird.Scripts.Controllers;

public partial class UIController : CanvasLayer
{
	[Export] public NodePath GameManagerPath;
	[Export] public NodePath StartMenuContainerPath;
	[Export] public NodePath StartButtonPath;
	[Export] public NodePath MenuHighScoreLabelPath;
	[Export] public NodePath TotalGamesLabelPath;
	[Export] public NodePath TotalCoinsLabelPath;
	[Export] public NodePath AverageScoreLabelPath;
	[Export] public NodePath ScoreLabelPath;
	[Export] public NodePath CoinLabelPath;
	[Export] public NodePath GameOverContainerPath;
	[Export] public NodePath FinalScoreLabelPath;
	[Export] public NodePath HighScoreLabelPath;
	[Export] public NodePath CoinsCollectedLabelPath;
	[Export] public NodePath RestartButtonPath;

	private GameManager _gameManager;
	private Control _startMenuContainer;
	private Button _startButton;
	private Label _menuHighScoreLabel;
	private Label _totalGamesLabel;
	private Label _totalCoinsLabel;
	private Label _averageScoreLabel;
	private Label _scoreLabel;
	private Label _coinLabel;
	private Control _gameOverContainer;
	private Label _finalScoreLabel;
	private Label _highScoreLabel;
	private Label _coinsCollectedLabel;
	private Button _restartButton;
	private int _lastFinalScore;

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>(GameManagerPath);
		InitializeNodes();
		ConnectSignals();
		SetUIState(UIState.Menu);
	}

	private void InitializeNodes()
	{
		_startMenuContainer = GetNodeOrNull<Control>(StartMenuContainerPath);
		_startButton = GetNodeOrNull<Button>(StartButtonPath);
		_menuHighScoreLabel = GetNodeOrNull<Label>(MenuHighScoreLabelPath);
		_totalGamesLabel = GetNodeOrNull<Label>(TotalGamesLabelPath);
		_totalCoinsLabel = GetNodeOrNull<Label>(TotalCoinsLabelPath);
		_averageScoreLabel = GetNodeOrNull<Label>(AverageScoreLabelPath);
		_scoreLabel = GetNodeOrNull<Label>(ScoreLabelPath);
		_coinLabel = GetNodeOrNull<Label>(CoinLabelPath);
		_gameOverContainer = GetNodeOrNull<Control>(GameOverContainerPath);
		_finalScoreLabel = GetNodeOrNull<Label>(FinalScoreLabelPath);
		_highScoreLabel = GetNodeOrNull<Label>(HighScoreLabelPath);
		_coinsCollectedLabel = GetNodeOrNull<Label>(CoinsCollectedLabelPath);
		_restartButton = GetNodeOrNull<Button>(RestartButtonPath);
	}

	private void ConnectSignals()
	{
		_startButton?.Connect("pressed", new Callable(this, nameof(OnStartButtonPressed)));
		_restartButton?.Connect("pressed", new Callable(this, nameof(OnRestartButtonPressed)));

		if (_gameManager == null)
		{
			return;
		}

		_gameManager.GameStarted += OnGameStarted;
		_gameManager.GameOver += OnGameOver;
		_gameManager.ScoreChanged += OnScoreChanged;
		_gameManager.CoinsChanged += OnCoinsChanged;
	}

	private void OnStartButtonPressed() => _gameManager?.StartGame();

	private void OnRestartButtonPressed() => _gameManager?.RestartGame();

	private void OnGameStarted() => SetUIState(UIState.Playing);

	private void OnGameOver(int finalScore)
	{
		_lastFinalScore = finalScore;
		SetUIState(UIState.GameOver);
	}

	private void OnScoreChanged(int newScore)
	{
		if (_scoreLabel != null)
		{
			_scoreLabel.Text = newScore.ToString();
		}
	}

	private void OnCoinsChanged(int totalCoins)
	{
		if (_coinLabel != null)
		{
			_coinLabel.Text = $"Coins: {totalCoins}";
		}
	}

	private void SetUIState(UIState state)
	{
		switch (state)
		{
			case UIState.Menu:
				SetVisibility(startMenu: true, gameplay: false, gameOver: false);
				LoadStatistics();
				break;

			case UIState.Playing:
				SetVisibility(startMenu: false, gameplay: true, gameOver: false);
				ResetGameplayLabels();
				break;

			case UIState.GameOver:
				SetVisibility(startMenu: false, gameplay: false, gameOver: true);
				UpdateGameOverLabels();
				break;
		}
	}

	private void SetVisibility(bool startMenu, bool gameplay, bool gameOver)
	{
		if (_startMenuContainer != null)
		{
			_startMenuContainer.Visible = startMenu;
		}

		if (_scoreLabel != null)
		{
			_scoreLabel.Visible = gameplay;
		}

		if (_coinLabel != null)
		{
			_coinLabel.Visible = gameplay;
		}

		if (_gameOverContainer != null)
		{
			_gameOverContainer.Visible = gameOver;
		}
	}

	private void ResetGameplayLabels()
	{
		if (_scoreLabel != null)
		{
			_scoreLabel.Text = "0";
		}

		if (_coinLabel != null)
		{
			_coinLabel.Text = "Coins: 0";
		}
	}

	private void LoadStatistics()
	{
		if (_gameManager == null)
		{
			return;
		}

		try
		{
			GameStatistics stats = _gameManager.GetDatabaseService().GetStatistics();

			if (_menuHighScoreLabel != null)
			{
				_menuHighScoreLabel.Text = $"High Score: {stats.HighScore}";
			}

			if (_totalGamesLabel != null)
			{
				_totalGamesLabel.Text = $"Total Games: {stats.TotalGamesPlayed}";
			}

			if (_totalCoinsLabel != null)
			{
				_totalCoinsLabel.Text = $"Total Coins: {stats.TotalCoinsCollected}";
			}

			if (_averageScoreLabel != null)
			{
				_averageScoreLabel.Text = $"Average Score: {stats.AverageScore}";
			}
		}
		catch (System.Exception) { }
	}

	private void UpdateGameOverLabels()
	{
		if (_finalScoreLabel != null)
		{
			_finalScoreLabel.Text = $"Score: {_lastFinalScore}";
		}

		if (_highScoreLabel != null && _gameManager != null)
		{
			try
			{
				GameStatistics stats = _gameManager.GetDatabaseService().GetStatistics();
				_highScoreLabel.Text = $"High Score: {stats.HighScore}";
			}
			catch (System.Exception) { }
		}

		if (_coinsCollectedLabel != null && _gameManager != null)
		{
			_coinsCollectedLabel.Text = $"Coins: {_gameManager.GetCoinsCollected()}";
		}
	}
}
