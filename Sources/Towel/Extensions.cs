﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Towel.DataStructures;
using static Towel.Statics;

namespace Towel
{
	/// <summary>Contains Extension methods on common System types.</summary>
	public static class Extensions
	{
		#region System.String

		#region Replace + ReplaceCached (multiple replace)

		/// <summary>Returns a new <see cref="string"/> in which all occurrences of Unicode <see cref="string"/> patterns in this instance are replaced with a relative Unicode <see cref="string"/> replacements.</summary>
		/// <remarks>Uses Regex without a timeout.</remarks>
		/// <param name="this">The <see cref="string"/> to perform the replacements on.</param>
		/// <param name="rules">The patterns and relative replacements to apply to this <see cref="string"/>.</param>
		/// <returns>A new <see cref="string"/> in which all occurrences of Unicode <see cref="string"/> patterns in this instance are replaced with a relative Unicode <see cref="string"/> replacements.</returns>
		/// <exception cref="ArgumentNullException">Thrown if any of the parameters are null or contain null values.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="rules"/> is empty, <paramref name="rules"/> contains empty patterns, or <paramref name="rules"/> contains duplicate patterns.</exception>
		public static string Replace(this string @this, params (string Pattern, string Replacement)[] rules) => ReplaceBase(@this, rules, false);

		/// <summary>Returns a new <see cref="string"/> in which all occurrences of Unicode <see cref="string"/> patterns in this instance are replaced with a relative Unicode <see cref="string"/> replacements. Caches internal values relative to the instance of rules.</summary>
		/// <remarks>Uses Regex without a timeout. This method is not thread-safe.</remarks>
		/// <param name="this">The <see cref="string"/> to perform the replacements on.</param>
		/// <param name="rules">The patterns and relative replacements to apply to this <see cref="string"/>.</param>
		/// <returns>A new <see cref="string"/> in which all occurrences of Unicode <see cref="string"/> patterns in this instance are replaced with a relative Unicode <see cref="string"/> replacements.</returns>
		/// <exception cref="ArgumentNullException">Thrown if any of the parameters are null or contain null values.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="rules"/> is empty, <paramref name="rules"/> contains empty patterns, or <paramref name="rules"/> contains duplicate patterns.</exception>
		public static string ReplaceCached(this string @this, params (string Pattern, string Replacement)[] rules) => ReplaceBase(@this, rules, true);

		/// <summary>Cache for the <see cref="ReplaceCached"/> method.</summary>
		private readonly static System.Collections.Generic.Dictionary<object, (Regex Regex, System.Collections.Generic.Dictionary<string, string> RuleSet)> ReplaceCachedCache =
			new System.Collections.Generic.Dictionary<object, (Regex Regex, System.Collections.Generic.Dictionary<string, string> RuleSet)>();

		private static string ReplaceBase(string @this, (string Pattern, string Replacement)[] rules, bool cached)
		{
			if (@this is null) throw new ArgumentNullException(nameof(@this));
			if (rules is null) throw new ArgumentNullException(nameof(rules));
			if (rules.Length <= 0) throw new ArgumentException($"{nameof(rules)}.{nameof(rules.Length)} <= 0");
			if (!cached || !ReplaceCachedCache.TryGetValue(rules, out (Regex Regex, System.Collections.Generic.Dictionary<string, string> RuleSet) regexAndRuleSet))
			{
				regexAndRuleSet.RuleSet = new System.Collections.Generic.Dictionary<string, string>(rules.Length);
				foreach (var (Pattern, Replacement) in rules)
				{
					if (Pattern is null) throw new ArgumentNullException(nameof(rules), $"{nameof(rules)} contains null {nameof(Pattern)}s");
					if (Replacement is null) throw new ArgumentNullException(nameof(rules), $"{nameof(rules)} contains null {nameof(Replacement)}s");
					if (Pattern == string.Empty) throw new ArgumentException(nameof(rules), $"{nameof(rules)} contains empty {nameof(Pattern)}s");
					// TODO: TryAdd when available
					if (regexAndRuleSet.RuleSet.ContainsKey(Pattern))
					{
						throw new ArgumentException($"{nameof(rules)} contains duplicate {nameof(Pattern)}s");
					}
					else
					{
						regexAndRuleSet.RuleSet.Add(Pattern, Replacement);
					}
				}
				string regexPattern = string.Join("|", System.Linq.Enumerable.Select(rules, rule => Regex.Escape(rule.Pattern)));
				RegexOptions regexOptions = cached ? RegexOptions.Compiled : RegexOptions.None;

				// NOTE:
				// Regex has a matchTimeout parameter that prevents security threats of long-running
				// regex patterns. However, an overload with a TimeSpan parameter likely is not required as
				// this impementation is not allowing special syntax in the regex. It is only flat strings
				// that are OR-ed together.

				regexAndRuleSet.Regex = new Regex(regexPattern, regexOptions);
				if (cached)
				{
					ReplaceCachedCache.Add(rules, regexAndRuleSet);
				}
			}
			return regexAndRuleSet.Regex.Replace(@this, match => regexAndRuleSet.RuleSet[match.Value]);
		}

		/// <summary>Clears the cache for the <see cref="ReplaceCached"/> method.</summary>
		public static void ClearReplaceCache() => ReplaceCachedCache.Clear();

		/// <summary>Removes a rule set from the <see cref="ReplaceCached"/> method cache if it exists.</summary>
		/// <param name="rules">The rule set to remove from the cache.</param>
		public static void RemoveFromReplaceCache((string Pattern, string Replacement)[] rules)
		{
			if (ReplaceCachedCache.ContainsKey(rules))
			{
				ReplaceCachedCache.Remove(rules);
			}
		}

		#endregion

		/// <summary>Checks if a string contains any of a collections on characters.</summary>
		/// <param name="string">The string to see if it contains any of the specified characters.</param>
		/// <param name="chars">The characters to check if the string contains any of them.</param>
		/// <returns>True if the string contains any of the provided characters. False if not.</returns>
		public static bool ContainsAny(this string @string, params char[] chars)
		{
			_ = chars ?? throw new ArgumentNullException(nameof(chars));
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			if (chars.Length < 1)
			{
				throw new InvalidOperationException("Attempting a contains check with an empty set.");
			}

			ISet<char> set = new SetHashLinked<char>();
			foreach (char c in chars)
			{
				set.Add(c);
			}
			foreach (char c in @string)
			{
				if (set.Contains(c))
				{
					return true;
				}
			}
			return false;
		}

		internal static string RemoveCarriageReturns(this string @string) =>
			@string.Replace("\r", string.Empty);

		/// <summary>Removes carriage returns and then replaces all new line characters with System.Environment.NewLine.</summary>
		/// <param name="string">The string to standardize the new lines of.</param>
		/// <returns>The new line standardized string.</returns>
		internal static string StandardizeNewLines(this string @string) =>
			@string.RemoveCarriageReturns().Replace("\n", Environment.NewLine);

		/// <summary>Creates a string of a repreated string a provided number of times.</summary>
		/// <param name="string">The string to repeat.</param>
		/// <param name="count">The number of repetitions of the string to repeat.</param>
		/// <returns>The string of the repeated string to repeat.</returns>
		public static string Repeat(this string @string, int count)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), count, "!(" + nameof(count) + " >= 0)");
			}
			string[] sets = new string[count];
			for (int i = 0; i < count; i++)
			{
				sets[i] = @string;
			}
			return string.Concat(sets);
		}

		/// <summary>Splits the string into the individual lines.</summary>
		/// <param name="string">The string to get the lines of.</param>
		/// <returns>an array of the individual lines of the string.</returns>
		public static string[] SplitLines(this string @string)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			return @string.RemoveCarriageReturns().Split('\n');
		}

		/// <summary>Indents every line in a string with a single tab character.</summary>
		/// /// <param name="string">The string to indent the lines of.</param>
		/// <returns>The indented string.</returns>
		public static string IndentLines(this string @string) =>
			PadLinesLeft(@string, "\t");

		/// <summary>Indents every line in a string with a given number of tab characters.</summary>
		/// <param name="string">The string to indent the lines of.</param>
		/// <param name="count">The number of tabs of the indention.</param>
		/// <returns>The indented string.</returns>
		public static string IndentLines(this string @string, int count) =>
			PadLinesLeft(@string, new string('\t', count));

		/// <summary>Indents after every new line sequence found between two string indeces.</summary>
		/// <param name="string">The string to be indented.</param>
		/// <param name="start">The starting index to look for new line sequences to indent.</param>
		/// <param name="end">The starting index to look for new line sequences to indent.</param>
		/// <returns>The indented string.</returns>
		public static string IndentNewLinesBetweenIndeces(this string @string, int start, int end) =>
			PadLinesLeftBetweenIndeces(@string, "\t", start, end);

		/// <summary>Indents after every new line sequence found between two string indeces.</summary>
		/// <param name="string">The string to be indented.</param>
		/// <param name="count">The number of tabs of this indention.</param>
		/// <param name="start">The starting index to look for new line sequences to indent.</param>
		/// <param name="end">The starting index to look for new line sequences to indent.</param>
		/// <returns>The indented string.</returns>
		public static string IndentNewLinesBetweenIndeces(this string @string, int count, int start, int end) =>
			PadLinesLeftBetweenIndeces(@string, new string('\t', count), start, end);

		/// <summary>Indents a range of line numbers in a string.</summary>
		/// <param name="string">The string to indent specified lines of.</param>
		/// <param name="startingLineNumber">The line number to start line indention on.</param>
		/// <param name="endingLineNumber">The line number to stop line indention on.</param>
		/// <returns>The string with the specified lines indented.</returns>
		public static string IndentLineNumbers(this string @string, int startingLineNumber, int endingLineNumber) =>
			PadLinesLeft(@string, "\t", startingLineNumber, endingLineNumber);

		/// <summary>Indents a range of line numbers in a string.</summary>
		/// <param name="string">The string to indent specified lines of.</param>
		/// <param name="count">The number of tabs for the indention.</param>
		/// <param name="startingLineNumber">The line number to start line indention on.</param>
		/// <param name="endingLineNumber">The line number to stop line indention on.</param>
		/// <returns>The string with the specified lines indented.</returns>
		public static string IndentLineNumbers(this string @string, int count, int startingLineNumber, int endingLineNumber) =>
			PadLinesLeft(@string, new string('\t', count), startingLineNumber, endingLineNumber);

		/// <summary>Adds a string onto the beginning of every line in a string.</summary>
		/// <param name="string">The string to pad.</param>
		/// <param name="padding">The padding to add to the front of every line.</param>
		/// <returns>The padded string.</returns>
		public static string PadLinesLeft(this string @string, string padding)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			if (padding.CompareTo(string.Empty) == 0)
			{
				return @string;
			}
			return string.Concat(padding, @string.RemoveCarriageReturns().Replace("\n", Environment.NewLine + padding));
		}

		/// <summary>Adds a string onto the end of every line in a string.</summary>
		/// <param name="string">The string to pad.</param>
		/// <param name="padding">The padding to add to the front of every line.</param>
		/// <returns>The padded string.</returns>
		public static string PadSubstringLinesRight(this string @string, string padding)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			if (padding.CompareTo(string.Empty) == 0)
			{
				return @string;
			}
			return string.Concat(@string.RemoveCarriageReturns().Replace("\n", padding + Environment.NewLine), padding);
		}

		/// <summary>Adds a string after every new line squence found between two indeces of a string.</summary>
		/// <param name="string">The string to be padded.</param>
		/// <param name="padding">The padding to apply after every newline sequence found.</param>
		/// <param name="start">The starting index of the string to search for new line sequences.</param>
		/// <param name="end">The ending index of the string to search for new line sequences.</param>
		/// <returns>The padded string.</returns>
		public static string PadLinesLeftBetweenIndeces(this string @string, string padding, int start, int end)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			if (start < 0 || start >= @string.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(start), start, "!(0 <= " + nameof(start) + " <= " + nameof(@string) + "." + nameof(@string.Length) + ")");
			}
			if (end >= @string.Length || end < start)
			{
				throw new ArgumentOutOfRangeException(nameof(end), end, "!(" + nameof(start) + " <= " + nameof(end) + " <= " + nameof(@string) + "." + nameof(@string.Length) + ")");
			}
			string header = @string.Substring(0, start).StandardizeNewLines();
			string body = string.Concat(@string[start..end].RemoveCarriageReturns().Replace("\n", Environment.NewLine + padding));
			string footer = @string[end..].StandardizeNewLines();
			return string.Concat(header, body, footer);
		}

		/// <summary>Adds a string before every new line squence found between two indeces of a string.</summary>
		/// <param name="string">The string to be padded.</param>
		/// <param name="padding">The padding to apply before every newline sequence found.</param>
		/// <param name="start">The starting index of the string to search for new line sequences.</param>
		/// <param name="end">The ending index of the string to search for new line sequences.</param>
		/// <returns>The padded string.</returns>
		public static string PadLinesRightBetweenIndeces(this string @string, string padding, int start, int end)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			if (start < 0 || start >= @string.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(start), start, "!(0 <= " + nameof(start) + " <= " + nameof(@string) + "." + nameof(@string.Length) + ")");
			}
			if (end >= @string.Length || end < start)
			{
				throw new ArgumentOutOfRangeException(nameof(end), end, "!(" + nameof(start) + " <= " + nameof(end) + " <= " + nameof(@string) + "." + nameof(@string.Length) + ")");
			}
			string header = @string.Substring(0, start).StandardizeNewLines();
			string body = string.Concat(@string[start..end].RemoveCarriageReturns().Replace("\n", padding + Environment.NewLine));
			string footer = @string[end..].StandardizeNewLines();
			return string.Concat(header, body, footer);
		}

		/// <summary>Adds a string after every new line squence found between two indeces of a string.</summary>
		/// <param name="string">The string to be padded.</param>
		/// <param name="padding">The padding to apply after every newline sequence found.</param>
		/// <param name="startingLineNumber">The starting index of the line in the string to pad.</param>
		/// <param name="endingLineNumber">The ending index of the line in the string to pad.</param>
		/// <returns>The padded string.</returns>
		public static string PadLinesLeft(this string @string, string padding, int startingLineNumber, int endingLineNumber)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			string[] lines = @string.SplitLines();
			if (startingLineNumber < 0 || startingLineNumber >= lines.Length)
			{
				throw new ArgumentOutOfRangeException("startingLineNumber");
			}
			if (endingLineNumber >= lines.Length || endingLineNumber < startingLineNumber)
			{
				throw new ArgumentOutOfRangeException("endingLineNumber");
			}
			for (int i = startingLineNumber; i <= endingLineNumber; i++)
			{
				lines[i] = padding + lines[i];
			}
			return string.Concat(lines);
		}

		/// <summary>Adds a string before every new line squence found between two indeces of a string.</summary>
		/// <param name="string">The string to be padded.</param>
		/// <param name="padding">The padding to apply before every newline sequence found.</param>
		/// <param name="startingLineNumber">The starting index of the line in the string to pad.</param>
		/// <param name="endingLineNumber">The ending index of the line in the string to pad.</param>
		/// <returns>The padded string.</returns>
		public static string PadLinesRight(this string @string, string padding, int startingLineNumber, int endingLineNumber)
		{
			_ = @string ?? throw new ArgumentNullException(nameof(@string));
			_ = padding ?? throw new ArgumentNullException(nameof(padding));
			string[] lines = @string.SplitLines();
			if (startingLineNumber < 0 || startingLineNumber >= lines.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(startingLineNumber), startingLineNumber, "!(0 <= " + nameof(startingLineNumber) + " < [Line Count])");
			}
			if (endingLineNumber >= lines.Length || endingLineNumber < startingLineNumber)
			{
				throw new ArgumentOutOfRangeException(nameof(endingLineNumber), endingLineNumber, "!(" + nameof(startingLineNumber) + " < " + nameof(endingLineNumber) + " < [Line Count])");
			}
			for (int i = startingLineNumber; i <= endingLineNumber; i++)
			{
				lines[i] += padding;
			}
			return string.Concat(lines);
		}

		/// <summary>Reverses the characters in a string.</summary>
		/// <param name="string">The string to reverse the characters of.</param>
		/// <returns>The reversed character string.</returns>
		public static string Reverse(this string @string)
		{
			char[] characters = @string.ToCharArray();
			Array.Reverse(characters);
			return new string(characters);
		}

		/// <summary>Removes all the characters from a string based on a predicate.</summary>
		/// <param name="string">The string to remove characters from.</param>
		/// <param name="where">The predicate determining removal of each character.</param>
		/// <returns>The string after removing any predicated characters.</returns>
		public static string Remove(this string @string, Predicate<char> where)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (char c in @string)
			{
				if (!where(c))
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		/// <summary>Counts the number of lines in the string.</summary>
		/// <param name="str">The string to get the line count of.</param>
		/// <returns>The number of lines in the string.</returns>
		public static int CountLines(this string str) =>
			Regex.Matches(str.StandardizeNewLines(), Environment.NewLine).Count + 1;

		#endregion

		#region System.Random

		#region int

		internal static CompareResult ToCompareResult(this int result) =>
			result < 0
				? Less
				: result > 0
					? Greater
					: Equal;

		#endregion

		#region bool

		/// <summary>Generates a random <see cref="bool"/> value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated <see cref="bool"/> value.</returns>
		public static bool NextBool(this Random random) => random.Next(2) == 0;

		#endregion

		#region byte

		/// <summary>Generates a random <see cref="byte"/> value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated <see cref="byte"/> value.</returns>
		public static byte NextByte(this Random random) => (byte)random.Next(byte.MinValue, byte.MaxValue);

		#endregion

		#region string

		/// <summary>Generates a random <see cref="string"/> of a given length using the System.Random generator.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized <see cref="string"/>.</returns>
		public static string NextString(this Random random, int length)
		{
			_ = random ?? throw new ArgumentNullException(nameof(random));
			if (length < 1)
			{
				throw new ArgumentException("(" + nameof(length) + " < 1)");
			}
			char[] randomstring = new char[length];
			for (int i = 0; i < randomstring.Length; i++)
			{
				randomstring[i] = (char)random.Next(char.MinValue, char.MaxValue);
			}
			return new string(randomstring);
		}

		/// <summary>Generates a random <see cref="string"/> of a given length using the System.Random generator with a specific set of characters.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized <see cref="string"/> to generate.</param>
		/// <param name="characterPool">The set of allowable characters.</param>
		/// <returns>The generated randomized <see cref="string"/>.</returns>
		public static string NextString(this Random random, int length, char[] characterPool)
		{
			_ = random ?? throw new ArgumentNullException(nameof(random));
			_ = characterPool ?? throw new ArgumentNullException(nameof(characterPool));
			if (length < 1)
			{
				throw new ArgumentException("(" + nameof(length) + " < 1)");
			}
			if (characterPool.Length < 1)
			{
				throw new ArgumentException("(" + nameof(characterPool) + "." + nameof(characterPool.Length) + " < 1)");
			}
			char[] randomstring = new char[length];
			for (int i = 0; i < randomstring.Length; i++)
			{
				randomstring[i] = random.Choose(characterPool);
			}
			return new string(randomstring);
		}

		internal const string UpperCaseEnglishCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		internal const string LowerCaseEnglishCharacters = "abcdefghijklmnopqrstuvwxyz";
		internal const string EnglishDigits = "0123456789";

		/// <summary>Generates a random English alphanumeric <see cref="string"/> of a given length (includes upper and lower case characters).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphanumeric <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphanumeric <see cref="string"/>.</returns>
		public static string NextEnglishAlphaNumericString(this Random random, int length) =>
			NextString(random, length, (UpperCaseEnglishCharacters + LowerCaseEnglishCharacters + EnglishDigits).ToCharArray());

		/// <summary>Generates a random English alphanumeric <see cref="string"/> of a given length (upper case characters only).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphanumeric <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphanumeric <see cref="string"/>.</returns>
		public static string NextUpperCaseEnglishAlphaNumericString(this Random random, int length) =>
			NextString(random, length, (UpperCaseEnglishCharacters + EnglishDigits).ToCharArray());

		/// <summary>Generates a random English alphanumeric <see cref="string"/> of a given length (lower case characters only).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphanumeric <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphanumeric <see cref="string"/>.</returns>
		public static string NextLowerCaseEnglishAlphaNumericString(this Random random, int length) =>
			NextString(random, length, (LowerCaseEnglishCharacters + EnglishDigits).ToCharArray());

		/// <summary>Generates a random English numeric <see cref="string"/> of a given length.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized numeric <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized numeric <see cref="string"/>.</returns>
		public static string NumericEnglishString(this Random random, int length) =>
			NextString(random, length, EnglishDigits.ToCharArray());

		/// <summary>Generates a random English alhpabetical <see cref="string"/> of a given length (includes upper and lower case characters).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphabetical <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphabetical <see cref="string"/>.</returns>
		public static string NextEnglishAlphabeticString(this Random random, int length) =>
			NextString(random, length, (UpperCaseEnglishCharacters + LowerCaseEnglishCharacters).ToCharArray());

		/// <summary>Generates a random English alhpabetical <see cref="string"/> of a given length (upper case characters only).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphabetical <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphabetical <see cref="string"/>.</returns>
		public static string NextUpperCaseEnglishAlphabeticString(this Random random, int length) =>
			NextString(random, length, (UpperCaseEnglishCharacters).ToCharArray());

		/// <summary>Generates a random English alhpabetical <see cref="string"/> of a given length (lower case characters only).</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="length">The length of the randomized alphabetical <see cref="string"/> to generate.</param>
		/// <returns>The generated randomized alphabetical <see cref="string"/>.</returns>
		public static string NextLowerCaseEnglishAlphabeticString(this Random random, int length) =>
			NextString(random, length, (LowerCaseEnglishCharacters).ToCharArray());

		#endregion

		#region char

		/// <summary>Generates a random char value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated char value.</returns>
		public static char NextChar(this Random random) =>
			NextChar(random, char.MinValue, char.MaxValue);

		/// <summary>Generates a random char value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="min">Minimum allowed value of the random generation.</param>
		/// <param name="max">Maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated char value.</returns>
		public static char NextChar(this Random random, char min, char max) =>
			(char)random.Next(min, max);

		#endregion

		#region long

		/// <summary>Generates a random long value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated long value.</returns>
		public static long NextLong(this Random random) =>
			NextLong(random, long.MaxValue);

		/// <summary>Generates a random long value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="max">Maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated long value.</returns>
		public static long NextLong(this Random random, long max) =>
			NextLong(random, 0, max);

		/// <summary>Generates a random long value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="min">Minimum allowed value of the random generation.</param>
		/// <param name="max">Maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated long value.</returns>
		public static long NextLong(this Random random, long min, long max)
		{
			_ = random ?? throw new ArgumentNullException(nameof(random));
			if (min > max)
			{
				throw new ArgumentException("!(" + nameof(min) + " <= " + nameof(max) + ")");
			}
			byte[] buffer = new byte[8];
			random.NextBytes(buffer);
			long longRand = BitConverter.ToInt64(buffer, 0);
			return Math.Abs(longRand % (max - min)) + min;
		}

		#endregion

		#region decimal

		/// <summary>Generates a random <see cref="decimal"/> value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated <see cref="decimal"/> value.</returns>
		public static decimal NextDecimal(this Random random) =>
			new decimal(
				random.Next(int.MinValue, int.MaxValue),
				random.Next(int.MinValue, int.MaxValue),
				random.Next(int.MinValue, int.MaxValue),
				random.NextBool(),
				(byte)random.Next(29));

		/// <summary>Generates a random <see cref="decimal"/> value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="min">The minimum allowed value of the random generation.</param>
		/// <param name="max">The maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated <see cref="decimal"/> value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Called if the range of random generation is too large and therefore invalid.</exception>
		public static decimal NextDecimal(this Random random, decimal min, decimal max)
		{
			try
			{
				decimal next = NextDecimal(random);
				return checked((next < 0 ? -next : next) % (max - min) + min);
			}
			catch (OverflowException exception)
			{
				throw new ArgumentOutOfRangeException("The bounds of random generation were to large (min...max).", exception);
			}
		}

		#endregion

		#region DateTime

		/// <summary>Generates a random DateTime value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated DateTime value.</returns>
		public static DateTime NextDateTime(this Random random) =>
			NextDateTime(random, DateTime.MaxValue);

		/// <summary>Generates a random DateTime value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="max">The maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated DateTime value.</returns>
		public static DateTime NextDateTime(this Random random, DateTime max) =>
			NextDateTime(random, DateTime.MinValue, max);

		/// <summary>Generates a random DateTime value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="min">The minimum allowed value of the random generation.</param>
		/// <param name="max">The maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated DateTime value.</returns>
		public static DateTime NextDateTime(this Random random, DateTime min, DateTime max)
		{
			_ = random ?? throw new ArgumentNullException(nameof(random));
			if (min > max)
			{
				throw new ArgumentException("!(" + nameof(min) + " <= " + nameof(max) + ")");
			}
			TimeSpan randomTimeSpan = NextTimeSpan(random, TimeSpan.Zero, max - min);
			return min.Add(randomTimeSpan);
		}

		#endregion

		#region TimeSpan

		/// <summary>Generates a random TimeSpan value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <returns>A randomly generated TimeSpan value.</returns>
		public static TimeSpan NextTimeSpan(this Random random) =>
			NextTimeSpan(random, TimeSpan.MaxValue);

		/// <summary>Generates a random TimeSpan value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="max">The maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated TimeSpan value.</returns>
		public static TimeSpan NextTimeSpan(this Random random, TimeSpan max) =>
			NextTimeSpan(random, TimeSpan.Zero, max);

		/// <summary>Generates a random TimeSpan value.</summary>
		/// <param name="random">The random generation algorithm.</param>
		/// <param name="min">The minimum allowed value of the random generation.</param>
		/// <param name="max">The maximum allowed value of the random generation.</param>
		/// <returns>A randomly generated TimeSpan value.</returns>
		public static TimeSpan NextTimeSpan(this Random random, TimeSpan min, TimeSpan max)
		{
			_ = random ?? throw new ArgumentNullException(nameof(random));
			if (min > max)
			{
				throw new ArgumentException("!(" + nameof(min) + " <= " + nameof(max) + ")");
			}
			long tickRange = max.Ticks - min.Ticks;
			long randomLong = random.NextLong(0, tickRange);
			return TimeSpan.FromTicks(min.Ticks + randomLong);
		}

		#endregion

		#region NextUnique

		/// <summary>
		/// Generates <paramref name="count"/> unique random <see cref="int"/> values in the
		/// [<paramref name="minValue"/>..<paramref name="maxValue"/>] range where <paramref name="minValue"/> is
		/// inclusive and <paramref name="maxValue"/> is exclusive.
		/// </summary>
		/// <typeparam name="Step">The function to perform on each generated <see cref="int"/> value.</typeparam>
		/// <param name="random">The random to generation algorithm.</param>
		/// <param name="count">The number of <see cref="int"/> values to generate.</param>
		/// <param name="minValue">Inclusive endpoint of the random generation range.</param>
		/// <param name="maxValue">Exclusive endpoint of the random generation range.</param>
		/// <param name="step">The function to perform on each generated <see cref="int"/> value.</param>
		public static void NextUnique<Step>(this Random random, int count, int minValue, int maxValue, Step step = default)
			where Step : struct, IAction<int> =>
			NextUnique<Step, RandomNextIntMinValueIntMaxValue>(count, minValue, maxValue, random ?? new Random(), step);

		/// <summary>
		/// Generates <paramref name="count"/> unique random <see cref="int"/> values in the
		/// [<paramref name="minValue"/>..<paramref name="maxValue"/>] range where <paramref name="minValue"/> is
		/// inclusive and <paramref name="maxValue"/> is exclusive.
		/// </summary>
		/// <typeparam name="Step">The function to perform on each generated <see cref="int"/> value.</typeparam>
		/// <param name="random">The random to generation algorithm.</param>
		/// <param name="count">The number of <see cref="int"/> values to generate.</param>
		/// <param name="minValue">Inclusive endpoint of the random generation range.</param>
		/// <param name="maxValue">Exclusive endpoint of the random generation range.</param>
		/// <param name="step">The function to perform on each generated <see cref="int"/> value.</param>
		[Obsolete("It is recommended you use " + nameof(NextUnique) + " method instead.", false)]
		public static void NextUniqueRollTracking<Step>(this Random random, int count, int minValue, int maxValue, Step step = default)
			where Step : struct, IAction<int> =>
			NextUniqueRollTracking<Step, RandomNextIntMinValueIntMaxValue>(count, minValue, maxValue, random ?? new Random(), step);

		/// <summary>
		/// Generates <paramref name="count"/> unique random <see cref="int"/> values in the
		/// [<paramref name="minValue"/>..<paramref name="maxValue"/>] range where <paramref name="minValue"/> is
		/// inclusive and <paramref name="maxValue"/> is exclusive.
		/// </summary>
		/// <typeparam name="Step">The function to perform on each generated <see cref="int"/> value.</typeparam>
		/// <param name="random">The random to generation algorithm.</param>
		/// <param name="count">The number of <see cref="int"/> values to generate.</param>
		/// <param name="minValue">Inclusive endpoint of the random generation range.</param>
		/// <param name="maxValue">Exclusive endpoint of the random generation range.</param>
		/// <param name="step">The function to perform on each generated <see cref="int"/> value.</param>
		[Obsolete("It is recommended you use " + nameof(NextUnique) + " method instead.", false)]
		public static void NextUniquePoolTracking<Step>(this Random random, int count, int minValue, int maxValue, Step step = default)
			where Step : struct, IAction<int> =>
			NextUniquePoolTracking<Step, RandomNextIntMinValueIntMaxValue>(count, minValue, maxValue, random ?? new Random(), step);

		/// <summary>
		/// Generates <paramref name="count"/> unique random <see cref="int"/> values in the
		/// [<paramref name="minValue"/>..<paramref name="maxValue"/>] range where <paramref name="minValue"/> is
		/// inclusive and <paramref name="maxValue"/> is exclusive.
		/// </summary>
		/// <param name="random">The random to generation algorithm.</param>
		/// <param name="count">The number of <see cref="int"/> values to generate.</param>
		/// <param name="minValue">Inclusive endpoint of the random generation range.</param>
		/// <param name="maxValue">Exclusive endpoint of the random generation range.</param>
		/// <param name="step">The function to perform on each generated <see cref="int"/> value.</param>
		public static void NextUnique(this Random random, int count, int minValue, int maxValue, Action<int> step)
		{
			if (step is null)
				throw new ArgumentNullException(nameof(step));
			NextUnique<ActionRuntime<int>>(random, count, minValue, maxValue, step);
		}

		/// <summary>
		/// Generates <paramref name="count"/> unique random <see cref="int"/> values in the
		/// [<paramref name="minValue"/>..<paramref name="maxValue"/>] range where <paramref name="minValue"/> is
		/// inclusive and <paramref name="maxValue"/> is exclusive.
		/// </summary>
		/// <param name="random">The random to generation algorithm.</param>
		/// <param name="count">The number of <see cref="int"/> values to generate.</param>
		/// <param name="minValue">Inclusive endpoint of the random generation range.</param>
		/// <param name="maxValue">Exclusive endpoint of the random generation range.</param>
		public static int[] NextUnique(this Random random, int count, int minValue, int maxValue) =>
			Statics.NextUnique<RandomNextIntMinValueIntMaxValue>(count, minValue, maxValue, random ?? new Random());

		#endregion

		#region Choose

		/// <summary>Chooses an item at random (all equally weighted).</summary>
		/// <typeparam name="T">The generic type of the items to choose from.</typeparam>
		/// <param name="random">The random algorithm for index generation.</param>
		/// <param name="values">The values to choose from.</param>
		/// <returns>A randomly selected value from the supplied options.</returns>
		public static T Choose<T>(this Random random, params T[] values) =>
			values[random.Next(values.Length)];

		#endregion

		#region Next (Weighted)

		/// <summary>Selects a random value from a collection of weighted options.</summary>
		/// <typeparam name="T">The generic type to select a random instance of.</typeparam>
		/// <param name="random">The random algorithm.</param>
		/// <param name="pool">The pool of weighted values to choose from.</param>
		/// <returns>A randomly selected value from the weighted pool.</returns>
		public static T Next<T>(this Random random, params (T Value, double Weight)[] pool) =>
			random.Next((System.Collections.Generic.IEnumerable<(T Value, double Weight)>)pool);

		/// <summary>Selects a random value from a collection of weighted options.</summary>
		/// <typeparam name="T">The generic type to select a random instance of.</typeparam>
		/// <param name="random">The random algorithm.</param>
		/// <param name="pool">The pool of weighted values to choose from.</param>
		/// <param name="totalWeight">The total weight of all the values in the pool.</param>
		/// <returns>A randomly selected value from the weighted pool.</returns>
		public static T Next<T>(this Random random, double totalWeight, params (T Value, double Weight)[] pool) =>
			random.Next(pool, totalWeight);

		/// <summary>Selects a random value from a collection of weighted options.</summary>
		/// <typeparam name="T">The generic type to select a random instance of.</typeparam>
		/// <param name="random">The random algorithm.</param>
		/// <param name="pool">The pool of weighted values to choose from.</param>
		/// <param name="totalWeight">The total weight of all the values in the pool.</param>
		/// <returns>A randomly selected value from the weighted pool.</returns>
		public static T Next<T>(this Random random, System.Collections.Generic.IEnumerable<(T Value, double Weight)> pool, double? totalWeight = null)
		{
			if (!totalWeight.HasValue)
			{
				totalWeight = 0;
				foreach (var (Value, Weight) in pool)
				{
					if (Weight < 0)
						throw new ArgumentOutOfRangeException("A value in the pool had a weight less than zero.");
					totalWeight += Weight;
				}
			}
			else
			{
				if (totalWeight < 0)
					throw new ArgumentOutOfRangeException("The provided total weight of the pool was less than zero.");
			}
			if (totalWeight == 0)
				throw new ArgumentOutOfRangeException("The total weight of all values in the pool was zero.");
			if (double.IsInfinity(totalWeight.Value))
				throw new ArgumentOutOfRangeException("The total weight of all values in the pool was an infinite double.");
			double randomDouble = random.NextDouble();
			double range = 0;
			foreach (var (Value, Weight) in pool)
			{
				range += Weight;
				if (range > totalWeight)
					throw new ArgumentOutOfRangeException("The provided total weight of the pool was less than the actual total weight.");
				if (randomDouble < range / totalWeight)
					return Value;
			}
			throw new TowelBugException("There is a bug in the code.");
		}

#if false

		//public static T Next<T>(this Random random, StepperBreak<(T Value, double Weight)> pool, double? totalWeight = null)
		//{
		//	if (!totalWeight.HasValue)
		//	{
		//		totalWeight = 0;
		//		pool(x =>
		//		{
		//			if (x.Weight < 0)
		//				throw new ArgumentOutOfRangeException("A value in the pool had a weight less than zero.");
		//			totalWeight += x.Weight;
		//			return Continue;
		//		});
		//	}
		//	else
		//	{
		//		if (totalWeight < 0)
		//			throw new ArgumentOutOfRangeException("The provided total weight of the pool was less than zero.");
		//	}
		//	if (totalWeight == 0)
		//		throw new ArgumentOutOfRangeException("The total weight of all values in the pool was zero.");
		//	if (double.IsInfinity(totalWeight.Value))
		//		throw new ArgumentOutOfRangeException("The total weight of all values in the pool was an infinite double.");
		//	double randomDouble = random.NextDouble();
		//	double range = 0;
		//	T @return = default;
		//	StepStatus status = pool(x =>
		//	{
		//		range += x.Weight;
		//		if (range > totalWeight)
		//			throw new ArgumentOutOfRangeException("The provided total weight of the pool was less than the actual total weight.");
		//		if (randomDouble < range / totalWeight)
		//		{
		//			@return = x.Value;
		//			return Break;
		//		}
		//		return Continue;
		//	});
		//	return status is Break
		//		? @return
		//		: throw new TowelBugException("There is a bug in the code.");
		//}

#endif

		#endregion

		#endregion

		#region Array

		#region Stepper

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T, Step>(this T[] array, Step step = default)
			where Step : struct, IAction<T> =>
			StepperRef<T, StepToStepRef<T, Step>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T>(this T[] array, Action<T> step) =>
			Stepper<T, ActionRuntime<T>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void StepperRef<T, Step>(this T[] array, Step step = default)
			where Step : struct, IStepRef<T> =>
			StepperRefBreak<T, StepRefBreakFromStepRef<T, Step>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T>(this T[] array, StepRef<T> step) =>
			StepperRef<T, StepRefRuntime<T>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus StepperBreak<T, Step>(this T[] array, Step step = default)
			where Step : struct, IFunc<T, StepStatus> =>
			StepperRefBreak<T, StepRefBreakFromStepBreak<T, Step>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus Stepper<T>(this T[] array, Func<T, StepStatus> step) =>
			StepperBreak<T, StepBreakRuntime<T>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus Stepper<T>(this T[] array, StepRefBreak<T> step) =>
			StepperRefBreak<T, StepRefBreakRuntime<T>>(array, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus StepperRefBreak<T, Step>(this T[] array, Step step = default)
			where Step : struct, IStepRefBreak<T> =>
			StepperRefBreak<T, Step>(array, 0, array.Length, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T, Step>(this T[] array, int start, int end, Step step = default)
			where Step : struct, IAction<T> =>
			StepperRef<T, StepToStepRef<T, Step>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T>(this T[] array, int start, int end, Action<T> step) =>
			Stepper<T, ActionRuntime<T>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void StepperRef<T, Step>(this T[] array, int start, int end, Step step = default)
			where Step : struct, IStepRef<T> =>
			StepperRefBreak<T,StepRefBreakFromStepRef<T, Step>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static void Stepper<T>(this T[] array, int start, int end, StepRef<T> step) =>
			StepperRef<T, StepRefRuntime<T>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus StepperBreak<T, Step>(this T[] array, int start, int end, Step step = default)
			where Step : struct, IFunc<T, StepStatus> =>
			StepperRefBreak<T, StepRefBreakFromStepBreak<T, Step>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus Stepper<T>(this T[] array, int start, int end, Func<T, StepStatus> step) =>
			StepperBreak<T, StepBreakRuntime<T>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus Stepper<T>(this T[] array, int start, int end, StepRefBreak<T> step) =>
			StepperRefBreak<T, StepRefBreakRuntime<T>>(array, start, end, step);

		/// <summary>Traverses an array and performs an operation on each value.</summary>
		/// <typeparam name="T">The element type in the array.</typeparam>
		/// <typeparam name="Step">The operation to perform on each value of th traversal.</typeparam>
		/// <param name="array">The array to traverse.</param>
		/// <param name="start">The inclusive starting index.</param>
		/// <param name="end">The non-inclusive ending index.</param>
		/// <param name="step">The operation to perform on each value of th traversal.</param>
		/// <returns>The status of the traversal.</returns>
		public static StepStatus StepperRefBreak<T, Step>(this T[] array, int start, int end, Step step = default)
			where Step : struct, IStepRefBreak<T>
		{
			for (int i = start; i < end; i++)
			{
				if (step.Do(ref array[i]) is Break)
				{
					return Break;
				}
			}
			return Continue;
		}

		#endregion

		/// <summary>Builds an array from a size and initialization delegate.</summary>
		/// <typeparam name="T">The generic type of the array.</typeparam>
		/// <param name="size">The size of the array to build.</param>
		/// <param name="func">The initialization pattern.</param>
		/// <returns>The built array.</returns>
		public static T[] BuildArray<T>(int size, Func<int, T> func)
		{
			T[] array = new T[size];
			for (int i = 0; i < size; i++)
			{
				array[i] = func(i);
			}
			return array;
		}

		/// <summary>Formats an array so that all values are the same.</summary>
		/// <typeparam name="T">The generic type of the array to format.</typeparam>
		/// <param name="array">The array to format.</param>
		/// <param name="value">The value to format all entries in the array with.</param>
		public static void Format<T>(this T[] array, T value)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = value;
			}
		}

		/// <summary>Formats an array so that all values are the same.</summary>
		/// <typeparam name="T">The generic type of the array to format.</typeparam>
		/// <param name="array">The array to format.</param>
		/// <param name="func">The per-index format function.</param>
		public static void Format<T>(this T[] array, Func<int, T> func)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = func(i);
			}
		}

		/// <summary>Constructs a square jagged array of the desired dimensions.</summary>
		/// <typeparam name="T">The generic type to store in the jagged array.</typeparam>
		/// <param name="length1">The length of the first dimension.</param>
		/// <param name="length2">The length of the second dimension.</param>
		/// <returns>The constructed jagged array.</returns>
		public static T[][] ConstructRectangularJaggedArray<T>(int length1, int length2)
		{
			T[][] jaggedArray = new T[length1][];
			for (int i = 0; i < length1; i++)
			{
				jaggedArray[i] = new T[length2];
			}
			return jaggedArray;
		}

		/// <summary>Constructs a square jagged array of the desired dimensions.</summary>
		/// <typeparam name="T">The generic type to store in the jagged array.</typeparam>
		/// <param name="sideLength">The length of each dimension.</param>
		/// <returns>The constructed jagged array.</returns>
		public static T[][] ConstructSquareJaggedArray<T>(int sideLength) =>
			ConstructRectangularJaggedArray<T>(sideLength, sideLength);

		#endregion

		#region To English Words

		internal static class ToEnglishWordsDefinitions
		{
			internal static string[] Digit =
			{
				/* 0 */ null,
				/* 1 */ "One",
				/* 2 */ "Two",
				/* 3 */ "Three",
				/* 4 */ "Four",
				/* 5 */ "Five",
				/* 6 */ "Six",
				/* 7 */ "Seven",
				/* 8 */ "Eight",
				/* 9 */ "Nine",
			};

			internal static string[] FractionalSufix =
			{
				/*  0 */ null,
				/*  1 */ "Tenths",
				/*  2 */ "Hundredths",
				/*  3 */ "Thousandths",
				/*  4 */ "Ten-Thousandths",
				/*  5 */ "Hundred-Thousandths",
				/*  6 */ "Millionths",
				/*  7 */ "Ten-Millionths",
				/*  8 */ "Hundred-Millionths",
				/*  9 */ "Billionths",
				/* 10 */ "Ten-Billionths",
				/* 11 */ "Hundred-Billionths",
				/* 12 */ "Trilionths",
				/* 13 */ "Ten-Trilionths",
				/* 14 */ "Hundred-Trilionths",
				/* 15 */ "Quadrillionths",
				/* 16 */ "Ten-Quadrillionths",
				/* 17 */ "Hundred-Quadrillionths",
				/* 18 */ "Quintrillionths",
				/* 19 */ "Ten-Quintrillionths",
				/* 20 */ "Hundred-Quintrillionths",
				/* 21 */ "Sextillionths",
				/* 22 */ "Ten-Sextillionths",
				/* 23 */ "Hundred-Sextillionths",
				/* 24 */ "Septillionths",
				/* 25 */ "Ten-Septillionths",
				/* 26 */ "Hundred-Septillionths",
				/* 27 */ "Octillionths",
				/* 28 */ "Ten-Octillionths",
				/* 29 */ "Hundred-Octillionths",
			};

			internal static string[] Ten =
			{
				/* 0 */ null,
				/* 1 */ "Ten",
				/* 2 */ "Twenty",
				/* 3 */ "Thirty",
				/* 4 */ "Forty",
				/* 5 */ "Fifty",
				/* 6 */ "Sixty",
				/* 7 */ "Seventy",
				/* 8 */ "Eighty",
				/* 9 */ "Ninety",
			};

			internal static string[] Teen =
			{
				/* 0 */ null,
				/* 1 */ "Eleven",
				/* 2 */ "Twelve",
				/* 3 */ "Thirteen",
				/* 4 */ "Fourteen",
				/* 5 */ "Fifteen",
				/* 6 */ "Sixteen",
				/* 7 */ "Seventeen",
				/* 8 */ "Eighteen",
				/* 9 */ "Nineteen",
			};

			internal static string[] Group =
			{
				/*  0 */ null,
				/*  1 */ null,
				/*  2 */ "Thousand",
				/*  3 */ "Million",
				/*  4 */ "Billion",
				/*  5 */ "Trillion",
				/*  6 */ "Quadrillion",
				/*  7 */ "Quintillion",
				/*  8 */ "Sextillion",
				/*  9 */ "Septillion",
				/* 10 */ "Octillion",
			};
		}

		internal static string ToEnglishWords(string number) =>
			ToEnglishWords(number.AsSpan());

		internal static string ToEnglishWords(ReadOnlySpan<char> number)
		{
			if (number.Length == 1 && number[0] == '0')
			{
				return "Zero";
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool spaceNeeded = false;
			if (number[0] == '-')
			{
				Append("Negative");
				spaceNeeded = true;
				number = number[1..];
			}
			if (number[0] == '0')
			{
				number = number[1..];
			}
			int decimalIndex = number.IndexOf('.');
			if (decimalIndex != 0)
			{
				ReadOnlySpan<char> wholeNumber = decimalIndex >= 0
					? number[0..decimalIndex]
					: number;
				WholeNumber(wholeNumber);
			}
			if (decimalIndex >= 0)
			{
				if (decimalIndex != 0)
				{
					AppendLeadingSpace();
					Append("And");
				}
				ReadOnlySpan<char> fractionalNumber = number[(decimalIndex + 1)..];
				WholeNumber(fractionalNumber);
				AppendLeadingSpace();
				Append(ToEnglishWordsDefinitions.FractionalSufix[fractionalNumber.Length]);
			}
			string result = stringBuilder.ToString();
			return result;

			void Append(string @string) => stringBuilder.Append(@string);

			void AppendLeadingSpace()
			{
				if (!spaceNeeded)
				{
					spaceNeeded = true;
				}
				else
				{
					Append(" ");
				}
			}

			void WholeNumber(ReadOnlySpan<char> wholeNumber)
			{
				// A "digit group" is a set of hundreds + tens + ones digits.
				// In the number 123456789 the digit groups are the following: 123, 456, 789
				// In the number 12345 the digit groups are the following: 12, 345

				int mod3 = wholeNumber.Length % 3;
				int digitGroup = wholeNumber.Length / 3 + (mod3 == 0 ? 0 : 1);
				while (digitGroup > 0)
				{
					int i__X = (wholeNumber.Length - (digitGroup - 1) * 3) - 1; // index of ones digit
					int i_X_ = i__X - 1;                                        // index of tens digit
					int iX__ = i_X_ - 1;                                        // index of hundreds digit

					char c__X = wholeNumber[i__X];                   // character of ones digit
					char c_X_ = i_X_ >= 0 ? wholeNumber[i_X_] : '0'; // character of tens digit
					char cX__ = iX__ >= 0 ? wholeNumber[iX__] : '0'; // character of hundreds digit

					if (c__X > '0' || c_X_ > '0' || cX__ > '0')
					{
						DigitGroup(cX__, c_X_, c__X);
						if (digitGroup > 1)
						{
							AppendLeadingSpace();
							Append(ToEnglishWordsDefinitions.Group[digitGroup]);
						}
					}
					digitGroup--;
				}
			}

			void DigitGroup(char hundredsDigit, char tensDigit, char onesDigit)
			{
				int hundred = hundredsDigit - '0',
					ten = tensDigit - '0',
					one = onesDigit - '0';
				if (hundred > 0)
				{
					AppendLeadingSpace();
					Append(ToEnglishWordsDefinitions.Digit[hundred]);
					Append(" Hundred");
				}
				if (ten > 0)
				{
					AppendLeadingSpace();
					if (one == 0)
					{
						Append(ToEnglishWordsDefinitions.Ten[ten]);
					}
					else if (ten == 1)
					{
						Append(ToEnglishWordsDefinitions.Teen[one]);
					}
					else
					{
						Append(ToEnglishWordsDefinitions.Ten[ten]);
						Append("-");
						Append(ToEnglishWordsDefinitions.Digit[one]);
					}
				}
				else if (one > 0)
				{
					AppendLeadingSpace();
					Append(ToEnglishWordsDefinitions.Digit[one]);
				}
			}
		}

		/// <summary>Converts a <see cref="decimal"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="decimal">The <see cref="decimal"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="decimal"/> value.</returns>
		public static string ToEnglishWords(this decimal @decimal) => ToEnglishWords(@decimal.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="int"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="int">The <see cref="int"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="int"/> value.</returns>
		public static string ToEnglishWords(this int @int) => ToEnglishWords(@int.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="long"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="long">The <see cref="long"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="long"/> value.</returns>
		public static string ToEnglishWords(this long @long) => ToEnglishWords(@long.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="byte"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="byte">The <see cref="byte"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="byte"/> value.</returns>
		public static string ToEnglishWords(this byte @byte) => ToEnglishWords(@byte.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="sbyte"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="sbyte">The <see cref="sbyte"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="sbyte"/> value.</returns>
		public static string ToEnglishWords(this sbyte @sbyte) => ToEnglishWords(@sbyte.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="uint"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="uint">The <see cref="uint"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="uint"/> value.</returns>
		public static string ToEnglishWords(this uint @uint) => ToEnglishWords(@uint.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="ulong"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="ulong">The <see cref="ulong"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="ulong"/> value.</returns>
		public static string ToEnglishWords(this ulong @ulong) => ToEnglishWords(@ulong.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="short"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="short">The <see cref="short"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="short"/> value.</returns>
		public static string ToEnglishWords(this short @short) => ToEnglishWords(@short.ToString(System.Globalization.CultureInfo.InvariantCulture));

		/// <summary>Converts a <see cref="ushort"/> to the English words <see cref="string"/> representation.</summary>
		/// <param name="ushort">The <see cref="ushort"/> value to convert to English words <see cref="string"/> representation.</param>
		/// <returns>The English words <see cref="string"/> representation of the <see cref="ushort"/> value.</returns>
		public static string ToEnglishWords(this ushort @ushort) => ToEnglishWords(@ushort.ToString(System.Globalization.CultureInfo.InvariantCulture));

		#endregion

		#region System.Action

		/// <summary>Times an action using System.DateTime.</summary>
		/// <param name="action">The action to time.</param>
		/// <returns>The TimeSpan the action took to complete.</returns>
		public static TimeSpan Time_DateTime(this Action action)
		{
			DateTime a = DateTime.Now;
			action();
			DateTime b = DateTime.Now;
			return b - a;
		}

		/// <summary>Times an action using System.Diagnostics.Stopwatch.</summary>
		/// <param name="action">The action to time.</param>
		/// <returns>The TimeSpan the action took to complete.</returns>
		public static TimeSpan Time_StopWatch(this Action action)
		{
			Stopwatch watch = new Stopwatch();
			watch.Restart();
			action();
			watch.Stop();
			return watch.Elapsed;
		}

		#endregion

		#region System.Collections.Generic.IEnumerable<T>

		/// <summary>Tries to get the first value in an <see cref="System.Collections.Generic.IEnumerable{T}"/>.</summary>
		/// <typeparam name="T">The generic type of <see cref="System.Collections.Generic.IEnumerable{T}"/>.</typeparam>
		/// <param name="iEnumerable">The IEnumerable to try to get the first value of.</param>
		/// <param name="first">The first value of the <see cref="System.Collections.Generic.IEnumerable{T}"/> or default if empty.</param>
		/// <returns>True if the <see cref="System.Collections.Generic.IEnumerable{T}"/> has a first value or false if it is empty.</returns>
		public static bool TryFirst<T>(this System.Collections.Generic.IEnumerable<T> iEnumerable, out T first)
		{
			foreach (T value in iEnumerable)
			{
				first = value;
				return true;
			}
			first = default;
			return false;
		}

		#endregion

		#region System.Reflection.MethodInfo

		/// <summary>Creates a delegate of the specified type from this <see cref="MethodInfo"/>.</summary>
		/// <typeparam name="Delegate">The type of the delegate to create.</typeparam>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> to create the delegate from.</param>
		/// <returns>The delegate for this <see cref="MethodInfo"/>.</returns>
		/// <remarks>This extension is syntax sugar so you don't have to cast the return.</remarks>
		public static Delegate CreateDelegate<Delegate>(this MethodInfo methodInfo)
			where Delegate : System.Delegate =>
			(Delegate)methodInfo.CreateDelegate(typeof(Delegate));

		#endregion

		#region Enum (Generic)

		/// <summary>Returns an indication whether a constant with a specified value exists in a specified enumeration.</summary>
		/// <typeparam name="T">The type of the enum.</typeparam>
		/// <param name="value">The value to determine if it is defined.</param>
		/// <returns>true if a constant in enumType has a value equal to value; otherwise, false.</returns>
		public static bool IsDefined<T>(this T value) where T : Enum =>
			Enum.IsDefined(typeof(T), value);

		#endregion

		#region System.Range

		/// <summary>Converts a <see cref="System.Range"/> to an <see cref="System.Collections.Generic.IEnumerable{T}"/>.</summary>
		/// <param name="range">The <see cref="System.Range"/> to convert int a <see cref="System.Collections.Generic.IEnumerable{T}"/>.</param>
		/// <returns>The resulting <see cref="System.Collections.Generic.IEnumerable{T}"/> of the conversion.</returns>
		internal static System.Collections.Generic.IEnumerable<int> ToIEnumerable(this Range range)
		{
			int a = range.Start.Value < range.End.Value ? range.Start.Value : range.End.Value;
			int b = range.Start.Value > range.End.Value ? range.Start.Value : range.End.Value;
			for (int i = a; i < b; i++)
			{
				yield return i;
			}
		}

		#endregion

		#region Step

		/// <summary>Adds a step to the gaps (in-betweens) of another step funtion.</summary>
		/// <typeparam name="T">The generic type of the step function.</typeparam>
		/// <param name="step">The step to add a gap step to.</param>
		/// <param name="gapStep">The step to perform in the gaps.</param>
		/// <returns>The combined step + gapStep function.</returns>
		public static Action<T> Gaps<T>(this Action<T> step, Action<T> gapStep)
		{
			bool first = true;
			return
				x =>
				{
					if (!first)
					{
						gapStep(x);
					}
					step(x);
					first = false;
				};
		}

		#endregion

		#region Stepper

		/// <summary>Converts the values in this stepper to another type.</summary>
		/// <typeparam name="A">The generic type of the values of the original stepper.</typeparam>
		/// <typeparam name="B">The generic type of the values to convert the stepper into.</typeparam>
		/// <param name="stepper">The stepper to convert.</param>
		/// <param name="func">The conversion function.</param>
		/// <returns>The converted stepper.</returns>
		public static Action<Action<B>> Convert<A, B>(this Action<Action<A>> stepper, Func<A, B> func) =>
			b => stepper(a => b(func(a)));

		/// <summary>Appends values to the stepper.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to append to.</param>
		/// <param name="values">The values to append to the stepper.</param>
		/// <returns>The resulting stepper with the appended values.</returns>
		public static Action<Action<T>> Append<T>(this Action<Action<T>> stepper, params T[] values) =>
			stepper.Concat(values.ToStepper());

		/// <summary>Builds a stepper from values.</summary>
		/// <typeparam name="T">The generic type of the stepper to build.</typeparam>
		/// <param name="values">The values to build the stepper from.</param>
		/// <returns>The resulting stepper function for the provided values.</returns>
		public static Action<Action<T>> Build<T>(params T[] values) =>
			values.ToStepper();

		/// <summary>Concatenates steppers.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The first stepper of the contactenation.</param>
		/// <param name="otherSteppers">The other steppers of the concatenation.</param>
		/// <returns>The concatenated steppers as a single stepper.</returns>
		public static Action<Action<T>> Concat<T>(this Action<Action<T>> stepper, params Action<Action<T>>[] otherSteppers) =>
			step =>
			{
				stepper(step);
				foreach (Action<Action<T>> otherStepper in otherSteppers)
				{
					otherStepper(step);
				}
			};

		/// <summary>Filters a stepper using a where predicate.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to filter.</param>
		/// <param name="predicate">The predicate of the where filter.</param>
		/// <returns>The filtered stepper.</returns>
		public static Action<Action<T>> Where<T>(this Action<Action<T>> stepper, Func<T, bool> predicate) =>
			step => stepper(x =>
			{
				if (predicate(x))
				{
					step(x);
				}
			});

		/// <summary>Steps through a set number of integers.</summary>
		/// <param name="iterations">The number of times to iterate.</param>
		/// <param name="step">The step function.</param>
		public static void Iterate(int iterations, Action<int> step)
		{
			for (int i = 0; i < iterations; i++)
			{
				step(i);
			}
		}

		/// <summary>Converts an IEnumerable into a stepper delegate./></summary>
		/// <typeparam name="T">The generic type being iterated.</typeparam>
		/// <param name="iEnumerable">The IEnumerable to convert.</param>
		/// <returns>The stepper delegate comparable to the IEnumerable provided.</returns>
		public static Action<Action<T>> ToStepper<T>(this System.Collections.Generic.IEnumerable<T> iEnumerable) =>
			step =>
			{
				foreach (T value in iEnumerable)
				{
					step(value);
				}
			};

		/// <summary>Converts an IEnumerable into a stepper delegate./></summary>
		/// <typeparam name="T">The generic type being iterated.</typeparam>
		/// <param name="iEnumerable">The IEnumerable to convert.</param>
		/// <returns>The stepper delegate comparable to the IEnumerable provided.</returns>
		public static Func<Func<T, StepStatus>, StepStatus> ToStepperBreak<T>(this System.Collections.Generic.IEnumerable<T> iEnumerable) =>
			step =>
			{
				foreach (T value in iEnumerable)
				{
					if (step(value) is Break)
					{
						return Break;
					};
				}
				return Continue;
			};

		/// <summary>Converts an array into a stepper delegate./></summary>
		/// <typeparam name="T">The generic type being iterated.</typeparam>
		/// <param name="array">The array to convert.</param>
		/// <returns>The stepper delegate comparable to the array provided.</returns>
		public static StepperRef<T> ToStepperRef<T>(this T[] array) =>
			step =>
			{
				for (int i = 0; i < array.Length; i++)
				{
					step(ref array[i]);
				}
			};

		/// <summary>Converts an array into a stepper delegate./></summary>
		/// <typeparam name="T">The generic type being iterated.</typeparam>
		/// <param name="array">The array to convert.</param>
		/// <returns>The stepper delegate comparable to the array provided.</returns>
		public static StepperRefBreak<T> ToStepperRefBreak<T>(this T[] array) =>
			step =>
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (step(ref array[i]) is Break)
					{
						return Break;
					};
				}
				return Continue;
			};

		/// <summary>Converts the stepper into an array.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to convert.</param>
		/// <returns>The array created from the stepper.</returns>
		public static T[] ToArray<T>(this Action<Action<T>> stepper)
		{
			int count = stepper.Count();
			T[] array = new T[count];
			int i = 0;
			stepper(x => array[i++] = x);
			return array;
		}

		/// <summary>Counts the number of items in the stepper.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to count the items of.</param>
		/// <returns>The number of items in the stepper.</returns>
		public static int Count<T>(this Action<Action<T>> stepper)
		{
			int count = 0;
			stepper(step => count++);
			return count;
		}

		/// <summary>Reduces the stepper to be every nth value.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to reduce.</param>
		/// <param name="nth">Represents the values to reduce the stepper to; "5" means every 5th value.</param>
		/// <returns>The reduced stepper function.</returns>
		public static Action<Action<T>> EveryNth<T>(this Action<Action<T>> stepper, int nth)
		{
			_ = stepper ?? throw new ArgumentNullException(nameof(stepper));
			if (nth <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(nth), nth, "!(" + nameof(nth) + " > 0)");
			}
			int i = 1;
			return step => stepper(x =>
			{
				if (i == nth)
				{
					step(x);
					i = 1;
				}
				else
				{
					i++;
				}
			});
		}

		/// <summary>Determines if the data contains any duplicates.</summary>
		/// <typeparam name="T">The generic type of the data.</typeparam>
		/// <param name="stepper">The stepper function for the data.</param>
		/// <param name="equate">An equality function for the data</param>
		/// <param name="hash">A hashing function for the data.</param>
		/// <returns>True if the data contains duplicates. False if not.</returns>
		public static bool ContainsDuplicates<T>(this Func<Func<T, StepStatus>, StepStatus> stepper, Func<T, T, bool> equate, Func<T, int> hash)
		{
			bool duplicateFound = false;
			SetHashLinked<T> set = new SetHashLinked<T>(equate, hash);
			stepper(x =>
			{
				if (set.Contains(x))
				{
					duplicateFound = true;
					return Break;
				}
				else
				{
					set.Add(x);
					return Continue;
				}
			});
			return duplicateFound;
		}

		/// <summary>Determines if the data contains any duplicates.</summary>
		/// <typeparam name="T">The generic type of the data.</typeparam>
		/// <param name="stepper">The stepper function for the data.</param>
		/// <param name="equate">An equality function for the data</param>
		/// <param name="hash">A hashing function for the data.</param>
		/// <returns>True if the data contains duplicates. False if not.</returns>
		/// <remarks>Use the StepperBreak overload if possible. It is more effiecient.</remarks>
		public static bool ContainsDuplicates<T>(this Action<Action<T>> stepper, Func<T, T, bool> equate, Func<T, int> hash)
		{
			bool duplicateFound = false;
			SetHashLinked<T> set = new SetHashLinked<T>(equate, hash);
			stepper(x =>
			{
				if (set.Contains(x))
				{
					duplicateFound = true;
				}
				else
				{
					set.Add(x);
				}
			});
			return duplicateFound;
		}

		/// <summary>Determines if the data contains any duplicates.</summary>
		/// <typeparam name="T">The generic type of the data.</typeparam>
		/// <param name="stepper">The stepper function for the data.</param>
		/// <returns>True if the data contains duplicates. False if not.</returns>
		public static bool ContainsDuplicates<T>(this Func<Func<T, StepStatus>, StepStatus> stepper) =>
			ContainsDuplicates(stepper, Equate, DefaultHash);

		/// <summary>Determines if the data contains any duplicates.</summary>
		/// <typeparam name="T">The generic type of the data.</typeparam>
		/// <param name="stepper">The stepper function for the data.</param>
		/// <returns>True if the data contains duplicates. False if not.</returns>
		/// <remarks>Use the StepperBreak overload if possible. It is more effiecient.</remarks>
		public static bool ContainsDuplicates<T>(this Action<Action<T>> stepper) =>
			ContainsDuplicates(stepper, Equate, DefaultHash);

		/// <summary>Determines if the stepper contains any of the predicated values.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to determine if any predicated values exist.</param>
		/// <param name="where">The predicate.</param>
		/// <returns>True if any of the predicated values exist or </returns>
		public static bool Any<T>(this Action<Action<T>> stepper, Predicate<T> where)
		{
			bool any = false;
			stepper(x => any = any || where(x));
			return any;
		}

		/// <summary>Determines if the stepper contains any of the predicated values.</summary>
		/// <typeparam name="T">The generic type of the stepper.</typeparam>
		/// <param name="stepper">The stepper to determine if any predicated values exist.</param>
		/// <param name="where">The predicate.</param>
		/// <returns>True if any of the predicated values exist or </returns>
		public static bool Any<T>(this Func<Func<T, StepStatus>, StepStatus> stepper, Predicate<T> where)
		{
			bool any = false;
			stepper(x => (any = where(x))
				? Break
				: Continue);
			return any;
		}

		/// <summary>Converts a stepper into a string of the concatenated chars.</summary>
		/// <param name="stepper">The stepper to concatenate the values into a string.</param>
		/// <returns>The string of the concatenated chars.</returns>
		public static string ConcatToString(this Action<Action<char>> stepper)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stepper(c => stringBuilder.Append(c));
			return stringBuilder.ToString();
		}

		#endregion
	}
}
