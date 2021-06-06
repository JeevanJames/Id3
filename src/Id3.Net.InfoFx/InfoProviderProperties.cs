#region --- License & Copyright Notice ---
/*
Copyright (c) 2005-2021 Jeevan James
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
        /// <summary>
        ///     Initializes a new instance of the <see cref="InfoProviderProperties"/> class with a
        ///     descriptive <paramref name="name"/> and optional <paramref name="url"/> and
        ///     <paramref name="registrationUrl"/>.
        /// </summary>
        /// <param name="name">A descriptive name of the info provider.</param>
        /// <param name="url">A URL containing details of the info provider.</param>
        /// <param name="registrationUrl">
        ///     An optional URL that can be used to register with the info provider, if needed.
        /// </param>
        public InfoProviderProperties(string name, string url = null, string registrationUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(@"Provider must have a name.", nameof(name));
            Name = name;
            Url = url;
            RegistrationUrl = registrationUrl;
        }

        /// <summary>
        ///     Gets the descriptive name for the info provider.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the URL containing details of the info provider. Could be a home page.
        /// </summary>
        public string Url { get; }

        /// <summary>
        ///     Gets the URL to a registration page, if the info provider service needs credentials
        ///     to be used.
        /// </summary>
        public string RegistrationUrl { get; }

        /// <summary>
        ///     Gets the types of ID3 frames that are required as inputs for the info provider.
        /// </summary>
        public FrameTypes RequiredInputs { get; } = new();

        /// <summary>
        ///     Gets the types of ID3 frames that can be used but are not required as inputs for the
        ///     info provider.
        /// </summary>
        public FrameTypes OptionalInputs { get; } = new();

        /// <summary>
        ///     Gets the types of ID3 frames that are output by the info provider.
        /// </summary>
        public FrameTypes AvailableOutputs { get; } = new();

        public InfoProviderRequirements Requirements { get; set; } = InfoProviderRequirements.Tag;
    }

    [Flags]
    public enum InfoProviderRequirements
    {
        Tag = 0x1,
        MediaFileName = 0x2,
        MediaStream = 0x4,
    }
}
