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
using System.Linq;

namespace Id3
{
    public class Id3SyncFrameList<TFrame> : Collection<TFrame>
        where TFrame : Id3Frame
    {
        /// <summary>
        ///     Reference to the main frame list
        /// </summary>
        private readonly Id3FrameList _mainList;

        /// <summary>
        ///     If true, indicates that an internal list update is taking place, and the list change notification methods should
        ///     not perform the corresponding updates on the main list.
        /// </summary>
        private bool _internalUpdate;

        internal Id3SyncFrameList(Id3FrameList mainList)
        {
            _mainList = mainList;
            _mainList.CollectionChanged += OnMainListChanged;

            //Extract all frames of type TFrame and store then in this collection.
            //The list change notification events should not be fired during this process.
            RunWithoutMainListSync(() => {
                foreach (TFrame frame in _mainList.OfType<TFrame>())
                    Add(frame);
            });
        }

        /// <summary>
        ///     Runs the specified action, which can make changes to the collection, without syncing the main list.
        /// </summary>
        /// <param name="action">The action to run</param>
        private void RunWithoutMainListSync(Action action)
        {
            _internalUpdate = true;
            try
            {
                action();
            } finally
            {
                _internalUpdate = false;
            }
        }

        private void OnMainListChanged(object sender, FrameListChangedEventArgs e)
        {
            RunWithoutMainListSync(() => {
                if (e.ChangeType == FrameListChangeType.Removed)
                {
                    if (e.FrameType == typeof(TFrame))
                        Remove((TFrame) e.Frame);
                } else if (e.ChangeType == FrameListChangeType.Cleared)
                {
                    Clear();
                }
            });
        }

        #region List change notification methods
        protected override void ClearItems()
        {
            if (!_internalUpdate)
            {
                for (int i = _mainList.Count - 1; i >= 0; i--)
                {
                    if (_mainList[i].GetType() == typeof(TFrame))
                        _mainList.RemoveAt(i);
                }
            }

            base.ClearItems();
        }

        protected override void InsertItem(int index, TFrame item)
        {
            if (!_internalUpdate)
                _mainList.Add(item);
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            if (!_internalUpdate)
            {
                TFrame frame = this[index];
                _mainList.Remove(frame);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TFrame item)
        {
            if (!_internalUpdate)
            {
                TFrame frame = this[index];
                int mainIndex = _mainList.IndexOf(frame);
                _mainList[mainIndex] = item;
            }

            base.SetItem(index, item);
        }
        #endregion
    }
}
