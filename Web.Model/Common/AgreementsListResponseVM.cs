using System.Collections.Generic;

namespace Web.Model.Common
{
    public class AgreementsListResponseVM
    {
        public IList<AgreementVM> AllAgreements { get; set; }
        public IList<AgreementVM> FrequentlyBookedAgreements { get; set; }
    }
}
