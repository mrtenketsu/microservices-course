using CommandService.Models;

namespace CommandService.Data;

public interface IPlatformRepo
{
    IEnumerable<Platform> GetAllPlatforms();
    void CratePlatform(Platform plat);
    bool PlatformExist(int platformId);
    bool ExternalPlatformExist(int externalPlatformId);

    bool SaveChanges();
}