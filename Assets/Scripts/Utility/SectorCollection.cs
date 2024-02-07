using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using System.Linq;
using System;
using System.Data.Common;

namespace Utility {
    public class SectorCollection<T> : IDictionary<Vector3,List<T>> where T: MonoBehaviour
    {
        private readonly Dictionary<Vector3,List<T>> sectors;

        private readonly Vector3 sectorWidth;

        public SectorCollection(Vector3 sectorWidth, List<T> values = null)
        {
            sectors = new();

            this.sectorWidth = sectorWidth;

            if (values != null){
                foreach (var item in values)
                {
                    Add(item);
                }
            }
        }

        public SectorCollection(float sectorWidth, List<T> values = null) : this(new Vector3(sectorWidth,sectorWidth,sectorWidth), values)
        {}

        public List<T> this[Vector3 key] { 
            get
            {
                return sectors[GetCorrectedKey(key)];
            } 
            set 
            {
                sectors[GetCorrectedKey(key)] = value;
            } 
        }

        /// <summary>
        /// Returns the key that a position fits in.
        /// </summary>
        /// <param name="key">a position</param>
        /// <returns>The corrected key</returns>
        private Vector3 GetCorrectedKey(Vector3 key)
        {
            return new(Mathf.Floor(key.x / sectorWidth.x) * sectorWidth.x, Mathf.Floor(key.y / sectorWidth.y) * sectorWidth.y,Mathf.Floor(key.z / sectorWidth.z) * sectorWidth.z);
        }

        public ICollection<Vector3> Keys => sectors.Keys;

        public ICollection<List<T>> Values => sectors.Values;

        public int Count => sectors.Count;

        public bool IsReadOnly => false;

        /// <summary>
        /// Puts all of the elements in value in their corresponding sectors,
        /// ignoring the key as the position is use as so.
        /// </summary>
        /// <param name="key">Is not used</param>
        /// <param name="value">the monobehaviours to be added to their sectors</param>
        [Obsolete("Use Add(List<T> value) instead.")]
        public void Add(Vector3 key, List<T> value)
        {
            Add(value);
        }
 
        /// <summary>
        /// Puts value in it's corresponding sector,
        /// ignoring the key as the position is used as so.
        /// </summary>
        /// <param name="key">Is not used</param>
        /// <param name="value">the monobehaviour to be added to the sectors</param>
        [Obsolete("Use Add(List<T> value) instead.")]
        public void Add(KeyValuePair<Vector3, List<T>> item)
        {
            Add(item.Value);
        }

        /// <summary>
        /// Puts a monobehavior in it's corresponding sector, using it's WorldPosition as a key.
        /// </summary>
        /// <param name="item">the monobehaviour to be added to the sectors</param>
        public void Add(T item)
        {
            Vector3 key = GetCorrectedKey(item.transform.position);

            if (sectors.ContainsKey(key)){
                sectors[key].Add(item);
            }
            else{
                sectors.Add(key, new List<T>{item});
            }
        }

        /// <summary>
        /// Puts a monobehavior in it's corresponding sector, using it's WorldPosition as a key.
        /// </summary>
        /// <param name="item">the monobehaviour to be added to the sectors</param>
        public void Add(List<T> items)
        {
            items.ForEach(i => Add(i));
        }

        public void Clear()
        {
            sectors.Clear();
        }

        public bool Contains(KeyValuePair<Vector3, List<T>> item)
        {
            return sectors.Contains(item);
        }

        public bool ContainsKey(Vector3 key)
        {
            return sectors.ContainsKey(GetCorrectedKey(key));
        }

        public void CopyTo(KeyValuePair<Vector3, List<T>>[] array, int arrayIndex)
        {
            sectors.ToArray().CopyTo(array,arrayIndex);
        }

        public IEnumerator<KeyValuePair<Vector3, List<T>>> GetEnumerator()
        {
            return sectors.GetEnumerator();
        }

        public bool Remove(Vector3 key)
        {
            return sectors.Remove(GetCorrectedKey(key));
        }

        public bool Remove(T item)
        {
            Vector3 key = GetCorrectedKey(item.transform.position);
            if (sectors.ContainsKey(key))
            {
                if (sectors[key].Count == 1)
                {
                    return sectors.Remove(key);
                }
                else
                {
                    return sectors[key].Remove(item);
                }
            }
            else
            {
                return false;
            }
        }

        public bool Remove(KeyValuePair<Vector3, List<T>> item)
        {
            return sectors.Remove(GetCorrectedKey(item.Key));
        }

        public bool TryGetValue(Vector3 key, out List<T> value)
        {
            return sectors.TryGetValue(GetCorrectedKey(key), out value);
        }

        /// <summary>
        /// Returns all the values in the sectors that are in the radius of the key.
        /// </summary>
        /// <param name="key">the center key from which we take the items</param>
        /// <param name="value">the list in which the items will be added (should be empty)</param>
        /// <param name="radius">the radius from which you take the items, the radius is in sectors</param>
        /// <returns>the items found in the radius of the center key</returns>
        /// <exception cref="ArgumentException">If the radius is not strictly positive</exception>
        public bool TryGetValue(Vector3 key, out List<T> value, int radius)
        {
            return TryGetValue(key, out value, new Vector3(radius,radius,radius));
        }

        /// <summary>
        /// Returns all the values in the sectors that are in the radius of the key.
        /// the radius can be different for x,y,z.
        /// </summary>
        /// <param name="key">the center key from which we take the items</param>
        /// <param name="value">the list in which the items will be added (should be empty)</param>
        /// <param name="radius">the radius from which you take the items, the radius is in sectors</param>
        /// <returns>the items found in the radius of the center key</returns>
        public bool TryGetValue(Vector3 key, out List<T> value, Vector3 radius)
        {
            key = GetCorrectedKey(key);

            List<T> values = new();
            Vector3 currentKey;

            for (int i = 0; i <= radius.x * 2; i++)
            {
                for (int j = 0; j <= radius.y * 2; j++)
                {
                    for (int k = 0; k <= radius.z * 2; k++)
                    { 
                        currentKey = new Vector3(
                            key.x + (i -radius.x) * sectorWidth.x,
                            key.y + (j -radius.y) * sectorWidth.y, 
                            key.z + (k -radius.z) * sectorWidth.z);

                        if (sectors.ContainsKey(currentKey))
                        {
                            values.AddRange(sectors[currentKey]);
                        }
                    }
                }
            }

            value = values;
            return values.Count > 0;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return sectors.GetEnumerator();
        }
    }
}