using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace MangaCrawlerLib
{
    // TODO: Zlikwidowac
    public abstract class VisualState
    {
        public abstract void Restore();
        // TODO: tylko to co sie zmienilo
        public abstract void ReloadItems<T>(IEnumerable<T> a_enum) where T : class;
    }
}
