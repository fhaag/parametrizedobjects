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

using System.Collections.Generic;

namespace ParametrizedObjects.Test.Data
{
	public class ObjectWithParams
	{
	}

	public class ObjectWithParams<T1>
	{
		[ObjectParameter(0)]
		public T1 Value1 { get; set; }
	}

	public class ObjectWithParams<T1, T2>
	{
		[ObjectParameter(0)]
		public T1 Value1 { get; set; }

		[ObjectParameter(1)]
		public T2 Value2 { get; set; }
	}

	public class ObjectWithParams<T1, T2, T3>
	{
		[ObjectParameter(2)]
		public T1 Value1 { get; set; }

		[ObjectParameter(3)]
		public T2 Value2 { get; set; }

		[ObjectParameter(4)]
		public T3 Value3 { get; set; }
	}

	public class ObjectWithParams<T1, T2, T3, T4>
	{
		[ObjectParameter(0)]
		public T1 Value1 { get; set; }

		[ObjectParameter(1)]
		public T2 Value2 { get; set; }

		[ObjectParameter(8)]
		public T3 Value3 { get; set; }

		[ObjectParameter(100)]
		public T4 Value4 { get; set; }
	}

	public class ObjectWithOptionalParam<T1, T2, T3>
	{
		[ObjectParameter(0)]
		public T1 Value1 { get; set; }

		[ObjectParameter(1, MinCount = 0)]
		public T2 Value2 { get; set; }

		[ObjectParameter(2)]
		public T3 Value3 { get; set; }
	}

	public static class WithEnumerable<TEnumerable, T>
		where TEnumerable : IEnumerable<T>
	{
		public class ObjectWithArrayParam
		{
			[ObjectParameter(10, MaxCount = 100)]
			public TEnumerable Value1 { get; set; }
		}

		public class ObjectWithArrayParam<T2>
		{
			[ObjectParameter(5, MaxCount = 100)]
			public TEnumerable Value1 { get; set; }

			[ObjectParameter(20)]
			public T2 Value2 { get; set; }
		}

		public class ObjectWithMinConstrainedArrayParam
		{
			[ObjectParameter(1, MinCount = 3, MaxCount = 100)]
			public TEnumerable Value1 { get; set; }
		}

		public class ObjectWithMaxConstrainedArrayParam
		{
			[ObjectParameter(1, MaxCount = 4)]
			public TEnumerable Value1 { get; set; }
		}

		public class ObjectWithTwoArrayParams
		{
			[ObjectParameter(1, MinCount = 0, MaxCount = 10)]
			public TEnumerable Value1 { get; set; }

			[ObjectParameter(2, MinCount = 2, MaxCount = 10)]
			public TEnumerable Value2 { get; set; }
		}

		public class ObjectWithTwoMandatoryArrayParams
		{
			[ObjectParameter(1, MinCount = 1, MaxCount = 10)]
			public TEnumerable Value1 { get; set; }

			[ObjectParameter(2, MinCount = 2, MaxCount = 10)]
			public TEnumerable Value2 { get; set; }
		}
	}
}
