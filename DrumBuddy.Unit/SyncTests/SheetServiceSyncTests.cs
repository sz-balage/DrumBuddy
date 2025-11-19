using System.Collections.Immutable;
using DrumBuddy.Api;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Data;
using DrumBuddy.IO.Models;
using DrumBuddy.Services;
using Moq;
using Shouldly;

namespace DrumBuddy.Unit.SyncTests;

public class SheetServiceSyncTests
{
    private readonly Mock<IApiClient> _mockApiClient;
    private readonly Mock<ISheetRepository> _mockRepository;
    private readonly Mock<IUserService> _mockUserService;
    private readonly SheetService _sheetService;

    public SheetServiceSyncTests()
    {
        _mockRepository = new Mock<ISheetRepository>();
        _mockUserService = new Mock<IUserService>();
        _mockApiClient = new Mock<IApiClient>();

        _sheetService = new SheetService(
            _mockRepository.Object,
            _mockUserService.Object,
            _mockApiClient.Object
        );
    }

    private Sheet CreateSheet(string name, Drum drum, Guid? id = null, DateTime? updatedAt = null)
    {
        return new Sheet(
            new Bpm(120),
            [
                new Measure([
                    new RythmicGroup([
                        new NoteGroup([
                            new Note(drum, NoteValue.Quarter)
                        ])
                    ])
                ])
            ],
            name,
            "",
            id ?? Guid.NewGuid(),
            updatedAt ?? DateTime.UtcNow
        );
    }

    private SheetSummaryDto CreateTestSummary(string name, Guid id, DateTime updatedAt)
    {
        return new SheetSummaryDto
        {
            Id = id,
            Name = name,
            UpdatedAt = updatedAt
        };
    }


    [Fact]
    public async Task LoadSheetsAsync_WhenOffline_LoadsFromLocalOnly()
    {
        // Arrange
        var userId = "test-user";
        var localSheets = ImmutableArray.Create(
            CreateSheet("Sheet 1", Drum.Kick),
            CreateSheet("Sheet 2", Drum.Kick)
        );

        _mockUserService.Setup(x => x.IsOnline).Returns(false);
        _mockUserService.Setup(x => x.UserId).Returns(userId);

        _mockRepository.Setup(x => x.LoadSheetsAsync(userId))
            .ReturnsAsync(localSheets);

        // Act
        var result = await _sheetService.LoadSheetsAsync();

        // Assert
        result.Length.ShouldBe(2);
        _mockRepository.Verify(x => x.LoadSheetsAsync(userId), Times.Once);
        _mockApiClient.Verify(x => x.GetSheetSummariesAsync(), Times.Never);
    }


    [Fact]
    public async Task LoadSheetsAsync_WhenOnline_TriggersSync()
    {
        var userId = "test-user";
        var sheetId = Guid.NewGuid();

        var localSheet = CreateSheet("Sheet 1", Drum.Snare, sheetId);
        var remoteSummary = CreateTestSummary("Sheet 1", sheetId, DateTime.UtcNow);
        var remoteSheet = CreateSheet("Sheet 1", Drum.Kick, sheetId);

        _mockUserService.Setup(x => x.IsOnline).Returns(true);
        _mockUserService.Setup(x => x.UserId).Returns(userId);

        _mockRepository.Setup(x => x.LoadSheetsAsync(userId))
            .ReturnsAsync([localSheet]);

        _mockApiClient.Setup(x => x.GetSheetSummariesAsync())
            .ReturnsAsync([remoteSummary]);

        _mockApiClient.Setup(x => x.GetSheetAsync(sheetId))
            .ReturnsAsync(remoteSheet);

        // Act
        _ = await _sheetService.LoadSheetsAsync();

        // Assert
        _mockApiClient.Verify(x => x.GetSheetSummariesAsync(), Times.Once);
    }


    [Fact]
    public async Task LoadSheetsAsync_WhenRemoteIsNewer_ReturnsUpdatedRemoteVersion()
    {
        var userId = "user";
        var sheetId = Guid.NewGuid();

        _mockUserService.Setup(x => x.IsOnline).Returns(true);
        _mockUserService.Setup(x => x.UserId).Returns(userId);

        var local = CreateSheet("MySheet", Drum.Snare, sheetId,
            new DateTime(2024, 1, 1));

        var remoteSummary = CreateTestSummary("MySheet", sheetId,
            new DateTime(2024, 2, 1));

        var remoteFull = CreateSheet("MySheet", Drum.Kick, sheetId,
            remoteSummary.UpdatedAt);

        _mockRepository.Setup(r => r.LoadSheetsAsync(userId))
            .ReturnsAsync([local]);

        _mockApiClient.Setup(a => a.GetSheetSummariesAsync())
            .ReturnsAsync([remoteSummary]);

        _mockApiClient.Setup(a => a.GetSheetAsync(sheetId))
            .ReturnsAsync(remoteFull);

        var result = await _sheetService.LoadSheetsAsync();

        var loaded = result.FirstOrDefault(s => s.Id == sheetId);
        loaded!.Measures[0].Groups[0].NoteGroups[0][0].Drum.ShouldBe(Drum.Kick);
    }


    [Fact]
    public async Task LoadSheetsAsync_WhenLocalIsNewer_PushesUpdateAndReturnsLocal()
    {
        var userId = "user";
        var sheetId = Guid.NewGuid();

        _mockUserService.Setup(x => x.IsOnline).Returns(true);
        _mockUserService.Setup(x => x.UserId).Returns(userId);

        var local = CreateSheet("Sheet", Drum.Snare, sheetId,
            new DateTime(2024, 2, 1));

        var remoteSummary = CreateTestSummary("Sheet", sheetId,
            new DateTime(2024, 1, 1));

        _mockRepository.Setup(r => r.LoadSheetsAsync(userId))
            .ReturnsAsync([local]);

        _mockApiClient.Setup(a => a.GetSheetSummariesAsync())
            .ReturnsAsync([remoteSummary]);

        var result = await _sheetService.LoadSheetsAsync();

        _mockApiClient.Verify(a =>
                a.UpdateSheetAsync(sheetId, local, local.UpdatedAt),
            Times.Once);

        result.ShouldContain(s => s.Id == sheetId &&
                                  s.Measures[0].Groups[0].NoteGroups[0][0].Drum == Drum.Snare);
    }


    [Fact]
    public async Task Sync_ShouldDownloadRemoteSheet_WhenNotPresentLocally()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var id = Guid.NewGuid();
        var remoteSummary = CreateTestSummary("RemoteOnly", id, DateTime.UtcNow);
        var remoteSheet = CreateSheet("RemoteOnly", Drum.Kick, id, remoteSummary.UpdatedAt);

        _mockRepository.Setup(r => r.LoadSheetsAsync("user")).ReturnsAsync([]);
        _mockApiClient.Setup(a => a.GetSheetSummariesAsync()).ReturnsAsync([remoteSummary]);
        _mockApiClient.Setup(a => a.GetSheetAsync(id)).ReturnsAsync(remoteSheet);

        var result = await _sheetService.LoadSheetsAsync();

        result.Length.ShouldBe(1);
        result[0].Measures[0].Groups[0].NoteGroups[0][0].Drum.ShouldBe(Drum.Kick);

        _mockRepository.Verify(r => r.SaveSheetAsync(remoteSheet, "user"), Times.Once);
    }


    [Fact]
    public async Task Sync_NameConflict_LocalNewer_ShouldDeleteRemote()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var localId = Guid.NewGuid();
        var remoteId = Guid.NewGuid();

        var local = CreateSheet("Beat", Drum.Snare, localId, new DateTime(2024, 2, 1));
        var remoteSummary = CreateTestSummary("Beat", remoteId, new DateTime(2024, 1, 1));

        _mockRepository.Setup(r => r.LoadSheetsAsync("user")).ReturnsAsync([local]);
        _mockApiClient.Setup(a => a.GetSheetSummariesAsync()).ReturnsAsync([remoteSummary]);

        var result = await _sheetService.LoadSheetsAsync();

        _mockApiClient.Verify(a => a.DeleteSheetAsync(remoteId), Times.Once);

        result.ShouldContain(s => s.Id == localId);
    }


    [Fact]
    public async Task Sync_NameConflict_RemoteNewer_ShouldReplaceLocal()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var localId = Guid.NewGuid();
        var remoteId = Guid.NewGuid();

        var local = CreateSheet("Beat", Drum.Snare, localId, new DateTime(2024, 1, 1));
        var remoteSummary = CreateTestSummary("Beat", remoteId, new DateTime(2024, 2, 1));
        var remoteFull = CreateSheet("Beat", Drum.Kick, remoteId, remoteSummary.UpdatedAt);

        _mockRepository.Setup(r => r.LoadSheetsAsync("user")).ReturnsAsync([local]);
        _mockApiClient.Setup(a => a.GetSheetSummariesAsync()).ReturnsAsync([remoteSummary]);
        _mockApiClient.Setup(a => a.GetSheetAsync(remoteId)).ReturnsAsync(remoteFull);

        var result = await _sheetService.LoadSheetsAsync();

        _mockRepository.Verify(r => r.DeleteSheetAsync(localId, "user"), Times.Once);
        _mockRepository.Verify(r => r.SaveSheetAsync(remoteFull, "user"), Times.Once);

        var replaced = result.FirstOrDefault(s => s.Id == remoteId);
        replaced!.Measures[0].Groups[0].NoteGroups[0][0].Drum.ShouldBe(Drum.Kick);
    }


    [Fact]
    public async Task Sync_LocalUnsynced_ShouldNotOverwriteOrUpload()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var id = Guid.NewGuid();

        var local = CreateSheet("Draft", Drum.Snare, id, DateTime.UtcNow);
        local.IsSyncEnabled = false;

        _mockRepository.Setup(r => r.LoadSheetsAsync("user")).ReturnsAsync([local]);
        _mockApiClient.Setup(a => a.GetSheetSummariesAsync()).ReturnsAsync([]);

        var result = await _sheetService.LoadSheetsAsync();

        _mockApiClient.Verify(a =>
                a.UpdateSheetAsync(It.IsAny<Guid>(), It.IsAny<Sheet>(), It.IsAny<DateTime>()),
            Times.Never);

        result.ShouldContain(s => s.Id == id);
    }


    [Fact]
    public async Task Sync_MultipleSheets_MixedNewerCases_ShouldSyncCorrectly()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var sheetA = CreateSheet("A", Drum.Snare, Guid.NewGuid(), new DateTime(2024, 1, 1));
        var sheetB = CreateSheet("B", Drum.Snare, Guid.NewGuid(), new DateTime(2024, 2, 1));

        var remoteA = CreateTestSummary("A", sheetA.Id, new DateTime(2024, 2, 1)); // remote newer
        var remoteB = CreateTestSummary("B", sheetB.Id, new DateTime(2024, 1, 1)); // local newer

        var remoteAFull = CreateSheet("A", Drum.Kick, sheetA.Id, remoteA.UpdatedAt);

        _mockRepository.Setup(r => r.LoadSheetsAsync("user"))
            .ReturnsAsync([sheetA, sheetB]);

        _mockApiClient.Setup(a => a.GetSheetSummariesAsync())
            .ReturnsAsync([remoteA, remoteB]);

        _mockApiClient.Setup(a => a.GetSheetAsync(sheetA.Id))
            .ReturnsAsync(remoteAFull);

        var result = await _sheetService.LoadSheetsAsync();

        result.ShouldContain(s =>
            s.Id == sheetA.Id &&
            s.Measures[0].Groups[0].NoteGroups[0][0].Drum == Drum.Kick);

        _mockApiClient.Verify(a =>
                a.UpdateSheetAsync(sheetB.Id, sheetB, sheetB.UpdatedAt),
            Times.Once);
    }


    [Fact]
    public async Task Sync_RemoteDeleted_ShouldKeepLocal()
    {
        _mockUserService.Setup(u => u.IsOnline).Returns(true);
        _mockUserService.Setup(u => u.UserId).Returns("user");

        var local = CreateSheet("LocalOnly", Drum.Snare, Guid.NewGuid(), DateTime.UtcNow);

        _mockRepository.Setup(r => r.LoadSheetsAsync("user"))
            .ReturnsAsync([local]);

        _mockApiClient.Setup(a => a.GetSheetSummariesAsync())
            .ReturnsAsync([]);

        var result = await _sheetService.LoadSheetsAsync();

        result.Length.ShouldBe(1);
        result[0].Id.ShouldBe(local.Id);

        _mockApiClient.Verify(a => a.DeleteSheetAsync(It.IsAny<Guid>()), Times.Never);
        _mockApiClient.Verify(a => a.CreateSheetAsync(It.IsAny<Sheet>(), It.IsAny<DateTime>()), Times.Never);
    }
}