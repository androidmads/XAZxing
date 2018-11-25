using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Util;
using Android.Widget;
using ComGoogleZxingIntegrationAndroid;

namespace ZxingSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            LinearLayout ll = new LinearLayout(this);
            Button button = new Button(this);
            button.Text = "Click to Scan using Zxing";
            button.Click += (s, e) =>
            {
                IntentIntegrator intentIntegrator = new IntentIntegrator(this);
                intentIntegrator.InitiateScan();
            };
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(Android.Views.ViewGroup.LayoutParams.MatchParent,
                Android.Views.ViewGroup.LayoutParams.WrapContent);
            ll.AddView(button, lp);
            SetContentView(ll);
        }

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
    }
}