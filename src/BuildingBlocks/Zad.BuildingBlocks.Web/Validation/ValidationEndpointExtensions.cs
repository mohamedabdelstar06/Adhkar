using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Zad.BuildingBlocks.Web.Validation;

public static class ValidationEndpointExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>();
}

