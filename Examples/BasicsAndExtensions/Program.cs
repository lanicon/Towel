﻿using System;
using System.IO;
using Towel;
using Towel.DataStructures;
using Towel.Mathematics;
using System.Linq;
using static Towel.Statics;

namespace BasicsAndExtensions
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("You are runnning the BasicsAndExtensions example.");
			Console.WriteLine("============================================");
			Console.WriteLine();

			#region Multi String Replace
			{
				Console.WriteLine("  Multi String Replace -----------------------");
				Console.WriteLine();

				string original = "a b c d";
				string modified = original.Replace(("a", "aaa"), ("c", "ccc"));

				Console.WriteLine("    original: \"a b c d\"");
				Console.WriteLine($@"    original.Replace((""a"", ""aaa""), (""c"", ""ccc""): ""{modified}""");
				Console.WriteLine();
			}
			#endregion

			#region TryParse
			{
				var (_, a) = TryParse<double>("123.4");
				var (_, b) = TryParse<float>("12.3");
				var (_, c) = TryParse<byte>("1");
				var (_, d) = TryParse<int>("1234");
				var (_, e) = TryParse<Program>("1234");
				var (_, f) = TryParse<ConsoleColor>("Red");
				var (_, g) = TryParse<StringComparison>("Ordinal");

				Console.WriteLine("  TryParse------------------------------------");
				Console.WriteLine();
				Console.WriteLine($"    TryParse(\"123.4\", out double a) := {a}d");
				Console.WriteLine($"    TryParse(\"12.3\", out float b) := {b}f");
				Console.WriteLine($"    TryParse(\"1\", out byte c) := {c}");
				Console.WriteLine($"    TryParse(\"1234\", out int d) := {d}");
				Console.WriteLine($"    TryParse(\"1234\", out Program e) := {e?.ToString() ?? "null"}");
				Console.WriteLine($"    TryParse(\"Red\", out ConsoleColor f) := {f}");
				Console.WriteLine($"    TryParse(\"Ordinal\", out StringComparison g) := {g}");
				Console.WriteLine();
			}
			#endregion

			#region Convert
			{
				// Note: the main use case for this is converting types when using generics.

				double a = Convert<int, double>(1234);
				float b = Convert<int, float>(123);
				int c = Convert<double, int>(123.4d);
				int d = Convert<float, int>(12.3f);

				Console.WriteLine("  Convert------------------------------------");
				Console.WriteLine();
				Console.WriteLine($"    Convert<int, double>(1234) := {a}d");
				Console.WriteLine($"    Convert<int, float>(123) := {b}f");
				Console.WriteLine($"    Convert<double, int>(123.4d) := {c}");
				Console.WriteLine($"    Convert<float, int>(12.3f) := {d}");
				Console.WriteLine();
			}
			#endregion

			#region Stepper
			{
				Console.WriteLine("  Stepper------------------------------------");
				Console.WriteLine();
				Console.WriteLine("    A Towel.Stepper<T> in Towel is similar to a ");
				Console.WriteLine("    System.Collections.Generic.IEnumerable<T> but");
				Console.WriteLine("    it uses delegates rather than an enumerator.");
				Console.WriteLine("    There are pros/cons to both methodologies.");
				Console.WriteLine();

				System.Collections.Generic.IEnumerable<int> iEnumerable = Ɐ(1, 2, 3);
				Console.Write("    iEnumerable values:");
				foreach (int value in iEnumerable)
				{
					Console.Write(" " + value);
				}
				Console.WriteLine();

				Action<Action<int>> stepper = Ɐ(1, 2, 3);
				Console.Write("    stepper values:");
				stepper(value => Console.Write(" " + value));
				Console.WriteLine();

				/// You can "break" a foreach loop, but you cannot break a stepper traversal.
				/// For this, there is another type of stepper that is breakable. "Towel.StepperBreak<T>"
				/// is a breakable version of the stepper.

				Func<Func<int, StepStatus>, StepStatus> stepperBreak = Ɐ(1, 2, 3, 4, 5, 6);
				Console.Write("    stepperBreak values:");
				stepperBreak(value =>
				{
					Console.Write(" " + value);
					return value >= 3 ? Break : Continue;
				});
				Console.WriteLine();

				/// You cannot alter the values of an IEnumerable during iteration, however,
				/// you can do so with a "Towel.StepperRef<T>".

				StepperRef<int> stepperRef = Ɐ(0, 1, 2);
				Console.Write("    stepperRef values:");
				stepperRef((ref int value) =>
				{
					value++;
					Console.Write(" " + value);
				});
				Console.WriteLine();

				/// The "Towel.StepperRefBreak<T>" is a stepper type that allows for altering
				/// values and breaking iteration.

				StepperRefBreak<int> stepperRefBreak = Ɐ(0, 1, 2, 3, 4, 5);
				Console.Write("    stepperRefBreak values:");
				stepperRefBreak((ref int value) =>
				{
					value++;
					Console.Write(" " + value);
					return value >= 3 ? Break : Continue;
				});
				Console.WriteLine();

				/// Here is an example of creating a stepper from only functions (no backing
				/// data structure).
				static void stepperFunctional(Action<int> s) { s(1); s(2); s(3); }
				Console.Write("    stepperFunctional values:");
				stepperFunctional(value => Console.Write(" " + value));

				Console.WriteLine();
				Console.WriteLine();
			}
			#endregion

			#region Decimal To Words
			{
				Console.WriteLine("  Converting Decimal To Words---------------------------");
				Console.WriteLine();

				decimal value1 = 12345.6789m;
				Console.WriteLine($"    Value1 = {value1}");
				Console.WriteLine($"    Value1 To Words = {value1.ToEnglishWords()}");

				decimal value2 = 999.888m;
				Console.WriteLine($"    Value2 = {value2}");
				Console.WriteLine($"    Value2 To Words = {value2.ToEnglishWords()}");

				decimal value3 = 1111111.2m;
				Console.WriteLine($"    Value3 = {value3}");
				Console.WriteLine($"    Value3 To Words = {value3.ToEnglishWords()}");
				Console.WriteLine();
			}
			#endregion

			#region Type To C# Source Code
			{
				Console.WriteLine("  Type To C# Source Code---------------------------");
				Console.WriteLine();
				Console.WriteLine("    Note: this can be useful for runtime compilation from strings");
				Console.WriteLine();

				Console.WriteLine("    " + typeof(IOmnitreePoints<Vector<double>, double, double, double>).ConvertToCSharpSource());
				Console.WriteLine("    " + typeof(Symbolics.Add).ConvertToCSharpSource());
				Console.WriteLine();
			}
			#endregion

			#region Random Extensions
			{
				Console.WriteLine("  Random Extensions---------------------------");
				Console.WriteLine();
				Console.WriteLine("    Note: there are overloads of these methods");
				Console.WriteLine();

				Random random = new Random();

				Console.WriteLine($"    Random.NextLong(): {random.NextLong()}");
				Console.WriteLine($"    Random.NextDateTime(): {random.NextDateTime()}");
				Console.WriteLine($"    Random.NextAlphaNumericString(15): {random.NextEnglishAlphaNumericString(15)}");
				Console.WriteLine($"    Random.NextChar('a', 'z'): {random.NextChar('a', 'z')}");
				Console.WriteLine($"    Random.NextDecimal(): {random.NextDecimal()}");
				Console.WriteLine($"    Random.NextTimeSpan(): {random.NextTimeSpan()}");
				Console.WriteLine($"    Random.NextUnique(5, 0, 100): {string.Join(", ", random.NextUnique(5, 0, 100))}");

				var weightedNames = new (string Name, double Weight)[]
				{
					("Dixie Normous ", 40d),
					("Harry Dick    ", 70d),
					("Ivana Humpalot", 40d),
					("Ben Dover     ", 80d),
					("Hue Mungus    ", 30d),
					("Mr. Bates     ", 20d),
				};
				Console.WriteLine("    Random.Next (weighted)... ");
				Console.WriteLine();
				Console.WriteLine("        | Name           | Weight |");
				Console.WriteLine("        |----------------|--------|");
				foreach (var (Name, Weight) in weightedNames)
					Console.WriteLine($"        | {Name} |   {Weight}   |");
				Console.WriteLine();
				Console.WriteLine($"        Random Weighted Selection: {random.Next(weightedNames)}");
				Console.WriteLine();
			}
			#endregion

			#region XML Code Documentation Via Reflection
			{
				Console.WriteLine("  XML Code Documentation Extensions------------");
				Console.WriteLine();
				Console.WriteLine("    You can access XML on source code via reflection");
				Console.WriteLine("    using Towel's extension methods.");
				Console.WriteLine();

				// This function loads in XML documentation so you can access it via reflection.
				Meta.LoadXmlDocumentation(File.ReadAllText(Path.Combine("..", "..", "..", "..", "..", "Sources", "Towel", "Towel.xml")));

				Console.Write("    XML Documentation On Towel.Statics:");
				Console.WriteLine(typeof(Towel.Statics).GetDocumentation());
				Console.Write("    XML Documentation On Towel.Constant<float>.Pi:");
				Console.WriteLine(typeof(Constant<float>).GetProperty(nameof(Constant<float>.Pi)).GetDocumentation());
			}
			#endregion

			#region Sorting
			{
				// Note: these functions are not restricted to array types. You can use the
				// overloads with "Get" and "Assign" delegates to use them on any int-indexed
				// data structure.

				Console.WriteLine("  Sorting Algorithms----------------------");
				Console.WriteLine();
				int[] dataSet = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				Console.Write($"    Data Set: {string.Join(", ", dataSet.Select(x => x.ToString()))}");
				Console.WriteLine();

				// Shuffling (Randomizing)
				Shuffle<int>(dataSet);
				Console.Write($"    Shuffle (Randomizing): {string.Join(", ", dataSet.Select(x => x.ToString()))}");
				Console.WriteLine();

				void ShuffleDataSet()
				{
					Console.WriteLine("    shuffling dataSet...");
					Shuffle<int>(dataSet);
				}

				string DataSetToString() => string.Join(", ", dataSet.Select(x => x.ToString()));

				Console.WriteLine();

				// Bubble
				SortBubble<int>(dataSet);
				Console.Write($"    Bubble:    {DataSetToString()}");
				Console.WriteLine();

				// Selection
				ShuffleDataSet();
				SortSelection<int>(dataSet);
				Console.Write($"    Selection: {DataSetToString()}");
				Console.WriteLine();

				// Insertion
				ShuffleDataSet();
				SortInsertion<int>(dataSet);
				Console.Write($"    Insertion: {DataSetToString()}");
				Console.WriteLine();

				// Quick
				ShuffleDataSet();
				SortQuick<int>(dataSet);
				Console.Write($"    Quick:     {DataSetToString()}");
				Console.WriteLine();

				// Merge
				ShuffleDataSet();
				SortMerge<int>(dataSet);
				Console.Write($"    Merge:     {DataSetToString()}");
				Console.WriteLine();

				// Heap
				ShuffleDataSet();
				SortHeap<int>(dataSet);
				Console.Write($"    Heap:      {DataSetToString()}");
				Console.WriteLine();

				// OddEven
				ShuffleDataSet();
				SortOddEven<int>(dataSet);
				Console.Write($"    OddEven:   {DataSetToString()}");
				Console.WriteLine();

				// Slow
				ShuffleDataSet();
				SortSlow<int>(dataSet);
				Console.Write($"    Slow:      {DataSetToString()}");
				Console.WriteLine();

				// Cocktail
				ShuffleDataSet();
				SortCocktail<int>(dataSet);
				Console.Write($"    Cocktail:  {DataSetToString()}");
				Console.WriteLine();

				// Shell
				ShuffleDataSet();
				SortShell<int>(dataSet);
				Console.Write($"    Shell:     {DataSetToString()}");
				Console.WriteLine();

				// Gnome
				ShuffleDataSet();
				SortGnome<int>(dataSet);
				Console.Write($"    Gnome:     {DataSetToString()}");
				Console.WriteLine();

				// Comb
				ShuffleDataSet();
				SortComb<int>(dataSet);
				Console.Write($"    Comb:      {DataSetToString()}");
				Console.WriteLine();

				// Bogo
				ShuffleDataSet();
				Console.Write("    Bogo:      Disabled (usually very slow...)");
				//Sort.Bogo(dataSet);
				//Console.Write("    Bogo:    " + string.Join(", ", dataSet.Select(x => x.ToString())));
				Console.WriteLine();

				Console.WriteLine();
			}
			#endregion

			#region Binary Search
			{
				Console.WriteLine("  Search.Binary----------------------");
				Console.WriteLine();

				int[] values = { -9, -7, -5, -3, -1, 1, 3, 5, 7, 9, };
				Console.WriteLine($"    {nameof(values)}: {{ {string.Join(", ", values)} }}");
				Console.WriteLine();

				int valueToSearchFor = values[^2];
				Console.WriteLine($"    Let's search for value {valueToSearchFor}...");

				var result = SearchBinary(values, valueToSearchFor);
				Console.WriteLine($"      Found: {result.Found}");
				Console.WriteLine($"      Index: {result.Index}");
				Console.WriteLine($"      Value: {result.Value}");
				Console.WriteLine();
			}
			#endregion

			#region IsPalindrome
			{
				Console.WriteLine("  Is Palindrome ----------------------");
				Console.WriteLine();

				string kayak = "kayak";
				Console.WriteLine($@"      IsPalindrome(""{kayak}""): {IsPalindrome(kayak)}");

				int[] values = { 1, 2, 3, 4 };
				Console.WriteLine($@"      IsPalindrome({{ {string.Join(", ", values)} }}): {IsPalindrome<int>(values)}");

				Console.WriteLine();
			}
			#endregion

			#region IsInterleaved
			{
				Console.WriteLine("  Is Interleaved ----------------------");
				Console.WriteLine();

				string abc = "abc";
				string xyz = "xyz";
				string axbycz = "axbycz";
				Console.WriteLine($@"      IsInterleavedRecursive(""{abc}"", ""{xyz}"", ""{axbycz}""): {IsInterleavedRecursive(abc, xyz, axbycz)}");
				Console.WriteLine($@"      IsInterleavedIterative(""{abc}"", ""{xyz}"", ""{axbycz}""): {IsInterleavedIterative(abc, xyz, axbycz)}");

				string a = "a";
				string b = "b";
				string c = "c";
				Console.WriteLine($@"      IsInterleavedRecursive(""{a}"", ""{b}"", ""{c}""): {IsInterleavedRecursive(a, b, c)}");
				Console.WriteLine($@"      IsInterleavedIterative(""{a}"", ""{b}"", ""{c}""): {IsInterleavedIterative(a, b, c)}");

				Console.WriteLine();
			}
			#endregion

			#region Hamming Distance
			{
				Console.WriteLine("  HammingDistance----------------");
				Console.WriteLine();
				{
					string a = "book";
					string b = "barf";
					Console.WriteLine($@"    Recursive(""{a}"", ""{b}""): {HammingDistance(a, b)}");
				}
				Console.WriteLine();
			}
			#endregion

			#region Levenshtein Distance
			{
				Console.WriteLine("  LevenshteinDistance----------------");
				Console.WriteLine();
				{
					string a = "book";
					string b = "barf";
					Console.WriteLine($@"    Recursive(""{a}"", ""{b}""): {LevenshteinDistanceRecursive(a, b)}");
				}
				{
					string a = "hello";
					string b = "help";
					Console.WriteLine($@"    Iterative(""{a}"", ""{b}""): {LevenshteinDistanceIterative(a, b)}");
				}
				Console.WriteLine();
			}
			#endregion

			#region Switch
			{
				Console.WriteLine("  Switch syntax----------------------");
				Console.WriteLine();
				Console.WriteLine("    I don't recommend using this Switch syntax.");
				Console.WriteLine("    I added it for fun. :D");
				Console.WriteLine();

				Console.Write("    With Parameter: ");
				for (int i = 1; i <= 4; i++)
				{
					// Parameter
					Switch (i)
					(
						(1,       () => Console.Write("1, ")),
						(2,       () => Console.Write("2, ")),
						(3,       () => Console.Write("3, ")),
						(Default, () => Console.Write("Default"))
					);
				}
				Console.WriteLine();

				Console.Write("    Without Parameter: ");
				for (int i = 1; i <= 4; i++)
				{
					// No Parameter
					Switch
					(
						(i == 1,  () => Console.Write("1, ")),
						(i == 2,  () => Console.Write("2, ")),
						(i == 3,  () => Console.Write("3, ")),
						(Default, () => Console.Write("Default"))
					);
				}
				Console.WriteLine();

				Console.Write("    Mixing Conditions & Values: ");
				for (int i = 1; i <= 4; i++)
				{
					// Parameter + Conditions
					Switch (i)
					(
						(1,          () => Console.Write("1, ")),
						(i == 2,     () => Console.Write("2, ")),
						(i % 3 == 0, () => Console.Write("3, ")),
						(Default,    () => Console.Write("Default"))
					);
				}
				Console.WriteLine();
				Console.WriteLine();
			}
			#endregion

			#region Permute
			{
				Console.WriteLine("  Permute---------------------------");
				Console.WriteLine();
				Console.WriteLine("    You can iterate all the permutations of an array with the");
				Console.WriteLine("    Permute methods: PermuteIterative and PermuteRecursive.");
				Console.WriteLine();

				int[] array = { 0, 1, 2, };
				void WriteArray() => Console.Write(string.Concat(array) + " ");

				Console.Write("    Recursive (array): ");
				PermuteRecursive<int>(array, WriteArray);
				Console.WriteLine();

				Console.Write("    Iterative (array): ");
				PermuteIterative<int>(array, WriteArray);
				Console.WriteLine();

				Console.WriteLine();
				Console.WriteLine("    This implementation may be used on any int-indexed collection.");
				Console.WriteLine("    It also supports cancellation (cut off) of the iteration. Here is");
				Console.WriteLine("    an example with a ListArray<int> that cancels after 3 permutations.");
				Console.WriteLine();

				int i = 0;
				ListArray<int> list = new ListArray<int> { 0, 1, 2, 3, 4 };
				void WriteList() => Console.Write(string.Concat(list) + " ");

				i = 0;
				Console.Write("    Recursive (list):  ");
				PermuteRecursive(0, list.Count - 1, WriteList, () => (++i >= 3 ? Break : Continue), i => list[i], (i, v) => list[i] = v);
				Console.WriteLine();

				i = 0;
				Console.Write("    Iterative (list):  ");
				PermuteIterative(0, list.Count - 1, WriteList, () => (++i >= 3 ? Break : Continue), i => list[i], (i, v) => list[i] = v);
				Console.WriteLine();
				Console.WriteLine();
			}
			#endregion

			#region Chance
			{
				Console.WriteLine("  Chance syntax----------------------");
				Console.WriteLine();

				Console.WriteLine($"    20% Chance: {20% Chance}");
				Console.WriteLine($"    50% Chance: {50% Chance}");
				Console.WriteLine($"    70% Chance: {70% Chance}");
				Console.WriteLine();
			}
			#endregion

			#region Inequality
			{
				Console.WriteLine("  Inequality syntax------------------");
				Console.WriteLine();
				// valid syntax
				{
					Console.WriteLine($"    {(Inequality<float>) 1 < 2 < 3 < 4 <= 4 < 5 < 6}");
					Console.WriteLine($"    {(Inequality<float>) 6 > 5 > 4 >= 4 > 3 > 2 > 1}");
					Console.WriteLine($"    {(Inequality<float>) 3 < 2 < 1}");
					Console.WriteLine($"    {(Inequality<float>) 1 > 2 > 3}");
				}
				// invalid syntax
				{
					//// this will not compile (a good thing)
					//if ((Inequality<float>) 1)
					//{
					//
					//}

					try
					{
						Console.WriteLine($"    {(Inequality<float>) 1}");
					}
					catch (InequalitySyntaxException)
					{
						Console.WriteLine("    Inequality Syntax Error");
					}

					try
					{
						Inequality<float> a = default;
						Console.WriteLine($"    {a < 1}");
					}
					catch (InequalitySyntaxException)
					{
						Console.WriteLine("    Inequality Syntax Error");
					}
				}
				Console.WriteLine();
			}
			#endregion

			#region Universal Quantifier
			{
				Console.WriteLine("  Universal Quantifier---------------");
				Console.WriteLine();
				Console.WriteLine("    (debug source code for examples)");

				// Ever wish there was one syntax that unified all the data structure types
				// and interfaces? Well... try out the "universal quantifier" syntax:

				System.Collections.Generic.IEnumerable<int> a = Ɐ(1, 2, 3);
				System.Collections.Generic.IList<int>       b = Ɐ(1, 2, 3);
				int[]                                       c = Ɐ(1, 2, 3);
				System.Collections.Generic.List<int>        d = Ɐ(1, 2, 3);
				System.Collections.Generic.HashSet<int>     e = Ɐ(1, 2, 3);
				System.Collections.Generic.LinkedList<int>  f = Ɐ(1, 2, 3);
				System.Collections.Generic.Stack<int>       g = Ɐ(1, 2, 3);
				System.Collections.Generic.Queue<int>       h = Ɐ(1, 2, 3);
				System.Collections.Generic.SortedSet<int>   i = Ɐ(1, 2, 3);
				Action<Action<int>>                         j = Ɐ(1, 2, 3);
				StepperRef<int>                             k = Ɐ(1, 2, 3);
				Func<Func<int, StepStatus>, StepStatus>     l = Ɐ(1, 2, 3);
				StepperRefBreak<int>                        m = Ɐ(1, 2, 3);
				Towel.DataStructures.Array<int>             n = Ɐ(1, 2, 3);
				Towel.DataStructures.ListArray<int>         o = Ɐ(1, 2, 3);
			}
			#endregion

			Console.WriteLine();
			Console.WriteLine("============================================");
			Console.WriteLine("Example Complete...");
			Console.WriteLine();
			ConsoleHelper.PromptPressToContinue();
		}
	}
}
