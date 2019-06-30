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

namespace ParametrizedObjects
{
	/// <summary>
	/// Stores information about how to store multiple values in a given property by means of a collection object.
	/// </summary>
	/// <remarks>
	/// <para>Instances of this class store information about how to store multiple values in a given property by means of a collection object.
	///   Typically, they will be created for a property that was recognized as a collection property by a <see cref="ICollectionPropertyHandler">collection property handler</see>, and will be populated with the factory and assignment functions returned by that object.</para>
	/// </remarks>
	public sealed class CollectionPropertyAdapter
    {
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="itemType">The type of an individual value in the collection.</param>
		/// <param name="createCollection">A function that creates a collection object that may hold any number of values of <paramref name="itemType"/>, or <see langword="null"/> if this operation is not feasible.</param>
		/// <param name="assignValues">A function that will take any number of values of <paramref name="itemType"/> and store them in an existing collection, or <see langword="null"/> if this operation is not feasible.</param>
		/// <exception cref="ArgumentNullException"><paramref name="itemType"/> is <see langword="null"/>.</exception>
		public CollectionPropertyAdapter(Type itemType, Func<IEnumerable<object>, object> createCollection, Action<IEnumerable<object>, object> assignValues)
		{
			if (itemType == null)
			{
				throw new ArgumentNullException(nameof(itemType));
			}

			this.createCollection = createCollection;
			this.assignValues = assignValues;
			ItemType = itemType;
		}

		/// <summary>
		/// The type of an individual value in the collection.
		/// </summary>
		public Type ItemType { get; }

		/// <summary>
		/// A function that creates a collection object that may hold any number of values, or <see langword="null"/> if this operation is not feasible.
		/// </summary>
		private readonly Func<IEnumerable<object>, object> createCollection;

		/// <summary>
		/// Indicates whether the adapter can create a new collection instance.
		/// </summary>
		public bool CanCreateCollection => createCollection != null;

		/// <summary>
		/// Creates a new collection instance.
		/// </summary>
		/// <param name="values">The values to add to the new collection.</param>
		/// <returns>The newly created collection instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="values"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The collection cannot be created.</exception>
		public object CreateCollection(IEnumerable<object> values)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			if (createCollection == null)
			{
				throw new InvalidOperationException("This adapter cannot create a new collection instance.");
			}

			return createCollection(values);
		}

		/// <summary>
		/// A function that will take any number of values and store them in an existing collection, or <see langword="null"/> if this operation is not feasible.
		/// </summary>
		private readonly Action<IEnumerable<object>, object> assignValues;

		/// <summary>
		/// Indicates whether the adapter can add values to an existing collection instance.
		/// </summary>
		public bool CanAssignValues => assignValues != null;

		/// <summary>
		/// Assigns values to an existing collection instance (after removing any previously contained values).
		/// </summary>
		/// <param name="values">The values to assign.</param>
		/// <param name="collection">The collection instance.</param>
		/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The values cannot be assigned.</exception>
		public void AssignValues(IEnumerable<object> values, object collection)
		{
			if (values == null)
			{
				throw new ArgumentNullException(nameof(values));
			}
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}
			if (assignValues == null)
			{
				throw new InvalidOperationException("This adapter cannot add values to an existing collection instance.");
			}

			assignValues(values, collection);
		}
	}
}
