using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_admitere_facultate
{
    internal class OptiuneCandidat
    {
        public int IdOptiune { get; set; }
        public int IdCandidat { get; set; }
        public int IdSpecializare1 { get; set; }
        public int? IdSpecializare2 { get; set; }
        public int? IdSpecializare3 { get; set; }
    }
}
