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
using System.Collections.ObjectModel;

using Id3.Frames;

namespace Id3.InfoFx
{
    /// <summary>
    ///     Collection of ID3 frame types.
    /// </summary>
    public sealed class FrameTypes : Collection<Type>
    {
        public void Add<T>() where T : Id3Frame
        {
            Add(typeof(T));
        }

        public void AddMultiple(params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            foreach (Type type in types)
                Add(type);
        }
    }
}