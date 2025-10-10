using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace DrumBuddy.IO.Data.Commands;

public static class DbExtensions
{
    public static async Task<int> ExecuteAsync(this SqlQueryString sql, string connectionString,
        object? sqlParameter = null)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var numberOfRowUpdated = await connection.ExecuteAsync(sql, sqlParameter);
        return numberOfRowUpdated;
    }

    public static async Task<int> ExecuteAsync(this SqlQueryString sql, SqliteConnection connection,
        object? sqlParameter = null, SqliteTransaction? transaction = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        var numberOfRowUpdated = await connection.ExecuteAsync(sql, sqlParameter, transaction);
        return numberOfRowUpdated;
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(this SqlQueryString sql, string connectionString,
        object? sqlParameter = null)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var records = await connection.QueryAsync<T>(sql, sqlParameter);
        return records;
    }

    public static T QuerySingle<T>(this SqlQueryString sql, string connectionString, object? sqlParameter = null)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();
        var record = connection.QuerySingle<T>(sql, sqlParameter);
        return record;
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(this SqlQueryString sql, SqliteConnection connection,
        object? sqlParameter = null, SqliteTransaction? transaction = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        var records = await connection.QueryAsync<T>(sql, sqlParameter, transaction);
        return records;
    }

    public static async Task<T> QuerySingleAsync<T>(this SqlQueryString sql, string connectionString,
        object? sqlParameter = null)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var record = await connection.QuerySingleAsync<T>(sql, sqlParameter);
        return record;
    }


    public static async Task<T> QuerySingleAsync<T>(this SqlQueryString sql, SqliteConnection connection,
        object? sqlParameter = null, SqliteTransaction? transaction = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        var record = await connection.QuerySingleAsync<T>(sql, sqlParameter, transaction);
        return record;
    }

    public static async Task<T?> QuerySingleOrDefaultAsync<T>(this SqlQueryString sql, string connectionString,
        object? sqlParameter = null)
    {
        using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync();
        var record = await connection.QuerySingleOrDefaultAsync<T>(sql, sqlParameter);
        return record;
    }

    public static async Task<T?> QuerySingleOrDefaultAsync<T>(this SqlQueryString sql, SqliteConnection connection,
        object? sqlParameter = null, SqliteTransaction? transaction = null)
    {
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();
        var record = await connection.QuerySingleOrDefaultAsync<T>(sql, sqlParameter, transaction);
        return record;
    }
}

public readonly record struct SqlQueryString
{
    public SqlQueryString()
    {
        throw new NotSupportedException("It is not allowed to create an empty SQL string.");
    }

    public SqlQueryString(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static SqlQueryString FromRawString(string rawSql)
    {
        return LooksLikeSql(rawSql)
            ? new SqlQueryString(rawSql)
            : throw new ArgumentException("The provided string does not look like a SQL query.", nameof(rawSql));
    }

    private static bool LooksLikeSql(string rawSql)
    {
        return rawSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) ||
               rawSql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase) ||
               rawSql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) ||
               rawSql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase);
    }

    public static implicit operator string(SqlQueryString sql)
    {
        return sql.Value;
    }
}