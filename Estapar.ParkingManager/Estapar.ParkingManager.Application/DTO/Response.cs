using System.Net;

namespace Estapar.ParkingManager.Application.DTO;

/// <summary>Envelope de resposta padrão retornado por todos os endpoints da Estapar.ParkingManager.Api, tanto em caso de sucesso quanto de erro.</summary>
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
