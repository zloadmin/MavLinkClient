using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace MavLinkNet
{
    /// <summary>
    /// Печать UasMessage через reflection + UasMessageMetadata
    /// </summary>
    public static class UasMessagePrinter
    {
        public static void Print(UasMessage msg)
        {
            if (msg == null)
            {
                Console.WriteLine("<null UasMessage>");
                return;
            }

            Console.WriteLine("==================================================");
            Console.WriteLine("MAVLINK MESSAGE");
            Console.WriteLine($"  Type       : {msg.GetType().Name}");
            Console.WriteLine($"  Message ID : {msg.MessageId}");
            Console.WriteLine($"  System ID  : {msg.SystemId}");
            Console.WriteLine($"  Component  : {msg.ComponentId}");
            Console.WriteLine("--------------------------------------------------");

            if (msg.GetMetadata() == null || msg.GetMetadata().Fields == null)
            {
                Console.WriteLine("No metadata available");
                Console.WriteLine("==================================================");
                return;
            }

            Type msgType = msg.GetType();

            foreach (var fieldMeta in msg.GetMetadata().Fields)
            {
                PrintField(msg, msgType, fieldMeta);
            }

            Console.WriteLine("==================================================\n");
        }

        private static void PrintField(
            UasMessage msg,
            Type msgType,
            UasFieldMetadata meta)
        {
            // Ищем public field или property по имени
            MemberInfo member =
                (MemberInfo)msgType.GetField(meta.Name, BindingFlags.Public | BindingFlags.Instance)
                ?? msgType.GetProperty(meta.Name, BindingFlags.Public | BindingFlags.Instance);

            if (member == null)
            {
                Console.WriteLine($"{meta.Name}: <field not found>");
                return;
            }

            object value = GetValue(msg, member);

            string header = meta.Name;
            if (!string.IsNullOrEmpty(meta.Description))
                header += $" — {meta.Description}";

            Console.WriteLine(header);

            if (value == null)
            {
                Console.WriteLine("    value: <null>");
                return;
            }

            // ===== ARRAY =====
            if (value is Array arr)
            {
                Console.WriteLine($"    value[{arr.Length}]: {FormatArray(arr)}");
                return;
            }

            // ===== ENUM =====
            if (meta.EnumMetadata != null)
            {
                PrintEnum(meta.EnumMetadata, value);
                return;
            }

            // ===== SCALAR =====
            Console.WriteLine($"    value: {value}");
        }

        private static object GetValue(UasMessage msg, MemberInfo member)
        {
            if (member is FieldInfo f)
                return f.GetValue(msg);

            if (member is PropertyInfo p)
                return p.GetValue(msg);

            return null;
        }

        private static void PrintEnum(UasEnumMetadata enumMeta, object value)
        {
            int intValue;

            try
            {
                intValue = Convert.ToInt32(value);
            }
            catch
            {
                Console.WriteLine($"    value: {value} (enum conversion failed)");
                return;
            }

            Console.WriteLine($"    value: {intValue}");

            var entry = enumMeta.Entries?
                .FirstOrDefault(e => e.Value == intValue);

            if (entry == null)
            {
                Console.WriteLine($"    enum : {enumMeta.Name}.<UNKNOWN>");
                return;
            }

            Console.WriteLine($"    enum : {enumMeta.Name}.{entry.Name}");

            if (!string.IsNullOrEmpty(entry.Description))
                Console.WriteLine($"    desc : {entry.Description}");

            if (entry.Params != null && entry.Params.Count > 0)
            {
                Console.WriteLine("    params:");
                foreach (string p in entry.Params)
                    Console.WriteLine($"      - {p}");
            }
        }

        private static string FormatArray(Array arr)
        {
            return "[" + string.Join(", ", arr.Cast<object>()) + "]";
        }
    }
}
