using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CineScope.ViewModels;
using Microsoft.Extensions.Options;

namespace CineScope.Services;

public class TmdbService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly TmdbSettings _settings;

    public TmdbService(HttpClient httpClient, IOptions<TmdbSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_settings.ApiKey);

    private bool UseBearerToken => IsConfigured &&
        (_settings.ApiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
         _settings.ApiKey.StartsWith("eyJ", StringComparison.OrdinalIgnoreCase));

    public async Task<List<ExternalMovieResultViewModel>> SearchMoviesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<ExternalMovieResultViewModel>();
        }

        EnsureConfigured();

        var genres = await GetMovieGenresAsync(cancellationToken);
        var response = await GetFromTmdbAsync<TmdbSearchResponse>(
            "search/movie",
            new Dictionary<string, string?>
            {
                ["query"] = searchTerm.Trim(),
                ["include_adult"] = "false",
                ["page"] = "1"
            },
            cancellationToken);

        return response?.Results?
            .Select(movie => MapSearchResult(movie, genres))
            .Where(movie => !string.IsNullOrWhiteSpace(movie.Title))
            .ToList() ?? new List<ExternalMovieResultViewModel>();
    }

    public async Task<List<ExternalMovieResultViewModel>> GetTrendingMoviesAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var genres = await GetMovieGenresAsync(cancellationToken);
        var response = await GetFromTmdbAsync<TmdbSearchResponse>(
            "trending/movie/week",
            new Dictionary<string, string?>
            {
                ["page"] = "1"
            },
            cancellationToken);

        return response?.Results?
            .Select(movie => MapSearchResult(movie, genres))
            .Where(movie => !string.IsNullOrWhiteSpace(movie.Title))
            .ToList() ?? new List<ExternalMovieResultViewModel>();
    }

    public async Task<ExternalMovieResultViewModel?> GetMovieDetailsAsync(int tmdbMovieId, CancellationToken cancellationToken = default)
    {
        EnsureConfigured();

        var details = await GetFromTmdbAsync<TmdbMovieDetailsResponse>(
            $"movie/{tmdbMovieId}",
            new Dictionary<string, string?>(),
            cancellationToken);

        if (details == null)
        {
            return null;
        }

        return new ExternalMovieResultViewModel
        {
            ExternalId = details.Id,
            Title = FirstNonEmpty(details.Title, details.OriginalTitle, "Untitled"),
            Genre = MapGenres(details.Genres),
            ReleaseYear = ParseReleaseYear(details.ReleaseDate),
            Rating = ConvertRating(details.VoteAverage),
            Duration = details.Runtime > 0 ? details.Runtime : null,
            PosterUrl = BuildPosterUrl(details.PosterPath),
            Description = FirstNonEmpty(details.Overview, "No description available.")
        };
    }

    private async Task<Dictionary<int, string>> GetMovieGenresAsync(CancellationToken cancellationToken)
    {
        var response = await GetFromTmdbAsync<TmdbGenreResponse>(
            "genre/movie/list",
            new Dictionary<string, string?>(),
            cancellationToken);

        return response?.Genres?
            .Where(genre => genre.Id > 0 && !string.IsNullOrWhiteSpace(genre.Name))
            .GroupBy(genre => genre.Id)
            .ToDictionary(group => group.Key, group => group.First().Name) ?? new Dictionary<int, string>();
    }

    private async Task<T?> GetFromTmdbAsync<T>(
        string path,
        Dictionary<string, string?> query,
        CancellationToken cancellationToken)
    {
        var requestUri = BuildRequestUri(path, query);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (UseBearerToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetBearerToken());
        }

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("TMDB rejected the configured API key. Please check appsettings.json.");
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"TMDB request failed with status code {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
    }

    private Uri BuildRequestUri(string path, Dictionary<string, string?> query)
    {
        var baseUrl = FirstNonEmpty(_settings.BaseUrl, "https://api.themoviedb.org/3").TrimEnd('/');
        var cleanPath = path.TrimStart('/');
        var queryValues = new Dictionary<string, string?>
        {
            ["language"] = FirstNonEmpty(_settings.Language, "en-US")
        };

        if (!UseBearerToken)
        {
            queryValues["api_key"] = _settings.ApiKey;
        }

        foreach (var item in query)
        {
            queryValues[item.Key] = item.Value;
        }

        var queryString = string.Join("&", queryValues
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value!)}"));

        return new Uri($"{baseUrl}/{cleanPath}?{queryString}");
    }

    private ExternalMovieResultViewModel MapSearchResult(TmdbMovieListItem movie, Dictionary<int, string> genres)
    {
        return new ExternalMovieResultViewModel
        {
            ExternalId = movie.Id,
            Title = FirstNonEmpty(movie.Title, movie.OriginalTitle, "Untitled"),
            Genre = MapGenres(movie.GenreIds, genres),
            ReleaseYear = ParseReleaseYear(movie.ReleaseDate),
            Rating = ConvertRating(movie.VoteAverage),
            PosterUrl = BuildPosterUrl(movie.PosterPath),
            Description = FirstNonEmpty(movie.Overview, "No description available.")
        };
    }

    private string BuildPosterUrl(string? posterPath)
    {
        if (string.IsNullOrWhiteSpace(posterPath))
        {
            return string.Empty;
        }

        var imageBaseUrl = FirstNonEmpty(_settings.ImageBaseUrl, "https://image.tmdb.org/t/p/w500").TrimEnd('/');

        return $"{imageBaseUrl}/{posterPath.TrimStart('/')}";
    }

    private static string MapGenres(IEnumerable<int>? genreIds, IReadOnlyDictionary<int, string> genres)
    {
        var names = genreIds?
            .Where(genres.ContainsKey)
            .Select(genreId => genres[genreId])
            .Distinct()
            .ToList() ?? new List<string>();

        return names.Any() ? string.Join(", ", names) : "Unknown";
    }

    private static string MapGenres(IEnumerable<TmdbGenre>? genres)
    {
        var names = genres?
            .Select(genre => genre.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList() ?? new List<string>();

        return names.Any() ? string.Join(", ", names) : "Unknown";
    }

    private static int ParseReleaseYear(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate) || releaseDate.Length < 4)
        {
            return 0;
        }

        return int.TryParse(releaseDate[..4], out var year) ? year : 0;
    }

    private static decimal ConvertRating(double? rating)
    {
        var value = rating.GetValueOrDefault();

        if (value < 0)
        {
            value = 0;
        }
        else if (value > 10)
        {
            value = 10;
        }

        return Math.Round((decimal)value, 1);
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("TMDB API key is missing. Add it to appsettings.json before using external movie import.");
        }
    }

    private string GetBearerToken()
    {
        return _settings.ApiKey.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? _settings.ApiKey["Bearer ".Length..].Trim()
            : _settings.ApiKey.Trim();
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }

    private sealed class TmdbSearchResponse
    {
        [JsonPropertyName("results")]
        public List<TmdbMovieListItem>? Results { get; set; }
    }

    private sealed class TmdbMovieListItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("original_title")]
        public string? OriginalTitle { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double? VoteAverage { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int>? GenreIds { get; set; }
    }

    private sealed class TmdbMovieDetailsResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("original_title")]
        public string? OriginalTitle { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [JsonPropertyName("vote_average")]
        public double? VoteAverage { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("runtime")]
        public int Runtime { get; set; }

        [JsonPropertyName("genres")]
        public List<TmdbGenre>? Genres { get; set; }
    }

    private sealed class TmdbGenreResponse
    {
        [JsonPropertyName("genres")]
        public List<TmdbGenre>? Genres { get; set; }
    }

    private sealed class TmdbGenre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
