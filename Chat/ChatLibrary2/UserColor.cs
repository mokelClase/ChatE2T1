using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChatLibrary
{
    public class UserColor
    {
        private int id;
        private int r;
        private int g;
        private int b;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int R
        {
            get { return r; }
            set { r = value; }
        }

        public int G
        {
            get { return g; }
            set {  g = value; }
        }

        public int B
        {
            get { return b; }
            set { b = value; }
        }

        public UserColor(int i, int r, int g, int b)
        {
            this.id = i;
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }
}
