using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    // TODO: jak wszystko bedzie dziajac rozbudowac, poprzenosisc wiele rzeczy wspolnych
    public abstract class Entity
    {
        protected internal abstract void DownloadingStarted();
    }
}
