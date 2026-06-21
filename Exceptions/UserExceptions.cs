namespace WheresWaldoApi.Exceptions;

public class UsernameAlreadyExistsException(): DomainException(
    "Username already exists."
    )
{
  public override string Code => ErrorCodes.UsernameAlreadyExists;
  public override int StatusCode => StatusCodes.Status409Conflict;
}

public class EmailAlreadyExistsException(): DomainException(
    "Email already exists."
    )
{
  public override string Code => ErrorCodes.EmailAlreadyExists;
  public override int StatusCode => StatusCodes.Status409Conflict;
}

public class InvalidCredentialsException(): DomainException(
    "Invalid username/email or password."
    )
{
  public override string Code => ErrorCodes.InvalidCredentials;
  public override int StatusCode => StatusCodes.Status401Unauthorized;
}