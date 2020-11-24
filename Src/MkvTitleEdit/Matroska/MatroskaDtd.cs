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

using NEbml.Core;

namespace NEbml.MkvTitleEdit.Matroska
{
	/// <summary>
	/// Matroska DTD. See http://matroska.org/technical/specs/rfc/index.html
	/// </summary>
	public class MatroskaDtd : DTDBase
	{
		private MatroskaDtd() { }

		public static readonly SegmentDesc
			Segment = new SegmentDesc();

		public static readonly TracksDesc
			Tracks = new TracksDesc();

		public sealed class SegmentDesc : MasterElementDescriptor
		{
			internal SegmentDesc() : base(0x18538067) { }

			public readonly SeekHeadDesc SeekHead = new SeekHeadDesc(0x114d9b74);
			public readonly SegmentInfoDesc Info = new SegmentInfoDesc(0x1549a966);
			public readonly ClusterDesc Cluster = new ClusterDesc(0x1f43b675);

			public sealed class SeekHeadDesc : MasterElementDescriptor
			{
				internal SeekHeadDesc(long identifier) : base(identifier) { }

				public readonly SeekDesc Seek = new SeekDesc(0x4dbb);

				public sealed class SeekDesc : MasterElementDescriptor
				{
					internal SeekDesc(long identifier)
						: base(identifier)
					{ }

					public readonly ElementDescriptor
					   SeekID = Binary(0x53ab),
					   SeekPosition = Uint(0x53ac);
				}
			}

			public sealed class SegmentInfoDesc : MasterElementDescriptor
			{
				internal SegmentInfoDesc(long identifier)
					: base(identifier)
				{ }

				public readonly ElementDescriptor
					SegmentUID = Binary(0x73a4),
					SegmentFilename = Utf8(0x7384),
					PrevUID = Binary(0x3cb923),
					PrevFilename = Utf8(0x3c83ab),
					NextUID = Binary(0x3eb923),
					NextFilename = Utf8(0x3e83bb),
					TimecodeScale = Uint(0x2ad7b1), //  [ def:1000000; ]
					Duration = Float(0x4489), // [ range:>0.0; ]
					DateUTC = Date(0x4461),
					Title = Utf8(0x7ba9),
					MuxingApp = Utf8(0x4d80),
					WritingApp = Utf8(0x5741);
			}

			public sealed class ClusterDesc : MasterElementDescriptor
			{
				internal ClusterDesc(long identifier)
					: base(identifier)
				{ }

				public readonly ElementDescriptor
					Timecode = Uint(0xe7),
					Position = Uint(0xa7),
					PrevSize = Uint(0xab);

				public readonly BlockGroupDesc
					BlockGroup = new BlockGroupDesc(0xa0);

				public sealed class BlockGroupDesc : MasterElementDescriptor
				{
					internal BlockGroupDesc(long identifier)
						: base(identifier)
					{ }

					public readonly BlockAdditionsDesc
						BlockAdditions = new BlockAdditionsDesc(0x75a1);

					public readonly ElementDescriptor
						Block = Binary(0xa1),
						BlockVirtual = Binary(0xa2),

						BlockDuration = Uint(0x9b), //[ def:TrackDuration; ];
						ReferencePriority = Uint(0xfa),
						ReferenceBlock = Int(0xfb), //[ card:*; ]
						ReferenceVirtual = Int(0xfd),
						CodecState = Binary(0xa4);

					public readonly SlicesDesc
						Slices = new SlicesDesc(0x8e);

					public sealed class BlockAdditionsDesc : MasterElementDescriptor
					{
						internal BlockAdditionsDesc(long identifier)
							: base(identifier)
						{ }

						public readonly BlockMoreDesc
							BlockMore = new BlockMoreDesc(0xa6);

						public sealed class BlockMoreDesc : MasterElementDescriptor
						{
							internal BlockMoreDesc(long identifier)
								: base(identifier)
							{ }

							public readonly ElementDescriptor
							   BlockAddID = Uint(0xee),	// [ range:1..; ]
							   BlockAdditional = Binary(0xa5);
						}
					}

					public sealed class SlicesDesc : MasterElementDescriptor
					{
						internal SlicesDesc(long identifier)
							: base(identifier)
						{ }

						public readonly TimeSliceDesc
							TimeSlice = new TimeSliceDesc(0xe8);

						public sealed class TimeSliceDesc : MasterElementDescriptor
						{
							internal TimeSliceDesc(long identifier) : base(identifier) { }

							public readonly ElementDescriptor
								LaceNumber = Uint(0xcc),  // [ def:0; ]
								FrameNumber = Uint(0xcd), // [ def:0; ]
								BlockAdditionID = Uint(0xcb), // [ def:0; ]
								Delay = Uint(0xce),       // [ def:0; ]
								Duration = Uint(0xcf);    // [ def:TrackDuration; ];
						}
					}
				}
			}
		}

		public sealed class TracksDesc : MasterElementDescriptor
		{
			internal TracksDesc(): base(0x1654ae6b) { }

			public readonly TrackEntryDesc
				TrackEntry = new TrackEntryDesc(0xae);

			public sealed class TrackEntryDesc : MasterElementDescriptor
			{
				internal TrackEntryDesc(long identifier)
					: base(identifier)
				{ }

				public readonly ElementDescriptor
					TrackNumber = Uint(0xd7).Named("Track#"), // [ range:1..; ]
					TrackUID = Uint(0x73c5),  // [ range:1..; ]
					TrackType = Uint(0x83).Named("TrackType"),   // [ range:1..254; ]
					FlagEnabled = Uint(0xb9), // [ range:0..1; def:1; ]
					FlagDefault = Uint(0x88), // [ range:0..1; def:1; ]
					FlagLacing = Uint(0x9c),  // [ range:0..1; def:1; ]
					MinCache = Uint(0x6de7),  // [ def:0; ]
					MaxCache = Uint(0x6df8),
					DefaultDuration = Uint(0x23e383),     //  uint [ range:1..; ]
					TrackTimecodeScale = Float(0x23314f), //  [ range:>0.0; def:1.0; ]
					Name = Utf8(0x536e).Named("Name"),
					Language = Ascii(0x22b59c).Named("Language"),           //  [ def:"eng"; ]
					CodecID = Ascii(0x86),
					CodecPrivate = Binary(0x63a2),
					CodecName = Utf8(0x258688).Named("CodecName"),
					CodecSettings = Utf8(0x3a9697),
					CodecInfoURL = Ascii(0x3b4040),
					CodecDownloadURL = Ascii(0x26b240),
					CodecDecodeAll = Uint(0xaa),          // [ range:0..1; def:1; ]
					TrackOverlay = Uint(0x6fab);

				public readonly VideoDesc
					Video = new VideoDesc();
				public readonly AudioDesc
					Audio = new AudioDesc();
				public readonly ContentEncodingsDesc
					ContentEncodings = new ContentEncodingsDesc();

				public sealed class VideoDesc : MasterElementDescriptor
				{
					internal VideoDesc() : base(0xe0)
					{}

					public readonly ElementDescriptor
						FlagInterlaced = Uint(0x9a),	// [range:0..1; def:0;]
						StereoMode = Uint(0x53b8),		// [range:0..3; def:0;]
						PixelWidth = Uint(0xb0),		// [range:1..;]
						PixelHeight = Uint(0xba),		// [range:1..;]
						DisplayWidth = Uint(0x54b0),	// [def:PixelWidth; ]
						DisplayHeight = Uint(0x54ba),	// [ def:PixelHeight; ]
						DisplayUnit = Uint(0x54b2),		// [ def:0; ]
						AspectRatioType = Uint(0x54b3),	// [ def:0; ]
						ColourSpace = Binary(0x2eb524),
						GammaValue = Float(0x2fb523)	// [range:>0.0;]
					;
				}

				public sealed class AudioDesc : MasterElementDescriptor
				{
					internal AudioDesc(): base(0xe1)
					{}

					public readonly ElementDescriptor
						SamplingFrequency = Float(0xb5),		// [ range:>0.0; def:8000.0; ]
						OutputSamplingFrequency = Float(0x78b5),// [range:>0.0; def:8000.0; ]
						Channels = Uint(0x94),					// [ range:1..; def:1; ]
						ChannelPositions = Binary(0x7d7b),
						BitDepth = Uint(0x6264)					// uint [ range:1..; ]
						;
				}

				public sealed class ContentEncodingsDesc : MasterElementDescriptor
				{
					internal ContentEncodingsDesc():base(0x6d80){}

					public sealed class ContentEncodingDesc : MasterElementDescriptor
					{
						internal ContentEncodingDesc()
							: base(0x6240)
						{ }

						public readonly ElementDescriptor
							ContentEncodingOrder = Uint(0x5031),
							ContentEncodingScope = Uint(0x5032),
							ContentEncodingType = Uint(0x5033);

						public readonly ContentCompressionDesc
							ContentCompression = new ContentCompressionDesc();

						public readonly ContentEncryptionDesc
							ContentEncryption = new ContentEncryptionDesc();

						public sealed class ContentCompressionDesc : MasterElementDescriptor
						{
							internal ContentCompressionDesc() : base(0x5034) { }

							public readonly ElementDescriptor
								ContentCompAlgo = Uint(0x4254),
								ContentCompSettings = Binary(0x4255);
						}
						public sealed class ContentEncryptionDesc : MasterElementDescriptor
						{
							internal ContentEncryptionDesc() : base(0x5035) { }

							public readonly ElementDescriptor
								ContentEncAlgo = Uint(0x47e1),
								ContentEncKeyID = Binary(0x47e2),
								ContentSignature = Binary(0x47e3),
								ContentSigKeyID = Binary(0x47e4),
								ContentSigAlgo = Uint(0x47e5),
								ContentSigHashAlgo = Uint(0x47e6);

						}
					}
				}
			}
		}


		/* TODO
		void elements() {
		  {

			// Cueing Data
			Cues := 1c53bb6b container {
			 CuePoint := bb container [ card:*; ] {
			   CueTime := b3 uint;
			   CueTrackPositions := b7 container [ card:*; ] {
				 CueTrack := f7 uint [ range:1..; ]
				 CueClusterPosition := f1 uint;
				 CueBlockNumber := 5378 uint [ range:1..; def:1; ]
				 CueCodecState := ea uint [ def:0; ]
				 CueReference := db container [ card:*; ] {
				   CueRefTime := 96 uint;
				   CueRefCluster := 97 uint;
				   CueRefNumber := 535f uint [ range:1..; def:1; ]
				   CueRefCodecState := eb uint [ def:0; ]
				 }
			   }
			 }
			}

			// Attachment
			Attachments := 1941a469 container {
			 AttachedFile := 61a7 container [ card:*; ] {
			   FileDescription := 467e string;
			   FileName := 466e string;
			   FileMimeType := 4660 string [ range:32..126; ]
			   FileData := 465c binary;
			   FileUID := 46ae uint;
			 }
			}

			// Chapters
			Chapters := 1043a770 container {
			 EditionEntry := 45b9 container [ card:*; ] {
			   ChapterAtom := b6 container [ card:*; ] {
				 ChapterUID := 73c4 uint [ range:1..; ]
				 ChapterTimeStart := 91 uint;
				 ChapterTimeEnd := 92 uint;
				 ChapterFlagHidden := 98 uint [ range:0..1; def:0; ]
				 ChapterFlagEnabled := 4598 uint [ range:0..1; def:0; ]
				 ChapterTrack := 8f container {
				   ChapterTrackNumber := 89 uint [ card:*; range:0..1; ]
				   ChapterDisplay := 80 container [ card:*; ] {
					 ChapString := 85 string;
					 ChapLanguage := 437c string [ card:*; def:"eng";
												   range:32..126; ]
					 ChapCountry := 437e string [ card:*; range:32..126; ]
				   }
				 }
			   }
			 }
			}

			// Tagging
			Tags := 1254c367 container [ card:*; ] {
			 Tag := 7373 container [ card:*; ] {
			   Targets := 63c0 container {
				 TrackUID := 63c5 uint [ card:*; def:0; ]
				 ChapterUID := 63c4 uint [ card:*; def:0; ]
				 AttachmentUID := 63c6 uint [ card:*; def:0; ]
			   }
			   SimpleTag := 67c8 container [ card:*; ] {
				 TagName := 45a3 string;
				 TagString := 4487 string;
				 TagBinary := 4485 binary;
			   }
			 }
			}
			}
		}
		 * */
	}
}
