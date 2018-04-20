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
            EventHandler<FrameListChangedEventArgs> collectionChanged = CollectionChanged;
            if (collectionChanged != null)
                collectionChanged(this, new FrameListChangedEventArgs(changeType, frame));
        }
    }

    internal sealed class FrameListChangedEventArgs : EventArgs
    {
        private readonly FrameListChangeType _changeType;
        private readonly Id3Frame _frame;
        private readonly Type _frameType;

        internal FrameListChangedEventArgs(FrameListChangeType changeType, Id3Frame frame)
        {
            _changeType = changeType;
            _frame = frame;
            if (frame != null)
                _frameType = frame.GetType();
        }

        internal FrameListChangeType ChangeType
        {
            get { return _changeType; }
        }

        public Id3Frame Frame
        {
            get { return _frame; }
        }

        internal Type FrameType
        {
            get { return _frameType; }
        }
    }

    internal enum FrameListChangeType
    {
        Removed,
        Cleared,
    }
}