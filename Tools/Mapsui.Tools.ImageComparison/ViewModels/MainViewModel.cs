using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Mapsui.Tools.ImageComparison.Services;
using ReactiveUI;
using SkiaSharp;

namespace Mapsui.Tools.ImageComparison.ViewModels;

enum DiffMode { GeneratedOnly = 0, GeneratedWithDiff = 1, DiffOnly = 2 }

sealed record DiffColorOption(string Name, Color Color)
{
    public SolidColorBrush Brush { get; } = new(Color);
}

sealed class MainViewModel : ReactiveObject, IDisposable
{
    readonly SettingsService _settings = new();
    readonly Func<Task<string?>> _pickFolder;

    string _rootPath;
    TestEntryViewModel? _selectedEntry;
    Bitmap? _originalBitmap;
    Bitmap? _diffOverlayBitmap;
    Bitmap? _generatedBitmap;
    string? _loadedOrigPath;
    string? _loadedGenPath;
    bool _hasGenerated;
    int _selectedDiffMode = (int)DiffMode.GeneratedWithDiff;
    DiffColorOption _selectedDiffColorOption = DiffColorPalette[0];
    string _statusMessage = string.Empty;
    CancellationTokenSource _thumbCts = new();

    public static IReadOnlyList<DiffColorOption> DiffColorPalette { get; } =
    [
        new("Red",     Colors.Red),
        new("Yellow",  Colors.Yellow),
        new("Cyan",    Colors.Cyan),
        new("Magenta", Colors.Magenta),
        new("White",   Colors.White),
    ];

    public MainViewModel(Func<Task<string?>> pickFolder)
    {
        _pickFolder = pickFolder;
        _rootPath = _settings.LoadRootPath();
        TestEntries = [];

        PickRootCommand = ReactiveCommand.CreateFromTask(PickRootAsync);
        RefreshCommand = ReactiveCommand.CreateFromTask(LoadTestNamesAsync);

        _ = LoadTestNamesAsync();
    }

    public string RootPath
    {
        get => _rootPath;
        private set => this.RaiseAndSetIfChanged(ref _rootPath, value);
    }

    public ObservableCollection<TestEntryViewModel> TestEntries { get; }

    public TestEntryViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEntry, value);
            _ = LoadImagesAsync();
        }
    }

    public Bitmap? OriginalBitmap
    {
        get => _originalBitmap;
        private set => this.RaiseAndSetIfChanged(ref _originalBitmap, value);
    }

    public Bitmap? DiffOverlayBitmap
    {
        get => _diffOverlayBitmap;
        private set => this.RaiseAndSetIfChanged(ref _diffOverlayBitmap, value);
    }

    public int SelectedDiffMode
    {
        get => _selectedDiffMode;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDiffMode, value);
            this.RaisePropertyChanged(nameof(ShowGeneratedImage));
            this.RaisePropertyChanged(nameof(ShowDiffOverlay));
        }
    }

    public DiffColorOption SelectedDiffColorOption
    {
        get => _selectedDiffColorOption;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDiffColorOption, value);
            _ = RecomputeOverlayAsync();
        }
    }

    public bool ShowGeneratedImage => _hasGenerated && _selectedDiffMode != (int)DiffMode.DiffOnly;
    public bool ShowDiffOverlay => _hasGenerated && _selectedDiffMode != (int)DiffMode.GeneratedOnly;

    public Bitmap? GeneratedBitmap
    {
        get => _generatedBitmap;
        private set => this.RaiseAndSetIfChanged(ref _generatedBitmap, value);
    }

    public bool HasGenerated
    {
        get => _hasGenerated;
        private set
        {
            this.RaiseAndSetIfChanged(ref _hasGenerated, value);
            this.RaisePropertyChanged(nameof(ShowGeneratedImage));
            this.RaisePropertyChanged(nameof(ShowDiffOverlay));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ReactiveCommand<Unit, Unit> PickRootCommand { get; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

    async Task PickRootAsync()
    {
        var path = await _pickFolder();
        if (path is null) return;
        RootPath = path;
        _settings.SaveRootPath(path);
        await LoadTestNamesAsync();
    }

    async Task LoadTestNamesAsync()
    {
        var origDir = Path.Combine(RootPath,
            Config.OriginalRelPath.Replace('/', Path.DirectorySeparatorChar));

        // Cancel any in-progress thumbnail loading
        await _thumbCts.CancelAsync();
        _thumbCts.Dispose();
        _thumbCts = new CancellationTokenSource();

        foreach (var entry in TestEntries) entry.Dispose();
        TestEntries.Clear();
        ClearImages();

        if (!Directory.Exists(origDir))
        {
            StatusMessage = $"Original folder not found: {origDir}";
            return;
        }

        var files = await Task.Run(() =>
        {
            var f = Directory.GetFiles(origDir, "*.png");
            Array.Sort(f);
            return f;
        });

        foreach (var f in files)
            TestEntries.Add(new TestEntryViewModel(Path.GetFileNameWithoutExtension(f)));

        StatusMessage = TestEntries.Count == 0
            ? $"No PNG files found in: {origDir}"
            : $"{TestEntries.Count} test images";

        // Auto-select the first image so the comparison is visible immediately
        if (TestEntries.Count > 0 && SelectedEntry is null)
            SelectedEntry = TestEntries[0];

        // Load thumbnails in the background with limited parallelism
        _ = LoadThumbnailsAsync(origDir, [.. TestEntries], _thumbCts.Token);
    }

    async Task LoadThumbnailsAsync(string origDir, TestEntryViewModel[] entries, CancellationToken ct)
    {
        using var semaphore = new SemaphoreSlim(4, 4);

        await Task.WhenAll(entries.Select(async entry =>
        {
            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                if (ct.IsCancellationRequested) return;
                var path = Path.Combine(origDir, entry.Name + ".png");
                entry.Thumbnail = await Task.Run(() => ImageDiffService.LoadThumbnail(path), ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) { /* expected on cancellation */ }
            finally
            {
                semaphore.Release();
            }
        }));
    }

    async Task LoadImagesAsync()
    {
        if (_selectedEntry is null) return;

        var origDir = Path.Combine(RootPath,
            Config.OriginalRelPath.Replace('/', Path.DirectorySeparatorChar));
        var genDir = Path.Combine(RootPath,
            Config.GenRelPath.Replace('/', Path.DirectorySeparatorChar));

        ClearImages();
        StatusMessage = "Loading…";

        var origPath = Path.Combine(origDir, _selectedEntry.Name + ".png");
        var genPath = Path.Combine(genDir, _selectedEntry.Name + ".png");

        var c = _selectedDiffColorOption.Color;
        var skColor = new SKColor(c.R, c.G, c.B, c.A);

        var (origAva, genAva, overlayAva, diffCount, error) = await Task.Run(() =>
        {
            Bitmap? orig = null;
            Bitmap? gen = null;
            Bitmap? overlay = null;
            try
            {
                using var origSk = ImageDiffService.LoadSkBitmap(origPath);
                if (origSk is null)
                    return ((Bitmap?)null, (Bitmap?)null, (Bitmap?)null, 0,
                        $"Original image not found: {origPath}");

                orig = ImageDiffService.ToAvaloniaBitmap(origSk);

                using var genSk = ImageDiffService.LoadSkBitmap(genPath);
                if (genSk is null)
                    return (orig, (Bitmap?)null, (Bitmap?)null, 0,
                        $"No generated image — run tests first. Expected: {genPath}");

                gen = ImageDiffService.ToAvaloniaBitmap(genSk);
                var (overlaySk, count) = ImageDiffService.ComputeDiffOverlay(origSk, genSk, skColor);
                overlay = ImageDiffService.ToAvaloniaBitmap(overlaySk);
                overlaySk.Dispose();
                return (orig, gen, overlay, count, (string?)null);
            }
            catch
            {
                orig?.Dispose();
                gen?.Dispose();
                overlay?.Dispose();
                throw;
            }
        });

        _loadedOrigPath = origPath;
        _loadedGenPath = genAva is not null ? genPath : null;
        OriginalBitmap = origAva;
        GeneratedBitmap = genAva;
        DiffOverlayBitmap = overlayAva;
        HasGenerated = genAva is not null;

        StatusMessage = error is not null
            ? error
            : diffCount == 0
                ? "✓ Images match"
                : $"⚠ {diffCount:N0} pixels differ";
    }

    void ClearImages()
    {
        OriginalBitmap?.Dispose();
        DiffOverlayBitmap?.Dispose();
        GeneratedBitmap?.Dispose();
        OriginalBitmap = null;
        DiffOverlayBitmap = null;
        GeneratedBitmap = null;
        _loadedOrigPath = null;
        _loadedGenPath = null;
        HasGenerated = false;
    }

    async Task RecomputeOverlayAsync()
    {
        if (_loadedOrigPath is null || _loadedGenPath is null) return;
        var c = _selectedDiffColorOption.Color;
        var skColor = new SKColor(c.R, c.G, c.B, c.A);
        var (origPath, genPath) = (_loadedOrigPath, _loadedGenPath);
        var newOverlay = await Task.Run(() =>
        {
            using var origSk = ImageDiffService.LoadSkBitmap(origPath);
            using var genSk = ImageDiffService.LoadSkBitmap(genPath);
            if (origSk is null || genSk is null) return (Bitmap?)null;
            var (overlaySk, _) = ImageDiffService.ComputeDiffOverlay(origSk, genSk, skColor);
            var ava = ImageDiffService.ToAvaloniaBitmap(overlaySk);
            overlaySk.Dispose();
            return ava;
        });
        if (newOverlay is null) return;
        DiffOverlayBitmap?.Dispose();
        DiffOverlayBitmap = newOverlay;
    }

    public void Dispose()
    {
        ClearImages();
        foreach (var entry in TestEntries) entry.Dispose();
        _thumbCts.Cancel();
        _thumbCts.Dispose();
        PickRootCommand.Dispose();
        RefreshCommand.Dispose();
    }
}
