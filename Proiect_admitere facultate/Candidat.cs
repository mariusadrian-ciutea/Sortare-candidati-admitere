using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_admitere_facultate
{
    public class Candidat
    {
        public int IdCandidat { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Adresa { get; set; }
        public int Varsta { get; set; }
        public string Sex { get; set; }
        public string CNP { get; set; }
        public double MedieBAC { get; set; }
        public double MedieLiceu { get; set; }
        public string Status { get; set; }
    }
}
