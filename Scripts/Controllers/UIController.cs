using Godot;
using Flappy_Bird.Scripts.Managers;
using Flappy_Bird.Scripts.Models;

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

	public override void _Ready()
	{
		_gameManager = GetNode<GameManager>(GameManagerPath);

		if (StartMenuContainerPath != null && !StartMenuContainerPath.IsEmpty)
			_startMenuContainer = GetNode<Control>(StartMenuContainerPath);

		if (StartButtonPath != null && !StartButtonPath.IsEmpty)
			_startButton = GetNode<Button>(StartButtonPath);

		if (MenuHighScoreLabelPath != null && !MenuHighScoreLabelPath.IsEmpty)
			_menuHighScoreLabel = GetNode<Label>(MenuHighScoreLabelPath);

		if (TotalGamesLabelPath != null && !TotalGamesLabelPath.IsEmpty)
			_totalGamesLabel = GetNode<Label>(TotalGamesLabelPath);

		if (TotalCoinsLabelPath != null && !TotalCoinsLabelPath.IsEmpty)
			_totalCoinsLabel = GetNode<Label>(TotalCoinsLabelPath);

		if (AverageScoreLabelPath != null && !AverageScoreLabelPath.IsEmpty)
			_averageScoreLabel = GetNode<Label>(AverageScoreLabelPath);

		if (ScoreLabelPath != null && !ScoreLabelPath.IsEmpty)
			_scoreLabel = GetNode<Label>(ScoreLabelPath);

		if (CoinLabelPath != null && !CoinLabelPath.IsEmpty)
			_coinLabel = GetNode<Label>(CoinLabelPath);

		if (GameOverContainerPath != null && !GameOverContainerPath.IsEmpty)
			_gameOverContainer = GetNode<Control>(GameOverContainerPath);

		if (FinalScoreLabelPath != null && !FinalScoreLabelPath.IsEmpty)
			_finalScoreLabel = GetNode<Label>(FinalScoreLabelPath);

		if (HighScoreLabelPath != null && !HighScoreLabelPath.IsEmpty)
			_highScoreLabel = GetNode<Label>(HighScoreLabelPath);

		if (CoinsCollectedLabelPath != null && !CoinsCollectedLabelPath.IsEmpty)
			_coinsCollectedLabel = GetNode<Label>(CoinsCollectedLabelPath);

		if (RestartButtonPath != null && !RestartButtonPath.IsEmpty)
			_restartButton = GetNode<Button>(RestartButtonPath);

		ConnectSignals();
		ShowMenuUI();
	}

	private void ConnectSignals()
	{
		if (_startButton != null)
			_startButton.Pressed += OnStartButtonPressed;

		if (_restartButton != null)
			_restartButton.Pressed += OnRestartButtonPressed;

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
			_scoreLabel.Text = newScore.ToString();
	}

	private void OnCoinsChanged(int totalCoins)
	{
		if (_coinLabel != null)
			_coinLabel.Text = $"Coins: {totalCoins}";
	}

	private void ShowMenuUI()
	{
		if (_startMenuContainer != null)
		{
			_startMenuContainer.Visible = true;
			LoadStatistics();
		}

		if (_scoreLabel != null)
			_scoreLabel.Visible = false;

		if (_coinLabel != null)
			_coinLabel.Visible = false;

		if (_gameOverContainer != null)
			_gameOverContainer.Visible = false;
	}

	private void LoadStatistics()
	{
		if (_gameManager == null)
			return;

		try
		{
			GameStatistics stats = _gameManager.GetDatabaseService().GetStatistics();

			if (_menuHighScoreLabel != null)
				_menuHighScoreLabel.Text = $"High Score: {stats.HighScore}";

			if (_totalGamesLabel != null)
				_totalGamesLabel.Text = $"Total Games: {stats.TotalGamesPlayed}";

			if (_totalCoinsLabel != null)
				_totalCoinsLabel.Text = $"Total Coins: {stats.TotalCoinsCollected}";

			if (_averageScoreLabel != null)
				_averageScoreLabel.Text = $"Average Score: {stats.AverageScore}";
		}
		catch (System.Exception) { }
	}

	private void ShowGameplayUI()
	{
		if (_startMenuContainer != null)
			_startMenuContainer.Visible = false;

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
			_gameOverContainer.Visible = false;
	}

	private void ShowGameOverUI(int finalScore)
	{
		if (_startMenuContainer != null)
			_startMenuContainer.Visible = false;

		if (_scoreLabel != null)
			_scoreLabel.Visible = false;

		if (_coinLabel != null)
			_coinLabel.Visible = false;

		if (_gameOverContainer != null)
			_gameOverContainer.Visible = true;

		if (_finalScoreLabel != null)
			_finalScoreLabel.Text = $"Score: {finalScore}";

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
			_coinsCollectedLabel.Text = $"Coins: {_gameManager.GetCoinsCollected()}";
	}
}
