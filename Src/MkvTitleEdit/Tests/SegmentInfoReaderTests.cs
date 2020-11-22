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

#if NUNIT

using System;
using System.Diagnostics;
using System.IO;
using NEbml.Core;
using NEbml.MkvTitleEdit.Matroska;
using NUnit.Framework;

namespace NEbml.MkvTitleEdit.Tests
{
	[TestFixture]
	public class SegmentInfoReaderTests
	{
		[Test, Explicit]
		public void SimpleUseCase()
		{
			var file = new FileInfo(@"Z:\!\t.mkv");

			var segmentInfo = new SegmentInfoUpdater();
			segmentInfo.Open(file);

			Debug.Print("Duration:   {0}", segmentInfo.Duration);
			Debug.Print("MuxingApp:  {0}", segmentInfo.MuxingApp);
			Debug.Print("WritingApp: {0}", segmentInfo.WritingApp);
			Debug.Print("Title:      {0}", segmentInfo.Title);

			segmentInfo.Title = "s4e16 The Cohabitation Formulation";
			segmentInfo.WritingApp = "NEbml.Viewer 0.1";

			segmentInfo.Write();
			segmentInfo.Close();
		}

		[Test, Description("Reads the segment data")]
		public void ReadsSegmentInfo()
		{
			var stream = new MemoryStream();
			var writer = new EbmlWriter(stream);

			using (var segment = writer.StartMasterElement(MatroskaDtd.Segment.Identifier))
			{
				using (var segmentInfo = segment.StartMasterElement(MatroskaDtd.Segment.Info.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaDtd.Segment.Info.Title.Identifier, "Test data");
					segmentInfo.WriteUtf(MatroskaDtd.Segment.Info.WritingApp.Identifier, "writing app");
					segmentInfo.WriteUtf(MatroskaDtd.Segment.Info.MuxingApp.Identifier, "mux app");
					segmentInfo.Write(MatroskaDtd.Segment.Info.Duration.Identifier, 1234f);
				}

				// write some dummy data
				segment.Write(VInt.MakeId(123), 0);
			}

			stream.Position = 0;
			var infoReader = new SegmentInfoUpdater();
			infoReader.Open(stream);

			Assert.AreEqual("Test data", infoReader.Title);
			Assert.AreEqual("writing app", infoReader.WritingApp);
			Assert.AreEqual("mux app", infoReader.MuxingApp);
			Assert.AreEqual(TimeSpan.FromMilliseconds(1234), infoReader.Duration);
		}

		[Test, Description("Verifies if updater preserved adjacent data and if it uses subsequent Void block to allocate data")]
		public void ReusesNextFiller()
		{
			var stream = new MemoryStream();
			var writer = new EbmlWriter(stream);

			writer.Write(VInt.MakeId(122), 321);
			using (var segment = writer.StartMasterElement(MatroskaDtd.Segment.Identifier))
			{
				using (var segmentInfo = segment.StartMasterElement(MatroskaDtd.Segment.Info.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaDtd.Segment.Info.Title.Identifier, "Test data");
				}

				// write some dummy data
				segment.Write(StandardDtd.Void.Identifier, new byte[1000]);
				segment.Write(VInt.MakeId(123), 123);	// marker
			}

			stream.Position = 0;

			// act
			var infoReader = new SegmentInfoUpdater();
			infoReader.Open(stream);
			infoReader.Title = new string('a', 500);
			infoReader.Write();

			// verify the stream is correct
			stream.Position = 0;
			var reader = new EbmlReader(stream);

			reader.ReadNext();
			Assert.AreEqual(122, reader.ElementId.Value, "start marker");
			Assert.AreEqual(321, reader.ReadInt());

			reader.ReadNext();
			Assert.AreEqual(MatroskaDtd.Segment.Identifier, reader.ElementId);
			reader.EnterContainer();

			reader.ReadNext();
			Assert.AreEqual(MatroskaDtd.Segment.Info.Identifier, reader.ElementId);
			Assert.Greater(reader.ElementSize, 500);

			reader.ReadNext();
			Assert.AreEqual(StandardDtd.Void.Identifier, reader.ElementId);

			reader.ReadNext();
			Assert.AreEqual(123, reader.ElementId.Value, "end data marker");
			Assert.AreEqual(123, reader.ReadInt());
		}

		[Test, Description("Verifies if updater preserved adjacent data and if it uses Void block before segment to allocate data")]
		public void ReusesPriorFiller()
		{
			var stream = new MemoryStream();
			var writer = new EbmlWriter(stream);

			writer.Write(VInt.MakeId(122), 321);
			using (var segment = writer.StartMasterElement(MatroskaDtd.Segment.Identifier))
			{
				segment.Write(StandardDtd.Void.Identifier, new byte[1000]);

				using (var segmentInfo = segment.StartMasterElement(MatroskaDtd.Segment.Info.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaDtd.Segment.Info.Title.Identifier, "Test data");
				}

				// write some dummy data
				segment.Write(VInt.MakeId(123), 123);	// marker
			}

			stream.Position = 0;

			// act
			var infoReader = new SegmentInfoUpdater();
			infoReader.Open(stream);
			infoReader.Title = new string('a', 500);
			infoReader.Write();

			// verify the stream is correct
			stream.Position = 0;
			var reader = new EbmlReader(stream);

			reader.ReadNext();
			Assert.AreEqual(122, reader.ElementId.Value, "start marker");
			Assert.AreEqual(321, reader.ReadInt());

			reader.ReadNext();
			Assert.AreEqual(MatroskaDtd.Segment.Identifier, reader.ElementId);
			reader.EnterContainer();

			reader.ReadNext();
			Assert.AreEqual(MatroskaDtd.Segment.Info.Identifier, reader.ElementId);
			Assert.Greater(reader.ElementSize, 500);

			reader.ReadNext();
			Assert.AreEqual(StandardDtd.Void.Identifier, reader.ElementId);

			reader.ReadNext();
			Assert.AreEqual(123, reader.ElementId.Value, "end data marker");
			Assert.AreEqual(123, reader.ReadInt());
		}

	}
}

#endif