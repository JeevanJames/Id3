#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2018 Jeevan James
All rights reserved.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Id3.Files.Resources;
using Id3.Frames;
using JetBrains.Annotations;

namespace Id3.Files
{
    /// <summary>
    ///     Provides naming suggestions for MP3 files based on the ID3 metadata.
    /// </summary>
    public sealed class FileNamer
    {
        [NotNull]
        private readonly List<string> _patterns;

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Id3.Files.FileNamer" /> class with a single file naming pattern.
        /// </summary>
        /// <param name="pattern">The file naming pattern to use.</param>
        public FileNamer([NotNull] string pattern) : this(new[] { pattern })
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Id3.Files.FileNamer" /> class with one or more file naming patterns,
        ///     specified in order of priority.
        /// </summary>
        /// <param name="patterns">The file naming patterns to use, in order of priority.</param>
        public FileNamer([NotNull] params string[] patterns) : this((IEnumerable<string>)patterns)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="FileNamer" /> class with one or more file naming patterns, specified
        ///     in order of priority.
        /// </summary>
        /// <param name="patterns">The file naming patterns to use, in order of priority.</param>
        /// <exception cref="ArgumentNullException">Thrown if the specified patterns parameter is null</exception>
        /// <exception cref="ArgumentException">Thrown if no patterns are specified or if there is a null or empty pattern</exception>
        public FileNamer([NotNull] IEnumerable<string> patterns)
        {
            if (patterns == null)
                throw new ArgumentNullException(nameof(patterns));
            _patterns = patterns.ToList();
            if (_patterns.Count == 0)
                throw new ArgumentException(FileNamerMessages.MissingPatterns, nameof(patterns));
            if (_patterns.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException(FileNamerMessages.FoundNullOrEmptyPatterns, nameof(patterns));
            ValidatePatterns(_patterns);
        }

        /// <summary>
        ///     Ensures that all placeholders in the pattern refer to existing tag frame properties. Then it attempts to mapping
        ///     dictionary between the frame name and the corresponding frame property, so that multiple renames can be done fast.
        /// </summary>
        /// <param name="patterns">The pattern to validate</param>
        /// <returns>A mapping of placeholder name to property info of the correspoding frame in <see cref="Id3Tag" /></returns>
        /// <exception cref="ArgumentException">Thrown if any pattern contains invalid placeholders.</exception>
        private static void ValidatePatterns(IEnumerable<string> patterns)
        {
            // Get all distinct placeholder names in all the patterns and check whether any of them
            // do not appear as a key in the _mappings dictionary. If so, throw an exception.
            string[] invalidPlaceholders = patterns
                .SelectMany(pattern => {
                    MatchCollection matches = FramePlaceholderPattern.Matches(pattern);
                    if (matches.Count == 0)
                        throw new ArgumentException(string.Format(FileNamerMessages.MissingPlaceholdersInPattern,
                            pattern));
                    return matches.Cast<Match>().Select(m => m.Groups[1].Value);
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(ph => !_mapping.ContainsKey(ph))
                .ToArray();
            if (invalidPlaceholders.Length <= 0)
                return;

            //Build detailed exception and throw it.
            string invalidPlaceholderNames = string.Join(", ", invalidPlaceholders);
            string allowedPlaceholderNames = string.Join(", ", _allowedFrames);
            string exceptionMessage = "The following placeholders are not allowed in the file naming patterns:" + Environment.NewLine +
                Environment.NewLine +
                invalidPlaceholderNames + Environment.NewLine +
                Environment.NewLine +
                "Only these placeholders are allowed:" + Environment.NewLine +
                Environment.NewLine +
                allowedPlaceholderNames;
            throw new ArgumentException(exceptionMessage, nameof(patterns));
        }

        /// <summary>
        ///     Gets naming suggestions for the MP3 files at the specified file paths.
        /// </summary>
        /// <param name="filePaths">File paths of the MP3 files</param>
        /// <returns>Collection of renaming suggestions for the specified files</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RenameSuggestions GetSuggestions(IEnumerable<string> filePaths)
        {
            if (filePaths == null)
                throw new ArgumentNullException(nameof(filePaths));
            return new RenameSuggestions(GetRenameSuggestions(filePaths));
        }

        /// <summary>
        ///     Gets naming suggestions for the MP3 files in the specified directory.
        /// </summary>
        /// <param name="directory">The directory where the MP3 files are located</param>
        /// <param name="fileMask">File mask to use when searching for the files. Defaults tp *.mp3.</param>
        /// <param name="searchOption">
        ///     Indicates whether to search just the specified directory or recursively search through its
        ///     subdirectories as well.
        /// </param>
        /// <returns>Collection of renaming suggestions for the matching files found in the specified directory</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">Thrown if the specified directory does not exist.</exception>
        public RenameSuggestions GetSuggestions(string directory, string fileMask = "*.mp3",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (directory == null)
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory))
                throw new ArgumentException(string.Format(FileNamerMessages.MissingDirectory, directory),
                    nameof(directory));
            if (string.IsNullOrWhiteSpace(fileMask))
                fileMask = "*.mp3";

            IEnumerable<string> files = Directory.EnumerateFiles(directory, fileMask, searchOption);
            return new RenameSuggestions(GetRenameSuggestions(files));
        }

        /// <summary>
        ///     Raised whenever the required frame data is missing for a MP3 file.
        /// </summary>
        public event EventHandler<ResolveMissingDataEventArgs> ResolveMissingData;

        /// <summary>
        ///     Raised before a file name is about to be suggested for an MP3 file. Provides the ability to override the suggested
        ///     name operation or to cancel it.
        /// </summary>
        public event EventHandler<RenamingEventArgs> Renaming;

        private IEnumerable<RenameSuggestion> GetRenameSuggestions(IEnumerable<string> filePaths)
        {
            foreach (string filePath in filePaths)
                yield return GetRenameSuggestion(filePath);
        }

        private RenameSuggestion GetRenameSuggestion(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return new RenameSuggestion(filePath, filePath, FileNamerMessages.InvalidFilePath);

            if (!File.Exists(filePath))
                return new RenameSuggestion(filePath, filePath, FileNamerMessages.MissingFile);

            string directory = Path.GetDirectoryName(filePath);
            string originalName = Path.GetFileName(filePath);

            using (var mp3 = new Mp3(filePath))
            {
                Id3Tag tag = mp3.GetTag(Id3Version.V23);
                if (tag == null)
                    return new RenameSuggestion(directory, originalName, FileNamerMessages.MissingId3v23TagInFile);

                //TODO: Get ID3v1 tag as well and merge with the v2 tag

                string newName = GetNewName(tag, originalName, out string missingFrameName);

                if (missingFrameName != null)
                    return new RenameSuggestion(directory, originalName,
                        string.Format(FileNamerMessages.MissingDataForFrame, missingFrameName));

                newName += ".mp3";
                RenamingEventArgs renamingEventResult = FireRenamingEvent(tag, originalName, newName);
                if (renamingEventResult.Cancel)
                    return new RenameSuggestion(directory, originalName, RenameStatus.Cancelled);

                newName = renamingEventResult.NewName;

                RenameStatus status = originalName.Equals(newName, StringComparison.Ordinal)
                    ? RenameStatus.CorrectlyNamed : RenameStatus.Rename;
                return new RenameSuggestion(directory, originalName, newName, status);
            }
        }

        private string GetNewName(Id3Tag tag, string originalName, out string missingFrameName)
        {
            missingFrameName = null;
            string missingFrame = null;

            //Make two passes of the patterns.
            //In the first pass, we try to find the perfect match without resorting to the
            //ResolveMissingData event.
            //If we still don't have a match, in the second pass, we use the ResolveMissingData event.
            for (var i = 0; i < 2; i++)
            {
                foreach (string pattern in _patterns)
                {
                    var hasMissingFrames = false;

                    int iteration = i;
                    string newName = FramePlaceholderPattern.Replace(pattern, match => {
                        //If this pattern already has missing frames, don't proces anything
                        if (hasMissingFrames)
                            return string.Empty;

                        string frameName = match.Groups[1].Value;
                        PropertyInfo frameProperty = _mapping[frameName];

                        //Because all frame properties in Id3Tag are lazily-loaded, this will never be null
                        var frame = (Id3Frame)frameProperty.GetValue(tag, null);

                        if (frame.IsAssigned)
                            return frame.ToString();

                        if (iteration == 1)
                        {
                            string frameValue = FireResolveMissingDataEvent(tag, frame, originalName);
                            if (!string.IsNullOrWhiteSpace(frameValue))
                                return frameValue;
                        }
                        hasMissingFrames = true;
                        missingFrame = frameName;
                        return string.Empty;
                    });

                    if (!hasMissingFrames)
                        return newName;
                }
            }
            
            missingFrameName = missingFrame;
            return null;
        }
        private static readonly Regex FramePlaceholderPattern = new Regex(@"{(\w+)}");

        private string FireResolveMissingDataEvent(Id3Tag tag, Id3Frame frame, string sourceName)
        {
            EventHandler<ResolveMissingDataEventArgs> resolveMissingData = ResolveMissingData;
            if (resolveMissingData == null)
                return null;
            var args = new ResolveMissingDataEventArgs(tag, frame, sourceName);
            resolveMissingData(this, args);
            return args.Value;
        }

        private RenamingEventArgs FireRenamingEvent(Id3Tag tag, string oldName, string newName)
        {
            EventHandler<RenamingEventArgs> renaming = Renaming;
            var args = new RenamingEventArgs(tag, oldName) {
                NewName = newName
            };
            renaming?.Invoke(this, args);
            return args;
        }

        /// <summary>
        ///     Mappings of <see cref="Id3Tag" /> property names to their <see cref="PropertyInfo" /> data for all allowed frame
        ///     properties that can be used as placeholders in the file naming patterns.
        ///     <para />
        ///     Allows for quick lookups of the property data.
        /// </summary>
        private static readonly Dictionary<string, PropertyInfo> _mapping;

        /// <summary>
        ///     List of <see cref="Id3Tag" /> frame properties that can be used as placeholders in the file naming patterns.
        ///     <para />
        ///     Typically, these would be textual frames that have single line values of reasonable length.
        /// </summary>
        private static readonly List<string> _allowedFrames = new List<string> {
            "Album",
            "Artists",
            "Band",
            "BeatsPerMinute",
            "Composers",
            "Conductor",
            "ContentGroupDescription",
            "Encoder",
            "EncodingSettings",
            "FileOwner",
            "FileType",
            "Genre",
            "Lyricists",
            "Publisher",
            "Subtitle",
            "Title",
            "Track",
            "Year"
        };

        /// <summary>
        ///     One-time initialization of the <see cref="_mapping" /> dictionary.
        /// </summary>
        static FileNamer()
        {
            Type tagType = typeof(Id3Tag);
            Type baseFrameType = typeof(Id3Frame);

            _mapping = new Dictionary<string, PropertyInfo>(_allowedFrames.Count, StringComparer.OrdinalIgnoreCase);
            foreach (string frame in _allowedFrames)
            {
                PropertyInfo property = tagType.GetProperty(frame);
                if (property == null)
                    throw new Exception($"No property named {frame} exists on Id3Tag. Please check the whitelist.");
                if (!property.PropertyType.IsSubclassOf(baseFrameType))
                    throw new Exception($"Property Id3Tag.{frame} is not a frame type. Please check the whitelist.");
                _mapping.Add(frame, property);
            }
        }
    }
}
