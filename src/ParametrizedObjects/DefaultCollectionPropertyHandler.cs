/*
MIT License

Copyright (c) 2019 Florian Haag

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ParametrizedObjects
{
	/// <summary>
	/// Recognizes some basic collection types and creates appropriate adapter objects.
	/// </summary>
    public class DefaultCollectionPropertyHandler : ICollectionPropertyHandler
    {
		/// <summary>
		/// A helper class to create strongly-typed generic collection objects.
		/// </summary>
		/// <typeparam name="T">The collection element type.</typeparam>
		private sealed class CollectionCreator<T>
		{
			/// <summary>
			/// Creates an array of type <typeparamref name="T"/> with a given set of elements.
			/// </summary>
			/// <param name="values">The elements.
			///   This must not be <see langword="null"/>.</param>
			/// <returns>The newly created array.</returns>
			public T[] CreateArray(IEnumerable<object> values)
			{
				return values.Cast<T>().ToArray();
			}

			/// <summary>
			/// Creates a <see cref="List{T}"/> instance for type <typeparamref name="T"/> with a given set of elements.
			/// </summary>
			/// <param name="values">The elements.
			///   This must not be <see langword="null"/>.</param>
			/// <returns>The newly created list object.</returns>
			public List<T> CreateGenericList(IEnumerable<object> values)
			{
				return new List<T>(values.Cast<T>());
			}
		}

		/// <summary>
		/// Creates a populated collection object of a given element type.
		/// </summary>
		/// <param name="itemType">The element type.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="methodName">The name of the method of <see cref="CollectionCreator{T}"/> to invoke.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="values">The elements.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The newly created collection.</returns>
		private object CreateCollection(Type itemType, string methodName, IEnumerable<object> values)
		{
			var creatorType = typeof(CollectionCreator<>).MakeGenericType(itemType);
			var creator = Activator.CreateInstance(creatorType);
			var creationMethod = creatorType.GetMethod(methodName);
			return creationMethod.Invoke(creator, new object[] { values });
		}

		/// <summary>
		/// Attempts to create a collection adapter for a property.
		/// </summary>
		/// <param name="property">The property.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>If <paramref name="property"/> is recognized as a collection property, an appropriate collection adapter, otherwise <see langword="null"/>.</returns>
		public CollectionPropertyAdapter CreateAdapter(PropertyInfo property)
		{
			if (property.PropertyType.IsArray)
			{
				return new CollectionPropertyAdapter(property.PropertyType.GetElementType(),
					values => CreateCollection(property.PropertyType.GetElementType(), nameof(CollectionCreator<object>.CreateArray), values),
					null);
			}

			if (property.PropertyType.IsGenericType)
			{
				var typeDef = property.PropertyType.GetGenericTypeDefinition();
				if ((typeDef == typeof(IEnumerable<>))
					|| (typeDef == typeof(IReadOnlyCollection<>))
					|| (typeDef == typeof(IReadOnlyList<>)))
				{
					return new CollectionPropertyAdapter(property.PropertyType.GetGenericArguments()[0],
						values => CreateCollection(property.PropertyType.GetGenericArguments()[0], nameof(CollectionCreator<object>.CreateArray), values),
						null);
				}

				if ((typeDef == typeof(ICollection<>))
					|| (typeDef == typeof(IList<>)))
				{
					return new CollectionPropertyAdapter(property.PropertyType.GetGenericArguments()[0],
						values => CreateCollection(property.PropertyType.GetGenericArguments()[0], nameof(CollectionCreator<object>.CreateArray), values),
						(values, container) =>
						{
							var clearMethod = property.PropertyType.GetMethod(nameof(ICollection<object>.Clear));
							var addMethod = property.PropertyType.GetMethod(nameof(ICollection<object>.Add));
							clearMethod.Invoke(container, new object[0]);
							foreach (var v in values)
							{
								addMethod.Invoke(container, new[] { v });
							}
						});
				}

				if (typeDef == typeof(List<>))
				{
					var itemType = property.PropertyType.GetGenericArguments()[0];
					return new CollectionPropertyAdapter(itemType,
						values => CreateCollection(itemType, nameof(CollectionCreator<object>.CreateGenericList), values),
						(values, container) =>
						{
							var clearMethod = property.PropertyType.GetMethod(nameof(List<object>.Clear));
							var addRangeMethod = property.PropertyType.GetMethod(nameof(List<object>.AddRange));
							var castMethod = typeof(Enumerable).GetMethod(nameof(Enumerable.Cast));
							clearMethod.Invoke(container, new object[0]);
							addRangeMethod.Invoke(container, new object[] { castMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { values }) });
						});
				}
			}

			if ((property.PropertyType == typeof(System.Collections.IEnumerable))
				|| (property.PropertyType == typeof(System.Collections.ICollection))
				|| (property.PropertyType == typeof(System.Collections.IList)))
			{
				return new CollectionPropertyAdapter(typeof(object),
					values => CreateCollection(typeof(object), nameof(CollectionCreator<object>.CreateArray), values),
					null);
			}

			return null;
		}
	}
}
