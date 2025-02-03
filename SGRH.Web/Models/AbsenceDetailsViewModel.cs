
namespace SGRH.Web.Models

{

    public class AbsenceDetailsViewModel

    {

        public string Category { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Comments { get; set; }

        public List<DocumentViewModel> Document { get; set; }

    }

    public class DocumentViewModel

    {

        public int Id { get; set; }

        public string FileName { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }

        public byte[] Content { get; set; }

        public static implicit operator List<object>(DocumentViewModel v)

        {

            throw new NotImplementedException();

        }

    }

}
