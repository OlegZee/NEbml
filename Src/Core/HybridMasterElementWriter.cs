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
using System.IO;

namespace NEbml.Core
{
	/// <summary>
	/// Implements a hybrid strategy for writing EBML master elements: buffers in memory until a limit, then switches to backpatching mode.
	/// Used by <see cref="EbmlWriter.StartMasterElement(VInt, EbmlWriter.MasterElementSizeStrategy, int)"/> when the Hybrid strategy is selected.
	/// </summary>
	internal class HybridMasterElementWriter : MasterElementWriterBase
	{
		internal HybridMasterElementWriter(EbmlWriter writer, VInt elementId, int sizeFieldLength, int bufferLimit)
			: base(new HybridStream(writer, elementId, sizeFieldLength, bufferLimit))
		{
			FlushImpl = ((HybridStream)BaseStream).CreateFlushAction();
		}

		/// <summary>
		/// A hybrid stream that buffers data until a limit, then switches to backpatching mode.
		/// </summary>
		private class HybridStream(EbmlWriter _parentWriter, VInt _elementId, int _sizeFieldLength, int _bufferLimit) : Stream
		{

			private MemoryStream _buffer = new();
			private bool _switchedToBackpatching = false;
			private long _sizePosition;
			private long _contentStartPosition;

			public Action CreateFlushAction()
			{
				return () =>
				{
					if (_switchedToBackpatching)
					{
						// Backpatch the size
						BackpatchSize();
					}
					else
					{
						// Use buffered approach
						FlushBuffered();
					}

					Dispose();
				};
			}

			private void FlushBuffered()
			{
				var data = _buffer.ToArray();
				var sizeVInt = VInt.EncodeSize((ulong)data.Length);
				_elementId.Write(_parentWriter.BaseStream);
				sizeVInt.Write(_parentWriter.BaseStream);
				_parentWriter.BaseStream.Write(data, 0, data.Length);
			}

			private void BackpatchSize()
			{
				long endPosition = _parentWriter.BaseStream.Position;
				long contentLength = endPosition - _contentStartPosition;
				var sizeVInt = VInt.EncodeSize((ulong)contentLength, _sizeFieldLength);
				long currentPos = _parentWriter.BaseStream.Position;
				_parentWriter.BaseStream.Seek(_sizePosition, SeekOrigin.Begin);
				sizeVInt.Write(_parentWriter.BaseStream);

				// Pad if necessary
				int written = (int)(_parentWriter.BaseStream.Position - _sizePosition);
				if (written < _sizeFieldLength)
				{
					var pad = new byte[_sizeFieldLength - written];
					_parentWriter.BaseStream.Write(pad, 0, pad.Length);
				}

				_parentWriter.BaseStream.Seek(currentPos, SeekOrigin.Begin);
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				if (!_switchedToBackpatching)
				{
					// Check if this write would exceed the buffer limit
					if (_buffer.Length + count > _bufferLimit)
					{
						SwitchToBackpatching();
						_parentWriter.BaseStream.Write(buffer, offset, count);
					}
					else
					{
						_buffer.Write(buffer, offset, count);
					}
				}
				else
				{
					_parentWriter.BaseStream.Write(buffer, offset, count);
				}
			}

			private void SwitchToBackpatching()
			{
				_switchedToBackpatching = true;

				// Write element ID and reserve space for size
				_elementId.Write(_parentWriter.BaseStream);
				_sizePosition = _parentWriter.BaseStream.Position;
				var zeroSize = new byte[_sizeFieldLength];
				_parentWriter.BaseStream.Write(zeroSize, 0, zeroSize.Length);
				_contentStartPosition = _parentWriter.BaseStream.Position;

				// Write buffered data to parent stream
				var bufferedData = _buffer.ToArray();
				if (bufferedData.Length > 0)
				{
					_parentWriter.BaseStream.Write(bufferedData, 0, bufferedData.Length);
				}

				// Dispose buffer as we no longer need it
				_buffer?.Dispose();
				_buffer = null;
			}

			// Stream implementation
			public override bool CanRead => false;
			public override bool CanSeek => false;
			public override bool CanWrite => true;

			public override long Length =>
				_switchedToBackpatching ? _parentWriter.BaseStream.Length : _buffer?.Length ?? 0;

			public override long Position
			{
				get => _switchedToBackpatching ? _parentWriter.BaseStream.Position : _buffer?.Position ?? 0;
				set => throw new NotSupportedException();
			}

			public override void Flush()
			{
				if (_switchedToBackpatching)
					_parentWriter.BaseStream.Flush();
				else
					_buffer?.Flush();
			}

			public override int Read(byte[] buffer, int offset, int count) =>
				throw new NotSupportedException();

			public override long Seek(long offset, SeekOrigin origin) =>
				throw new NotSupportedException();

			public override void SetLength(long value) =>
				throw new NotSupportedException();

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					_buffer?.Dispose();
					_buffer = null;
				}
				base.Dispose(disposing);
			}
		}
	}
}
