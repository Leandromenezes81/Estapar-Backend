using System.Net;

namespace Estapar.ParkingManager.Application.DTO;

public partial class Response
{
    public IList<string> ErrorMessages { get; private set; } = [];
    public bool HasErrors
    {
        get
        {
            return ErrorMessages.Any() && StatusCode != HttpStatusCode.OK;
        }
    }
    public string? Message { get; private set; }
    public HttpStatusCode StatusCode { get; private set; }
    public object? Collection { get; set; }
    public int Count { get; set; }
}
