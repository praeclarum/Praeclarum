#nullable enable

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using WebKit;

namespace Praeclarum.UI
{
	public class GalleryViewController : UIViewController, IWKNavigationDelegate
	{
		private readonly string galleryUrl;
		private readonly Regex downloadUrlRe;
		WKWebView? webBrowser;

		public Action<(NSUrl, Match)>? DownloadUrl;

		public GalleryViewController (string galleryUrl, Regex downloadUrlRe)
		{
			this.galleryUrl = galleryUrl;
			this.downloadUrlRe = downloadUrlRe;
			Title = "Gallery".Localize ();

			NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] {
				new UIBarButtonItem ("Back", UIBarButtonItemStyle.Plain, (s, e) => {
					webBrowser?.GoBack ();
				}),
			};

			NavigationItem.RightBarButtonItems = new UIBarButtonItem[] {
				new UIBarButtonItem (UIBarButtonSystemItem.Done, (s, e) => {
					DownloadUrl = null;
					DismissViewController (true, null);
				}),
			};
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			var config = new WKWebViewConfiguration ();
			var vc = this;
			webBrowser = new WKWebView (vc.View.Bounds, config) {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions
			};
			webBrowser.NavigationDelegate = this;
			webBrowser.LoadRequest (new NSUrlRequest (new NSUrl (galleryUrl)));
			vc.View.AddSubview (webBrowser);
		}

		[Export ("webView:decidePolicyForNavigationAction:decisionHandler:")]
		public void DecidePolicy (WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
		{
			var url = navigationAction?.Request.Url;
			var urls = url?.AbsoluteString ?? "";
			var m = downloadUrlRe.Match (urls);
			if (url != null && m.Success) {
				decisionHandler (WKNavigationActionPolicy.Cancel);
				BeginInvokeOnMainThread (() => {
					DownloadUrl?.Invoke ((url, m));
				});
			}
			else {
				decisionHandler (WKNavigationActionPolicy.Allow);
			}
		}
	}
}
