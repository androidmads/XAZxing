using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComGoogleZxingIntegrationAndroid
{
    public class IntentIntegrator
    {
        public static int REQUEST_CODE = 0x0000c0de; // Only use bottom 16 bits
        private static string TAG = "IntentIntegrator";

        public static string DEFAULT_TITLE = "Install Barcode Scanner?";
        public static string DEFAULT_MESSAGE = "This application requires Barcode Scanner. Would you like to install it?";
        public static string DEFAULT_YES = "Yes";
        public static string DEFAULT_NO = "No";

        private static string BS_PACKAGE = "com.google.zxing.client.android";
        private static string BSPLUS_PACKAGE = "com.srowen.bs.android";

        // supported barcode formats
        public static string[] PRODUCT_CODE_TYPES = new string[] { "UPC_A", "UPC_E", "EAN_8", "EAN_13", "RSS_14" };
        public static string[] ONE_D_CODE_TYPES = new string[] { "UPC_A", "UPC_E", "EAN_8", "EAN_13", "CODE_39", "CODE_93", "CODE_128", "ITF", "RSS_14", "RSS_EXPANDED" };
        public static string[] QR_CODE_TYPES = new string[] { "QR_CODE" };
        public static string[] DATA_MATRIX_TYPES = new string[] { "DATA_MATRIX" };
        public static string[] ALL_CODE_TYPES = null;
        public static string[] TARGET_BARCODE_SCANNER_ONLY = new string[] { BS_PACKAGE };
        public static string[] TARGET_ALL_KNOWN = new string[] {
                BSPLUS_PACKAGE,             // Barcode Scanner+
                BSPLUS_PACKAGE + ".simple", // Barcode Scanner+ Simple
                BS_PACKAGE                  // Barcode Scanner          
                                            // What else supports this intent?
        };

        // Should be FLAG_ACTIVITY_NEW_DOCUMENT in API 21+.
        // Defined once here because the current value is deprecated, so generates just one warning
        private static ActivityFlags FLAG_NEW_DOC = ActivityFlags.ClearWhenTaskReset;

        private Activity _activity;
        private Fragment _fragment;

        public string Title { get; set; }
        public string Message { get; set; }
        public string ButtonYes { get; set; }
        public string ButtonNo { get; set; }
        private string[] TargetApplications { get; set; }
        public Dictionary<string, object> moreExtras = new Dictionary<string, object>(3);

        public IntentIntegrator(Activity activity)
        {
            _activity = activity;
            _fragment = null;
            InitializeConfiguration();
        }

        public IntentIntegrator(Fragment fragment)
        {
            _activity = fragment.Activity;
            _fragment = fragment;
            InitializeConfiguration();
        }

        private void InitializeConfiguration()
        {
            Title = DEFAULT_TITLE;
            Message = DEFAULT_MESSAGE;
            ButtonYes = DEFAULT_YES;
            ButtonNo = DEFAULT_NO;
            TargetApplications = TARGET_ALL_KNOWN;
        }

        public AlertDialog InitiateScan()
        {
            return InitiateScan(ALL_CODE_TYPES, -1);
        }

        public AlertDialog InitiateScan(int cameraId)
        {
            return InitiateScan(ALL_CODE_TYPES, cameraId);
        }

        public AlertDialog InitiateScan(string[] desiredBarcodeFormats)
        {
            return InitiateScan(desiredBarcodeFormats, -1);
        }

        public AlertDialog InitiateScan(string[] desiredBarcodeFormats, int cameraId)
        {
            Intent intentScan = new Intent(BS_PACKAGE + ".SCAN");
            intentScan.AddCategory(Intent.CategoryDefault);

            // check which types of codes to scan for
            if (desiredBarcodeFormats != null)
            {
                // set the desired barcode types
                StringBuilder joinedByComma = new StringBuilder();
                foreach (string format in desiredBarcodeFormats)
                {
                    if (joinedByComma.Length > 0)
                    {
                        joinedByComma.Append(',');
                    }
                    joinedByComma.Append(format);
                }
                intentScan.PutExtra("SCAN_FORMATS", joinedByComma.ToString());
            }

            // check requested camera ID
            if (cameraId >= 0)
            {
                intentScan.PutExtra("SCAN_CAMERA_ID", cameraId);
            }

            string targetAppPackage = FindTargetAppPackage(intentScan);
            if (targetAppPackage == null)
            {
                return ShowDownloadDialog();
            }
            intentScan.SetPackage(targetAppPackage);
            intentScan.AddFlags(ActivityFlags.ClearTop);
            intentScan.AddFlags(FLAG_NEW_DOC);
            AttachMoreExtras(intentScan);
            StartActivityForResult(intentScan, REQUEST_CODE);
            return null;
        }

        protected void StartActivityForResult(Intent intent, int code)
        {
            if (_fragment == null)
            {
                _activity.StartActivityForResult(intent, code);
            }
            else
            {
                _fragment.StartActivityForResult(intent, code);
            }
        }

        private string FindTargetAppPackage(Intent intent)
        {
            PackageManager pm = _activity.PackageManager;
            IList<ResolveInfo> availableApps = pm.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            if (availableApps != null)
            {
                foreach (string targetApp in TargetApplications)
                {
                    if (Contains(availableApps, targetApp))
                    {
                        return targetApp;
                    }
                }
            }
            return null;
        }

        private static bool Contains(IList<ResolveInfo> availableApps, string targetApp)
        {
            foreach (ResolveInfo availableApp in availableApps)
            {
                string packageName = availableApp.ActivityInfo.PackageName;
                if (targetApp.Equals(packageName))
                {
                    return true;
                }
            }
            return false;
        }

        private AlertDialog ShowDownloadDialog()
        {
            AlertDialog.Builder downloadDialog = new AlertDialog.Builder(_activity);
            downloadDialog.SetTitle(Title);
            downloadDialog.SetMessage(Message);
            downloadDialog.SetPositiveButton(ButtonYes, (s, e) =>
            {
                string packageName;
                if (TargetApplications.ToList().Contains(BS_PACKAGE))
                {
                    // Prefer to suggest download of BS if it's anywhere in the list
                    packageName = BS_PACKAGE;
                }
                else
                {
                    // Otherwise, first option:
                    packageName = TargetApplications[0];
                }
                Android.Net.Uri uri = Android.Net.Uri.Parse("market://details?id=" + packageName);
                Intent intent = new Intent(Intent.ActionView, uri);
                try
                {
                    if (_fragment == null)
                    {
                        _activity.StartActivity(intent);
                    }
                    else
                    {
                        _fragment.StartActivity(intent);
                    }
                }
                catch (ActivityNotFoundException anfe)
                {
                }
            });
            downloadDialog.SetNegativeButton(ButtonNo, (s, e) => { });
            downloadDialog.SetCancelable(true);
            return downloadDialog.Show();
        }

        public static IntentResult ParseActivityResult(int requestCode, int resultCode, Intent intent)
        {
            if (requestCode == REQUEST_CODE)
            {
                if (resultCode == (int)Result.Ok)
                {
                    string contents = intent.GetStringExtra("SCAN_RESULT");
                    string formatName = intent.GetStringExtra("SCAN_RESULT_FORMAT");
                    byte[] rawBytes = intent.GetByteArrayExtra("SCAN_RESULT_BYTES");
                    int? intentOrientation = intent.GetIntExtra("SCAN_RESULT_ORIENTATION", int.MinValue);
                    int? orientation = intentOrientation == int.MinValue ? null : intentOrientation;
                    string errorCorrectionLevel = intent.GetStringExtra("SCAN_RESULT_ERROR_CORRECTION_LEVEL");
                    return new IntentResult(contents,
                                            formatName,
                                            rawBytes,
                                            orientation,
                                            errorCorrectionLevel);
                }
                return new IntentResult();
            }
            return null;
        }

        public AlertDialog ShareText(string text)
        {
            return ShareText(text, "TEXT_TYPE");
        }

        public AlertDialog ShareText(string text, string type)
        {
            Intent intent = new Intent();
            intent.AddCategory(Intent.CategoryDefault);
            intent.SetAction(BS_PACKAGE + ".ENCODE");
            intent.PutExtra("ENCODE_TYPE", type);
            intent.PutExtra("ENCODE_DATA", text);
            string targetAppPackage = FindTargetAppPackage(intent);
            if (targetAppPackage == null)
            {
                return ShowDownloadDialog();
            }
            intent.SetPackage(targetAppPackage);
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.AddFlags(FLAG_NEW_DOC);
            AttachMoreExtras(intent);
            if (_fragment == null)
            {
                _activity.StartActivity(intent);
            }
            else
            {
                _fragment.StartActivity(intent);
            }
            return null;
        }

        private static string[] List(string values)
        {
            return new string[] { values };
        }

        private void AttachMoreExtras(Intent intent)
        {
            foreach (KeyValuePair<string, object> entry in moreExtras)
            {
                string key = entry.Key;
                object value = entry.Value;
                // Kind of hacky
                if (value is int)
                {
                    intent.PutExtra(key, (int)value);
                }
                else if (value is long)
                {
                    intent.PutExtra(key, (long)value);
                }
                else if (value is bool)
                {
                    intent.PutExtra(key, (bool)value);
                }
                else if (value is double)
                {
                    intent.PutExtra(key, (double)value);
                }
                else if (value is float)
                {
                    intent.PutExtra(key, (float)value);
                }
                else if (value is Bundle)
                {
                    intent.PutExtra(key, (Bundle)value);
                }
                else
                {
                    intent.PutExtra(key, value.ToString());
                }
            }
        }

    }

    public class IntentResult
    {
        public string Contents { get; set; }
        public string FormatName { get; set; }
        public byte[] RawBytes { get; set; }
        public int? Orientation { get; set; }
        public string ErrorCorrectionLevel { get; set; }

        public IntentResult()
        {
            new IntentResult(null, null, null, null, null);
        }

        public IntentResult(string contents,
                     string formatName,
                     byte[] rawBytes,
                     int? orientation,
                     string errorCorrectionLevel)
        {
            Contents = contents;
            FormatName = formatName;
            RawBytes = rawBytes;
            Orientation = orientation;
            ErrorCorrectionLevel = errorCorrectionLevel;
        }
    }
}
