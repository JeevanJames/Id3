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
        //Reference to the main frame list
        private readonly Id3FrameList _mainList;

        //If true, indicates that an internal list update is taking place, and the list change
        //notification methods should not perform the corresponding updates on the main list.
        private bool _internalUpdate;

        internal Id3SyncFrameList(Id3FrameList mainList)
        {
            _mainList = mainList;
            _mainList.CollectionChanged += OnMainListChanged;

            //Extract all frames of type TFrame and store then in this collection.
            //The list change notification events should not be fired during this process.
            _internalUpdate = true;
            try
            {
                foreach (TFrame frame in _mainList.OfType<TFrame>())
                    Add(frame);
            }
            finally
            {
                _internalUpdate = false;
            }
        }

        private void OnMainListChanged(object sender, FrameListChangedEventArgs e)
        {
            _internalUpdate = true;
            try
            {
                switch (e.ChangeType)
                {
                    case FrameListChangeType.Removed:
                        if (e.FrameType == typeof(TFrame))
                            Remove((TFrame)e.Frame);
                        break;
                    case FrameListChangeType.Cleared:
                        Clear();
                        break;
                }
            }
            finally
            {
                _internalUpdate = false;
            }
        }

        #region Public utility methods
        public TFrame Find(Func<TFrame, bool> predicate)
        {
            return this.FirstOrDefault(predicate);
        }

        public TFrame[] FindAll(Func<TFrame, bool> predicate)
        {
            return this.Where(predicate).ToArray();
        }
        #endregion

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