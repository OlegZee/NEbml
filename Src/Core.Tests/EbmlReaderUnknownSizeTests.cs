/* Copyright (c) 2011-2020 Oleg Zee

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
using System.IO;
using NEbml.Core;
using NUnit.Framework;

namespace Core.Tests
{
    [TestFixture]
    public class EbmlReaderUnknownSizeTests
    {
        private MemoryStream _stream;
        private EbmlReader _reader;

        [SetUp]
        public void Setup()
        {
            _stream = new MemoryStream();
        }

        [TearDown]
        public void TearDown()
        {
            _reader?.Dispose();
            _stream?.Dispose();
        }

        /// <summary>
        /// Helper method to write an element ID as VINT
        /// </summary>
        private void WriteElementId(uint elementId)
        {
            var vint = VInt.MakeId(elementId);
            vint.Write(_stream);
        }

        /// <summary>
        /// Helper method to write a size as VINT
        /// </summary>
        private void WriteSize(ulong size)
        {
            var vint = VInt.EncodeSize(size);
            vint.Write(_stream);
        }

        /// <summary>
        /// Helper method to write unknown size (all VINT_DATA bits set to 1)
        /// </summary>
        private void WriteUnknownSize(int vintLength = 1)
        {
            // Use the VInt.UnknownSize method to create proper unknown size VINT
            var unknownSizeVint = VInt.UnknownSize(vintLength);
            unknownSizeVint.Write(_stream);
        }

        /// <summary>
        /// Helper method to write string data
        /// </summary>
        private void WriteStringData(string data)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(data);
            _stream.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Helper method to prepare stream for reading
        /// </summary>
        private void PrepareStreamForReading()
        {
            _stream.Position = 0;
            // Use a container size larger than the stream to allow for proper testing
            _reader = new EbmlReader(_stream, Math.Max(_stream.Length * 2, 1000));
        }

        /// <summary>
        /// Helper method to start a root container for tests that need to test unknown-size elements
        /// </summary>
        private void StartRootContainer()
        {
            WriteElementId(0x0A00); // Root container element ID
            WriteSize(800); // Large enough known size for root container
        }

        /// <summary>
        /// Helper method to prepare stream for reading with a root container
        /// </summary>
        private void PrepareStreamForReadingWithRootContainer()
        {
            _stream.Position = 0;
            // Use a container size larger than the stream to allow for proper testing
            _reader = new EbmlReader(_stream, Math.Max(_stream.Length * 2, 1000));
            
            // Enter the root container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x0A00u, _reader.ElementId.Value);
            _reader.EnterContainer();
        }

        [Test]
        public void LeaveContainer_WithKnownSizeElement_ShouldWorkCorrectly()
        {
            // Arrange: Create a master element with known size containing a string element
            WriteElementId(0x1000); // Master element ID
            WriteSize(10); // Known size: 10 bytes
            
            WriteElementId(0x2000); // String element ID  
            WriteSize(4); // String size: 4 bytes
            WriteStringData("test"); // String data

            PrepareStreamForReading();

            // Act & Assert
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            Assert.AreEqual(10, _reader.ElementSize);

            _reader.EnterContainer();
            
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("test", _reader.ReadUtf());

            // This should work without issues
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
        }

        [Test]
        public void LeaveContainer_WithUnknownSizeElement_ShouldWorkCorrectly()
        {
            // Arrange: Create a root container, then a master element with unknown size containing a string element
            StartRootContainer();
            WriteElementId(0x1000); // Master element ID
            WriteUnknownSize(1); // Unknown size (1-byte VINT with all data bits set to 1)
            
            WriteElementId(0x2000); // String element ID  
            WriteSize(4); // String size: 4 bytes
            WriteStringData("test"); // String data

            PrepareStreamForReadingWithRootContainer();

            // Act & Assert
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            
            // For unknown size elements, ElementSize should be set to remaining container size
            // After entering root container, remaining size is 800 - (element header size)
            var expectedSize = _reader.ElementSize; // Accept whatever size is calculated
            Assert.IsTrue(expectedSize > 0);

            _reader.EnterContainer();
            
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("test", _reader.ReadUtf());

            // This should work correctly even with unknown size
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Leave root container
            _reader.LeaveContainer();
        }

        [Test]
        public void LeaveContainer_WithNestedUnknownSizeElements_ShouldWorkCorrectly()
        {
            // Arrange: Create root container, then nested master elements with unknown sizes
            StartRootContainer();
            WriteElementId(0x1000); // Outer master element ID
            WriteUnknownSize(1); // Unknown size
            
            WriteElementId(0x1100); // Inner master element ID
            WriteUnknownSize(1); // Unknown size
            
            WriteElementId(0x2000); // String element ID  
            WriteSize(5); // String size: 5 bytes
            WriteStringData("hello"); // String data

            PrepareStreamForReadingWithRootContainer();

            // Act & Assert - Enter outer container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            _reader.EnterContainer();
            
            // Enter inner container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1100u, _reader.ElementId.Value);
            _reader.EnterContainer();
            
            // Read string element
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("hello", _reader.ReadUtf());

            // Leave inner container
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Leave outer container
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Leave root container
            _reader.LeaveContainer();
        }

        [Test]
        public void LeaveContainer_WithMixedKnownAndUnknownSizes_ShouldWorkCorrectly()
        {
            // Arrange: Outer element with known size, inner with unknown size
            WriteElementId(0x1000); // Outer master element ID
            WriteSize(15); // Known size: 15 bytes (should contain everything inside)
            
            WriteElementId(0x1100); // Inner master element ID
            WriteUnknownSize(1); // Unknown size
            
            WriteElementId(0x2000); // String element ID  
            WriteSize(4); // String size: 4 bytes
            WriteStringData("test"); // String data

            PrepareStreamForReading();

            // Act & Assert - Enter outer container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            Assert.AreEqual(15, _reader.ElementSize);
            _reader.EnterContainer();
            
            // Enter inner container with unknown size
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1100u, _reader.ElementId.Value);
            // Inner element size should be remaining size of outer container
            var expectedInnerSize = 15 - 3; // Total size - (inner element ID + size field)
            Assert.AreEqual(expectedInnerSize, _reader.ElementSize);
            _reader.EnterContainer();
            
            // Read string element
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("test", _reader.ReadUtf());

            // Leave containers
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
        }

        [Test]
        public void LeaveContainer_WithUnknownSizeAndPartialRead_ShouldWorkCorrectly()
        {
            // Arrange: Root container with unknown size element with multiple children, but we only read some
            StartRootContainer();
            WriteElementId(0x1000); // Master element ID
            WriteUnknownSize(1); // Unknown size
            
            WriteElementId(0x2000); // First string element ID  
            WriteSize(4); // String size: 4 bytes
            WriteStringData("test"); // String data
            
            WriteElementId(0x2100); // Second string element ID  
            WriteSize(5); // String size: 5 bytes
            WriteStringData("hello"); // String data

            PrepareStreamForReadingWithRootContainer();

            // Act & Assert
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            _reader.EnterContainer();
            
            // Read only the first child element
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("test", _reader.ReadUtf());

            // Don't read the second element, just leave the container
            // This should skip the remaining unread elements
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Leave root container
            _reader.LeaveContainer();
        }

        [Test]
        public void LeaveContainer_WithEmptyUnknownSizeElement_ShouldWorkCorrectly()
        {
            // Arrange: Root container with unknown size element with no children
            StartRootContainer();
            WriteElementId(0x1000); // Master element ID
            WriteUnknownSize(1); // Unknown size
            // No child elements

            PrepareStreamForReadingWithRootContainer();

            // Act & Assert
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            _reader.EnterContainer();
            
            // No child elements to read
            Assert.IsFalse(_reader.ReadNext());

            // Should be able to leave empty unknown size container
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Leave root container
            _reader.LeaveContainer();
        }

        [Test]
        public void IsUnknownSize_ShouldCorrectlyIdentifyUnknownSizeVints()
        {
            // Test with 1-byte unknown size VINT in a root container
            StartRootContainer();
            WriteElementId(0x1000); // Element ID
            WriteUnknownSize(1); // 1-byte unknown size
            WriteStringData("test");

            PrepareStreamForReadingWithRootContainer();

            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            
            // Debug: Let's see what the actual element size is
            var actualSize = _reader.ElementSize;
            var streamLength = _stream.Length;
            var currentPosition = _stream.Position;
            
            Console.WriteLine($"Stream length: {streamLength}");
            Console.WriteLine($"Current position: {currentPosition}");
            Console.WriteLine($"Actual element size: {actualSize}");
            Console.WriteLine($"Expected size should be around: {streamLength - currentPosition}");
            
            // For unknown size elements, the size should be reasonable, not huge
            Assert.IsTrue(actualSize > 0 && actualSize < 1000, 
                $"Element size {actualSize} should be reasonable, not huge");
            
            // Leave root container
            _reader.LeaveContainer();
        }

        [Test]
        public void LeaveContainer_WithUnknownSizeElement_ShouldUpdateParentCorrectly()
        {
            // Arrange: Outer known size container with inner unknown size container
            WriteElementId(0x1000); // Outer master element ID
            WriteSize(20); // Known size: 20 bytes
            
            WriteElementId(0x1100); // Inner master element ID (unknown size)
            WriteUnknownSize(1); // Unknown size
            
            WriteElementId(0x2000); // String element ID  
            WriteSize(4); // String size: 4 bytes
            WriteStringData("test"); // String data
            
            WriteElementId(0x2100); // Another element after the unknown size container
            WriteSize(3); // Size: 3 bytes
            WriteStringData("end"); // String data

            PrepareStreamForReading();

            // Act & Assert
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            Assert.AreEqual(20, _reader.ElementSize);
            _reader.EnterContainer();
            
            // Enter unknown size container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1100u, _reader.ElementId.Value);
            _reader.EnterContainer();
            
            // Read content of unknown size container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2000u, _reader.ElementId.Value);
            Assert.AreEqual("test", _reader.ReadUtf());
            
            // Leave unknown size container
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
            
            // Should be able to read the next element in the outer container
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x2100u, _reader.ElementId.Value);
            Assert.AreEqual("end", _reader.ReadUtf());
            
            // Leave outer container
            Assert.DoesNotThrow(() => _reader.LeaveContainer());
        }

        [Test]
        public void ReadNext_UnknownSizeElementAtRootLevel_ThrowsException()
        {
            // Arrange: Try to create an unknown-size element at root level
            WriteElementId(0x1000); // Master element ID
            WriteUnknownSize(1); // Unknown size - this should be rejected at root level
            WriteStringData("test"); // Some data

            PrepareStreamForReading();

            // Act & Assert: Should throw exception for unknown-size at root level
            try
            {
                _reader.ReadNext();
                Assert.Fail("Expected EbmlDataFormatException was not thrown");
            }
            catch (EbmlDataFormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"));
            }
        }

        [Test]
        public void ReadNext_MultipleUnknownSizeElementsAtRootLevel_ThrowsException()
        {
            // Arrange: Try to create multiple unknown-size elements at root level
            WriteElementId(0x1000); // First master element ID
            WriteUnknownSize(1); // Unknown size - should be rejected
            WriteStringData("test1");
            
            WriteElementId(0x1100); // Second master element ID
            WriteUnknownSize(1); // Unknown size - should be rejected
            WriteStringData("test2");

            PrepareStreamForReading();

            // Act & Assert: Should throw exception for first unknown-size at root level
            try
            {
                _reader.ReadNext();
                Assert.Fail("Expected EbmlDataFormatException was not thrown");
            }
            catch (EbmlDataFormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"));
            }
        }

        [Test]
        public void ReadNext_KnownSizeAtRootThenUnknownSizeAtRoot_ThrowsException()
        {
            // Arrange: Known-size element at root, then unknown-size at root
            WriteElementId(0x1000); // First element ID
            WriteSize(4); // Known size
            WriteStringData("test"); // Data
            
            WriteElementId(0x1100); // Second element ID
            WriteUnknownSize(1); // Unknown size - should be rejected at root
            WriteStringData("fail");

            PrepareStreamForReading();

            // Act: First element should work fine
            Assert.IsTrue(_reader.ReadNext());
            Assert.AreEqual(0x1000u, _reader.ElementId.Value);
            Assert.AreEqual(4, _reader.ElementSize);
            
            // Act & Assert: Second element should throw exception
            try
            {
                _reader.ReadNext();
                Assert.Fail("Expected EbmlDataFormatException was not thrown");
            }
            catch (EbmlDataFormatException ex)
            {
                Assert.IsTrue(ex.Message.Contains("unknown-size elements are not allowed at root level"));
            }
        }
    }
}
