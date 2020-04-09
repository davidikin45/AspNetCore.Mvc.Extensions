using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace AspNetCore.Mvc.Extensions.Helpers
{
    public static class SerializationHelper
    {
        public static byte[] SerializeToXMLBytes<T>(this T m)
        {
            DataContractSerializer serializer = new DataContractSerializer(m.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, m);
                return ms.ToArray();
            }
        }

        public static byte[] SerializeToBytes<T>(this T m)
        {

            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, m);
                return ms.ToArray();
            }
        }

        public static T DeserializeFromBytes<T>(this byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                var obj = new BinaryFormatter().Deserialize(ms);
                return (T)obj;
            }
        }

        public static String BytesToString(this byte[] byteArray)
        {
            return System.Text.Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
        }

        public static byte[] SerializeToJsonBytes(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var json = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(json);
        }

        public static object DeserializefromJsonBytes(this byte[] arrBytes, Type type)
        {
            var json = Encoding.Default.GetString(arrBytes);
            return JsonConvert.DeserializeObject(json, type);
        }
    }
}
