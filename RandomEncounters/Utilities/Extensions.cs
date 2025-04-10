using HarmonyLib;

namespace RandomEncounters
{
    internal static class Extensions
    {
        public static T GetPrivateField<T>(this object obj, string field)
        {
            return (T)Traverse.Create(obj).Field(field).GetValue();
        }

        public static void SetPrivateField(this object obj, string field, object value)
        {
            Traverse.Create(obj).Field(field).SetValue(value);
        }

        public static T InvokePrivateMethod<T>(this object obj, string method)
        {
            return Traverse.Create(obj).Method(method).GetValue<T>();
        }

        public static T InvokePrivateMethod<T>(this object obj, string method, params object[] parameters)
        {
            return Traverse.Create(obj).Method(method, parameters).GetValue<T>();
        }
    }
}
