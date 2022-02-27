using CommandService.Models;

namespace CommandService.Data;

public interface ICommandRepo
{
    IEnumerable<Command> GetCommandsForPlatforms(int platformId);
    Command GetCommand(int platformId, int commandId);
    void CreateCommand(int platformId, Command command);

    bool SaveChanges();

}