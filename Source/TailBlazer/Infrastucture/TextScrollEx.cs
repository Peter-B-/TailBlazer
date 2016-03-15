using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Formatting;

namespace TailBlazer.Infrastucture
{
    public static class TextScrollEx
    {
        public static string Virtualise(this string source, TextScrollInfo scroll)
        {
            // var items = source.AsArray();

            if (scroll == null || scroll.TotalChars == 0)
                return source;

            // var list = new List<DisplayText>(items.Length);
            // int lastIndex = scroll.FirstIndex + scroll.TotalChars;

            return new String(source.Skip(scroll.FirstIndex).Take(scroll.TotalChars).ToArray());
        }

        public static IEnumerable<DisplayText> Virtualise(this IEnumerable<DisplayText> source, TextScrollInfo scroll)
        {
            var items = source.AsArray();

            if (scroll == null || scroll.TotalChars == 0)
                return items;
            
            // var list = new List<DisplayText>(items.Length);
            int lastIndex = scroll.FirstIndex + scroll.TotalChars;

            var displayBounds = items.Aggregate(new List<DisplayWithIndex>(), (state, latest) =>
            {
            
                if (state.Count == 0)
                {
                    state.Add( new DisplayWithIndex(latest, 0));
                }
                else
                {
                    var last = state.Last();
                    state.Add(new DisplayWithIndex(latest, last.StartIndex + last.Text.Length));
                }

                return state;
            }).ToArray();

            var result = displayBounds
                .Select(item =>
                {
                    if (item.Inbounds(scroll.FirstIndex, lastIndex))
                    {
                        //clip it and yield
                        return item.Clip(scroll.FirstIndex, lastIndex);
                    }
                    return null;
                }).Where(item => item != null)
                .ToArray();

            return result;
        }

        private class DisplayWithIndex
        {
            public DisplayText Text { get;  }
            public int StartIndex { get;  }

            public int EndIndex => StartIndex + Text.Length;

            public DisplayWithIndex(DisplayText text, int startIndex)
            {
                Text = text;
                StartIndex = startIndex;
            }

            public bool Inbounds(int start, int end)
            {
                return start <= EndIndex && end >= StartIndex;
            }

            public  DisplayText Clip(int start, int lastIndex)
            {
                var clippedStart = Math.Max(start - StartIndex, 0);
                int maxLength = lastIndex - StartIndex;

                //int clippedLength = (lastIndex < EndIndex)
                //    ? maxLength
                //    : Math.Min(maxLength, Text.Length - clippedStart);

                var clippedLength=  Math.Min(maxLength, Text.Length - clippedStart);

                var clipped = Text.Text.Substring(clippedStart, clippedLength);
                return new DisplayText(Text, clipped);
            }

            public override string ToString()
            {
                return $"{Text}, {StartIndex}->{EndIndex}";
            }
        }

        public static DisplayText Clip(this DisplayText source, int start, int length)
        {
            var clipped = source.Text.Substring(start, Math.Min(length, source.Text.Length));
            return new DisplayText(source, clipped);
        }
    }
}