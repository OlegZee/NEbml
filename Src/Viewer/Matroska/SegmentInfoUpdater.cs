/* Copyright (c) 2011 Oleg Zee

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
using System.Linq;
using System.IO;
using NEbml.Core;

namespace NEbml.Viewer.Matroska
{
	/// <summary>
	/// Provides very basic Mkv Segment Info fields update functionality
	/// </summary>
	public class SegmentInfoUpdater : IDisposable
	{
		private string _title, _writingApp;
		private bool _dirty;

		private Stream _stream;
		private long _dataStart;
		private int _dataLength;

		private byte[] _oldSegmentInfoData = null;

		/// <summary>
		/// Initializes a new instance of the SegmentInfoUpdater class
		/// </summary>
		public SegmentInfoUpdater()
		{
		}

		/// <summary>
		/// Initializes a new instance of the SegmentInfoUpdater class
		/// </summary>
		/// <param name="file"></param>
		public SegmentInfoUpdater(FileInfo file)
		{
			Open(file);
		}

		public void Dispose()
		{
			Close();
		}

		#region Mkv data

		/// <summary>
		/// Gets or sets the movie title
		/// </summary>
		public string Title
		{
			get { return _title; }
			set
			{
				if (value == _title) return;
				_title = value;
				_dirty = true;
			}
		}

		/// <summary>
		/// Gets or sets writing application name
		/// </summary>
		public string WritingApp
		{
			get { return _writingApp; }
			set
			{
				if (value == _writingApp) return;
				_writingApp = value;
				_dirty = true;
			}
		}

		/// <summary>
		/// Gets the application name used to mux the MKV file
		/// </summary>
		public string MuxingApp { get; private set; }

		/// <summary>
		/// Gets the movie duration
		/// </summary>
		public TimeSpan Duration { get; private set; }

		#endregion

		/// <summary>
		/// Opens the file in read-write mode and reads data
		/// </summary>
		/// <param name="file"></param>
		public void Open(FileInfo file)
		{
			Open(file, FileAccess.ReadWrite);
		}

		/// <summary>
		/// Opens the file in readonly mode
		/// </summary>
		/// <param name="file"></param>
		public void OpenRead(FileInfo file)
		{
			Open(file, FileAccess.Read);
		}

		private void Open(FileInfo file, FileAccess access)
		{
			if(file == null || !file.Exists)
				throw new ArgumentException("File not found");

			Open(file.Open(FileMode.Open, access));
		}

		/// <summary>
		/// Opend stream and reads the data
		/// </summary>
		/// <param name="stream"></param>
		public void Open(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException("stream");

			ReadData(_stream = stream);
		}

		/// <summary>
		/// Gets true if file is updatable
		/// </summary>
		public bool IsWritable
		{
			get { return _dataLength > 0 && _stream.CanWrite; }
		}

		/// <summary>
		/// Writes the changes to the file
		/// </summary>
		public void Write()
		{
			if(!_dirty) return;
			
			WriteData();
			_dirty = false;
		}

		/// <summary>
		/// Closes the data stream
		/// </summary>
		public void Close()
		{
			_stream.Close();
		}

		#region implementation

		private void ReadData(Stream src)
		{
			var reader = new EbmlReader(src);

			var readMap = new[]
				{
					new {Element = MatroskaElementDescriptorProvider.Title, Code = new Action(() => _title = reader.ReadUtf())},
					new {Element = MatroskaElementDescriptorProvider.WritingApp, Code = new Action(() => _writingApp = reader.ReadUtf())},
					new {Element = MatroskaElementDescriptorProvider.MuxingApp, Code = new Action(() => MuxingApp = reader.ReadUtf())},
					new {Element = MatroskaElementDescriptorProvider.Duration, Code = new Action(() => Duration = TimeSpan.FromMilliseconds(reader.ReadFloat()))},
				};

			if (!reader.LocateElement(MatroskaElementDescriptorProvider.Segment))
				throw new InvalidDataException("Failed to locate Segment");

			reader.EnterContainer();

			var segmentInfoIdentifier = MatroskaElementDescriptorProvider.SegmentInfo.Identifier;

			// support for Void block right before SegmentInfo
			var priorElementId = VInt.MakeId(0);
			var priorElementStart = -1l;

			// locate SegmentInfo and track its predesessor
			while (reader.ReadNext())
			{
				if (reader.ElementId == segmentInfoIdentifier) break;
				priorElementId = reader.ElementId;
				priorElementStart = reader.ElementPosition;
			}

			if (reader.ElementId != segmentInfoIdentifier)
				throw new InvalidDataException("Failed to locate Segment");

			_dataStart = priorElementId == EbmlDescriptorProvider.Void.Identifier ? priorElementStart : reader.ElementPosition;
			var oldDataStart = reader.ElementPosition;

			reader.EnterContainer();

			while (reader.ReadNext())
			{
				var entry = readMap.FirstOrDefault(arg => arg.Element.Identifier == reader.ElementId);
				if (entry != null)
					entry.Code();
			}

			reader.LeaveContainer();

			// getting start of the next element
			reader.ReadNext();
			if (reader.ElementId == EbmlDescriptorProvider.Void.Identifier)
			{
				reader.ReadNext();
			}
			_dataLength = (int)(reader.ElementPosition - _dataStart);

			var oldDataLen = (int)(reader.ElementPosition - oldDataStart);
			_oldSegmentInfoData = new byte[oldDataLen];
			_stream.Seek(oldDataStart, SeekOrigin.Begin);
			_stream.Read(_oldSegmentInfoData, 0, oldDataLen);
		}

		private void WriteData()
		{
			var reader = new EbmlReader(new MemoryStream(_oldSegmentInfoData), _oldSegmentInfoData.Length);

			// prepare writing part
			var infoStream= new MemoryStream();
			var infoWriter = new EbmlWriter(infoStream);

			var writeMap = new[]
				{
					new {Id = MatroskaElementDescriptorProvider.Title.Identifier, Value = _title},
					new {Id = MatroskaElementDescriptorProvider.WritingApp.Identifier, Value = _writingApp},
				};

			// enter the Segment block
			reader.ReadNext();
			reader.EnterContainer();

			while (reader.ReadNext())
			{
				if (!writeMap.Any(arg => arg.Id == reader.ElementId))
				{
					// copying data as is
					var buffer = new byte[reader.ElementSize];
					reader.ReadBinary(buffer, 0, (int) reader.ElementSize);
					infoWriter.Write(reader.ElementId, buffer);
				}
			}

			// write title/app in case it was not present!
			foreach (var e in writeMap.Where(arg => !string.IsNullOrEmpty(arg.Value)))
			{
				infoWriter.WriteUtf(e.Id, e.Value);
			}

			var outStream = new MemoryStream(_dataLength);
			var writer = new EbmlWriter(outStream);

			writer.Write(MatroskaElementDescriptorProvider.SegmentInfo.Identifier, infoStream.ToArray());
			var extraByteCount = _dataLength - outStream.Position;

			if (extraByteCount != 0)
			{
				var extraHLen = EbmlDescriptorProvider.Void.Identifier.Length + 2;

				var blankDataLen = (int) (extraByteCount - extraHLen);
				if (blankDataLen < 0)
				{
					throw new InvalidOperationException(string.Format("Not enough space to put the new data, {0} bytes to feet", -blankDataLen));
				}

				writer.WriteElementHeader(EbmlDescriptorProvider.Void.Identifier, VInt.EncodeSize((ulong)blankDataLen, 2));
				writer.Write(new byte[blankDataLen], 0, blankDataLen);
			}

			if (outStream.Length != _dataLength)
				throw new InvalidOperationException("Failed to prepare modified data - block length mismatch");

			_stream.Position = _dataStart;
			_stream.Write(outStream.ToArray(), 0, _dataLength);
		}

		#endregion
	}
}
