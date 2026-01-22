using Godot;
using Flappy_Bird.Scripts.Entities;

namespace Flappy_Bird.Scripts.Managers;

public partial class CoinSpawner : Node
{
	private const float UpperPipeTip = -310.0f;
	private const float LowerPipeTip = 110.0f;
	private const float CoinMargin = 60.0f;
	private const float CoinSpawnChance = 0.7f;

	private PackedScene _coinScene;
	private bool _isSpawning = false;
	private int _coinsCollected = 0;

	[Signal]
	public delegate void CoinCollectedEventHandler(int totalCoins);

	public override void _Ready()
	{
		_coinScene = GD.Load<PackedScene>("res://coin.tscn");
	}

	public void StartSpawning()
	{
		_isSpawning = true;
		_coinsCollected = 0;
	}

	public void StopSpawning()
	{
		_isSpawning = false;
	}

	public void ClearAllCoins()
	{
		Godot.Collections.Array<Node> coins = GetTree().GetNodesInGroup("coins");

		foreach (Node coin in coins)
		{
			coin.QueueFree();
		}
	}

	public void TrySpawnCoinAtGap(float gapY, float positionX)
	{
		if (!_isSpawning || _coinScene == null)
			return;

		if (GD.Randf() > CoinSpawnChance)
			return;

		Coin coin = _coinScene.Instantiate<Coin>();

		float safeMinY = gapY + UpperPipeTip + CoinMargin;
		float safeMaxY = gapY + LowerPipeTip - CoinMargin;

		float coinY = (float)GD.RandRange(safeMinY, safeMaxY);

		coin.Position = new Vector2(positionX, coinY);
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
}
