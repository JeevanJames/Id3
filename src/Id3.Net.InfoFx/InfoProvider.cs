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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Id3.InfoFx
{
    /// <summary>
    ///     Represents the base class for info providers.
    /// </summary>
    public abstract class InfoProvider
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private InfoProviderProperties _properties;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputs"></param>
        /// <returns></returns>
        /// <exception cref="InfoProviderException">Thrown on any unhandled error.</exception>
        public Id3Tag[] GetInfo(InfoProviderInputs inputs)
        {
            try
            {
                Inputs = inputs ?? new InfoProviderInputs();
                if (!MeetsInputCriteria())
                    throw new InfoProviderException("Required inputs do not meet the criteria of the info provider.");

                Id3Tag[] result = GetTagInfo();
                return result;
            }
            catch (InfoProviderException)
            {
                throw;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null)
            {
                throw new InfoProviderException(ex.InnerException.Message, ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new InfoProviderException(ex.Message, ex);
            }
        }

        public InfoProviderException TryGetInfo(InfoProviderInputs inputs, out Id3Tag[] resultTags)
        {
            try
            {
                resultTags = GetInfo(inputs);
                return null;
            }
            catch (InfoProviderException ex)
            {
                resultTags = null;
                return ex;
            }
        }

        protected InfoProviderProperties Properties =>
            _properties ?? (_properties = GetProperties());

        /// <summary>
        ///     When overridden in a derived class, gets the tag details.
        /// </summary>
        /// <returns></returns>
        protected abstract Id3Tag[] GetTagInfo();

        /// <summary>
        ///     When overridden in a derived class, gets the properties of the info provider.
        /// </summary>
        /// <returns>A <see cref="InfoProviderProperties"/> instance.</returns>
        protected abstract InfoProviderProperties GetProperties();

        protected InfoProviderInputs Inputs { get; private set; }

        /// <summary>
        ///     Indicates whether the inputs meets the criteria for the info provider.
        /// </summary>
        /// <returns><c>true</c> if the inputs meet the criteria.</returns>
        private bool MeetsInputCriteria()
        {
            // If the tag is required, but is not specified.
            bool requiresTag = (Properties.Requirements & InfoProviderRequirements.Tag) == InfoProviderRequirements.Tag;
            if (requiresTag && Inputs.Tag == null)
                return false;

            // If a file name is required, but not specified.
            bool requiresFileName = (Properties.Requirements & InfoProviderRequirements.MediaFileName) == InfoProviderRequirements.MediaFileName;
            if (requiresFileName && string.IsNullOrWhiteSpace(Inputs.FileName))
                return false;

            // If a MP3 stream is required, but not specified.
            bool requiresStream = (Properties.Requirements & InfoProviderRequirements.MediaStream) == InfoProviderRequirements.MediaStream;
            if (requiresStream && Inputs.Mp3Stream == null)
                return false;

            // If the tag is required, but does not contain the required frames.
            if (requiresTag && !FramesMeetCriteria(Inputs.Tag, Properties.RequiredInputs))
                return false;

            return true;
        }

        /// <summary>
        ///     Indicates whether the specified ID3 <paramref name="tag"/> has the specified frames.
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <param name="frameTypes">The ID3 frame types that the tag must contain.</param>
        /// <returns><c>true</c> if the tag has all the specified frames.</returns>
        private static bool FramesMeetCriteria(Id3Tag tag, IEnumerable<Type> frameTypes)
        {
            foreach (Type frameType in frameTypes)
            {
                Type frameTypeCopy = frameType;
                // Frame must be assigned in order to be considered.
                bool frameExists = tag.Any(frame => frame.GetType() == frameTypeCopy && frame.IsAssigned);
                if (!frameExists)
                    return false;
            }
            return true;
        }

        protected static readonly Id3Tag[] Empty = new Id3Tag[0];
    }
}