using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    public abstract class VisualState
    {
        protected abstract void Clear();
        public abstract void Restore();
        public abstract void RaiseSelectionChanged();
        public abstract void ReloadItems<T>(IEnumerable<T> a_enum) where T : class;
        public abstract bool ItemSelected { get; }
    }
}
