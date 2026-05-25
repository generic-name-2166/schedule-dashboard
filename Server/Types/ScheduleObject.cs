namespace Server.Types;

public record ScheduleObject(
    int Id,
    int Level,
    string WbsCode,
    string Code,
    string Name,
    DateTime? Start,
    DateTime? End,
    /// index in the sorted array for a specific date
    int Index,
    /// index of the last descendant + 1 (for leaves: current index + 1)
    int DescendantEndIdx
);
