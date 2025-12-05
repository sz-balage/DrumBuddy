using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using DrumBuddy.Api;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Services;
using DrumBuddy.IO.Storage;
using ReactiveUI.SourceGenerators;

namespace DrumBuddy.Services;

public class ConfigurationService
{
    private readonly BehaviorSubject<Drum?> _listeningDrum = new(null);
    private readonly MetronomePlayer _metronomePlayer;
    private readonly ConfigurationRepository _configRepository;
    private readonly UserService _userService;
    private AppConfiguration _config;
    private ApiClient _apiClient;

    public ConfigurationService(ConfigurationRepository configRepository,
        MetronomePlayer metronomePlayer,
        UserService userService,
        ApiClient apiClient)
    {
        _configRepository = configRepository;
        _metronomePlayer = metronomePlayer;
        _userService = userService;
        _apiClient = apiClient;
        try
        {
            LoadConfig().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
    public async Task SaveAsync()
    {
        var now = DateTime.UtcNow;
        await _configRepository.UpdateConfigAsync(_config, _userService.UserId, now);
        if(CanSyncToServer)
        {
            try
            {
                await _apiClient.UpdateConfigurationAsync(_config, now);
            }
            catch (Exception e)
            {
                //ignore
            }
        }
    }
    public IReadOnlyDictionary<Drum, DrumPositionSlot> DrumPositions => _config.DrumPositions;

    public int MetronomeVolume
    {
        get => _config.MetronomeVolume;
        set
        {
            _config.MetronomeVolume = value;
            _metronomePlayer?.SetVolume(value);
        }
    }

    public bool KeyboardInput
    {
        get => _config.KeyboardInput;
        set
        {
            _config.KeyboardInput = value;
        }
    }

    public IReadOnlyDictionary<Drum, int> Mapping =>
        _config.KeyboardInput ? _config.KeyboardMapping : _config.DrumMapping;

    public Drum? ListeningDrum
    {
        get => _listeningDrum.Value;
        private set => _listeningDrum.OnNext(value);
    }

    public IObservable<Drum?> ListeningDrumChanged => _listeningDrum.AsObservable();
    
    private static DrumPositionSlot DefaultPosition(Drum drum)
    {
        return drum switch
        {
            Drum.Kick => DrumPositionSlot.BetweenLine1And2,
            Drum.Snare => DrumPositionSlot.BetweenLine3And4,
            Drum.FloorTom => DrumPositionSlot.BetweenLine2And3,
            Drum.Tom1 => DrumPositionSlot.BetweenLine4And5,
            Drum.Tom2 => DrumPositionSlot.OnLine4,
            Drum.Ride => DrumPositionSlot.OnLine5,
            Drum.HiHat => DrumPositionSlot.BetweenLine5And6,
            Drum.HiHat_Open => DrumPositionSlot.BetweenLine5And6,
            Drum.HiHat_Pedal => DrumPositionSlot.BelowLine1,
            Drum.Crash1 => DrumPositionSlot.OnLine6,
            Drum.Crash2 => DrumPositionSlot.OnLine6,
            _ => DrumPositionSlot.OnLine3
        };
    }

    public IReadOnlyDictionary<Drum, int> GetDrumMapping() => _config.DrumMapping;
    public IReadOnlyDictionary<Drum, int> GetKeyboardMapping() => _config.KeyboardMapping;
    

    public void StartListening(Drum drum) => ListeningDrum = drum;
    public void StopListening() => ListeningDrum = null;

    public async Task MapDrumAsync(int receivedNote)
    {
        if (ListeningDrum is null || receivedNote < 0)
            return;

        var targetMapping = _config.KeyboardInput ? _config.KeyboardMapping : _config.DrumMapping;

        var alreadyMappedDrum = targetMapping.FirstOrDefault(kvp => kvp.Value == receivedNote).Key;
        if (alreadyMappedDrum != default)
            targetMapping[alreadyMappedDrum] = -1;

        targetMapping[ListeningDrum.Value] = receivedNote;
        await SaveAsync();
        StopListening();
    }

    public async Task SetAsync<T>(string key, T value)
    {
        _config.UserSettings[key] = value?.ToString() ?? string.Empty;
        await SaveAsync();
    }

    public T? Get<T>(string key)
    {
        if (!_config.UserSettings.TryGetValue(key, out var str))
            return default;

        try
        {
            if (typeof(T) == typeof(string))
                return (T)(object)str;
            return (T)Convert.ChangeType(str, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public async Task SetDefaultDrumMappings()
    {
        foreach (var drum in Enum.GetValues<Drum>())
            if (drum != Drum.Rest)
                _config.DrumMapping[drum] = (int)drum;
        await SaveAsync();
    }

    public async Task SetDefaultKeyboardMappings()
    {
        foreach (var drum in Enum.GetValues<Drum>())
            if (drum != Drum.Rest)
                _config.KeyboardMapping[drum] = (int)drum;
        await SaveAsync();
    }

    public async Task LoadConfig()
    {
        CanSyncToServer = _userService.IsOnline;
        var local = await _configRepository.LoadConfigAsync(_userService.UserId);
        if (CanSyncToServer)
        {
            try
            {
                var serverConfig = await _apiClient.GetConfigurationAsync();
                if(serverConfig.UpdatedAt > local.UpdatedAt)
                {
                    _config = serverConfig.Configuration;
                    await _configRepository.UpdateConfigAsync(_config, _userService.UserId, serverConfig.UpdatedAt);
                }
                else if(serverConfig.UpdatedAt < local.UpdatedAt)
                {
                    _config = local.Config;
                    await _apiClient.UpdateConfigurationAsync(local.Config, local.UpdatedAt);
                }
                else
                {
                    _config = local.Config;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        else
        { 
            _config = local.Config;
        }
        if (_config.UserSettings is null)
            _config.UserSettings = new Dictionary<string, string>();
        if (_config.DrumMapping.Count == 0)
        {
            await SetDefaultDrumMappings();
        }

        if (_config.KeyboardMapping.Count == 0)
        {
            await SetDefaultKeyboardMappings();
        }

        foreach (var drum in Enum.GetValues<Drum>()) //always override positions with default for now
            if (drum != Drum.Rest)
                _config.DrumPositions[drum] = DefaultPosition(drum);
    }

    public bool CanSyncToServer { get; private set; }
}