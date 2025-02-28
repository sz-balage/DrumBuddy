using System.Xml.Linq;
using DrumBuddy.Core.Models;

namespace DrumBuddy.Core.Services
{
    public class MusicXmlConverter
    {
        public static string ConvertToMusicXml(List<Measure> measures)
        {
            var scorePartwise = new XElement("score-partwise",
                new XAttribute("version", "3.1"),
                new XElement("part-list",
                    new XElement("score-part",
                        new XAttribute("id", "P1"),
                        new XElement("part-name", "Music"))),
                new XElement("part",
                    new XAttribute("id", "P1"),
                    measures.Select((measure, index) => ConvertMeasureToXml(measure, index + 1))));

            var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), scorePartwise);
            return document.ToString();
        }

        private static XElement ConvertMeasureToXml(Measure measure, int measureNumber)
        {
            return new XElement("measure",
                new XAttribute("number", measureNumber),
                measure.Groups.Select(ConvertRythmicGroupToXml));
        }

        private static XElement ConvertRythmicGroupToXml(RythmicGroup group)
        {
            return new XElement("note",
                group.NoteGroups.Select(n => ConvertNoteToXml(n)));
        }

        private static XElement ConvertNoteToXml(NoteGroup note)
        {
            return new XElement("");
            //return new XElement("pitch",
            //    new XElement("step", note.Step),
            //    new XElement("octave", note.Octave),
            //    new XElement("duration", note.Duration),
            //    new XElement("type", note.Type));
        }
    }

}
