//--------------------------------------//               PowerUI////        For documentation or //    if you have any issues, visit//        powerUI.kulestar.com////    Copyright � 2013 Kulestar Ltd//          www.kulestar.com//--------------------------------------namespace Css{		/// <summary>	/// Values of the writing-mode: property.	/// </summary>		public static class WritingMode{			// Typographic mode		public const int HorizontalTM=1;		public const int VerticalTM=1 << 1;				// Writing mode		public const int HorizontalWM=1 << 2;		public const int VerticalWM=1 << 3;				// Flow direction		public const int TopToBottom=1 << 4;		public const int RightToLeft=1 << 5;		public const int LeftToRight=1 << 6;				// Everything below here is an abstraction of the above settings		public const int HorizontalTB= TopToBottom | HorizontalTM | HorizontalWM;		public const int VerticalRL= RightToLeft | VerticalTM | VerticalWM;		public const int VerticalLR= LeftToRight | VerticalTM | VerticalWM;		public const int SidewaysRL= RightToLeft | HorizontalTM | VerticalWM;		public const int SidewaysLR= LeftToRight | HorizontalTM | VerticalWM;			}	}