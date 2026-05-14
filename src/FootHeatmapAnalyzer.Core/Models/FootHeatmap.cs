namespace FootHeatmapAnalyzer.Core.Models;

/// <summary>
/// Represents one foot as a normalized two-dimensional sensor matrix.
/// </summary>
public sealed class FootHeatmap
{
    private readonly double[] values;

    /// <summary>
    /// Creates a normalized foot heatmap backed by a contiguous row-major value buffer.
    /// </summary>
    public FootHeatmap(FootSide side, int width, int height, double[] values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        }

        if (values.Length != width * height)
        {
            throw new ArgumentException("Value count must match width * height.", nameof(values));
        }

        this.values = values.ToArray();
        Side = side;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Identifies whether this matrix belongs to the left or right foot.
    /// </summary>
    public FootSide Side { get; }

    /// <summary>
    /// Number of sensor cells in each row.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Number of sensor rows in the matrix.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Row-major normalized pressure or temperature values exposed as read-only data.
    /// </summary>
    public IReadOnlyList<double> Values => values;

    /// <summary>
    /// Exposes the contiguous value buffer without allocating.
    /// </summary>
    public ReadOnlySpan<double> AsSpan() => values;

    /// <summary>
    /// Reads one normalized matrix value by column and row.
    /// </summary>
    public double At(int x, int y) => values[(y * Width) + x];

    /// <summary>
    /// Converts the row-major buffer to rows for JSON and UI rendering.
    /// </summary>
    public double[][] ToRows()
    {
        var rows = new double[Height][];
        for (var y = 0; y < Height; y++)
        {
            rows[y] = new double[Width];
            Array.Copy(values, y * Width, rows[y], 0, Width);
        }

        return rows;
    }
}
