using System.Threading.Tasks;
using DrumBuddy.Api.Models;
using DrumBuddy.Core.Models;
using DrumBuddy.IO.Models;
using Refit;

namespace DrumBuddy.Api.Refit;

public interface IConfigurationApi
{
    [Get("/api/config")]
    Task<ConfigurationResponse> GetConfigurationAsync();

    [Put("/api/config")]
    Task UpdateConfigurationAsync([Body] UpdateConfigurationRequest request);
}