using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RMDEditor.Node
{
    public class NodeDataConverter : TypeConverter
    {
        public static bool QuietErrors { get { return _QuietErrors; } set { _QuietErrors = value; } }
        private static bool _QuietErrors = true;

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string s = value as string;
                    if (s.StartsWith("0x"))
                        s = s.Remove(0, 2);

                    if (context.PropertyDescriptor.PropertyType == typeof(uint))
                        return uint.Parse(s, NumberStyles.HexNumber);

                    if (context.PropertyDescriptor.PropertyType == typeof(float))
                        return float.Parse(s);

                    if (context.PropertyDescriptor.PropertyType == typeof(ushort))
                        return ushort.Parse(s, NumberStyles.HexNumber);

                    if (context.PropertyDescriptor.PropertyType == typeof(byte))
                        return byte.Parse(s, NumberStyles.HexNumber);

                    if (context.PropertyDescriptor.PropertyType == typeof(string))
                        return value;
                }
                catch (Exception ex)
                {
                    if (_QuietErrors)
                        return context.PropertyDescriptor.GetValue(context.Instance);
                    else
                        throw ex;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value is uint)
                    return string.Format("0x{0:X8}", value);

                if (value is int)
                    return string.Format("0x{0:X8}", value);

                if (value is ushort)
                    return string.Format("0x{0:X4}", value);

                if (value is short)
                    return string.Format("0x{0:X4}", value);

                if (value is byte)
                    return string.Format("0x{0:X2}", value);

                if (value is sbyte)
                    return string.Format("0x{0:X2}", value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
