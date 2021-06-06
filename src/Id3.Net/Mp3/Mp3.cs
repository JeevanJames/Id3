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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Id3.Resources;

namespace Id3
{
    /// <inheritdoc />
    /// <summary>
    ///     Represents a stream of MP3 data. Use this class to load MP3 data, manipulate the tags and save
    ///     the data back to the stream.
    /// </summary>
    public class Mp3 : IDisposable
    {
        //MP3 file stream-related
        private Mp3Permissions _permissions;

        //Audio stream properties
        private AudioStreamProperties _audioProperties;

        //ID3 Handler management
        private IList<Id3Handler> _existingHandlers;

        private Stream Stream { get; set; }

        protected bool StreamOwned { get; set; }

        // For derived ctors
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

        private static readonly Dictionary<Mp3Permissions, FileAccess> PermissionsToFileAccessMapping = new(3)
        {
            [Mp3Permissions.Read] = FileAccess.Read,
            [Mp3Permissions.Write] = FileAccess.Write,
            [Mp3Permissions.ReadWrite] = FileAccess.ReadWrite,
        };

        /// <summary>
        ///     Initializes a new instance of the <see cref="Mp3"/> class by passing in a Stream object
        ///     containing the MP3 data.
        /// </summary>
        /// <param name="stream">The Stream object containing the MP3 data.</param>
        /// <param name="permissions">
        ///     The permissions applicable to the MP3 data. Defaults to read-only access.
        /// </param>
        public Mp3(Stream stream, Mp3Permissions permissions = Mp3Permissions.Read)
        {
            SetupStream(stream, permissions);

            //The stream is owned by the caller, so it is their responsibility to dispose it.
            StreamOwned = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Mp3"/> class by passing in the MP3 data
        ///     as a byte array.
        /// </summary>
        /// <param name="byteStream">The byte array representing the MP3 data.</param>
        /// <param name="permissions">
        ///     The permissions applicable to the MP3 data. Defaults to read-only access.
        /// </param>
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

        private void SetupStream(Stream stream, Mp3Permissions permissions)
        {
            if (stream is null)
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && StreamOwned)
                Stream?.Dispose();
        }

        private void EnsureWritePermissions(string errorMessage)
        {
            if (_permissions != Mp3Permissions.ReadWrite)
                throw new NotSupportedException(string.Format(errorMessage, GetType().Name));
        }

        #region Tag deleting methods

        /// <summary>
        ///     Deletes the ID3 tag of the specified version from the MP3 data.
        /// </summary>
        /// <param name="version">The tag version.</param>
        public async Task DeleteTag(Id3Version version)
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler handler = existingHandlers.FirstOrDefault(h => h.Version == version);
            if (handler is not null)
            {
                await handler.DeleteTag(Stream).ConfigureAwait(false);
                InvalidateExistingHandlers();
            }
        }

        /// <summary>
        ///     Deletes the ID3 tag of the specified tag family type from the MP3 data.
        /// </summary>
        /// <param name="family">The ID3 tag family type.</param>
        public async Task DeleteTag(Id3TagFamily family)
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler foundHandler = existingHandlers.FirstOrDefault(handler => handler.Family == family);
            if (foundHandler != null)
            {
                await foundHandler.DeleteTag(Stream).ConfigureAwait(false);
                InvalidateExistingHandlers();
            }
        }

        /// <summary>
        ///     Deletes all ID3 tags from the MP3 data.
        /// </summary>
        public async Task DeleteAllTags()
        {
            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotDeleteTag);
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            foreach (Id3Handler existingHandler in existingHandlers)
                await existingHandler.DeleteTag(Stream).ConfigureAwait(false);
            InvalidateExistingHandlers();
        }
        #endregion

        #region Tag retrieval methods

        /// <summary>
        ///     Returns a collection of all ID3 tags present in the MP3 data.
        /// </summary>
        /// <returns>A collection of all ID3 tags present in the MP3 data.</returns>
        public async Task<IEnumerable<Id3Tag>> GetAllTags()
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            return existingHandlers.Select(handler =>
            {
                (Id3Tag tag, _) = handler.ReadTag(Stream).GetAwaiter().GetResult();
                return tag;
            });
        }

        //TODO: Add IAsyncEnumerator overload for GetAllTags

        /// <summary>
        ///     Retrieves an ID3 tag of the specified tag family type - version 2.x or version 1.x.
        /// </summary>
        /// <param name="family">The ID3 tag family type required.</param>
        /// <returns>The ID3 tag of the specified tag family type, or null if it doesn't exist.</returns>
        public async Task<Id3Tag> GetTag(Id3TagFamily family)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler familyHandler = existingHandlers.FirstOrDefault(handler => handler.Family == family);
            if (familyHandler is null)
                return null;
            (Id3Tag tag, _) = await familyHandler.ReadTag(Stream).ConfigureAwait(false);
            return tag;
        }

        public async Task<(Id3Tag Tag, object AdditionalData)> GetTagWithAdditionalData(Id3TagFamily family)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler familyHandler = existingHandlers.FirstOrDefault(handler => handler.Family == family);
            return familyHandler is not null
                ? await familyHandler.ReadTag(Stream).ConfigureAwait(false)
                : (null, null);
        }

        /// <summary>
        ///     Retrieves an ID3 tag of the specified version number.
        /// </summary>
        /// <param name="version">The tag version number.</param>
        /// <returns>The ID3 tag of the specified version number, or null if it doesn't exist.</returns>
        public async Task<Id3Tag> GetTag(Id3Version version)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler handler = existingHandlers.FirstOrDefault(h => h.Version == version);
            if (handler is null)
                return null;
            (Id3Tag tag, _) = await handler.ReadTag(Stream).ConfigureAwait(false);
            return tag;
        }

        public async Task<(Id3Tag Tag, object AdditionalData)> GetTagWithAdditionalData(Id3Version version)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler handler = existingHandlers.FirstOrDefault(h => h.Version == version);
            return handler is not null
                ? await handler.ReadTag(Stream).ConfigureAwait(false)
                : (null, null);
        }

        /// <summary>
        ///     Retrieves the specified tag data as a byte array. This method does not attempt to read
        ///     the tag data, it simply reads the header and if present the tag bytes are read directly
        ///     from the stream. This means that typical exceptions that get thrown in a tag read will
        ///     not occur in this method.
        /// </summary>
        /// <param name="version">The tag version number.</param>
        /// <returns>A byte array of the tag data.</returns>
        public async Task<byte[]> GetTagBytes(Id3Version version)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler handler = existingHandlers.FirstOrDefault(h => h.Version == version);
            return handler is not null ? await handler.GetTagBytes(Stream).ConfigureAwait(false) : null;
        }

        #endregion

        #region Tag querying methods

        public async Task<bool> HasTagOfFamily(Id3TagFamily family)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            return existingHandlers.Any(handler => handler.Family == family);
        }

        public async Task<bool> HasTagOfVersion(Id3Version version)
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            return existingHandlers.Any(h => h.Version == version);
        }

        public async Task<IEnumerable<Id3Version>> AvailableTagVersions()
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            return existingHandlers.Select(h => h.Version);
        }

        public async Task<bool> HasTags()
        {
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            return existingHandlers.Count > 0;
        }

        #endregion

        #region Tag writing methods
        public Task<bool> UpdateTag(Id3Tag tag)
        {
            return WriteTag(tag, WriteConflictAction.Replace);
        }

        public Task<bool> WriteTag(Id3Tag tag, Id3Version version,
            WriteConflictAction conflictAction = WriteConflictAction.NoAction)
        {
            tag.Version = version;
            return WriteTag(tag, conflictAction);
        }

        public Task<bool> WriteTag(Id3Tag tag, WriteConflictAction conflictAction = WriteConflictAction.NoAction)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            EnsureWritePermissions(Mp3Messages.NoWritePermissions_CannotWriteTag);

            return WriteTagInternal(tag, conflictAction);
        }

        private async Task<bool> WriteTagInternal(Id3Tag tag, WriteConflictAction conflictAction)
        {
            //If a tag already exists from the same family, but is a different version than the passed tag,
            //delete it if conflictAction is Replace.
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            Id3Handler familyHandler = existingHandlers.FirstOrDefault(handler => handler.Family == tag.Family);
            if (familyHandler is not null)
            {
                Id3Handler handler = familyHandler;
                if (handler.Version != tag.Version)
                {
                    if (conflictAction == WriteConflictAction.NoAction)
                        return false;
                    if (conflictAction == WriteConflictAction.Replace)
                    {
                        Id3Handler handlerCopy = handler; //TODO: Why did we need a copy of the handler?
                        await handlerCopy.DeleteTag(Stream).ConfigureAwait(false);
                    }
                }
            }

            //Write the tag to the file. The handler will know how to overwrite itself.
            var writeHandler = Id3Handler.GetHandler(tag.Version);
            bool writeSuccessful = await writeHandler.WriteTag(Stream, tag).ConfigureAwait(false);
            if (writeSuccessful)
                InvalidateExistingHandlers();
            return writeSuccessful;
        }

        #endregion

        #region Audio stream members

        public async Task<byte[]> GetAudioStream()
        {
            byte[] startBytes = null, endBytes = null;
            IList<Id3Handler> existingHandlers = await GetExistingHandlers().ConfigureAwait(false);
            foreach (Id3Handler handler in existingHandlers)
            {
                if (handler.Family == Id3TagFamily.Version2X)
                    startBytes = await handler.GetTagBytes(Stream).ConfigureAwait(false);
                else
                    endBytes = await handler.GetTagBytes(Stream).ConfigureAwait(false);
            }

            long audioStreamLength = Stream.Length - (startBytes?.Length ?? 0) - (endBytes?.Length ?? 0);
            var audioStream = new byte[audioStreamLength];
            Stream.Seek(startBytes?.Length ?? 0, SeekOrigin.Begin);
            await Stream.ReadAsync(audioStream, 0, (int)audioStreamLength).ConfigureAwait(false);
            return audioStream;
        }

        public async Task<AudioStreamProperties> GetAudioStreamProperties()
        {
            if (_audioProperties is not null)
                return _audioProperties;

            byte[] audioStream = await GetAudioStream().ConfigureAwait(false);
            if (audioStream is null || audioStream.Length == 0)
                throw new Id3Exception(Mp3Messages.AudioStreamMissing);

            _audioProperties = new AudioStream(audioStream).Calculate();
            return _audioProperties;
        }

        #endregion

        #region ID3 handler registration/management

        private void InvalidateExistingHandlers()
        {
            _existingHandlers = null;
        }

        // The list of registered ID3 handlers for existing tags in the file. This list is
        // dynamically built and is the basis for most of the GetXXXX methods.
        // Whenever the MP3 stream is changed (such as when WriteTag or DeleteTag is called), the
        // _existingHandlers field should be reset to null so that this list can be recreated the
        // next time it is accessed.
        private async Task<IList<Id3Handler>> GetExistingHandlers()
        {
            if (_existingHandlers is not null)
                return _existingHandlers;

            var v2HandlerFound = false;
            _existingHandlers = new List<Id3Handler>(2);
            foreach (Id3HandlerMetadata handlerMetadata in Id3Handler.AvailableHandlers)
            {
                Id3Handler handler = handlerMetadata.Instance;
                if (handler.Family == Id3TagFamily.Version2X && v2HandlerFound)
                    continue;
                if (await handler.HasTag(Stream).ConfigureAwait(false))
                {
                    _existingHandlers.Add(handler);
                    v2HandlerFound = handler.Family == Id3TagFamily.Version2X;
                }
            }

            return _existingHandlers;
        }

        #endregion
    }
}
