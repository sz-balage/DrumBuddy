using System.Data;
using System.Data.SQLite;
using Dapper;
using DrumBuddy.Core.Models;

namespace DrumBuddy.IO.Data;

public static class SheetDbQueries
{
    private const string SelectSheetSql = """
        SELECT id AS Id,
               name AS Name,
               description AS Description,
               tempo AS Tempo,
               measures_data AS MeasuresData
        FROM Sheets
        WHERE name = @Name
        """;
    
    const string SelectAllSheetsSql = """
        SELECT id AS Id,
               name AS Name,
               description AS Description,
               tempo AS Tempo,
               measures_data AS MeasuresData
        FROM Sheets
        """;

    const string SheetExistsSql = """
          SELECT COUNT(1)
          FROM Sheets
          WHERE name = @Name
          """;
    public static async Task<IEnumerable<SheetDbRecord>> SelectAllSheetsAsync(string connectionString)
        => await SqlQueryString.FromRawString(SelectAllSheetsSql)
            .QueryAsync<SheetDbRecord>(connectionString);
    
    public static async Task<SheetDbRecord?> SelectSheetAsync(string connectionString, string name)
    {
        var result = await SqlQueryString.FromRawString(SelectSheetSql)
            .QuerySingleOrDefaultAsync<SheetDbRecord>(connectionString, new { Name = name });
        return result;
    }
    public static bool SheetExists(string connectionString, string sheetName)
    {
        return SqlQueryString.FromRawString(SheetExistsSql).QuerySingle<int>(connectionString, new { Name = sheetName }) > 0;
    }
    public sealed record SheetDbRecord(
        long Id,
        string Name,
        string Description,
        long Tempo,
        byte[] MeasuresData
    );


    
}