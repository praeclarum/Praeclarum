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
		UIBarButtonItem backItem;

		public Action<(NSUrl, Match)>? DownloadUrl;

		public GalleryViewController (string galleryUrl, Regex downloadUrlRe)
		{
			this.galleryUrl = galleryUrl;
			this.downloadUrlRe = downloadUrlRe;
			Title = "Gallery".Localize ();

			backItem = new UIBarButtonItem("Back", UIBarButtonItemStyle.Plain, (s, e) => {
				webBrowser?.GoBack();
			});
			backItem.Enabled = false;
			NavigationItem.LeftBarButtonItems = new UIBarButtonItem[] {
				backItem,
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
			var vcv = vc.View;
			if (vcv is null) {
				return;
			}
			webBrowser = new WKWebView (vcv.Bounds, config) {
				AutoresizingMask = UIViewAutoresizing.FlexibleDimensions
			};
			webBrowser.NavigationDelegate = this;
			webBrowser.LoadRequest (new NSUrlRequest (new NSUrl (galleryUrl)));
			vcv.AddSubview (webBrowser);
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

		[Export ("webView:didFinishNavigation:")]
		public void DidFinishNavigation (WKWebView webView, WKNavigation navigation)
		{
			backItem.Enabled = webView.CanGoBack;
		}
	}
}
