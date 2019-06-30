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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ParametrizedObjects
{
	/// <summary>
	/// The class that is responsible for instantiating and populating parametrized objects.
	/// </summary>
	/// <typeparam name="T">The base class for all parametrized objects.</typeparam>
    public class ParametrizedObjectsFactory<T>
    {
		/// <summary>
		/// Initializes a new instance.
		/// </summary>
		public ParametrizedObjectsFactory()
		{
			collectionPropertyHandlers = new Lazy<ICollectionPropertyHandler[]>(() => CollectionPropertyHandlers.ToArray());
		}

		/// <summary>
		/// Initializes a new instance and registers eligible types.
		/// </summary>
		/// <param name="eligibleTypes">Optionally, an enumeration of eligible types to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="eligibleTypes"/> contains <see langword="null"/>."</exception>
		/// <exception cref="ArgumentException"><paramref name="eligibleTypes"/> contains a type that is not compatible with <typeparamref name="T"/>.</exception>
		public ParametrizedObjectsFactory(IEnumerable<Type> eligibleTypes) : this()
		{
			if (eligibleTypes != null)
			{
				foreach (var et in eligibleTypes)
				{
					AddEligibleType(et);
				}
			}
		}

		/// <summary>
		/// Stores information about a parametrized type.
		/// </summary>
		private sealed class ParametrizedTypeInfo
		{
			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="owner">The owner instance.</param>
			/// <param name="type">The parametrized type.</param>
			/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
			public ParametrizedTypeInfo(ParametrizedObjectsFactory<T> owner, Type type)
			{
				if (owner == null)
				{
					throw new ArgumentNullException(nameof(owner));
				}
				if (type == null)
				{
					throw new ArgumentNullException(nameof(type));
				}

				Type = type;
				props = type.GetProperties().Select(p => Tuple.Create(p, p.GetCustomAttributes(typeof(ObjectParameterAttribute), false).OfType<ObjectParameterAttribute>().FirstOrDefault())).Where(pInfo => pInfo.Item2 != null).OrderBy(pInfo => pInfo.Item2.Sorting).ToArray();
				expr = owner.GenerateRegex(props, false);
			}

			/// <summary>
			/// The type.
			/// </summary>
			public Type Type { get; }

			/// <summary>
			/// The regular expression that restricts the allowable parameter signature.
			/// </summary>
			private readonly Regex expr;

			/// <summary>
			/// The properties used for parametrization.
			/// </summary>
			/// <seealso cref="Properties"/>
			private readonly Tuple<PropertyInfo, ObjectParameterAttribute>[] props;

			/// <summary>
			/// The properties used for parametrization.
			/// </summary>
			public IReadOnlyList<Tuple<PropertyInfo, ObjectParameterAttribute>> Properties => props;

			/// <summary>
			/// Checks whether a given signature matches the restrictions of the type.
			/// </summary>
			/// <param name="signature">The signature.</param>
			/// <returns>A value that indicates whether the signature fits.</returns>
			public bool FitsSignature(string signature)
			{
				return expr.IsMatch(signature);
			}
		}

		/// <summary>
		/// The list of eligible parametrizable types.
		/// </summary>
		private readonly List<ParametrizedTypeInfo> eligibleTypes = new List<ParametrizedTypeInfo>();

		/// <summary>
		/// Registers a parametrizable type that may be instantiated by the instance.
		/// </summary>
		/// <param name="type">The type to register.</param>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="type"/> is not compatible with <typeparamref name="T"/>.</exception>
		public void AddEligibleType(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (!typeof(T).IsAssignableFrom(type))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
					"Eligible type {0} is not assignable to type {1}.",
					type, typeof(T)));
			}

			eligibleTypes.Add(new ParametrizedTypeInfo(this, type));
		}

		/// <summary>
		/// Registers a parametrizable type that may be instantiated by the instance.
		/// </summary>
		/// <typeparam name="TEligible">The type to register.</typeparam>
		public void AddEligibleType<TEligible>()
			where TEligible : T
		{
			AddEligibleType(typeof(TEligible));
		}

		/// <summary>
		/// Stores some information about all known parameter types.
		/// </summary>
		private readonly ParameterTypeDirectory types = new ParameterTypeDirectory();

		/// <summary>
		/// Generates a string representation of a parameter signature based upon an enumeration of parameter values.
		/// </summary>
		/// <param name="arguments">The parameter values.
		///   This must not be <see langword="null"/></param>
		/// <returns>The string representation of the signature.</returns>
		private string GetArgumentsSignature(IEnumerable<object> arguments)
		{
			return string.Join("", arguments.Select(arg => "@" + types.GetTypeIdentifier(GetArgumentType(arg))));
		}

		/// <summary>
		/// Determines a suitable parametrizable type that is compatible with a given signature.
		/// </summary>
		/// <param name="argSignature">The string representation of the signature to match.</param>
		/// <param name="arguments">The parameter values.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>A type information object of the picked type.</returns>
		/// <exception cref="InvalidOperationException">No single type could be determined for the list of parameters.</exception>
		private ParametrizedTypeInfo PickType(string argSignature, IEnumerable<object> arguments)
		{
			var candidates = eligibleTypes.Where(et => et.FitsSignature(argSignature)).ToArray();
			switch (candidates.Length)
			{
				case 0:
					throw new InvalidOperationException("No suitable type was found.");
				case 1:
					return candidates[0];
				default:
					{
						var candidateIndex = PickType(arguments, candidates.Select(c => c.Type).ToList().AsReadOnly());
						if ((candidateIndex >= 0) && (candidateIndex < candidates.Length))
						{
							return candidates[candidateIndex];
						}
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
							"Ambiguous invocation: {0} suitable types were found.",
							candidates.Length));
					}
			}
		}

		/// <summary>
		/// Picks a parametrizable type out of a set of several eligible types.
		/// </summary>
		/// <param name="arguments">The parameter values.</param>
		/// <param name="suitableTypes">A list of suitable types that are compatible with <paramref name="arguments"/>.</param>
		/// <returns>The zero-based index of the picked type in <paramref name="suitableTypes"/>.</returns>
		/// <remarks>
		/// <para>This method picks a parametrizable type out of a set of several eligible types.
		///   It is only invoked if more than one type have been found to match the signature determined based on the supplied arguments.
		///   If the method cannot determine a single type (by returned an index value that lies outside of the bounds of <paramref name="suitableTypes"/>), an exception will be thrown by the caller.</para>
		/// <para>The default implementation of this method will always return <c>-1</c>.
		///   Override this method with customm logic if you expect situations in which more than one parametrizable type may be eligible.</para>
		/// </remarks>
		protected virtual int PickType(IEnumerable<object> arguments, IReadOnlyList<Type> suitableTypes)
		{
			return -1;
		}

		/// <summary>
		/// Creates a parametrized object instance.
		/// </summary>
		/// <param name="arguments">The parameter values.</param>
		/// <returns>The newly created instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="arguments"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">No parametrized object could be created based on the suppiled arguments.</exception>
		public T Create(IEnumerable<object> arguments)
		{
			return Create(arguments, (object)null, (t, ctx) => (T)Activator.CreateInstance(t));
		}

		/// <summary>
		/// Creates a parametrized object instance by means of a custom factory function.
		/// </summary>
		/// <param name="arguments">The parameter values.</param>
		/// <param name="createFunc">The factory function for the parametrizable object.</param>
		/// <returns>The newly created instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="arguments"/> or <paramref name="createFunc"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">No parametrized object could be created based on the suppiled arguments.</exception>
		public T Create(IEnumerable<object> arguments, Func<Type, T> createFunc)
		{
			if (createFunc == null)
			{
				throw new ArgumentNullException(nameof(createFunc));
			}

			return Create<object>(arguments, null, (t, ctx) => createFunc(t));
		}

		/// <summary>
		/// Creates a parametrized object instance by means of a custom factory function that receives an object with context information.
		/// </summary>
		/// <typeparam name="TContext">The type of a context object that may be passed to the factory function.</typeparam>
		/// <param name="arguments">The parameter values.</param>
		/// <param name="contextInfo">A context object that will be passed to the factory function.</param>
		/// <param name="createFunc">The factory function for the parametrizable object.</param>
		/// <returns>The newly created instance.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="arguments"/> or <paramref name="createFunc"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">No parametrized object could be created based on the suppiled arguments.</exception>
		public T Create<TContext>(IEnumerable<object> arguments, TContext contextInfo, Func<Type, TContext, T> createFunc)
		{
			if (arguments == null)
			{
				throw new ArgumentNullException(nameof(arguments));
			}
			if (createFunc == null)
			{
				throw new ArgumentNullException(nameof(createFunc));
			}

			var argSignature = GetArgumentsSignature(arguments);
			var pti = PickType(argSignature, arguments);
			var result = createFunc(pti.Type, contextInfo);

			using (var argEnumerator = arguments.GetEnumerator())
			{
				AssignArguments(argSignature, argEnumerator, result, pti);
			}

			return result;
		}

		/// <summary>
		/// Assigns parameter values to an instantiated parametrizable object.
		/// </summary>
		/// <param name="argSignature">The string representation of the signature of <paramref name="arguments"/>.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="arguments">The parameter values.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="obj">The parametrizable object instance to populate with parameter values.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="pti">An object with some relevant runtime information about the type of <paramref name="obj"/>.
		///   This must not be <see langword="null"/>.</param>
		private void AssignArguments(string argSignature, IEnumerator<object> arguments, object obj, ParametrizedTypeInfo pti)
		{
			var groupedExpr = GenerateRegex(pti.Properties, true);
			var match = groupedExpr.Match(argSignature);

			if (match.Groups.Count != pti.Properties.Count + 1)
			{
				throw new InvalidOperationException("Matched group count did not match property count.");
			}
			
			for (var i = 0; i < pti.Properties.Count; i++)
			{
				var prop = pti.Properties[i];

				var valCount = match.Groups[i + 1].Captures.Count;
				var values = new List<object>();
				while (valCount > 0)
				{
					arguments.MoveNext();
					values.Add(arguments.Current);

					valCount--;
				}

				AssignPropertyValues(obj, prop.Item1, values);
			}
		}

		/// <summary>
		/// Assigns a set of parameter values to a given property on a parametrizable object.
		/// </summary>
		/// <param name="instance">The parametrizable object instance.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="property">The property to assign to.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="values">The values to assign.
		///   This must not be <see langword="null"/>.</param>
		private void AssignPropertyValues(object instance, PropertyInfo property, IReadOnlyList<object> values)
		{
			var typeInfo = new PropertyTypeInfo(this, property);

			if (property.CanWrite)
			{
				if (typeInfo.CollectionPropertyAdapter != null)
				{
					if (typeInfo.CollectionPropertyAdapter.CanCreateCollection)
					{
						property.SetValue(instance, typeInfo.CollectionPropertyAdapter.CreateCollection(values));
						return;
					}
				}

				if (values.Count > 1)
				{
					throw new InvalidOperationException();
				}
				if (values.Count <= 0)
				{
					if (!property.PropertyType.IsClass)
					{
						throw new InvalidOperationException();
					}

					property.SetValue(instance, null);
					return;
				}

				property.SetValue(instance, values[0]);
				return;
			}
			else
			{
				var propVal = property.GetValue(instance);
				if (propVal != null)
				{
					if (typeInfo.CollectionPropertyAdapter != null)
					{
						if (typeInfo.CollectionPropertyAdapter.CanAssignValues)
						{
							typeInfo.CollectionPropertyAdapter.AssignValues(values, propVal);
							return;
						}
					}
				}
			}

			throw new InvalidOperationException();
		}

		/// <summary>
		/// Retrieves the type of a parameter value.
		/// </summary>
		/// <param name="obj">The parameter value.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The type.</returns>
		/// <remarks>
		/// <para>This method determines the type of a parameter value.
		///   This information will be used to match the signature of an argument list with the accepted signatures of parametrizable object types.</para>
		/// <para>The default implementation will invoke the <see cref="Object.GetType()"/> method to determine the type.
		///   Override this method to manipulate the recognized arguments signature.
		///   It lies within the responsibility of overriders to ensure the actual type of <paramref name="obj"/> is still assignment-compatible to the returned type.</para>
		/// </remarks>
		protected virtual Type GetArgumentType(object obj)
		{
			return obj.GetType();
		}

		/// <summary>
		/// Generates a regular expression that recognizes all argument list signatures that are compatible with the properties of a given parametrizable type.
		/// </summary>
		/// <param name="properties">The auto-populated properties of the parametrizable type.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="withGroups">Indicates whether each property should be enclosed in a separate capturing group.</param>
		/// <returns>The generated regular expression.</returns>
		private Regex GenerateRegex(IReadOnlyList<Tuple<PropertyInfo, ObjectParameterAttribute>> properties, bool withGroups)
		{
			var exprText = new StringBuilder("^");

			foreach (var prop in properties)
			{
				exprText.Append(GetPropertyRegexText(prop.Item1, prop.Item2, withGroups));
			}

			exprText.Append("$");
			return new Regex(exprText.ToString());
		}

		/// <summary>
		/// Generates the regular expression that represents a single property in the <see cref="GenerateRegex"/> method.
		/// </summary>
		/// <param name="prop">The property.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="attr">The attribute applied to the property.
		///   This must not be <see langword="null"/>.</param>
		/// <param name="withGroups">Indicates whether the property should be enclosed in a capturing group.</param>
		/// <returns>The regular expression string.</returns>
		private string GetPropertyRegexText(PropertyInfo prop, ObjectParameterAttribute attr, bool withGroups)
		{
			var propTypeInfo = new PropertyTypeInfo(this, prop);
			var identifier = Regex.Escape("@" + types.GetTypeIdentifier(propTypeInfo.PropertyType));
			if (withGroups)
			{
				identifier = "(" + identifier + ")";
			}
			if (((propTypeInfo.CollectionPropertyAdapter != null) && (attr.MaxCount != 1)) || (attr.MinCount == 0))
			{
				return String.Format(CultureInfo.InvariantCulture,
					@"(?:{0}){{{1},{2}}}",
					identifier, Math.Min(attr.MinCount, attr.MaxCount), attr.MaxCount > 0 ? attr.MaxCount.ToString(CultureInfo.InvariantCulture) : "");
			}
			else
			{
				return identifier;
			}
		}

		/// <summary>
		/// Stores information about an auto-populated property with respect to its type.
		/// </summary>
		private sealed class PropertyTypeInfo
		{
			/// <summary>
			/// Initializes a new instance.
			/// </summary>
			/// <param name="owner">The owner object.</param>
			/// <param name="prop">The property.</param>
			/// <exception cref="ArgumentNullException">Any of the arguments is <see langword="null"/>.</exception>
			public PropertyTypeInfo(ParametrizedObjectsFactory<T> owner, PropertyInfo prop)
			{
				if (owner == null)
				{
					throw new ArgumentNullException(nameof(owner));
				}
				if (prop == null)
				{
					throw new ArgumentNullException(nameof(prop));
				}

				Property = prop;
				CollectionPropertyAdapter = owner.collectionPropertyHandlers.Value.Select(cph => cph.CreateAdapter(prop)).FirstOrDefault(cpa => cpa != null);
			}

			/// <summary>
			/// The property type.
			/// </summary>
			public Type PropertyType => CollectionPropertyAdapter != null ? CollectionPropertyAdapter.ItemType : Property.PropertyType;

			/// <summary>
			/// The property.
			/// </summary>
			public PropertyInfo Property { get; }

			/// <summary>
			/// If this is a collection property, contains an appropriate collection property adapter.
			/// </summary>
			public CollectionPropertyAdapter CollectionPropertyAdapter { get; }
		}

		/// <summary>
		/// Returns the active collection property handlers.
		/// </summary>
		/// <value>
		/// <para>This property returns the active collection property handlers that will be taken into consideration when determining the actual type of an auto-populated property.</para>
		/// <para>The default implementation will only return a fixed set of pre-defined collection property handlers.
		///   Override this method to extend or replace that set.
		///   This method can also be overridden in order to employ an automated discovery scheme for collection property handler types.</para>
		/// </value>
		protected virtual IEnumerable<ICollectionPropertyHandler> CollectionPropertyHandlers
		{
			get
			{
				yield return new DefaultCollectionPropertyHandler();
			}
		}

		/// <summary>
		/// Stores the known set of collection property handlers after initialization of the instance.
		/// </summary>
		private readonly Lazy<ICollectionPropertyHandler[]> collectionPropertyHandlers;
    }
}
