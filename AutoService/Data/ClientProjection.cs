using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoService.Data
{
    public class ClientProjection
    {
        public int ID { get; set; }
        public string GenderCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public DateTime? Birthday { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? LastVisit { get; set; }
        public int VisitCount { get; set; }
        public string Tags { get; set; } // Теги теперь строка
    }

}
