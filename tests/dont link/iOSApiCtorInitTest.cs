//
// Test the generated API `init` selectors are usable by developers
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2013 Xamarin Inc. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Reflection;
#if XAMCORE_2_0
#if !__TVOS__
using PassKit;
#endif
using Foundation;
#if !__WATCHOS__
using Metal;
#endif
using ObjCRuntime;
using UIKit;
#else
using MonoTouch.PassKit;
using MonoTouch.Foundation;
using MonoTouch.Metal;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
#endif

using NUnit.Framework;
using TouchUnit.Bindings;

namespace MonoTouchFixtures {

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class iOSApiCtorInitTest : ApiCtorInitTest {

		public iOSApiCtorInitTest ()
		{
			Class.ThrowOnInitFailure = false;
			ContinueOnFailure = true;
			//LogProgress = true;
		}

		protected override bool Skip (Type type)
		{
			switch (type.Namespace) {
			// all default ctor did not work and were replaced with [Obsolete("",true)] placeholders
			// reflecting on those would create invalid instances (no handle) that crash the app
			case "CoreBluetooth":
			case "MonoTouch.CoreBluetooth":
				return true;

			case "CoreAudioKit":
			case "MonoTouch.CoreAudioKit":
			case "Metal":
			case "MonoTouch.Metal":
				// they works with iOS9 beta 4 (but won't work on older simulators)
				if ((Runtime.Arch == Arch.SIMULATOR) && !CheckiOSOrTVOSSystemVersion (9,0))
					return true;
				break;
#if !__WATCHOS__
			case "MetalKit":
			case "MonoTouch.MetalKit":
			case "MetalPerformanceShaders":
			case "MonoTouch.MetalPerformanceShaders":
				if (Runtime.Arch == Arch.SIMULATOR)
					return true;
				// some devices don't support metal and that crash some API that does not check that, e.g. #33153
				if (!CheckiOSOrTVOSSystemVersion (9,0) || (MTLDevice.SystemDefault == null))
					return true;
				break;
#endif // !__WATCHOS__
			}

			switch (type.Name) {
			// under iOS7 creating this type will crash later (after test execution) with a stack similar to:
			// https://gist.github.com/rolfbjarne/457f78e20c8c31edef5c
			case "EKCalendarChooserDelegate":
			case "EKEventEditViewController":
				return true;

			// Objective-C exception thrown.  Name: NSInternalInconsistencyException Reason: There can only be one UIApplication instance.
			case "UIApplication":
				return true;
			// Objective-C exception thrown.  Name: NSInvalidArgumentException Reason: UISplitViewController is only supported when running under UIUserInterfaceIdiomPad
			case "UISplitViewController":
#if !__WATCHOS__
			// Objective-C exception thrown.  Name: NSInternalInconsistencyException Reason: ADInterstitialAd is available on iPad only.
			case "ADInterstitialAd":
				return UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Phone;
#endif

			case "UIVideoEditorController":
				return true;
			// shows an alert on the simulator
			case "MFMessageComposeViewController":
				return true;
			// shows an alert on the device (if no email address is configured)
			case "MFMailComposeViewController":
				return true;
				
#if !__TVOS__
			// PassKit is not available on iPads
			case "PKPassLibrary":
				return !PKPassLibrary.IsAvailable;
#endif // !__TVOS__


			// default ctor started to throw on iOS7 - we should never have exposed it but, for API compatibility,
			// we now have an "empty" obsolete ctor
			case "UIFont":
				return true;
			
			case "NSUrlSessionConfiguration":
			case "NSUrlSession":
				// This crashes when arc frees this object at the end of the scope:
				// { NSURLSession *var = [[NSURLSession alloc] init]; }
				return true;

			case "GKAchievementViewController":
			case "GKLeaderboardViewController":
				// technically available since 4.1 - however it got a new base class in 6.0
				// and that new base class GKGameCenterViewController did not exists before 6.0
				// which makes the type unusable in 5.x, ref: https://gist.github.com/spouliot/271b6230a3aa2b58bc6e
				return !CheckiOSSystemVersion (6,0);

			// mistake - we should not have exposed those default ctor and now we must live with them
			case "GCControllerElement":
			case "GCControllerAxisInput":
			case "GCControllerButtonInput":
			case "GCControllerDirectionPad":
			case "GCGamepad":
			case "GCExtendedGamepad":
			case "GCController":
				return true;

			// default constructor are not working on iOS8 so we removed them
			// and can't test them even in earlier iOS versions
			case "JSManagedValue":
			case "MKLocalSearch":
			case "MKTileOverlayRenderer":
			case "AVAssetResourceLoadingDataRequest":
			case "CLBeaconRegion":
			case "NSPersistentStoreCoordinator":
				return true;

			// Metal is not available on the (iOS8) simulator
			case "CAMetalLayer":
				return (Runtime.Arch == Arch.SIMULATOR);

#if !XAMCORE_2_0
			// from iOS8 (beta4) they do not return a valid handle
			case "AVAssetResourceLoader":
			case "AVAssetResourceLoadingRequest":
			case "AVAssetResourceLoadingContentInformationRequest":
				return true;
			// Started with iOS8 on simulator (always) but it looks like it can happen on devices too
			// NSInvalidArgumentException Use initWithAccessibilityContainer:
			case "UIAccessibilityElement":
				return CheckiOSSystemVersion (8,0);
#endif
			// in 8.2 beta 1 this crash the app (simulator) without giving any details in the logs
			case "WKUserNotificationInterfaceController":
				return true;

			// Both reported in radar #21548819
			// NSUnknownKeyException [<CIDepthOfField 0x158586970> valueForUndefinedKey:]: this class is not key value coding-compliant for the key inputPoint2.
			case "CIDepthOfField":
			// NSUnknownKeyException [<CISunbeamsGenerator 0x1586d0810> valueForUndefinedKey:]: this class is not key value coding-compliant for the key inputCropAmount.
			case "CISunbeamsGenerator":
				return true;

			case "MPMediaItemArtwork":
				// NSInvalidArgumentException Reason: image must be non-nil
				return true;
			default:
				return base.Skip (type);
			}
		}

		static List<NSObject> do_not_dispose = new List<NSObject> ();

		protected override void Dispose (NSObject obj, Type type)
		{
			switch (type.Name) {
			// this crash the application after test completed their execution so we keep it alive
			case "AVAudioRecorder":
			case "AVCaptureConnection":
			case "GKFriendRequestComposeViewController":
			case "SKView":
			// NSInvalidArgumentException *** -[__NSDictionaryM removeObjectForKey:]: key cannot be nil
			case "SKTextureAtlas":
			// fails under iOS5 with NSInvalidArgumentException Reason: -[__NSCFDictionary removeObjectForKey:]: attempt to remove nil key
			case "NSBundle":
			case "NSUrlConnection": // crash too (only on iOS5)
			// iOS8 beta 5 -> SIGABRT (only on devices)
			case "CABTMidiCentralViewController":
			case "CABTMidiLocalPeripheralViewController":
				do_not_dispose.Add (obj);
				break;
			// iOS 9 beta 1 - crash when disposed
			case "MidiNetworkConnection":
			case "WKNavigation":
			case "CIImageAccumulator":
			case "NEAppProxyTcpFlow":
			case "NEAppProxyUdpFlow":
				do_not_dispose.Add (obj);
				break;
			default:
				base.Dispose (obj, type);
				break;
			}
		}

		protected override void CheckHandle (NSObject obj)
		{
			bool result = obj.Handle != IntPtr.Zero;
			if (!result) {
				string name = obj.GetType ().Name;
				switch (name) {
				// FIXME: it's not clear what's the alternative to 'init' and it could be because I have no phone device
				case "CTCallCenter":
				case "CTTelephonyNetworkInfo":
					return;
				// to avoid crashes we do not really create (natively) default instances (iOS gives them to us) 
				// for compatibility purpose - we should never had included the default .ctor in monotouch.dll
				case "CAMediaTimingFunction":
				case "CLHeading":
				case "CLRegion":
				case "CLPlacemark":
				case "CMAccelerometerData":
				case "CMLogItem":
				case "CMAttitude":
				case "CMDeviceMotion":
				case "CMGyroData":
				case "CMMagnetometerData":
					return;
				// under iOS5 only - MPMediaPickerController: Unable to access iPod library.
				case "MPMediaPickerController":
					return;
				// re-enabled as an [Obsolete ("", true)] but it will crash if we create it (which we can since we use reflection)
				case "NSTimer":
				case "NSCompoundPredicate":
					return;
				// iOS9 - the instance was "kind of valid" before
				case "PKPaymentAuthorizationViewController":
					if (CheckiOSSystemVersion (9,0))
						return;
					break;
				}
				base.CheckHandle (obj);
			}
		}

		protected override void CheckToString (NSObject obj)
		{
			string name = obj.GetType ().Name;
			switch (name) {
			// crash at at MonoTouch.Foundation.NSObject.get_Description () [0x0000b] in /mono/ios/monotouch-ios7/monotouch/src/Foundation/NSObject.g.cs:500
			case "SKTexture":
			case "MCSession":
			// crash at at MonoTouch.Foundation.NSObject.get_Description () [0x0000b] in /Developer/MonoTouch/Source/monotouch/src/Foundation/NSObject.g.cs:554
			case "AVPlayerItemTrack":
			case "AVCaptureConnection":
				return;
			// worked before ios6.0 beta 1
			case "AVComposition":
			// new API in iOS7 
			case "AVAssetResourceLoadingDataRequest":
			// Objective-C exception thrown.  Name: NSInvalidArgumentException Reason: Unable to create description in descriptionForLayoutAttribute_layoutItem_coefficient. Something is nil
			case "NSLayoutConstraint":
			// new in 6.0
			case "AVAssetResourceLoadingRequest":
			case "GKScoreChallenge": // Objective-C exception thrown.  Name: NSInvalidArgumentException Reason: -[GKScoreChallenge challengeID]: unrecognized selector sent to instance 0x18acc340
			case "GKAchievementChallenge": // Objective-C exception thrown.  Name: NSInvalidArgumentException Reason: -[GKAchievementChallenge challengeID]: unrecognized selector sent to instance 0x160f4840
				if (CheckiOSOrTVOSSystemVersion (6,0))
					return;
				break;
			// crash (when asking `description`) under iOS5 (only) simulator
			case "NSUrlConnection":
				return;
			// iOS 9 beta 1 - crash when called
			case "WKFrameInfo":
			case "WKNavigation":
			case "WKNavigationAction":
				if (CheckiOSSystemVersion (9,0))
					return;
				break;
			default:
				base.CheckToString (obj);
				break;
			}
		}


		protected override void CheckNSObjectProtocol (NSObject obj)
		{
			switch (obj.GetType ().Name) {
			case "NSString":
				// according to bots `isKindOf (null)` returns true before iOS 8, ref: #36726
				if (!CheckiOSOrTVOSSystemVersion (8, 0))
					return;
				break;
			}
			base.CheckNSObjectProtocol (obj);
		}

		// notes:
		// * Splitview controller <UISplitViewController: 0xda106e0> is expected to have a view controller at index 0 before it's used!
		//	this happens when we dispose an empty UISplitViewController, harmless
	}
}
