using System;
using System.Collections.Generic;
using System.Text;

namespace FootbalSim.Core.News
{
    public readonly struct NewsEvent
    {
        public readonly string Tag;
        public readonly int Severity;      // 0 = filler, 1 = notable, 2 = headline
        public readonly string Subject;    // player name
        public readonly string Club;
        public readonly int Number;

        public NewsEvent(string tag, int severity, string subject, string club = null, int number = 0)
        {
            Tag = tag;
            Severity = severity;
            Subject = subject;
            Club = club;
            Number = number;
        }
    }

    /// <summary>
    /// GDD §4.6 — turns tagged events into TV/newspaper lines. One source, several surfaces.
    /// </summary>
    public sealed class NewsComposer
    {
        public const int DefaultMaxLines = 4;

        private readonly Dictionary<string, string[]> _templates;

        public NewsComposer() : this(DefaultTemplates()) { }

        public NewsComposer(Dictionary<string, string[]> templates)
        {
            _templates = templates ?? throw new ArgumentNullException(nameof(templates));
        }

        public IReadOnlyList<string> Compose(IEnumerable<NewsEvent> events, int maxLines = DefaultMaxLines)
        {
            var result = new List<string>(maxLines);
            if (events == null) return result;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var ordered = new List<NewsEvent>();
            foreach (NewsEvent evt in events)
            {
                if (evt.Severity <= 0) continue;
                if (!seen.Add(evt.Tag)) continue;   // one line per tag per bulletin
                ordered.Add(evt);
            }

            ordered.Sort((a, b) => b.Severity.CompareTo(a.Severity));

            for (int i = 0; i < ordered.Count && result.Count < maxLines; i++)
            {
                string line = Render(ordered[i]);
                if (!string.IsNullOrEmpty(line)) result.Add(line);
            }

            return result;
        }

        public string Render(NewsEvent evt)
        {
            if (!_templates.TryGetValue(evt.Tag, out string[] variants) || variants.Length == 0)
                return null;

            int index = StableHash(evt.Tag) % variants.Length;
            string template = variants[index];

            var sb = new StringBuilder(template.Length + 32);
            sb.Append(template);
            sb.Replace("{player}", string.IsNullOrEmpty(evt.Subject) ? "The player" : evt.Subject);
            sb.Replace("{club}", string.IsNullOrEmpty(evt.Club) ? "the club" : evt.Club);
            sb.Replace("{value}", evt.Number.ToString());
            return sb.ToString();
        }

        /// <summary>FNV-1a: stable across processes, unlike string.GetHashCode.</summary>
        private static int StableHash(string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                for (int i = 0; i < value.Length; i++)
                {
                    hash ^= value[i];
                    hash *= 16777619u;
                }
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        public static Dictionary<string, string[]> DefaultTemplates() =>
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                { "match.rating.high", new[]
                    {
                        "{player} put in an outstanding performance in today's match.",
                        "{player} was the difference in today's match.",
                        "A display to remember from {player} this afternoon."
                    }
                },
                { "match.rating.low", new[]
                    {
                        "{player} struggled as {club} fell short.",
                        "A difficult afternoon for {player}.",
                        "Questions asked of {player} after today's display."
                    }
                },
                { "bar.binge", new[]
                    {
                        "{player} was spotted leaving the bar in the early hours.",
                        "A long night out for {player}, according to onlookers."
                    }
                },
                { "club.missed", new[]
                    {
                        "Discipline questions at {club} after {player} missed a session.",
                        "{club} left short-handed as {player} failed to report."
                    }
                },
                { "purchase.car", new[]
                    {
                        "{player} arrived in style today, behind the wheel of a new car.",
                        "A new set of wheels for {player}."
                    }
                },
                { "transfer.offer", new[]
                    {
                        "{club} have made an approach for {player}.",
                        "Interest building: {club} enquire about {player}."
                    }
                },
                { "transfer.accepted", new[]
                    {
                        "Done deal: {player} completes a move to {club}.",
                        "{player} signs for {club}."
                    }
                },
                { "transfer.rejected", new[]
                    {
                        "{club} turned down an approach from {player}.",
                        "No deal: {club} rebuff {player}."
                    }
                },
                { "training.milestone", new[]
                    {
                        "{player}'s {value} is drawing attention in training.",
                        "Coaches are talking about {player}'s improvement."
                    }
                }
            };
    }
}
