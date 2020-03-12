using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.Linq;
using System.Text;
using Microsoft.Advertising.Mobile.Xna;

namespace ShuffleBowl
{
    public class HUD
    {
        public DrawableAd bannerAd;
        public AdGameComponent adGameComponent;

        //private static readonly string AdUnitId = "10059751"; ShuffleBowl ID
        private static readonly string AdUnitId = "10064490"; //Custom ID

       // private GeoCoordinateWatcher gcw = null;

        private Vector2 scorePos = new Vector2(20, 10);
        public SpriteFont Font { get; set; }

        public int Score { get; set; }
        private List<Vector2> position;
        private List<int> point;
        public List<int> count;
        private List<Color> color;
        SoundEffectInstance soundEngine;  

        public HUD(SoundEffect sound)
        {
            point = new List<int>();
            position = new List<Vector2>();
            count = new List<int>();
            color = new List<Color>();
            soundEngine = sound.CreateInstance();
            soundEngine.Volume = .5f;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the Score in the top-left of screen
            spriteBatch.DrawString(
                Font,                          // SpriteFont
                Score.ToString(),  // Text
                scorePos,                      // Position
                Color.White);                  // Tint

            for (int i = 0; i < position.Count; i++)
            {
                count[i]--;
                if (count[i] == 0)
                    remove(i);
                else
                    spriteBatch.DrawString(
                    Font,                          // SpriteFont
                    point[i].ToString(),  // Text
                    ConvertUnits.ToDisplayUnits(position[i]),                      // Position
                    color[i]);                  // Tint
            }
        }
        public void add(Vector2 pos, int pts, Color clr, bool playSound)
        {
            position.Add(pos);
            point.Add(pts);
            color.Add(clr);
            count.Add(10);
            if (playSound)
            {
                soundEngine.Play();
            }
        }
        public void remove(int index)
        {
            position.RemoveAt(index);
            point.RemoveAt(index);
            color.RemoveAt(index);
            count.RemoveAt(index);
        }

        public void CreateAd(Viewport viewport)
        {
            int width = 480;
            int height = 80;
            int x = 0;
            int y = viewport.Height - height;

            bannerAd = adGameComponent.CreateAd(AdUnitId, new Rectangle(x, y, width, height), false);
            bannerAd.Visible = true;
            adGameComponent.Visible = true;
            // Set some visual properties (optional).
            //bannerAd.BorderEnabled = true; // default is true
            //bannerAd.BorderColor = Color.White; // default is White
            //bannerAd.DropShadowEnabled = true; // default is true

            // Provide the location to the ad for better targeting (optional).
            // This is done by starting a GeoCoordinateWatcher and waiting for the location to
            // available.

            // The callback will set the location into the ad.

            // Note: The location may not be available in time for the first ad request.

            //adGameComponent.Enabled = false;
 //           this.gcw = new GeoCoordinateWatcher();
 //           this.gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
 //           this.gcw.Start();
        }
  /*      private void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs e)
        {
            // Stop the GeoCoordinateWatcher now that we have the device location.
            this.gcw.Stop();

            bannerAd.LocationLatitude = e.Position.Location.Latitude;
            bannerAd.LocationLongitude = e.Position.Location.Longitude;

            AdGameComponent.Current.Enabled = true;
        }*/
        public void removeAd()
        {
            adGameComponent.RemoveAd(bannerAd);
            // adGameComponent.RemoveAll();
        } 
    }
}
