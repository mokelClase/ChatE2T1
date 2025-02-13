using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    public class User
    {
        private string izena;
        private UserColor userColor;
        private List<Msg> msgList = new List<Msg>();

        public string Izena
        {
            get { return izena; }
            set { izena = value; }
        }

        public UserColor UserColor
        {
            get { return userColor; }
            set { userColor = value; }
        }

        public List<Msg> MsgList
        {
            get { return msgList; }
            set { msgList = value; }
        }

        public User(string i)
        {
            this.izena = i;
        }

        public User(string i, UserColor uC)
        {
            this.izena = i;
            this.userColor = uC;
        }
    }
}
