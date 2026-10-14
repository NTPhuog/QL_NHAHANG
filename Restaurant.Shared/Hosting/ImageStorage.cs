using Microsoft.AspNetCore.Http;

namespace RestaurantManagement.Hosting;

public sealed class ImageStorage(string root)
{
    public const long MaxBytes = 5 * 1024 * 1024;
    public string Root { get; } = Path.GetFullPath(root);

    public async Task<string?> SaveAsync(IFormFile? file, string category)
    {
        if (file is null) return null;
        if (category is not ("mon-an" or "ban-an")) throw new ArgumentException("Unknown image category");
        if (file.Length == 0 || file.Length > MaxBytes)
            throw new InvalidDataException("Ảnh phải có dung lượng từ 1 byte đến 5 MB.");
        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream();
        var block = new byte[81920];
        int read;
        while ((read = await input.ReadAsync(block)) != 0)
        {
            if (buffer.Length + read > MaxBytes) throw new InvalidDataException("Ảnh vượt quá giới hạn 5 MB.");
            await buffer.WriteAsync(block.AsMemory(0, read));
        }
        var bytes = buffer.ToArray();
        var extension = DetectExtension(bytes);
        if (extension is null) throw new InvalidDataException("Chỉ nhận ảnh JPG, PNG hoặc WebP. Tệp đã chọn không có định dạng ảnh hợp lệ.");
        var directory = Path.Combine(Root, category);
        Directory.CreateDirectory(directory);
        var name = Guid.NewGuid().ToString("N") + extension;
        var path = Path.Combine(directory, name);
        try { await File.WriteAllBytesAsync(path, bytes); }
        catch { if (File.Exists(path)) File.Delete(path); throw; }
        return $"/uploads/{category}/{name}";
    }

    public static string? DetectExtension(byte[] bytes)
    {
        if (bytes.Length >= 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) && bytes.AsSpan(12,4).SequenceEqual("IHDR"u8)) return ".png";
        if (bytes.Length >= 4 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255 && bytes[^2] == 255 && bytes[^1] == 217) return ".jpg";
        if (bytes.Length >= 20 && bytes.AsSpan(0,4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8,4).SequenceEqual("WEBP"u8) && (bytes.AsSpan(12,4).SequenceEqual("VP8 "u8) || bytes.AsSpan(12,4).SequenceEqual("VP8L"u8) || bytes.AsSpan(12,4).SequenceEqual("VP8X"u8))) return ".webp";
        return null;
    }

    // Delete only a newly-created managed upload when its database write failed.
    public void Discard(string? url)
    {
        if (url is null || !System.Text.RegularExpressions.Regex.IsMatch(url, @"^/uploads/(mon-an|ban-an)/[a-f0-9]{32}\.(png|jpg|webp)$")) return;
        var path = Path.Combine(Root, url[9..].Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path)) File.Delete(path);
    }
}
