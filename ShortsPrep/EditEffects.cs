namespace ShortsPrep;

/// <summary>Un segment de la source à conserver dans le montage final, dans l'ordre choisi.</summary>
public record EditSegment(double StartSeconds, double EndSeconds)
{
    public double Duration => Math.Max(0, EndSeconds - StartSeconds);
}

/// <summary>Réglages colorimétriques et de rotation appliqués au montage.</summary>
public record EditEffects(
    double Brightness,   // -1..1, 0 = neutre
    double Contrast,     // 0..2, 1 = neutre
    double Saturation,   // 0..3, 1 = neutre
    int RotationDegrees) // 0, 90, 180, 270
{
    public static readonly EditEffects None = new(0, 1, 1, 0);
    public bool IsNeutral => Brightness == 0 && Contrast == 1 && Saturation == 1 && RotationDegrees == 0;
}

public enum CutExportMode
{
    /// <summary>Copie des flux, zéro ré-encodage, coupes alignées sur l'image clé la plus proche.</summary>
    FastStreamCopy,
    /// <summary>Ré-encodage CRF 0 (vidéo) + FLAC (audio) : précis à la frame, 100% sans perte.</summary>
    PreciseLosslessReencode
}
