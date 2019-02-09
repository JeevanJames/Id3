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
        /// <summary>
        ///     Adds a frame type to the collection, based on the generic argument parameter.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="Id3Frame"/> to add.</typeparam>
        /// <returns>A reference to this <see cref="FrameTypes"/>, allowing for chaining calls.</returns>
        public FrameTypes Add<T>() where T : Id3Frame
        {
            base.Add(typeof(T));
            return this;
        }

        /// <summary>
        ///     Adds one or more <see cref="Id3Frame"/> types to the collection.
        /// </summary>
        /// <param name="types">The <see cref="Id3Frame"/> types to add.</param>
        /// <returns>A reference to this <see cref="FrameTypes"/>, allowing for chaining calls.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="types"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if any type in <paramref name="types"/> is <c>null</c> or does not derive from <see cref="Id3Frame"/>.
        /// </exception>
        public FrameTypes Add(params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));
            foreach (Type type in types)
            {
                if (type == null)
                    throw new ArgumentException($"Cannot specify null frame types", nameof(types));
                if (!type.IsSubclassOf(typeof(Id3Frame)))
                    throw new ArgumentException($"The type '{type.FullName}' is not a Id3Frame type.", nameof(types));
                base.Add(type);
            }
            return this;
        }
    }
}
