using System.ComponentModel.DataAnnotations;

namespace Bramki.Models
{
    public class GatePrivilege
    {
        [Key]
        public int GatesId { get; set; }       // dbo.DGates.gates_id (PK)

        public int DUserId { get; set; }       // dbo.DGates.duser_id (FK -> DUser.ID)

        public Person Person { get; set; } = null!;

        public bool? GatesForklifts { get; set; }   // dbo.DGates.gates_forklifts
        public bool? GatesCranes { get; set; }      // dbo.DGates.gates_cranes
        public bool? GatesGantries { get; set; }      // dbo.DGates.gates_gantries
    }
}