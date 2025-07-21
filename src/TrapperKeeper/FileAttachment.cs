namespace TrapperKeeper;

public class FileAttachment
{
    public string Id { get; set; } = "";
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long Size { get; set; }
    public string StorageUrl { get; set; } = "";
    public DateTime UploadedAt { get; set; }
}