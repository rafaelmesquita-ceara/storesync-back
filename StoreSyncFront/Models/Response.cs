using System.IO.Compression;

namespace StoreSyncFront.Models;

public class Response
{
    public int Status { get; set; }
    public string? Body { get; set; }

    public Response(int status, string? body)
    {
        Status = status;
        Body = body;
    }

    public bool IsSuccess()
    {
        return Status >= 200 && Status < 300;
    }
}