using TestPlatform.Services.Hash;

namespace TestPlatform.Services.Kdf;

public class KdfService : IKdfService
{
    private readonly IHashService _hashService;

    public KdfService(IHashService hashService)
    {
        _hashService = hashService;
    }

    public String GetDerivedKey(String password, String salt)
    {
        return _hashService.Hash(password + salt);
    }
}