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
using System.Collections.ObjectModel;
using System.Linq;

namespace Id3
{
    /// <summary>
    ///     Represents an ID3 tag handler for a given ID3 tag type.
    /// </summary>
    internal sealed class RegisteredId3Handler
    {
        private Id3Handler _handler;

        internal RegisteredId3Handler(Type type)
        {
            Type = type;
        }

        /// <summary>
        ///     Instance of the ID3 handler. This is lazily instantiated on first access to the property.
        /// </summary>
        internal Id3Handler Handler =>
            _handler ?? (_handler = (Id3Handler) Activator.CreateInstance(Type));

        /// <summary>
        ///     The type of the ID3 handler class
        /// </summary>
        internal Type Type { get; }
    }

    /// <inheritdoc />
    /// <summary>
    ///     Collection of registered ID3 tag handlers.
    /// </summary>
    internal sealed class RegisteredId3Handlers : Collection<RegisteredId3Handler>
    {
        /// <summary>
        ///     Returns the ID3 tag handler for the specified tag version.
        /// </summary>
        /// <param name="majorVersion">Major version of ID3 tag</param>
        /// <param name="minorVersion">Minor version of ID3 tag</param>
        /// <returns>The registered tag handler or null if it is not in the collection.</returns>
        internal RegisteredId3Handler GetHandler(int majorVersion, int minorVersion)
        {
            RegisteredId3Handler registeredHandler =
                this.FirstOrDefault(handler =>
                    handler.Handler.MajorVersion == majorVersion && handler.Handler.MinorVersion == minorVersion);
            return registeredHandler;
        }

        /// <summary>
        ///     Returns all ID3 tag handlers for the specified tag version family.
        /// </summary>
        /// <param name="family">The tag version family</param>
        /// <returns>All registered tag handlers that match</returns>
        internal IEnumerable<RegisteredId3Handler> GetHandlers(Id3TagFamily family)
        {
            return this.Where(handler => handler.Handler.Family == family);
        }

        /// <summary>
        ///     Registers a handler with this collection.
        /// </summary>
        /// <typeparam name="THandler">The type of ID3 handler</typeparam>
        internal void Register<THandler>() where THandler : Id3Handler
        {
            Type handlerType = typeof(THandler);
            Add(new RegisteredId3Handler(handlerType));
        }
    }
}
