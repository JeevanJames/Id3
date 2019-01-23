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
using System.Linq;
using System.Reflection;

namespace Id3.InfoFx
{
    public abstract class InfoProvider
    {
        private InfoProviderProperties _properties;

        public Id3Tag[] GetInfo(InfoProviderInputs inputs) => GetInfo(null, inputs);

        public Id3Tag[] GetInfo(Id3Tag tag) => GetInfo(tag, InfoProviderInputs.Default);

        public Id3Tag[] GetInfo(Id3Tag tag, InfoProviderInputs inputs)
        {
            try
            {
                Inputs = inputs ?? InfoProviderInputs.Default;
                if (!MeetsInputCriteria(tag))
                    throw new InfoProviderException("Required inputs do not exist in the tag parameter");

                Id3Tag[] result = GetTagInfo(tag);
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

        public InfoProviderException TryGetInfo(Id3Tag tag, out Id3Tag[] resultTags)
        {
            try
            {
                resultTags = GetInfo(tag);
                return null;
            }
            catch (InfoProviderException ex)
            {
                resultTags = null;
                return ex;
            }
        }

        public bool MeetsInputCriteria(Id3Tag tag)
        {
            if (!Properties.CanOmitTag && tag == null)
                return false;
            if (Properties.RequiresFilename && string.IsNullOrEmpty(Inputs.FileName))
                return false;
            if (!Properties.CanOmitTag && !FramesMeetCriteria(tag, Properties.RequiredInputs))
                return false;
            return true;
        }

        public InfoProviderProperties Properties =>
            _properties ?? (_properties = GetProperties());

        protected abstract Id3Tag[] GetTagInfo(Id3Tag tag);

        protected abstract InfoProviderProperties GetProperties();

        protected InfoProviderInputs Inputs { get; private set; }

        private static bool FramesMeetCriteria(Id3Tag tag, IEnumerable<Type> frameTypes)
        {
            foreach (Type frameType in frameTypes)
            {
                Type frameTypeCopy = frameType;
                bool frameExists = tag.Any(frame => frame.GetType() == frameTypeCopy && frame.IsAssigned);
                if (!frameExists)
                    return false;
            }
            return true;
        }

        protected static readonly Id3Tag[] Empty = new Id3Tag[0];
    }
}