using System;
using System.Runtime.InteropServices;

namespace GameSDK.Plugins.YaGames.Extension
{
    public static class YaInterop
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void YaGamesFreeStringPtr(IntPtr ptr);
#else
        private static void YaGamesFreeStringPtr(IntPtr ptr) { }
#endif

        public static string ConsumeUtf8(this IntPtr ptr)
        {
            try
            {
                return ptr != IntPtr.Zero ? Marshal.PtrToStringUTF8(ptr) : null;
            }
            finally
            {
                if (ptr != IntPtr.Zero) YaGamesFreeStringPtr(ptr);
            }
        }

        public static string ConsumeUtf8(this string str) => str;

        public static T WithPtr<T>(this Func<IntPtr> nativeCall, Func<string, T> parse)
        {
            var p = nativeCall();
            try
            {
                var s = p != IntPtr.Zero ? Marshal.PtrToStringUTF8(p) : null;
                return parse(s);
            }
            finally
            {
                if (p != IntPtr.Zero) YaGamesFreeStringPtr(p);
            }
        }
        
        public static T WithPtr<T>(this Func<string> nativeCall, Func<string, T> parse)
        {
            var s = nativeCall();
            return parse(s);
        }
    }
}