
using System;
using System.Collections;


namespace NAnt.ToDo
{


	public class TokenCollection: CollectionBase
    {
        /// <summary>
        /// The default property for the collection
        /// </summary>
        /// <param name="index">The index of the item to retrieve</param>
        /// <returns>The Schema object associated with the index provided.</returns>
        public Token this[int index]
        {
            get { return List[index] as Token; }
            set { List[index] = value; }
        }
        
        /// <summary>
        /// Adds a value to the collection
        /// </summary>
        /// <param name="value">The value to add to the collection</param>
        /// <returns>The number of the items currently in the collection.</returns>
        public int Add(Token value)
        {
            return List.Add(value);
        }

        /// <summary>
        /// Examines the collection to obtain the index of a given value.
        /// </summary>
        /// <param name="value">The value to locate</param>
        /// <returns>Returns the index of the located item or a -1 if not found.</returns>
        public int IndexOf(Token value)
        {
            return List.IndexOf(value);
        }

        /// <summary>
        /// Insert a value into the collection at a specific index.
        /// </summary>
        /// <param name="index">The index to add the item to the collection.</param>
        /// <param name="value">The item to add to the collection.</param>
        public void Insert(int index, Token value)
        {
            List.Insert(index, value);
        }

        /// <summary>
        /// Removes a specific item from the collection.
        /// </summary>
        /// <param name="value">The value to remove</param>
        public void Remove(Token value)
        {
            List.Remove(value);
        }

        /// <summary>
        /// Checks to see if the value is contained in the collection.
        /// </summary>
        /// <param name="value">The value to identify</param>
        /// <returns>True if the item is found else false.</returns>
        public bool Contains(Token value)
        {
            return List.Contains(value);
        }

        /// <summary>
        /// Valides that the object is of type schema.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        protected override void OnValidate(object value)
        {
            if (value.GetType() != typeof(Token))
            {
                throw new ArgumentException("Value must be of type Schema");
            }
        }
    }

}
