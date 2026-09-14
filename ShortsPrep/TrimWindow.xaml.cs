using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace ShortsPrep;

public partial class TrimWindow : Window
{
    private readonly VideoProcessor _processor = new();
    private readonly DispatcherTimer _previewStopTimer = new();
    private readonly string _sourcePath;
    private string? _proxyPath;
    private bool _updatingFromCode;
    private bool _mediaReady;

    /// <summary>Résultat validé par l'utilisateur (null si annulé).</summary>
    public TrimRange? Result { get; private set; }

    public TrimWindow(string mediaPath)
    {
        InitializeComponent();
        _sourcePath = mediaPath;

        _previewStopTimer.Tick += (_, _) =>
        {
            _previewStopTimer.Stop();
            Media.Pause();
        };

        Loaded += async (_, _) => await PreparePreviewAsync();
    }

    /// <summary>
    /// Génère un petit proxy H.264/AAC toujours lisible par le lecteur Windows, même si
    /// le fichier d'origine est dans un codec qu'il ne sait pas décoder (HEVC, etc.).
    /// La sélection porte sur les timestamps, identiques entre le proxy et l'original.
    /// </summary>
    private async Task PreparePreviewAsync()
    {
        try
        {
            var info = await _processor.ProbeAsync(_sourcePath);
            double total = info.DurationSeconds > 0 ? info.DurationSeconds : 1;

            _updatingFromCode = true;
            StartSlider.Maximum = total;
            EndSlider.Maximum = total;
            StartSlider.Value = 0;
            EndSlider.Value = total;
            _updatingFromCode = false;
            UpdateLabels();

            StatusMessage("Préparation de l'aperçu (0%)...");
            _proxyPath = Path.Combine(Path.GetTempPath(), $"shortsprep_trimpreview_{Guid.NewGuid():N}.mp4");
            var percent = new Progress<int>(p => StatusMessage($"Préparation de l'aperçu ({p}%)..."));
            await _processor.CreateCompatiblePreviewAsync(_sourcePath, _proxyPath, percent);

            Media.Source = new Uri(_proxyPath, UriKind.Absolute);
        }
        catch (Exception ex)
        {
            StatusMessage("Aperçu indisponible (" + ex.Message + "). Tu peux quand même choisir " +
                          "une plage en te basant sur la durée affichée.");
        }
    }

    private void Media_MediaOpened(object sender, RoutedEventArgs e)
    {
        _mediaReady = true;
        UpdateLabels();
        Media.Play();
        Media.Pause();
    }

    private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        StatusMessage("L'aperçu n'a pas pu être affiché, mais la sélection reste utilisable " +
                      "(basée sur la durée détectée).");
    }

    private void StatusMessage(string text)
    {
        DurationLabel.Text = text;
    }

    private void StartSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingFromCode) return;
        if (StartSlider.Value >= EndSlider.Value)
            StartSlider.Value = Math.Max(0, EndSlider.Value - 0.5);
        UpdateLabels();
    }

    private void EndSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingFromCode) return;
        if (EndSlider.Value <= StartSlider.Value)
            EndSlider.Value = Math.Min(EndSlider.Maximum, StartSlider.Value + 0.5);
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        StartLabel.Text = FormatTime(StartSlider.Value);
        EndLabel.Text = FormatTime(EndSlider.Value);
        DurationLabel.Text = $"Durée sélectionnée : {(EndSlider.Value - StartSlider.Value):F1}s";
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 100}";
    }

    private void PreviewButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_mediaReady)
        {
            StatusMessage("Aperçu indisponible pour ce fichier.");
            return;
        }

        Media.Position = TimeSpan.FromSeconds(StartSlider.Value);
        Media.Play();

        var previewLength = Math.Min(EndSlider.Value - StartSlider.Value, 15);
        _previewStopTimer.Interval = TimeSpan.FromSeconds(Math.Max(0.2, previewLength));
        _previewStopTimer.Stop();
        _previewStopTimer.Start();
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        Result = new TrimRange(StartSlider.Value, EndSlider.Value);
        CleanupAndClose(true);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Result = null;
        CleanupAndClose(false);
    }

    private void CleanupAndClose(bool dialogResult)
    {
        try { Media.Stop(); Media.Close(); } catch { /* ignore */ }
        DialogResult = dialogResult;
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_proxyPath is not null && File.Exists(_proxyPath))
        {
            try { File.Delete(_proxyPath); } catch { /* best effort */ }
        }
        base.OnClosed(e);
    }
}
