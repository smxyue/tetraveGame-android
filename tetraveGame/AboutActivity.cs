using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tetraveGame
{
    [Activity(Label = "AboutActivity")]
    public class AboutActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.layoutAbout);
            ImageView img = (ImageView)FindViewById(Resource.Id.imageViewGame);
            img.Click += (s, arg) =>
            {
                this.Finish();
            };
                // Create your application here
            }
    }
}