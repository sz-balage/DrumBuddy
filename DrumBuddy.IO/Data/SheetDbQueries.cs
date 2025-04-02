using System.Data;
using System.Data.SQLite;
using Dapper;
using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Data;

public static class SheetDbQueries
{
    const string SelectAllSheetsSql = """
        SELECT id AS Id,
               name AS Name,
               description AS Description,
               tempo AS Tempo,
               measures_data AS MeasuresData
        FROM Sheets
        """;

    public static async Task<IEnumerable<SheetDbRecord>> SelectAllSheetsAsync(string connectionString)
        => await SqlQueryString.FromRawString(SelectAllSheetsSql)
            .QueryAsync<SheetDbRecord>(connectionString);
   
    // public static async Task<IEnumerable<SheetDbRecord>> SelectAllSheetsAsync(string connectionString)
    // {
    //     var records = new List<SheetDbRecord>();
    //
    //     using var connection = new SQLiteConnection(connectionString);
    //     await connection.OpenAsync();
    //
    //     using var command = new SQLiteCommand(SelectAllSheetsSql, connection);
    //     using var reader = await command.ExecuteReaderAsync();
    //
    //     while (await reader.ReadAsync())
    //     {
    //         var id = reader.GetInt32(0);
    //         var name = reader.GetString(1);
    //         var description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
    //         var tempo = reader.GetInt32(3);
    //         var measuresData = reader.IsDBNull(4) ? Array.Empty<byte>() : reader["MeasuresData"] as byte[];
    //
    //         records.Add(new SheetDbRecord(id, name, description, tempo, measuresData ?? Array.Empty<byte>()));
    //     }
    //
    //     return records;
    // }
    public sealed record SheetDbRecord(
        int Id,
        string Name,
        string Description,
        int Tempo,
        string MeasuresData
    );
}