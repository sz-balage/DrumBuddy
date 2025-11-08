using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Endpoint.Models;
using ProtoBuf;

namespace DrumBuddy.Endpoint.Services;

public class SheetProtobufSerializationService
{
    public byte[] SerializeSheet(ImmutableArray<Measure> measures)
    {
        var protoSheet = ConvertToProto(measures);
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, protoSheet);
        return ms.ToArray();
    }

    public ImmutableArray<Measure> DeserializeSheet(byte[] data)
    {
        using var ms = new MemoryStream(data);
        var protoMeasures = Serializer.Deserialize<ImmutableArray<MeasureProto>>(ms);
        return ConvertFromProto(protoMeasures);
    }

    private ImmutableArray<MeasureProto> ConvertToProto(ImmutableArray<Measure> measures) =>
    [
        ..measures
            .Select(m => new MeasureProto
            {
                Groups = m.Groups
                    .Select(g => new RythmicGroupProto
                    {
                        NoteGroups = g.NoteGroups
                            .Select(ng => new NoteGroupProto
                            {
                                Notes = ng
                                    .Select(n => new NoteProto
                                    {
                                        Drum = (int)n.Drum,
                                        NoteValue = (int)n.Value
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
    ];

    private ImmutableArray<Measure> ConvertFromProto(ImmutableArray<MeasureProto> protoMeasures) =>
    [
        ..protoMeasures
            .Select(m => new Measure(
                m.Groups
                    .Select(g => new RythmicGroup(
                        [
                            ..g.NoteGroups.Select(ng =>
                                new NoteGroup(ng.Notes.Select(n => new Note((Drum)n.Drum, (NoteValue)n.NoteValue))))
                        ]
                    ))
                    .ToList()
            ))
    ];

}