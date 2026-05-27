namespace apbd_2026_12.DTOs;

public class GetRoomDto
{
    public string Id { get; set; } = string.Empty;
    public bool HasTv { get; set; }
    public GetWardDto Ward { get; set; } = null!;
}