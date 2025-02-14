using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    public class Msg
    {
        private User bidaltzaile { get; set; }
        private string testu { get; set; }
        private string bidaliData { get; set; }
        private bool mio {  get; set; }

        public new string ToString()
        {
            return (testu+"_"+bidaltzaile.Izena+"_"+bidaltzaile.UserColor.R+"_"+ bidaltzaile.UserColor.G + "_"+ bidaltzaile.UserColor.B+ "_"+bidaliData);
        }
        public bool Mio
        {
            get { return mio; }
            set { mio = value; }
        }
        public User Bidaltzaile
        {
            get { return bidaltzaile; }
            set { bidaltzaile = value; }
        }
        public string Testu
        {
            get { return testu; }
            set { testu = value; }
        }
        public string BidaliData
        {
            get { return bidaliData; }
            set { bidaliData = value; }
        }

        public Msg(User b, string t, DateTime d)
        {
            this.bidaltzaile = b;
            this.testu = t;
            this.bidaliData = d.ToString("HH:dd:ss");
        }

        public Msg(User b, string t, string d)
        {
            this.bidaltzaile = b;
            this.testu = t;
            this.bidaliData = d;
        }
    }
}
