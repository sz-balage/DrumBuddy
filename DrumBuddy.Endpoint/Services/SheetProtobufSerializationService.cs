using System.Collections.Immutable;
using DrumBuddy.Core.Enums;
using DrumBuddy.Core.Models;
using DrumBuddy.Endpoint.Models;
using ProtoBuf;

namespace DrumBuddy.Endpoint.Services;

public class SheetProtobufSerializationService
{
    public byte[] SerializeSheet(Sheet sheet)
    {
        var protoSheet = ConvertToProto(sheet);
        using var ms = new MemoryStream();
        Serializer.Serialize(ms, protoSheet);
        return ms.ToArray();
    }

    public Sheet DeserializeSheet(byte[] data)
    {
        using var ms = new MemoryStream(data);
        var protoSheet = Serializer.Deserialize<SheetProto>(ms);
        return ConvertFromProto(protoSheet);
    }

    private SheetProto ConvertToProto(Sheet sheet)
    {
        return new SheetProto
        {
            Name = sheet.Name,
            Description = sheet.Description,
            TempoValue = sheet.Tempo.Value,
            Measures = sheet.Measures
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
                .ToList()
        };
    }

    private Sheet ConvertFromProto(SheetProto proto)
    {
        var measures = proto.Measures
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
            .ToImmutableArray();

        return new Sheet(
            new Bpm(proto.TempoValue),
            measures,
            proto.Name,
            proto.Description
        );
    }
}