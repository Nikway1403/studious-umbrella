using Contracts.DTOs.ValidationDtos;

namespace Abstractions.Services;

public interface IValidationService
{
    ValidationDto ValidateAccessToken(string accessToken);
}