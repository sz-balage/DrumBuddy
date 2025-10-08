namespace DrumBuddy.IO.Data;

public static class SheetDbCommands
{
    private const string InsertSheetSql = """
                                          INSERT INTO Sheets(name, description, tempo, measures_data)
                                          VALUES(@Name, @Description, @Tempo, @MeasuresData)
                                          """;

    private const string UpdateSheetSql = """
                                          UPDATE Sheets
                                          SET name = @NewName, 
                                              description = @Description,
                                              tempo = @Tempo,
                                              measures_data = @MeasuresData
                                          WHERE name = @OldName
                                          """;

    private const string DeleteSheetSql = """
                                          DELETE FROM Sheets
                                          WHERE name = @Name
                                          """;

    public static async Task<int> InsertSheetAsync(string connectionString, string name, int tempo, byte[] measuresData,
        string? description = null)
    {
        return await SqlQueryString.FromRawString(InsertSheetSql)
            .ExecuteAsync(connectionString,
                new { Name = name, MeasuresData = measuresData, Tempo = tempo, Description = description });
    }

    public static async Task<int> UpdateSheetAsync(string connectionString, string name, int tempo, byte[] measuresData,
        string? newName = null, string? description = null)
    {
        return await SqlQueryString.FromRawString(UpdateSheetSql)
            .ExecuteAsync(connectionString,
                new
                {
                    OldName = name, MeasuresData = measuresData, Tempo = tempo, NewName = newName,
                    Description = description
                });
    }

    public static async Task DeleteSheetAsync(string connectionString, string name)
    {
        await SqlQueryString.FromRawString(DeleteSheetSql)
            .ExecuteAsync(connectionString, new { Name = name });
    }
}