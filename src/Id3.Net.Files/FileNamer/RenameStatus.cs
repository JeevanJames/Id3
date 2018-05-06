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

namespace Id3.Files
{
    /// <summary>
    ///     Indicates the status of a file rename suggestion.
    /// </summary>
    public enum RenameStatus
    {
        /// <summary>
        ///     The file is already correctly-named.
        /// </summary>
        CorrectlyNamed,

        /// <summary>
        ///     The file was needs to be renamed.
        /// </summary>
        Rename,

        /// <summary>
        ///     The FileNamer operation is cancelled. Any suggestions processed so far will be returned.
        /// </summary>
        Cancelled,

        /// <summary>
        ///     An error occurred while trying to suggest a new name.
        /// </summary>
        Error
    }
}
