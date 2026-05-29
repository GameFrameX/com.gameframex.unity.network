using System;

namespace GameFrameX.Network.Runtime
{
    public static class MessageSerializerRegistry
    {
        private static IMessageSerializer _global;

        public static IMessageSerializer Global
        {
            get { return _global ?? DefaultMessageSerializer.Instance; }
        }

        public static void RegisterGlobal(IMessageSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            _global = serializer;
        }

        public static void Reset()
        {
            _global = null;
        }
    }
}
