//
// Unit tests for CGImage
//
// Authors:
//	Marek Safar (marek.safar@gmail.com)
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2013, 2015 Xamarin Inc. All rights reserved.
//

using System;
using System.IO;

#if XAMCORE_2_0
using Foundation;
using UIKit;
using CoreGraphics;
#else
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif
using NUnit.Framework;

namespace MonoTouchFixtures.CoreGraphics {
	
	[TestFixture]
	[Preserve (AllMembers = true)]
	public class CGImageTest {
		
		[Test]
		public void FromPNG ()
		{
			string file = Path.Combine (NSBundle.MainBundle.ResourcePath, "basn3p08.png");
			using (var dp = new CGDataProvider (file))
			using (var img = CGImage.FromPNG (dp, null, false, CGColorRenderingIntent.Default))
			using (var ui = new UIImage (img, 1.0f, UIImageOrientation.Up)) {
				Assert.IsNotNull (ui.CGImage, "CGImage");

				if (TestRuntime.CheckiOSSystemVersion (9,0))
					Assert.That (img.UTType.ToString (), Is.EqualTo ("public.png"), "UTType");
			}
		}
	}
}
