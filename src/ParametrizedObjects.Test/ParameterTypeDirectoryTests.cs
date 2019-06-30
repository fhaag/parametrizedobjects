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
using NUnit.Framework;

namespace ParametrizedObjects.Test
{
	[TestFixture]
	public class ParameterTypeDirectoryTests
	{
		private ParameterTypeDirectory typeDir;

		private static readonly Type[] types = new[]
		{
			typeof(Data.A), typeof(Data.B), typeof(Data.C), typeof(Data.D),
			typeof(System.String), typeof(System.Int32), typeof(System.Globalization.CultureInfo)
		};

		[SetUp]
		public void InitTest()
		{
			typeDir = new ParameterTypeDirectory();
		}

		[Test]
		public void IdentifiersAreUnique()
		{
			var id1 = typeDir.GetTypeIdentifier(typeof(Data.A));
			var id2 = typeDir.GetTypeIdentifier(typeof(Data.B));
			var id3 = typeDir.GetTypeIdentifier(typeof(Data.Sub.A));

			CollectionAssert.AllItemsAreUnique(new[] { id1, id2, id3 });
		}

		[Test]
		public void TypeCanBeRetrieved()
		{
			var typesWithIds = new List<Tuple<Type, string>>();
			foreach (var t in types)
			{
				var id = typeDir.GetTypeIdentifier(t);
				typesWithIds.Add(Tuple.Create(t, id));
			}

			foreach (var pair in typesWithIds)
			{
				Assert.AreEqual(pair.Item1, typeDir.GetTypeByIdentifier(pair.Item2));
			}
		}

		[Test]
		public void TypeIdRemainsSaved()
		{
			var typesWithIds = new List<Tuple<Type, string>>();
			foreach (var t in types)
			{
				var id = typeDir.GetTypeIdentifier(t);
				typesWithIds.Add(Tuple.Create(t, id));
			}

			foreach (var pair in typesWithIds)
			{
				Assert.AreEqual(pair.Item2, typeDir.GetTypeIdentifier(pair.Item1));
			}
		}

		[Test]
		public void UnknownTypeYieldsNull()
		{
			var usedIdentifiers = new HashSet<string>();
			foreach (var t in types)
			{
				usedIdentifiers.Add(typeDir.GetTypeIdentifier(t));
			}

			string unusedId;
			int idx = 0;
			do
			{
				unusedId = "t" + idx++;
			} while (usedIdentifiers.Contains(unusedId));

			Assert.IsNull(typeDir.GetTypeByIdentifier(unusedId));
		}
	}
}
