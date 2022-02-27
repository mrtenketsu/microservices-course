using CommandService.Models;

namespace CommandService.Data;

public class PlatformRepo : BaseRepo, IPlatformRepo
{
    public PlatformRepo(AppDbContext context) : base(context)
    {
        
    }
    
    public IEnumerable<Platform> GetAllPlatforms()
    {
        return Context.Platforms;
    }

    public void CratePlatform(Platform plat)
    {
        if (plat == null)
            throw new ArgumentNullException(nameof(plat));
        
        Context.Platforms.Add(plat);
    }

    public bool PlatformExist(int platformId)
    {
        return Context.Platforms.Any(p => p.Id == platformId);
    }
}