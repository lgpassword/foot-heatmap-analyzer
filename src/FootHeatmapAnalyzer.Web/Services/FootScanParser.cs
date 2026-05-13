using System.Text.RegularExpressions;
using FootHeatmapAnalyzer.Web.Models;

namespace FootHeatmapAnalyzer.Web.Services;

/// <summary>
/// Parses the demo binary format: width, height, left matrix, right matrix.
/// </summary>
public sealed class FootScanParser : IFootScanParser
{
    private const int HeaderLength = 2;
    private const long MaxUploadBytes = 1024 * 1024;
    private const int MaxDimension = 64;

    public ParsedFootScan ParseBytes(byte[] payload)
    {
        if (payload.Length < HeaderLength)
        {
            throw new InvalidDataException("Payload must start with width and height bytes.");
        }

        var width = payload[0];
        var height = payload[1];
        if (width <= 0 || height <= 0 || width > MaxDimension || height > MaxDimension)
        {
            throw new InvalidDataException($"Width and height must be between 1 and {MaxDimension}.");
        }

        var cellCount = width * height;
        var expectedLength = HeaderLength + (cellCount * 2);
        if (payload.Length != expectedLength)
        {
            throw new InvalidDataException($"Payload length must be {expectedLength} bytes for {width}x{height} left/right heatmaps.");
        }

        var left = ReadMatrix(payload, HeaderLength, width, height, FootSide.Left);
        var right = ReadMatrix(payload, HeaderLength + cellCount, width, height, FootSide.Right);
        return new ParsedFootScan(left, right);
    }

    public ParsedFootScan ParseText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidDataException("Input text is empty.");
        }

        var compact = Regex.Replace(input.Trim(), @"[\s,_:-]+", "");
        var bytes = IsBinary(compact)
            ? ParseBinaryBits(compact)
            : IsHex(compact)
                ? Convert.FromHexString(compact)
                : Convert.FromBase64String(input.Trim());

        return ParseBytes(bytes);
    }

    public async Task<ParsedFootScan> ParseFileAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            throw new InvalidDataException("Uploaded file is empty.");
        }

        if (file.Length > MaxUploadBytes)
        {
            throw new InvalidDataException($"Uploaded file must be {MaxUploadBytes / 1024} KB or smaller.");
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        return extension switch
        {
            ".bin" or ".dat" => ParseBytes(bytes),
            ".hex" or ".txt" or ".b64" or ".base64" => ParseText(System.Text.Encoding.UTF8.GetString(bytes)),
            _ => TryParseUnknownFile(bytes)
        };
    }

    private static FootHeatmap ReadMatrix(byte[] payload, int offset, int width, int height, FootSide side)
    {
        var rows = new double[height][];
        for (var y = 0; y < height; y++)
        {
            rows[y] = new double[width];
            for (var x = 0; x < width; x++)
            {
                rows[y][x] = payload[offset + (y * width) + x] / 255d;
            }
        }

        return new FootHeatmap(side, width, height, rows);
    }

    private static bool IsBinary(string compact) => compact.Length % 8 == 0 && compact.All(c => c is '0' or '1');

    private static bool IsHex(string compact) => compact.Length % 2 == 0 && compact.All(Uri.IsHexDigit);

    private static byte[] ParseBinaryBits(string compact)
    {
        var bytes = new byte[compact.Length / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(compact.Substring(i * 8, 8), 2);
        }

        return bytes;
    }

    private ParsedFootScan TryParseUnknownFile(byte[] bytes)
    {
        try
        {
            return ParseBytes(bytes);
        }
        catch (InvalidDataException)
        {
            return ParseText(System.Text.Encoding.UTF8.GetString(bytes));
        }
    }
}
