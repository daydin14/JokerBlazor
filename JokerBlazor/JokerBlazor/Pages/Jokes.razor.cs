using JokerBlazor.Models;
using JokerBlazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace JokerBlazor.Pages;

public partial class Jokes
{
    [Inject] public JokeService JokeService { get; set; } = default!;
    [Inject] public JokeLibrary Library { get; set; } = default!;
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Inject] public NavigationManager Navigation { get; set; } = default!;
    [Inject] public ISnackbar Snackbar { get; set; } = default!;
    [Inject] public DarkModeService Theme { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "id")]
    public int? RequestedId { get; set; }

    private readonly SessionHistory _history = new();
    private readonly Dictionary<string, bool> _flags = new(StringComparer.OrdinalIgnoreCase);
    private JokeQuery _query = JokeQuery.SafeDefault();
    private CancellationTokenSource? _requests;
    private DotNetObjectReference<Jokes>? _self;
    private bool _loading = true;
    private bool _ready;
    private bool _revealed;
    private bool _quiz;
    private bool _help;
    private bool _pop;
    private string? _error;
    private string? _guess;

    private JokeCard? Current => _history.Current;
    private bool CanBack => _history.CanBack && !_loading;
    private bool CanForward => _history.CanForward && !_loading;
    private bool IsFavorite => Current is not null && Library.Favorites.Contains(Current.Id);
    private int Score => Current is null ? 0 : Library.Ratings.ScoreFor(Current.Id) ?? 0;

    protected override void OnInitialized() => SyncFlags();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _self = DotNetObjectReference.Create(this);
        await JS.InvokeVoidAsync("joker.listenKeys", _self);
        await Library.EnsureLoadedAsync();
        _query = Library.Settings;
        SyncFlags();
        _ready = true;
        if (RequestedId is int id && id > 0)
        {
            await ShowById(id);
        }
        else
        {
            await LoadNew();
        }

        StateHasChanged();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_ready || RequestedId is not int id || id <= 0 || Current?.Id == id)
        {
            return;
        }

        await ShowById(id);
    }

    private async Task LoadNew() =>
        await Run(async token =>
        {
            JokeFetchResult? last = null;
            for (var attempt = 0; attempt < 5; attempt++)
            {
                last = await JokeService.GetJokeAsync(_query, token);
                if (!last.Ok || last.Joke is null || !_history.HasSeen(last.Joke.Id) || attempt == 4)
                {
                    return last;
                }
            }

            return last!;
        });

    private async Task LoadDaily()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var result = await JokeService.GetJokeOfTheDayAsync(today);
        if (result.Ok && result.Joke is not null && _history.MoveTo(result.Joke.Id))
        {
            ResetReveal();
            SyncAddress(result.Joke.Id);
            return;
        }

        await Run(_ => Task.FromResult(result));
    }

    private Task ShowById(int id)
    {
        if (_history.MoveTo(id))
        {
            ResetReveal();
            SyncAddress(id);
            return Task.CompletedTask;
        }

        return Run(token => JokeService.GetByIdAsync(id, token));
    }

    private async Task Run(Func<CancellationToken, Task<JokeFetchResult>> fetch)
    {
        _requests?.Cancel();
        _requests?.Dispose();
        _requests = new CancellationTokenSource();
        var token = _requests.Token;
        _loading = true;
        _error = null;
        try
        {
            var result = await fetch(token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            if (!result.Ok || result.Joke is null)
            {
                var offline = await Library.OfflineJokesAsync();
                var fallback = offline.FirstOrDefault(joke => !_history.HasSeen(joke.Id)) ?? offline.FirstOrDefault();
                if (fallback is not null)
                {
                    Accept(fallback);
                    _error = "Showing a saved joke while JokeAPI is unreachable.";
                    Snackbar.Add(_error, Severity.Warning);
                    return;
                }

                _error = result.ErrorMessage ?? "No joke came back.";
                Snackbar.Add(_error, Severity.Warning);
                return;
            }

            Accept(result.Joke);
            await Library.RememberOfflineAsync(result.Joke);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                _loading = false;
            }
        }
    }

    private void Accept(JokeCard joke)
    {
        _history.Append(joke);
        ResetReveal();
        SyncAddress(joke.Id);
    }

    private void Back()
    {
        if (!_history.Back() || Current is null)
        {
            return;
        }

        ResetReveal();
        SyncAddress(Current.Id);
    }

    private void Forward()
    {
        if (!_history.Forward() || Current is null)
        {
            return;
        }

        ResetReveal();
        SyncAddress(Current.Id);
    }

    private void Reveal()
    {
        if (Current?.HasPunchline == true)
        {
            _revealed = true;
        }
    }

    private async Task ToggleFavorite()
    {
        if (Current is null)
        {
            return;
        }

        var added = await Library.ToggleFavoriteAsync(Current);
        if (added)
        {
            _pop = true;
            await JS.InvokeVoidAsync("joker.celebrate");
            Snackbar.Add("Saved to favorites", Severity.Success);
            _ = ClearPop();
        }
        else
        {
            Snackbar.Add("Removed from favorites", Severity.Normal);
        }
    }

    private async Task Rate(int score)
    {
        if (Current is null)
        {
            return;
        }

        await Library.RateAsync(Current, score);
        Snackbar.Add(Score switch
        {
            1 => "Added to your top jokes",
            -1 => "Thumbs down saved on this device",
            _ => "Rating cleared"
        }, Severity.Info);
    }

    private async Task CopyJoke()
    {
        if (Current is null)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("joker.copy", SpokenText());
            Snackbar.Add("Copied", Severity.Success);
        }
        catch (JSException)
        {
            Snackbar.Add("Copy is unavailable in this browser", Severity.Warning);
        }
    }

    private async Task ShareJoke()
    {
        if (Current is null)
        {
            return;
        }

        var url = Permalink(Current.Id);
        var status = await JS.InvokeAsync<string>("joker.share", "Joker's Manor", JokeText.ShareText(Current, true), url);
        if (status == "shared")
        {
            Snackbar.Add("Shared", Severity.Success);
            return;
        }

        if (status == "cancelled")
        {
            return;
        }

        await JS.InvokeVoidAsync("joker.copy", url);
        Snackbar.Add("Share isn't available here. The link is copied.", Severity.Info);
    }

    private async Task Speak()
    {
        if (Current is null)
        {
            return;
        }

        var ok = await JS.InvokeAsync<bool>("joker.speak", SpokenText());
        if (!ok)
        {
            Snackbar.Add("This browser cannot read aloud", Severity.Warning);
            return;
        }

        if (Current.HasPunchline && !_revealed)
        {
            Snackbar.Add("Reveal the punchline to hear the rest", Severity.Info);
        }
    }

    private async Task DownloadCard()
    {
        if (Current is null)
        {
            return;
        }

        await JS.InvokeVoidAsync("joker.downloadCard", new
        {
            primary = Current.Primary,
            punchline = Current.Punchline,
            category = Current.Category,
            dark = Theme.IsDarkMode,
            filename = $"joke-{Current.Id}.png"
        });
        Snackbar.Add("Joke card downloaded", Severity.Success);
    }

    private async Task CheckGuess()
    {
        if (Current?.Punchline is null)
        {
            return;
        }

        if (QuizMatch.IsCorrect(_guess, Current.Punchline))
        {
            _revealed = true;
            await JS.InvokeVoidAsync("joker.celebrate");
            Snackbar.Add("That's the punchline", Severity.Success);
            return;
        }

        Snackbar.Add("Not quite. Try again or reveal it.", Severity.Info);
    }

    private void ToggleQuiz()
    {
        if (Current?.HasPunchline != true)
        {
            Snackbar.Add("Quiz mode uses two-part jokes", Severity.Info);
            return;
        }

        _quiz = !_quiz;
    }

    private async Task SetCategory(string value)
    {
        _query.Category = value;
        await Library.SaveSettingsAsync(_query);
    }

    private async Task SetType(string value)
    {
        _query.JokeType = value;
        await Library.SaveSettingsAsync(_query);
    }

    private async Task OnFlag(string flag, bool selected)
    {
        _flags[flag] = selected;
        _query.Blacklist = _flags.Where(pair => pair.Value).Select(pair => pair.Key).ToList();
        await Library.SaveSettingsAsync(_query);
    }

    private bool FlagOn(string flag) => _flags.TryGetValue(flag, out var selected) && selected;

    [JSInvokable]
    public async Task OnShortcut(string key)
    {
        switch (key)
        {
            case "next":
                if (_history.CanForward)
                {
                    Forward();
                }
                else
                {
                    await LoadNew();
                }
                break;
            case "favorite":
                await ToggleFavorite();
                break;
            case "reveal":
                Reveal();
                break;
            case "copy":
                await CopyJoke();
                break;
            case "help":
                _help = !_help;
                break;
        }

        StateHasChanged();
    }

    private string SpokenText() => Current is null
        ? string.Empty
        : JokeText.ShareText(Current, _revealed || !Current.HasPunchline);

    private string Permalink(int id) => $"{Navigation.BaseUri}jokes?id={id}";

    private void SyncAddress(int id)
    {
        if (RequestedId == id)
        {
            return;
        }

        Navigation.NavigateTo($"jokes?id={id}", replace: true);
    }

    private void ResetReveal()
    {
        _revealed = false;
        _guess = string.Empty;
    }

    private void SyncFlags()
    {
        var selected = new HashSet<string>(_query.Blacklist, StringComparer.OrdinalIgnoreCase);
        foreach (var flag in JokeCatalog.BlacklistFlags)
        {
            _flags[flag] = selected.Contains(flag);
        }
    }

    private async Task ClearPop()
    {
        await Task.Delay(350);
        _pop = false;
        await InvokeAsync(StateHasChanged);
    }

    private static string FormatFlag(string flag) => flag.ToLowerInvariant() switch
    {
        "nsfw" => "NSFW",
        _ => char.ToUpperInvariant(flag[0]) + flag[1..]
    };

    public void Dispose()
    {
        _requests?.Cancel();
        _requests?.Dispose();
        _ = JS.InvokeVoidAsync("joker.stopKeys");
        _ = JS.InvokeVoidAsync("joker.stopSpeak");
        _self?.Dispose();
    }
}
