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
        private DateTime bidaliData { get; set; }

        /* usar esto para sacar fecha y hora
         * DateTime soloFecha = now.Date; // Solo la fecha (hora será 00:00:00)
         * TimeSpan soloHora = now.TimeOfDay; // Solo la hora
         */

        private Msg(User b, string t, DateTime d)
        {
            this.bidaltzaile = b;
            this.testu = t;
            this.bidaliData = d;
        }
    }
}
