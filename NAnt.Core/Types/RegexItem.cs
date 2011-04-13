using System;
using System.Collections;
using System.Reflection;

using NAnt.Core;
using NAnt.Core.Attributes;
using NAnt.Core.Types;

namespace NAnt.Babel.Types
{
	/// <summary>
	/// Summary description for RegexItem.
	/// </summary>
	[ElementName("regex")]
	public class RegexItem : Element
	{
		public string _value;

		[TaskAttribute("value")]
		[StringValidator(AllowEmpty = false)]
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public RegexItem()
		{
		}
	}

	[Serializable]
	public class RegexItemCollection : CollectionBase
	{
		public RegexItem this[int index]
		{
			get
			{
				return (RegexItem)base.List[index];
			}
			set
			{
				base.List[index] = value;
			}
		}

		public RegexItemCollection()
		{
		}

		public RegexItemCollection(RegexItemCollection value)
		{
			this.AddRange(value);
		}

		public RegexItemCollection(RegexItem[] value)
		{
			this.AddRange(value);
		}

		public int Add(RegexItem item)
		{
			return base.List.Add(item);
		}

		public void AddRange(RegexItem[] items)
		{
			for (int i = 0; i < items.Length; i++)
			{
				this.Add(items[i]);
			}
		}

		public void AddRange(RegexItemCollection items)
		{
			for (int i = 0; i < items.Count; i++)
			{
				this.Add(items[i]);
			}
		}

		public bool Contains(RegexItem item)
		{
			return base.List.Contains(item);
		}

		public void CopyTo(RegexItem[] array, int index)
		{
			base.List.CopyTo(array, index);
		}

		public int IndexOf(RegexItem item)
		{
			return base.List.IndexOf(item);
		}

		public void Insert(int index, RegexItem item)
		{
			base.List.Insert(index, item);
		}

		public void Remove(RegexItem item)
		{
			base.List.Remove(item);
		}

		public new RegexItemEnumerator GetEnumerator()
		{
			return new RegexItemEnumerator(this);
		}
	}

	public class RegexItemEnumerator : IEnumerator
	{
		private IEnumerator _baseEnumerator;

		internal RegexItemEnumerator(RegexItemCollection arguments)
		{
			IEnumerable enumerable = arguments;
			_baseEnumerator = enumerable.GetEnumerator();
		}

		public RegexItem Current
		{
			get
			{
				return (RegexItem)_baseEnumerator.Current;
			}
		}

		public bool MoveNext()
		{
			return _baseEnumerator.MoveNext();
		}

		public void Reset()
		{
			_baseEnumerator.Reset();
		}

		object IEnumerator.Current
		{
			get { return _baseEnumerator.Current; }
		}
	}
}
