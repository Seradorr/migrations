using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SimpleJSON;

namespace Migrations {
    public class JSONOps<T> {
        public static string Encode(Object o) {
            Dictionary<string, Object> dict = new Dictionary<string, object>();

            foreach (FieldInfo fi in o.GetType().GetFields()) {
                dict.Add(fi.Name, fi.GetValue(o));
            }

            return JSONEncoder.Encode(dict);
        }

        public static T Decode(string j, int limit = 99) {
            JObject jo = JSONDecoder.Decode(j);
            return Decode(jo, limit);
        }

        public static T Decode(JObject jo, int limit = 99) {
            T o = Activator.CreateInstance<T>();

            foreach (FieldInfo fi in o.GetType().GetFields()) {
                fi.SetValue(o, DecodeRecursive(jo[fi.Name], limit - 1));
            }

            return o;
        }

        private static Object DecodeRecursive(JObject jo, int limit) {
            if (limit != 0) switch (jo.Kind) {
                case JObjectKind.Object:
                    Dictionary<string, Object> d = new Dictionary<string, Object>();

                    foreach (KeyValuePair<string, JObject> kvp in jo.ObjectValue) {
                        d.Add(kvp.Key, DecodeRecursive(kvp.Value, limit - 1));
                    }

                    return d;

                case JObjectKind.Array:
                    List<Object> l = new List<Object>(jo.ArrayValue.Count);

                    foreach (JObject joo in jo.ArrayValue) {
                        l.Add(DecodeRecursive(joo, limit - 1));
                    }

                    return l;

                case JObjectKind.String:
                    return jo.StringValue;

                case JObjectKind.Number:
                    return jo.IsFractional ? jo.FloatValue : jo.IntValue;

                case JObjectKind.Boolean:
                    return jo.BooleanValue;

                case JObjectKind.Null:
                    return null;

                default:
                    return null;
            }

            else return null;
        }
    }
}
