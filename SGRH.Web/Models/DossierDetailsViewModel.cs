using SGRH.Web.Enums;
using System;
using System.Collections.Generic;

namespace SGRH.Web.Models
{
    public class DossierDetailsViewModel
    {
        public DocumentType DocumentType { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public List<DossierDocumentViewModel> Document { get; set; }
    }

    public class DossierDocumentViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public long Size { get; set; }
        public byte[] Content { get; set; }
    }
}