using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MangaCrawlerLib
{
    public abstract class Entity : IClassMapping
    {
        public virtual int ID { get; protected set; }
        protected virtual int Version { get; set; }
        public virtual string URL { get; protected set; }

        protected internal abstract Crawler Crawler { get; }
    }
}
