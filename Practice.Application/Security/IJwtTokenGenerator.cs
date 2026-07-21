using Practice.Domain.Models;

namespace Practice.Application.Security;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}
