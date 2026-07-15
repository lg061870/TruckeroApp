namespace TruckeroApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
#if ANDROID
		blazorWebView.HandlerChanged += OnBlazorWebViewHandlerChanged;
		ConfigureAndroidBlazorWebView();
#endif
	}

#if ANDROID
	private void OnBlazorWebViewHandlerChanged(object? sender, EventArgs e)
	{
		ConfigureAndroidBlazorWebView();
	}

	private void ConfigureAndroidBlazorWebView()
	{
		if (blazorWebView?.Handler?.PlatformView is Android.Webkit.WebView webView)
		{
			// Allow HTTP tile resources from the host API proxy while running inside the HTTPS BlazorWebView origin.
			webView.Settings.MixedContentMode = Android.Webkit.MixedContentHandling.AlwaysAllow;
			webView.Settings.DomStorageEnabled = true;
		}
	}
#endif
}
