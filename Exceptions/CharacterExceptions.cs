namespace WheresWaldoApi.Exceptions;

public class CharacterAlreadyExistsException(string characterType, Guid imageId) : DomainException(
    $"Character '{characterType}' already exists for image '{imageId}'."
    )
{
  public override string Code => ErrorCodes.CharacterAlreadyExists;
  public override int StatusCode => StatusCodes.Status409Conflict;
}

public class CharacterNotFoundException(Guid characterId) : DomainException(
    $"Character with ID '{characterId}' was not found."
    )
{
  public override string Code => ErrorCodes.CharacterNotFound;
  public override int StatusCode => StatusCodes.Status404NotFound;
}