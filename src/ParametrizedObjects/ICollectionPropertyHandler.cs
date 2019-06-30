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

using System.Reflection;

namespace ParametrizedObjects
{
	/// <summary>
	/// The interface for objects that can determine whether a given property contains a collection of individual values.
	/// </summary>
	/// <remarks>
	/// <para>This interface must be implemented by classes that can determine whether a given property contains a collection of individual values.
	///   Implement it to identify, instantiate, and populate custom collection types.</para>
	/// </remarks>
	public interface ICollectionPropertyHandler
    {
		/// <summary>
		/// Attempts to create a collection adapter for a property.
		/// </summary>
		/// <param name="property">The property.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>If <paramref name="property"/> is recognized as a collection property, an appropriate collection adapter, otherwise <see langword="null"/>.</returns>
		CollectionPropertyAdapter CreateAdapter(PropertyInfo property);
	}
}
