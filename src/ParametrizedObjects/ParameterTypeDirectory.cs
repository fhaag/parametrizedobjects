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
using System.Text.RegularExpressions;

namespace ParametrizedObjects
{
	/// <summary>
	/// Maps parameter types to type identifiers and vice-versa.
	/// </summary>
    internal class ParameterTypeDirectory
    {
		/// <summary>
		/// The mapping from known types to their IDs.
		/// </summary>
		private readonly Dictionary<Type, string> type2id = new Dictionary<Type, string>();

		/// <summary>
		/// The mapping from known IDs to their types.
		/// </summary>
		private readonly Dictionary<string, Type> id2type = new Dictionary<string, Type>();

		/// <summary>
		/// A counter whose current value is appended to auto-generated IDs.
		/// </summary>
		private int nextAutoIndex = 1;

		/// <summary>
		/// Retrieves an identifier for a given type and creates one if the type has not been requested before.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>The identifier.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
		public string GetTypeIdentifier(Type type)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			string result;
			if (!type2id.TryGetValue(type, out result))
			{
				result = GenerateIdentifier(type);
				type2id[type] = result;
				id2type[result] = type;
			}
			return result;
		}

		/// <summary>
		/// A regular expression used to extract what is presumably a recognizable abbreviation of the full type name.
		/// </summary>
		private static readonly Regex fullNamePartPattern = new Regex(@"\.([A-Z])");

		/// <summary>
		/// Returns generator functions for somewhat readable identifiers based on the properties of a type.
		/// </summary>
		/// <param name="type">The type.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>An enumeration of generator functions.</returns>
		private static IEnumerable<Func<string>> GetStringGenerators(Type type)
		{
			if (!string.IsNullOrEmpty(type.Name))
			{
				yield return () => type.Name.Substring(0, 1);
				yield return () => string.Join("", type.Name.Where(ch => char.IsUpper(ch)));
				yield return () => string.Join("", fullNamePartPattern.Matches(type.FullName).Cast<Match>().Select(m => m.Groups[1].Value));
			}
		}

		/// <summary>
		/// Generates an idenifier for a given type that is unique in the current instance.
		/// </summary>
		/// <param name="type">The type.
		///   This must not be <see langword="null"/>.</param>
		/// <returns>The generated type identifier.</returns>
		private string GenerateIdentifier(Type type)
		{
			string candidate;

			foreach (var gen in GetStringGenerators(type))
			{
				candidate = gen();
				if (!id2type.ContainsKey(candidate))
				{
					return candidate;
				}
			}

			do
			{
				candidate = "%" + nextAutoIndex.ToString(CultureInfo.InvariantCulture);
				nextAutoIndex++;
			} while (id2type.ContainsKey(candidate));

			return candidate;
		}

		/// <summary>
		/// Retrieves a type based upon its type identifier.
		/// </summary>
		/// <param name="identifier">The type identifier.</param>
		/// <returns>The type, or <see langword="null"/> if <paramref name="identifier"/> is unknown.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="identifier"/> is <see langword="null"/>.</exception>
		public Type GetTypeByIdentifier(string identifier)
		{
			if (identifier == null)
			{
				throw new ArgumentNullException(nameof(identifier));
			}

			Type result;
			id2type.TryGetValue(identifier, out result);
			return result;
		}
    }
}
