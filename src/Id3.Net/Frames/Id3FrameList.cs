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
using System.Collections.ObjectModel;

namespace Id3
{
    internal sealed class Id3FrameList : Collection<Id3Frame>
    {
        internal event EventHandler<FrameListChangedEventArgs> CollectionChanged;

        protected override void ClearItems()
        {
            base.ClearItems();
            FireCollectionChangedEvent(FrameListChangeType.Cleared, null);
        }

        protected override void RemoveItem(int index)
        {
            Id3Frame frame = this[index];
            base.RemoveItem(index);
            FireCollectionChangedEvent(FrameListChangeType.Removed, frame);
        }

        private void FireCollectionChangedEvent(FrameListChangeType changeType, Id3Frame frame)
        {
            CollectionChanged?.Invoke(this, new FrameListChangedEventArgs(changeType, frame));
        }
    }

    internal sealed class FrameListChangedEventArgs : EventArgs
    {
        internal FrameListChangedEventArgs(FrameListChangeType changeType, Id3Frame frame)
        {
            ChangeType = changeType;
            Frame = frame;
            if (frame != null)
                FrameType = frame.GetType();
        }

        internal FrameListChangeType ChangeType { get; }

        internal Id3Frame Frame { get; }

        internal Type FrameType { get; }
    }

    internal enum FrameListChangeType
    {
        Removed,
        Cleared,
    }
}