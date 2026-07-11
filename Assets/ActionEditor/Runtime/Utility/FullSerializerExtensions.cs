using System;
using UnityEngine;
using FullSerializer;

namespace PKC.ActionEditor
{
    public class Json
    {
        public static string Serialize(object value,bool isCompressed = false)
        {
            try
            {
                new fsSerializer().TrySerialize(value,out var data).AssertSuccessWithoutWarnings();
                if (isCompressed)
                {
                    return fsJsonPrinter.CompressedJson(data);
                }

                return fsJsonPrinter.PrettyJson(data);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return string.Empty;
            }
        }

        public static object Deserialize(Type type, string serializedState)
        {
            try
            {
                fsData data = fsJsonParser.Parse(serializedState);
                object deserialized = null;
                var ser = new fsSerializer();
                ser.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();
                return deserialized;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}
