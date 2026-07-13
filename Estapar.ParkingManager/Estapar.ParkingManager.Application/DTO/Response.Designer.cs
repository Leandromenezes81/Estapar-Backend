using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Estapar.ParkingManager.Application.DTO;

partial class Response
{
    /// <summary>Define a mensagem de sucesso ou erro da resposta.</summary>
    public void SetMessage(string message) => Message = message;

    /// <summary>Define o código de status HTTP da resposta.</summary>
    public void SetStatusCode(HttpStatusCode statusCode) => StatusCode = statusCode;

    /// <summary>Adiciona uma mensagem de erro à lista de erros da resposta.</summary>
    public void AddErrorMessages(string message) => ErrorMessages.Add(message);

    /// <summary>Monta e retorna a resposta preenchida com status, mensagem e a coleção de dados retornada.</summary>
    public Task<Response> GenerateResponse(
        HttpStatusCode? statusCode = null,
        string message = null,
        object collection = default,
        int count = default)
    {
        StatusCode = (HttpStatusCode)statusCode;
        Message = message;
        Collection = collection;
        Count = count;

        return Task.FromResult(this);
    }
}
