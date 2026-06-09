namespace WheresWaldoApi.Exceptions;

public class ImageAlreadyExistsException(string imageName) : DomainException(
    $"Image '{imageName}' already exists."
    )
{
  public override string Code => ErrorCodes.ImageAlreadyExists;
  public override int StatusCode => StatusCodes.Status409Conflict;
}

public class ImageNotFoundException(Guid imageId): DomainException(
    $"Image '{imageId}' was not found."
    )
{
  public override string Code => ErrorCodes.ImageNotFound;
  public override int StatusCode => StatusCodes.Status404NotFound;
}