using Godot;
using Flappy_Bird.Scripts.Entities;

namespace Flappy_Bird.Scripts.Managers;

public partial class CoinSpawner : Node
{
	private const float SpawnInterval = 2.5f;
	private const float SpawnPositionX = 2000.0f;
	private const float MinGapY = 400.0f;
	private const float MaxGapY = 650.0f;
	private const float UpperPipeTip = -310.0f;
	private const float LowerPipeTip = 110.0f;
	private const float CoinMargin = 50.0f;

	private PackedScene _coinScene;
	private Timer _spawnTimer;
	private bool _isSpawning = false;
	private int _coinsCollected = 0;

	[Signal]
	public delegate void CoinCollectedEventHandler(int totalCoins);

	public override void _Ready()
	{
		_coinScene = GD.Load<PackedScene>("res://coin.tscn");

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Timeout += OnSpawnTimerTimeout;

		AddChild(_spawnTimer);
	}

	public void StartSpawning()
	{
		_isSpawning = true;
		_coinsCollected = 0;
		_spawnTimer.Start();
	}

	public void StopSpawning()
	{
		_isSpawning = false;
		_spawnTimer.Stop();
	}

	public void ClearAllCoins()
	{
		Godot.Collections.Array<Node> coins = GetTree().GetNodesInGroup("coins");

		foreach (Node coin in coins)
		{
			coin.QueueFree();
		}
	}

	private void OnSpawnTimerTimeout()
	{
		if (_isSpawning)
		{
			SpawnCoin();
		}
	}

	private void SpawnCoin()
	{
		if (_coinScene == null)
			return;

		Coin coin = _coinScene.Instantiate<Coin>();

		float pipeGapY = (float)GD.RandRange(MinGapY, MaxGapY);

		float safeMinY = pipeGapY + UpperPipeTip + CoinMargin;
		float safeMaxY = pipeGapY + LowerPipeTip - CoinMargin;

		float coinY = (float)GD.RandRange(safeMinY, safeMaxY);

		coin.Position = new Vector2(SpawnPositionX, coinY);
		coin.AddToGroup("coins");

		coin.CoinCollected += OnCoinCollected;

		GetParent().AddChild(coin);
	}

	private void OnCoinCollected()
	{
		_coinsCollected++;
		EmitSignal(SignalName.CoinCollected, _coinsCollected);
	}

	public int GetCoinsCollected() => _coinsCollected;

	public void SetSpawnInterval(float interval)
	{
		if (_spawnTimer != null)
		{
			_spawnTimer.WaitTime = interval;
		}
	}
}
