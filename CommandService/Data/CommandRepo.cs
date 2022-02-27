using CommandService.Models;

namespace CommandService.Data;

public class CommandRepo : BaseRepo, ICommandRepo
{
    public CommandRepo(AppDbContext context) : base(context)
    {
        
    }
    
    public IEnumerable<Command> GetCommandsForPlatforms(int platformId)
    {
        return Context.Commands
            .Where(c => c.PlatformId == platformId)
            .OrderBy(c => c.Platform.Name);
    }

    public Command GetCommand(int platformId, int commandId)
    {
        return Context.Commands.FirstOrDefault(
            c => c.PlatformId == platformId && c.Id == commandId);
    }

    public void CreateCommand(int platformId, Command command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        command.PlatformId = platformId;
        Context.Commands.Add(command);
    }
}