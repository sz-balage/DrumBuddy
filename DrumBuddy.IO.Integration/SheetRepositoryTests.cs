using System.Collections.Immutable;
using DrumBuddy.Core.Models;
using DrumBuddy.Core.Services;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using Shouldly;

namespace DrumBuddy.IO.Integration;

public class SheetRepositoryTests : DbTestBase
{
    [Fact]
    public async Task SaveAndLoadSheet_ShouldWork()
    {
        var repo = new SheetRepository(Db, SerializationService);

        var id = Guid.NewGuid();
        var measures = new[]
        {
            new Measure(new List<RythmicGroup>()) // empty measure OK
        }.ToImmutableArray();

        var bytes = SerializationService.SerializeMeasurementData(measures);

        var dto = new SheetDto
        {
            Id = id,
            Name = "TestSheet",
            Description = "Desc",
            Tempo = 120,
            MeasureBytes = bytes
        };

        await repo.SaveSheetAsync(dto, UserId);

        var sheets = await repo.LoadSheetsAsync(UserId);

        sheets.Length.ShouldBe(1);
        sheets[0].Id.ShouldBe(id);
    }

    [Fact]
    public async Task SaveSheet_DuplicateName_ShouldThrowCustomError()
    {
        var repo = new SheetRepository(Db, new SerializationService());
        var measures = new[]
        {
            new Measure(new List<RythmicGroup>()) // empty measure OK
        }.ToImmutableArray();

        var bytes = SerializationService.SerializeMeasurementData(measures);
        var dto = new SheetDto
        {
            Id = Guid.NewGuid(),
            Name = "Beat",
            Description = "",
            Tempo = 120,
            MeasureBytes = bytes
        };

        await repo.SaveSheetAsync(dto, UserId);
        dto.Id = Guid.NewGuid();
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await repo.SaveSheetAsync(dto, UserId)
        );
    }

    [Fact]
    public async Task UpdateSheet_ShouldModifyExistingRecord()
    {
        var repo = new SheetRepository(Db, new SerializationService());

        var id = Guid.NewGuid();
        var measures = new[]
        {
            new Measure(new List<RythmicGroup>()) // empty measure OK
        }.ToImmutableArray();

        var bytes = SerializationService.SerializeMeasurementData(measures);
        var dto = new SheetDto
        {
            Id = id,
            Name = "Old",
            Description = "D",
            Tempo = 100,
            MeasureBytes = bytes
        };

        await repo.SaveSheetAsync(dto, UserId);

        dto.Name = "NewName";

        await repo.UpdateSheetAsync(dto, DateTime.UtcNow, UserId);

        var loaded = await repo.GetSheetByIdAsync(id, UserId);

        loaded!.Name.ShouldBe("NewName");
    }
}