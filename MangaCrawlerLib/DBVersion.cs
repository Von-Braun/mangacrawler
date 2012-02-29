using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Mapping.ByCode;
using System.Reflection;

namespace MangaCrawlerLib
{
    public class DBVersion : IClassMapping
    {
        public virtual int ID { get; protected set; }
        public virtual string Version { get; set; }

        private void Map(ModelMapper a_mapper)
        {
            a_mapper.Class<DBVersion>(m => 
            {
                m.Property(c => c.Version);
                m.Id(c => c.ID, mapper => mapper.Generator(Generators.Native));
            });
        }
    }
}
