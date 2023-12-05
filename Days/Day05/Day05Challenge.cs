using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoC2023.Days.Day05
{
    internal class Day05Challenge : AoCChallengeBase
    {
        [System.Diagnostics.DebuggerDisplay("{SourceStart}-{SourceEnd} -> {DestinationStart}-{DestinationEnd}")]
        class Mapping
        {
            public long SourceStart { get; set; }
            public long SourceEnd { get; set; }
            public long DestinationStart { get; set; }
            public long DestinationEnd { get; set; }
        }

        public override int Day => 5;
        public override string Name => "If You Give A Seed A Fertilizer";

        protected override object? ExpectedTestResultPartOne => 35L;
        protected override object? ExpectedTestResultPartTwo => 46L;

        private List<long> _seeds = new();
        private List<Mapping> _seedToSoil = new();
        private List<Mapping> _soilToFertilizer = new();
        private List<Mapping> _fertilizerToWater = new();
        private List<Mapping> _waterToLight = new();
        private List<Mapping> _lightToTemperature = new();
        private List<Mapping> _temperatureToHumidity = new();
        private List<Mapping> _humidityToLocation = new();

        protected override object SolvePartOneInternal(string[] inputData)
        {
            this.ReadSeedMap(inputData);
            var lowestLocation = long.MaxValue;
            foreach(var seed in this._seeds)
            {
                var location = this.GetLocationForSeed(seed);
                lowestLocation = Math.Min(lowestLocation, location);
            }

            return lowestLocation;
        }

        protected override object SolvePartTwoInternal(string[] inputData)
        {
            this.ReadSeedMap(inputData);

            var maps = new List<List<Mapping>>()
            {
                this._seedToSoil.OrderBy(x => x.SourceStart).ToList(),
                this._soilToFertilizer.OrderBy(x => x.SourceStart).ToList(),
                this._fertilizerToWater.OrderBy(x => x.SourceStart).ToList(),
                this._waterToLight.OrderBy(x => x.SourceStart).ToList(),
                this._lightToTemperature.OrderBy(x => x.SourceStart).ToList(),
                this._temperatureToHumidity.OrderBy(x => x.SourceStart).ToList(),
                this._humidityToLocation.OrderBy(x => x.SourceStart).ToList()
            };

            // Debug: ensure there is no overlapping
            foreach(var map in maps)
            {
                // check that map entries don't overlap
                for(var i=0; i<map.Count-1; i++)
                {
                    if (map[i].SourceEnd >= map[i+1].SourceStart)
                    {
                        throw new Exception("Map entries overlap");
                    }
                }                
            }

            // fill in the gaps
            foreach(var map in maps)
            {
                if (map[0].SourceStart > 0)
                {
                    map.Insert(0, new Mapping
                    {
                        SourceStart = 0,
                        SourceEnd = map[0].SourceStart - 1,
                        DestinationStart = 0,
                        DestinationEnd = map[0].SourceStart - 1
                    });
                }

                for(var i=0; i<map.Count-1; i++)
                {
                    var gap = map[i+1].SourceStart - map[i].SourceEnd - 1;
                    if(gap > 0)
                    {
                        map.Insert(i+1, new Mapping
                        {
                            SourceStart = map[i].SourceEnd + 1,
                            SourceEnd = map[i+1].SourceStart - 1,
                            DestinationStart = map[i].SourceEnd + 1,
                            DestinationEnd = map[i + 1].SourceStart - 1
                        });
                    }
                }

                if (map[^1].SourceEnd < long.MaxValue)
                {
                    map.Add(new Mapping
                    {
                        SourceStart = map[^1].SourceEnd + 1,
                        SourceEnd = long.MaxValue,
                        DestinationStart = map[^1].SourceEnd + 1,
                        DestinationEnd = long.MaxValue
                    });
                }
            }

            // combine every two seeds to a tuple
            var seedTuples = new List<(long seedStart, long seedCount)>();
            for(var s=0; s<this._seeds.Count; s+=2)
            {
                seedTuples.Add((this._seeds[s], this._seeds[s+1]));
            }

            // find the lowest location for each seed tuple
            var lowestLocation = long.MaxValue;
            foreach(var seed in seedTuples)
            {
                var lowestSeedLocation = _calcLowestLocation(seed.seedStart, seed.seedStart + seed.seedCount - 1, 0);
                lowestLocation = Math.Min(lowestLocation, lowestSeedLocation);
            }

            return lowestLocation;

            long _calcLowestLocation(long srcFrom, long srcTo, int mapIndex)
            {
                var m = maps[mapIndex];
                var lowestLocation = long.MaxValue;
                foreach(var mapping in m)
                {
                    // get common range between mapping and seed
                    var commonFrom = Math.Max(srcFrom, mapping.SourceStart);
                    var commonTo = Math.Min(srcTo, mapping.SourceEnd);
                    if(commonFrom > commonTo) continue;

                    // if this is the last map, we're done
                    if(mapIndex == maps.Count - 1)
                    {
                        // calculate the location for the seed
                        var location = mapping.DestinationStart + (commonFrom - mapping.SourceStart);
                        lowestLocation = Math.Min(lowestLocation, location);
                    }
                    else
                    {
                        // otherwise, recurse
                        var location = _calcLowestLocation(mapping.DestinationStart + (commonFrom - mapping.SourceStart), mapping.DestinationStart + (commonTo - mapping.SourceStart), mapIndex + 1);
                        lowestLocation = Math.Min(lowestLocation, location);
                    }
                }

                return lowestLocation;
            }
        }

        private long GetLocationForSeed(long seed)
        {
            long _mapping(long value, List<Mapping> map)
            {
                foreach(var mapping in map)
                {
                    if(value >= mapping.SourceStart && value <= mapping.SourceEnd)
                    {
                        return (long)(mapping.DestinationStart + (value - mapping.SourceStart));
                    }
                }
                return value;
            }

            return
            _mapping(
                _mapping(
                    _mapping(
                        _mapping(
                            _mapping(
                                _mapping(
                                    _mapping(seed, this._seedToSoil),
                                    this._soilToFertilizer),
                                this._fertilizerToWater),
                            this._waterToLight),
                        this._lightToTemperature),
                    this._temperatureToHumidity),
                this._humidityToLocation);
        }

        private void ReadSeedMap(string[] inputData)
        {
            List<Mapping>? currentMap = null;

            foreach (var line in inputData)
            {
                if(line.StartsWith("seeds:"))
                {
                    this._seeds = line[6..].Split(' ').Where(s => !string.IsNullOrWhiteSpace(s)).Select(long.Parse).ToList();
                } else if(line.StartsWith("seed-to-soil"))
                {
                    currentMap = this._seedToSoil;
                    currentMap.Clear();
                } else if(line.StartsWith("soil-to-fertilizer"))
                {
                    currentMap = this._soilToFertilizer;
                    currentMap.Clear();
                }
                else if(line.StartsWith("fertilizer-to-water"))
                {
                    currentMap = this._fertilizerToWater;
                    currentMap.Clear();
                }
                else if(line.StartsWith("water-to-light"))
                {
                    currentMap = this._waterToLight;
                    currentMap.Clear();
                }
                else if(line.StartsWith("light-to-temperature"))
                {
                    currentMap = this._lightToTemperature;
                    currentMap.Clear();
                }
                else if(line.StartsWith("temperature-to-humidity"))
                {
                    currentMap = this._temperatureToHumidity;
                    currentMap.Clear();
                }
                else if(line.StartsWith("humidity-to-location"))
                {
                    currentMap = this._humidityToLocation;
                    currentMap.Clear();
                }
                else if(currentMap != null && !string.IsNullOrEmpty(line))
                {
                    var parts = line.Split(' ');
                    var destination = long.Parse(parts[0]);
                    var source = long.Parse(parts[1]);
                    var length = long.Parse(parts[2]);
                    currentMap.Add(new Mapping { 
                        SourceStart = source, 
                        SourceEnd = source + length - 1,
                        DestinationStart = destination,
                        DestinationEnd = destination + length - 1
                    });
                }
            }
        }
    }
}
