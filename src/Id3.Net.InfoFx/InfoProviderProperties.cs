#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2019 Jeevan James
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

namespace Id3.InfoFx
{
    /// <summary>
    ///     Details of an info provider.
    /// </summary>
    public sealed class InfoProviderProperties
    {
        public InfoProviderProperties(string name, string url = null, string registrationUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Provider must have a name.", nameof(name));
            Name = name;
            Url = url;
            RegistrationUrl = registrationUrl;
        }

        /// <summary>
        ///     Descriptive name for the info provider.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     URL containing details of the info provider. Could be a home page.
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     URL to a registration page, if the info provider service needs credentials to be used.
        /// </summary>
        public string RegistrationUrl { get; }

        /// <summary>
        ///     Types of ID3 frames that are required for the info provider.
        /// </summary>
        public FrameTypes RequiredInputs { get; } = new FrameTypes();

        /// <summary>
        ///     Types of ID3 frames that can be used but are not required for the info provider.
        /// </summary>
        public FrameTypes OptionalInputs { get; } = new FrameTypes();

        /// <summary>
        ///     Types of ID3 frames that are output by the info provider.
        /// </summary>
        public FrameTypes AvailableOutputs { get; } = new FrameTypes();

        /// <summary>
        ///     Indicates whether the info provider requires the tag instance to work.
        /// </summary>
        public bool CanOmitTag { get; set; }

        /// <summary>
        ///     Indicates whether the info provider needs a MP3 file name to work.
        /// </summary>
        public bool RequiresFilename { get; set; }

        /// <summary>
        ///     Indicates whether the info provider needs a MP3 stream to work.
        /// </summary>
        public bool RequiresStream { get; set; }
    }
}
