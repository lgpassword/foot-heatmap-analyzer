using System.Text.RegularExpressions;
using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Parses the demo binary format: width, height, left matrix, right matrix.
/// </summary>
public sealed partial class FootScanParser : IFootScanParser
{
    private const int HeaderLength = 2;
    private const int MaxDimension = 64;
    // Provides a stable user-facing parse error instead of leaking format exceptions.
    private const string UnknownTextFormatMessage = "无法识别为十六进制、二进制位串或 Base64 数据。";

    public ParsedFootScan ParseBytes(byte[] payload)
    {
        if (payload.Length < HeaderLength)
        {
            throw new InvalidDataException("数据必须以宽度和高度两个字节开头。");
        }

        var width = payload[0];
        var height = payload[1];
        if (width <= 0 || height <= 0 || width > MaxDimension || height > MaxDimension)
        {
            throw new InvalidDataException($"宽度和高度必须在 1 到 {MaxDimension} 之间。");
        }

        var cellCount = width * height;
        var expectedLength = HeaderLength + (cellCount * 2);
        if (payload.Length != expectedLength)
        {
            throw new InvalidDataException($"{width}x{height} 的左右足热力图需要 {expectedLength} 字节数据。");
        }

        var left = ReadMatrix(payload, HeaderLength, width, height, FootSide.Left);
        var right = ReadMatrix(payload, HeaderLength + cellCount, width, height, FootSide.Right);
        return new ParsedFootScan(left, right);
    }

    public ParsedFootScan ParseText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new InvalidDataException("输入内容为空。");
        }

        var trimmed = input.Trim();
        var compact = SeparatorsRegex().Replace(trimmed, "");
        var bytes = IsBinary(compact)
            ? ParseBinaryBits(compact)
            : IsHex(compact)
                ? Convert.FromHexString(compact)
                : ParseBase64(trimmed);

        return ParseBytes(bytes);
    }

    private static FootHeatmap ReadMatrix(byte[] payload, int offset, int width, int height, FootSide side)
    {
        var values = new double[width * height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                values[(y * width) + x] = payload[offset + (y * width) + x] / 255d;
            }
        }

        return new FootHeatmap(side, width, height, values);
    }

    private static bool IsBinary(string compact) => compact.Length % 8 == 0 && compact.All(c => c is '0' or '1');

    private static bool IsHex(string compact) => compact.Length % 2 == 0 && compact.All(Uri.IsHexDigit);

    /// <summary>
    /// Parses Base64 text and normalizes invalid input to the parser's public error contract.
    /// </summary>
    private static byte[] ParseBase64(string trimmed)
    {
        try
        {
            return Convert.FromBase64String(trimmed);
        }
        catch (FormatException ex)
        {
            throw new InvalidDataException(UnknownTextFormatMessage, ex);
        }
    }

    private static byte[] ParseBinaryBits(string compact)
    {
        var bytes = new byte[compact.Length / 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(compact.Substring(i * 8, 8), 2);
        }

        return bytes;
    }

    /// <summary>
    /// Matches separators allowed in pasted textual payloads.
    /// </summary>
    [GeneratedRegex(@"[\s,_:-]+")]
    private static partial Regex SeparatorsRegex();
}
