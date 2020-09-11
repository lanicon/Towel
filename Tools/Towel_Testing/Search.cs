﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Towel;

namespace Towel_Testing
{
	[TestClass] public class Search_Testing
	{
		[TestMethod] public void Binary()
		{
			{ // [even] collection size [found]
				int[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, };
				for (int i = 0; i < values.Length; i++)
				{
					var result = Search.Binary(values, i);
					Assert.IsTrue(result.Success);
					Assert.IsTrue(result.Index == i);
					Assert.IsTrue(result.Value == i);
				}
			}
			{ // [odd] collection size [found]
				int[] values = { 0, 1, 2, 3, 4, 5, 6, 7, 8, };
				for (int i = 0; i < values.Length; i++)
				{
					var result = Search.Binary(values, i);
					Assert.IsTrue(result.Success);
					Assert.IsTrue(result.Index == i);
					Assert.IsTrue(result.Value == i);
				}
			}
			{ // [even] collection size [not found]
				int[] values = { -9, -7, -5, -3, -1, 1, 3, 5, 7, 9, };
				for (int i = 0, j = -10; j <= 10; i++, j += 2)
				{
					var result = Search.Binary(values, j);
					Assert.IsTrue(!result.Success);
					Assert.IsTrue(result.Index == i - 1);
					Assert.IsTrue(result.Value == default);
				}
			}
			{ // [odd] collection size [not found]
				int[] values = { -9, -7, -5, -3, -1, 1, 3, 5, 7, };
				for (int i = 0, j = -10; j <= 8; i++, j += 2)
				{
					var result = Search.Binary(values, j);
					Assert.IsTrue(!result.Success);
					Assert.IsTrue(result.Index == i - 1);
					Assert.IsTrue(result.Value == default);
				}
			}
			{ // exception: invalid compare function
				int[] values = { -9, -7, -5, -3, -1, 1, 3, 5, 7, };
				Assert.ThrowsException<ArgumentException>(() => Search.Binary(values, a => (CompareResult)int.MinValue));
			}
			{ // exception: null argument
				int[] values = null;
				Assert.ThrowsException<ArgumentNullException>(() => Search.Binary(values, 7));
			}
		}
	}
}