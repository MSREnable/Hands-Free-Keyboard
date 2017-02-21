using System;
using System.Collections.Generic;

namespace Microsoft.HandsFree.Helpers.Telemetry
{
    /// <summary>
    /// Maintain a mapping between name strings id integers.
    /// </summary>
    public static class IdMapper
    {
        readonly static Dictionary<string, int> _nameToId = new Dictionary<string, int>();
        readonly static Dictionary<int, string> _idToName = new Dictionary<int, string>();
        static int _maxId;

        /// <summary>
        /// Obtain an ID for a given name.
        /// </summary>
        /// <param name="name">The name to encode.</param>
        /// <returns>The name as its unique ID.</returns>
        public static int GetId(string name)
        {
            int id;
            if (!_nameToId.TryGetValue(name, out id))
            {
                id = ++_maxId;

                _nameToId.Add(name, id);
                _idToName.Add(id, name);
            }

            return id;
        }

        /// <summary>
        /// Obtain an ID for a given enum type member.
        /// </summary>
        /// <typeparam name="T">The type whose name to encode.</typeparam>
        /// <returns>The type name as its unique ID.</returns>
        public static int GetId<T>(T name)
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException("Generic Type T must be an enum");
            }

            var id = GetId(name.ToString());

            return id;
        }

        /// <summary>
        /// Obtain an ID for a given type name.
        /// </summary>
        /// <typeparam name="T">The type whose name to encode.</typeparam>
        /// <returns>The type name as its unique ID.</returns>
        public static int GetId<T>()
        {
            var type = typeof(T);
            if (!type.IsEnum)
            {
                throw new NotSupportedException("Generic Type T must be an enum");
            }

            var id = GetId(type.Name);

            return id;
        }

        /// <summary>
        /// Get the name corresponding to an ID.
        /// </summary>
        /// <param name="id">The ID to decode.</param>
        /// <returns>The name associated with the ID.</returns>
        public static string GetName(int id)
        {
            string name;
            if (!_idToName.TryGetValue(id, out name))
            {
                name = id.ToString();
            }

            return name;
        }
    }
}
