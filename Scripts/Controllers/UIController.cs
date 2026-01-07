using Godot;
using Flappy_Bird.Scripts.Managers;

namespace Flappy_Bird.Scripts.Controllers;

public partial class UIController : CanvasLayer
{
	[Export] public NodePath GameManagerPath;
	[Export] public NodePath StartButtonPath;
	[Export] public NodePath ScoreLabelPath;
	[Export] public NodePath GameOverContainerPath;
	[Export] public NodePath FinalScoreLabelPath;
	[Export] public NodePath RestartButtonPath;
	private GameManager _gameManager;
	private Button _startButton;
	private Label _scoreLabel;
	private Control _gameOverContainer;
	private Label _finalScoreLabel;
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

		if (GameOverContainerPath != null && !GameOverContainerPath.IsEmpty)
		{
			_gameOverContainer = GetNode<Control>(GameOverContainerPath);
		}

		if (FinalScoreLabelPath != null && !FinalScoreLabelPath.IsEmpty)
		{
			_finalScoreLabel = GetNode<Label>(FinalScoreLabelPath);
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

		if (_gameOverContainer != null)
		{
			_gameOverContainer.Visible = true;
		}

		if (_finalScoreLabel != null)
		{
			_finalScoreLabel.Text = $"Score: {finalScore}";
		}
	}
}
