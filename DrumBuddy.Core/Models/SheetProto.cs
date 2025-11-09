using ProtoBuf;

namespace DrumBuddy.Core.Models;
[ProtoContract]
public class MeasureProto
{
    [ProtoMember(1)]
    public List<RythmicGroupProto> Groups { get; set; } = new();
}

[ProtoContract]
public class RythmicGroupProto
{
    [ProtoMember(1)]
    public List<NoteGroupProto> NoteGroups { get; set; } = new();
}

[ProtoContract]
public class NoteGroupProto
{
    [ProtoMember(1)]
    public List<NoteProto> Notes { get; set; } = new();
}

[ProtoContract]
public class NoteProto
{
    [ProtoMember(1)]
    public int Drum { get; set; }

    [ProtoMember(2)]
    public int NoteValue { get; set; }
}