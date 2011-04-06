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

using NEbml.Core;

namespace NEbml.MkvTitleEdit.Matroska
{
	public class MatroskaDtd : DTDBase
	{
		private MatroskaDtd() { }

		public static readonly SegmentDesc
			Segment = new SegmentDesc(0x18538067);

		public sealed class SegmentDesc : MasterElementDescriptor
		{
			internal SegmentDesc(long identifier) : base(identifier) {}

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
		}

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

		/* TODO
		void elements() {
		  {
			// Track
			Tracks := 1654ae6b container [ card:*; ] {
			 TrackEntry := ae container [ card:*; ] {
			   TrackNumber := d7 uint [ range:1..; ]
			   TrackUID := 73c5 uint [ range:1..; ]
			   TrackType := 83 uint [ range:1..254; ]
			   FlagEnabled := b9 uint [ range:0..1; def:1; ]
			   FlagDefault := 88 uint [ range:0..1; def:1; ]
			   FlagLacing  := 9c uint [ range:0..1; def:1; ]
			   MinCache := 6de7 uint [ def:0; ]
			   MaxCache := 6df8 uint;
			   DefaultDuration := 23e383 uint [ range:1..; ]
			   TrackTimecodeScale := 23314f float [ range:>0.0; def:1.0; ]
			   Name := 536e string;
			   Language := 22b59c string [ def:"eng"; range:32..126; ]
			   CodecID := 86 string [ range:32..126; ];
			   CodecPrivate := 63a2 binary;
			   CodecName := 258688 string;
			   CodecSettings := 3a9697 string;
			   CodecInfoURL := 3b4040 string [ card:*; range:32..126; ]
			   CodecDownloadURL := 26b240 string [ card:*; range:32..126; ]
			   CodecDecodeAll := aa uint [ range:0..1; def:1; ]
			   TrackOverlay := 6fab uint;

			   // Video
			   Video := e0 container {
				 FlagInterlaced := 9a uint [ range:0..1; def:0; ]
				 StereoMode := 53b8 uint [ range:0..3; def:0; ]
				 PixelWidth := b0 uint [ range:1..; ]
				 PixelHeight := ba uint [ range:1..; ]
				 DisplayWidth := 54b0 uint [ def:PixelWidth; ]
				 DisplayHeight := 54ba uint [ def:PixelHeight; ]
				 DisplayUnit := 54b2 uint [ def:0; ]
				 AspectRatioType := 54b3 uint [ def:0; ]
				 ColourSpace := 2eb524 binary;
				 GammaValue := 2fb523 float [ range:>0.0; ]
			   }

			   // Audio
			   Audio := e1 container {
				 SamplingFrequency := b5 float [ range:>0.0; def:8000.0; ]
				 OutputSamplingFrequency := 78b5 float [ range:>0.0;
														 def:8000.0; ]
				 Channels := 94 uint [ range:1..; def:1; ]
				 ChannelPositions := 7d7b binary;
				 BitDepth := 6264 uint [ range:1..; ]
			   }

			   // Content Encoding
			   ContentEncodings := 6d80 container {
				 ContentEncoding := 6240 container [ card:*; ] {
				   ContentEncodingOrder := 5031 uint [ def:0; ]
				   ContentEncodingScope := 5032 uint [ range:1..; def:1; ]
				   ContentEncodingType := 5033 uint;
				   ContentCompression := 5034 container {
					 ContentCompAlgo := 4254 uint [ def:0; ]
					 ContentCompSettings := 4255 binary;
				   }
				   ContentEncryption := 5035 container {
					 ContentEncAlgo := 47e1 uint [ def:0; ]
					 ContentEncKeyID := 47e2 binary;
					 ContentSignature := 47e3 binary;
					 ContentSigKeyID := 47e4 binary;
					 ContentSigAlgo := 47e5 uint;
					 ContentSigHashAlgo := 47e6 uint;
				   }
				 }
			   }
			 }
			}

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
