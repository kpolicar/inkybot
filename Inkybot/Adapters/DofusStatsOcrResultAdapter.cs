using System;
using System.Linq;
using System.Text.RegularExpressions;
using Inkybot.Dofus;
using Inkybot.Dofus.Repositories;
using Inkybot.Domain;
using Inkybot.Exceptions;

namespace Inkybot.Adapters
{
    public class DofusStatsOcrResultAdapter : DofusOcrResultAdapter
    {
        private readonly string[] statLines;

        public DofusStatsOcrResultAdapter(string[] statLines) {
            this.statLines = statLines;
        }

        public ItemStatRepository ToItemStats() {
            return new ItemStatRepository(statLines.Select(statLine => {
                var (segmentedStatLine, isWeaponEffectStatLine) = SegmentItemStatLine(statLine);

                try {
                    return StatLineToItemStat(segmentedStatLine.Groups, isWeaponEffectStatLine);
                } catch (CouldNotSegmentStatLineException) {
                    throw new CouldNotSegmentStatLineException($"Error occured trying to segment stat line: {statLine}");
                }
            }).ToArray());
        }

        private (int? min, int? max, int? value)
            GetDataFromScanResult(GroupCollection statLineSegments, bool parseMin=true, bool parseMax=true, bool parseValue=true) {
            try {
                var (min, max, value) =
                    (statLineSegments[1].Value, statLineSegments[2].Value, statLineSegments[3].Value);
                
                var parsedMin = parseMin
                    ? (int?)(min != "-" ? int.Parse(min) : 0)
                    : null;
                var parsedMax = parseMax
                    ? (int?)(max != "-" ? int.Parse(max) : 0)
                    : null;
                var parsedValue = parseValue
                    ? (int?) (value.Length > 0 ? int.Parse(value) : 0)
                    : null;

                return (parsedMin, parsedMax, parsedValue);
            } catch (Exception exception) {
                throw new CouldNotResolveStatValueException($"Error occured resolving value for stat {statLineSegments[4].Value}", exception);
            }
        }
        
        private ItemStat StatLineToItemStat(GroupCollection statLineSegments, bool isWeaponEffectStatLine) {
            if (statLineSegments.Count != 5)
                throw new CouldNotSegmentStatLineException($"Error occured trying to segment stat line");
            
            var name = statLineSegments[4].Value;
            var (min, max, value) = GetDataFromScanResult(statLineSegments, parseValue: !isWeaponEffectStatLine);
            
            if (isWeaponEffectStatLine) {
                return new ItemStat(new Stat("Weapon " + name), 0, min!.Value, max!.Value);
            }
            
            var stat = GetStatFromName(name);
            return new ItemStat(stat, value!.Value, min!.Value, max!.Value);
        }

        private (Match segmentedStatLine, bool isWeaponEffectStatLine) SegmentItemStatLine(string statLine) {
            var segments = Regex.Match(statLine, Properties.Regex.ItemStatLinePattern);
            if (!segments.Success)
                return (Regex.Match(statLine, Properties.Regex.ItemWeaponEffectStatLinePattern), true);
            
            return (segments, false);
        }
    }
}
