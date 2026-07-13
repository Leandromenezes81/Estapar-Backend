using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Estapar.ParkingManager.Application.DTO;

partial class Response
{
    public void SetMessage(string message) => Message = message;

    public void SetStatusCode(HttpStatusCode statusCode) => StatusCode = statusCode;

    public void AddErrorMessages(string message) => ErrorMessages.Add(message);

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
