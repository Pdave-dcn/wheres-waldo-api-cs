namespace WheresWaldoApi.Exceptions;

public class ForbiddenException() : DomainException (
  "You do not have permission to perform this action."
)
{
  public override string Code => ErrorCodes.Forbidden;
  public override int StatusCode => StatusCodes.Status403Forbidden;
}