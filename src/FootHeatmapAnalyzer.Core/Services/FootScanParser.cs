using System.Text.RegularExpressions;
using FootHeatmapAnalyzer.Core.Models;

namespace FootHeatmapAnalyzer.Core.Services;

/// <summary>
/// Parses the demo binary format: width, height, left matrix, right matrix.
/// </summary>
public sealed class FootScanParser : IFootScanParser
{
    private const int HeaderLength = 2;
    private const int MaxDimension = 64;

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

        var compact = Regex.Replace(input.Trim(), @"[\s,_:-]+", "");
        var bytes = IsBinary(compact)
            ? ParseBinaryBits(compact)
            : IsHex(compact)
                ? Convert.FromHexString(compact)
                : Convert.FromBase64String(input.Trim());

        return ParseBytes(bytes);
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

}
