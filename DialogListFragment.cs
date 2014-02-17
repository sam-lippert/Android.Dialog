using System;
using Android.Support.V4.App;
using Android.Text.Style;

namespace Android.Dialog
{
    public class DialogListFragment : ListFragment
    {
        private RootElement _root = null;
        public RootElement Root
        {
            get { return DialogAdapter == null ? null : DialogAdapter.Root; }
            set
            {
                value.Context = Activity;
                value.ValueChanged -= HandleValueChangedEvent;
                value.ValueChanged += HandleValueChangedEvent;

                if (ListAdapter == null)
                    _root = value;
                else
                    DialogAdapter.Root = value;

            }
        }

        public override Views.View OnCreateView(Views.LayoutInflater p0, Views.ViewGroup p1, OS.Bundle p2)
        {
            ListAdapter = DialogAdapter;
            return base.OnCreateView(p0, p1, p2);
        }

        public override void OnViewCreated(Views.View p0, OS.Bundle p1)
        {
            if (DialogAdapter == null)
            {
                ListAdapter = DialogAdapter = new DialogAdapter(Activity, _root, ListView);
                _root = null;
            }

            DialogAdapter.List = ListView;
            DialogAdapter.RegisterListView();
            base.OnViewCreated(p0, p1);
        }

        public DialogAdapter DialogAdapter { get; set; }

        public event EventHandler ValueChanged;

        private void HandleValueChangedEvent(object sender, EventArgs args)
        {
            if (ValueChanged != null)
                ValueChanged(sender, args);
        }

        public void ReloadData()
        {
            if (Root == null) return;
            DialogAdapter.ReloadData();
        }
    }
}