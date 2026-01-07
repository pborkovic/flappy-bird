using Godot;
using Flappy_Bird.Scripts.Entities;

namespace Flappy_Bird.Scripts.Managers;

public partial class SpawnManager : Node
{
	private const float SpawnInterval = 2.0f;
	private const float SpawnPositionX = 2000.0f;
	private const float MinGapY = 200.0f;
	private const float MaxGapY = 880.0f;
	private PackedScene _pipeScene;
	private Timer _spawnTimer;
	private bool _isSpawning = false;

	public override void _Ready()
	{
		_pipeScene = GD.Load<PackedScene>("res://pipe.tscn");

		_spawnTimer = new Timer();
		_spawnTimer.WaitTime = SpawnInterval;
		_spawnTimer.Timeout += OnSpawnTimerTimeout;
		
		AddChild(_spawnTimer);
	}

	public void StartSpawning()
	{
		_isSpawning = true;
		_spawnTimer.Start();
		SpawnPipe();
	}

	public void StopSpawning()
	{
		_isSpawning = false;
		_spawnTimer.Stop();
	}

	public void ClearAllPipes()
	{
		var pipes = GetTree().GetNodesInGroup("pipes");
		
		foreach (var pipe in pipes)
		{
			if (pipe is Node node)
			{
				node.QueueFree();
			}
		}
	}

	private void OnSpawnTimerTimeout()
	{
		if (_isSpawning)
		{
			SpawnPipe();
		}
	}

	private void SpawnPipe()
	{
		if (_pipeScene == null)
			return;

		var pipe = _pipeScene.Instantiate<Pipe>();
		var randomGapY = (float)GD.RandRange(MinGapY, MaxGapY);
		
		pipe.Position = new Vector2(SpawnPositionX, randomGapY);

		pipe.AddToGroup("pipes");

		GetParent().AddChild(pipe);
	}

	public void SetSpawnInterval(float interval)
	{
		if (_spawnTimer != null)
		{
			_spawnTimer.WaitTime = interval;
		}
	}
}
