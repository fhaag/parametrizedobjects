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
using NUnit.Framework;

namespace ParametrizedObjects.Test
{
	[TestFixture]
	public class ParametrizedObjectsFactoryTests
	{
		private ParametrizedObjectsFactory<object> factory;

		[SetUp]
		public void InitTest()
		{
			factory = new ParametrizedObjectsFactory<object>();
		}

		[Test]
		public void ArgumentNumberIsRespected()
		{
			factory.AddEligibleType<Data.ObjectWithParams<int>>();
			factory.AddEligibleType<Data.ObjectWithParams<int, int>>();
			factory.AddEligibleType<Data.ObjectWithParams<int, int, int>>();
			factory.AddEligibleType<Data.ObjectWithParams<int, int, int, int>>();

			Assert.IsInstanceOf<Data.ObjectWithParams<int>>(factory.Create(new object[] { 1 }));
			Assert.IsInstanceOf<Data.ObjectWithParams<int, int>>(factory.Create(new object[] { 9, 30 }));
			Assert.IsInstanceOf<Data.ObjectWithParams<int, int, int>>(factory.Create(new object[] { 1, 3, 15 }));
			Assert.IsInstanceOf<Data.ObjectWithParams<int, int, int, int>>(factory.Create(new object[] { 399, 28, 18, 399 }));
		}

		[Test]
		public void ArgumentTypesAreRespected()
		{
			factory.AddEligibleType<Data.ObjectWithParams<int, int>>();
			factory.AddEligibleType<Data.ObjectWithParams<int, string>>();
			factory.AddEligibleType<Data.ObjectWithParams<Guid, string>>();

			Assert.IsInstanceOf<Data.ObjectWithParams<int, int>>(factory.Create(new object[] { 3, 15 }));
			Assert.IsInstanceOf<Data.ObjectWithParams<int, string>>(factory.Create(new object[] { 3, "test" }));
			Assert.IsInstanceOf<Data.ObjectWithParams<Guid, string>>(factory.Create(new object[] { Guid.Empty, "" }));
		}

		[Test]
		public void OptionalArgumentsMayBeSkipped()
		{
			factory.AddEligibleType<Data.ObjectWithOptionalParam<Data.A, Data.B, Data.C>>();

			var args = new object[] { new Data.A(), new Data.B(), new Data.C() };

			var obj = factory.Create(new object[] { args[0], args[1], args[2] });
			Assert.IsInstanceOf<Data.ObjectWithOptionalParam<Data.A, Data.B, Data.C>>(obj);
			var typedObj = (Data.ObjectWithOptionalParam<Data.A, Data.B, Data.C>)obj;
			Assert.AreSame(args[0], typedObj.Value1);
			Assert.AreSame(args[1], typedObj.Value2);
			Assert.AreSame(args[2], typedObj.Value3);

			obj = factory.Create(new object[] { args[0], args[2] });
			Assert.IsInstanceOf<Data.ObjectWithOptionalParam<Data.A, Data.B, Data.C>>(obj);
			typedObj = (Data.ObjectWithOptionalParam<Data.A, Data.B, Data.C>)obj;
			Assert.AreSame(args[0], typedObj.Value1);
			Assert.IsNull(typedObj.Value2);
			Assert.AreSame(args[2], typedObj.Value3);
		}

		[Test]
		public void ArgumentsAreAssigned()
		{
			var a = new Data.A();
			var b = new Data.B();
			var c = new Data.C();

			factory.AddEligibleType<Data.ObjectWithParams<Data.A, Data.B, Data.C>>();

			var obj = factory.Create(new object[] { a, b, c });
			Assert.IsInstanceOf<Data.ObjectWithParams<Data.A, Data.B, Data.C>>(obj);

			var typedObj = (Data.ObjectWithParams<Data.A, Data.B, Data.C>)obj;
			Assert.AreSame(a, typedObj.Value1);
			Assert.AreSame(b, typedObj.Value2);
			Assert.AreSame(c, typedObj.Value3);
		}

		[Test]
		public void ArgumentOrderIsRespected()
		{
			factory.AddEligibleType<Data.ObjectWithParams<Data.A, Data.A, Data.A>>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A() };
			var obj = (Data.ObjectWithParams<Data.A, Data.A, Data.A>)factory.Create(args);
			Assert.AreSame(args[0], obj.Value1);
			Assert.AreSame(args[1], obj.Value2);
			Assert.AreSame(args[2], obj.Value3);
		}

		private MethodInfo GetGenericEnumerableMethod(string methodName, Type enumerableType)
		{
			var m = GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
			return m.MakeGenericMethod(enumerableType);
		}

		private void TypedArrayIsCreated<TEnumerable>()
			where TEnumerable : IEnumerable<Data.A>
		{
			factory.AddEligibleType<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam>();
			factory.AddEligibleType<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam<Data.B>>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A(), new Data.A() };
			var obj = factory.Create(args);
			Assert.IsInstanceOf<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam>(obj);
			Assert.IsNotNull(((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam)obj).Value1);
			Assert.AreEqual(args.Length, ((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam)obj).Value1.Count());
			CollectionAssert.AreEqual(args, ((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithArrayParam)obj).Value1);
		}

		[Test]
		public void ArrayIsCreated()
		{
			GetGenericEnumerableMethod(nameof(TypedArrayIsCreated), typeof(Data.A).MakeArrayType()).Invoke(this, new object[0]);
		}

		[Test]
		public void ArrayIsCreatedForGenericPropertyType([Values(typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>))] Type t)
		{
			GetGenericEnumerableMethod(nameof(TypedArrayIsCreated), t.MakeGenericType(typeof(Data.A))).Invoke(this, new object[0]);
		}

		private void TypedMinCountIsEnforced<TEnumerable>()
			where TEnumerable : IEnumerable<Data.A>
		{
			factory.AddEligibleType<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMinConstrainedArrayParam>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A(), new Data.A() };

			var obj = factory.Create(args);
			Assert.IsInstanceOf<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMinConstrainedArrayParam>(obj);
			Assert.IsNotNull(((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMinConstrainedArrayParam)obj).Value1);
			CollectionAssert.AreEqual(args, ((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMinConstrainedArrayParam)obj).Value1);

			args = args.Take(2).ToArray();
			Assert.Throws<InvalidOperationException>(() => factory.Create(args));
		}

		[Test]
		public void MinCountIsEnforced()
		{
			GetGenericEnumerableMethod(nameof(TypedMinCountIsEnforced), typeof(Data.A).MakeArrayType()).Invoke(this, new object[0]);
		}

		[Test]
		public void MinCountIsEnforcedForGenericPropertyType([Values(typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>))] Type t)
		{
			GetGenericEnumerableMethod(nameof(TypedMinCountIsEnforced), t.MakeGenericType(typeof(Data.A))).Invoke(this, new object[0]);
		}

		private void TypedMaxCountIsEnforced<TEnumerable>()
			where TEnumerable : IEnumerable<Data.A>
		{
			factory.AddEligibleType<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMaxConstrainedArrayParam>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A() };

			var obj = factory.Create(args);
			Assert.IsInstanceOf<Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMaxConstrainedArrayParam>(obj);
			Assert.IsNotNull(((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMaxConstrainedArrayParam)obj).Value1);
			CollectionAssert.AreEqual(args, ((Data.WithEnumerable<TEnumerable, Data.A>.ObjectWithMaxConstrainedArrayParam)obj).Value1);

			args = args.Concat(args).ToArray();
			Assert.Throws<InvalidOperationException>(() => factory.Create(args));
		}

		[Test]
		public void MaxCountIsEnforced()
		{
			GetGenericEnumerableMethod(nameof(TypedMaxCountIsEnforced), typeof(Data.A).MakeArrayType()).Invoke(this, new object[0]);
		}

		[Test]
		public void MaxCountIsEnforcedForGenericPropertyType([Values(typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>))] Type t)
		{
			GetGenericEnumerableMethod(nameof(TypedMaxCountIsEnforced), t.MakeGenericType(typeof(Data.A))).Invoke(this, new object[0]);
		}

		[Test]
		public void MultipleArraysAreFilled()
		{
			factory.AddEligibleType<Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoArrayParams>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A(), new Data.A() };

			Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoArrayParams obj;

			Assert.Throws<InvalidOperationException>(() => factory.Create(new object[0]));
			Assert.Throws<InvalidOperationException>(() => factory.Create(args.Take(1).ToArray()));

			obj = (Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoArrayParams)factory.Create(args.Take(2).ToArray());
			CollectionAssert.IsEmpty(obj.Value1);
			CollectionAssert.AreEqual(args.Take(2), obj.Value2);

			obj = (Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoArrayParams)factory.Create(args.Take(3).ToArray());
			CollectionAssert.AreEqual(args.Take(1), obj.Value1);
			CollectionAssert.AreEqual(args.Skip(1).Take(2), obj.Value2);

			obj = (Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoArrayParams)factory.Create(args.Take(4).ToArray());
			CollectionAssert.AreEqual(args.Take(2), obj.Value1);
			CollectionAssert.AreEqual(args.Skip(2).Take(2), obj.Value2);
		}

		[Test]
		public void MultipleMandatoryArraysAreFilled()
		{
			factory.AddEligibleType<Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoMandatoryArrayParams>();

			var args = new object[] { new Data.A(), new Data.A(), new Data.A(), new Data.A() };

			Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoMandatoryArrayParams obj;

			Assert.Throws<InvalidOperationException>(() => factory.Create(new object[0]));
			Assert.Throws<InvalidOperationException>(() => factory.Create(args.Take(1).ToArray()));
			Assert.Throws<InvalidOperationException>(() => factory.Create(args.Take(2).ToArray()));

			obj = (Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoMandatoryArrayParams)factory.Create(args.Take(3).ToArray());
			CollectionAssert.AreEqual(args.Take(1), obj.Value1);
			CollectionAssert.AreEqual(args.Skip(1).Take(2), obj.Value2);

			obj = (Data.WithEnumerable<List<Data.A>, Data.A>.ObjectWithTwoMandatoryArrayParams)factory.Create(args.Take(4).ToArray());
			CollectionAssert.AreEqual(args.Take(2), obj.Value1);
			CollectionAssert.AreEqual(args.Skip(2).Take(2), obj.Value2);
		}
	}
}
