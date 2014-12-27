using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;

namespace AngularJSAuthRefreshToken.Web
{
    public static class ProtectData
    {
        public static MemoryStream SerializeToStream(object o)
        {
            var stream = new MemoryStream();
            (new BinaryFormatter()).Serialize(stream, o);
            stream.Position = 0L;
            return stream;
        }

        public static byte[] SerializeToArray(object o)
        {
            return SerializeToStream(o).ToArray();
        }

        public static T DeserializeFromStream<T>(MemoryStream stream)
            where T : class, new()
        {
            stream.Seek(0, SeekOrigin.Begin);
            return (new BinaryFormatter()).Deserialize(stream) as T;
        }

        public static T DeserializeFromStream<T>(byte[] stream)
            where T : class, new()
        {
            return DeserializeFromStream<T>(new MemoryStream(stream));
        }

        public static byte[] ProtectObject(object value)
        {
            if (value == null)
            {
                return null;
            }

            var userData = SerializeToArray(value);

            return MachineKey.Protect(userData);
        }

        public static string ProtectObjectToUrlToken(object value)
        {
            var protectedData = ProtectObject(value);
            if (protectedData == null)
            {
                return null;
            }
            return HttpServerUtility.UrlTokenEncode(protectedData);
        }

        public static T UnprotectObjectFromUrlToken<T>(string value)
            where T : class, new()
        {
            if (value == null)
            {
                return default(T);
            }

            var rawData = HttpServerUtility.UrlTokenDecode(value);

            return UnprotectObject<T>(rawData);
        }

        public static T UnprotectObject<T>(byte[] value)
            where T : class, new()
        {
            if (value == null)
            {
                return default(T);
            }

            try
            {
                var unprotectData = MachineKey.Unprotect(value);

                return DeserializeFromStream<T>(unprotectData);
            }
            catch (CryptographicException)
            {
                return default(T);
            }
        }

        public static byte[] ProtectString(string value)
        {
            if (value == null)
            {
                return null;
            }

            var userData = Encoding.Unicode.GetBytes(value);

            return MachineKey.Protect(userData);
        }

        public static string ProtectStringToUrlToken(string value)
        {
            var protectedData = ProtectString(value);
            if (protectedData == null)
            {
                return null;
            }
            return HttpServerUtility.UrlTokenEncode(protectedData);
        }

        public static string UnprotectString(byte[] value)
        {
            if (value == null)
            {
                return null;
            }

            try
            {
                var unprotectedData = MachineKey.Unprotect(value);

                return Encoding.Unicode.GetString(unprotectedData);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        public static string UnprotectFromUrlToken(string value)
        {
            if (value == null)
            {
                return null;
            }

            var rawData = HttpServerUtility.UrlTokenDecode(value);

            return UnprotectString(rawData);
        }

    }
}