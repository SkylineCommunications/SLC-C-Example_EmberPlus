namespace QAction_1
{
	using System;
	using System.Collections.Generic;
	using global::Skyline.DataMiner.Scripting;

	public static class ProtocolExtensions
	{
		private static readonly Type DateTimeType = typeof(DateTime);

		/// <summary>
		///     Gets a column as dictionary, with the table keys as keys for this dictionary.
		/// </summary>
		/// <typeparam name="TKey">Type of the keys.</typeparam>
		/// <typeparam name="TValue">Type of the Values.</typeparam>
		/// <param name="protocol"><see cref="SLProtocol" /> Instance used to communicate with DataMiner.</param>
		/// <param name="tableId">ID of the Table.</param>
		/// <param name="keyIndex">Index of the table keys column.</param>
		/// <param name="columnIdx">Index of the column to retrieve.</param>
		/// <returns>A dictionary with the desired column.</returns>
		public static Dictionary<TKey, TValue> GetColumnAsDictionary<TKey, TValue>(
			this SLProtocol protocol,
			int tableId,
			uint keyIndex,
			uint columnIdx)
			where TKey : IConvertible
			where TValue : IConvertible
		{
			if (protocol == null)
			{
				throw new ArgumentNullException("protocol");
			}

			var columns = (object[])protocol.NotifyProtocol(321, tableId, new[] { keyIndex, columnIdx });

			var keys = (object[])columns[0];
			var values = (object[])columns[1];

			var retrunValue = new Dictionary<TKey, TValue>();

			for (var i = 0; i < keys.Length; i++)
			{
				retrunValue.Add(
					keys[i].ChangeType<TKey>(),
					values[i].ChangeType<TValue>());
			}

			return retrunValue;
		}

		/// <summary>
		///     Converts an object to the desired type.
		/// </summary>
		/// <typeparam name="T">Type of the result.</typeparam>
		/// <param name="obj">Object to convert.</param>
		/// <returns>The converted object.</returns>
		/// <exception cref="InvalidCastException">
		///     This conversion is not supported. Or <paramref name="obj" /> does not implement
		///     the <see cref="IConvertible" /> interface.
		/// </exception>
		/// <exception cref="FormatException"><paramref name="obj" /> is not in a format rmecognized by conversionType.</exception>
		/// <exception cref="OverflowException">
		///     <paramref name="obj" /> represents a number that is out of the range of
		///     conversionType.
		/// </exception>
		private static T ChangeType<T>(this object obj)
			where T : IConvertible
		{
			if (obj == null)
			{
				return default;
			}

			var type = typeof(T);

			if (type.IsEnum)
			{
				return (T)Enum.ToObject(type, obj.ChangeType<int>());
			}

			if (type == DateTimeType)
			{
				var oadate = Convert.ToDouble(obj);

				if (!oadate.InRange(-657435.0, 2958465.99999999))
				{
					throw new OverflowException($"{obj} is not a valid OA Date, supported range -657435.0 to 2958465.99999999");
				}

				object date = DateTime.FromOADate(oadate);

				return (T)date;
			}

			return (T)Convert.ChangeType(obj, type);
		}

		/// <summary>
		///     Checks if a values is inside an interval.
		/// </summary>
		/// <typeparam name="T">Type of the value.</typeparam>
		/// <param name="value">Value to check.</param>
		/// <param name="fromInclusive">Lower Range, inclusive value.</param>
		/// <param name="toInclusive">High Range, inclusive value.</param>
		/// <returns>True if the value is between the given interval; otherwise false.</returns>
		/// <exception cref="ArgumentNullException">If <paramref name="value" /> is null.</exception>
		private static bool InRange<T>(this T value, T fromInclusive, T toInclusive)
			where T : IComparable
		{
			if (Equals(value, default(T)))
			{
				throw new ArgumentNullException("value");
			}

			return value.CompareTo(fromInclusive) >= 0 && value.CompareTo(toInclusive) <= 0;
		}
	}
}