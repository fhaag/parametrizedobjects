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

namespace ParametrizedObjects
{
	/// <summary>
	/// Marks a property as an auto-populated object parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ObjectParameterAttribute : Attribute
    {
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		/// <param name="sorting">Determines the ordering of the parameter with respect to other parameters.</param>
		public ObjectParameterAttribute(int sorting)
		{
			Sorting = sorting;
		}

		/// <summary>
		/// Determines the ordering of the parameter with respect to other parameters.
		/// </summary>
		/// <value>
		/// <para>Gets a value that determines the ordering of the parameter with respect to other parameters declared within the same class.
		///   The sorting values by different object parameter attributes do not need to form an uninterrupted sequence.
		///   However, uniqueness is adviseable to guarantee a stable sorting order.</para>
		/// </value>
		public int Sorting { get; }

		/// <summary>
		/// The minimum number of values expected.
		/// </summary>
		/// <value>
		/// <para>Gets or sets the minimum number of values expected for the parameter.
		///   For non-collection parameters, this should be either zero or one.</para>
		/// </value>
		public int MinCount { get; set; } = 1;

		/// <summary>
		/// The maximum number of values expected.
		/// </summary>
		/// <value>
		/// <para>Gets or sets the maximum number of values expected for the parameter.
		///   For non-collection parameters, this will be ignored.</para>
		/// </value>
		public int MaxCount { get; set; } = 0;
    }
}
