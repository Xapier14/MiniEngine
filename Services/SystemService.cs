using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MiniEngine.Utility;
using MiniEngine.Collections;

namespace MiniEngine
{
    public static class SystemService
    {
        private static readonly SystemList _systemList = new();

        internal static void LoadSystems()
        {
        }

        public static void RegisterBefore<T>(System system) where T : System
        {
            if (typeof(T) == typeof(void))
            {
                _systemList.Add(system);
            }
            _systemList.FindSystemNodeBySystemType<T>()?.InsertBefore(new SystemNode
            {
                Value = system
            });
        }

        public static void RegisterAfter<T>(System system) where T : System
        {
            if (typeof(T) == typeof(void))
            {
                _systemList.Add(system);
            }
            _systemList.FindSystemNodeBySystemType<T>()?.InsertAfter(new SystemNode
            {
                Value = system
            });
        }

        public static void RegisterComponentInstance(Component component)
        {

        }

        public static void PurgeComponents()
        {

        }
    }
}
