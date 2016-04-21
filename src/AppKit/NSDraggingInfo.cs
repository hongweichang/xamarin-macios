using System;
using System.Runtime.InteropServices;

using XamCore.CoreGraphics;
using XamCore.Foundation;
using XamCore.ObjCRuntime;

namespace XamCore.AppKit {
	public partial class NSDraggingInfo {
		public void EnumerateDraggingItems (NSDraggingItemEnumerationOptions enumOpts, NSView view, NSPasteboardReading [] classArray, NSDictionary searchOptions, NSDraggingEnumerator enumerator)
		{
			var nsa_classArray = NSArray.FromNSObjects (classArray);
			EnumerateDraggingItems (enumOpts, view, nsa_classArray.Handle, searchOptions, enumerator);
			nsa_classArray.Dispose ();
		}

		public void EnumerateDraggingItems (NSDraggingItemEnumerationOptions enumOpts, NSView view, NSArray classArray, NSDictionary searchOptions, NSDraggingEnumerator enumerator)
		{
			EnumerateDraggingItems (enumOpts, view, classArray.Handle, searchOptions, enumerator);
		}
	}
}