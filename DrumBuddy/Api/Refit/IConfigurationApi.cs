using System.Threading.Tasks;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface IConfigurationApi
{
    [Get("/api/config")]
    Task<AppConfiguration> GetConfigurationAsync();

    [Put("/api/config")]
    Task UpdateConfigurationAsync([Body] UpdateConfigurationRequest request);
}