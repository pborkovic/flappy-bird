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
				LastPlayedDate TEXT NOT NULL,
				AverageScore INTEGER NOT NULL DEFAULT 0
			)";

		string createSessionsTable = @"
			CREATE TABLE IF NOT EXISTS GameSessions (
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				Score INTEGER NOT NULL,
				PipesPassed INTEGER NOT NULL,
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

		EnsureStatisticsRecordExists(connection);
	}

	private void EnsureStatisticsRecordExists(SqliteConnection connection)
	{
		string checkQuery = "SELECT COUNT(*) FROM GameStatistics";
		using SqliteCommand checkCommand = new SqliteCommand(checkQuery, connection);
		int count = Convert.ToInt32(checkCommand.ExecuteScalar());

		if (count == 0)
		{
			string insertQuery = @"
				INSERT INTO GameStatistics (HighScore, TotalGamesPlayed, TotalDeaths, TotalPipesPassed, LastPlayedDate, AverageScore)
				VALUES (0, 0, 0, 0, @date, 0)";

			using SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection);
			insertCommand.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
			insertCommand.ExecuteNonQuery();
		}
	}

	public void SaveGameSession(int score, int pipesPassed, double sessionDuration)
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		using SqliteTransaction transaction = connection.BeginTransaction();

		try
		{
			string insertSessionQuery = @"
				INSERT INTO GameSessions (Score, PipesPassed, PlayedDate, SessionDuration)
				VALUES (@score, @pipes, @date, @duration)";

			using (SqliteCommand command = new SqliteCommand(insertSessionQuery, connection, transaction))
			{
				command.Parameters.AddWithValue("@score", score);
				command.Parameters.AddWithValue("@pipes", pipesPassed);
				command.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
				command.Parameters.AddWithValue("@duration", sessionDuration);
				command.ExecuteNonQuery();
			}

			UpdateStatistics(connection, transaction, score, pipesPassed);

			transaction.Commit();
		}
		catch (Exception)
		{
			transaction.Rollback();
			throw;
		}
	}

	private void UpdateStatistics(SqliteConnection connection, SqliteTransaction transaction, int score, int pipesPassed)
	{
		GameStatistics stats = GetStatistics();

		int newHighScore = Math.Max(stats.HighScore, score);
		int newTotalGames = stats.TotalGamesPlayed + 1;
		int newTotalDeaths = stats.TotalDeaths + 1;
		int newTotalPipes = stats.TotalPipesPassed + pipesPassed;

		int totalScore = (stats.AverageScore * stats.TotalGamesPlayed) + score;
		int newAverageScore = totalScore / newTotalGames;

		string updateQuery = @"
			UPDATE GameStatistics SET
				HighScore = @highScore,
				TotalGamesPlayed = @totalGames,
				TotalDeaths = @totalDeaths,
				TotalPipesPassed = @totalPipes,
				LastPlayedDate = @date,
				AverageScore = @avgScore
			WHERE Id = 1";

		using SqliteCommand command = new SqliteCommand(updateQuery, connection, transaction);
		command.Parameters.AddWithValue("@highScore", newHighScore);
		command.Parameters.AddWithValue("@totalGames", newTotalGames);
		command.Parameters.AddWithValue("@totalDeaths", newTotalDeaths);
		command.Parameters.AddWithValue("@totalPipes", newTotalPipes);
		command.Parameters.AddWithValue("@date", DateTime.Now.ToString("o"));
		command.Parameters.AddWithValue("@avgScore", newAverageScore);
		command.ExecuteNonQuery();
	}

	public GameStatistics GetStatistics()
	{
		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		string query = "SELECT * FROM GameStatistics WHERE Id = 1";

		using SqliteCommand command = new SqliteCommand(query, connection);
		using SqliteDataReader reader = command.ExecuteReader();

		if (reader.Read())
		{
			return new GameStatistics
			{
				Id = reader.GetInt32(0),
				HighScore = reader.GetInt32(1),
				TotalGamesPlayed = reader.GetInt32(2),
				TotalDeaths = reader.GetInt32(3),
				TotalPipesPassed = reader.GetInt32(4),
				LastPlayedDate = DateTime.Parse(reader.GetString(5)),
				AverageScore = reader.GetInt32(6)
			};
		}

		return new GameStatistics();
	}

	public List<GameSession> GetRecentSessions(int limit = 10)
	{
		List<GameSession> sessions = new List<GameSession>();

		using SqliteConnection connection = new SqliteConnection(_connectionString);
		connection.Open();

		string query = "SELECT * FROM GameSessions ORDER BY PlayedDate DESC LIMIT @limit";

		using SqliteCommand command = new SqliteCommand(query, connection);
		command.Parameters.AddWithValue("@limit", limit);

		using SqliteDataReader reader = command.ExecuteReader();

		while (reader.Read())
		{
			sessions.Add(new GameSession
			{
				Id = reader.GetInt32(0),
				Score = reader.GetInt32(1),
				PipesPassed = reader.GetInt32(2),
				PlayedDate = DateTime.Parse(reader.GetString(3)),
				SessionDuration = reader.GetDouble(4)
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
