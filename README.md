# XAZxing
This library is a port of Zxing IntentIntegrator for Xamarin.Android
## How to Download
You can download the library [here](https://androidmads.github.io/store/Com.Google.Zxing.Integration.Android.dll)
## How to Use
#### The steps to use this Library
* First add the following lines to open ZXing Barcoded Scanner App
``` csharp
IntentIntegrator intentIntegrator = new IntentIntegrator(this);
intentIntegrator.InitiateScan();
```
* Then override <b>OnActivityResult</b> method
``` csharp
protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
{

	IntentResult result = IntentIntegrator.ParseActivityResult(requestCode, (int)resultCode, data);
	if (result != null)
	{
		if (result.Contents == null)
		{
			Log.Debug("MainActivity", "Cancelled scan");
			Toast.MakeText(this, "Cancelled", ToastLength.Long).Show();
		}
		else
		{
			Log.Debug("MainActivity", "Scanned");
			Toast.MakeText(this, "Scanned: " + result.Contents, ToastLength.Long).Show();
		}
	}
	else
	{
		base.OnActivityResult(requestCode, resultCode, data);
	}
}
```
## License
<pre>
MIT License

Copyright (c) 2018 AndroidMad / Mushtaq M A

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated<br/>documentation files (the "Software"), to deal in the Software without restriction, including without limitation<br/>the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, <br/>and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all<br/> copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT<br/>NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. <br/>IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,<br/>WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE <br/>SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
</pre>
