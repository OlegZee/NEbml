#if NUNIT

using System;
using System.Diagnostics;
using System.IO;
using NEbml.Core;
using NEbml.Viewer.Matroska;
using NUnit.Framework;

namespace NEbml.Viewer.Tests
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

			using (var segment = writer.StartMasterElement(MatroskaElementDescriptorProvider.Segment.Identifier))
			{
				using (var segmentInfo = segment.StartMasterElement(MatroskaElementDescriptorProvider.SegmentInfo.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaElementDescriptorProvider.Title.Identifier, "Test data");
					segmentInfo.WriteUtf(MatroskaElementDescriptorProvider.WritingApp.Identifier, "writing app");
					segmentInfo.WriteUtf(MatroskaElementDescriptorProvider.MuxingApp.Identifier, "mux app");
					segmentInfo.Write(MatroskaElementDescriptorProvider.Duration.Identifier, 1234f);
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
			using (var segment = writer.StartMasterElement(MatroskaElementDescriptorProvider.Segment.Identifier))
			{
				using (var segmentInfo = segment.StartMasterElement(MatroskaElementDescriptorProvider.SegmentInfo.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaElementDescriptorProvider.Title.Identifier, "Test data");
				}

				// write some dummy data
				segment.Write(EbmlDescriptorProvider.Void.Identifier, new byte[1000]);
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
			Assert.AreEqual(MatroskaElementDescriptorProvider.Segment.Identifier, reader.ElementId);
			reader.EnterContainer();

			reader.ReadNext();
			Assert.AreEqual(MatroskaElementDescriptorProvider.SegmentInfo.Identifier, reader.ElementId);
			Assert.Greater(reader.ElementSize, 500);

			reader.ReadNext();
			Assert.AreEqual(EbmlDescriptorProvider.Void.Identifier, reader.ElementId);

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
			using (var segment = writer.StartMasterElement(MatroskaElementDescriptorProvider.Segment.Identifier))
			{
				segment.Write(EbmlDescriptorProvider.Void.Identifier, new byte[1000]);

				using (var segmentInfo = segment.StartMasterElement(MatroskaElementDescriptorProvider.SegmentInfo.Identifier))
				{
					segmentInfo.WriteUtf(MatroskaElementDescriptorProvider.Title.Identifier, "Test data");
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
			Assert.AreEqual(MatroskaElementDescriptorProvider.Segment.Identifier, reader.ElementId);
			reader.EnterContainer();

			reader.ReadNext();
			Assert.AreEqual(MatroskaElementDescriptorProvider.SegmentInfo.Identifier, reader.ElementId);
			Assert.Greater(reader.ElementSize, 500);

			reader.ReadNext();
			Assert.AreEqual(EbmlDescriptorProvider.Void.Identifier, reader.ElementId);

			reader.ReadNext();
			Assert.AreEqual(123, reader.ElementId.Value, "end data marker");
			Assert.AreEqual(123, reader.ReadInt());
		}

	}
}

#endif