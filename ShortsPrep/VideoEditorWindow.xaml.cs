using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ShortsPrep;

public partial class VideoEditorWindow : Window
{
    private readonly string _sourcePath;
    private readonly VideoProcessor _processor = new();
    private readonly DispatcherTimer _positionTimer = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private readonly List<EditSegment> _segments = new();
    private bool _mediaReady;
    private bool _updatingFromCode;
    private double? _markStart;
    private bool _isPlaying;

    public VideoEditorWindow(string sourcePath)
    {
        InitializeComponent();
        _sourcePath = sourcePath;

        _positionTimer.Tick += (_, _) =>
        {
            if (!_mediaReady) return;
            _updatingFromCode = true;
            ScrubberSlider.Value = Media.Position.TotalSeconds;
            _updatingFromCode = false;
            CurrentTimeText.Text = FormatTime(Media.Position.TotalSeconds);
        };
        _positionTimer.Start();

        Loaded += (_, _) =>
        {
            try
            {
                Media.Source = new Uri(Path.GetFullPath(_sourcePath), UriKind.Absolute);
            }
            catch (Exception ex)
            {
                Log("Impossible de charger le fichier : " + ex.Message);
            }
        };

        UpdateExportModeHint();
    }

    private void Media_MediaOpened(object sender, RoutedEventArgs e)
    {
        _mediaReady = true;
        var total = Media.NaturalDuration.HasTimeSpan ? Media.NaturalDuration.TimeSpan.TotalSeconds : 0;
        ScrubberSlider.Maximum = Math.Max(1, total);
        TotalTimeText.Text = FormatTime(total);
        Media.Play();
        Media.Pause();
    }

    private void Media_MediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        Log("Aperçu impossible pour ce fichier (codec non lu par le lecteur Windows intégré). " +
            "Le montage/export reste possible, mais sans prévisualisation ni marquage précis à l'image.");
    }

    private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_mediaReady) return;
        if (_isPlaying) Media.Pause(); else Media.Play();
        _isPlaying = !_isPlaying;
    }

    private void ScrubberSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_updatingFromCode || !_mediaReady) return;
        Media.Position = TimeSpan.FromSeconds(ScrubberSlider.Value);
    }

    private void MarkStartButton_Click(object sender, RoutedEventArgs e)
    {
        _markStart = _mediaReady ? Media.Position.TotalSeconds : ScrubberSlider.Value;
        MarkStartText.Text = $"Début : {FormatTime(_markStart.Value)}";
    }

    private void MarkEndButton_Click(object sender, RoutedEventArgs e)
    {
        var end = _mediaReady ? Media.Position.TotalSeconds : ScrubberSlider.Value;
        MarkEndText.Text = $"Fin : {FormatTime(end)}";
        _pendingEnd = end;
    }
    private double? _pendingEnd;

    private void AddSegmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (_markStart is null || _pendingEnd is null)
        {
            Log("Marque un début et une fin avant d'ajouter un segment.");
            return;
        }
        if (_pendingEnd <= _markStart)
        {
            Log("La fin doit être après le début.");
            return;
        }

        var segment = new EditSegment(_markStart.Value, _pendingEnd.Value);
        _segments.Add(segment);
        SegmentsList.Items.Add($"{FormatTime(segment.StartSeconds)} -> {FormatTime(segment.EndSeconds)}  ({segment.Duration:F1}s)");

        _markStart = null;
        _pendingEnd = null;
        MarkStartText.Text = "Début : -";
        MarkEndText.Text = "Fin : -";
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
            SegmentsList.Items.Add($"{FormatTime(s.StartSeconds)} -> {FormatTime(s.EndSeconds)}  ({s.Duration:F1}s)");
        if (selectIndex >= 0 && selectIndex < SegmentsList.Items.Count)
            SegmentsList.SelectedIndex = selectIndex;
    }

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
            ExportModeHint.Text = "Des effets sont actifs : le mode rapide sera automatiquement ignoré au profit du mode précis (un effet ne peut pas s'appliquer à une simple copie de flux).";
        else if (FastModeRadio.IsChecked == true)
            ExportModeHint.Text = "Zéro ré-encodage, zéro perte garantie. Les coupes peuvent démarrer un peu avant le point exact marqué (alignement sur l'image clé la plus proche).";
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
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        try { Media.Stop(); } catch { /* ignore */ }
        _positionTimer.Stop();
        Close();
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
