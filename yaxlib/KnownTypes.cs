// Copyright 2009 - 2010 Sina Iravanian - <sina@sinairv.com>
//
// This source file(s) may be redistributed, altered and customized
// by any means PROVIDING the authors name and all copyright
// notices remain intact.
// THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED. USE IT AT YOUR OWN RISK. THE AUTHOR ACCEPTS NO
// LIABILITY FOR ANY DATA DAMAGE/LOSS THAT THIS PRODUCT MAY CAUSE.
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using System.Linq;

namespace YAXLib
{
    public abstract class KnownType
    {
        public abstract void Serialize(object obj, XElement ele);

        public abstract object Deserialize(XElement ele);

        public abstract Type Type { get; }
    }

    public class RectangleKnownType : KnownType
    {
        public override void Serialize(object obj, XElement ele)
        {
            Rectangle rect = (Rectangle)obj;
            ele.Add(
                new XElement("Left", rect.Left),
                new XElement("Top", rect.Top),
                new XElement("Width", rect.Width),
                new XElement("Height", rect.Height));
        }

        public override object Deserialize(XElement ele)
        {
            return new Rectangle(
                Int32.Parse(ele.Element("Left").Value),
                Int32.Parse(ele.Element("Top").Value),
                Int32.Parse(ele.Element("Width").Value),
                Int32.Parse(ele.Element("Height").Value));
        }

        public override Type Type
        {
            get
            {
                return typeof(Rectangle);
            }
        }
    }

    public class ColorKnownType : KnownType
    {
        private static Color[] SystemColors;
        private static Color[] Colors;

        static ColorKnownType()
        {
            SystemColors = (from ct in typeof(System.Drawing.SystemColors).GetProperties()
                            where ct.PropertyType == typeof(Color)
                            select (Color)ct.GetValue(null, null)).ToArray();

            Colors = (from ct in typeof(System.Drawing.Color).GetProperties()
                      where ct.PropertyType == typeof(Color)
                      select (Color)ct.GetValue(null, null)).ToArray();
        }

        public override void Serialize(object obj, XElement ele)
        {
            Color color = (Color)obj;

            ele.Add(
                new XElement("R", color.R),
                new XElement("G", color.G),
                new XElement("B", color.B),
                new XElement("A", color.A));

            if (color.IsNamedColor)
            {
                if (!color.IsKnownColor)
                    throw new NotImplementedException();
                if (String.IsNullOrWhiteSpace(color.Name))
                    throw new NotImplementedException();

                if (color.IsSystemColor)
                    ele.Add(new XElement("SystemColorName", color.Name));
                else
                    ele.Add(new XElement("ColorName", color.Name));

            }
            else
            {
                if (color.IsKnownColor)
                    throw new NotImplementedException();
            }
        }

        public override object Deserialize(XElement ele)
        {
            int A = Int32.Parse(ele.Element("A").Value);
            int R = Int32.Parse(ele.Element("R").Value);
            int G = Int32.Parse(ele.Element("G").Value);
            int B = Int32.Parse(ele.Element("B").Value);

            XElement name_ele = ele.Element("SystemColorName");

            if (name_ele != null)
            {
                string name = name_ele.Value;

                if (SystemColors.Any(c => c.Name == name))
                    return SystemColors.First(c => c.Name == name);
            }

            name_ele = ele.Element("ColorName");

            if (name_ele != null)
            {
                string name = name_ele.Value;
            
                if (Colors.Any(c => c.Name == name))
                    return Colors.First(c => c.Name == name);
            }

            return Color.FromArgb(A, R, G, B);
        }

        public override Type Type
        {
            get
            {
                return typeof(Color);
            }
        }
    }

    public class TimeSpanKnownType : KnownType
    {
        public override void Serialize(object obj, XElement ele)
        {
            ele.Value = obj.ToString();
        }

        public override object Deserialize(XElement ele)
        {
            return TimeSpan.Parse(ele.Value);
        }

        public override Type Type
        {
            get
            {
                return typeof(TimeSpan);
            }
        }
    }

    public class GuidKnownType : KnownType
    {
        public override void Serialize(object obj, XElement ele)
        {
            ele.Value = obj.ToString();
        }

        public override object Deserialize(XElement ele)
        {
            string strGuidValue = ele.Value;
            Guid g = new Guid(strGuidValue);
            return g;
        }

        public override Type Type
        {
            get
            {
                return typeof(Guid);
            }
        }
    }

    internal class KnownTypes
    {
        private static Dictionary<Type, KnownType> m_dict = new Dictionary<Type, KnownType>();

        static KnownTypes()
        {
            Add(new RectangleKnownType());
            Add(new GuidKnownType());
            Add(new TimeSpanKnownType());
            Add(new ColorKnownType());
        }

        public static void Add(KnownType kt)
        {
            m_dict[kt.Type] = kt;
        }

        public static bool IsKnowType(Type type)
        {
            return m_dict.ContainsKey(type);
        }

        public static void Serialize(object obj, XElement ele)
        {
            m_dict[obj.GetType()].Serialize(obj, ele);
        }

        public static object Deserialize(XElement baseElement, Type type)
        {
            return m_dict[type].Deserialize(baseElement);
        }
    }
}