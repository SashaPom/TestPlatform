namespace TestPlatform.Services.Kdf;

public interface IKdfService
{
    String GetDerivedKey(String password, String salt);
}