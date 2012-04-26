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

namespace YAXLib
{
    /// <summary>
    /// The base class for all attributes defined in YAXLib.
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public abstract class YAXBaseAttribute : System.Attribute
    {
    }

    /// <summary>
    /// Creates a comment node per each line of the comment string provided.
    /// This attribute is applicable to classes, structures, fields, and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXCommentAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCommentAttribute"/> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public YAXCommentAttribute(string comment)
        {
            this.Comment = comment;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Add this attribute to types, structs or classes which you want to override
    /// their default serialization behaviour. This attribute is optional.
    /// This attribute is applicable to classes and structures.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXSerializeAttribute : YAXBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXSerializeAttribute"/> class.
        /// </summary>
        public YAXSerializeAttribute()
        {
            FieldsToSerialize = YAXSerializationFields.AttributedFieldsOnly;
        }

        /// <summary>
        /// Gets or sets the fields which YAXLib selects for serialization
        /// </summary>
        /// <value>The fields to serialize.</value>
        public YAXSerializationFields FieldsToSerialize { get; set; }
    }

    /// <summary>
    /// Add this attribute to properties or fields which you wish to be serialized, when
    /// the enclosing class uses the <c>YAXSerializableType</c> attribute in which <c>FieldsToSerialize</c>
    /// has been set to <c>AttributedFieldsOnly</c>.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXNodeAttribute : YAXBaseAttribute
    {
        internal static Object DefaultValueNotSet = new Object();

        public YAXNodeAttribute()
        {
            Default = DefaultValueNotSet;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXNodeAttribute"/> class.
        /// </summary>
        /// <param name="serializeAs">the alias for the property or field under which the property or field 
        /// will be serialized.</param>
        public YAXNodeAttribute(string alias)
        {
            Alias = alias;
            Default = DefaultValueNotSet;
        }

        /// <summary>
        /// Gets or sets the alias for the property under which the property will be serialized.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// This node data will be encapsulate with CDATA
        /// </summary>
        public bool CDATA { get; set; }

        /// <summary>
        /// This node is not required in xml, default value will be used instead.
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// When value is equal default this field will not be serialized.
        /// </summary>
        public bool OmitWhenDefault { get; set; }
    }

    /// <summary>
    /// Makes a property to appear as an attribute for the enclosing class (i.e. the parent element) if possible.
    /// This attribute is applicable to fields and properties only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXAttributeForClassAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Makes a field or property to appear as an attribute for another element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXAttributeForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes an attribute.</param>
        public YAXAttributeForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the element of which the property becomes an attribute.
        /// </summary>
        public string Parent { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Makes a field or property to appear as a value for another element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXValueForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXAttributeForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes an attribute.</param>
        public YAXValueForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the element for which the property becomes a value.
        /// </summary>
        public string Parent { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Makes a field or property to appear as a value for its parent element, if possible.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXValueForClassAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXValueForClassAttribute"/> class.
        /// </summary>
        public YAXValueForClassAttribute()
        {
        }

        #endregion Constructors
    }

    /// <summary>
    /// Prevents serialization of some field or property.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXDontSerializeAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Defines an alias for the field, property, class, or struct under
    /// which it will be serialized. This attribute is applicable to fields,
    /// properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXSerializeAsAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXSerializeAsAttribute"/> class.
        /// </summary>
        /// <param name="alias">the alias for the property or filed under which the property or filed 
        /// will be serialized.</param>
        public YAXSerializeAsAttribute(string alias)
        {
            this.Alias = alias;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the alias for the property under which the property will be serialized.
        /// </summary>
        public string Alias { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Makes a property or field to appear as a child element
    /// for another element. This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXElementForAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXElementForAttribute"/> class.
        /// </summary>
        /// <param name="parent">The element of which the property becomes a child element.</param>
        public YAXElementForAttribute(string parent)
        {
            this.Parent = parent;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the element of which the property becomes a child element.
        /// </summary>
        /// <value>The element of which the property becomes a child element.</value>
        public string Parent { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Controls the serialization of collection instances.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXCollectionAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCollectionAttribute"/> class.
        /// </summary>
        public YAXCollectionAttribute()
        {
            this.SerializationType = YAXCollectionSerializationTypes.Recursive;
            this.SeparateBy = " ";
            this.ElementName = null;
            this.IsWhiteSpaceSeparator = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCollectionAttribute"/> class.
        /// </summary>
        public YAXCollectionAttribute(string a_name)
        {
            this.Name = a_name;
            this.SerializationType = YAXCollectionSerializationTypes.Recursive;
            this.SeparateBy = " ";
            this.ElementName = null;
            this.IsWhiteSpaceSeparator = true;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the type of the serialization of the collection.
        /// </summary>
        /// <value>The type of the serialization of the collection.</value>
        public YAXCollectionSerializationTypes SerializationType { get; set; }

        /// <summary>
        /// Gets or sets the string to separate collection items, if the Serialization type is set to <c>Serially</c>.
        /// </summary>
        /// <value>the string to separate collection items, if the Serialization Type is set to <c>Serially</c>.</value>
        public string SeparateBy { get; set; }

        /// <summary>
        /// Gets or sets the name of each child element corresponding to the collection members, if the Serialization type is set to <c>Recursive</c>.
        /// </summary>
        /// <value>The name of each child element corresponding to the collection members, if the Serialization type is set to <c>Recursive</c>.</value>
        public string ElementName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether white space characters are to be
        /// treated as sparators or not.
        /// </summary>
        /// <value>
        /// <c>true</c> if white space separator characters are to be
        /// treated as sparators; otherwise, <c>false</c>.
        /// </value>
        public bool IsWhiteSpaceSeparator { get; set; }

        public string Name;

        #endregion Properties
    }

    /// <summary>
    /// Controls the serialization of generic Dictionary instances.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXDictionaryAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXDictionaryAttribute"/> class.
        /// </summary>
        public YAXDictionaryAttribute()
        {
            this.KeyName = null;
            this.ValueName = null;
            this.EachPairName = null;
            this.SerializeKeyAs = YAXNodeTypes.Element;
            this.SerializeValueAs = YAXNodeTypes.Element;
            this.KeyFormatString = null;
            this.ValueFormatString = null;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the alias for the key part of the dicitonary.
        /// </summary>
        /// <value></value>
        public string KeyName { get; set; }

        /// <summary>
        /// Gets or sets alias for the value part of the dicitonary.
        /// </summary>
        /// <value></value>
        public string ValueName { get; set; }

        /// <summary>
        /// Gets or sets alias for the element containing the Key-Value pair.
        /// </summary>
        /// <value></value>
        public string EachPairName { get; set; }

        /// <summary>
        /// Gets or sets the node type according to which the key part of the dictionary is serialized.
        /// </summary>
        /// <value></value>
        public YAXNodeTypes SerializeKeyAs { get; set; }

        /// <summary>
        /// Gets or sets the node type according to which the value part of the dictionary is serialized.
        /// </summary>
        /// <value></value>
        public YAXNodeTypes SerializeValueAs { get; set; }

        /// <summary>
        /// Gets or sets the key format string.
        /// </summary>
        /// <value></value>
        public string KeyFormatString { get; set; }

        /// <summary>
        /// Gets or sets the value format string.
        /// </summary>
        /// <value></value>
        public string ValueFormatString { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Specifies the behavior of the deserialization method, if the element/attribute corresponding to this property is missed in the XML input.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXErrorIfMissedAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXErrorIfMissedAttribute"/> class.
        /// </summary>
        /// <param name="treatAs">The value indicating this situation is going to be treated as Error or Warning.</param>
        public YAXErrorIfMissedAttribute(YAXExceptionTypes treatAs)
        {
            this.TreatAs = treatAs;
            this.DefaultValue = null;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the value indicating this situation is going to be treated as Error or Warning.
        /// </summary>
        /// <value>The value indicating this situation is going to be treated as Error or Warning.</value>
        public YAXExceptionTypes TreatAs { get; set; }

        /// <summary>
        /// Gets or sets the default value for the property if the element/attribute corresponding to this property is missed in the XML input.
        /// Setting <c>null</c> means do nothing.
        /// </summary>
        /// <value>The default value.</value>
        public object DefaultValue { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Specifies the format string provided for serializing data. The format string is the parameter
    /// passed to the <c>ToString</c> method.
    /// If this attribute is applied to collection classes, the format, therefore, is applied to
    /// the collection members.
    /// This attribute is applicable to fields and properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXFormatAttribute : YAXBaseAttribute
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXFormatAttribute"/> class.
        /// </summary>
        /// <param name="format">The format string.</param>
        public YAXFormatAttribute(string format)
        {
            this.Format = format;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the format string needed to serialize data. The format string is the parameter
        /// passed to the <c>ToString</c> method.
        /// </summary>
        /// <value></value>
        public string Format { get; set; }

        #endregion Properties
    }

    /// <summary>
    /// Specifies that a particular class, or a particular property or variable type, that is
    /// driven from <c>IEnumerable</c> should not be treated as a collection class/object.
    /// This attribute is applicable to fields, properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXNotCollectionAttribute : YAXBaseAttribute
    {
    }

    /// <summary>
    /// Specifies an alias for an enum member.
    /// This attribute is applicable to enum members.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class YAXEnumAttribute : YAXBaseAttribute
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="YAXEnumAttribute"/> class.
        /// </summary>
        /// <param name="alias">The alias.</param>
        public YAXEnumAttribute(string alias)
        {
            this.Alias = alias.Trim();
        }

        #endregion Constructor

        #region Public Properties

        /// <summary>
        /// Gets the alias for the enum member.
        /// </summary>
        /// <value>The alias for the enum member.</value>
        public string Alias { get; private set; }

        #endregion Public Properties
    }

    /// <summary>
    /// Specifies a custom serializer class for a field, property, class, or struct. YAXLib will instantiate an object
    /// from the specified type in this attribute, and calls appropriate methods while serializing.
    /// This attribute is applicable to fields, properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXCustomSerializerAttribute : YAXBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCustomSerializerAttribute"/> class.
        /// </summary>
        /// <param name="customSerializerType">Type of the custom serializer.</param>
        public YAXCustomSerializerAttribute(Type customSerializerType)
        {
            this.CustomSerializerType = customSerializerType;
        }

        /// <summary>
        /// Gets or sets the type of the custom serializer.
        /// </summary>
        /// <value>The type of the custom serializer.</value>
        public Type CustomSerializerType { get; private set; }
    }

    /// <summary>
    /// Specifies a custom deserializer class for a field, property, class, or struct. YAXLib will instantiate an object
    /// from the specified type in this attribute, and calls appropriate methods while deserializing.
    /// This attribute is applicable to fields, properties, classes, and structs.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public class YAXCustomDeserializerAttribute : YAXBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YAXCustomDeserializerAttribute"/> class.
        /// </summary>
        /// <param name="customDeserializerType">Type of the custom deserializer.</param>
        public YAXCustomDeserializerAttribute(Type customDeserializerType)
        {
            this.CustomDeserializerType = customDeserializerType;
        }

        /// <summary>
        /// Gets or sets the type of the custom deserializer.
        /// </summary>
        /// <value>The type of the custom deserializer.</value>
        public Type CustomDeserializerType { get; private set; }
    }

    /// <summary>
    ///
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class YAXOnDeserializedAttribute : YAXBaseAttribute
    {
    }
}