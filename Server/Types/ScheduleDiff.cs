namespace Server.Types;

public record ScheduleDiff(
    int Id,
    string WbsCode,
    string Name,
    DateTime OldStart,
    DateTime NewStart,
    DateTime OldEnd,
    DateTime NewEnd
);
