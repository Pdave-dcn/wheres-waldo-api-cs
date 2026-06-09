namespace WheresWaldoApi.Exceptions;

public abstract class DomainException(string message) : Exception(message)
{
  public abstract string  Code {get;}
  public abstract int StatusCode {get;}
}