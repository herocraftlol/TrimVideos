using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

using Path = System.IO.Path;

namespace ShortsPrep;

public partial class VideoEditorWindow : Window
{
    private readonly string _sourcePath;
    private readonly VideoProcessor _processor = new();
    private readonly DispatcherTimer _positionTimer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private readonly List<EditSegment> _segments = new();
    private string? _proxyPath;
    private bool _mediaReady;
    private bool _scrubbing;
    private double _durationSeconds = 1;
    private double? _markStart;
    private double? _markEnd;
    private bool _isPlaying;
    private Rectangle? _playhead;

    public VideoEditorWindow(string sourcePath)
    {
        InitializeComponent();
        _sourcePath = sourcePath;

        _positionTimer.Tick += (_, _) =>
        {
            if (!_mediaReady || _scrubbing) return;
            CurrentTimeText.Text = FormatTime(Media.Position.TotalSeconds);
            UpdatePlayheadPosition(Media.Position.TotalSeconds);
        };
        _positionTimer.Start();

        Loaded += async (_, _) => await PreparePreviewAsync();
        UpdateExportModeHint();
    }

    /// <summary>
    /// Génère un proxy de prévisualisation universellement compatible (H.264 baseline +
    /// AAC, basse résolution) via ffmpeg. Corrige les cas où le fichier d'origine (HEVC,
    /// codec exotique, conteneur inhabituel...) n'est pas lisible par le lecteur intégré
    /// à Windows : le proxy, lui, est toujours lisible car on en maîtrise entièrement le
    /// format. L'export final travaille toujours sur le fichier d'origine en pleine
    /// qualité — le proxy ne sert qu'à l'aperçu et au marquage des coupes.
    /// </summary>
    private async Task PreparePreviewAsync()
    {
        try
        {
            var info = await _processor.ProbeAsync(_sourcePath);
            _durationSeconds = Math.Max(1, info.DurationSeconds);
            TotalTimeText.Text = FormatTime(_durationSeconds);

            _proxyPath = Path.Combine(Path.GetTempPath(), $"shortsprep_preview_{Guid.NewGuid():N}.mp4");
            LoadingText.Text = "Préparation de l'aperçu (peut prendre quelques secondes)...";
            await _processor.CreateCompatiblePreviewAsync(_sourcePath, _proxyPath);

            Media.Source = new Uri(_proxyPath, UriKind.Absolute);
        }
        catch (Exception ex)
        {
            Log("Impossible de préparer l'aperçu : " + ex.Message);
            Log("Le montage et l'export restent possibles ; seule la prévisualisation est indisponible.");
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingText.Text = "Aperçu indisponible (voir le journal ci-dessous).";
            DrawTimelineBackground();
        }
    }

    private void Media_MediaOpened(object sender, RoutedEventArgs e)
    {
        _mediaReady = true;
        LoadingOverlay.Visibility = Visibility.Collapsed;
        if (Media.NaturalDuration.HasTimeSpan)
        {
            _durationSeconds = Math.Max(1, Media.NaturalDuration.TimeSpan.TotalSeconds);
            TotalTimeText.Text = FormatTime(_durationSeconds);
        }
        Media.Play();
        Media.Pause();
        DrawTimelineBackground();
        RedrawSegments();
    }

    private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        Log("L'aperçu généré n'a pas pu être lu (cas rare). Le montage/export sur le fichier " +
            "d'origine reste possible, mais tu devras marquer les temps sans prévisualisation.");
        LoadingText.Text = "Aperçu indisponible.";
        DrawTimelineBackground();
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_mediaReady) return;
        if (_isPlaying) { Media.Pause(); PlayPauseButton.Content = "▶"; }
        else { Media.Play(); PlayPauseButton.Content = "⏸"; }
        _isPlaying = !_isPlaying;
    }

    // --- Timeline visuelle (dessinée à la main sur un Canvas) ---

    private void TimelineCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        DrawTimelineBackground();
        RedrawSegments();
    }

    private void DrawTimelineBackground()
    {
        TimelineCanvas.Children.Clear();
        double w = TimelineCanvas.ActualWidth;
        double h = TimelineCanvas.ActualHeight;
        if (w <= 0 || h <= 0) return;

        var track = new Rectangle
        {
            Width = w, Height = h,
            Fill = new SolidColorBrush(Color.FromRgb(0x1F, 0x18, 0x15))
        };
        TimelineCanvas.Children.Add(track);

        _playhead = new Rectangle
        {
            Width = 2, Height = h,
            Fill = (Brush)FindResource("AccentGoldBrush")
        };
        Canvas.SetLeft(_playhead, 0);
        TimelineCanvas.Children.Add(_playhead);
    }

    private void RedrawSegments()
    {
        double w = TimelineCanvas.ActualWidth;
        double h = TimelineCanvas.ActualHeight;
        if (w <= 0 || h <= 0) return;

        var toRemove = TimelineCanvas.Children.OfType<Rectangle>()
            .Where(r => r.Tag as string == "segment").ToList();
        foreach (var r in toRemove) TimelineCanvas.Children.Remove(r);

        foreach (var seg in _segments)
        {
            double x = seg.StartSeconds / _durationSeconds * w;
            double segW = Math.Max(2, seg.Duration / _durationSeconds * w);
            var rect = new Rectangle
            {
                Width = segW, Height = h - 8,
                Fill = (Brush)FindResource("AccentBrush"),
                RadiusX = 4, RadiusY = 4,
                Opacity = 0.85,
                Tag = "segment"
            };
            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, 4);
            TimelineCanvas.Children.Insert(1, rect); // au-dessus du fond, sous le playhead
        }

        if (_playhead is not null) UpdatePlayheadPosition(_mediaReady ? Media.Position.TotalSeconds : 0);
    }

    private void UpdatePlayheadPosition(double seconds)
    {
        if (_playhead is null) return;
        double w = TimelineCanvas.ActualWidth;
        if (w <= 0) return;
        double x = Math.Clamp(seconds / _durationSeconds * w, 0, w - 2);
        Canvas.SetLeft(_playhead, x);
    }

    private double PositionFromMouse(MouseEventArgs e)
    {
        double w = TimelineCanvas.ActualWidth;
        double x = Math.Clamp(e.GetPosition(TimelineCanvas).X, 0, w);
        return w > 0 ? x / w * _durationSeconds : 0;
    }

    private void TimelineCanvas_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _scrubbing = true;
        TimelineCanvas.CaptureMouse();
        SeekTo(PositionFromMouse(e));
    }

    private void TimelineCanvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (_scrubbing) SeekTo(PositionFromMouse(e));
    }

    private void TimelineCanvas_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _scrubbing = false;
        TimelineCanvas.ReleaseMouseCapture();
    }

    private void SeekTo(double seconds)
    {
        if (_mediaReady) Media.Position = TimeSpan.FromSeconds(seconds);
        CurrentTimeText.Text = FormatTime(seconds);
        UpdatePlayheadPosition(seconds);
    }

    // --- Marquage des segments ---

    private void MarkStartButton_Click(object sender, RoutedEventArgs e)
    {
        _markStart = _mediaReady ? Media.Position.TotalSeconds : 0;
        UpdateMarkRangeText();
    }

    private void MarkEndButton_Click(object sender, RoutedEventArgs e)
    {
        _markEnd = _mediaReady ? Media.Position.TotalSeconds : 0;
        UpdateMarkRangeText();
    }

    private void UpdateMarkRangeText()
    {
        string start = _markStart is null ? "?" : FormatTime(_markStart.Value);
        string end = _markEnd is null ? "?" : FormatTime(_markEnd.Value);
        MarkRangeText.Text = _markStart is null && _markEnd is null ? "Aucun point marqué" : $"{start} → {end}";
    }

    private void AddSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (_markStart is null || _markEnd is null)
        {
            Log("Marque un début et une fin avant d'ajouter un segment.");
            return;
        }
        if (_markEnd <= _markStart)
        {
            Log("La fin doit être après le début.");
            return;
        }

        _segments.Add(new EditSegment(_markStart.Value, _markEnd.Value));
        RefreshSegmentsList(_segments.Count - 1);

        _markStart = null;
        _markEnd = null;
        UpdateMarkRangeText();
    }

    private void MoveSegmentUp_Click(object sender, RoutedEventArgs e)
    {
        int i = SegmentsList.SelectedIndex;
        if (i <= 0) return;
        (_segments[i - 1], _segments[i]) = (_segments[i], _segments[i - 1]);
        RefreshSegmentsList(i - 1);
    }

    private void MoveSegmentDown_Click(object sender, RoutedEventArgs e)
    {
        int i = SegmentsList.SelectedIndex;
        if (i < 0 || i >= _segments.Count - 1) return;
        (_segments[i + 1], _segments[i]) = (_segments[i], _segments[i + 1]);
        RefreshSegmentsList(i + 1);
    }

    private void RemoveSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        int i = SegmentsList.SelectedIndex;
        if (i < 0) return;
        _segments.RemoveAt(i);
        RefreshSegmentsList(-1);
    }

    private void RefreshSegmentsList(int selectIndex)
    {
        SegmentsList.Items.Clear();
        foreach (var s in _segments)
            SegmentsList.Items.Add($"{FormatTime(s.StartSeconds)}  →  {FormatTime(s.EndSeconds)}   ({s.Duration:F1}s)");
        if (selectIndex >= 0 && selectIndex < SegmentsList.Items.Count)
            SegmentsList.SelectedIndex = selectIndex;
        RedrawSegments();
    }

    // --- Effets / export ---

    private void MotionCheck_Changed(object sender, RoutedEventArgs e)
    {
        if (KenBurnsIntensityPanel is null || BassSensitivityPanel is null) return;
        KenBurnsIntensityPanel.Visibility = KenBurnsCheck.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        BassSensitivityPanel.Visibility = BassReactiveCheck.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        UpdateExportModeHint();
    }

    private void ExportMode_Changed(object sender, RoutedEventArgs e) => UpdateExportModeHint();

    private bool EffectsRequireReencode() =>
        KenBurnsCheck.IsChecked == true || BassReactiveCheck.IsChecked == true ||
        RotationCombo.SelectedIndex != 0 ||
        (int)BrightnessSlider.Value != 0 || (int)ContrastSlider.Value != 100 || (int)SaturationSlider.Value != 100;

    private void UpdateExportModeHint()
    {
        if (ExportModeHint is null) return;
        if (EffectsRequireReencode() && FastModeRadio.IsChecked == true)
            ExportModeHint.Text = "Des effets sont actifs : le mode précis sera utilisé automatiquement (une copie de flux ne peut pas être filtrée).";
        else if (FastModeRadio.IsChecked == true)
            ExportModeHint.Text = "Zéro ré-encodage, zéro perte garantie. Les coupes peuvent démarrer un peu avant le point exact marqué (image clé la plus proche).";
        else
            ExportModeHint.Text = "Coupes exactes à l'image près, effets appliqués, 100% sans perte (vidéo CRF 0 + audio FLAC). Plus lent, fichier volumineux. Export en .mkv.";
    }

    private EditEffects GetEffects() => new(
        Brightness: BrightnessSlider.Value / 100.0,
        Contrast: ContrastSlider.Value / 100.0,
        Saturation: SaturationSlider.Value / 100.0,
        RotationDegrees: RotationCombo.SelectedIndex switch { 1 => 90, 2 => 180, 3 => 270, _ => 0 });

    private MotionSettings GetMotion() => new(
        KenBurnsEnabled: KenBurnsCheck.IsChecked == true,
        KenBurnsIntensity01: KenBurnsIntensitySlider.Value / 100.0,
        BassReactiveEnabled: BassReactiveCheck.IsChecked == true,
        BassSensitivity01: BassSensitivitySlider.Value / 100.0);

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_segments.Count == 0)
        {
            Log("Ajoute au moins un segment à garder avant d'exporter.");
            return;
        }

        bool mustReencode = EffectsRequireReencode() || PreciseModeRadio.IsChecked == true;
        var mode = mustReencode ? CutExportMode.PreciseLosslessReencode : CutExportMode.FastStreamCopy;

        var saveDialog = new SaveFileDialog
        {
            Title = "Nommer la vidéo montée",
            Filter = mode == CutExportMode.PreciseLosslessReencode
                ? "Vidéo sans perte (.mkv)|*.mkv"
                : "Vidéo MP4|*.mp4",
            FileName = Path.GetFileNameWithoutExtension(_sourcePath) +
                       (mode == CutExportMode.PreciseLosslessReencode ? "_montage.mkv" : "_montage.mp4"),
            InitialDirectory = Path.GetDirectoryName(_sourcePath)
        };
        if (saveDialog.ShowDialog() != true) return;

        ExportButton.IsEnabled = false;
        ProgressBarCtrl.Visibility = Visibility.Visible;
        ProgressPercentText.Visibility = Visibility.Visible;
        ProgressBarCtrl.Value = 0;
        try
        {
            var percent = new Progress<int>(p =>
            {
                ProgressBarCtrl.Value = p;
                ProgressPercentText.Text = $"{p}%";
            });

            await _processor.EditVideoAsync(
                _sourcePath, saveDialog.FileName, _segments, GetEffects(), mode, GetMotion(),
                new Progress<string>(Log), percent);

            Log($"Terminé -> {saveDialog.FileName}");
            Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log("ERREUR : " + ex.Message);
        }
        finally
        {
            ExportButton.IsEnabled = true;
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => CleanupAndClose();

    private void CleanupAndClose()
    {
        try { Media.Stop(); Media.Close(); } catch { /* ignore */ }
        _positionTimer.Stop();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_proxyPath is not null && File.Exists(_proxyPath))
        {
            try { File.Delete(_proxyPath); } catch { /* fichier temporaire, best effort */ }
        }
        base.OnClosed(e);
    }

    private static string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(Math.Max(0, seconds));
        return $"{(int)ts.TotalMinutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 100}";
    }

    private void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LogText.Text += message + Environment.NewLine;
            LogScroll.ScrollToEnd();
        });
    }
}
