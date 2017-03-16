using System;
using System.ComponentModel;

namespace Turbina.Engine
{
    public static class ValueConverter
    {
        public static TTo Convert<TFrom, TTo>(TFrom item)
        {
            if (item == null)
            {
                return default(TTo);
            }
            if (typeof(TTo) == typeof(object))
            {
                return (TTo)(object)item;
            }
            var converterTo = TypeDescriptor.GetConverter(typeof(TTo));
            if (converterTo.CanConvertFrom(typeof(TFrom)))
            {
                return (TTo)converterTo.ConvertFrom(item);
            }

            if (typeof(TTo) == typeof(TimeSpan))
            {
                var converterFrom = TypeDescriptor.GetConverter(typeof(TFrom));
                if (converterFrom.CanConvertTo(typeof(double)))
                {
                    var interval = (double)converterFrom.ConvertTo(item, typeof(double));
                    return (TTo)(object)TimeSpan.FromSeconds(interval);
                }
            }

            try
            {
                return (TTo)System.Convert.ChangeType(item, typeof(TTo));
            }
            catch
            {
                if (converterTo.CanConvertFrom(typeof(string)))
                {
                    return (TTo)converterTo.ConvertFrom(item.ToString());
                }
                throw;
            }
        }
    }
}