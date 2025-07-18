/* Copyright (c) 2011-2025 Oleg Zee

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * */

using System;
using System.Collections.Generic;
using System.IO;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
	[TestFixture]
	public class EbmlWriterNestingAndAdvancedEdgeCasesTests : EbmlWriterTestBase
	{
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		public void SomeStrategies_DeepNesting_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			const int nestingDepth = 20;
			var baseId = 2000;
			var leafId = VInt.MakeId(9999);

			// Create deeply nested structure (avoid Hybrid for deep nesting due to stream limitations)
			var masters = new List<IDisposable>();
			try
			{
				for (int i = 0; i < nestingDepth; i++)
				{
					var masterId = VInt.MakeId((uint)(baseId + i));
					if (i == 0)
					{
						masters.Add(_writer.StartMasterElement(masterId, strategy));
					}
					else
					{
						var parentWriter = (EbmlWriter)masters[i - 1];
						masters.Add(parentWriter.StartMasterElement(masterId, strategy));
					}
				}

				// Write leaf element at deepest level
				var deepestWriter = (EbmlWriter)masters[masters.Count - 1];
				deepestWriter.WriteAscii(leafId, "deep leaf");
			}
			finally
			{
				// Dispose in reverse order
				for (int i = masters.Count - 1; i >= 0; i--)
				{
					masters[i].Dispose();
				}
			}

			// Verify deep structure is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			// Navigate to deepest level
			for (int i = 0; i < nestingDepth; i++)
			{
				Assert.IsTrue(reader.ReadNext(), $"Failed to read element at nesting level {i}");
				Assert.AreEqual(VInt.MakeId((uint)(baseId + i)), reader.ElementId);
				reader.EnterContainer();
			}

			// Read leaf element
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(leafId, reader.ElementId);
			Assert.AreEqual("deep leaf", reader.ReadAscii());

			// Exit all containers
			for (int i = 0; i < nestingDepth; i++)
			{
				reader.LeaveContainer();
			}
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_StructuredWrites_WorkTogether(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var masterId = VInt.MakeId(3000);
			var childId = VInt.MakeId(3001);

			using (var master = _writer.StartMasterElement(masterId, strategy))
			{
				// Only use proper structured writes within master elements
				master.WriteAscii(childId, "ascii text");
				master.WriteUtf(childId, "utf text 🌟");
				master.Write(childId, 12345L);
				master.Write(childId, 3.14159);
				master.Write(childId, new byte[] { 0x01, 0x02, 0x03 });
			}

			// Verify structured content is readable
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(masterId, reader.ElementId);
			reader.EnterContainer();

			// Read all elements
			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("ascii text", reader.ReadAscii());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual("utf text 🌟", reader.ReadUtf());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(12345L, reader.ReadInt());

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(3.14159, reader.ReadFloat(), 0.00001);

			Assert.IsTrue(reader.ReadNext());
			CollectionAssert.AreEqual(new byte[] { 0x01, 0x02, 0x03 }, ReadAllBinary(reader));

			reader.LeaveContainer();
		}

		[TestCase(EbmlWriter.MasterElementSizeStrategy.Buffered)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Backpatching)]
		[TestCase(EbmlWriter.MasterElementSizeStrategy.Hybrid)]
		public void AllStrategies_ProperDispose_HandlesCorrectly(EbmlWriter.MasterElementSizeStrategy strategy)
		{
			var rootId = VInt.MakeId(4000);
			var childId = VInt.MakeId(4001);
			var grandchildId = VInt.MakeId(4002);

			// Test proper disposal order (children before parents)
			var root = _writer.StartMasterElement(rootId, strategy);
			var child = root.StartMasterElement(childId, strategy);
			child.WriteAscii(grandchildId, "test content");

			// Dispose in correct order (child before parent)
			child.Dispose();
			root.Dispose();

			// Verify content was written correctly
			_stream.Position = 0;
			var reader = new EbmlReader(_stream);

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(rootId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(childId, reader.ElementId);
			reader.EnterContainer();

			Assert.IsTrue(reader.ReadNext());
			Assert.AreEqual(grandchildId, reader.ElementId);
			Assert.AreEqual("test content", reader.ReadAscii());

			reader.LeaveContainer();
			reader.LeaveContainer();
		}
	}
}
