﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;

namespace MangaCrawlerLib
{
    public abstract class Entity
    {
        public ulong ID { get; internal set; }
        public string URL { get; protected set; }

        protected Entity(ulong a_id)
        {
            ID = a_id;
        }

        internal abstract Crawler Crawler { get; }
        protected internal abstract void RemoveOrphan();
    }
}
