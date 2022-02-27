namespace CommandService.Data;

public abstract class BaseRepo
{
    protected AppDbContext Context { get; }

    protected BaseRepo(AppDbContext context)
    {
        Context = context;
    }
    
    public bool SaveChanges()
    {
        return (Context.SaveChanges() >= 0);
    }
}