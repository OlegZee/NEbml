#if NUNIT

using System;
using System.IO;
using NEbml.Core;
using NEbml.MkvTitleEdit.Matroska;
using NUnit.Framework;

namespace NEbml.MkvTitleEdit.Tests
{
	[TestFixture]
	public class MiscTests
	{
		[TestCase(@"\\nas\movies\HD\Inception (2010).mkv"), Explicit]
		public void MkvInfo(string filePath)
		{
			using (var dataStream = File.OpenRead(filePath))
			{
				var reader = new EbmlReader(dataStream);
				//reader.EnterContainer();

				var headDumper = MakeElementDumper(
					StandardDtd.EBMLDesc.EBMLVersion,
					StandardDtd.EBMLDesc.EBMLReadVersion,
					StandardDtd.EBMLDesc.DocTypeVersion,
					StandardDtd.EBMLDesc.DocType,
					StandardDtd.EBMLDesc.DocTypeReadVersion);

				var trackInfoDumper = MakeElementDumper(
					MatroskaDtd.Tracks.TrackEntry.TrackNumber,
					MatroskaDtd.Tracks.TrackEntry.Name,
					MatroskaDtd.Tracks.TrackEntry.Language,
					MatroskaDtd.Tracks.TrackEntry.TrackType,
					MatroskaDtd.Tracks.TrackEntry.CodecName
					);

				reader.ReadNext();
				Assert.AreEqual(StandardDtd.EBML.Identifier, reader.ElementId);

				headDumper(reader);

				if (reader.LocateElement(MatroskaDtd.Segment))
				{
					reader.EnterContainer();

					if (reader.LocateElement(MatroskaDtd.Tracks))
					{
						Console.WriteLine("Tracks");
						reader.EnterContainer();
						while (reader.ReadNext())
						{
							if (reader.ElementId == MatroskaDtd.Tracks.TrackEntry.Identifier)
							{
								trackInfoDumper(reader);
							}
							Console.WriteLine();
						}
						reader.LeaveContainer();
						Console.WriteLine("end of Tracks");
					}

					if (reader.LocateElement(MatroskaDtd.Segment.Cluster))
					{
						Console.WriteLine("Got first track");

						// TODO have to deal with interlaced track data
					}
					// reader.LeaveContainer();
				}

			}
		}

		private Action<EbmlReader> MakeElementDumper(params ElementDescriptor[] descriptors)
		{
			var dumpers = Array.ConvertAll(descriptors, MakeElementDumper);

			return reader =>
				{
					reader.EnterContainer();

					while (reader.ReadNext())
					{
						foreach (var dumper in dumpers)
						{
							dumper(reader);
						}
					}

					reader.LeaveContainer();
				};
		}

		private Action<EbmlReader> MakeElementDumper(ElementDescriptor element)
		{
			Func<EbmlReader,string> dump = null;
			switch (element.Type)
			{
				case ElementType.AsciiString:
					dump = reader => reader.ReadAscii();
					break;
				case ElementType.Binary:
					dump = _ => "binary data";
					break;
				case ElementType.Date:
					dump = r => r.ReadDate().ToString();
					break;
				case ElementType.Float:
					dump = r => r.ReadFloat().ToString();
					break;
				case ElementType.SignedInteger:
					dump = r => r.ReadInt().ToString();
					break;
				case ElementType.UnsignedInteger:
					dump = r => r.ReadUInt().ToString();
					break;
				case ElementType.Utf8String:
					dump = r => r.ReadUtf();
					break;
				default:
					dump = _ => string.Format("unknown (id:{0})", element.Type.ToString());
					break;
			}

			return reader =>
				{
					if (reader.ElementId == element.Identifier)
					{
						Console.WriteLine("{0}: {1}", element.Name, dump(reader));
					}
				};
		}
	}
}

#endif