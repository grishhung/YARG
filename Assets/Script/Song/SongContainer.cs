﻿using System.Collections.Generic;
using YARG.Core.Song.Cache;
using YARG.Core.Song;
using System;
using YARG.Helpers.Extensions;

namespace YARG.Song
{
    public readonly struct SongCategory
    {
        public readonly string Category;
        public readonly List<SongMetadata> Songs;

        public SongCategory(string category, List<SongMetadata> songs)
        {
            Category = category;
            Songs = songs;
        }

        public void Deconstruct(out string category, out List<SongMetadata> songs)
        {
            category = Category;
            songs = Songs;
        }
    }

    public class SongContainer
    {
        private readonly SongCache _songCache = new();
        private readonly List<SongMetadata> _songs = new();

        private readonly List<SongCategory> _sortTitles = new();
        private readonly List<SongCategory> _sortArtists = new();
        private readonly List<SongCategory> _sortAlbums = new();
        private readonly List<SongCategory> _sortGenres = new();
        private readonly List<SongCategory> _sortYears = new();
        private readonly List<SongCategory> _sortCharters = new();
        private readonly List<SongCategory> _sortPlaylists = new();
        private readonly List<SongCategory> _sortSources = new();
        private readonly List<SongCategory> _sortArtistAlbums = new();
        private readonly List<SongCategory> _sortSongLengths = new();
        private readonly List<SongCategory> _sortDatesAdded = new();
        private readonly List<SongCategory> _sortInstruments = new();

        public IReadOnlyDictionary<string, List<SongMetadata>> Titles => _songCache.Titles;
        public IReadOnlyDictionary<string, List<SongMetadata>> Years => _songCache.Years;
        public IReadOnlyDictionary<string, List<SongMetadata>> ArtistAlbums => _songCache.ArtistAlbums;
        public IReadOnlyDictionary<string, List<SongMetadata>> SongLengths => _songCache.SongLengths;
        public IReadOnlyDictionary<string, List<SongMetadata>> Instruments => _songCache.Instruments;
        public IReadOnlyDictionary<DateTime, List<SongMetadata>> AddedDates => _songCache.DatesAdded;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Artists => _songCache.Artists;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Albums => _songCache.Albums;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Genres => _songCache.Genres;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Charters => _songCache.Charters;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Playlists => _songCache.Playlists;
        public IReadOnlyDictionary<SortString, List<SongMetadata>> Sources => _songCache.Sources;

        public int Count => _songs.Count;
        public IReadOnlyDictionary<HashWrapper, List<SongMetadata>> SongsByHash => _songCache.Entries;
        public IReadOnlyList<SongMetadata> Songs => _songs;

        public SongContainer() { }

        public SongContainer(SongCache cache)
        {
            _songCache = cache;
            foreach (var node in cache.Entries)
                _songs.AddRange(node.Value);
            _songs.TrimExcess();

            _sortArtists = Convert(cache.Artists, SongAttribute.Artist);
            _sortAlbums = Convert(cache.Albums, SongAttribute.Album);
            _sortGenres = Convert(cache.Genres, SongAttribute.Genre);
            _sortCharters = Convert(cache.Charters, SongAttribute.Charter);
            _sortPlaylists = Convert(cache.Playlists, SongAttribute.Playlist);
            _sortSources = Convert(cache.Sources, SongAttribute.Source);

            _sortTitles = Cast(cache.Titles);
            _sortYears = Cast(cache.Years);
            _sortArtistAlbums = Cast(cache.ArtistAlbums);
            _sortSongLengths = Cast(cache.SongLengths);
            _sortInstruments = Cast(cache.Instruments);

            _sortDatesAdded = new();
            foreach (var node in cache.DatesAdded)
            {
                _sortDatesAdded.Add(new(node.Key.ToLongDateString(), node.Value));
            }

            static List<SongCategory> Convert(SortedDictionary<SortString, List<SongMetadata>> list, SongAttribute attribute)
            {
                List<SongCategory> sections = new(list.Count);
                foreach (var node in list)
                {
                    string key = node.Key;
                    if (attribute == SongAttribute.Genre && key.Length > 0 && char.IsLower(key[0]))
                    {
                        key = char.ToUpperInvariant(key[0]).ToString();
                        if (node.Key.Length > 1)
                            key += node.Key.Str[1..];
                    }
                    sections.Add(new(key, node.Value));
                }
                return sections;
            }

            static List<SongCategory> Cast(SortedDictionary<string, List<SongMetadata>> list)
            {
                List<SongCategory> sections = new(list.Count);
                foreach (var section in list)
                {
                    sections.Add(new SongCategory(section.Key, section.Value));
                }
                return sections;
            }
        }

        public IReadOnlyList<SongCategory> GetSortedSongList(SongAttribute sort)
        {
            return sort switch
            {
                SongAttribute.Name => _sortTitles,
                SongAttribute.Artist => _sortArtists,
                SongAttribute.Album => _sortAlbums,
                SongAttribute.Genre => _sortGenres,
                SongAttribute.Year => _sortYears,
                SongAttribute.Charter => _sortCharters,
                SongAttribute.Playlist => _sortPlaylists,
                SongAttribute.Source => _sortSources,
                SongAttribute.Artist_Album => _sortArtistAlbums,
                SongAttribute.SongLength => _sortSongLengths,
                SongAttribute.DateAdded => _sortDatesAdded,
                SongAttribute.Instrument => _sortInstruments,
                _ => throw new Exception("stoopid"),
            };
        }

        public SongMetadata GetRandomSong()
        {
            return _songs.Pick();
        }
    }
}