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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Id3.Files
{
    /// <summary>
    ///     Represents the result of a rename or a rename suggestion request.
    /// </summary>
    public sealed class RenameSuggestion
    {
        internal RenameSuggestion(string directory, string originalName, RenameStatus status)
        {
            Directory = directory;
            OriginalName = originalName;
            Status = status;
        }

        internal RenameSuggestion(string directory, string originalName, string errorMessage)
        {
            Directory = directory;
            OriginalName = originalName;
            Status = RenameStatus.Error;
            ErrorMessage = errorMessage;
        }

        internal RenameSuggestion(string directory, string originalName, string newName, RenameStatus status)
        {
            Directory = directory;
            OriginalName = originalName;
            NewName = newName;
            Status = status;
        }

        /// <summary>
        ///     Gets the directory containing the MP3 file.
        /// </summary>
        public string Directory { get; }

        /// <summary>
        ///     Gets the original name of the MP3 file, with the extension.
        /// </summary>
        public string OriginalName { get; }

        /// <summary>
        ///     Gets the new name of the MP3 file, with the extension.
        /// </summary>
        public string NewName { get; }

        /// <summary>
        ///     Gets the status of the rename suggestion.
        /// </summary>
        public RenameStatus Status { get; }

        /// <summary>
        ///     Gets in the case of an error (Status is <see cref="RenameStatus.Error"/>), contains
        ///     the error message.
        /// </summary>
        public string ErrorMessage { get; }

        public override string ToString()
        {
            return Status switch
            {
                RenameStatus.CorrectlyNamed => $"Correctly named: {OriginalName}",
                RenameStatus.Rename => $"{OriginalName} ==> {NewName}",
                RenameStatus.Cancelled => $"Cancelled: {OriginalName}",
                RenameStatus.Error => $"Error: {OriginalName} [{ErrorMessage}]",
                _ => base.ToString(),
            };
        }
    }

    public sealed class RenameSuggestions : Collection<RenameSuggestion>
    {
        internal RenameSuggestions(IEnumerable<RenameSuggestion> items)
        {
            foreach (RenameSuggestion item in items)
                Add(item);
        }

        public IEnumerable<RenameSuggestion> CorrectlyNamed => this.Where(action => action.Status == RenameStatus.CorrectlyNamed);

        public IEnumerable<RenameSuggestion> Renamed => this.Where(action => action.Status == RenameStatus.Rename);

        public IEnumerable<RenameSuggestion> Errors => this.Where(action => action.Status == RenameStatus.Error);

        public IEnumerable<RenameSuggestion> Cancelled => this.Where(action => action.Status == RenameStatus.Cancelled);
    }
}
