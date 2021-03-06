using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Linq;

namespace Android.Dialog
{
    public class DialogAdapter : BaseAdapter<Section>, AdapterView.IOnItemClickListener, AdapterView.IOnItemLongClickListener
    {
        public DialogAdapter(Context context, RootElement root, ListView listView = null)
        {
            _root = root;
            Root.Context = context;

            // This is only really required when using a DialogAdapter with a ListView, in a non DialogActivity based activity.
            List = listView;
            RegisterListView();
        }

        public ListView List { get; set; }

        private readonly object _syncLock = new object();
        public void RegisterListView()
        {
            lock (_syncLock)
            {
                if (List == null) return;
                var elements = Root.Sections.SelectMany(e => e).ToList();
                if (elements.Any(e => e.Click != null))
                    List.OnItemClickListener = this;
                if (elements.Any(e => e.LongClick != null))
                    List.OnItemLongClickListener = this;
            }
        }

        public void DeregisterListView()
        {
            lock (_syncLock)
            {
                if (List == null) return;
                if (List.OnItemClickListener == this)
                    List.OnItemClickListener = null;
                if (List.OnItemLongClickListener == this)
                    List.OnItemLongClickListener = null;
                List = null;
            }
        }

        private RootElement _root;
        public RootElement Root
        {
            get { return _root; }
            set
            {
                if (_root != null && _root.Context != null)
                    value.Context = _root.Context;
                _root = value;
                ReloadData();
            }
        }

        public override bool IsEnabled(int position)
        {
            var element = ElementAtIndex(position);
            return !(element is Section) && element != null && element.IsSelectable;
        }

        public override int Count
        {
            get
            {
                //Get each adapter's count + 2 for the header and footer
                return Root.Sections.Sum(s => s.Count() + 2);
            }
        }

        public override int ViewTypeCount
        {
            get
            {
                // ViewTypeCount is the same as Count for these,
                // there are as many ViewTypes as Views as every one is unique!
                return Count > 0 ? Count : 1;
            }
        }

        /// <summary>
        /// Return the Element for the flattened/dereferenced position value.
        /// </summary>
        /// <param name="position">The direct index to the Element.</param>
        /// <returns>The Element object at the specified position or null if too out of bounds.</returns>
        public Element ElementAtIndex(int position)
        {
            int sectionIndex = 0;
            foreach (var s in Root.Sections)
            {
                if (position == 0)
                    return Root.Sections[sectionIndex];

                // note: plus two for the section header and footer views
                var size = s.Count() + 2;
                if (position == size - 1)
                    return null;
                if (position < size)
                    return Root.Sections[sectionIndex][position - 1];
                position -= size;
                sectionIndex++;
            }

            return null;
        }

        public override Section this[int position]
        {
            get { return Root.Sections[position]; }
        }

        public override bool AreAllItemsEnabled()
        {
            return false;
        }

        public override int GetItemViewType(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var element = ElementAtIndex(position);
            if (element == null)
            {
                element = ElementAtIndex(position - 1);
                while (!(element is Section))
                    element = element.Parent;
                return ((Section)element).GetFooterView(Root.Context, convertView, parent);
            }
            return element.GetView(Root.Context, convertView, parent);
        }

        public void ReloadData()
        {
            if (Root != null && Root.Context != null)
                ((Activity)Root.Context).RunOnUiThread(() =>
                {
                    if (Root != null)
                    {
                        NotifyDataSetChanged();
                    }
                });
        }

        #region Implementation of IOnItemClickListener

        /// <summary>
        /// Handles the ItemClick event of the ListView control.
        /// </summary>
        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            var elem = ElementAtIndex(position);
            if (elem == null) return;
            elem.Selected();
            if (elem.Click != null)
                elem.Click(parent, EventArgs.Empty);
        }

        #endregion

        #region Implementation of IOnItemLongClickListener

        /// <summary>
        /// Handles the ItemLongClick event of the ListView control.
        /// </summary>
        public bool OnItemLongClick(AdapterView parent, View view, int position, long id)
        {
            var elem = ElementAtIndex(position);
            if (elem != null && elem.LongClick != null)
            {
                elem.LongClick(parent, EventArgs.Empty);
                return true;
            }
            return false;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            DeregisterListView();
            base.Dispose(disposing);
        }
    }
}