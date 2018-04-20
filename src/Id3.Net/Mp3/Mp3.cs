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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Id3.Resources;
using Id3.v1;
using Id3.v2;

namespace Id3
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a stream of MP3 data. Use this class to load MP3 data, manipulate the tags and save
    ///     the data back to the stream.
    /// </summary>
    public class Mp3 : IDisposable
    {
        #region Fields and properties
        //MP3 file stream-related
        private Stream _stream;
        private Mp3Permissions _permissions;

        //Audio stream properties
        private AudioStreamProperties _audioProperties;

        //ID3 Handler management
        private RegisteredId3Handlers _existingHandlers;

        private Stream Stream
        {
            get => _stream;
            set
            {
                Debug.Assert(_stream == null);
                _stream = value;
            }
        }

        protected bool StreamOwned { get; set; }
        #endregion

        #region Construction & destruction
        //For derived ctors
        protected Mp3()
        {
        }

        public Mp3(string filename, Mp3Permissions permissions = Mp3Permissions.Read)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            FileAccess fileAccess = PermissionsToFileAccessMapping[permissions];
            FileStream fileStream = File.Open(filename, FileMode.Open, fileAccess, FileShare.Read);
            SetupStream(fileStream, permissions);

            //Since we created the stream, we are responsible for disposing it when we're done
            StreamOwned = true;
        }

        public Mp3(FileInfo fileInfo, Mp3Permissions permissions = Mp3Permissions.Read)
        {
            if (fileInfo == null)
                throw new ArgumentNullException(nameof(fileInfo));

            FileAccess fileAccess = PermissionsToFileAccessMapping[permissions];
            FileStream fileStream = fileInfo.Open(FileMode.Open, fileAccess, FileShare.Read);
            SetupStream(fileStream, permissions);

            //Since we created the stream, we are responsible for disposing it when we're done
            StreamOwned = true;
        }

        private static readonly Dictionary<Mp3Permissions, FileAccess> PermissionsToFileAccessMapping = new Dictionary<Mp3Permissions, FileAccess>(3) {
            { Mp3Permissions.Read, FileAccess.Read },
            { Mp3Permissions.Write, FileAccess.Write },
            { Mp3Permissions.ReadWrite, FileAccess.ReadWrite },
        };

        /// <summary>
        ///     Creates an instance of the Mp3 class by passing in a Stream object containing the
        ///     MP3 data.
        /// </summary>
        /// <param name="stream">The Stream object containing the MP3 data.</param>
        /// <param name="permissions">The permissions applicable to the MP3 data. Defaults to read-only access.</param>
        public Mp3(Stream stream, Mp3Permissions permissions = Mp3Permissions.Read)
        {
            SetupStream(stream, permissions);

            //The stream is owned by the caller, so it is their responsibility to dispose it.
            StreamOwned = false;
        }

        /// <summary>
        ///     Creates an instance of the Mp3 class by passing in the MP3 data as a byte array.
        /// </summary>
        /// <param name="byteStream">The byte array representing the MP3 data.</param>
        /// <param name="permissions">The permissions applicable to the MP3 data. Defaults to read-only access.</param>
        public Mp3(byte[] byteStream, Mp3Permissions permissions = Mp3Permissions.Read)
        {
            if (byteStream == null)
                throw new ArgumentNullException(nameof(byteStream));

            //Note: For Write permissions, we cannot use the MemoryStream ctor that takes the byte
            //array as a parameter. Those streams cannot increase their capacity using the SetLength
            //method, which is needed by the framework when adding tag information to the stream.
            var stream = new MemoryStream(byteStream.Length);
            stream.Write(byteStream, 0, byteStream.Length);

            SetupStream(stream, permissions);
        }

        protected void SetupStream(Stream stream, Mp3Permissions permissions)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (permissions == Mp3Permissions.Write)
                permissions = Mp3Permissions.ReadWrite;

            if (!stream.CanRead || !stream.CanSeek)
                throw new Id3Exception(Mp3Messages.StreamNotReadableOrSeekable);
            if (permissions == Mp3Permissions.ReadWrite && !stream.CanWrite)
                throw new Id3Exception(Mp3Messages.StreamNotWritable);

            Stream = stream;
            _permissions = permissions;
        }

        public void Dispose()
        {
            if (StreamOwned)
                Stream?.Dispose();
        }
        #endregion

        private void EnsureWritePermissions(string errorMessage)
        {
            if (_permissions != Mp3Permissions.ReadWrite)
                throw new NotSupportedException(string.Format(errorMessage, GetType().Name));
        }

        #region Tag deleting methods
        /// <summary>
        ///     Deletes the ID3 tag of the specified version from the MP3 data.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number.</param>
        public void DeleteTag(int majorVersion, int minorVersion)
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            RegisteredId3Handler registeredHandler = ExistingHandlers.GetHandler(majorVersion, minorVersion);
            if (registeredHandler != null)
            {
                registeredHandler.Handler.DeleteTag(_stream);
                InvalidateExistingHandlers();
            }
        }

        /// <summary>
        ///     Deletes the ID3 tag of the specified tag family type from the MP3 data.
        /// </summary>
        /// <param name="family">The ID3 tag family type.</param>
        public void DeleteTag(Id3TagFamily family)
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            IEnumerable<RegisteredId3Handler> registeredHandlers = ExistingHandlers.GetHandlers(family);
            RegisteredId3Handler registeredHandler = registeredHandlers.FirstOrDefault();
            if (registeredHandler != null)
            {
                Id3Handler handler = registeredHandler.Handler;
                handler.DeleteTag(_stream);
                InvalidateExistingHandlers();
            }
        }

        /// <summary>
        ///     Deletes all ID3 tags from the MP3 data.
        /// </summary>
        public void DeleteAllTags()
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            foreach (RegisteredId3Handler existingHandler in ExistingHandlers)
                existingHandler.Handler.DeleteTag(_stream);
            InvalidateExistingHandlers();
        }
        #endregion

        #region Tag retrieval methods
        /// <summary>
        ///     Returns a collection of all ID3 tags present in the MP3 data.
        /// </summary>
        /// <returns>A collection of all ID3 tags present in the MP3 data.</returns>
        public IEnumerable<Id3Tag> GetAllTags()
        {
            var tags = new Id3Tag[ExistingHandlers.Count];
            for (int i = 0; i < tags.Length; i++)
            {
                Id3Handler handler = ExistingHandlers[i].Handler;
                tags[i] = handler.ReadTag(_stream);
            }

            return tags;
        }

        /// <summary>
        ///     Retrieves an ID3 tag of the specified tag family type - version 2.x or version 1.x.
        /// </summary>
        /// <param name="family">The ID3 tag family type required.</param>
        /// <returns>The ID3 tag of the specified tag family type, or null if it doesn't exist.</returns>
        public Id3Tag GetTag(Id3TagFamily family)
        {
            IEnumerable<RegisteredId3Handler> familyHandlers = ExistingHandlers.GetHandlers(family);

            RegisteredId3Handler familyHandler = familyHandlers.FirstOrDefault();
            if (familyHandler == null)
                return null;
            Id3Handler handler = familyHandler.Handler;
            Id3Tag tag = handler.ReadTag(_stream);
            return tag;
        }

        /// <summary>
        ///     Retrieves an ID3 tag of the specified version number.
        /// </summary>
        /// <param name="majorVersion">The major version number.</param>
        /// <param name="minorVersion">The minor version number</param>
        /// <returns>The ID3 tag of the specified version number, or null if it doesn't exist.</returns>
        public Id3Tag GetTag(int majorVersion, int minorVersion)
        {
            RegisteredId3Handler registeredHandler = ExistingHandlers.GetHandler(majorVersion, minorVersion);
            return registeredHandler?.Handler.ReadTag(_stream);
        }

        //Retrieves the tag as a byte array. This method does not attempt to read the tag data,
        //it simply reads the header and if present the tag bytes are read directly from the
        //stream. This means that typical exceptions that get thrown in a tag read will not
        //occur in this method.
        public byte[] GetTagBytes(int majorVersion, int minorVersion)
        {
            RegisteredId3Handler registeredHandler = RegisteredHandlers.GetHandler(majorVersion, minorVersion);
            byte[] tagBytes = registeredHandler.Handler.GetTagBytes(_stream);
            return tagBytes;
        }
        #endregion

        #region Tag querying methods
        public bool HasTagOfFamily(Id3TagFamily family)
        {
            return ExistingHandlers.Any(handler => handler.Handler.Family == family);
        }

        public bool HasTagOfVersion(int majorVersion, int minorVersion)
        {
            return
                ExistingHandlers.Any(handler =>
                    handler.Handler.MajorVersion == majorVersion && handler.Handler.MinorVersion == minorVersion);
        }

        public IEnumerable<Version> AvailableTagVersions
        {
            get
            {
                return ExistingHandlers.Select(handler =>
                    new Version(handler.Handler.MajorVersion, handler.Handler.MinorVersion));
            }
        }

        public bool HasTags => ExistingHandlers.Count > 0;
        #endregion

        #region Tag writing methods
        public bool WriteTag(Id3Tag tag, WriteConflictAction conflictAction = WriteConflictAction.NoAction)
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotWriteTag);
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            //The tag should specify major version number
            if (tag.MajorVersion == 0)
                throw new ArgumentException(Mp3Messages.MajorTagVersionMissing, nameof(tag));

            //Get any existing handlers from the same family as the tag
            IEnumerable<RegisteredId3Handler> familyHandlers = ExistingHandlers.GetHandlers(tag.Family);

            //If a tag already exists from the same family, but is a different version than the passed tag,
            //delete it if conflictAction is Replace.
            RegisteredId3Handler familyHandler = familyHandlers.FirstOrDefault();
            if (familyHandler != null)
            {
                Id3Handler handler = familyHandler.Handler;
                if (handler.MajorVersion != tag.MajorVersion || handler.MinorVersion != tag.MinorVersion)
                {
                    if (conflictAction == WriteConflictAction.NoAction)
                        return false;
                    if (conflictAction == WriteConflictAction.Replace)
                    {
                        Id3Handler handlerCopy = handler;
                        handlerCopy.DeleteTag(_stream);
                    }
                }
            }

            //Write the tag to the file. The handler will know how to overwrite itself.
            RegisteredId3Handler registeredHandler = RegisteredHandlers.GetHandler(tag.MajorVersion, tag.MinorVersion);
            bool writeSuccessful = registeredHandler.Handler.WriteTag(_stream, tag);
            if (writeSuccessful)
                InvalidateExistingHandlers();
            return writeSuccessful;
        }

        public bool WriteTag(Id3Tag tag, int majorVersion, int minorVersion,
            WriteConflictAction conflictAction = WriteConflictAction.NoAction)
        {
            tag.MajorVersion = majorVersion;
            tag.MinorVersion = minorVersion;
            return WriteTag(tag, conflictAction);
        }
        #endregion

        #region Audio stream members
        public byte[] GetAudioStream()
        {
            byte[] startBytes = null, endBytes = null;
            foreach (RegisteredId3Handler registeredHandler in ExistingHandlers)
            {
                if (registeredHandler.Handler.Family == Id3TagFamily.Version2X)
                    startBytes = registeredHandler.Handler.GetTagBytes(_stream);
                else
                    endBytes = registeredHandler.Handler.GetTagBytes(_stream);
            }

            long audioStreamLength = _stream.Length - (startBytes?.Length ?? 0) - (endBytes?.Length ?? 0);
            var audioStream = new byte[audioStreamLength];
            _stream.Seek(startBytes?.Length ?? 0, SeekOrigin.Begin);
            _stream.Read(audioStream, 0, (int) audioStreamLength);
            return audioStream;
        }

        public AudioStreamProperties Audio
        {
            get
            {
                if (_audioProperties == null)
                {
                    byte[] audioStream = GetAudioStream();
                    if (audioStream == null || audioStream.Length == 0)
                        throw new AudioStreamException(Mp3Messages.AudioStreamMissing);
                    _audioProperties = new AudioStream(audioStream).Calculate();
                }

                return _audioProperties;
            }
        }
        #endregion

        #region ID3 handler registration/management
        private void InvalidateExistingHandlers()
        {
            _existingHandlers = null;
        }

        //The list of registered ID3 handlers for existing tags in the file. This list is
        //dynamically built and is the basis for most of the GetXXXX methods.
        //Whenever the MP3 stream is changed (such as when WriteTag or DeleteTag is called), the
        //_existingHandlers field should be reset to null so that this list can be recreated the
        //next time it is accessed.
        private RegisteredId3Handlers ExistingHandlers
        {
            get
            {
                if (_existingHandlers != null)
                    return _existingHandlers;

                var v2HandlerFound = false;
                _existingHandlers = new RegisteredId3Handlers();
                foreach (RegisteredId3Handler registeredHandler in RegisteredHandlers)
                {
                    Id3Handler handler = registeredHandler.Handler;
                    if (handler.Family == Id3TagFamily.Version2X && v2HandlerFound)
                        continue;
                    if (handler.HasTag(_stream))
                    {
                        _existingHandlers.Add(registeredHandler);
                        v2HandlerFound = handler.Family == Id3TagFamily.Version2X;
                    }
                }

                return _existingHandlers;
            }
        }

        static Mp3()
        {
            RegisteredHandlers.Register<Id3V23Handler>();
            RegisteredHandlers.Register<Id3V1Handler>();
            RegisteredHandlers.Register<Id3V24Handler>();
            RegisteredHandlers.Register<Id3V22Handler>();
        }

        internal static RegisteredId3Handlers RegisteredHandlers { get; } = new RegisteredId3Handlers();
        #endregion
    }
}
