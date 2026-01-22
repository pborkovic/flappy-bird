using Godot;
using Flappy_Bird.Scripts.Managers;
using Flappy_Bird.Scripts.Models;

namespace Flappy_Bird.Scripts.Controllers;

public partial class UIController : CanvasLayer
{
	[Export] public NodePath GameManagerPath;
	[Export] public NodePath StartButtonPath;
	[Export] public NodePath ScoreLabelPath;
	[Export] public NodePath CoinLabelPath;
	[Export] public NodePath GameOverContainerPath;
	[Export] public NodePath FinalScoreLabelPath;
	[Export] public NodePath HighScoreLabelPath;
	[Export] public NodePath CoinsCollectedLabelPath;
	[Export] public NodePath RestartButtonPath;
	private GameManager _gameManager;
	private Button _startButton;
	private Label _scoreLabel;
	private Label _coinLabel;
	private Control _gameOverContainer;
	private Label _finalScoreLabel;
	private Label _highScoreLabel;
	private Label _coinsCollectedLabel;
	private Button _restartButton;

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>(GameManagerPath);

		if (StartButtonPath != null && !StartButtonPath.IsEmpty)
		{
			_startButton = GetNode<Button>(StartButtonPath);
		}

		if (ScoreLabelPath != null && !ScoreLabelPath.IsEmpty)
		{
			_scoreLabel = GetNode<Label>(ScoreLabelPath);
		}

		if (CoinLabelPath != null && !CoinLabelPath.IsEmpty)
		{
			_coinLabel = GetNode<Label>(CoinLabelPath);
		}

		if (GameOverContainerPath != null && !GameOverContainerPath.IsEmpty)
		{
			_gameOverContainer = GetNode<Control>(GameOverContainerPath);
		}

		if (FinalScoreLabelPath != null && !FinalScoreLabelPath.IsEmpty)
		{
			_finalScoreLabel = GetNode<Label>(FinalScoreLabelPath);
		}

		if (HighScoreLabelPath != null && !HighScoreLabelPath.IsEmpty)
		{
			_highScoreLabel = GetNode<Label>(HighScoreLabelPath);
		}

		if (CoinsCollectedLabelPath != null && !CoinsCollectedLabelPath.IsEmpty)
		{
			_coinsCollectedLabel = GetNode<Label>(CoinsCollectedLabelPath);
		}

		if (RestartButtonPath != null && !RestartButtonPath.IsEmpty)
		{
			_restartButton = GetNode<Button>(RestartButtonPath);
		}

		ConnectSignals();
		ShowMenuUI();
	}

	private void ConnectSignals()
	{
		if (_startButton != null)
		{
			_startButton.Pressed += OnStartButtonPressed;
		}

		if (_restartButton != null)
		{
			_restartButton.Pressed += OnRestartButtonPressed;
		}

		if (_gameManager != null)
		{
			_gameManager.GameStarted += OnGameStarted;
			_gameManager.GameOver += OnGameOver;
			_gameManager.ScoreChanged += OnScoreChanged;
			_gameManager.CoinsChanged += OnCoinsChanged;
		}
	}

	private void OnStartButtonPressed()
	{
		_gameManager?.StartGame();
	}

	private void OnRestartButtonPressed()
	{
		_gameManager?.RestartGame();
	}

	private void OnGameStarted()
	{
		ShowGameplayUI();
	}

	private void OnGameOver(int finalScore)
	{
		ShowGameOverUI(finalScore);
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

	private void ShowMenuUI()
	{
		if (_startButton != null)
		{
			_startButton.Visible = true;
		}

		if (_scoreLabel != null)
		{
			_scoreLabel.Visible = false;
		}

		if (_coinLabel != null)
		{
			_coinLabel.Visible = false;
		}

		if (_gameOverContainer != null)
		{
			_gameOverContainer.Visible = false;
		}
	}

	private void ShowGameplayUI()
	{
		if (_startButton != null)
		{
			_startButton.Visible = false;
		}

		if (_scoreLabel != null)
		{
			_scoreLabel.Visible = true;
			_scoreLabel.Text = "0";
		}

		if (_coinLabel != null)
		{
			_coinLabel.Visible = true;
			_coinLabel.Text = "Coins: 0";
		}

		if (_gameOverContainer != null)
		{
			_gameOverContainer.Visible = false;
		}
	}

	private void ShowGameOverUI(int finalScore)
	{
		if (_startButton != null)
		{
			_startButton.Visible = false;
		}

		if (_scoreLabel != null)
		{
			_scoreLabel.Visible = false;
		}

		if (_coinLabel != null)
		{
			_coinLabel.Visible = false;
		}

		if (_gameOverContainer != null)
		{
			_gameOverContainer.Visible = true;
		}

		if (_finalScoreLabel != null)
		{
			_finalScoreLabel.Text = $"Score: {finalScore}";
		}

		if (_highScoreLabel != null && _gameManager != null)
		{
			GameStatistics stats = _gameManager.GetDatabaseService().GetStatistics();
			_highScoreLabel.Text = $"High Score: {stats.HighScore}";
		}

		if (_coinsCollectedLabel != null && _gameManager != null)
		{
			_coinsCollectedLabel.Text = $"Coins: {_gameManager.GetCoinsCollected()}";
		}
	}
}
