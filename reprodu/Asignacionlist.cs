using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reprodu
{
    class Asignacionlist
    {
        string cancioneslist;
        string nombrelista;

        public string Contentlist
        {
            get
            {
                return cancioneslist;
            }

            set
            {
                cancioneslist = value;
            }
        }

        public string Nombrelista
        {
            get
            {
                return nombrelista;
            }

            set
            {
                nombrelista = value;
            }
        }
    }
}
