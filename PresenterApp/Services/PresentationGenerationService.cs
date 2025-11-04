// File: Services/PresentationGenerationService.cs
using PresenterApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace PresenterApp.Services
{
    public class PresentationGenerationService
    {
        private List<LyricLine> ParseLyrics(string rawLyrics)
        {
            var lines = new List<LyricLine>();
            if (string.IsNullOrWhiteSpace(rawLyrics)) return lines;

            string currentType = "Verse"; // Mặc định
            var rawLines = rawLyrics.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            foreach (var line in rawLines)
            {
                var trimmedLine = line.Trim();
                var match = Regex.Match(trimmedLine, @"^\s*\[(.*?)\]\s*$");

                if (match.Success)
                {
                    currentType = match.Groups[1].Value;
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    lines.Add(new LyricLine { Content = trimmedLine, Type = currentType });
                }
            }
            return lines;
        }

        public List<SlideDefinition> GenerateSlides(Song song, PresentationTemplate template)
        {
            var slideDefinitions = new List<SlideDefinition>();
            var lyricLines = ParseLyrics(song.Lyrics);

            var groupedLyrics = lyricLines
               .GroupBy(line => line.Type, (key, group) => new { Type = key, Lines = group.ToList() });

            foreach (var group in groupedLyrics)
            {
                var rule = template.Rules.FirstOrDefault(r => r.TargetType.Equals(group.Type, StringComparison.OrdinalIgnoreCase))
                         ?? template.DefaultRule;

                if (rule == null) continue;

                for (int i = 0; i < group.Lines.Count; i += rule.LinesPerSlide)
                {
                    var slideDef = new SlideDefinition
                    {
                        Title = $"{song.Title} ({group.Type})",
                        IsBold = rule.IsBold,
                        FontSize = rule.FontSize
                    };

                    var linesForSlide = group.Lines.Skip(i).Take(rule.LinesPerSlide).Select(l => l.Content);
                    slideDef.Lines.AddRange(linesForSlide);

                    slideDefinitions.Add(slideDef);
                }
            }

            return slideDefinitions;
        }
    }
}