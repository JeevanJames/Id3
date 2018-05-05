#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2012 Jeevan James
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
    public abstract class RenameAction
    {
        internal RenameAction()
        {
            Status = RenameStatus.Renamed;
        }

        /// <summary>
        ///     The directory containing the MP3 file.
        /// </summary>
        public string Directory { get; internal set; }

        /// <summary>
        ///     The original name of the MP3 file, with the extension.
        /// </summary>
        public string OriginalName { get; internal set; }

        /// <summary>
        ///     The new name of the MP3 file, with the extension.
        /// </summary>
        public string NewName { get; internal set; }

        /// <summary>
        ///     The status of the rename or rename suggestion.
        /// </summary>
        public RenameStatus Status { get; internal set; }

        /// <summary>
        ///     In the case of an error (Status is RenameStatus.Error), contains the error message
        /// </summary>
        public string ErrorMessage { get; internal set; }

        public override string ToString()
        {
            switch (Status)
            {
                case RenameStatus.CorrectlyNamed:
                    return $"Correctly named: {OriginalName}";
                case RenameStatus.Renamed:
                    return $"{OriginalName} ==> {NewName}";
                case RenameStatus.Cancelled:
                    return $"Cancelled: {OriginalName}";
                case RenameStatus.Error:
                    return $"Error: {OriginalName} [{ErrorMessage}]";
                default:
                    return base.ToString();
            }
        }
    }

    public abstract class RenameActions<T> : Collection<T>
        where T : RenameAction
    {
        protected RenameActions(IEnumerable<T> items)
        {
            foreach (T item in items)
                Add(item);
        }

        public IEnumerable<T> CorrectlyNamed => this.Where(action => action.Status == RenameStatus.CorrectlyNamed);

        public IEnumerable<T> Renamed => this.Where(action => action.Status == RenameStatus.Renamed);

        public IEnumerable<T> Errors => this.Where(action => action.Status == RenameStatus.Error);

        public IEnumerable<T> Cancelled => this.Where(action => action.Status == RenameStatus.Cancelled);
    }

    public sealed class RenameResult : RenameAction
    {
    }

    public sealed class RenameResults : RenameActions<RenameResult>
    {
        public RenameResults(IEnumerable<RenameResult> items)
            : base(items)
        {
        }
    }

    public sealed class RenameSuggestion : RenameAction
    {
    }

    public sealed class RenameSuggestions : RenameActions<RenameSuggestion>
    {
        public RenameSuggestions(IEnumerable<RenameSuggestion> items)
            : base(items)
        {
        }
    }
}
