using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomanuExtensions;
using System.IO;
using System.Web;

namespace MangaCrawlerLib
{
    public abstract class Entity
    {
        public ulong ID { get; internal set; }
        public ulong LimiterOrder;
        public string URL { get; internal set; }

        protected Entity(ulong a_id)
        {
            ID = a_id;
        }

        public override bool Equals(object a_obj)
        {
            if (a_obj == null)
                return false;
            Entity entity = a_obj as Entity;
            if (Object.ReferenceEquals(entity, null))
                return false;
            return this == entity;
        }

        public static bool operator ==(Entity a_left, Entity a_right)
        {
            if (Object.ReferenceEquals(a_left, a_right))
                return true;
            if (Object.ReferenceEquals(a_left, null))
                return false;
            if (Object.ReferenceEquals(a_right, null))
                return false;
            return a_left.ID == a_right.ID;
        }

        public static bool operator !=(Entity a_left, Entity a_right)
        {
            return !(a_left == a_right);
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool IsDirectoryExists()
        {
            try
            {
                return new DirectoryInfo(GetDirectory()).Exists;
            }
            catch
            {
                return false;
            }
        }

        public static string HtmlDecode(string a_str)
        {
            a_str = HttpUtility.HtmlDecode(a_str);
            return Uri.UnescapeDataString(a_str);
        }

        internal abstract Crawler Crawler { get; }
        public abstract bool IsDownloading { get; }
        public abstract string GetDirectory();
    }
}
