using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Flappy_Bird.Scripts.Models;
using Godot;

namespace Flappy_Bird.Scripts.Services;

public class DatabaseService
{
	private readonly string _connectionString;
	private const string DatabaseFileName = "flappy_bird_stats.db";

	public DatabaseService()
	{
		string dbPath = System.IO.Path.Combine(OS.GetUserDataDir(), DatabaseFileName);
		_connectionString = $"Data Source={dbPath}";
		InitializeDatabase();
	}

	private void InitializeDatabase()
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		string createStatisticsTable = @"
			CREATE TABLE IF NOT EXISTS GameStatistics (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				HighScore INTEGER NOT NULL DEFAULT 0,
				TotalGamesPlayed INTEGER NOT NULL DEFAULT 0,
				TotalDeaths INTEGER NOT NULL DEFAULT 0,
				TotalPipesPassed INTEGER NOT NULL DEFAULT 0,
				TotalCoinsCollected INTEGER NOT NULL DEFAULT 0,
				LastPlayedDate TEXT NOT NULL,
				AverageScore INTEGER NOT NULL DEFAULT 0
			)";

		string createSessionsTable = @"
			CREATE TABLE IF NOT EXISTS GameSessions (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				Score INTEGER NOT NULL,
				PipesPassed INTEGER NOT NULL,
				CoinsCollected INTEGER NOT NULL DEFAULT 0,
				PlayedDate TEXT NOT NULL,
				SessionDuration REAL NOT NULL
			)";

		using (SqliteCommand command = new SqliteCommand(createStatisticsTable, connection))
		{
			command.ExecuteNonQuery();
		}

		using (SqliteCommand command = new SqliteCommand(createSessionsTable, connection))
		{
			command.ExecuteNonQuery();
		}

		MigrateDatabase(connection);
		EnsureStatisticsRecordExists(connection);
	}

	private void MigrateDatabase(SqliteConnection connection)
	{
		try
		{
			using (SqliteCommand command = new SqliteCommand(
				"ALTER TABLE GameStatistics ADD COLUMN TotalCoinsCollected INTEGER NOT NULL DEFAULT 0", connection))
			{
				command.ExecuteNonQuery();
			}
		}
		catch (SqliteException) { }

		try
		{
			using (SqliteCommand command = new SqliteCommand(
				"ALTER TABLE GameSessions ADD COLUMN CoinsCollected INTEGER NOT NULL DEFAULT 0", connection))
			{
				command.ExecuteNonQuery();
			}
		}
		catch (SqliteException) { }
	}

	private void EnsureStatisticsRecordExists(SqliteConnection connection)
	{
		string checkQuery = "SELECT COUNT(*) FROM GameStatistics";
		using SqliteCommand checkCommand = new SqliteCommand(checkQuery, connection);
		int count = Convert.ToInt32(checkCommand.ExecuteScalar());

		if (count == 0)
		{
			string insertQuery = @"
				INSERT INTO GameStatistics (HighScore, TotalGamesPlayed, TotalDeaths, TotalPipesPassed, TotalCoinsCollected, LastPlayedDate, AverageScore)
				VALUES (0, 0, 0, 0, 0, @date, 0)";

			using SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection);
			insertCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
			insertCommand.ExecuteNonQuery();
		}
	}

	public void SaveGameSession(int score, int pipesPassed, int coinsCollected, double sessionDuration)
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		using SqliteTransaction transaction = connection.BeginTransaction();

		try
		{
			string insertSessionQuery = @"
				INSERT INTO GameSessions (Score, PipesPassed, CoinsCollected, PlayedDate, SessionDuration)
				VALUES (@score, @pipes, @coins, @date, @duration)";

			using (SqliteCommand command = new SqliteCommand(insertSessionQuery, connection, transaction))
			{
				command.Parameters.AddWithValue("@score", score);
				command.Parameters.AddWithValue("@pipes", pipesPassed);
				command.Parameters.AddWithValue("@coins", coinsCollected);
				command.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
				command.Parameters.AddWithValue("@duration", sessionDuration);
				command.ExecuteNonQuery();
			}

			UpdateStatistics(connection, transaction, score, pipesPassed, coinsCollected);

			transaction.Commit();
		}
		catch (Exception)
		{
			transaction.Rollback();
			throw;
		}
	}

	private void UpdateStatistics(SqliteConnection connection, SqliteTransaction transaction, int score, int pipesPassed, int coinsCollected)
	{
		GameStatistics stats = GetStatistics();

		int newHighScore = Math.Max(stats.HighScore, score);
		int newTotalGames = stats.TotalGamesPlayed + 1;
		int newTotalDeaths = stats.TotalDeaths + 1;
		int newTotalPipes = stats.TotalPipesPassed + pipesPassed;
		int newTotalCoins = stats.TotalCoinsCollected + coinsCollected;

		int totalScore = (stats.AverageScore * stats.TotalGamesPlayed) + score;
		int newAverageScore = totalScore / newTotalGames;

		string updateQuery = @"
			UPDATE GameStatistics SET
				HighScore = @highScore,
				TotalGamesPlayed = @totalGames,
				TotalDeaths = @totalDeaths,
				TotalPipesPassed = @totalPipes,
				TotalCoinsCollected = @totalCoins,
				LastPlayedDate = @date,
				AverageScore = @avgScore
			WHERE Id = 1";

		using SqliteCommand command = new SqliteCommand(updateQuery, connection, transaction);
		command.Parameters.AddWithValue("@highScore", newHighScore);
		command.Parameters.AddWithValue("@totalGames", newTotalGames);
		command.Parameters.AddWithValue("@totalDeaths", newTotalDeaths);
		command.Parameters.AddWithValue("@totalPipes", newTotalPipes);
		command.Parameters.AddWithValue("@totalCoins", newTotalCoins);
		command.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
		command.Parameters.AddWithValue("@avgScore", newAverageScore);
		command.ExecuteNonQuery();
	}

	public GameStatistics GetStatistics()
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		string query = "SELECT Id, HighScore, TotalGamesPlayed, TotalDeaths, TotalPipesPassed, TotalCoinsCollected, LastPlayedDate, AverageScore FROM GameStatistics WHERE Id = 1";

		using SqliteCommand command = new SqliteCommand(query, connection);
		using SqliteDataReader reader = command.ExecuteReader();

		if (reader.Read())
		{
			return new GameStatistics
			{
				Id = reader.GetInt32(reader.GetOrdinal("Id")),
				HighScore = reader.GetInt32(reader.GetOrdinal("HighScore")),
				TotalGamesPlayed = reader.GetInt32(reader.GetOrdinal("TotalGamesPlayed")),
				TotalDeaths = reader.GetInt32(reader.GetOrdinal("TotalDeaths")),
				TotalPipesPassed = reader.GetInt32(reader.GetOrdinal("TotalPipesPassed")),
				TotalCoinsCollected = reader.GetInt32(reader.GetOrdinal("TotalCoinsCollected")),
				LastPlayedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("LastPlayedDate"))),
				AverageScore = reader.GetInt32(reader.GetOrdinal("AverageScore"))
			};
		}

		return new GameStatistics();
	}

	public List<GameSession> GetRecentSessions(int limit = 10)
	{
		List<GameSession> sessions = new List<GameSession>();

		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		string query = "SELECT Id, Score, PipesPassed, CoinsCollected, PlayedDate, SessionDuration FROM GameSessions ORDER BY PlayedDate DESC LIMIT @limit";

		using SqliteCommand command = new SqliteCommand(query, connection);
		command.Parameters.AddWithValue("@limit", limit);

		using SqliteDataReader reader = command.ExecuteReader();

		while (reader.Read())
		{
			sessions.Add(new GameSession
			{
				Id = reader.GetInt32(reader.GetOrdinal("Id")),
				Score = reader.GetInt32(reader.GetOrdinal("Score")),
				PipesPassed = reader.GetInt32(reader.GetOrdinal("PipesPassed")),
				CoinsCollected = reader.GetInt32(reader.GetOrdinal("CoinsCollected")),
				PlayedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("PlayedDate"))),
				SessionDuration = reader.GetDouble(reader.GetOrdinal("SessionDuration"))
			});
		}

		return sessions;
	}

	public void ResetAllStatistics()
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		using SqliteTransaction transaction = connection.BeginTransaction();

		try
		{
			using (SqliteCommand command = new SqliteCommand("DELETE FROM GameSessions", connection, transaction))
			{
				command.ExecuteNonQuery();
			}

			string resetQuery = @"
				UPDATE GameStatistics SET
					HighScore = 0,
					TotalGamesPlayed = 0,
					TotalDeaths = 0,
					TotalPipesPassed = 0,
					TotalCoinsCollected = 0,
					LastPlayedDate = @date,
					AverageScore = 0
				WHERE Id = 1";

			using (SqliteCommand command = new SqliteCommand(resetQuery, connection, transaction))
			{
				command.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
				command.ExecuteNonQuery();
			}

			transaction.Commit();
		}
		catch (Exception)
		{
			transaction.Rollback();
			throw;
		}
	}
}
